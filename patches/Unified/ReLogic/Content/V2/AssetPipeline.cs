#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ReLogic.Content.Readers;
using ReLogic.Content.Sources;

namespace ReLogic.Content;

public enum AssetLoadAttemptState
{
	Succeeded,
	Rejected,
}

internal readonly record struct AssetLoadAttempt(
	AssetLoadAttemptState State,
	object? Value,
	string? Reason = null,
	Exception? Error = null
)
{
	public bool Succeeded => State == AssetLoadAttemptState.Succeeded;

	public bool Rejected => State == AssetLoadAttemptState.Rejected;

	public static AssetLoadAttempt Success(object? value)
	{
		return new AssetLoadAttempt(
			AssetLoadAttemptState.Succeeded,
			value
		);
	}

	public static AssetLoadAttempt Reject(string? reason = null, Exception? error = null)
	{
		return new AssetLoadAttempt(
			AssetLoadAttemptState.Rejected,
			null,
			reason,
			error
		);
	}
}

internal enum AssetBackgroundLoadState
{
	Loaded,
	WaitingForMainThread,
	RejectedAllSources,
}

internal readonly record struct AssetBackgroundLoadResult(
	AssetBackgroundLoadState State,
	PreparedAsset? Prepared
)
{
	public static AssetBackgroundLoadResult Loaded()
	{
		return new AssetBackgroundLoadResult(
			AssetBackgroundLoadState.Loaded,
			null
		);
	}

	public static AssetBackgroundLoadResult WaitingForMainThread(PreparedAsset prepared)
	{
		return new AssetBackgroundLoadResult(
			AssetBackgroundLoadState.WaitingForMainThread,
			prepared
		);
	}

	public static AssetBackgroundLoadResult RejectedAllSources()
	{
		return new AssetBackgroundLoadResult(
			AssetBackgroundLoadState.RejectedAllSources,
			null
		);
	}
}

public sealed class AssetPipeline(
	IContentSource[] sources,
	AssetReaderRegistry readers,
	int maxConcurrentPrepares,
	IServiceProvider? services = null
)
{
	private readonly ConcurrentDictionary<AssetKey, AssetRecord> records = [];
	private readonly ConcurrentQueue<PreparedAsset> preparedQueue = [];
	private readonly SemaphoreSlim prepareGate = new(maxConcurrentPrepares, maxConcurrentPrepares);

	public Asset<T> Request<T>(string path, AssetRequestMode mode) where T : class
	{
		var normalizedPath = NormalizePath(path);
		var key = new AssetKey(typeof(T), normalizedPath);

		var record = records.GetOrAdd(
			key,
			static (k, defaultValue) => {
				var record = new AssetRecord {
					Key = k,
					AssetWrapper = null!,
					State = AssetState.Unloaded,
				};
				var asset = new Asset<T>(record, defaultValue);
				record.AssetWrapper = asset;

				return record;
			},
			GetDefaultValue<T>()
		);

		record.Failures.Clear();
		record.Error = null;

		EnsureScheduled(record);

		if (mode == AssetRequestMode.ImmediateLoad) {
			TryCompleteImmediately(record);
		}

		return (Asset<T>)record.AssetWrapper;
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

	private void EnsureScheduled(AssetRecord record)
	{
		lock (record.Sync) {
			if (record.State is AssetState.Loaded or AssetState.Preparing or AssetState.WaitingForMainThread) {
				return;
			}

			if (record.State == AssetState.Failed) {
				return;
			}

			record.State = AssetState.Queued;
			record.Reader = readers.GetReader(record.Key.AssetType);
			record.Version++;

			var version = record.Version;
			record.PrepareTask = Task.Run(() => PrepareAsync(record, version, 0));
		}
	}

	private async Task<PreparedAsset?> PrepareAsync(AssetRecord record, int version, int startSourceIndex)
	{
		await prepareGate.WaitAsync().ConfigureAwait(false);

		try {
			lock (record.Sync) {
				if (record.Version != version) {
					return null;
				}

				record.State = AssetState.Preparing;
			}

			var reader = record.Reader!;

			for (var sourceIndex = startSourceIndex; sourceIndex < sources.Length; sourceIndex++) {
				var source = sources[sourceIndex];
				var context = new AssetLoadContext(record.Key.Path, source, services);

				if (!source.HasAsset(record.Key.Path)) {
					continue;
				}

				try {
					var prepareResult = await reader.PrepareAsync(context, CancellationToken.None).ConfigureAwait(false);
					if (!prepareResult.Succeeded) {
						lock (record.Sync) {
							if (record.Version != version)
								return null;

							record.Failures.Add(new AssetSourceFailure(source, prepareResult.Reason, prepareResult.Error));
						}

						continue;
					}

					if (reader.FinalizeThread == AssetFinalizeThread.WorkerThread) {
						var finalizeResult = reader.Finalize(context, prepareResult.PreparedData!);
						if (!finalizeResult.Succeeded) {
							lock (record.Sync) {
								if (record.Version != version)
									return null;

								record.Failures.Add(new AssetSourceFailure(source, finalizeResult.Reason, finalizeResult.Error));
							}

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
						if (record.Version != version)
							return null;

						record.State = AssetState.WaitingForMainThread;
					}

					var prepared = new PreparedAsset(
						record,
						reader,
						source,
						sourceIndex,
						prepareResult.PreparedData!,
						version
					);
					preparedQueue.Enqueue(prepared);
					return prepared;
				}
				catch (Exception ex) {
					lock (record.Sync) {
						if (record.Version != version)
							return null;

						record.Failures.Add(new AssetSourceFailure(source, null, ex));
					}
				}
			}

			lock (record.Sync) {
				if (record.Version != version)
					return null;

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

		var context = new AssetLoadContext(record.Key.Path, prepared.Source, services);

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

			RetryFromNextSource(
				record,
				prepared.Version,
				prepared.SourceIndex + 1,
				finalizeResult.Reason,
				finalizeResult.Error
			);
		}
		catch (Exception e) {
			RetryFromNextSource(
				record,
				prepared.Version,
				prepared.SourceIndex + 1,
				null,
				e
			);
		}
	}

	private void RetryFromNextSource(
		AssetRecord record,
		int version,
		int nextSourceIndex,
		string? reason,
		Exception? exception
	)
	{
		lock (record.Sync) {
			if (record.Version != version)
				return;

			if (nextSourceIndex >= sources.Length) {
				record.Error = new AssetLoadFailureException(
					record.Key.Path,
					record.Key.AssetType,
					record.Failures.ToArray()
				);

				record.State = AssetState.Failed;
				return;
			}

			record.State = AssetState.Queued;

			if (reason is not null || exception is not null) {
				record.Failures.Add(
					new AssetSourceFailure(
						sources[nextSourceIndex - 1],
						reason,
						exception
					)
				);
			}

			record.PrepareTask = Task.Run(() => PrepareAsync(record, version, nextSourceIndex));
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

	private static string NormalizePath(string path)
	{
		return path.Replace('\\', '/');
	}

	private static T GetDefaultValue<T>() where T : class
	{
		return null!;
	}
}