using ReLogic.Content.Readers;

namespace ReLogic.Content;

partial class AssetReaderCollection
{
	private string[] _extensions;

	public bool TryGetReader(string extension, out IAssetReader reader)
	{
		return _readersByExtension.TryGetValue(extension.ToLower(), out reader);
	}

	public string[] GetSupportedExtensions()
	{
		return _extensions;
	}
}
