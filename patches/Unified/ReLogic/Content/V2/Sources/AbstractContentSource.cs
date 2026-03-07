#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReLogic.Content.Sources;

public abstract class AbstractContentSource : IContentSource
{
	protected string[] AssetPaths { get; set; } = [];

	protected Dictionary<string, string> AssetExtensions { get; } = new();

	public IEnumerable<string> EnumerateAssets()
	{
		return AssetPaths;
	}

	public string? GetExtension(string assetName)
	{
		return AssetExtensions.GetValueOrDefault(AssetPathHelper.CleanPath(assetName));
	}

	public abstract Stream OpenStream(string path);

	public abstract void Refresh();

	protected void SetAssetNames(IEnumerable<string> paths)
	{
		AssetPaths = paths.ToArray();
		AssetExtensions.Clear();

		foreach (var path in AssetPaths) {
			var ext = Path.GetExtension(path);

			// ReLogic sets all assets to use Path.DirectorySepChar in their
			// paths in AssetPathHelper.
			var name = AssetPathHelper.CleanPath(path[..^ext.Length]);
			if (AssetExtensions.TryGetValue(name, out var ext2)) {
				throw new Exception($"Multiple extensions for asset {name}, ({ext}, {ext2})");
			}

			AssetExtensions[name] = ext;
		}
	}
}