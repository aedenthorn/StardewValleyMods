using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Network;

namespace BossCreatures
{
	public class SquidKidBoss : SquidKid
	{
		private readonly float difficulty;
		private readonly int width;
		private readonly int height;
		private readonly float unhitableHeight;
		private readonly float hitableHeight;
		private float lastIceBall;
		private float lastLightning = 2000f;
		private bool startedLightning = false;
		private Vector2 playerPosition;

		public SquidKidBoss()
		{
		}

		public SquidKidBoss(Vector2 spawnPos, float _difficulty) : base(spawnPos)
		{
			width = ModEntry.Config.SquidKidBossWidth;
			height = ModEntry.Config.SquidKidBossHeight;
			Sprite.SpriteWidth = width;
			Sprite.SpriteHeight = height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Scale = ModEntry.Config.SquidKidBossScale;
			unhitableHeight = 0;
			hitableHeight = Scale * height - unhitableHeight;
			difficulty = _difficulty;
			Health = (int)Math.Round(Health * 1500 * difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(damageToFarmer.Value * 2 * difficulty);
			farmerPassesThrough = true;
			moveTowardPlayerThreshold.Value = 20;
		}

		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			base.MovePosition(time, viewport, currentLocation);
			if (ModEntry.IsLessThanHalfHealth(this))
			{
				base.MovePosition(time, viewport, currentLocation);
			}
		}

		private int projectileCount = 0;

		public override void behaviorAtGameTick(GameTime time)
		{
			lastFireball = 1000f;
			base.behaviorAtGameTick(time);
			if (Health <= 0)
			{
				return;
			}
			if (withinPlayerThreshold(20))
			{
				lastIceBall = Math.Max(0f, lastIceBall - time.ElapsedGameTime.Milliseconds);
				lastLightning = Math.Max(0f, lastLightning - time.ElapsedGameTime.Milliseconds);

				if (!startedLightning && lastLightning < (ModEntry.IsLessThanHalfHealth(this) ? 500f : 1000f))
				{
					startedLightning = true;

					List<Farmer> farmers = new();
					FarmerCollection.Enumerator enumerator = currentLocation.farmers.GetEnumerator();

					while (enumerator.MoveNext())
					{
						farmers.Add(enumerator.Current);
					}
					playerPosition = farmers[Game1.random.Next(0, farmers.Count)].position.Value;

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
					Vector2 trajectory = ModEntry.VectorFromDegree(Game1.random.Next(0,360)) * 10f;

					currentLocation.projectiles.Add(new BossProjectile((int)Math.Round(10 * difficulty), 9, 3, 4, 0f, trajectory.X, trajectory.Y, getStandingPosition(), "", "", "", true, false, currentLocation, this, null, null, "19"));
					projectileCount++;
					if (projectileCount >= (ModEntry.IsLessThanHalfHealth(this) ? 8 : 4))
					{
						projectileCount = 0;
						lastIceBall = Game1.random.Next(1200, 3500);
					}
					else
					{
						lastIceBall = 100;
					}
					if (lastIceBall != 0f && Game1.random.NextDouble() < 0.05)
					{
						Halt();
						setTrajectory((int)Utility.getVelocityTowardPlayer(Utility.Vector2ToPoint(getStandingPosition()), 8f, Player).X, (int)(-(int)Utility.getVelocityTowardPlayer(Utility.Vector2ToPoint(getStandingPosition()), 8f, Player).Y));
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

			FarmerCollection.Enumerator enumerator = currentLocation.farmers.GetEnumerator();

			while (enumerator.MoveNext())
			{
				if (enumerator.Current.currentLocation == currentLocation && enumerator.Current.GetBoundingBox().Intersects(new Rectangle((int)Math.Round(playerLocation.X - 32), (int)Math.Round(playerLocation.Y - 32), 64, 64)))
				{
					enumerator.Current.takeDamage((int)Math.Round(10 * difficulty), true, null);
				}
			}
		}

		public override Rectangle GetBoundingBox()
		{
			const float xOffset = 3f;
			const float yOffset = -11f;
			const float widthOffset = 0f;
			const float heightOffset = -3f;

			return new((int)(Position.X - Scale * (width + widthOffset - xOffset) / 2 * 4f), (int)(Position.Y - (Scale * (height + heightOffset - yOffset) / 2 - unhitableHeight) * 4f), (int)(Scale * (width + widthOffset) * 4f), (int)((Scale * heightOffset + hitableHeight) * 4f));
		}

		public override void drawAboveAllLayers(SpriteBatch b)
		{
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(width * 2, 21 + yOffset), new Rectangle?(Sprite.SourceRect), Color.White, 0f, new Vector2(width / 2, height), scale.Value * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : (StandingPixel.Y / 10000f)));
			b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(width * 2, height * 4), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + yOffset / 20f, SpriteEffects.None, (StandingPixel.Y - 1) / 10000f);
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
