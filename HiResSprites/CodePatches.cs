using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace HiResSprites
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.Draw), new Type[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) })]
        public class SpriteBatch_Draw_Patch2
        {
            public static void Prefix(SpriteBatch __instance, Texture2D texture, ref float scale)
            {
                if (!Config.EnableMod || texture?.Name is null)
                    return;
                if (scaleDict.TryGetValue(texture.Name, out TextureData data))
                {
                    scale /= data.scale;
                }
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.getSourceRectForStandardTileSheet))]
        public class GameLocation_getSourceRectForStandardTileSheet_Patch
        {
            public static void Prefix(Texture2D tileSheet, ref int width, ref int height)
            {
                if (!Config.EnableMod || !scaleDict.TryGetValue(tileSheet.Name, out TextureData data))
                    return;
                width = (int)(width * data.scale);
                height = (int)(height * data.scale);

            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.getSourceRectForObject))]
        public class GameLocation_getSourceRectForObject_Patch
        {
            public static bool Prefix(int tileIndex, ref Rectangle __result)
            {
                if (!Config.EnableMod || !scaleDict.TryGetValue("Maps/springobjects", out TextureData data))
                    return true;
                int newSize = (int)(16 * data.scale);
                __result = new Rectangle(tileIndex * newSize % Game1.objectSpriteSheet.Width, tileIndex * newSize / Game1.objectSpriteSheet.Width * newSize, newSize, newSize);
                return false;

            }
        }
    }
}