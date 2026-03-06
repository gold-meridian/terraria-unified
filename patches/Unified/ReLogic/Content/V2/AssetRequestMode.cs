namespace ReLogic.Content;

/// <summary>
///		The mode in which the asset should be requested.
/// </summary>
public enum AssetRequestMode
{
	/// <summary>
	///		Do not load the asset at all.  This lets you obtain an
	///		<see cref="Asset{T}"/> instance without actually triggering the
	///		asset to be loaded.
	/// </summary>
	DoNotLoad,

	/// <summary>
	///		Begins loading the asset asynchronously, immediately producing an
	///		<see cref="Asset{T}"/> instance with a dummy value until the frame
	///		the asset is loaded.
 	/// </summary>
	AsyncLoad,

	/// <summary>
	///		Triggers an asynchronous asset load but blocks execution until the
	///		asset has loaded, forcing the returned <see cref="Asset{T}"/> to
	///		finish loading.
	/// </summary>
	ImmediateLoad,
}
