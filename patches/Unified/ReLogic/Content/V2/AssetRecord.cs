#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReLogic.Content.Readers;

namespace ReLogic.Content;

/// <summary>
///		The canonical representation of an asset.
/// </summary>
internal sealed class AssetRecord
{
	/// <summary>
	///		The unique identifier for the asset.
	/// </summary>
	public required AssetKey Key;

	public required object AssetWrapper;

	public volatile AssetState State;
	public object? Value;
	public Exception? Error;

	public Task<PreparedAsset?>? PrepareTask;
	public AssetLoadPlan? LoadPlan;
	public bool IsTracked;
	public int Version;

	public readonly List<AssetCandidateFailure> Failures = [];
	public readonly object Sync = new();
}