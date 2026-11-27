using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StackedItemIcons
{
    public partial class ModEntry 
    {
        [HarmonyPatch(typeof(Object), nameof(Object.drawInMenu))]
        public class Object_drawInMenu_Patch
        {
            public static void Prefix(Object __instance, SpriteBatch spriteBatch, ref Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                if (!Config.EnableMod || !AllowedToShow(__instance) || __instance.Stack < Config.MinForDoubleStack)
                    return;

                var trans = (float)Math.Pow(transparency, 1.5);
                //jiggle = (float)Math.Sin(Game1.ticks / 10f) / 10f;
                float offset = Config.Spacing / 4f;
                var sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, __instance.ParentSheetIndex, 16, 16);
                var pos1 = location + new Vector2(-offset, -offset) * scaleSize * (__instance.Stack >= Config.MinForTripleStack ? 2 : 1) + new Vector2(32f * scaleSize, 32f * scaleSize);
                spriteBatch.Draw(Game1.objectSpriteSheet, pos1, sourceRect, color * trans, 0, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, 0);

                location += new Vector2(offset, offset) * scaleSize;
                if (__instance.Stack >= Config.MinForTripleStack)
                {
                    spriteBatch.Draw(Game1.objectSpriteSheet, pos1 + new Vector2(offset, offset) * 2, sourceRect, color * trans, 0, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, 0);
                    location += new Vector2(offset, offset) * scaleSize;
                }
            }

        }
    }
}