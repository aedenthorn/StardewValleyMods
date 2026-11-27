using Microsoft.Xna.Framework;
using StardewValley;

namespace AreaOfEffect
{
    public class MovingSpriteData
    {
        public Character Parent { get; set; }
        public Vector2 LastPos { get; set; }
        public GameLocation Location { get; set; }
    }
}