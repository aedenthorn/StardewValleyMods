using Microsoft.Xna.Framework.Graphics;

namespace CustomWallpaperFramework
{
    public class WallpaperData
    {
        public string id;
        public string texturePath;
        public Texture2D texture;
        public int width = 1;
        public int height = 3;
        public float scale = 4;
        public bool isFloor;
    }
}