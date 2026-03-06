#nullable enable

using ReLogic.Content.Readers;
using ReLogic.Content.Sources;

namespace ReLogic.Content;

internal readonly record struct PreparedAsset(
	AssetRecord Record,
	IAssetReader Reader,
	IContentSource Source,
	int SourceIndex,
	object PreparedData,
	int Version
);