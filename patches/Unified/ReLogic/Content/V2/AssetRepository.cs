#nullable enable

using System.Collections.Generic;
using System.IO;
using ReLogic.Content.Sources;

namespace ReLogic.Content;

public sealed class AssetRepository(AssetPipeline pipeline)
{
	public Asset<T> Request<T>(string assetName, AssetRequestMode mode = AssetRequestMode.ImmediateLoad) where T : class
	{
		return pipeline.Request<T>(assetName, mode);
	}

	public void SetSources(IReadOnlyList<IContentSource> sources)
	{
		pipeline.SetSources(sources);
	}

	public Asset<T> CreateUntracked<T>(Stream stream, string extension, AssetRequestMode mode = AssetRequestMode.ImmediateLoad) where T : class
	{
		return pipeline.CreateUntracked<T>(stream, extension, mode);
	}
}