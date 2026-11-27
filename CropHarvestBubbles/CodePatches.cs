using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CropHarvestBubbles
{
	public partial class ModEntry
	{
		public class Crop_draw_Patch
		{
			public static void Postfix(Crop __instance, SpriteBatch b, Vector2 tileLocation)
			{
				DrawHarvestBubble(__instance, b, tileLocation);
			}
		}

		public class Crop_drawWithOffset_Patch
		{
			public static void Postfix(Crop __instance, SpriteBatch b, Vector2 tileLocation, Vector2 offset)
			{
				if (Config.IgnorePots)
					return;
                DrawHarvestBubble(__instance, b, tileLocation, (int)offset.Y - 32);
            }
        }

		private static void DrawHarvestBubble(Crop __instance, SpriteBatch b, Vector2 tileLocation, int offset = 0)
		{
			if (!Config.ModEnabled || (Config.KeyPressToggle && !Config.Toggled)  || (Config.RequireKeyPress && !Config.KeyPressToggle && !Config.PressKeys.IsDown()) || __instance.forageCrop.Value || __instance.dead.Value || __instance.currentPhase.Value < __instance.phaseDays.Count - 1 || (__instance.fullyGrown.Value && __instance.dayOfCurrentPhase.Value > 0) || !Game1.objectData.TryGetValue(__instance.indexOfHarvest.Value, out var value) || (Config.IgnoreFlowers && (value.ContextTags?.Contains("flower_item") == true || value.Category == Object.flowersCategory)))
				return;

			ParsedItemData item = ItemRegistry.GetDataOrErrorItem(__instance.indexOfHarvest.Value);
			float base_sort = (float)((tileLocation.Y + 1) * 64) / 10000f + tileLocation.X / 50000f;
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2) + offset;
			float movePercent = (100 - Config.SizePercent) / 100f;

			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64 - 8 + movePercent * 40, tileLocation.Y * 64 - 96 - 16 + yOffset + movePercent * 96)), new Rectangle?(new Rectangle(141, 465, 20, 24)), Color.White * (Config.OpacityPercent / 100f), 0f, Vector2.Zero, 4f * (Config.SizePercent / 100f), SpriteEffects.None, base_sort + 1E-06f);
			b.Draw(item.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64 + 32, tileLocation.Y * 64 - 64 - 8 + yOffset + movePercent * 56)), item.GetSourceRect(), Color.White * (Config.OpacityPercent / 100f), 0f, new Vector2(8f, 8f), 4f * (Config.SizePercent / 100f), SpriteEffects.None, base_sort + 1E-05f);
			if (__instance.programColored.Value)
			{
				b.Draw(item.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(tileLocation.X * 64 + 32), (float)(tileLocation.Y * 64 - 64 - 8) + yOffset)), item.GetSourceRect(), __instance.tintColor.Value * (Config.OpacityPercent / 100f), 0f, new Vector2(8f, 8f), 4f * (Config.SizePercent / 100f), SpriteEffects.None, base_sort + 1.1E-05f);
			}
		}
	}
}
