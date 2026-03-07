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

public delegate void AssetValueUpdated(AssetRecord asset, object value);

public delegate void AssetWatcherUpdateFailed(AssetRecord asset, Exception e);

public delegate void AssetWatcherValueUpdated(AssetRecord asset);

public delegate void ContentFileUpdated(IContentSource contentSource, string path, string fullPath);

public delegate void FailedToLoadAssetCustomAction(string assetName, Exception e);

// This interface exists purely for compatibility with old vanilla code.
public interface IAssetRepository : IDisposable
{
	int PendingAssets { get; }

	AssetValueUpdated? AssetValueUpdatedHandler { get; set; }

	FailedToLoadAssetCustomAction? AssetLoadFailHandler { get; set; }

	AssetWatcherValueUpdated? AssetWatcherValueUpdatedHandler { get; set; }

	AssetWatcherUpdateFailed? AssetWatcherUpdateFailedHandler { get; set; }

	ContentFileUpdated? ContentFileUpdatedHandler { get; set; }

	Asset<T> Request<T>(
		string assetName,
		AssetRequestMode mode = AssetRequestMode.ImmediateLoad
	) where T : class;

	Asset<T> CreateUntracked<T>(
		Stream stream,
		string extension,
		AssetRequestMode mode = AssetRequestMode.ImmediateLoad
	) where T : class;

	void SetSources(IReadOnlyList<IContentSource> newSources);

	void TransferCompletedAssets();

	void EnableAssetWatcher();
}

