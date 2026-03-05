using System.Collections.Generic;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria;

public partial class MapRenderer
{
	public static void Resize()
	{
		for (int x = 0; x < numTargetsX; x++) {
			for (int y = 0; y < numTargetsY; y++) {
				if (mapTarget[x, y] != null && !mapTarget[x, y].IsDisposed)
					mapTarget[x, y].Dispose();
			}
		}

		numTargetsX = Main.maxTilesX / textureMaxWidth + 1;
		numTargetsY = Main.maxTilesY / textureMaxHeight + 1;
		mapTarget = new RenderTarget2D[numTargetsX, numTargetsY];
		initMap = new bool[numTargetsX, numTargetsY];
		mapWasContentLost = new bool[numTargetsX, numTargetsY];
		changeQueues = new List<Point16>[numTargetsX, numTargetsY];
		for (int x = 0; x < numTargetsX; x++) {
			for (int y = 0; y < numTargetsY; y++) {
				changeQueues[x, y] = new List<Point16>(capacity: ChangeRefreshThreshold);
			}
		}
	}
}