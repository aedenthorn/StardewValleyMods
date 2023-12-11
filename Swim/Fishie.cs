using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swim
{
    public class Fishie : Serpent
    {
        public List<string> fishTextures = new List<string>()
        {
            "BlueFish",
            "PinkFish",
            "GreenFish",
        };

        public Fishie() : base()
        {
        }

        public Fishie(Vector2 position) : base(position)
        {
            Sprite.LoadTexture("aedenthorn.Swim/Fishies/" + fishTextures[Game1.random.Next(fishTextures.Count)]);
            Scale = ((float)Game1.random.NextDouble() + 0.1f) * 0.35f;
            DamageToFarmer = 0;
            moveTowardPlayerThreshold.Value = 50;

        }
        public override void drawAboveAllLayers(SpriteBatch b)
        {
            invincibleCountdown = 1000;
            if (Utility.isOnScreen(base.Position, 128))
            {
                b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(64f, (float)GetBoundingBox().Height), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, (float)(base.StandingPixel.Y - 1) / 10000f);
                b.Draw(Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(64f, (float)(GetBoundingBox().Height / 2)), new Rectangle?(Sprite.SourceRect), Color.White, rotation, new Vector2(16f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(base.StandingPixel.Y + 8) / 10000f)));
                if (isGlowing)
                {
                    b.Draw(Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(64f, (float)(GetBoundingBox().Height / 2)), new Rectangle?(Sprite.SourceRect), glowingColor * glowingTransparency, rotation, new Vector2(16f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(base.StandingPixel.Y + 8) / 10000f + 0.0001f)));
                }
            }
        }
        protected override void updateAnimation(GameTime time)
        {
            base.updateAnimation(time);
            if (wasHitCounter >= 0)
            {
                wasHitCounter -= time.ElapsedGameTime.Milliseconds;
            }
            Sprite.Animate(time, 0, 9, 40f);
            float xSlope = (float)(-(float)(base.Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X));
            float ySlope = (float)(base.Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y);
            float t = Math.Max(1f, Math.Abs(xSlope) + Math.Abs(ySlope));
            if (t < 64f)
            {
                xVelocity = Math.Max(-7f, Math.Min(7f, xVelocity * 1.1f));
                yVelocity = Math.Max(-7f, Math.Min(7f, yVelocity * 1.1f));
            }
            xSlope /= t;
            ySlope /= t;
            if (wasHitCounter <= 0)
            {
                targetRotation = (float)Math.Atan2((double)(-(double)ySlope), (double)xSlope) - 1.57079637f;
                if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) > 2.748893571891069 && Game1.random.NextDouble() < 0.5)
                {
                    turningRight = true;
                }
                else if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) < 0.39269908169872414)
                {
                    turningRight = false;
                }
                if (turningRight)
                {
                    rotation -= (float)Math.Sign(targetRotation - rotation) * 0.0490873866f;
                }
                else
                {
                    rotation += (float)Math.Sign(targetRotation - rotation) * 0.0490873866f;
                }
                rotation %= 6.28318548f;
                wasHitCounter = 5 + Game1.random.Next(-1, 2);
            }
            float maxAccel = Math.Min(7f, Math.Max(2f, 7f - t / 64f / 2f));
            xSlope = (float)Math.Cos((double)rotation + 1.5707963267948966);
            ySlope = -(float)Math.Sin((double)rotation + 1.5707963267948966);
            xVelocity += -xSlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
            yVelocity += -ySlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
            if (Math.Abs(xVelocity) > Math.Abs(-xSlope * 7f))
            {
                xVelocity -= -xSlope * maxAccel / 6f;
            }
            if (Math.Abs(yVelocity) > Math.Abs(-ySlope * 7f))
            {
                yVelocity -= -ySlope * maxAccel / 6f;
            }
            base.resetAnimationSpeed();
        }

        private float targetRotation;
        private bool turningRight;
        private int wasHitCounter;

    }
}