public sealed class AssetPipeline(
	IContentSource[] sources,
	AssetReaderCollection readers,
	int maxConcurrentPrepares,
	IServiceProvider? services = null
) : IAssetRepository
{
	private readonly ConcurrentDictionary<AssetKey, AssetRecord> records = [];
	private readonly ConcurrentQueue<Action> preparedQueue = [];
	private readonly SemaphoreSlim prepareGate = new(maxConcurrentPrepares, maxConcurrentPrepares);
	private bool isDisposed;

	private IContentSource[] sources = sources;

	int IAssetRepository.PendingAssets {
		get {
			var count = 0;

			foreach (var pair in records) {
				switch (pair.Value.State) {
					case AssetState.Queued:
					case AssetState.Preparing:
					case AssetState.WaitingForMainThread:
						count++;
						break;
				}
			}

			return count;
		}
	}

	AssetValueUpdated? IAssetRepository.AssetValueUpdatedHandler { get; set; }

	FailedToLoadAssetCustomAction? IAssetRepository.AssetLoadFailHandler { get; set; }

	AssetWatcherValueUpdated? IAssetRepository.AssetWatcherValueUpdatedHandler { get; set; }

	AssetWatcherUpdateFailed? IAssetRepository.AssetWatcherUpdateFailedHandler { get; set; }

	ContentFileUpdated? IAssetRepository.ContentFileUpdatedHandler { get; set; }

	public void SetSources(IReadOnlyList<IContentSource> newSources)
	{
		ThrowIfDisposed();

		sources = newSources.ToArray();

		foreach (var (_, record) in records) {
			if (!record.IsTracked) {
				continue;
			}

			var plan = BuildTrackedPlan(record.Key);

			// TODO: One day we might want to support async or no-op?  Async
			//       would be useful to avoid freezing the game and instead
			//       displaying a loading screen...
			/*
			if (mode == AssetRequestMode.DoNotLoad)
			{
				lock (record.Sync)
				{
					record.LoadPlan = plan;
					record.Error = null;
					record.Failures.Clear();
					record.Version++;
					record.State = AssetState.Unloaded;
					record.Value = null;
				}

				continue;
			}
			*/

			record.ImmediateReloadAction?.Invoke(this, record, plan);
		}
	}

	public Asset<T> Request<T>(string path, AssetRequestMode mode) where T : class
	{
		ThrowIfDisposed();

		var normalizedPath = NormalizePath(path);
		var key = new AssetKey(typeof(T), normalizedPath);

		var record = records.GetOrAdd(
			key,
			static k => {
				var record = new AssetRecord {
					Key = k,
					AssetWrapper = null!,
					State = AssetState.Unloaded,
					IsTracked = true,
					ImmediateReloadAction = static (pipeline, record, plan) => pipeline.ReloadImmediately<T>(record, plan),
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

		EnsureScheduled<T>(record, record.LoadPlan, mode);
		return (Asset<T>)record.AssetWrapper;
	}

	public Asset<T> CreateUntracked<T>(
		Func<Stream> streamFactory,
		string extension,
		AssetRequestMode mode
	) where T : class
	{
		ThrowIfDisposed();

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

		EnsureScheduled<T>(record, record.LoadPlan, mode);
		return asset;
	}

	public Asset<T> CreateUntracked<T>(Stream stream, string extension, AssetRequestMode mode) where T : class
	{
		ThrowIfDisposed();

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

	void IAssetRepository.TransferCompletedAssets()
	{
		ProcessMainThread();
	}

	public void ProcessMainThread(int maxFinalizations = int.MaxValue)
	{
		ThrowIfDisposed();

		for (var i = 0; i < maxFinalizations; i++) {
			if (!preparedQueue.TryDequeue(out var preparedAction)) {
				break;
			}

			preparedAction.Invoke();
		}
	}

	private void EnsureScheduled<T>(
		AssetRecord record,
		AssetLoadPlan plan,
		AssetRequestMode mode
	)
		where T : class
	{
		lock (record.Sync) {
			record.LoadPlan = plan;

			if (record.State is AssetState.Loaded or AssetState.Preparing or AssetState.WaitingForMainThread) {
				if (mode == AssetRequestMode.ImmediateLoad) {
					TryCompleteImmediately<T>(record);
				}

				return;
			}

			record.Error = null;
			record.Failures.Clear();
			record.State = AssetState.Queued;
			record.Version++;

			var version = record.Version;
			record.PrepareTask = Task.Run(() => PrepareAsync<T>(record, version, plan, 0));
		}

		if (mode == AssetRequestMode.ImmediateLoad) {
			TryCompleteImmediately<T>(record);
		}
	}

	private void ReloadImmediately<T>(AssetRecord record, AssetLoadPlan plan)
		where T : class
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
			LoadSynchronously<T>(record, record.Version, plan, 0, out var newValue);

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

	private void TryCompleteImmediately<T>(AssetRecord record)
		where T : class
	{
		var prepareTask = record.PrepareTask;
		if (prepareTask is null) {
			return;
		}

		var prepared = prepareTask.GetAwaiter().GetResult();
		if (prepared is { } pending) {
			FinalizePrepared<T>(pending);
		}
	}

	private async Task<PreparedAsset?> PrepareAsync<T>(
		AssetRecord record,
		int version,
		AssetLoadPlan plan,
		int startCandidateIndex
	) where T : class
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

				if (plan.IsTracked && IsCandidateRejected(candidate)) {
					continue;
				}

				if (!readers.TryGetReader(record.Key.AssetType, candidate.Extension, out var reader)) {
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

						var finalizedAsset = finalizeResult.Asset!;
						if (plan.IsTracked && !ValidateTrackedAsset(candidate, (T)finalizedAsset, out var validationRejection)) {
							RejectCandidate(candidate, validationRejection!);
							RecordFailure(record, version, candidate, validationRejection!.GetReason(), null);
							reader.Dispose(finalizedAsset);
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
						record.Reader = reader;
					}

					var prepared = new PreparedAsset(
						record,
						reader,
						plan,
						candidateIndex,
						prepareResult.PreparedData!,
						version
					);
					preparedQueue.Enqueue(() => FinalizePrepared<T>(prepared));
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

	private void FinalizePrepared<T>(PreparedAsset prepared)
		where T : class
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

				if (prepared.Plan.IsTracked && !ValidateTrackedAsset(candidate, (T)assetValue, out var validationRejection)) {
					RejectCandidate(candidate, validationRejection!);
					prepared.Reader.Dispose(assetValue);
					ContinueFromNextCandidate<T>(
						record,
						prepared.Version,
						prepared.Plan,
						prepared.CandidateIndex,
						candidate,
						validationRejection!.GetReason(),
						null
					);
					return;
				}

				object? oldValue;
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

					oldValue = record.Value;
					record.Value = assetValue;
					record.Error = null;
					record.State = AssetState.Loaded;
				}

				if (oldValue is not null && !ReferenceEquals(oldValue, assetValue)) {
					prepared.Reader.Dispose(oldValue);
				}

				return;
			}

			ContinueFromNextCandidate<T>(
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
			ContinueFromNextCandidate<T>(
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

	private void ContinueFromNextCandidate<T>(
		AssetRecord record,
		int version,
		AssetLoadPlan plan,
		int failedCandidateIndex,
		AssetLoadCandidate failedCandidate,
		string? reason,
		Exception? exception
	) where T : class
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
			record.PrepareTask = Task.Run(() => PrepareAsync<T>(record, version, plan, nextCandidateIndex));
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

	private void LoadSynchronously<T>(
		AssetRecord record,
		int version,
		AssetLoadPlan plan,
		int startCandidateIndex,
		out object? loadedValue
	) where T : class
	{
		loadedValue = null;

		lock (record.Sync) {
			record.State = AssetState.Preparing;
		}

		for (var candidateIndex = startCandidateIndex; candidateIndex < plan.Candidates.Count; candidateIndex++) {
			var candidate = plan.Candidates[candidateIndex];

			if (plan.IsTracked && IsCandidateRejected(candidate)) {
				continue;
			}

			if (!readers.TryGetReader(record.Key.AssetType, candidate.Extension, out var reader)) {
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

				var finalizedAsset = finalizeResult.Asset!;
				if (plan.IsTracked && !ValidateTrackedAsset(candidate, (T)finalizedAsset, out var validationRejection)) {
					RejectCandidate(candidate, validationRejection!);
					RecordFailure(record, version, candidate, validationRejection!.GetReason(), null);
					reader.Dispose(finalizedAsset);
					continue;
				}

				lock (record.Sync) {
					if (record.Version != version) {
						reader.Dispose(finalizedAsset);
						return;
					}

					record.Reader = reader;
					record.Value = finalizedAsset;
					record.Error = null;
					record.State = AssetState.Loaded;
				}

				loadedValue = finalizedAsset;
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

	private static IContentSource? GetCandidateSource(AssetLoadCandidate candidate)
	{
		return candidate.SourceTag as IContentSource;
	}

	private static bool IsCandidateRejected(AssetLoadCandidate candidate)
	{
		var source = GetCandidateSource(candidate);
		return source is not null && source.Rejections.IsRejected(candidate.Name);
	}

	private static void RejectCandidate(
		AssetLoadCandidate candidate,
		IRejectionReason rejectionReason
	)
	{
		var source = GetCandidateSource(candidate);
		source?.Rejections.Reject(candidate.Name, rejectionReason);
	}

	private static bool ValidateTrackedAsset<T>(
		AssetLoadCandidate candidate,
		T asset,
		out IRejectionReason? rejectionReason
	) where T : class
	{
		var source = GetCandidateSource(candidate);
		if (source?.ContentValidator is null) {
			rejectionReason = null;
			return true;
		}

		return source.ContentValidator.AssetIsValid(asset, candidate.Name, out rejectionReason);
	}

	private AssetLoadPlan BuildTrackedPlan(AssetKey key)
	{
		var candidates = new List<AssetLoadCandidate>(sources.Length);

		foreach (var source in sources) {
			if (source.GetExtension(key.Path) is not { } ext) {
				continue;
			}

			if (!readers.TryGetReader(key.AssetType, ext, out _)) {
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

	public void EnableAssetWatcher()
	{
		// TODO
	}

	public void Dispose()
	{
		if (isDisposed) {
			return;
		}

		isDisposed = true;

		while (preparedQueue.TryDequeue(out _)) { }

		foreach (var (_, record) in records) {
			object? value;
			lock (record.Sync) {
				record.Version++;
				value = record.Value;
				record.Value = null;
				record.Error = null;
				record.Failures.Clear();
				record.State = AssetState.Unloaded;
			}

			if (value is null) {
				continue;
			}

			record.Reader?.Dispose(value);
		}

		records.Clear();
		prepareGate.Dispose();
	}

	private void ThrowIfDisposed()
	{
		ObjectDisposedException.ThrowIf(isDisposed, this);
	}

	private static string NormalizePath(string path)
	{
		return path.Replace('\\', '/');
	}
}