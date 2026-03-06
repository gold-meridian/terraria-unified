#nullable enable

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ReLogic.Content.Readers;
using ReLogic.Content.Sources;

namespace ReLogic.Content;

public sealed class AssetPipeline(
	IContentSource contentSource,
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
			record.PrepareTask = Task.Run(() => PrepareAsync(record, version));
		}
	}

	private async Task PrepareAsync(AssetRecord record, int version)
	{
		await prepareGate.WaitAsync().ConfigureAwait(false);

		try {
			lock (record.Sync) {
				if (record.Version != version) {
					return;
				}

				record.State = AssetState.Preparing;
			}

			var reader = record.Reader!;
			var context = new AssetLoadContext(record.Key.Path, contentSource, services);
			var preparedData = await reader.PrepareAsync(context, CancellationToken.None).ConfigureAwait(false);

			if (reader.FinalizeThread == AssetFinalizeThread.WorkerThread) {
				var value = reader.Finalize(context, preparedData);

				lock (record.Sync) {
					if (record.Version != version) {
						reader.Dispose(value);
						return;
					}

					record.Value = value;
					record.PreparedData = null;
					record.Error = null;
					record.State = AssetState.Loaded;
				}

				return;
			}

			lock (record.Sync) {
				if (record.Version != version) {
					return;
				}

				record.PreparedData = preparedData;
				record.State = AssetState.WaitingForMainThread;
			}

			preparedQueue.Enqueue(new PreparedAsset(record, reader, preparedData, version));
		}
		catch (Exception e) {
			lock (record.Sync) {
				if (record.Version != version) {
					return;
				}

				record.Error = e;
				record.State = AssetState.Failed;
			}
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

		try {
			var context = new AssetLoadContext(record.Key.Path, contentSource, services);
			var assetValue = prepared.Reader.Finalize(context, prepared.PreparedData);

			lock (record.Sync)
			{
				if (record.Version != prepared.Version)
				{
					prepared.Reader.Dispose(assetValue);
					return;
				}

				record.Value = assetValue;
				record.PreparedData = null;
				record.Error = null;
				record.State = AssetState.Loaded;
			}
		}
		catch (Exception e)
		{
			lock (record.Sync)
			{
				if (record.Version != prepared.Version)
					return;

				record.Error = e;
				record.State = AssetState.Failed;
			}
		}
	}

	private void TryCompleteImmediately(AssetRecord record)
	{
		record.PrepareTask?.GetAwaiter().GetResult();

		if (record is { State: AssetState.WaitingForMainThread, PreparedData: not null, Reader: not null })
		{
			FinalizePrepared(new PreparedAsset(record, record.Reader, record.PreparedData, record.Version));
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