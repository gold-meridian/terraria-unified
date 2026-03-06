using System;

namespace ReLogic.Content;

partial class Asset<T>
{
	public static T DefaultValue { get; set; }

	private T ownValue;

	public Action Continuation { get; set; }

	public Action Wait { get; set; }
}
