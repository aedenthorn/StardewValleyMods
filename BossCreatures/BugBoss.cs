using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;

namespace BossCreatures
{
	public class BugBoss : Bat
	{
		private readonly int width;
		private readonly int height;
		private readonly float unhitableHeight;
		private readonly float hitableHeight;
		private readonly int MaxFlies;
		private float lastFly;
		private float lastDebuff;
		public float difficulty;

		public BugBoss()
		{
		}

		public BugBoss(Vector2 spawnPos, float _difficulty) : base(spawnPos)
		{
			width = ModEntry.Config.BugBossWidth;
			height = ModEntry.Config.BugBossHeight;
			Sprite.SpriteWidth = width;
			Sprite.SpriteHeight = height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Scale = ModEntry.Config.BugBossScale;
			unhitableHeight = 0;
			hitableHeight = Scale * height - unhitableHeight;
			difficulty = _difficulty;
			Health = (int)Math.Round(Health * 100 * difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(DamageToFarmer * difficulty);
			MaxFlies = 4;
			moveTowardPlayerThreshold.Value = 20;
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (Health <= 0)
			{
				return;
			}
			lastFly = Math.Max(0f, lastFly - time.ElapsedGameTime.Milliseconds);
			lastDebuff = Math.Max(0f, lastDebuff - time.ElapsedGameTime.Milliseconds);
			if (withinPlayerThreshold(10))
			{
				if (lastDebuff == 0f)
				{
					Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(GetBoundingBox().Center, 15f, Player);

					currentLocation.projectiles.Add(new DebuffingProjectile("14", 2, 4, 4, 0.196349546f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2(GetBoundingBox().X, GetBoundingBox().Y), currentLocation, this));
					lastDebuff = Game1.random.Next(3000, 6000);
				}
				if (lastFly == 0f)
				{
					int flies = 0;
					using (List<NPC>.Enumerator enumerator = currentLocation.characters.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							NPC j = enumerator.Current;

							if (j is ToughFly)
							{
								flies++;
							}
						}
					}

					if (flies < (Health < MaxHealth / 2 ? MaxFlies * 2 : MaxFlies))
					{
						if (Health < MaxHealth / 2)
						{
							Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(GetBoundingBox().Center, 15f, Player);

							currentLocation.projectiles.Add(new DebuffingProjectile("13", 7, 4, 4, 0.196349546f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2(GetBoundingBox().X, GetBoundingBox().Y), currentLocation, this));
						}
						currentLocation.characters.Add(new ToughFly(Position, difficulty)
						{
							focusedOnFarmers = true
						});
						lastFly = Game1.random.Next(4000, 8000);
					}
				}
			}
		}

		public override Rectangle GetBoundingBox()
		{
			const float xOffset = 4f;
			const float yOffset = 9.5f;
			const float widthOffset = 2f;
			const float heightOffset = -4.5f;

			return new((int)(Position.X - Scale * (width + widthOffset - xOffset) / 2 * 4f), (int)(Position.Y - (Scale * (height + heightOffset - yOffset) / 2 - unhitableHeight) * 4f), (int)(Scale * (width + widthOffset) * 4f), (int)((Scale * heightOffset + hitableHeight) * 4f));
		}

		public override void drawAboveAllLayers(SpriteBatch b)
		{
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(width * 2, height * 2), new Rectangle?(Sprite.SourceRect), (shakeTimer > 0) ? Color.Red : Color.White, 0f, new Vector2(width / 2, height / 2), scale.Value * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.92f);
			b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(width * 2, height * 2), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, wildernessFarmMonster ? 0.0001f : ((StandingPixel.Y - 1) / 10000f));
			if (isGlowing)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(width * 2, height * 2), new Rectangle?(Sprite.SourceRect), glowingColor * glowingTransparency, 0f, new Vector2(width / 2, height / 2), scale.Value * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.99f : (StandingPixel.Y / 10000f + 0.001f)));
			}
		}

		public override void shedChunks(int number, float scale)
		{
			Game1.createRadialDebris(currentLocation, Sprite.textureName.Value, new Rectangle(0, height * 4, width, height), width / 2, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)Tile.Y, Color.White, 4f);
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
