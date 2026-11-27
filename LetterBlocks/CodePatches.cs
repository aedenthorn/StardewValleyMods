using BmFont;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace LetterBlocks
{
	public partial class ModEntry
    {
        public static Dictionary<string, FontFile> fontFileCache = new Dictionary<string, FontFile>();
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class Object_draw_Patch
        {
            public static void Postfix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.ModEnabled || !TryGetBlockData(__instance.ItemId, out var data))
                    return;
                if (!__instance.modData.TryGetValue(letterKey, out var letterData))
                {
                    letterData = $"0,0,{data.DefaultColor}";
                    __instance.modData[letterKey] = letterData;
                }
                __instance.modData[letterKey] = letterData;
                var split = letterData.Split(',');
                var letter = data.Letters[int.Parse(split[0])].ToString();
                var colorI = int.Parse(split[1]);
                var color = data.Colors != null ? data.Colors[Math.Min(data.Colors.Length - 1, colorI)] : DiscreteColorPicker.getColorFromSelection(int.Parse(split[2]));
                if (color.A == 0)
                {
                    colorI = Math.Min(data.Colors.Length - 1, colorI);
                    if (data.Colors2 != null && data.Colors2.Length > colorI)
                    {
                        color = Utility.Get2PhaseColor(data.Colors[colorI], data.Colors2[colorI]);
                    }
                    else
                    {
                        color = Utility.GetPrismaticColor();
                    }
                }
                var layer = (__instance.GetBoundingBoxAt(x, y).Center.Y + 1) / 10000f;
                var pos = Game1.GlobalToLocal(new Vector2(x, y) * 64) + new Vector2(32, 32);
                if (data.SpriteText)
                {
                    FontFile file = null;
                    if (!string.IsNullOrEmpty(data.FontPath))
                    {

                        if(!fontFileCache.TryGetValue(data.FontPath, out var ff))
                        {
                            ff = (FontFile)AccessTools.Method(typeof(SpriteText), "loadFont").Invoke(null, new object[] { data.FontPath });
                            fontFileCache[data.FontPath] = ff;
                        }
                        file = SpriteText.FontFile;
                        SpriteText.FontFile = ff;
                    }
                    int px = (int)(pos.X - SpriteText.getWidthOfString(letter) / 2f * data.FontScale);
                    int py = (int)(pos.Y - SpriteText.getHeightOfString(letter) / 2f * data.FontScale);
                    SpriteText.drawString(spriteBatch, letter, px, py, color: color, layerDepth: layer);
                    
                    if(file != null)
                        SpriteText.FontFile = file;
                }
                else
                {
                    var font = SHelper.GameContent.Load<SpriteFont>(data.FontPath) ?? Game1.dialogueFont;
                    var size = font.MeasureString(letter) * data.FontScale;
                    pos -= new Vector2(size.X / 2, size.Y / 2);
                    spriteBatch.DrawString(font, letter, pos, color, 0, Vector2.Zero, data.FontScale, SpriteEffects.None, layer);
                }

            }
        }
    }
}
