using System;

namespace ReLogic.Content;

internal sealed class ContentRejectionAssetReaderException(Exception e) : IRejectionReason
{
	public string GetReason()
	{
		return e.ToString();
	}
}
