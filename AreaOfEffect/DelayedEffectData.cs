using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace AreaOfEffect
{
    public class DelayedEffectData
    {
        public DelayedEffectData(GameLocation l, Farmer who, IEnumerable<Vector2> tiles, SpellEffect e, int ms, int radius = 0)
        {
            Location = l;
            Who = who;
            Tiles = tiles;
            Effect = e;
            Milliseconds = ms;
            Radius = radius;
        }
        public Vector2 Tile => Tiles.Any() ? Tiles.First() : Vector2.Zero;
        public SpellEffect Effect { get; set; }
        public GameLocation Location { get; set; }
        public IEnumerable<Vector2> Tiles { get; set; }
        public Farmer Who { get; set; }
        public int Milliseconds { get; set; }
        public int Radius { get; set; }

    }
}