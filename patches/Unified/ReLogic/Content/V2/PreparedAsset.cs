#nullable enable

using ReLogic.Content.Readers;
using ReLogic.Content.Sources;

namespace ReLogic.Content;

public readonly record struct PreparedAsset(
	AssetRecord Record,
	IAssetReader Reader,
	AssetLoadPlan Plan,
	int CandidateIndex,
	object PreparedData,
	int Version
);