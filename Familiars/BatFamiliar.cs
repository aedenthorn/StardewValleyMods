using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace Familiars
{
	public class BatFamiliar : Bat
	{
		public BatFamiliar()
		{
		}

		public BatFamiliar(Vector2 position, Farmer _owner) : base(position)
		{
			owner = _owner;
			base.Slipperiness = 20 + Game1.random.Next(-5, 6);

			if (ModEntry.Config.DefaultBatColor)
			{
				double d = Game1.random.NextDouble();
				if (d < 0.05)
				{
					base.Name = "Iridium Bat";
					base.parseMonsterInfo("Iridium Bat");
					this.extraVelocity = 1f;
				}
				else if (d < 0.15)
				{
					base.Name = "Lava Bat";
					base.parseMonsterInfo("Lava Bat");
				}
				else if (d < 0.3)
				{
					base.Name = "Frost Bat";
					base.parseMonsterInfo("Frost Bat");
				}
			}
			this.reloadSprite();

			farmerPassesThrough = true;
			this.Halt();
			base.IsWalkingTowardPlayer = false;
			base.HideShadow = true;
			damageToFarmer.Value = 0;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(new INetSerializable[]
			{
				this.wasHitCounter,
				this.lastHitCounter,
				this.turningRight,
				this.seenPlayer,
				this.cursedDoll,
				this.hauntedSkull
			});
		}

		public override void reloadSprite()
		{
			if(Name == "Bat")
            {
				if (this.Sprite == null)
				{
					this.Sprite = new AnimatedSprite(ModEntry.Config.BatTexture);
				}
				else
				{
					this.Sprite.textureName.Value = ModEntry.Config.BatTexture;
				}
				if (!ModEntry.Config.DefaultBatColor)
				{
					typeof(AnimatedSprite).GetField("spriteTexture", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Sprite, Utils.ColorFamiliar(Sprite.Texture, ModEntry.Config.BatMainColor, ModEntry.Config.BatRedColor, ModEntry.Config.BatGreenColor, ModEntry.Config.BatBlueColor));
				}
			}
			else
            {
				if (this.Sprite == null)
				{
					this.Sprite = new AnimatedSprite("Characters\\Monsters\\" + base.Name);
				}
				else
				{
					this.Sprite.textureName.Value = "Characters\\Monsters\\" + base.Name;
				}
			}
			base.HideShadow = true;
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			if (who != null)
			{
				return 0;
			}
			return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
		}


		public override void behaviorAtGameTick(GameTime time)
		{
			invincibleCountdown = 1000;
			if (this.timeBeforeAIMovementAgain > 0f)
			{
				this.timeBeforeAIMovementAgain -= (float)time.ElapsedGameTime.Milliseconds;
			}
			if (lastHitCounter >= 0)
            {
				lastHitCounter.Value -= time.ElapsedGameTime.Milliseconds;
			}

			chargingMonster = false;
			if(lastHitCounter < 0)
            {
				foreach (NPC npc in currentLocation.characters)
				{
					if (ModEntry.familiarTypes.Contains(npc.GetType()))
						continue;

					if (npc is Monster && Utils.monstersColliding(this, (Monster)npc))
					{
						currentLocation.damageMonster(GetBoundingBox(), ModEntry.Config.BatMinDamage, ModEntry.Config.BatMaxDamage, false, owner);
						lastHitCounter.Value = 5000;
						chargingMonster = false;
						break;
					}
					else if (npc is Monster && Utils.withinMonsterThreshold(this, (Monster)npc, 2))
					{
						chargingMonster = true;
						if (currentTarget == null || Vector2.Distance(npc.position, position) < Vector2.Distance(currentTarget.position, position))
						{
							currentTarget = (Monster)npc;
						}
					}
				}
			}

			if (this.wasHitCounter >= 0)
			{
				this.wasHitCounter.Value -= time.ElapsedGameTime.Milliseconds;
			}

			if (chargingMonster || focusedOnFarmers || this.withinPlayerThreshold(6) || this.seenPlayer)
			{
				this.seenPlayer.Value = true;

				Rectangle targetRect = chargingMonster ? currentTarget.GetBoundingBox() : owner.GetBoundingBox();

				float xSlope = (float)(-(float)(targetRect.Center.X - this.GetBoundingBox().Center.X));
				float ySlope = (float)(targetRect.Center.Y - this.GetBoundingBox().Center.Y);
				float t = Math.Max(1f, Math.Abs(xSlope) + Math.Abs(ySlope));
				if (t < (float)((this.extraVelocity > 0f) ? 192 : 64))
				{
					this.xVelocity = Math.Max(-this.maxSpeed, Math.Min(this.maxSpeed, this.xVelocity * 1.05f));
					this.yVelocity = Math.Max(-this.maxSpeed, Math.Min(this.maxSpeed, this.yVelocity * 1.05f));
				}
				xSlope /= t;
				ySlope /= t;
				if (this.wasHitCounter <= 0)
				{
					this.targetRotation = (float)Math.Atan2((double)(-(double)ySlope), (double)xSlope) - 1.57079637f;
					if ((double)(Math.Abs(this.targetRotation) - Math.Abs(this.rotation)) > 2.748893571891069 && Game1.random.NextDouble() < 0.5)
					{
						this.turningRight.Value = true;
					}
					else if ((double)(Math.Abs(this.targetRotation) - Math.Abs(this.rotation)) < 0.39269908169872414)
					{
						this.turningRight.Value = false;
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
					this.wasHitCounter.Value = 0;
				}
				float maxAccel = Math.Min(5f, Math.Max(1f, 5f - t / 64f / 2f)) + this.extraVelocity;
				xSlope = (float)Math.Cos((double)this.rotation + 1.5707963267948966);
				ySlope = -(float)Math.Sin((double)this.rotation + 1.5707963267948966);
				this.xVelocity += -xSlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				this.yVelocity += -ySlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				if (Math.Abs(this.xVelocity) > Math.Abs(-xSlope * this.maxSpeed))
				{
					this.xVelocity -= -xSlope * maxAccel / 6f;
				}
				if (Math.Abs(this.yVelocity) > Math.Abs(-ySlope * this.maxSpeed))
				{
					this.yVelocity -= -ySlope * maxAccel / 6f;
				}
			}
		}

		protected override void updateAnimation(GameTime time)
		{
			if (base.focusedOnFarmers || this.withinPlayerThreshold(6) || this.seenPlayer)
			{
				this.Sprite.Animate(time, 0, 4, 80f);
				if (this.Sprite.currentFrame % 3 == 0 && Utility.isOnScreen(base.Position, 512) && (this.batFlap == null || !this.batFlap.IsPlaying) && Game1.soundBank != null && base.currentLocation == Game1.currentLocation && !this.cursedDoll)
				{
					this.batFlap = Game1.soundBank.GetCue("batFlap");
					this.batFlap.Play();
				}
				if (this.cursedDoll.Value)
				{
					this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.shakeTimer < 0)
					{
						if (!this.hauntedSkull.Value)
						{
							base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 103, 16, 16), this.position + new Vector2(0f, -32f), false, 0.1f, new Color(255, 50, 255) * 0.8f)
							{
								scale = 4f
							});
						}
						this.shakeTimer = 50;
					}
					this.previousPositions.Add(base.Position);
					if (this.previousPositions.Count > 8)
					{
						this.previousPositions.RemoveAt(0);
					}
				}
			}
			else
			{
				this.Sprite.currentFrame = 4;
				this.Halt();
			}
			base.resetAnimationSpeed();
		}


		private readonly NetInt wasHitCounter = new NetInt(0);

		private readonly NetInt lastHitCounter = new NetInt(0);

		private float targetRotation;

		private readonly NetBool turningRight = new NetBool();

		private readonly NetBool seenPlayer = new NetBool();

		private readonly NetBool cursedDoll = new NetBool();

		private readonly NetBool hauntedSkull = new NetBool();

		private ICue batFlap;

		private float extraVelocity;

		private float maxSpeed = 5f;

		private List<Vector2> previousPositions = new List<Vector2>();
        private Farmer owner;
		public Monster currentTarget = null;
		public bool followingPlayer = true;
		private bool chargingMonster;
	}
}
