using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
	public class SlimeBoss : BigSlime
    {
		public int timeUntilNextAttack;
		private int maxTimeUntilNextAttack;
		public readonly NetBool firing = new NetBool(false);
		public NetInt attackState = new NetInt();
		public int nextFireTime;
		public int totalFireTime;
		private float difficulty;
		private int width;
		private int height;
		private int j = 0;

		public SlimeBoss() { 
		}

		public SlimeBoss(Vector2 position, float difficulty) : base(position, 121)
        {
			width = ModEntry.Config.SlimeBossWidth;
			height = ModEntry.Config.SlimeBossHeight;
			Sprite.SpriteWidth = width;
			Sprite.SpriteHeight = height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Sprite.UpdateSourceRect();

			Scale = ModEntry.Config.SlimeBossScale;

			this.difficulty = difficulty;
			Health = (int)Math.Round(base.Health * 10 * difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(base.damageToFarmer * difficulty);
			timeUntilNextAttack = 100;
			this.moveTowardPlayerThreshold.Value = 20;

			this.willDestroyObjectsUnderfoot = true;
		}

		public override void reloadSprite()
		{
			base.reloadSprite();
			this.Sprite.SpriteWidth = width;
			this.Sprite.SpriteHeight = height;
			this.Sprite.interval = 300f;
			this.Sprite.ignoreStopAnimation = true;
			this.ignoreMovementAnimations = true;
			base.HideShadow = true;
			this.Sprite.framesPerAnimation = 8;
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

			List<Farmer> farmers = new List<Farmer>();
			FarmerCollection.Enumerator enumerator = currentLocation.farmers.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.currentLocation == currentLocation && enumerator.Current.GetBoundingBox().Intersects(GetBoundingBox()))
				{
					enumerator.Current.takeDamage((int)Math.Round(20 * difficulty), true, null);
					this.totalFireTime = 0;
					this.nextFireTime = 10;
					this.attackState.Set(0);
					timeUntilNextAttack = Game1.random.Next(1000, 2000);
				}
			}

			if (this.attackState.Value == 0 && this.withinPlayerThreshold(20))
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
						}
						float fire_angle = 0f;
						Vector2 shot_origin = new Vector2((float)this.GetBoundingBox().Center.X, (float)this.GetBoundingBox().Center.Y);
						base.faceGeneralDirection(player.Position, 0, false);
						switch (this.facingDirection.Value)
						{
							case 0:
								fire_angle = 90f;
								break;
							case 1:
								fire_angle = 0f;
								break;
							case 2:
								fire_angle = 270f;
								break;
							case 3:
								fire_angle = 180f;
								break;
						}
						fire_angle += (float)Math.Sin((double)((float)this.totalFireTime / 1000f * 180f) * 3.1415926535897931 / 180.0) * 25f;
						Vector2 shot_velocity = new Vector2((float)Math.Cos((double)fire_angle * 3.1415926535897931 / 180.0), -(float)Math.Sin((double)fire_angle * 3.1415926535897931 / 180.0));
						shot_velocity *= 5f;

						for (int i = 0; i < 8; i++)
						{

							bool one = i < 4;
							bool two = i % 4 < 2;
							bool three = i % 2 == 0;

							Vector2 v = new Vector2((three ? shot_velocity.X : shot_velocity.Y) * (one ? -1 : 1), (three ? shot_velocity.Y : shot_velocity.X) * (two ? -1 : 1));
							//v = ModEntry.RotateVector(v, j);
							BasicProjectile projectile = new BossProjectile((int)(5 * difficulty), 766, 0, 1, 0.196349546f, v.X, v.Y, shot_origin, "", "", false, false, base.currentLocation, this, true, null, 13, true);
							projectile.ignoreLocationCollision.Value = true;
							projectile.ignoreTravelGracePeriod.Value = true;
							projectile.maxTravelDistance.Value = 512;
							base.currentLocation.projectiles.Add(projectile);

							if (!ModEntry.IsLessThanHalfHealth(this))
							{
								i++;
							}

						}
						if (ModEntry.IsLessThanHalfHealth(this))
						{
							j += 1;
						}
						j %= 360;


						this.nextFireTime = 10;
					}
				}
				if (this.totalFireTime <= 0)
				{
					this.totalFireTime = 0;
					this.nextFireTime = 10;
					this.attackState.Set(0);

					timeUntilNextAttack = 0;
				}
			}
		}
		public override Rectangle GetBoundingBox()
		{
			Rectangle r = new Rectangle((int)(Position.X - Scale*width/2), (int)(Position.Y - Scale * height/2), (int)(Scale*width), (int)(Scale*height));
			return r;
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int result = base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
			if (Health <= 0)
			{
				ModEntry.BossDeath(currentLocation, this, difficulty);
			}
			else
			{
				base.currentLocation.characters.Add(new GreenSlime(base.Position, (int)(120*difficulty)));
				base.currentLocation.characters[base.currentLocation.characters.Count - 1].setTrajectory(xTrajectory / 8 + Game1.random.Next(-20, 20), yTrajectory / 8 + Game1.random.Next(-20, 20));
				base.currentLocation.characters[base.currentLocation.characters.Count - 1].willDestroyObjectsUnderfoot = false;
				base.currentLocation.characters[base.currentLocation.characters.Count - 1].moveTowardPlayer(20);
			}
			ModEntry.MakeBossHealthBar(Health, MaxHealth);
			return result;
		}
	}
}