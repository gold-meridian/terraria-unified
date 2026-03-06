using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ReLogic.Content;
using ReLogic.Content.Sources;
using Terraria.Initializers;

namespace Terraria.Unified;

internal static class ContentSources
{
	public sealed class AssemblyResourcesContentSource : ContentSource
	{
		private readonly string rootPath;
		private readonly Assembly assembly;
		private readonly string[] excludedStartingPaths;

		public override string FileWatcherPath => null;

		public AssemblyResourcesContentSource(Assembly assembly, string rootPath = null, IEnumerable<string> excludedStartingPaths = null)
		{
			this.rootPath = rootPath ?? "";
			this.assembly = assembly;
			this.excludedStartingPaths = excludedStartingPaths?.ToArray() ?? [];

			Refresh();
		}

		public override Stream OpenStream(string assetName) => assembly.GetManifestResourceStream(rootPath + assetName + GetExtension(assetName));

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

	public static AssetRepository ManifestAssets { get; set; }

	public static AssemblyResourcesContentSource ManifestContentSource { get; set; }

	public static void PrepareAssets()
	{
		ManifestContentSource = new AssemblyResourcesContentSource(
			Assembly.GetExecutingAssembly(),
			excludedStartingPaths: []
		);

		ManifestAssets = new AssetRepository(AssetInitializer.assetReaderCollection, [ManifestContentSource]) {
			AssetLoadFailHandler = Main.instance.OnceFailedLoadingAnAsset,
		};
	}
}
