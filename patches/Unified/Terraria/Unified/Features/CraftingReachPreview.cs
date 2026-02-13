using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.Unified.Features;

internal static class CraftingReachPreview
{
	public enum PreviewMode
	{
		Disabled,
		OutlineOnly,
		OutlineAndBox,
		Max,
	}

	public static (int Current, int Max) Outline = (0, 25);
	public static (int Current, int Max) Box = (0, 25);

	public static float OutlineAnim { get; set; } = 0f;

	public static float BoxAnim { get; set; } = 0f;

	public static PreviewMode Mode { get; set; } = PreviewMode.Disabled;

	public static Color GetCraftingReachColor(float averageTileLighting, float alpha)
	{
		int r = (int)((averageTileLighting / 3f) * alpha);
		int g = (int)((averageTileLighting / 2f) * alpha);
		int b = (int)((averageTileLighting / 1.4f) * alpha);
		return new Color(r, g, b, 1);
	}

	public static bool DrawCraftingReachIcon(int pivotTopLeftX, int pivotTopLeftY, bool pushSideToolsUp, int gamepadPointOffset)
	{
		if (!Main.playerInventory) {
			return false;
		}

		var yPosition = (pushSideToolsUp ? 71 : 92) + pivotTopLeftY;
		var scale = 0.8f;
		var icon = TextureAssets.Extra[ExtrasID.GolfBallMinimapOutline].Value;
		var spriteFrame = icon.Frame(1, 1, 0);
		var hovered = false;
		if (Main.mouseX > pivotTopLeftX && Main.mouseX < pivotTopLeftX + spriteFrame.Width * scale && Main.mouseY > yPosition && Main.mouseY < yPosition + spriteFrame.Height * scale && !PlayerInput.IgnoreMouseInterface) {
			hovered = true;
			Main.LocalPlayer.mouseInterface = true;
			Main.instance.MouseText(Language.GetTextValue("GameUI.CraftingReachPreview" + (int)Mode), 0, 0);
			Main.mouseText = true;
			if (Main.mouseLeft && Main.mouseLeftRelease) {
				SoundEngine.PlaySound(SoundID.Grab);
				SoundEngine.PlaySound(SoundID.Tink);
				Mode = (PreviewMode)((int)(Mode + 1) % (int)PreviewMode.Max);
			}
		}

		Vector2 vector = new Vector2(pivotTopLeftX + 3.5f, yPosition);
		Main.spriteBatch.Draw(icon, vector, spriteFrame, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		Main.spriteBatch.Draw(icon, vector, spriteFrame, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		if (hovered) {
			Main.spriteBatch.Draw(icon, vector, icon.Frame(1, 1, 0), Main.OurFavoriteColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}

		UILinkPointNavigator.SetPosition(6000 + gamepadPointOffset, vector + spriteFrame.Size() * 0.65f);
		return hovered;
	}
}
