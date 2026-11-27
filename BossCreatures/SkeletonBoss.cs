using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Pathfinding;
using StardewValley.Projectiles;

namespace BossCreatures
{
	public class SkeletonBoss : Skeleton
	{
		private readonly NetInt width = new();
		private readonly NetInt height = new();
		private readonly NetFloat unhitableHeight = new();
		private readonly NetFloat hitableHeight = new();
		private readonly NetFloat difficulty = new();
		private const int throwBurst = 10;
		private int controllerAttemptTimer;
		private int throwTimer = 0;
		private int throws = 0;

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

		public SkeletonBoss()
		{
		}

		public SkeletonBoss(Vector2 spawnPos, float difficulty) : base(spawnPos)
		{
			Width = ModEntry.Config.SkeletonBossWidth;
			Height = ModEntry.Config.SkeletonBossHeight;
			Sprite.SpriteWidth = Width;
			Sprite.SpriteHeight = Height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Scale = ModEntry.Config.SkeletonBossScale;
			UnhitableHeight = Scale * Height * 2 / 3;
			HitableHeight = Scale * Height - UnhitableHeight;
			Difficulty = difficulty;
			Health = (int)Math.Round(Health * 20 * Difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(damageToFarmer.Value * 2 * Difficulty);
			farmerPassesThrough = true;
			moveTowardPlayerThreshold.Value = 20;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			NetFields.AddField(width).AddField(height).AddField(unhitableHeight).AddField(hitableHeight).AddField(difficulty);
		}

		public override void reloadSprite(bool onlyAppearance = false)
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\Skeleton")
			{
				SpriteWidth = Width,
				SpriteHeight = Height
			};
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
		}

		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			base.MovePosition(time, viewport, currentLocation);
			base.MovePosition(time, viewport, currentLocation);
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			if (!throwing.Value)
			{
				throwTimer -= time.ElapsedGameTime.Milliseconds;
				base.behaviorAtGameTick(time);
			}
			if (Health > 0)
			{
				if (!spottedPlayer && !wildernessFarmMonster && Utility.doesPointHaveLineOfSightInMine(currentLocation, Tile, Player.Tile, 8))
				{
					controller = new PathFindController(this, currentLocation, new Point(Player.StandingPixel.X / 64, Player.StandingPixel.Y / 64), Game1.random.Next(4), null, 200);
					spottedPlayer = true;
					facePlayer(Player);
					IsWalkingTowardPlayer = true;
				}
				else if (throwing.Value)
				{
					if (invincibleCountdown > 0)
					{
						invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
						if (invincibleCountdown <= 0)
						{
							stopGlowing();
						}
					}
					Sprite.Animate(time, 20, 4, 150f);
					if (Sprite.currentFrame == 23)
					{
						float projectileOffsetX = 0f;
						float projectileOffsetY = -(Scale * Height);

						throwing.Value = false;
						Sprite.currentFrame = 0;
						faceDirection(2);

						Vector2 v = Utility.getVelocityTowardPlayer(new Point((int)Position.X + (int)projectileOffsetX, (int)Position.Y + (int)projectileOffsetY), 8f, Player);

						if (ModEntry.IsLessThanHalfHealth(this))
						{
							currentLocation.projectiles.Add(new BasicProjectile(DamageToFarmer, 4, 0, 0, 0.196349546f, v.X, v.Y, new Vector2(Position.X + projectileOffsetX, Position.Y + projectileOffsetY), "", "", "", false, false, currentLocation, this));
							currentLocation.projectiles.Add(new BasicProjectile(DamageToFarmer, 10, 0, 4, 0.196349546f, v.X, v.Y, new Vector2(Position.X + projectileOffsetX, Position.Y + projectileOffsetY), "", "", "fireball", true, false, currentLocation, this));
							if (++throws > throwBurst * 2)
							{
								throwTimer = 1000;
								throws = 0;
							}
							else
							{
								throwTimer = 100;
							}
						}
						else
						{
							BasicProjectile projectile = new(DamageToFarmer, 4, 0, 0, 0.196349546f, v.X, v.Y, new Vector2(Position.X + projectileOffsetX, Position.Y + projectileOffsetY), "skeletonHit", "", "skeletonStep", false, false, currentLocation, this);

							projectile.collisionBehavior = (location, xPosition, yPosition, who) =>
							{
								projectile.piercesLeft.Value = 0;
							};
							currentLocation.projectiles.Add(projectile);
							if (++throws > throwBurst)
							{
								throwTimer = 1000;
								throws = 0;
							}
							else
							{
								throwTimer = 10;
							}
						}
					}
				}
				else if (spottedPlayer && controller is null && Game1.random.NextDouble() < 0.5 && !wildernessFarmMonster && Utility.doesPointHaveLineOfSightInMine(currentLocation, Tile, Player.Tile, 8) && throwTimer <= 0)
				{
					throwing.Value = true;
					Sprite.currentFrame = 20;
				}
				else if (ModEntry.WithinAnyPlayerThreshold(this, 20))
				{
					controller = null;
				}
				else if (spottedPlayer && controller is null && controllerAttemptTimer <= 0)
				{
					controller = new PathFindController(this, currentLocation, new Point(Player.StandingPixel.X / 64, Player.StandingPixel.Y / 64), Game1.random.Next(4), null, 200);
					facePlayer(Player);
					controllerAttemptTimer = 2000;
				}
				else if (wildernessFarmMonster)
				{
					spottedPlayer = true;
					IsWalkingTowardPlayer = true;
				}
				controllerAttemptTimer -= time.ElapsedGameTime.Milliseconds;
			}
		}

		public override Rectangle GetBoundingBox()
		{
			const float xOffset = 5.5f;
			const float yOffset = -8f;
			const float widthOffset = 0.5f;
			const float heightOffset = -5.5f;
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

		public override Vector2 GetShadowOffset()
		{
			return base.GetShadowOffset() + new Vector2(0, HitableHeight);
		}

		public override void Halt()
		{
		}

		public override void shedChunks(int number)
		{
			Game1.createRadialDebris(currentLocation, Sprite.textureName.Value, new Rectangle(0, Height * 4, Width, Width), 8, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)Tile.Y, Color.White, 4f);
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
