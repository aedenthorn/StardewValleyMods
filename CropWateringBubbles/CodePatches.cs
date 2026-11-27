using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Tools;
using StardewValley.TerrainFeatures;

namespace CropWateringBubbles
{
	public partial class ModEntry
	{
		public class Crop_draw_Patch
		{
			public static void Postfix(Crop __instance, SpriteBatch b, Vector2 tileLocation)
			{
				DrawWateringBubble(__instance, b, tileLocation);
			}
		}

		public class Crop_drawWithOffset_Patch
		{
			public static void Postfix(Crop __instance, SpriteBatch b, Vector2 tileLocation, Vector2 offset)
			{
				DrawWateringBubble(__instance, b, tileLocation, (int)offset.Y - 32);
			}
		}

		private static void DrawWateringBubble(Crop __instance, SpriteBatch b, Vector2 tileLocation, int offset = 0)
		{
			HoeDirt hoeDirt = __instance.currentLocation?.GetHoeDirtAtTile(__instance.tilePosition);

			if (!Config.ModEnabled || !isEmoting.Value || hoeDirt is null || __instance.dead.Value || hoeDirt.state.Value != 0 || (__instance.currentPhase.Value >= __instance.phaseDays.Count - 1 && (!__instance.fullyGrown.Value || __instance.dayOfCurrentPhase.Value <= 0) && !CanBecomeGiant(hoeDirt)) || __instance.isPaddyCrop() || (Config.OnlyWhenWatering && Game1.player.CurrentTool is not WateringCan))
				return;

			float base_sort = (float)((tileLocation.Y + 1) * 64) / 10000f + tileLocation.X / 50000f;
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2) + offset;
			float movePercent = (100 - Config.SizePercent) / 100f;

			b.Draw(Game1.emoteSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64 + movePercent * 32 + Config.XOffset, tileLocation.Y * 64 - 64 - 16 + yOffset + movePercent * 64 + Config.YOffset)), new Rectangle?(new Rectangle(currentEmoteFrame.Value * 16 % Game1.emoteSpriteSheet.Width, currentEmoteFrame.Value * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16)), Color.White * (Config.OpacityPercent / 100f), 0f, Vector2.Zero, 4f * Config.SizePercent / 100f, SpriteEffects.None, base_sort + 1E-06f);
		}
	}
}
