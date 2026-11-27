using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TextureTweaks
{
    public partial class ModEntry
    {
        public static void CheckTexture(ref Texture2D texture, ref Rectangle? sourceRectangle, ref Color color)
        {
            if (!Config.ModEnabled || textureDict is null || texture.Name is null || !textureDict.TryGetValue(texture.Name, out TextureData data))
                return;
            if (data.scale != 1 && sourceRectangle != null)
            {
                var rect = sourceRectangle.GetValueOrDefault();
                sourceRectangle = new Rectangle?(new Rectangle(rect.Location, new Point((int)Math.Round(rect.Width * data.scale), (int)Math.Round(rect.Height * data.scale))));
            }
        }
        public static void CheckScaledTexture(ref Texture2D texture, ref Rectangle? sourceRectangle, ref Color color, ref Vector2 scale)
        {
            if (!Config.ModEnabled || textureDict is null || texture.Name is null || !textureDict.TryGetValue(texture.Name, out TextureData data))
                return;
            if (data.scale != 1)
            {
                if(sourceRectangle != null)
                {
                    var rect = sourceRectangle.GetValueOrDefault();
                    sourceRectangle = new Rectangle?(new Rectangle(rect.Location, new Point((int)Math.Round(rect.Width * data.scale), (int)Math.Round(rect.Height * data.scale))));
                }
                scale /= data.scale;
            }
        }
        public static void CheckScaledTexture(ref Texture2D texture, ref Rectangle? sourceRectangle, ref Color color, ref float scale)
        {
            if (!Config.ModEnabled || textureDict is null || texture.Name is null || !textureDict.TryGetValue(texture.Name, out TextureData data))
                return;
            if (data.scale != 1)
            {
                if (sourceRectangle != null)
                {
                    var rect = sourceRectangle.GetValueOrDefault();
                    sourceRectangle = new Rectangle?(new Rectangle(new Point((int)Math.Round(rect.X * data.scale), (int)Math.Round(rect.Y * data.scale)), new Point(rect.Width, rect.Height)));
                }
                scale = 0.5f;
            }
        }
    }
}