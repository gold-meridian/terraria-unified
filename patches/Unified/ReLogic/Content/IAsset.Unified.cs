using System;

namespace ReLogic.Content;

partial interface IAsset
{
	Action Continuation { get; set; }
}
