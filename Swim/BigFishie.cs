using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using System.Collections.Generic;

namespace Swim
{
    public class BigFishie : DinoMonster
    {
        public BigFishie() : base()
        {
        }

        public List<string> bigFishTextures = new List<string>()
        {
            "BigFishBlack",
            "BigFishBlue",
            "BigFishGold",
            "BigFishGreen",
            "BigFishGreenWhite",
            "BigFishGrey",
            "BigFishRed",
            "BigFishWhite"
        };

        public BigFishie(Vector2 position) : base(position)
        {
            Sprite.LoadTexture("aedenthorn.Swim/Fishies/" + bigFishTextures[Game1.random.Next(bigFishTextures.Count)]);
            Scale = 0.5f + (float)Game1.random.NextDouble()/4f;
            DamageToFarmer = 0;
            Slipperiness = 24 + Game1.random.Next(10);
            collidesWithOtherCharacters.Value = false;
            farmerPassesThrough = true;
        }
        public override void drawAboveAllLayers(SpriteBatch b)
        {
            invincibleCountdown = 1000;
        }

        public override void behaviorAtGameTick(GameTime time)
        {
            base.IsWalkingTowardPlayer = false;
            nextChangeDirectionTime -= time.ElapsedGameTime.Milliseconds;
            nextWanderTime -= time.ElapsedGameTime.Milliseconds;
            if (nextChangeDirectionTime < 0)
            {
                nextChangeDirectionTime = Game1.random.Next(500, 1000);
                int facingDirection = base.FacingDirection;
                this.facingDirection.Value = (this.facingDirection.Value + (Game1.random.Next(0, 3) - 1) + 4) % 4;
            }
            if (nextWanderTime < 0)
            {
                if (wanderState)
                {
                    nextWanderTime = Game1.random.Next(1000, 2000);
                }
                else
                {
                    nextWanderTime = Game1.random.Next(1000, 3000);
                }
                wanderState = !wanderState;
            }
            if (wanderState)
            {
                moveLeft = (moveUp = (moveRight = (moveDown = false)));
                base.tryToMoveInDirection(facingDirection.Value, false, base.DamageToFarmer, isGlider.Value);
            }
        }
    }
}
