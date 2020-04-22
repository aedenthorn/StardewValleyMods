using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Network;
using System;

namespace BossCreatures
{
    internal class ToughGhost : Ghost
    {

        public ToughGhost(Vector2 position, float difficulty) : base(position)
        {
            damageToFarmer.Value = (int)Math.Round(damageToFarmer.Value * difficulty);
            Health = (int)(health * difficulty);

            Scale = 2;
        }
        public override void onDealContactDamage(Farmer who)
        {
            if (Game1.random.Next(10) >= who.immunity)
            {
                Game1.buffsDisplay.addOtherBuff(new Buff(18));
                currentLocation.playSound("debuffHit", NetAudio.SoundContext.Default);
            }
            base.onDealContactDamage(who);
        }
		private int wasHitCounter;
		private bool turningRight;
		private float targetRotation;

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			this.faceDirection(0);
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
			float maxAccel = Math.Min(7f, Math.Max(2f, 7f - t / 64f / 2f))*2;
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
		}

	}
}