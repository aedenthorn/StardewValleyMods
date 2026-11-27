using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.HomeRenovations;
using System;

namespace Renovations
{
    public class CustomRenovationData
    {
        public string gameLocation;
        public string mapPath;
        public Rect sourceRect = null;
        public Rect destRect = null;

        public Rectangle? SourceRect()
        {
            return sourceRect == null ? null : new Rectangle?(new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height));
        }
        public Rectangle? DestRect()
        {
            return destRect == null ? null : new Rectangle?(new Rectangle(destRect.X, destRect.Y, destRect.Width, destRect.Height));
        }
    }
}