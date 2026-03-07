using System.Reflection;
using ReLogic.Content;
using ReLogic.Content.Sources;
using Terraria.Initializers;

namespace Terraria.Unified;

internal static class ContentSources
{
	public static IAssetRepository ManifestAssets { get; set; }

	public static AssemblyContentSource ManifestContentSource { get; set; }

	public static void PrepareAssets()
	{
		ManifestContentSource = new AssemblyContentSource(
			Assembly.GetExecutingAssembly(),
			excludedStartingPaths: []
		);

		ManifestAssets = new AssetPipeline([ManifestContentSource], AssetInitializer.assetReaderCollection, 1);
		ManifestAssets.AssetLoadFailHandler = Main.instance.OnceFailedLoadingAnAsset;
	}
}
