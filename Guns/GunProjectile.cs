using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Projectiles;
using System;

namespace Guns
{
    internal class GunProjectile : BasicProjectile
    {


        public GunProjectile(float rotation, float scale, int v1, int v2, int v3, int v4, int v5, float x, float y, Vector2 vector2, string v6, string v7, bool v8, bool v9, GameLocation currentLocation, Farmer player, bool v10, object value) : base(v1, v2, v3, v4, v5, x, y, vector2, v6, v7, v8, v9, currentLocation, player, v10, null)
        {
            this.rotation = rotation;
            localScale = scale;
        }
    }
}