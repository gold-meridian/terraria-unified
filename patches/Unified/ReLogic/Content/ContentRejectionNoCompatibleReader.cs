namespace ReLogic.Content;

public sealed class ContentRejectionNoCompatibleReader(string extension, string[] supportedExtensions) : IRejectionReason
{
	private readonly string reason = $"Files of type '{extension}' cannot be read. Supported extensions are: {string.Join(" ", supportedExtensions)}";

	public string GetReason()
	{
		return reason;
	}
}
