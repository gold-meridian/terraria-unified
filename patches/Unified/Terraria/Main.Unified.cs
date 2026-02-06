using System.Collections.Generic;
using Terraria.DataStructures;

namespace Terraria;

partial class Main
{
	internal static string UnifiedVersion => "0.1.1";
	internal static bool UnifiedBranding => true;

	public static bool Vsync { get; set; } = true;

	// 0 - disabled
	// 1 - outline only
	// 2 - outline & box
	public static int CraftingReachPreview = 0;

	internal static List<TitleLinkButton> UnifiedLinks { get; } = [];
}
