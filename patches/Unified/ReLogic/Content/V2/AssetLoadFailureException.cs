#nullable enable

using System;
using System.Collections.Generic;
using ReLogic.Content.Sources;

namespace ReLogic.Content;

internal readonly record struct AssetSourceFailure(
	IContentSource? Source,
	string? Reason,
	Exception? Exception
);

internal sealed class AssetLoadFailureException(
	string assetPath,
	Type assetType,
	IReadOnlyList<AssetSourceFailure> failures
) : Exception($"Failed to load asset '{assetPath}' as '{assetType.Name}' from all content sources.")
{
	public IReadOnlyList<AssetSourceFailure> Failures { get; } = failures;
}