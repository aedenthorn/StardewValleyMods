using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;

namespace BossCreatures
{
	public class SerpentBoss : Serpent
	{
		private readonly NetInt width = new();
		private readonly NetInt height = new();
		private readonly NetFloat unhitableHeight = new();
		private readonly NetFloat hitableHeight = new();
		private readonly NetFloat difficulty = new();
		private readonly NetInt attackState = new();
		private readonly NetBool firing = new(false);
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

		public SerpentBoss()
		{
		}

		public SerpentBoss(Vector2 position, float difficulty) : base(position)
		{
			Width = ModEntry.Config.SerpentBossWidth;
			Height = ModEntry.Config.SerpentBossHeight;
			Sprite.SpriteWidth = Width;
			Sprite.SpriteHeight = Height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Scale = ModEntry.Config.SerpentBossScale;
			UnhitableHeight = 0;
			HitableHeight = Scale * Height - UnhitableHeight;
			Difficulty = difficulty;
			Health = (int)Math.Round(Health * 15 * Difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(damageToFarmer.Value * Difficulty);
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
			Sprite = new AnimatedSprite("Characters\\Monsters\\Serpent")
			{
				SpriteWidth = Width,
				SpriteHeight = Height
			};
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			HideShadow = true;
		}

		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			base.MovePosition(time, viewport, currentLocation);
			if (ModEntry.IsLessThanHalfHealth(this))
			{
				base.MovePosition(time, viewport, currentLocation);
			}
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (Health > 0)
			{
				timeUntilNextAttack -= time.ElapsedGameTime.Milliseconds;
				if (AttackState == 0 && ModEntry.WithinAnyPlayerThreshold(this, 5))
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
								currentLocation.playSound("furnace");
							}

							float fire_angle = 0f;
							Vector2 shot_origin = new(GetBoundingBox().Center.X, GetBoundingBox().Center.Y);

							faceGeneralDirection(Player.Position, 0, false);
							switch (facingDirection.Value)
							{
								case 0:
									yVelocity = -1f;
									shot_origin.Y -= 64f;
									fire_angle = 90f;
									break;
								case 1:
									xVelocity = -1f;
									shot_origin.X += 64f;
									fire_angle = 0f;
									break;
								case 2:
									yVelocity = 1f;
									fire_angle = 270f;
									break;
								case 3:
									xVelocity = 1f;
									shot_origin.X -= 64f;
									fire_angle = 180f;
									break;
							}
							fire_angle += (float)Math.Sin(MathHelper.ToRadians(totalFireTime / 1000f * 180f)) * 25f;

							Vector2 shot_velocity = new Vector2((float)Math.Cos(MathHelper.ToRadians(fire_angle)), -(float)Math.Sin(MathHelper.ToRadians(fire_angle))) * 10f;
							BasicProjectile projectile = new((int)Math.Round(10 * Difficulty), 10, 0, 1, 0.196349546f, shot_velocity.X, shot_velocity.Y, shot_origin, "", "", "", false, false, currentLocation, this);

							projectile.ignoreTravelGracePeriod.Value = true;
							projectile.maxTravelDistance.Value = 512;
							currentLocation.projectiles.Add(projectile);
							if (ModEntry.IsLessThanHalfHealth(this))
							{
								currentLocation.projectiles.Add(new BasicProjectile((int)Math.Round(10 * Difficulty), 10, 3, 4, 0f, shot_velocity.X, shot_velocity.Y, shot_origin, "", "", "", true, false, currentLocation, this));
							}
							nextFireTime = 50;
						}
					}
					if (totalFireTime <= 0)
					{
						totalFireTime = 0;
						nextFireTime = 0;
						AttackState = 0;
						timeUntilNextAttack = Game1.random.Next(2000, 4000);
					}
				}
			}
		}

		public override Rectangle GetBoundingBox()
		{
			const float xOffset = 16f;
			const float yOffset = 30f;
			const float widthOffset = -2f;
			const float heightOffset = -5f;

			return new((int)(Position.X - Scale * (Width + widthOffset - xOffset) / 2 * 4f), (int)(Position.Y - (Scale * (Height + heightOffset - yOffset) / 2 - UnhitableHeight) * 4f), (int)(Scale * (Width + widthOffset) * 4f), (int)((Scale * heightOffset + HitableHeight) * 4f));
		}

		public override void drawAboveAllLayers(SpriteBatch b)
		{
			if (Utility.isOnScreen(Position, 128))
			{
				b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(64f, GetBoundingBox().Height), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (StandingPixel.Y - 1) / 10000f);
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(Width * 2, GetBoundingBox().Height / 2), new Rectangle?(Sprite.SourceRect), Color.White, rotation, new Vector2(Width / 2, Height / 2), scale.Value * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((StandingPixel.Y + 8) / 10000f)));
				if (isGlowing)
				{
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(Width * 2, GetBoundingBox().Height / 2), new Rectangle?(Sprite.SourceRect), glowingColor * glowingTransparency, rotation, new Vector2(Width / 2, Height / 2), scale.Value * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((StandingPixel.Y + 8) / 10000f + 0.0001f)));
				}
			}
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int result = base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);

			if (Health <= 0)
			{
				ModEntry.HandleBossDefeat(currentLocation, this, Difficulty);
			}
			ModEntry.GenerateBossHealthBarTexture(Health, MaxHealth);
			return result;
		}
	}
}
