using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReLogic.Content.Sources;

public sealed class XnaDirectContentSource : AbstractContentSource
{
	private readonly string[] _rootDirectories;

	public XnaDirectContentSource(IEnumerable<string> rootDirectories)
	{
		_rootDirectories = rootDirectories.Select(AssetPathHelper.CleanPath).ToArray();
		Refresh();
	}

	public override Stream OpenStream(string assetName)
	{
		try {
			return File.OpenRead(_rootDirectories.Select(rootDir => Path.Combine(rootDir, assetName)).First(File.Exists));
		}
		catch (Exception e) {
			throw new FileNotFoundException(assetName, e);
		}
	}

	public override void Refresh()
	{
		SetAssetNames(
			_rootDirectories
			   .SelectMany(rootDir => Directory.GetFiles(rootDir, "*.xnb", SearchOption.AllDirectories).Select(path => path.Substring(rootDir.Length + 1)))
			   .ToHashSet()
		);
	}
}