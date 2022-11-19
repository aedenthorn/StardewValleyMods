using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TextureTweaks
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.Draw), new Type[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color)  })]
        public class SpriteBatch_Draw_Patch_1
        {
            public static void Prefix(ref Texture2D texture, ref Rectangle? sourceRectangle, ref Color color)
            {
                CheckTexture(ref texture, ref sourceRectangle, ref color);

            }
        }
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.Draw), new Type[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color)  })]
        public class SpriteBatch_Draw_Patch_2
        {
            public static void Prefix(ref Texture2D texture, ref Rectangle? sourceRectangle, ref Color color)
            {
                CheckTexture(ref texture, ref sourceRectangle, ref color);

            }
        }
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.Draw), new Type[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(SpriteEffects), typeof(float) })]
        public class SpriteBatch_Draw_Patch_3
        {
            public static void Prefix(ref Texture2D texture, ref Rectangle? sourceRectangle, ref Color color)
            {
                CheckTexture(ref texture, ref sourceRectangle, ref color);

            }
        }
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.Draw), new Type[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2),  typeof(Vector2), typeof(SpriteEffects), typeof(float) })]
        public class SpriteBatch_Draw_Patch_4
        {
            public static void Prefix(ref Texture2D texture, ref Rectangle? sourceRectangle, ref Color color, Vector2 scale)
            {
                CheckScaledTexture(ref texture, ref sourceRectangle, ref color, ref scale);

            }
        }
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.Draw), new Type[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2),  typeof(float), typeof(SpriteEffects), typeof(float) })]
        public class SpriteBatch_Draw_Patch_5
        {
            public static void Prefix(ref Texture2D texture, ref Rectangle? sourceRectangle, ref Color color, float scale)
            {
                CheckScaledTexture(ref texture, ref sourceRectangle, ref color, ref scale);
                scale = 1;
            }
        }
    }
}