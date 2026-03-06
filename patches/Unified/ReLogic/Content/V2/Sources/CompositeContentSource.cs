#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ReLogic.Content.Sources;

public sealed class CompositeContentSource(params IContentSource[] sources) : IContentSource
{
	public IEnumerable<string> EnumerateAssets()
	{
		var uniquePaths = new HashSet<string>();
		foreach (var source in sources) {
			foreach (var path in source.EnumerateAssets()) {
				uniquePaths.Add(path);
			}
		}

		return uniquePaths;
	}

	public string? GetExtension(string assetName)
	{
		foreach (var source in sources) {
			if (source.GetExtension(assetName) is { } extension) {
				return extension;
			}
		}

		return null;
	}

	public async ValueTask<Stream> OpenStreamAsync(string path, CancellationToken cancellationToken)
	{
		foreach (var source in sources) {
			if (!source.HasAsset(path)) {
				continue;
			}

			return await source.OpenStreamAsync(path, cancellationToken).ConfigureAwait(false);
		}

		throw new FileNotFoundException($"Asset not found: {path}");
	}

	public void Refresh()
	{
		foreach (var source in sources) {
			source.Refresh();
		}
	}
}