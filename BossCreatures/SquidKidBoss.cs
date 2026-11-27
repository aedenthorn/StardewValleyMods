using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Monsters;

namespace BossCreatures
{
	public class SquidKidBoss : SquidKid
	{
		private readonly NetInt width = new();
		private readonly NetInt height = new();
		private readonly NetFloat unhitableHeight = new();
		private readonly NetFloat hitableHeight = new();
		private readonly NetFloat difficulty = new();
		private const int maxIceBalls = 4;
		private bool startedLightning = false;
		private float lastIceBall;
		private float lastLightning = 2000f;
		private int projectileCount = 0;
		private Vector2 playerPosition;

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

		public SquidKidBoss()
		{
		}

		public SquidKidBoss(Vector2 spawnPos, float difficulty) : base(spawnPos)
		{
			Width = ModEntry.Config.SquidKidBossWidth;
			Height = ModEntry.Config.SquidKidBossHeight;
			Sprite.SpriteWidth = Width;
			Sprite.SpriteHeight = Height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Scale = ModEntry.Config.SquidKidBossScale;
			UnhitableHeight = 0;
			HitableHeight = Scale * Height - UnhitableHeight;
			Difficulty = difficulty;
			Health = (int)Math.Round(Health * 1500 * Difficulty);
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
			Sprite = new AnimatedSprite("Characters\\Monsters\\Squid Kid")
			{
				SpriteWidth = Width,
				SpriteHeight = Height
			};
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
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
			lastFireball = 1000f;
			base.behaviorAtGameTick(time);
			if (Health > 0)
			{
				lastIceBall = Math.Max(0f, lastIceBall - time.ElapsedGameTime.Milliseconds);
				lastLightning = Math.Max(0f, lastLightning - time.ElapsedGameTime.Milliseconds);
				if (ModEntry.WithinAnyPlayerThreshold(this, 20))
				{
					if (!startedLightning && lastLightning < (ModEntry.IsLessThanHalfHealth(this) ? 500f : 1000f))
					{
						startedLightning = true;
						playerPosition = currentLocation.farmers.ElementAt(Game1.random.Next(currentLocation.farmers.Count)).position.Value;

						Rectangle lightningSourceRect = new(0, 0, 16, 16);
						float markerScale = 8f;
						Vector2 drawPosition = playerPosition + new Vector2(-16 * markerScale / 2 + 32f, -16 * markerScale / 2 + 32f);

						Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\Projectiles", lightningSourceRect, 9999f, 1, 999, drawPosition, false, Game1.random.NextDouble() < 0.5, (playerPosition.Y + 32f) / 10000f + 0.001f, 0.025f, Color.White, markerScale, 0f, 0f, 0f, false)
						{
							lightId = "SquidKidBoss_Lightning",
							lightRadius = 2f,
							delayBeforeAnimationStart = 200,
							lightcolor = Color.Black
						});
					}
					if (lastLightning == 0f)
					{
						startedLightning = false;
						LightningStrike(playerPosition);
						lastLightning = Game1.random.Next(2000, 4000) * (ModEntry.IsLessThanHalfHealth(this) ? 1 : 2);
					}
					if (lastIceBall == 0f)
					{
						Vector2 trajectory = ModEntry.VectorFromDegrees(Game1.random.Next(0,360)) * 10f;
						int dynamicMaxIceBalls = ModEntry.IsLessThanHalfHealth(this) ? 2 * maxIceBalls: maxIceBalls;

						currentLocation.projectiles.Add(new BossProjectile((int)Math.Round(10 * Difficulty), 9, 3, 4, 0f, trajectory.X, trajectory.Y, getStandingPosition(), "", "", "", true, false, currentLocation, this, null, null, "19"));
						projectileCount++;
						if (projectileCount < dynamicMaxIceBalls)
						{
							lastIceBall = 100;
						}
						else
						{
							projectileCount = 0;
							lastIceBall = Game1.random.Next(1200, 3500);
						}
						if (lastIceBall != 0f && Game1.random.NextDouble() < 0.05)
						{
							Halt();
							setTrajectory((int)Utility.getVelocityTowardPlayer(Utility.Vector2ToPoint(getStandingPosition()), 8f, Player).X, -(int)Utility.getVelocityTowardPlayer(Utility.Vector2ToPoint(getStandingPosition()), 8f, Player).Y);
						}
					}
				}
			}
		}

		private void LightningStrike(Vector2 playerLocation)
		{
			Farm.LightningStrikeEvent lightningEvent = new()
			{
				bigFlash = true,
				createBolt = true,
				boltPosition = playerLocation + new Vector2(32f, 32f)
			};

			Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
			Game1.playSound("thunder");
			Utility.drawLightningBolt(lightningEvent.boltPosition, currentLocation);
			foreach (Farmer farmer in currentLocation.farmers)
			{
				if (farmer.currentLocation == currentLocation && farmer.GetBoundingBox().Intersects(new Rectangle((int)Math.Round(playerLocation.X - 32), (int)Math.Round(playerLocation.Y - 32), 64, 64)))
				{
					farmer.takeDamage((int)Math.Round(10 * Difficulty), true, null);
				}
			}
		}

		public override Rectangle GetBoundingBox()
		{
			const float xOffset = 3f;
			const float yOffset = -11f;
			const float widthOffset = 0f;
			const float heightOffset = -3f;

			return new((int)(Position.X - Scale * (Width + widthOffset - xOffset) / 2 * 4f), (int)(Position.Y - (Scale * (Height + heightOffset - yOffset) / 2 - UnhitableHeight) * 4f), (int)(Scale * (Width + widthOffset) * 4f), (int)((Scale * heightOffset + HitableHeight) * 4f));
		}

		public override void drawAboveAllLayers(SpriteBatch b)
		{
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(Width * 2, 21 + yOffset), new Rectangle?(Sprite.SourceRect), Color.White, 0f, new Vector2(Width / 2, Height), scale.Value * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : (StandingPixel.Y / 10000f)));
			b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(Width * 2, Height * 4), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + yOffset / 20f, SpriteEffects.None, (StandingPixel.Y - 1) / 10000f);
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
