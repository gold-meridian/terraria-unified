using System;
using System.Collections.Generic;
using System.Text;

namespace Terraria.GameContent;

public partial class LeashedEntity
{
	public static void Resize()
	{
		BySection = new SectionEntityList[Main.maxTilesX / 200 + 1, Main.maxTilesY / 150 + 1];
	}
}
