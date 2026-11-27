using Microsoft.Xna.Framework;

namespace FishTankWallpaper
{
    public class WallpaperData
    {
        public string Texture { get; set; } = ModEntry.fishTankPath;
        public Rectangle WallpaperBounds { get; set; }
    }
}