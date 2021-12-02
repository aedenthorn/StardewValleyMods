using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Network;
using StardewValley.Projectiles;
using System.Collections.Generic;
using xTile.Dimensions;

namespace BossCreatures
{
    public class BossProjectile : BasicProjectile
    {
        private Vector2 startingPosition;
        private bool pullPlayerIn;

        public BossProjectile()
        {

        }
        public BossProjectile(int damageToFarmer, int parentSheetIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, string collisionSound, string firingSound, bool explode, bool damagesMonsters = false, GameLocation location = null, Character firer = null, bool spriteFromObjectSheet = false, BasicProjectile.onCollisionBehavior collisionBehavior = null, int debuff = -1, bool pullIn = false) : base(damageToFarmer, parentSheetIndex, bouncesTillDestruct, tailLength, rotationVelocity, xVelocity, yVelocity, startingPosition, collisionSound, firingSound, explode, damagesMonsters, location, firer, spriteFromObjectSheet, collisionBehavior)
        {
            this.debuff.Value = debuff;
            this.startingPosition = startingPosition;

            pullPlayerIn = pullIn;

        }

        public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player)
        {
            base.behaviorOnCollisionWithPlayer(location, player);
            if (debuff > 0)
            {
                Game1.buffsDisplay.addOtherBuff(new Buff(debuff));
            }
            if (pullPlayerIn)
            {
                Vector2 newPos = player.Position + (startingPosition - player.Position) * 0.02f;
                if(location.isTilePassable(new Location((int)(newPos.X/64), (int)(newPos.Y/64)), Game1.viewport))
                {
                    player.Position = newPos;
                }
            }
        }
    }
}