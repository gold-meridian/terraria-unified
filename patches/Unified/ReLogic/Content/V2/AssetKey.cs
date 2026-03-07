#nullable enable

using System;

namespace ReLogic.Content;

/// <summary>
///		Identifies an <see cref="AssetRecord"/>
/// </summary>
internal readonly record struct AssetKey(
	Type AssetType,
	string Path
);