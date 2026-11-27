using System;
using StardewValley;

namespace BatForm
{
	public partial class ModEntry
	{
		public class Farmer_draw_Patch
		{
			public static bool Prefix(Farmer __instance)
			{
				if (Config.ModEnabled && BatFormStatus(__instance) != BatForm.Inactive)
					return false;
				return true;
			}
		}

		public class Options_zoomLevel_Patch
		{
			public static void Postfix(ref float __result)
			{
				if (!Config.ModEnabled || !Config.ZoomOutEnabled || Game1.currentLocation == null || BatFormStatus(Game1.player) == BatForm.Inactive)
					return;

				float nextZoomLevel = __result - height.Value / 100f;
				int nextViewportWidth = (int) Math.Ceiling(Game1.game1.localMultiplayerWindow.Width * (1.0 / (double) nextZoomLevel));
				int nextViewportHeight = (int) Math.Ceiling(Game1.game1.localMultiplayerWindow.Height * (1.0 / (double) nextZoomLevel));
				bool viewportWidthHasOrWillOverflowMapWidth = Game1.viewport.Size.Width > Game1.currentLocation.Map.DisplayWidth || nextViewportWidth > Game1.currentLocation.Map.DisplayWidth;
				bool viewportHeightHasOrWillOverflowMapHeight = Game1.viewport.Size.Height > Game1.currentLocation.Map.DisplayHeight || nextViewportHeight > Game1.currentLocation.Map.DisplayHeight;

				if (viewportWidthHasOrWillOverflowMapWidth || viewportHeightHasOrWillOverflowMapHeight || (BatFormStatus(Game1.player) == BatForm.SwitchingFrom && height.Value >= heightViewportLimit.Value))
				{
					float zoomLevelBasedOnMaxViewportWidth = (float) Game1.game1.localMultiplayerWindow.Width / Game1.currentLocation.Map.DisplayWidth;
					float zoomLevelBasedOnMaxViewportHeight = (float) Game1.game1.localMultiplayerWindow.Height / Game1.currentLocation.Map.DisplayHeight;

					if (viewportWidthHasOrWillOverflowMapWidth && viewportHeightHasOrWillOverflowMapHeight)
					{
						__result = Math.Min(__result, Math.Max(zoomLevelBasedOnMaxViewportWidth, zoomLevelBasedOnMaxViewportHeight));
					}
					else
					{
						if (viewportWidthHasOrWillOverflowMapWidth)
							__result = Math.Min(__result, zoomLevelBasedOnMaxViewportWidth);
						else
							__result = Math.Min(__result, zoomLevelBasedOnMaxViewportHeight);
					}
					if (heightViewportLimit.Value == maxHeight)
						heightViewportLimit.Value = height.Value;
				}
				else
				{
					__result = nextZoomLevel;
				}
			}
		}

		public class FarmerSprite_checkForFootstep_Patch
		{
			public static bool Prefix(Farmer ___owner)
			{
				if (!Config.ModEnabled || BatFormStatus(Game1.player) == BatForm.Inactive || ___owner != Game1.player)
					return true;
				return false;
			}
		}

		public class Grass_doCollisionAction_Patch
		{
			public static bool Prefix(Character who)
			{
				if (!Config.ModEnabled || BatFormStatus(Game1.player) == BatForm.Inactive || who != Game1.player)
					return true;
				return false;
			}
		}

		public class Farmer_takeDamage_Patch
		{
			public static bool Prefix(Farmer __instance)
			{
				if (!Config.ModEnabled || BatFormStatus(Game1.player) == BatForm.Inactive || __instance != Game1.player)
					return true;
				return false;
			}
		}

		public class Game1_pressActionButton_Patch
		{
			public static bool Prefix()
			{
				if (!Config.ModEnabled || Config.ActionsEnabled || BatFormStatus(Game1.player) == BatForm.Inactive)
					return true;
				return false;
			}
		}

		public class Game1_pressUseToolButton_Patch
		{
			public static bool Prefix()
			{
				if (!Config.ModEnabled || Config.ActionsEnabled || BatFormStatus(Game1.player) == BatForm.Inactive)
					return true;
				return false;
			}
		}
	}
}
