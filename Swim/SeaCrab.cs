using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace Swim
{
	public class SeaCrab : RockCrab
	{
		public List<string> crabTextures = new List<string>()
		{
			"HermitCrab",
			"ChestCrab",
		};
		private NetBool shellGone = new NetBool();
        private NetInt shellHealth = new NetInt(5);

		public SeaCrab() : base()
		{
		}

		public SeaCrab(Vector2 position) : base(position)
		{
			Sprite.LoadTexture("Fishies/" + crabTextures[Game1.random.Next(8) < 7 ? 0 : 1]);
			moveTowardPlayerThreshold.Value = 1;
			damageToFarmer.Value = 0;
		}
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(new INetSerializable[]
			{
				this.shellGone,
				this.shellHealth,
			});
			this.position.Field.AxisAlignedMovement = true;
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			return 0;
		}
		public override bool hitWithTool(Tool t)
		{
			if (t is Pickaxe && t.getLastFarmerToUse() != null && this.shellHealth > 0)
			{
				base.currentLocation.playSound("hammer", NetAudio.SoundContext.Default);
				NetInt netInt = this.shellHealth;
				int value = netInt.Value;
				netInt.Value = value - 1;
				base.shake(500);
				this.setTrajectory(Utility.getAwayFromPlayerTrajectory(this.GetBoundingBox(), t.getLastFarmerToUse()));
				if (this.shellHealth <= 0)
				{
					this.shellGone.Value = true;
					base.moveTowardPlayer(-1);
					base.currentLocation.playSound("stoneCrack", NetAudio.SoundContext.Default);
					Game1.createRadialDebris(base.currentLocation, 14, base.getTileX(), base.getTileY(), Game1.random.Next(2, 7), false, -1, false, -1);
					Game1.createRadialDebris(base.currentLocation, 14, base.getTileX(), base.getTileY(), Game1.random.Next(2, 7), false, -1, false, -1);
				}
				return true;
			}
			return base.hitWithTool(t);
		}
		public override void behaviorAtGameTick(GameTime time)
		{
			if (shellGone)
			{
				Sprite.CurrentFrame = 16 + this.Sprite.currentFrame % 4;
			}
			if (withinPlayerThreshold())
			{
				if (Math.Abs(base.Player.GetBoundingBox().Center.Y - this.GetBoundingBox().Center.Y) < Math.Abs(base.Player.GetBoundingBox().Center.X - this.GetBoundingBox().Center.X))
				{
					if (base.Player.GetBoundingBox().Center.X - this.GetBoundingBox().Center.X > 0 && getTileLocationPoint().X > 0)
					{
						this.SetMovingLeft(true);
					}
					else if (base.Player.GetBoundingBox().Center.X - this.GetBoundingBox().Center.X < 0 && getTileLocationPoint().X < currentLocation.map.Layers[0].TileWidth)
					{
						this.SetMovingRight(true);
					}
				}
				else if (base.Player.GetBoundingBox().Center.Y - this.GetBoundingBox().Center.Y > 0 && getTileLocationPoint().Y > 0)
				{
					this.SetMovingUp(true);
				}
				else if (base.Player.GetBoundingBox().Center.Y - this.GetBoundingBox().Center.Y < 0 && getTileLocationPoint().Y < currentLocation.map.Layers[0].TileHeight)
				{
					this.SetMovingDown(true);
				}
				MovePosition(time, Game1.viewport, currentLocation);
			}
			else
            {
				Halt();
            }
		}

		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			if (this.isMoving())
			{
				if (base.FacingDirection == 0)
				{
					this.Sprite.AnimateUp(time, 0, "");
				}
				else if (base.FacingDirection == 3)
				{
					this.Sprite.AnimateLeft(time, 0, "");
				}
				else if (base.FacingDirection == 1)
				{
					this.Sprite.AnimateRight(time, 0, "");
				}
				else if (base.FacingDirection == 2)
				{
					this.Sprite.AnimateDown(time, 0, "");
				}
			}
			else
			{
				this.Sprite.StopAnimation();
			}
			if (this.isMoving() && this.Sprite.currentFrame % 4 == 0)
			{
				this.Sprite.currentFrame++;
				this.Sprite.UpdateSourceRect();
			}
			if (this.shellGone)
			{
				base.updateGlow();
				if (this.invincibleCountdown > 0)
				{
					this.glowingColor = Color.Cyan;
					this.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
					if (this.invincibleCountdown <= 0)
					{
						base.stopGlowing();
					}
				}
				this.Sprite.currentFrame = 16 + this.Sprite.currentFrame % 4;
			}
		}
	}
}
