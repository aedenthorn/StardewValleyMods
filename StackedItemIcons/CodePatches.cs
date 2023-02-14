using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace StackedItemIcons
{
    public partial class ModEntry 
    {
        [HarmonyPatch(typeof(Object), nameof(Object.drawInMenu))]
        public class Object_drawInMenu_Patch
        {
            public static void Prefix(Object __instance, SpriteBatch spriteBatch, ref Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                if (!Config.EnableMod || __instance.Stack < Config.MinForDoubleStack || __instance.bigCraftable.Value)
                    return;

                //jiggle = (float)Math.Sin(Game1.ticks / 10f) / 10f;
                float offset = Config.Spacing / 4f;
                if(__instance.Stack < Config.MinForTripleStack)
                {
                    spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(-offset, -offset) * scaleSize + new Vector2((float)((int)(32f * scaleSize)), (float)((int)(32f * scaleSize))), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, __instance.ParentSheetIndex, 16, 16)), color * transparency, 0, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, 0);

                    location += new Vector2(offset, offset) * scaleSize;
                }
                else
                {
                    spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(-offset, -offset) * 2 * scaleSize + new Vector2((float)((int)(32f * scaleSize)), (float)((int)(32f * scaleSize))), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, __instance.ParentSheetIndex, 16, 16)), color * transparency, 0, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, 0);
                    spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2((float)((int)(32f * scaleSize)), (float)((int)(32f * scaleSize))), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, __instance.ParentSheetIndex, 16, 16)), color * transparency, 0, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, 0);
                    location += new Vector2(offset, offset) * 2 * scaleSize;

                }
            }
        }
    }
}