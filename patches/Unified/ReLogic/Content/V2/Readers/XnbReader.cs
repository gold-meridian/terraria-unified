#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace ReLogic.Content.Readers;

public sealed class InternalContentManager(IServiceProvider serviceProvider) : ContentManager(serviceProvider)
{
	public Stream? Stream { get; set; }

	public T Load<T>() => ReadAsset<T>("XnaAsset", null!);

	protected override Stream OpenStream(string assetName) => Stream!;
}

public sealed class XnbReader<T>(IServiceProvider services) : IAssetReader<T, XnbReader<T>.StreamHandle> where T : notnull
{
	public readonly record struct StreamHandle(
		Stream Stream,
		InternalContentManager ContentLoader,
		T? Asset,
		bool AssetLoaded
	);

	// ReSharper disable once StaticMemberInGenericType
	public static bool LoadOnMainThread { get; set; }

	public AssetFinalizeThread FinalizeThread => LoadOnMainThread ? AssetFinalizeThread.MainThread : AssetFinalizeThread.WorkerThread;

	private readonly ThreadLocal<InternalContentManager> contentLoader = new(() => new InternalContentManager(services));

	public async ValueTask<StreamHandle> PrepareAsync(AssetLoadContext context, CancellationToken cancellationToken)
	{
		var content = contentLoader.Value!;

		var stream = await context.ContentSource.OpenStreamAsync(context.Path, cancellationToken).ConfigureAwait(false);
		if (LoadOnMainThread) {
			return new StreamHandle(
				stream,
				content,
				Asset: default(T?),
				AssetLoaded: false
			);
		}

		return new StreamHandle(
			stream,
			content,
			Asset: LoadAsset(stream, content),
			AssetLoaded: true
		);
	}

	public T Finalize(AssetLoadContext context, StreamHandle preparedData)
	{
		try {
			if (preparedData.AssetLoaded) {
				return preparedData.Asset!;
			}

			return LoadAsset(preparedData.Stream, preparedData.ContentLoader);
		}
		finally {
			preparedData.Stream.Dispose();
		}
	}

	private static T LoadAsset(Stream stream, InternalContentManager contentLoader)
	{
		contentLoader.Stream = stream;
		return contentLoader.Load<T>();
	}

	public void Dispose(T asset) { }

	public void Dispose() { }
}