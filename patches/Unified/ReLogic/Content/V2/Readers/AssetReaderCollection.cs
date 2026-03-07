#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ReLogic.Content.Readers;

public sealed class AssetReaderCollection
{
	public string[] Extensions { get; private set; } = [];

	private readonly Dictionary<string, IAssetReader> readersByExtension = [];

	public void RegisterReader(IAssetReader reader, params string[] extensions)
	{
		foreach (string text in extensions) {
			readersByExtension[text.ToLower()] = reader;
		}

		Extensions = readersByExtension.Keys.ToArray();
	}

	public bool TryGetReader(string extension, [NotNullWhen(returnValue: true)] out IAssetReader? reader)
	{
		return readersByExtension.TryGetValue(extension.ToLower(), out reader);
	}
}