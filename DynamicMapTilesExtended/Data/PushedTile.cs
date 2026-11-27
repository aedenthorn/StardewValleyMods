using Microsoft.Xna.Framework;
using StardewValley;
using xTile.Tiles;

namespace DMT.Data
{
    public record PushedTile
    {
        public Tile Tile { get; set; }

        public Farmer Farmer { get; set; }

        public Point Origin { get; set; }

        public Point Position { get; set; }

        public int Direction { get; set; }

        public Point Destination { get; set; }
    }
}
