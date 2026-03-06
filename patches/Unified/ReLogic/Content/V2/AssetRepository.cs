#nullable enable

namespace ReLogic.Content;

public sealed class AssetRepository(AssetPipeline pipeline)
{
	public Asset<T> Request<T>(string assetName, AssetRequestMode mode = AssetRequestMode.ImmediateLoad) where T : class
	{
		return pipeline.Request<T>(assetName, mode);
	}
}