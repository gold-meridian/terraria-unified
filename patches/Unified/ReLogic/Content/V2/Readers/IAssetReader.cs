#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReLogic.Content.Readers;

public interface IAssetReader : IDisposable
{
	Type AssetType { get; }

	AssetFinalizeThread FinalizeThread { get; }

	ValueTask<object> PrepareAsync(AssetLoadContext context, CancellationToken cancellationToken);

	object Finalize(AssetLoadContext context, object preparedData);

	void Dispose(object asset);
}

public interface IAssetReader<TAsset> : IAssetReader
	where TAsset : notnull
{
	Type IAssetReader.AssetType => typeof(TAsset);

	async ValueTask<object> IAssetReader.PrepareAsync(AssetLoadContext context, CancellationToken cancellationToken)
	{
		await PrepareAsync(context, cancellationToken);
		return null!;
	}

	object IAssetReader.Finalize(AssetLoadContext context, object preparedData)
	{
		return Finalize(context);
	}

	void IAssetReader.Dispose(object asset)
	{
		if (asset is TAsset t) {
			Dispose(t);
		}
	}

	new ValueTask PrepareAsync(AssetLoadContext context, CancellationToken cancellationToken);

	TAsset Finalize(AssetLoadContext context);

	void Dispose(TAsset asset);
}

public interface IAssetReader<TAsset, TData> : IAssetReader
	where TAsset : notnull
	where TData : notnull
{
	Type IAssetReader.AssetType => typeof(TAsset);

	async ValueTask<object> IAssetReader.PrepareAsync(AssetLoadContext context, CancellationToken cancellationToken)
	{
		return await PrepareAsync(context, cancellationToken);
	}

	object IAssetReader.Finalize(AssetLoadContext context, object preparedData)
	{
		return Finalize(context, (TData)preparedData);
	}

	void IAssetReader.Dispose(object asset)
	{
		if (asset is TAsset t) {
			Dispose(t);
		}
	}

	new ValueTask<TData> PrepareAsync(AssetLoadContext context, CancellationToken cancellationToken);

	TAsset Finalize(AssetLoadContext context, TData preparedData);

	void Dispose(TAsset asset);
}