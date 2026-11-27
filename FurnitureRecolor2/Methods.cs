using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Object = StardewValley.Object;

namespace FurnitureRecolor
{
    public partial class ModEntry
    {
        public static void MakeColorList(List<Color> list, string str)
        {
            list.Clear();
            var split = str.Split(';');
            for (int i = 0; i < split.Length; i++)
            {
                var c = split[i].Split(',');
                list.Add(new Color(byte.Parse(c[0]), byte.Parse(c[1]), byte.Parse(c[2])));
            }
        }
        public static void CheckTexture(SpriteBatch b, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, Furniture f)
        {
            if (Config.ModEnabled && f.modData.TryGetValue(colorsKey, out var str))
            {
                ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(f.QualifiedItemId);
                string textureName = itemData.TextureName;
                if (texture != itemData.GetTexture())
                {
                    if (texture == SHelper.GameContent.Load<Texture2D>(textureName + "Front"))
                    {
                        textureName += "Front";
                    }
                    else
                    {
                        b.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
                        return;
                    }
                }
                if (tileSheetDict.TryGetValue(f.ItemId, out var dict))
                {
                    List<Color> list = new();
                    List<Color> keys = dict.Keys.ToList();
                    MakeColorList(list, str);
                    if(list.Count == keys.Count)
                    {
                        var offset = sourceRectangle.Value.Location - Furniture.GetDefaultSourceRect(f.ItemId).Location;
                        var rect = new Rectangle(offset, sourceRectangle.Value.Size);
                        for (int i = 0; i < list.Count; i++)
                        {
                            b.Draw(dict[keys[i]], position, rect, list[i], rotation, origin, scale, effects, layerDepth + i / 100000f);
                        }
                        if (transparentDict.TryGetValue(f.ItemId, out var trans))
                        {
                            b.Draw(trans, position, rect, Color.White, rotation, origin, scale, effects, layerDepth + list.Count / 100000f);
                        }
                        return;
                    }
                    f.modData.Remove(colorsKey);
                }
            }
            b.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
            return;
        }
        public static string SanitizeFileName(string itemId)
        {
            return string.Join("_", itemId.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }
        public static string ColorToHexString(Color value)
        {
            return $"#{value.R:X2}{value.G:X2}{value.B:X2}";
        }
    }
}