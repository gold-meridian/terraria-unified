#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ReLogic.Content.Sources;

namespace ReLogic.Content;

public readonly record struct AssetLoadContext(
	string AssetName,
	string Extension,
	Func<Stream> OpenStream,
	object? SourceTag,
	IServiceProvider? Services = null
)
{
	public Stream OpenOwnedStream()
	{
		return OpenStream.Invoke();
	}
}