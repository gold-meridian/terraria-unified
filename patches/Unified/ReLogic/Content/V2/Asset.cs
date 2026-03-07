#nullable enable

namespace ReLogic.Content;

/// <summary>
///		A typed wrapper over an <see cref="AssetRecord"/> providing mechanisms
///		for retrieving and working with its stored value.
/// </summary>
public sealed class Asset<T> where T : class
{
	private readonly AssetRecord record;

	/// <summary>
	///		The default, fallback asset value (for when the asset is not yet
	///		loaded).
	/// </summary>
	public T DefaultValue { get; }

	/// <summary>
	///		Whether the asset has been loaded and has its real value.
	/// </summary>
	public bool IsLoaded => record.State == AssetState.Loaded;

	public string Name => record.Key.Path;

	/// <summary>
	///		Fetches the asset's value, falling back to the
	///		<see cref="DefaultValue"/> if it's not yet loaded.
	/// </summary>
	public T Value {
		get {
			if (record.Value is T value) {
				return value;
			}

			return DefaultValue;
		}
	}

	/// <summary>
	///		A typed wrapper over an <see cref="AssetRecord"/> providing mechanisms
	///		for retrieving and working with its stored value.
	/// </summary>
	internal Asset(AssetRecord record, T defaultValue)
	{
		this.record = record;
		DefaultValue = defaultValue;
	}
}