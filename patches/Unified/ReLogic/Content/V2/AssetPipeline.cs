#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReLogic.Content.Readers;
using ReLogic.Content.Sources;

namespace ReLogic.Content;

public sealed class AssetPipeline(
	IContentSource[] sources,
	AssetReaderCollection readers,
	int maxConcurrentPrepares,
	IServiceProvider? services = null
)
{
	private readonly ConcurrentDictionary<AssetKey, AssetRecord> records = [];
	private readonly ConcurrentQueue<PreparedAsset> preparedQueue = [];
	private readonly SemaphoreSlim prepareGate = new(maxConcurrentPrepares, maxConcurrentPrepares);

	private IContentSource[] sources = sources;

	public void SetSources(IReadOnlyList<IContentSource> newSources)
	{
		sources = newSources.ToArray();

		foreach (var (_, record) in records) {
			if (!record.IsTracked) {
				continue;
			}

			var plan = BuildTrackedPlan(record.Key);
			ReloadImmediately(record, plan);
		}
	}

	public Asset<T> Request<T>(string path, AssetRequestMode mode) where T : class
	{
		var normalizedPath = NormalizePath(path);
		var key = new AssetKey(typeof(T), normalizedPath);

		var record = records.GetOrAdd(
			key,
			static k => {
				var record = new AssetRecord {
					Key = k, AssetWrapper = null!, State = AssetState.Unloaded, IsTracked = true,
				};
				var asset = new Asset<T>(record);
				record.AssetWrapper = asset;

				return record;
			}
		);

		lock (record.Sync) {
			record.LoadPlan ??= BuildTrackedPlan(key);

			/*
			record.Failures.Clear();
			record.Error = null;
			*/
		}

		EnsureScheduled(record, record.LoadPlan!, mode);
		return (Asset<T>)record.AssetWrapper;
	}

	public Asset<T> CreateUntracked<T>(
		Func<Stream> streamFactory,
		string extension,
		AssetRequestMode mode
	) where T : class
	{
		var key = new AssetKey(typeof(T), "<untracked>");
		var record = new AssetRecord {
			Key = key,
			AssetWrapper = null!,
			State = AssetState.Unloaded,
			IsTracked = false,
			LoadPlan = BuildUntrackedPlan(extension, streamFactory),
		};
		var asset = new Asset<T>(record);
		record.AssetWrapper = asset;

		EnsureScheduled(record, record.LoadPlan, mode);
		return asset;
	}

	public Asset<T> CreateUntracked<T>(Stream stream, string extension, AssetRequestMode mode) where T : class
	{
		var consumed = 0;
		return CreateUntracked<T>(
			() => {
				if (Interlocked.Exchange(ref consumed, 1) != 0) {
					throw new InvalidOperationException("This untracked asset stream can only be opened once. Use a stream factory for retryable loads.");
				}

				return stream;
			},
			extension,
			mode
		);
	}

	public void ProcessMainThread(int maxFinalizations = int.MaxValue)
	{
		for (var i = 0; i < maxFinalizations; i++) {
			if (!preparedQueue.TryDequeue(out var prepared)) {
				break;
			}

			FinalizePrepared(prepared);
		}
	}

	private void EnsureScheduled(AssetRecord record, AssetLoadPlan plan, AssetRequestMode mode)
	{
		lock (record.Sync) {
			record.LoadPlan = plan;

			if (record.State is AssetState.Loaded or AssetState.Preparing or AssetState.WaitingForMainThread) {
				if (mode == AssetRequestMode.ImmediateLoad) {
					TryCompleteImmediately(record);
				}

				return;
			}

			record.Error = null;
			record.Failures.Clear();
			record.State = AssetState.Queued;
			record.Version++;

			var version = record.Version;
			record.PrepareTask = Task.Run(() => PrepareAsync(record, version, plan, 0));
		}

		if (mode == AssetRequestMode.ImmediateLoad) {
			TryCompleteImmediately(record);
		}
	}

	private void ReloadImmediately(AssetRecord record, AssetLoadPlan plan)
	{
		object? oldValue;
		lock (record.Sync) {
			record.LoadPlan = plan;
			record.Error = null;
			record.Failures.Clear();
			record.Version++;
			record.State = AssetState.Queued;
			oldValue = record.Value;
			record.Value = null;
		}

		try {
			LoadSynchronously(record, record.Version, plan, 0, out var newValue);

			lock (record.Sync) {
				if (record.State == AssetState.Loaded && oldValue is IDisposable disposable && !ReferenceEquals(oldValue, newValue)) {
					disposable.Dispose();
				}
			}
		}
		catch {
			if (oldValue is IDisposable disposable) {
				disposable.Dispose();
			}

			throw;
		}
	}

	private void TryCompleteImmediately(AssetRecord record)
	{
		var prepareTask = record.PrepareTask;
		if (prepareTask is null) {
			return;
		}

		var prepared = prepareTask.GetAwaiter().GetResult();
		if (prepared is { } pending) {
			FinalizePrepared(pending);
		}
	}

	private async Task<PreparedAsset?> PrepareAsync(AssetRecord record, int version, AssetLoadPlan plan, int startCandidateIndex)
	{
		await prepareGate.WaitAsync().ConfigureAwait(false);

		try {
			lock (record.Sync) {
				if (record.Version != version) {
					return null;
				}

				record.State = AssetState.Preparing;
			}

			for (var candidateIndex = startCandidateIndex; candidateIndex < plan.Candidates.Count; candidateIndex++) {
				var candidate = plan.Candidates[candidateIndex];
				if (!readers.TryGetReader(candidate.Extension, out var reader)) {
					continue;
				}

				var context = new AssetLoadContext(candidate.Name, candidate.Extension, candidate.OpenStream, candidate.SourceTag, services);

				try {
					var prepareResult = await reader.PrepareAsync(context, CancellationToken.None).ConfigureAwait(false);
					if (!prepareResult.Succeeded) {
						RecordFailure(record, version, candidate, prepareResult.Reason, prepareResult.Error);
						continue;
					}

					if (reader.FinalizeThread == AssetFinalizeThread.WorkerThread) {
						var finalizeResult = reader.Finalize(context, prepareResult.PreparedData!);
						if (!finalizeResult.Succeeded) {
							RecordFailure(record, version, candidate, finalizeResult.Reason, finalizeResult.Error);
							continue;
						}

						lock (record.Sync) {
							if (record.Version != version) {
								reader.Dispose(finalizeResult.Asset!);
								return null;
							}

							record.Value = finalizeResult.Asset;
							record.Error = null;
							record.State = AssetState.Loaded;
						}

						// No need to return it here since it already finishes preparing
						// on the main thread.
						return null;
					}

					lock (record.Sync) {
						if (record.Version != version) {
							return null;
						}

						record.State = AssetState.WaitingForMainThread;
					}

					var prepared = new PreparedAsset(
						record,
						reader,
						plan,
						candidateIndex,
						prepareResult.PreparedData!,
						version
					);
					preparedQueue.Enqueue(prepared);
					return prepared;
				}
				catch (Exception ex) {
					RecordFailure(record, version, candidate, null, ex);
				}
			}

			lock (record.Sync) {
				if (record.Version != version) {
					return null;
				}

				record.Error = new AssetLoadFailureException(
					record.Key.Path,
					record.Key.AssetType,
					record.Failures.ToArray()
				);
				record.State = AssetState.Failed;
			}

			return null;
		}
		finally {
			prepareGate.Release();
		}
	}

	private void FinalizePrepared(PreparedAsset prepared)
	{
		var record = prepared.Record;

		lock (record.Sync) {
			if (record.Version != prepared.Version) {
				return;
			}

			if (record.State != AssetState.WaitingForMainThread) {
				return;
			}
		}

		var candidate = prepared.Plan.Candidates[prepared.CandidateIndex];
		var context = new AssetLoadContext(candidate.Name, candidate.Extension, candidate.OpenStream, candidate.SourceTag, services);

		try {
			var finalizeResult = prepared.Reader.Finalize(context, prepared.PreparedData);
			if (finalizeResult.Succeeded) {
				var assetValue = finalizeResult.Asset!;

				lock (record.Sync) {
					if (record.Version != prepared.Version) {
						prepared.Reader.Dispose(assetValue);
						return;
					}

					// Prevent a possible double-finalization if PrepareAsync
					// enqueued to preparedQueue in an Immediate request.
					if (record.State != AssetState.WaitingForMainThread) {
						prepared.Reader.Dispose(assetValue);
						return;
					}

					record.Value = assetValue;
					record.Error = null;
					record.State = AssetState.Loaded;
				}

				return;
			}

			ContinueFromNextCandidate(
				record,
				prepared.Version,
				prepared.Plan,
				prepared.CandidateIndex,
				candidate,
				finalizeResult.Reason,
				finalizeResult.Error
			);
		}
		catch (Exception e) {
			ContinueFromNextCandidate(
				record,
				prepared.Version,
				prepared.Plan,
				prepared.CandidateIndex,
				candidate,
				null,
				e
			);
		}
	}

	private void ContinueFromNextCandidate(
		AssetRecord record,
		int version,
		AssetLoadPlan plan,
		int failedCandidateIndex,
		AssetLoadCandidate failedCandidate,
		string? reason,
		Exception? exception
	)
	{
		lock (record.Sync) {
			if (record.Version != version) {
				return;
			}

			record.Failures.Add(new AssetCandidateFailure(failedCandidate, reason, exception));

			var nextCandidateIndex = failedCandidateIndex + 1;
			if (nextCandidateIndex >= plan.Candidates.Count) {
				record.Error = new AssetLoadFailureException(plan.Name, record.Key.AssetType, record.Failures.ToArray());
				record.State = AssetState.Failed;
				return;
			}

			record.State = AssetState.Queued;
			record.PrepareTask = Task.Run(() => PrepareAsync(record, version, plan, nextCandidateIndex));
		}
	}

	private static void RecordFailure(
		AssetRecord record,
		int version,
		AssetLoadCandidate candidate,
		string? reason,
		Exception? exception
	)
	{
		lock (record.Sync) {
			if (record.Version != version) {
				return;
			}

			record.Failures.Add(new AssetCandidateFailure(candidate, reason, exception));
		}
	}

	private void LoadSynchronously(AssetRecord record, int version, AssetLoadPlan plan, int startCandidateIndex, out object? loadedValue)
	{
		loadedValue = null;

		lock (record.Sync) {
			record.State = AssetState.Preparing;
		}

		for (var candidateIndex = startCandidateIndex; candidateIndex < plan.Candidates.Count; candidateIndex++) {
			var candidate = plan.Candidates[candidateIndex];
			if (!readers.TryGetReader(candidate.Extension, out var reader)) {
				continue;
			}

			var context = new AssetLoadContext(candidate.Name, candidate.Extension, candidate.OpenStream, candidate.SourceTag, services);

			try {
				var prepareResult = reader.PrepareAsync(context, CancellationToken.None).GetAwaiter().GetResult();
				if (!prepareResult.Succeeded) {
					RecordFailure(record, version, candidate, prepareResult.Reason, prepareResult.Error);
					continue;
				}

				var finalizeResult = reader.Finalize(context, prepareResult.PreparedData!);
				if (!finalizeResult.Succeeded) {
					RecordFailure(record, version, candidate, finalizeResult.Reason, finalizeResult.Error);
					continue;
				}

				lock (record.Sync) {
					if (record.Version != version) {
						return;
					}

					record.Value = finalizeResult.Asset;
					record.Error = null;
					record.State = AssetState.Loaded;
				}

				loadedValue = finalizeResult.Asset;
				return;
			}
			catch (Exception ex) {
				RecordFailure(record, version, candidate, null, ex);
			}
		}

		lock (record.Sync) {
			if (record.Version != version) {
				return;
			}

			record.Error = new AssetLoadFailureException(plan.Name, record.Key.AssetType, record.Failures.ToArray());
			record.State = AssetState.Failed;
		}
	}

	private AssetLoadPlan BuildTrackedPlan(AssetKey key)
	{
		var candidates = new List<AssetLoadCandidate>(sources.Length);

		foreach (var source in sources) {
			if (source.GetExtension(key.Path) is not { } ext) {
				continue;
			}

			if (!readers.TryGetReader(ext, out var reader)) {
				continue;
			}

			candidates.Add(
				new AssetLoadCandidate(
					key.Path,
					ext,
					() => source.OpenStream(key.Path),
					source
				)
			);
		}

		return new AssetLoadPlan(key.Path, true, candidates);
	}

	private static AssetLoadPlan BuildUntrackedPlan(string extension, Func<Stream> streamFactory)
	{
		var candidate = new AssetLoadCandidate(
			"<untracked>",
			extension,
			streamFactory,
			null
		);

		return new AssetLoadPlan(
			"<untracked>",
			false,
			[candidate]
		);
	}

	private static string NormalizePath(string path)
	{
		return path.Replace('\\', '/');
	}
}