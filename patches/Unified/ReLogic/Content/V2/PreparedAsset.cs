#nullable enable

using ReLogic.Content.Readers;

namespace ReLogic.Content;

internal readonly record struct PreparedAsset(
	AssetRecord Record,
	IAssetReader Reader,
	object PreparedData,
	int Version
);