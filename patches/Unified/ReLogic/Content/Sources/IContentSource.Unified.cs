using System;
using System.Collections.Generic;
using System.Linq;

namespace ReLogic.Content.Sources;

partial interface IContentSource
{
	RejectedAssetCollection Rejections { get; }

	IEnumerable<string> EnumerateAssets();

	bool HasAsset(string assetName)
	{
		return !Rejections.IsRejected(assetName) && GetExtension(assetName) != null;
	}

	IEnumerable<string> GetAllAssetsStartingWith(string assetNameStart, bool ignoreCase = false)
	{
		var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

		return EnumerateAssets().Where(s => s.StartsWith(assetNameStart, comparison));
	}
}
