#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReLogic.Content.Sources;

/// <summary>
///		A content source contract which provides assets.
/// </summary>
public interface IContentSource
{
	/// <summary>
	///		Enumerates every known asset.
	/// </summary>
	/// <returns></returns>
	IEnumerable<string> EnumerateAssets();

	/// <summary>
	///		Gets the file extension for the asset name.
	/// </summary>
	string? GetExtension(string assetName);

	/*
	/// <summary>
	///		Asynchronously opens the stream to the requested asset.
	/// </summary>
	ValueTask<Stream> OpenStreamAsync(string path, CancellationToken cancellationToken);
	*/
	Stream OpenStream(string path);

	/// <summary>
	///		Refreshes the assets known to this content source by triggering it
	///		to recalculate its assets.
	/// </summary>
	void Refresh();
}

/// <summary>
///		A content source with a file watcher path.
/// </summary>
public interface IWatchableContentSource : IContentSource
{
	/// <summary>
	///		The file watcher path to watch for changes.
	/// </summary>
	string FileWatcherPath { get; }
}

public static class ContentSourceExtensions
{
	extension(IContentSource source)
	{
		public bool HasAsset(string assetName)
		{
			return source.GetExtension(assetName) is not null;
		}

		public IEnumerable<string> GetAllAssetsStartingWith(string assetNameStart, bool ignoreCase = false)
		{
			var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			return source.EnumerateAssets().Where(x => x.StartsWith(assetNameStart, comparison));
		}
	}
}