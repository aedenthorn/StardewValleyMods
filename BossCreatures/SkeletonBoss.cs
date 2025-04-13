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
		private readonly float difficulty;
		private readonly int width;
		private readonly int height;
		private readonly float unhitableHeight;
		private readonly float hitableHeight;
		private readonly int throwBurst = 10;
		private int controllerAttemptTimer;
		private int throwTimer = 0;
		private int throws = 0;

		public SkeletonBoss()
		{
		}

		public SkeletonBoss(Vector2 spawnPos, float difficulty) : base(spawnPos)
		{
			width = ModEntry.Config.SkeletonBossWidth;
			height = ModEntry.Config.SkeletonBossHeight;
			Sprite.SpriteWidth = width;
			Sprite.SpriteHeight = height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Scale = ModEntry.Config.SkeletonBossScale;
			unhitableHeight = Scale * height * 2 / 3;
			hitableHeight = Scale * height - unhitableHeight;
			this.difficulty = difficulty;
			Health = (int)Math.Round(Health * 20 * difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(damageToFarmer.Value * 2 * difficulty);
			farmerPassesThrough = true;
			moveTowardPlayerThreshold.Value = 20;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			NetFields.AddField(throwing);
			position.Field.AxisAlignedMovement = true;
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
			if (Health <= 0)
			{
				return;
			}
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
					float projectileOffsetY = -(Scale * height);

					throwing.Value = false;
					Sprite.currentFrame = 0;
					faceDirection(2);

					Vector2 v = Utility.getVelocityTowardPlayer(new Point((int)Position.X + (int)projectileOffsetX, (int)Position.Y + (int)projectileOffsetY), 8f, Player);

					if (Health < MaxHealth / 2)
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
			else if (spottedPlayer && controller == null && Game1.random.NextDouble() < 0.5 && !wildernessFarmMonster && Utility.doesPointHaveLineOfSightInMine(currentLocation, Tile, Player.Tile, 8) && throwTimer <= 0)
			{
				throwing.Value = true;
				Sprite.currentFrame = 20;
			}
			else if (withinPlayerThreshold(20))
			{
				controller = null;
			}
			else if (spottedPlayer && controller == null && controllerAttemptTimer <= 0)
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

		public override Rectangle GetBoundingBox()
		{
			const float xOffset = 5.5f;
			const float yOffset = -8f;
			const float widthOffset = 0.5f;
			const float heightOffset = -5.5f;
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

		public override Vector2 GetShadowOffset()
		{
			return base.GetShadowOffset() + new Vector2(0, hitableHeight);
		}

		public override void Halt()
		{
		}

		public override void shedChunks(int number)
		{
			Game1.createRadialDebris(currentLocation, Sprite.textureName.Value, new Rectangle(0, height * 4, width, width), 8, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)Tile.Y, Color.White, 4f);
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int result = base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);

			if (Health <= 0)
			{
				ModEntry.BossDeath(currentLocation, this, difficulty);
			}
			ModEntry.MakeBossHealthBar(Health, MaxHealth);
			return result;
		}
	}
}
