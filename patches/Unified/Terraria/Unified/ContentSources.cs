using System.Reflection;
using ReLogic.Content;
using ReLogic.Content.Sources;
using Terraria.Initializers;

namespace Terraria.Unified;

internal static class ContentSources
{
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
