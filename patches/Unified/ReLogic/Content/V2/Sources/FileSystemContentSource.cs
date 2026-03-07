using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReLogic.Content.Sources;

public sealed class FileSystemContentSource : AbstractContentSource, IWatchableContentSource
{
	public string FileWatcherPath => basePath;

	public int FileCount => nameToAbsolutePath.Count;

	private readonly string basePath;
	private readonly Dictionary<string, string> nameToAbsolutePath = new();

	public FileSystemContentSource(string basePath)
	{
		this.basePath = Path.GetFullPath(basePath);
		if (!this.basePath.EndsWith("/") && !this.basePath.EndsWith("\\")) {
			this.basePath += Path.DirectorySeparatorChar;
		}

		Refresh();

		foreach (var pair in AssetExtensions.ToArray()) {
			AssetExtensions.TryAdd(pair.Key + pair.Value, pair.Value);
		}
	}

	public override Stream OpenStream(string assetName)
	{
		if (!nameToAbsolutePath.TryGetValue(assetName, out var value)) {
			throw new FileNotFoundException(assetName);
		}

		if (!File.Exists(value)) {
			throw new FileNotFoundException(assetName);
		}

		try {
			return File.OpenRead(value);
		}
		catch (Exception e) {
			throw new FileNotFoundException(assetName, e);
		}
	}

	public override void Refresh()
	{
		nameToAbsolutePath.Clear();
		if (Directory.Exists(basePath)) {
			var files = Directory.GetFiles(basePath, "*", SearchOption.AllDirectories);
			foreach (var file in files) {
				var fullPath = Path.GetFullPath(file);
				var path = fullPath.Substring(basePath.Length);
				path = AssetPathHelper.CleanPath(path);
				nameToAbsolutePath[path] = fullPath;
			}
		}

		SetAssetNames(nameToAbsolutePath.Keys);
	}
}