using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace ParrotPerch
{
    internal class PerchParrot : EmilysParrot
    {
        public Vector2 tile;

        public PerchParrot(Vector2 location, Vector2 tile) : base(location)
        {
            this.tile = tile;
            layerDepth = 1;
        }
    }
}