using System;
using System.Collections.Generic;
using System.Text;

namespace Terraria.DataStructures;

public static partial class ActiveSections
{
	public static void Resize()
	{
		LastActiveTime = new uint[Main.maxTilesX / 200 + 1, Main.maxTilesY / 150 + 1];
	}
}
