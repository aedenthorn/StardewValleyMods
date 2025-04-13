using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;

namespace BossCreatures
{
	internal class ToughGhost : Ghost
	{
		private new int wasHitCounter;
		private new bool turningRight;
		private new float targetRotation;

		public ToughGhost()
		{
		}

		public ToughGhost(Vector2 position, float difficulty) : base(position)
		{
			damageToFarmer.Value = (int)Math.Round(damageToFarmer.Value * 2 * difficulty);
			Health = (int)(health.Value * 2 * difficulty);
			Scale = 2;
		}

		public override void onDealContactDamage(Farmer who)
		{
			if (Game1.random.Next(10) >= who.Immunity)
			{
				who.applyBuff(new Buff("18"));
				currentLocation.playSound("debuffHit");
			}
			base.onDealContactDamage(who);
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			faceDirection(0);

			float xSlope = -(float)(Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X);
			float ySlope = Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y;
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
				targetRotation = (float)Math.Atan2(-(double)ySlope, (double)xSlope) - 1.57079637f;
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

			float maxAccel = Math.Min(7f, Math.Max(2f, 7f - t / 64f / 2f))*2;

			xSlope = (float)Math.Cos(rotation + 1.5707963267948966);
			ySlope = -(float)Math.Sin(rotation + 1.5707963267948966);
			xVelocity += -xSlope * maxAccel / 6f + Game1.random.Next(-10, 10) / 100f;
			yVelocity += -ySlope * maxAccel / 6f + Game1.random.Next(-10, 10) / 100f;
			if (Math.Abs(xVelocity) > Math.Abs(-xSlope * 7f))
			{
				xVelocity -= -xSlope * maxAccel / 6f;
			}
			if (Math.Abs(yVelocity) > Math.Abs(-ySlope * 7f))
			{
				yVelocity -= -ySlope * maxAccel / 6f;
			}
		}
	}
}
