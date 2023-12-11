using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;

namespace Swim
{
    public class AbigailProjectile : BasicProjectile
    {
        private string myCollisionSound;
        private bool myExplode;

        public AbigailProjectile(int damageToFarmer, int ParentSheetIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, string collisionSound, string firingSound, bool explode, bool damagesMonsters = false, GameLocation location = null, Character firer = null, bool spriteFromObjectSheet = false, BasicProjectile.onCollisionBehavior collisionBehavior = null) : base(damageToFarmer, ParentSheetIndex, bouncesTillDestruct, tailLength, rotationVelocity, xVelocity, yVelocity, startingPosition, collisionSound, firingSound, null, true, false)
        {
            IgnoreLocationCollision = true;
            myCollisionSound = collisionSound;
            myExplode = explode;
        }

        public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
        {
            explosionAnimation(location);
            if (n is Monster)
            {
                location.characters.Remove(n);
                return;
            }
        }
        private void explosionAnimation(GameLocation location)
        {
            Rectangle sourceRect = this.GetSourceRect();
            sourceRect.X += 28;
            sourceRect.Y += 28;
            sourceRect.Width = 8;
            sourceRect.Height = 8;
            if (base.itemId.Value != null)
            {
                int whichDebris;
                whichDebris = 12;
                switch (base.itemId.Value)
                {
                    case "(O)390":
                        whichDebris = 14;
                        break;
                    case "(O)378":
                        whichDebris = 0;
                        break;
                    case "(O)380":
                        whichDebris = 2;
                        break;
                    case "(O)384":
                        whichDebris = 6;
                        break;
                    case "(O)386":
                        whichDebris = 10;
                        break;
                    case "(O)382":
                        whichDebris = 4;
                        break;
                }
                Game1.createRadialDebris(location, whichDebris, (int)(base.position.X + 32f) / 64, (int)(base.position.Y + 32f) / 64, 6, resource: false);
            }
            else
            {
                Game1.createRadialDebris(location, "TileSheets\\Projectiles", sourceRect, 4, (int)base.position.X + 32, (int)base.position.Y + 32, 12, (int)(base.position.Y / 64f) + 1);
            }
            if (myCollisionSound != null && !myCollisionSound.Equals(""))
            {
                location.playSound(myCollisionSound, null, null, SoundContext.Default);
            }
            destroyMe = true;

        }
    }
}