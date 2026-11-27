using xTile.Layers;
using xTile.Tiles;

namespace StardewOpenWorld
{
    public class StaticTileInfo
    {
        public string layer;
        public string ts;
        public BlendMode blend;
        public int tileIndex;

        public StaticTileInfo(string layer, string ts, BlendMode blend, int tileIndex)
        {
            this.layer = layer;
            this.ts = ts;
            this.blend = blend;
            this.tileIndex = tileIndex;
        }
    }
}