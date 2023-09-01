using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CropMarkers
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Crop), nameof(Crop.draw))]
        public class Crop_draw_Patch
        {
            public static void Postfix(Crop __instance, SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
            {
                if (!Config.ModEnabled || (Config.RequireKeyPress && !Config.PressKeys.IsDown()) || __instance.forageCrop.Value || __instance.dead.Value || (Config.IgnoreGrown && __instance.currentPhase.Value >= __instance.phaseDays.Count - 1 && (!__instance.fullyGrown.Value || __instance.dayOfCurrentPhase.Value == 0)))
                    return;

                float size = (Config.SizePercent / 100f);
                float opacity = (Config.OpacityPercent / 100f);

                Vector2 pos = Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64 + Config.OffsetX, tileLocation.Y * 64 + Config.OffsetY - 4));
                float base_sort = (float)((tileLocation.Y) * 64 + Config.OffsetY) / 10000f + tileLocation.X / 50000f;
                b.Draw(Game1.bigCraftableSpriteSheet, new Rectangle((int)(pos.X - 8 * size), (int)(pos.Y - 16 * size), (int)(16 * size), (int)(36 * size)), new Rectangle(80, 128, 16, 32), Color.White * opacity, 0, Vector2.Zero, SpriteEffects.None, base_sort - 1.2E-05f);

                b.Draw(Game1.objectSpriteSheet, pos, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, __instance.indexOfHarvest.Value, 16, 16)), Color.White * opacity, 0f, new Vector2(8f, 8f), size, SpriteEffects.None, base_sort - 1.1E-05f);
                if (__instance.programColored.Value)
                {
                    b.Draw(Game1.objectSpriteSheet, pos, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, __instance.indexOfHarvest.Value + 1, 16, 16)), __instance.tintColor.Value * opacity, 0f, new Vector2(8f, 8f), size, SpriteEffects.None, base_sort - 1E-05f);
                }
            }
        }
    }
}