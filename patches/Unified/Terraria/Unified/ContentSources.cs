using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ReLogic.Content;
using ReLogic.Content.Sources;
using Terraria.GameContent;

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

		public override Stream OpenStream(string assetName)
		{
			return assembly.GetManifestResourceStream(rootPath + '.' + assetName.Replace('\\', '.').Replace('/', ',') + GetExtension(assetName))
				?? throw AssetLoadException.FromMissingAsset(assetName);
		}

		public override void Refresh()
		{
			var resourceNames = (IEnumerable<string>)assembly.GetManifestResourceNames();

			foreach (string startingPath in excludedStartingPaths ?? Enumerable.Empty<string>()) {
				resourceNames = resourceNames.Where(p => !p.StartsWith(startingPath));
			}

			if (!string.IsNullOrEmpty(rootPath)) {
				resourceNames = resourceNames
					.Where(p => p.StartsWith(rootPath))
					.Select(p => p[rootPath.Length..]);
			}

			resourceNames = resourceNames.Select(static p => {
				var ext = Path.GetExtension(p);
				var name = Path.ChangeExtension(p, null);
				return name.Replace('.', '/') + (ext is null ? "" : ext);
			});
			resourceNames = resourceNames
				.Select(x => x.StartsWith('/') ? x[1..] : x)
				.Select(AssetPathHelper.CleanPath);
			SetAssetNames(resourceNames);
		}
	}

	public static void PrepareAssets()
	{
		// new XnaDirectContentSource(((UnifiedContentManager)Content).RootDirectories)
		var vanillaContent = new XnaDirectContentSource([Main.instance.Content.RootDirectory]);
		var unifiedContent = new AssemblyResourcesContentSource(
			Assembly.GetExecutingAssembly(),
			rootPath: "Terraria.Unified.Assets",
			excludedStartingPaths: []
		);

		Main.AssetSourceController = new AssetSourceController(Main.Assets, [
			vanillaContent,
			unifiedContent,
		]);
	}
}
