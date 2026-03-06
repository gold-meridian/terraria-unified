#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReLogic.Content.Readers;

public enum AssetFinalizeThread
{
	WorkerThread,
	MainThread,
}

public enum AssetPrepareState
{
	Succeeded,
	Rejected,
}

public readonly record struct AssetPrepareResult<T>(
	AssetPrepareState State,
	T PreparedData,
	string? Reason = null,
	Exception? Error = null
)
{
	public bool Succeeded => State == AssetPrepareState.Succeeded;

	public bool Rejected => State == AssetPrepareState.Rejected;

	public static AssetPrepareResult<T> Success(T preparedData)
	{
		return new AssetPrepareResult<T>(
			AssetPrepareState.Succeeded,
			preparedData
		);
	}

	public static AssetPrepareResult<T> Reject(string? reason = null, Exception? error = null)
	{
		return new AssetPrepareResult<T>(
			AssetPrepareState.Rejected,
			default(T)!,
			reason,
			error
		);
	}
}

public enum AssetFinalizeState
{
	Succeeded,
	Rejected,
}

public readonly record struct AssetFinalizeResult<T>(
	AssetFinalizeState State,
	T? Asset,
	string? Reason,
	Exception? Error
)
{
	public bool Succeeded => State == AssetFinalizeState.Succeeded;

	public bool Rejected => State == AssetFinalizeState.Rejected;

	public static AssetFinalizeResult<T> Success(T asset)
	{
		return new AssetFinalizeResult<T>(AssetFinalizeState.Succeeded, asset, null, null);
	}

	public static AssetFinalizeResult<T> Reject(string? reason = null, Exception? error = null)
	{
		return new AssetFinalizeResult<T>(AssetFinalizeState.Rejected, default(T?), reason, error);
	}
}

public interface IAssetReader : IDisposable
{
	Type AssetType { get; }

	AssetFinalizeThread FinalizeThread { get; }

	ValueTask<AssetPrepareResult<object?>> PrepareAsync(AssetLoadContext context, CancellationToken cancellationToken);

	AssetFinalizeResult<object> Finalize(AssetLoadContext context, object preparedData);

	void Dispose(object asset);
}

public interface IAssetReader<TAsset, TData> : IAssetReader
	where TAsset : notnull
	where TData : notnull
{
	Type IAssetReader.AssetType => typeof(TAsset);

	async ValueTask<AssetPrepareResult<object?>> IAssetReader.PrepareAsync(AssetLoadContext context, CancellationToken cancellationToken)
	{
		var attempt = await PrepareAsync(context, cancellationToken);
		return new AssetPrepareResult<object?>(
			attempt.State,
			attempt.PreparedData,
			attempt.Reason,
			attempt.Error
		);
	}

	AssetFinalizeResult<object> IAssetReader.Finalize(AssetLoadContext context, object preparedData)
	{
		var attempt = Finalize(context, (TData)preparedData);
		return new AssetFinalizeResult<object>(attempt.State, attempt.Asset, attempt.Reason, attempt.Error);
	}

	void IAssetReader.Dispose(object asset)
	{
		if (asset is TAsset t) {
			Dispose(t);
		}
	}

	new ValueTask<AssetPrepareResult<TData>> PrepareAsync(AssetLoadContext context, CancellationToken cancellationToken);

	AssetFinalizeResult<TAsset> Finalize(AssetLoadContext context, TData preparedData);

	void Dispose(TAsset asset);
}