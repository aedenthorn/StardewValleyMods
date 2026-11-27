using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowerColors
{
    public partial class ModEntry
    {
        public static Color PreventSkipWhite(Crop crop)
        {
            if(!Config.ModEnabled || !Config.FixWhiteFlowerDrawing || !crop.programColored.Value)
                return Color.White;
            return Color.Transparent;
        }
        public static bool TryGetColoredCrop(out Crop crop)
        {
            crop = Game1.currentLocation.terrainFeatures.TryGetValue(Game1.currentCursorTile, out var tf) && tf is HoeDirt dirt && dirt.crop is Crop acrop && acrop.programColored.Value ? acrop : null;
            return crop != null;
        }

        public static Color? GetNewColor(List<string> tintColors, Color oldColor, int delta)
        {
            if (tintColors == null)
                return null;
            var color = ColorToHexString(oldColor);
            int which = tintColors.IndexOf(color);
            if (which == -1)
                which = 0;
            which += Math.Sign(delta);
            if (which < 0)
                which = tintColors.Count - 1;
            else if (which >= tintColors.Count)
                which = 0;
            return Utility.StringToColor(tintColors[which]);
        }

        private static string ColorToHexString(Color value)
        {
            return $"#{value.R:X2}{value.G:X2}{value.B:X2}";
        }
        public static void TryReturnObject(Item obj, Farmer who)
        {
            if (obj is null)
                return;
            if (!who.addItemToInventoryBool(obj))
            {
                who.currentLocation.debris.Add(new Debris(obj, who.Position));
            }
        }

        public static void CombineFlowers(ColoredObject co, ColoredObject co2)
        {

            Dictionary<Color, int> colors = new();
            if (co.modData.TryGetValue(colorsKey, out var cs))
            {
                var strs = cs.Split('|');
                foreach (var str in strs)
                {
                    var split = str.Split('=');
                    colors.Add(Utility.StringToColor(split[0]) ?? Color.Transparent, int.Parse(split[1]));
                }

            }
            else
            {
                colors[co.color.Value] = co.Stack;
            }
            if (co2.modData.TryGetValue(colorsKey, out var cs2))
            {
                var strs = cs2.Split('|');
                foreach (var str in strs)
                {
                    var split = str.Split('=');
                    var color = Utility.StringToColor(split[0]) ?? Color.Transparent;
                    if (color == Color.Transparent)
                        continue;
                    int add = int.Parse(split[1]);
                    if (colors.TryGetValue(color, out var count))
                    {
                        colors[color] = count + add;
                    }
                    else
                    {
                        colors.Add(color, add);
                    }
                }
            }
            else
            {
                if (colors.TryGetValue(co2.color.Value, out var count))
                {
                    colors[co2.color.Value] = count + co2.Stack;
                }
                else
                {
                    colors.Add(co2.color.Value, co2.Stack);
                }
            }
            co.modData[colorsKey] = string.Join('|', colors.Select(kvp => $"{ColorToHexString(kvp.Key)}={kvp.Value}"));
            co.Stack = colors.Sum(p => p.Value);
        }
    }
}