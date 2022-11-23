using Microsoft.Xna.Framework;
using xTile.Tiles;

namespace DynamicMapTiles
{
    public class PushedTile
    {
        public Tile tile;
        public Point position;
        public Point destination;
        public int dir;
    }
}