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
		public Fishie() : base()
		{
		}

		public Fishie(Vector2 position) : base(position)
		{
			Sprite.LoadTexture("Fishies/" + ModEntry.fishTextures[Game1.random.Next(ModEntry.fishTextures.Count)]);
			Scale = ((float)Game1.random.NextDouble() + 0.1f) * 0.35f;
			DamageToFarmer = 0;
			moveTowardPlayerThreshold.Value = 50;
			hideShadow.Value = true;

		}
		public override void drawAboveAllLayers(SpriteBatch b)
		{
			invincibleCountdown = 1000;
			if (Utility.isOnScreen(base.Position, 128))
			{
				b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(64f, (float)this.GetBoundingBox().Height), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, (float)(base.getStandingY() - 1) / 10000f);
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(64f, (float)(this.GetBoundingBox().Height / 2)), new Rectangle?(this.Sprite.SourceRect), Color.White, this.rotation, new Vector2(16f, 16f), Math.Max(0.2f, this.scale) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)(base.getStandingY() + 8) / 10000f)));
				if (this.isGlowing)
				{
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(64f, (float)(this.GetBoundingBox().Height / 2)), new Rectangle?(this.Sprite.SourceRect), this.glowingColor * this.glowingTransparency, this.rotation, new Vector2(16f, 16f), Math.Max(0.2f, this.scale) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)(base.getStandingY() + 8) / 10000f + 0.0001f)));
				}
			}
		}
		protected override void updateAnimation(GameTime time)
		{
			base.updateAnimation(time);
			if (this.wasHitCounter >= 0)
			{
				this.wasHitCounter -= time.ElapsedGameTime.Milliseconds;
			}
			this.Sprite.Animate(time, 0, 9, 40f);
			float xSlope = (float)(-(float)(base.Player.GetBoundingBox().Center.X - this.GetBoundingBox().Center.X));
			float ySlope = (float)(base.Player.GetBoundingBox().Center.Y - this.GetBoundingBox().Center.Y);
			float t = Math.Max(1f, Math.Abs(xSlope) + Math.Abs(ySlope));
			if (t < 64f)
			{
				this.xVelocity = Math.Max(-7f, Math.Min(7f, this.xVelocity * 1.1f));
				this.yVelocity = Math.Max(-7f, Math.Min(7f, this.yVelocity * 1.1f));
			}
			xSlope /= t;
			ySlope /= t;
			if (this.wasHitCounter <= 0)
			{
				this.targetRotation = (float)Math.Atan2((double)(-(double)ySlope), (double)xSlope) - 1.57079637f;
				if ((double)(Math.Abs(this.targetRotation) - Math.Abs(this.rotation)) > 2.748893571891069 && Game1.random.NextDouble() < 0.5)
				{
					this.turningRight = true;
				}
				else if ((double)(Math.Abs(this.targetRotation) - Math.Abs(this.rotation)) < 0.39269908169872414)
				{
					this.turningRight = false;
				}
				if (this.turningRight)
				{
					this.rotation -= (float)Math.Sign(this.targetRotation - this.rotation) * 0.0490873866f;
				}
				else
				{
					this.rotation += (float)Math.Sign(this.targetRotation - this.rotation) * 0.0490873866f;
				}
				this.rotation %= 6.28318548f;
				this.wasHitCounter = 5 + Game1.random.Next(-1, 2);
			}
			float maxAccel = Math.Min(7f, Math.Max(2f, 7f - t / 64f / 2f));
			xSlope = (float)Math.Cos((double)this.rotation + 1.5707963267948966);
			ySlope = -(float)Math.Sin((double)this.rotation + 1.5707963267948966);
			this.xVelocity += -xSlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
			this.yVelocity += -ySlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
			if (Math.Abs(this.xVelocity) > Math.Abs(-xSlope * 7f))
			{
				this.xVelocity -= -xSlope * maxAccel / 6f;
			}
			if (Math.Abs(this.yVelocity) > Math.Abs(-ySlope * 7f))
			{
				this.yVelocity -= -ySlope * maxAccel / 6f;
			}
			base.resetAnimationSpeed();
		}

		private float targetRotation;
		private bool turningRight;
		private int wasHitCounter;

	}
}
