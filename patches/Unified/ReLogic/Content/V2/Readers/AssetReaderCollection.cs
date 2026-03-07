#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ReLogic.Content.Readers;

public sealed class AssetReaderCollection
{
	public string[] Extensions { get; private set; } = [];

	private readonly Dictionary<string, Dictionary<Type, IAssetReader>> readersByExtension = [];

	public void RegisterReader(IAssetReader reader, params string[] extensions)
	{
		foreach (string text in extensions) {
			if (!readersByExtension.TryGetValue(text, out var readers)) {
				readersByExtension[text.ToLower()] = readers = [];
			}

			readers[reader.AssetType] = reader;
		}

		Extensions = readersByExtension.Keys.ToArray();
	}

	public bool TryGetReader(Type type, string extension, [NotNullWhen(returnValue: true)] out IAssetReader? reader)
	{
		if (!readersByExtension.TryGetValue(extension.ToLower(), out var readers)) {
			reader = null;
			return false;
		}

		return readers.TryGetValue(type, out reader);
	}
}