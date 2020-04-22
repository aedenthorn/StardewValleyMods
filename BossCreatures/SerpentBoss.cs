using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;

namespace BossCreatures
{
    internal class SerpentBoss : Serpent
    {
		public int timeUntilNextAttack;
		public readonly NetBool firing = new NetBool(false);
		public NetInt attackState = new NetInt();
		public int nextFireTime;
		public int totalFireTime;
		private float difficulty;

		public SerpentBoss(Vector2 position, float difficulty) : base(position)
        {
			this.difficulty = difficulty;
			Health = (int)Math.Round(base.Health * 10 * difficulty);
			MaxHealth = Health;
            Scale = 2f;
            DamageToFarmer = (int)Math.Round(base.damageToFarmer * difficulty);
			timeUntilNextAttack = 100;
			this.moveTowardPlayerThreshold.Value = 20;
		}
		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			base.MovePosition(time, viewport, currentLocation);
			if (Health < MaxHealth / 2)
			{
				base.MovePosition(time, viewport, currentLocation);
			}

		}

		protected override void updateAnimation(GameTime time)
        {
            base.updateAnimation(time);
        }

		protected override void initNetFields()
		{
			base.NetFields.AddFields(new INetSerializable[]
			{
				this.attackState,
				this.firing
			});
			base.initNetFields();
		}
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);

			if (Health <= 0)
			{
				return;
			}

			// fire!

			this.timeUntilNextAttack -= time.ElapsedGameTime.Milliseconds;
			if (this.attackState.Value == 0 && this.withinPlayerThreshold(5))
			{
				this.firing.Set(false);
				if (this.timeUntilNextAttack < 0)
				{
					this.timeUntilNextAttack = 0;
					this.attackState.Set(1);
					this.nextFireTime = 50;
					this.totalFireTime = 3000;
					return;
				}
			}
			else if (this.totalFireTime > 0)
			{
				Farmer player = base.Player;
				if (!this.firing)
				{
					if (player != null)
					{
						base.faceGeneralDirection(player.Position, 0, false);
					}
				}
				this.totalFireTime -= time.ElapsedGameTime.Milliseconds;
				if (this.nextFireTime > 0)
				{
					this.nextFireTime -= time.ElapsedGameTime.Milliseconds;
					if (this.nextFireTime <= 0)
					{
						if (!this.firing.Value)
						{
							this.firing.Set(true);
							base.currentLocation.playSound("furnace", NetAudio.SoundContext.Default);
						}
						float fire_angle = 0f;
						Vector2 shot_origin = new Vector2((float)this.GetBoundingBox().Center.X - 32f, (float)this.GetBoundingBox().Center.Y - 32f);
						base.faceGeneralDirection(player.Position, 0, false);
						switch (this.facingDirection.Value)
						{
							case 0:
								this.yVelocity = -1f;
								shot_origin.Y -= 64f;
								fire_angle = 90f;
								break;
							case 1:
								this.xVelocity = -1f;
								shot_origin.X += 64f;
								fire_angle = 0f;
								break;
							case 2:
								this.yVelocity = 1f;
								fire_angle = 270f;
								break;
							case 3:
								this.xVelocity = 1f;
								shot_origin.X -= 64f;
								fire_angle = 180f;
								break;
						}
						fire_angle += (float)Math.Sin((double)((float)this.totalFireTime / 1000f * 180f) * 3.1415926535897931 / 180.0) * 25f;
						Vector2 shot_velocity = new Vector2((float)Math.Cos((double)fire_angle * 3.1415926535897931 / 180.0), -(float)Math.Sin((double)fire_angle * 3.1415926535897931 / 180.0));
						shot_velocity *= 10f;
						BasicProjectile projectile = new BasicProjectile((int)Math.Round(20 * difficulty), 10, 0, 1, 0.196349546f, shot_velocity.X, shot_velocity.Y, shot_origin, "", "", false, false, base.currentLocation, this, false, null);
						projectile.ignoreTravelGracePeriod.Value = true;
						projectile.maxTravelDistance.Value = 512;
						base.currentLocation.projectiles.Add(projectile);
						this.nextFireTime = 50;
					}
				}
				if (this.totalFireTime <= 0)
				{
					this.totalFireTime = 0;
					this.nextFireTime = 0;
					this.attackState.Set(0);
					if (Health < MaxHealth / 2)
					{
						this.timeUntilNextAttack = Game1.random.Next(800, 1500);
					}
					else
					{
						this.timeUntilNextAttack = Game1.random.Next(1500, 3000);
					}
				}
			}
		}
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int mHealth = Health;
			int result = base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
			if (mHealth - result <= 0)
			{
				ModEntry.PHelper.Events.Display.RenderedHud -= ModEntry.OnRenderedHud;

				ModEntry.SpawnBossLoot(currentLocation, position.X, position.Y, difficulty);

				Game1.playSound("Cowboy_Secret");
				ModEntry.RevertMusic();
			}
			return result;
		}
	}
}