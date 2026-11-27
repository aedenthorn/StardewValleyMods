using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;

namespace BossCreatures
{
	public class SlimeBoss : BigSlime
	{
		private readonly NetInt width = new();
		private readonly NetInt height = new();
		private readonly NetFloat unhitableHeight = new();
		private readonly NetFloat hitableHeight = new();
		private readonly NetFloat difficulty = new();
		private readonly NetInt attackState = new();
		private readonly NetBool firing = new(false);
		private const int maxTentacles = 8;
		public int timeUntilNextAttack;
		public int nextFireTime;
		public int totalFireTime;

		public int Width
		{
			get => width.Value;
			set => width.Value = value;
		}

		public int Height
		{
			get => height.Value;
			set => height.Value = value;
		}

		public float UnhitableHeight
		{
			get => unhitableHeight.Value;
			set => unhitableHeight.Value = value;
		}

		public float HitableHeight
		{
			get => hitableHeight.Value;
			set => hitableHeight.Value = value;
		}

		public float Difficulty
		{
			get => difficulty.Value;
			set => difficulty.Value = value;
		}

		public int AttackState
		{
			get => attackState.Value;
			set => attackState.Value = value;
		}

		public bool Firing
		{
			get => firing.Value;
			set => firing.Value = value;
		}

		public SlimeBoss()
		{
		}

		public SlimeBoss(Vector2 position, float difficulty) : base(position, 121)
		{
			Width = ModEntry.Config.SlimeBossWidth;
			Height = ModEntry.Config.SlimeBossHeight;
			Sprite.SpriteWidth = Width;
			Sprite.SpriteHeight = Height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Sprite.UpdateSourceRect();
			Scale = ModEntry.Config.SlimeBossScale;
			UnhitableHeight = Scale * Height * 1 / 3;
			HitableHeight = Scale * Height - UnhitableHeight;
			Difficulty = difficulty;
			Health = (int)Math.Round(Health * 10 * Difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(damageToFarmer.Value * 2 * Difficulty);
			farmerPassesThrough = true;
			timeUntilNextAttack = 100;
			moveTowardPlayerThreshold.Value = 20;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			NetFields.AddField(width).AddField(height).AddField(unhitableHeight).AddField(hitableHeight).AddField(difficulty).AddField(attackState).AddField(firing);
		}

		public override void reloadSprite(bool onlyAppearance = false)
		{
			base.reloadSprite(onlyAppearance);
			Sprite = new AnimatedSprite("Characters\\Monsters\\Big Slime")
			{
				SpriteWidth = Width,
				SpriteHeight = Height,
				framesPerAnimation = 8,
				interval = 300f,
				ignoreStopAnimation = true
			};
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (Health > 0)
			{
				timeUntilNextAttack -= time.ElapsedGameTime.Milliseconds;
				foreach (Farmer farmer in currentLocation.farmers)
				{
					if (farmer.currentLocation == currentLocation && farmer.GetBoundingBox().Intersects(GetBoundingBox()))
					{
						farmer.takeDamage((int)Math.Round(20 * Difficulty), true, null);
						totalFireTime = 0;
						nextFireTime = 10;
						AttackState = 0;
						timeUntilNextAttack = Game1.random.Next(1000, 2000);
					}
				}
				if (AttackState == 0 && ModEntry.WithinAnyPlayerThreshold(this, 20))
				{
					Firing = false;
					if (timeUntilNextAttack < 0)
					{
						timeUntilNextAttack = 0;
						AttackState = 1;
						nextFireTime = 50;
						totalFireTime = 3000;
					}
				}
				else if (totalFireTime > 0)
				{
					if (!Firing)
					{
						if (Player is not null)
						{
							faceGeneralDirection(Player.Position, 0, false);
						}
					}
					totalFireTime -= time.ElapsedGameTime.Milliseconds;
					if (nextFireTime > 0)
					{
						nextFireTime -= time.ElapsedGameTime.Milliseconds;
						if (nextFireTime <= 0)
						{
							if (!Firing)
							{
								Firing = true;
							}

							float fire_angle = 0f;

							faceGeneralDirection(Player.Position, 0, false);
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
							fire_angle += (float)Math.Sin(MathHelper.ToRadians(totalFireTime / 1000f * 180f)) * 25f;

							Vector2 shot_velocity = new Vector2((float)Math.Cos(MathHelper.ToRadians(fire_angle)), -(float)Math.Sin(MathHelper.ToRadians(fire_angle))) * 5f;

							for (int i = 0; i < maxTentacles; i += ModEntry.IsLessThanHalfHealth(this) ? 1 : 2)
							{
								float projectileOffsetX = Scale * Width * 1 / 4;
								float projectileOffsetY = 0f;
								bool one = i < 4;
								bool two = i % 4 < 2;
								bool three = i % 2 == 0;
								Vector2 v = new((three ? shot_velocity.X : shot_velocity.Y) * (one ? -1 : 1), (three ? shot_velocity.Y : shot_velocity.X) * (two ? -1 : 1));
								BasicProjectile projectile = new BossProjectile((int)(5 * Difficulty), 766, 0, 1, 0.196349546f, v.X, v.Y, new Vector2(Position.X + projectileOffsetX, Position.Y + projectileOffsetY), "", "", "", false, false, currentLocation, this, null, "766", "13", true)
								{
									IgnoreLocationCollision = true
								};

								projectile.ignoreTravelGracePeriod.Value = true;
								projectile.maxTravelDistance.Value = 512;
								currentLocation.projectiles.Add(projectile);
							}
							nextFireTime = 20;
						}
					}
					if (totalFireTime <= 0)
					{
						totalFireTime = 0;
						nextFireTime = 20;
						AttackState = 0;
						timeUntilNextAttack = 0;
					}
				}
			}
		}

		public override Rectangle GetBoundingBox()
		{
			const float xOffset = 10f;
			const float yOffset = 6.5f;
			const float widthOffset = 1f;
			const float heightOffset = -8.5f;
			float localUnhitableHeight = IsCalledFromProjectile() ? 0 : UnhitableHeight;
			float localHitableHeight = Scale * Height - localUnhitableHeight;

			static bool IsCalledFromProjectile()
			{
				IEnumerable<Type> callingMethods = new System.Diagnostics.StackTrace().GetFrames()
					.Select(frame => frame.GetMethod())
					.Where(method => method is not null)
					.Select(method => method.DeclaringType);

				return callingMethods.Any(type => type == typeof(Projectile));
			}

			return new((int)(Position.X - Scale * (Width + widthOffset - xOffset) / 2 * 4f), (int)(Position.Y - (Scale * (Height + heightOffset - yOffset) / 2 - localUnhitableHeight) * 4f), (int)(Scale * (Width + widthOffset) * 4f), (int)((Scale * heightOffset + localHitableHeight) * 4f));
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int result = base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);

			if (Health <= 0)
			{
				ModEntry.HandleBossDefeat(currentLocation, this, Difficulty);
			}
			else
			{
				if (Game1.random.NextDouble() < 0.5f)
				{
					currentLocation.characters.Add(new GreenSlime(Position, (int)(120 * Difficulty)));
				}
				currentLocation.characters[^1].setTrajectory(xTrajectory / 8 + Game1.random.Next(-20, 20), yTrajectory / 8 + Game1.random.Next(-20, 20));
				currentLocation.characters[^1].willDestroyObjectsUnderfoot = false;
				currentLocation.characters[^1].moveTowardPlayer(20);
			}
			ModEntry.GenerateBossHealthBarTexture(Health, MaxHealth);
			return result;
		}
	}
}
