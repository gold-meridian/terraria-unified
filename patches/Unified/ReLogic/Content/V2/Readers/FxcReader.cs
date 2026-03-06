#nullable enable

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace ReLogic.Content.Readers;

public sealed class FxcReader(GraphicsDevice graphicsDevice) : IAssetReader<Effect, MemoryStream>
{
	public AssetFinalizeThread FinalizeThread => AssetFinalizeThread.MainThread;

	public async ValueTask<AssetPrepareResult<MemoryStream>> PrepareAsync(AssetLoadContext context, CancellationToken cancellationToken)
	{
		await using var stream = await context.OpenOwnedStreamAsync(cancellationToken);

		var ms = new MemoryStream();
		{
			await stream.CopyToAsync(ms, cancellationToken);
		}

		return AssetPrepareResult<MemoryStream>.Success(ms);
	}

	public AssetFinalizeResult<Effect> Finalize(AssetLoadContext context, MemoryStream preparedData)
	{
		return AssetFinalizeResult<Effect>.Success(new Effect(graphicsDevice, preparedData.ToArray()));
	}

	public void Dispose(Effect asset)
	{
		asset.Dispose();
	}

	public void Dispose() { }
}