using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using Color = Microsoft.Xna.Framework.Color;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Arabic
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(LanguageSelectionMenu), nameof(LanguageSelectionMenu.ApplyLanguageChange))]
        public class LanguageSelectionMenu_ApplyLanguageChange_Patch
        {
            public static void Postfix()
            {
                SHelper.GameContent.InvalidateCache($"Fonts/SpriteFont1_international");

                foreach (var key in languageDict.Keys)
                {
                    SHelper.GameContent.InvalidateCache($"Fonts/SpriteFont1.{key}");
                    SHelper.GameContent.InvalidateCache($"Fonts/SmallFont.{key}");
                    SHelper.GameContent.InvalidateCache($"Fonts/tinyFont.{key}");
                }
                SHelper.GameContent.InvalidateCache($"Fonts/SpriteFont1");
                SHelper.GameContent.InvalidateCache($"Fonts/SmallFont");
                SHelper.GameContent.InvalidateCache($"Fonts/tinyFont");
            }
        }
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
        [HarmonyPatch(typeof(SpriteBatch), nameof(SpriteBatch.DrawString), new Type[] {typeof(SpriteFont), typeof(string), typeof(Vector2), typeof(Color), typeof(float), typeof(Vector2), typeof(Vector2), typeof(SpriteEffects), typeof(float) })]
        public class SpriteBatch_Patch4
        {
            public static void Prefix(ref SpriteFont spriteFont, ref string text, ref Vector2 position)
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