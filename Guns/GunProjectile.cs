using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Projectiles;
using System;

namespace Guns
{
    internal class GunProjectile : BasicProjectile
    {


        public GunProjectile(float rotation, float scale, int damage, int index, int bounces, int tail, int rotVelocity, float xVelocity, float yVelocity, Vector2 startPos, string collisionSound, string firingSound, bool explode, bool damageMonsters, GameLocation location, Farmer firer, bool spriteFromObjSheet, onCollisionBehavior behavior) : base(damage, index, bounces, tail, rotVelocity, xVelocity, yVelocity, startPos, collisionSound, firingSound, explode, damageMonsters, location, firer, spriteFromObjSheet, behavior)
        {
            ignoreTravelGracePeriod.Value = true;
            this.rotation = rotation;
            localScale = scale;
            ignoreLocationCollision.Value = false;
        }

        public override bool isColliding(GameLocation location)
        {
            return location.isCollidingPosition(getBoundingBox(), Game1.viewport, false, 0, false, theOneWhoFiredMe.Get(location), false, true, false) || location.doesPositionCollideWithCharacter(getBoundingBox(), false) != null;
        }
    }
}