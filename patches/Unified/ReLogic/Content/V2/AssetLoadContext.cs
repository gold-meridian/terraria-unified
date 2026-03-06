#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ReLogic.Content.Sources;

namespace ReLogic.Content;

public readonly record struct AssetLoadContext(
	string? Path,
	IContentSource? ContentSource,
	Func<Stream>? OpenStream,
	IServiceProvider? Services = null
)
{
	public async ValueTask<Stream> OpenOwnedStreamAsync(CancellationToken cancellationToken)
	{
		if (OpenStream is not null) {
			return OpenStream.Invoke();
		}

		if (Path is not null && ContentSource is not null) {
			return await ContentSource.OpenStreamAsync(Path, cancellationToken);
		}

		throw new InvalidOperationException("Could not open owned stream");
	}
}