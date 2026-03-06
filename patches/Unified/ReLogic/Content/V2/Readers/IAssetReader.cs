#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReLogic.Content.Readers;

public interface IAssetReader
{
	Type AssetType { get; }

	ValueTask<object> PrepareAsync(AssetLoadContext context, CancellationToken cancellationToken);

	object Finalize(AssetLoadContext context, object preparedData);

	void Dispose(object asset);
}

public interface IAssetReader<T> : IAssetReader where T : notnull
{
	Type IAssetReader.AssetType => typeof(T);

	ValueTask<object> IAssetReader.PrepareAsync(AssetLoadContext context, CancellationToken cancellationToken)
	{

	}

	object IAssetReader.Finalize(AssetLoadContext context, object preparedData)
	{
		return Finalize(context, preparedData);
	}

	void IAssetReader.Dispose(object asset)
	{
		if (asset is T t) {
			Dispose(t);
		}
	}

	new T Finalize(AssetLoadContext context, object preparedData);

	void Dispose(T asset);
}