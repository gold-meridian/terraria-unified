#nullable enable

using System;
using System.Collections.Generic;
using ReLogic.Content.Readers;

namespace ReLogic.Content;

public sealed class AssetReaderRegistry
{
	private readonly Dictionary<Type, IAssetReader> readers = [];

	public void Register(IAssetReader reader)
	{
		readers[reader.AssetType] = reader;
	}

	public IAssetReader GetReader(Type assetType)
	{
		if (readers.TryGetValue(assetType, out var reader)) {
			return reader;
		}

		throw new InvalidOperationException($"No asset reader registered for type '{assetType.FullName}'.");
	}
}