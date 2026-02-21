using System.Collections.Generic;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria
{
    public partial class MapRenderer
    {
		public static void Resize()
		{
			for (int i = 0; i < numTargetsX; i++) {
				for (int j = 0; j < numTargetsY; j++) {
					if (mapTarget[i, j] != null && !mapTarget[i, j].IsDisposed)
						mapTarget[i, j].Dispose();
				}
			}
			numTargetsX = Main.maxTilesX / textureMaxWidth + 1;
			numTargetsY = Main.maxTilesY / textureMaxHeight + 1;
			mapTarget = new RenderTarget2D[numTargetsX, numTargetsY];
			initMap = new bool[numTargetsX, numTargetsY];
			mapWasContentLost = new bool[numTargetsX, numTargetsY];
			changeQueues = new List<Point16>[numTargetsX, numTargetsY];
			for (int i = 0; i < numTargetsX; i++) {
				for (int j = 0; j < numTargetsY; j++) {
					changeQueues[i, j] = new List<Point16>(ChangeRefreshThreshold);
				}
			}
		}
    }
}
