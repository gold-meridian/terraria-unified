#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace ReLogic.Content.Readers;

public sealed class PngReader(GraphicsDevice graphicsDevice) : IAssetReader<Texture2D, PngReader.ImageHandle>
{
	public readonly record struct ImageHandle(
		int Width,
		int Height,
		nint Pointer,
		int Length
	);

	public AssetFinalizeThread FinalizeThread => AssetFinalizeThread.MainThread;

	public async ValueTask<ImageHandle> PrepareAsync(AssetLoadContext context, CancellationToken cancellationToken)
	{
		await using var stream = await context.ContentSource.OpenStreamAsync(context.Path, cancellationToken).ConfigureAwait(false);

		var pImage = FNA3D.ReadImageStream(stream, out int width, out int height, out int len);
		{
			PreMultiplyAlpha(pImage, len);
		}

		return new ImageHandle(width, height, pImage, len);
	}

	public Texture2D Finalize(AssetLoadContext context, ImageHandle preparedData)
	{
		var tex = new Texture2D(graphicsDevice, preparedData.Width, preparedData.Height);
		{
			tex.SetDataPointerEXT(0, null, preparedData.Pointer, preparedData.Length);
		}
		FNA3D.FNA3D_Image_Free(preparedData.Pointer);

		return tex;
	}

	public void Dispose(Texture2D asset)
	{
		asset.Dispose();
	}

	public void Dispose() { }

	private static unsafe void PreMultiplyAlpha(nint img, int len)
	{
		byte* colors = (byte*)img.ToPointer();

		for (int i = 0; i < len; i += 4) {
			int a = colors[i + 3];
			colors[i] = (byte)(colors[i] * a / byte.MaxValue);
			colors[i + 1] = (byte)(colors[i + 1] * a / byte.MaxValue);
			colors[i + 2] = (byte)(colors[i + 2] * a / byte.MaxValue);
		}
	}
}