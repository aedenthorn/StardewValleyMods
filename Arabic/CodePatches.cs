using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Text;
using Color = Microsoft.Xna.Framework.Color;

namespace Arabic
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.DrawString), new Type[] {typeof(SpriteFont), typeof(string), typeof(Vector2), typeof(Color) })]
        public class SpriteBatch_Patch1
        {
            public static void Prefix(ref SpriteFont spriteFont, ref string text, ref Vector2 position)
            {
                FixForArabic(ref spriteFont, ref text, ref position);
            }
        }
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.DrawString), new Type[] {typeof(SpriteFont), typeof(StringBuilder), typeof(Vector2), typeof(Color) })]
        public class SpriteBatch_Patch2
        {
            public static void Prefix(ref SpriteFont spriteFont, ref StringBuilder text, ref Vector2 position)
            {
                FixForArabic(ref spriteFont, ref text, ref position);
            }
        }
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.DrawString), new Type[] {typeof(SpriteFont), typeof(string), typeof(Vector2), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) })]
        public class SpriteBatch_Patch3
        {
            public static void Prefix(ref SpriteFont spriteFont, ref string text, ref Vector2 position)
            {
                FixForArabic(ref spriteFont, ref text, ref position);
            }
        }
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.DrawString), new Type[] {typeof(SpriteFont), typeof(string), typeof(Vector2), typeof(Color), typeof(float), typeof(Vector2), typeof(Vector2), typeof(SpriteEffects), typeof(float) })]
        public class SpriteBatch_Patch4
        {
            public static void Prefix(ref SpriteFont spriteFont, ref string text, ref Vector2 position)
            {
                FixForArabic(ref spriteFont, ref text, ref position);
            }
        }
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.DrawString), new Type[] {typeof(SpriteFont), typeof(StringBuilder), typeof(Vector2), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) })]
        public class SpriteBatch_Patch5
        {
            public static void Prefix(ref SpriteFont spriteFont, ref StringBuilder text, ref Vector2 position)
            {
                FixForArabic(ref spriteFont, ref text, ref position);
            }
        }
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.DrawString), new Type[] {typeof(SpriteFont), typeof(StringBuilder), typeof(Vector2), typeof(Color), typeof(float), typeof(Vector2), typeof(Vector2), typeof(SpriteEffects), typeof(float) })]
        public class SpriteBatch_Patch6
        {
            public static void Prefix(ref SpriteFont spriteFont, ref StringBuilder text, ref Vector2 position)
            {
                FixForArabic(ref spriteFont, ref text, ref position);
            }
        }
    }
}