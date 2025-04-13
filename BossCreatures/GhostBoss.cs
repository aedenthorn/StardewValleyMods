using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;

namespace BossCreatures
{
	internal class GhostBoss : Ghost
	{
		private readonly float difficulty;
		private readonly int width;
		private readonly int height;
		private readonly float unhitableHeight;
		private readonly float hitableHeight;
		private readonly int MaxGhosts;
		private float lastGhost;
		private float lastDebuff;

		public GhostBoss()
		{
		}

		public GhostBoss(Vector2 spawnPos, float difficulty) : base(spawnPos)
		{
			width = ModEntry.Config.GhostBossWidth;
			height = ModEntry.Config.GhostBossHeight;
			Sprite.SpriteWidth = width;
			Sprite.SpriteHeight = height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Scale = ModEntry.Config.GhostBossScale;
			unhitableHeight = 0;
			hitableHeight = Scale * height - unhitableHeight;
			this.difficulty = difficulty;
			Health = (int)Math.Round(Health * 20 * difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(DamageToFarmer * difficulty);
			MaxGhosts = 4;
			moveTowardPlayerThreshold.Value = 20;
		}

		public override void reloadSprite(bool onlyAppearance = false)
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\Ghost");
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);

			if (Health <= 0)
			{
				return;
			}
			lastGhost = Math.Max(0f, lastGhost - time.ElapsedGameTime.Milliseconds);
			lastDebuff = Math.Max(0f, lastDebuff - time.ElapsedGameTime.Milliseconds);
			if (withinPlayerThreshold(10))
			{
				if (lastDebuff == 0f)
				{
					Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(GetBoundingBox().Center, 15f, Player);

					if (ModEntry.IsLessThanHalfHealth(this))
					{
						for (int i = 0; i < 12; i++)
						{
							Vector2 trajectory = ModEntry.VectorFromDegree(i * 30) * 10f;
							currentLocation.projectiles.Add(new BossProjectile((int)Math.Round(20 * difficulty), 9, 3, 4, 0f, trajectory.X, trajectory.Y, getStandingPosition(), "", "", "", true, false, currentLocation, this, null, null, "19"));
						}
					}
					else
					{
						currentLocation.projectiles.Add(new BossProjectile((int)Math.Round(20 * difficulty), 9, 3, 4, 0f, velocityTowardPlayer.X, velocityTowardPlayer.Y, getStandingPosition(), "", "", "", true, false, currentLocation, this, null, null, "19"));
					}
					lastDebuff = Game1.random.Next(3000, 6000);
				}
				if (lastGhost == 0f)
				{
					int ghosts = 0;

					using (List<NPC>.Enumerator enumerator = currentLocation.characters.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							NPC j = enumerator.Current;
							if (j is ToughGhost)
							{
								ghosts++;
							}
						}
					}
					if (ghosts < (Health < MaxHealth / 2 ? MaxGhosts * 2 : MaxGhosts))
					{
						currentLocation.characters.Add(new ToughGhost(Position, difficulty)
						{
							focusedOnFarmers = true
						});
						lastGhost = (float)Game1.random.Next(3000, 80000);
					}
				}
			}
		}

		public override Rectangle GetBoundingBox()
		{
			const float xOffset = 4f;
			const float yOffset = -4f;
			const float widthOffset = 0f;
			const float heightOffset = -4f;

			return new((int)(Position.X - Scale * (width + widthOffset - xOffset) / 2 * 4f), (int)(Position.Y - (Scale * (height + heightOffset - yOffset) / 2 - unhitableHeight) * 4f), (int)(Scale * (width + widthOffset) * 4f), (int)((Scale * heightOffset + hitableHeight) * 4f));
		}

		public override void drawAboveAllLayers(SpriteBatch b)
		{
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(width * 2, 21 + yOffset), new Microsoft.Xna.Framework.Rectangle?(Sprite.SourceRect), Color.White, 0f, new Vector2(width / 2, width), scale.Value * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : (StandingPixel.Y / 10000f)));
			b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(width * 2, width * 4), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + yOffset / 20f * width / 16, SpriteEffects.None, (StandingPixel.Y - 1) / 10000f);
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
