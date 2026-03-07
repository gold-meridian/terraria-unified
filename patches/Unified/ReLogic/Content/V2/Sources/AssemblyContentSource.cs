using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ReLogic.Content.Sources;

public sealed class AssemblyContentSource : AbstractContentSource
{
	private readonly string rootPath;
	private readonly Assembly assembly;
	private readonly string[] excludedStartingPaths;

	public AssemblyContentSource(Assembly assembly, string rootPath = null, IEnumerable<string> excludedStartingPaths = null)
	{
		this.rootPath = rootPath ?? "";
		this.assembly = assembly;
		this.excludedStartingPaths = excludedStartingPaths?.ToArray() ?? [];

		Refresh();
	}

	public override Stream OpenStream(string assetName)
	{
		var stream = assembly.GetManifestResourceStream(rootPath + assetName + GetExtension(assetName));
		if (stream is null) {
			throw new FileNotFoundException(assetName);
		}

		return stream;
	}

	public override void Refresh()
	{
		IEnumerable<string> resourceNames = assembly.GetManifestResourceNames();

		foreach (string startingPath in excludedStartingPaths ?? Enumerable.Empty<string>()) {
			resourceNames = resourceNames.Where(p => !p.StartsWith(startingPath));
		}

		if (!string.IsNullOrEmpty(rootPath)) {
			resourceNames = resourceNames
			               .Where(p => p.StartsWith(rootPath))
			               .Select(p => p.Substring(rootPath.Length));
		}

		SetAssetNames(resourceNames);
	}
}