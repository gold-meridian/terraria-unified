#nullable enable

using System;

namespace ReLogic.Content;

/// <summary>
///		Identifies an <see cref="AssetRecord"/>
/// </summary>
/// <param name="AssetType"></param>
/// <param name="Path"></param>
internal readonly record struct AssetKey(
	Type AssetType,
	string Path
);