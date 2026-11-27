using System.Collections.Generic;

namespace ChestFullnessTextures
{
    public class ChestTextureDataShell
    {
        public List<ChestTextureData> Entries;
    }
    public class ChestTextureData
    {
        public string texturePath;
        public int items;
        public int frames;
        public int ticksPerFrame;
        public int xOffset;
        public int yOffset;
        public int tileWidth;
        public int tileHeight;
    }
}