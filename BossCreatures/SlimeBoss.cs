using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Projectiles;

namespace BossCreatures
{
	public class SlimeBoss : BigSlime
	{
		private readonly float difficulty;
		private readonly int width;
		private readonly int height;
		private readonly float unhitableHeight;
		public int timeUntilNextAttack;
		public readonly NetBool firing = new(false);
		public NetInt attackState = new();
		public int nextFireTime;
		public int totalFireTime;

		public SlimeBoss()
		{
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
			unhitableHeight = Scale * height * 1 / 3;
			this.difficulty = difficulty;
			Health = (int)Math.Round(Health * 10 * difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(damageToFarmer.Value * 2 * difficulty);
			farmerPassesThrough = true;
			timeUntilNextAttack = 100;
			moveTowardPlayerThreshold.Value = 20;
		}

		public override void reloadSprite(bool onlyAppearance = false)
		{
			base.reloadSprite(onlyAppearance);
			Sprite.SpriteWidth = width;
			Sprite.SpriteHeight = height;
			Sprite.interval = 300f;
			Sprite.ignoreStopAnimation = true;
			ignoreMovementAnimations = true;
			HideShadow = true;
			Sprite.framesPerAnimation = 8;
		}

		protected override void updateAnimation(GameTime time)
		{
			base.updateAnimation(time);
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			NetFields.AddField(attackState).AddField(firing);
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (Health <= 0)
			{
				return;
			}
			timeUntilNextAttack -= time.ElapsedGameTime.Milliseconds;

			FarmerCollection.Enumerator enumerator = currentLocation.farmers.GetEnumerator();

			while (enumerator.MoveNext())
			{
				if (enumerator.Current.currentLocation == currentLocation && enumerator.Current.GetBoundingBox().Intersects(GetBoundingBox()))
				{
					enumerator.Current.takeDamage((int)Math.Round(20 * difficulty), true, null);
					totalFireTime = 0;
					nextFireTime = 10;
					attackState.Set(0);
					timeUntilNextAttack = Game1.random.Next(1000, 2000);
				}
			}

			if (attackState.Value == 0 && withinPlayerThreshold(20))
			{
				firing.Set(false);
				if (timeUntilNextAttack < 0)
				{
					timeUntilNextAttack = 0;
					attackState.Set(1);
					nextFireTime = 50;
					totalFireTime = 3000;
					return;
				}
			}
			else if (totalFireTime > 0)
			{
				Farmer player = Player;

				if (!firing.Value)
				{
					if (player != null)
					{
						faceGeneralDirection(player.Position, 0, false);
					}
				}
				totalFireTime -= time.ElapsedGameTime.Milliseconds;
				if (nextFireTime > 0)
				{
					nextFireTime -= time.ElapsedGameTime.Milliseconds;
					if (nextFireTime <= 0)
					{
						float projectileOffsetX = Scale * width * 1 / 4;
						float projectileOffsetY = 0f;

						if (!firing.Value)
						{
							firing.Set(true);
						}

						float fire_angle = 0f;

						faceGeneralDirection(player.Position, 0, false);
						switch (facingDirection.Value)
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
						fire_angle += (float)Math.Sin((double)(totalFireTime / 1000f * 180f) * 3.1415926535897931 / 180.0) * 25f;

						Vector2 shot_velocity = new((float)Math.Cos((double)fire_angle * 3.1415926535897931 / 180.0), -(float)Math.Sin((double)fire_angle * 3.1415926535897931 / 180.0));

						shot_velocity *= 5f;
						for (int i = 0; i < 8; i++)
						{
							bool one = i < 4;
							bool two = i % 4 < 2;
							bool three = i % 2 == 0;
							Vector2 v = new((three ? shot_velocity.X : shot_velocity.Y) * (one ? -1 : 1), (three ? shot_velocity.Y : shot_velocity.X) * (two ? -1 : 1));
							BasicProjectile projectile = new BossProjectile((int)(5 * difficulty), 766, 0, 1, 0.196349546f, v.X, v.Y, new Vector2(Position.X + projectileOffsetX, Position.Y + projectileOffsetY), "", "", "", false, false, currentLocation, this, null, "766", "13", true)
							{
								IgnoreLocationCollision = true
							};

							projectile.ignoreTravelGracePeriod.Value = true;
							projectile.maxTravelDistance.Value = 512;
							currentLocation.projectiles.Add(projectile);
							if (!ModEntry.IsLessThanHalfHealth(this))
							{
								i++;
							}

						}
						nextFireTime = 20;
					}
				}
				if (totalFireTime <= 0)
				{
					totalFireTime = 0;
					nextFireTime = 20;
					attackState.Set(0);
					timeUntilNextAttack = 0;
				}
			}
		}

		public override Rectangle GetBoundingBox()
		{
			const float xOffset = 10f;
			const float yOffset = 6.5f;
			const float widthOffset = 1f;
			const float heightOffset = -8.5f;
			float localUnhitableHeight = IsCalledFromProjectile() ? 0 : unhitableHeight;
			float localHitableHeight = Scale * height - localUnhitableHeight;

			static bool IsCalledFromProjectile()
			{
				IEnumerable<Type> callingMethods = new System.Diagnostics.StackTrace().GetFrames()
				.Select(frame => frame.GetMethod())
				.Where(method => method != null)
				.Select(method => method.DeclaringType);

				return callingMethods.Any(type => type == typeof(Projectile));
			}

			return new((int)(Position.X - Scale * (width + widthOffset - xOffset) / 2 * 4f), (int)(Position.Y - (Scale * (height + heightOffset - yOffset) / 2 - localUnhitableHeight) * 4f), (int)(Scale * (width + widthOffset) * 4f), (int)((Scale * heightOffset + localHitableHeight) * 4f));
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
				if (Game1.random.NextDouble() < 0.5f)
				{
					currentLocation.characters.Add(new GreenSlime(Position, (int)(120 * difficulty)));
				}
				currentLocation.characters[^1].setTrajectory(xTrajectory / 8 + Game1.random.Next(-20, 20), yTrajectory / 8 + Game1.random.Next(-20, 20));
				currentLocation.characters[^1].willDestroyObjectsUnderfoot = false;
				currentLocation.characters[^1].moveTowardPlayer(20);
			}
			ModEntry.MakeBossHealthBar(Health, MaxHealth);
			return result;
		}
	}
}
