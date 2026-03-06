namespace ReLogic.Content;

/// <summary>
///		Represents the asset loading state.
/// </summary>
internal enum AssetState
{
	/// <summary>
	///		The asset is unloaded and is not being prepared to be loaded.
	/// </summary>
	Unloaded,

	/// <summary>
	///		The asset is queued for loading.
	/// </summary>
	Queued,

	/// <summary>
	///		The asset is preparing itself for loading, typically indicating
	///		initial processing of non-thread-dependent data.
	/// </summary>
	Preparing,

	/// <summary>
	///		The asset has prepared itself and is awaiting the main thread to
	///		complete loading.
	/// </summary>
	WaitingForMainThread,

	/// <summary>
	///		The asset has finished loading and is ready to be consumed.
	/// </summary>
	Loaded,

	/// <summary>
	///		For whatever reason, the asset failed to load.
	/// </summary>
	Failed,
}
