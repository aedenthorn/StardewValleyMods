using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;

namespace AreaOfEffect
{
    public class LinearProjectileInstance
    {
        public Farmer firer;
        public GameLocation location;
        public Vector2 target;
        public HashSet<Vector2> affectedTiles;
        public SpellCastData spell;
    }
}