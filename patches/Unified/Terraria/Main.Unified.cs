using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.IO;
using Terraria.Map;
using Terraria.Unified.Features;
using Terraria.Unified.Startup;

namespace Terraria;

partial class Main
{
	internal static string UnifiedVersion => "0.2.0";

	internal static bool UnifiedBranding => true;

	public static bool Vsync { get; set; } = true;

	internal static List<TitleLinkButton> UnifiedLinks { get; } = [];

	private static void SaveUnifiedSettings(Preferences config)
	{
		config.Put("CraftingReachPreviewMode", (int)CraftingReachPreview.Mode);
		config.Put(nameof(FixHotbarItemName), FixHotbarItemName.Enabled);
	}

	private static void LoadUnifiedSettings(Preferences config)
	{
		CraftingReachPreview.Mode = (CraftingReachPreview.PreviewMode)config.Get<int>("CraftingReachPreviewMode", 0);
		FixHotbarItemName.Enabled = config.Get<bool>(nameof(FixHotbarItemName), true);
	}

	private static void DrawInterface_4_1_CraftingReach()
	{
		var anim = CraftingReachPreview.BoxAnim;
		var player = LocalPlayer;
		if (anim <= 0f || !playerInventory || player.spectating >= 0) {
			return;
		}

		var range = player.CraftingReachRectangle;
		var pixel = TextureAssets.MagicPixel.Value;

		float r = 0.5f, g = 0.5f, b = 0.7f;
		var color = new Color(r, g, b) * (anim / 4.6f);
		const int thickness = 2;

		var left = range.Left * 16;
		var right = (range.Right + 1) * 16;
		var top = range.Top * 16;
		var bottom = (range.Bottom + 1) * 16;

		var animWidth = (right - left) * anim;
		var animHeight = (bottom - top) * (0.5f + anim * 0.5f);
		var centerX = (left + right) * 0.5f;
		var centerY = (top + bottom) * 0.5f;
		var leftA = centerX - animWidth * 0.5f;
		var rightA = centerX + animWidth * 0.5f;
		var topA = centerY - animHeight * 0.5f;
		var bottomA = centerY + animHeight * 0.5f;

		var screenTL = new Vector2(leftA, topA) - screenPosition;
		var screenBR = new Vector2(rightA, bottomA) - screenPosition;

		// top
		spriteBatch.Draw(pixel,
			ReverseGravitySupport(new Vector2(screenTL.X + thickness, screenTL.Y), thickness),
			new Rectangle(0, 0, (int)(screenBR.X - screenTL.X - thickness * 2), thickness),
			color
		);

		// bottom
		spriteBatch.Draw(pixel,
			ReverseGravitySupport(new Vector2(screenTL.X + thickness, screenBR.Y - thickness), thickness),
			new Rectangle(0, 0, (int)(screenBR.X - screenTL.X - thickness * 2), thickness),
			color
		);

		// left
		spriteBatch.Draw(pixel,
			ReverseGravitySupport(new Vector2(screenTL.X, screenTL.Y), thickness),
			new Rectangle(0, 0, thickness, (int)(screenBR.Y - screenTL.Y)),
			color
		);

		// right
		spriteBatch.Draw(pixel,
			ReverseGravitySupport(new Vector2(screenBR.X - thickness, screenTL.Y), thickness),
			new Rectangle(0, 0, thickness, (int)(screenBR.Y - screenTL.Y)),
			color
		);

		// fill
		spriteBatch.Draw(pixel,
			ReverseGravitySupport(new Vector2(screenTL.X, screenTL.Y), thickness),
			new Rectangle(0, 0, (int)(screenBR.X - screenTL.X), (int)(screenBR.Y - screenTL.Y)),
			color
		);
	}

	public static void ResizeWorldMap()
	{
		Map = new WorldMap(maxTilesX + 1, maxTilesY + 1);
		MapRenderer.Resize();
	}

	public static void ResizeSectionBasedThings()
	{
		sectionManager = new WorldSections(maxTilesX / 200 + 1, maxTilesY / 150 + 1);
		ActiveSections.Resize();
		LeashedEntity.Resize();
	}

	private void InitUnifiedContentManager()
	{
		if (dedServ) {
			return;
		}

		var vanillaContentFolder = GameLaunch.Instance.Host.Services.GetRequiredService<IContentDirectoryResolver>().GetContentDirectory();

		/*
		UnifiedContentManager localOverrideContentManager = null;
		if (Directory.Exists(Path.Combine("Content", "Images"))) {
			localOverrideContentManager = new UnifiedContentManager(Content.ServiceProvider, "Content", null);
		}

		base.Content = new UnifiedContentManager(Content.ServiceProvider, vanillaContentFolder, localOverrideContentManager);
		*/

		// base.Content = new UnifiedContentManager(Content.ServiceProvider, vanillaContentFolder);

		base.Content.RootDirectory = vanillaContentFolder;

		// TODO: Fix file casings.
	}
}
