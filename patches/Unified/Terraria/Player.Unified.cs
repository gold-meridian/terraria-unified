using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Unified.Features;

namespace Terraria;

partial class Player
{
	public HashSet<Point16> TilesWithinCraftingReach { get; } = [];

	public Rectangle CraftingReachRectangle { get; set; }

	private void UpdateCraftingReachAnimFrames()
	{
		switch (CraftingReachPreview.Mode) {
			case CraftingReachPreview.PreviewMode.OutlineAndBox:
				BoxStep(1);
				OutlineStep(1);
				break;

			case CraftingReachPreview.PreviewMode.OutlineOnly:
				BoxStep(-1);
				OutlineStep(1);
				break;

			default:
				BoxStep(-1);
				OutlineStep(-1);
				break;
		}

		CraftingReachPreview.OutlineAnim = Easing_QuadInOut(CraftingReachPreview.Outline.Current / (float)CraftingReachPreview.Outline.Max);
		CraftingReachPreview.BoxAnim = Easing_QuadInOut(CraftingReachPreview.Box.Current / (float)CraftingReachPreview.Box.Max);

		return;

		static void OutlineStep(int dir)
		{
			if (dir == -1) {
				if (CraftingReachPreview.Outline.Current > 0) {
					CraftingReachPreview.Outline.Current--;
				}
			}
			else {
				if (CraftingReachPreview.Outline.Current < CraftingReachPreview.Outline.Max) {
					CraftingReachPreview.Outline.Current++;
				}
			}
		}

		static void BoxStep(int dir)
		{
			if (dir == -1) {
				if (CraftingReachPreview.Box.Current > 0) {
					CraftingReachPreview.Box.Current--;
				}
			}
			else {
				if (CraftingReachPreview.Box.Current < CraftingReachPreview.Box.Max) {
					CraftingReachPreview.Box.Current++;
				}
			}
		}

		static float Easing_QuadInOut(float t)
		{
			return t < 0.5f ? 2f * t * t : 1f - 2f * (1f - t) * (1f - t);
		}
	}
}
