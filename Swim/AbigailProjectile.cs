using Microsoft.Xna.Framework;
using StardewValley;
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

        public AbigailProjectile(int damageToFarmer, int parentSheetIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, string collisionSound, string firingSound, bool explode, bool damagesMonsters = false, GameLocation location = null, Character firer = null, bool spriteFromObjectSheet = false, BasicProjectile.onCollisionBehavior collisionBehavior = null) : base(damageToFarmer, parentSheetIndex, bouncesTillDestruct, tailLength, rotationVelocity, xVelocity, yVelocity, startingPosition, collisionSound, firingSound, explode, true, location, firer, true, null)
        {
            IgnoreLocationCollision = true;
            myCollisionSound = collisionSound;
            myExplode = explode;
        }

        public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
        {
            this.explosionAnimation(location);
            if (n is Monster)
            {
                location.characters.Remove(n);
                return;
            }
        }
        private void explosionAnimation(GameLocation location)
        {
            Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(this.spriteFromObjectSheet ? Game1.objectSpriteSheet : Projectile.projectileSheet, this.currentTileSheetIndex, -1, -1);
            sourceRect.X += 28;
            sourceRect.Y += 28;
            sourceRect.Width = 8;
            sourceRect.Height = 8;
            int whichDebris = 12;
            int value = this.currentTileSheetIndex.Value;
            switch (value)
            {
                case 378:
                    whichDebris = 0;
                    break;
                case 379:
                case 381:
                case 383:
                case 385:
                    break;
                case 380:
                    whichDebris = 2;
                    break;
                case 382:
                    whichDebris = 4;
                    break;
                case 384:
                    whichDebris = 6;
                    break;
                case 386:
                    whichDebris = 10;
                    break;
                default:
                    if (value == 390)
                    {
                        whichDebris = 14;
                    }
                    break;
            }
            if (this.spriteFromObjectSheet)
            {
                Game1.createRadialDebris(location, whichDebris, (int)(this.position.X + 32f) / 64, (int)(this.position.Y + 32f) / 64, 6, false, -1, false, -1);
            }
            else
            {
                Game1.createRadialDebris(location, "TileSheets\\Projectiles", sourceRect, 4, (int)this.position.X + 32, (int)this.position.Y + 32, 12, (int)(this.position.Y / 64f) + 1);
            }
            if (myCollisionSound != null && !this.myCollisionSound.Equals(""))
            {
                location.playSound(this.myCollisionSound, NetAudio.SoundContext.Default);
            }
            destroyMe = true;

        }
    }
}