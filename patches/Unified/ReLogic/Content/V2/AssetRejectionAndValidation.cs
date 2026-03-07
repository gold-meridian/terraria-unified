using System.Collections.Generic;

namespace ReLogic.Content;

public interface IRejectionReason
{
	string GetReason();
}

public interface IContentValidator
{
	bool AssetIsValid<T>(T content, string contentPath, out IRejectionReason rejectionReason) where T : class;
}

public sealed class RejectedAssetCollection
{
	private readonly Dictionary<string, IRejectionReason> rejectedAssetsAndReasons = new();

	public void Reject(string assetPath, IRejectionReason reason)
	{
		lock (rejectedAssetsAndReasons) {
			rejectedAssetsAndReasons[assetPath] = reason;
		}
	}

	public bool IsRejected(string assetPath)
	{
		lock (rejectedAssetsAndReasons) {
			return rejectedAssetsAndReasons.ContainsKey(assetPath);
		}
	}

	public void Clear()
	{
		lock (rejectedAssetsAndReasons) {
			rejectedAssetsAndReasons.Clear();
		}
	}

	public bool TryGetRejections(List<string> rejectionReasons)
	{
		lock (rejectedAssetsAndReasons) {
			foreach (var rejectedAssetsAndReason in rejectedAssetsAndReasons)
				rejectionReasons.Add(rejectedAssetsAndReason.Value.GetReason());

			return rejectedAssetsAndReasons.Count > 0;
		}
	}
}