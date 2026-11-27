using xTile.Layers;
using xTile.Tiles;

namespace StardewOpenWorld
{
    public class AnimatedTileData
    {
        public string layer;
        public StaticTileInfo[] tileFrames;
        public long frameInterval;

        public AnimatedTileData(string layer, StaticTileInfo[] tileFrames, long frameInterval)
        {
            this.layer = layer;
            this.tileFrames = tileFrames;
            this.frameInterval = frameInterval;
        }
    }
}