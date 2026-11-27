using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Monsters;

namespace BossCreatures
{
	internal class GhostBoss : Ghost
	{
		private readonly NetInt width = new();
		private readonly NetInt height = new();
		private readonly NetFloat unhitableHeight = new();
		private readonly NetFloat hitableHeight = new();
		private readonly NetFloat difficulty = new();
		private const int maxGhosts = 4;
		private int lastGhost;
		private int lastDebuff;

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

		public GhostBoss()
		{
		}

		public GhostBoss(Vector2 position, float difficulty) : base(position)
		{
			Width = ModEntry.Config.GhostBossWidth;
			Height = ModEntry.Config.GhostBossHeight;
			Sprite.SpriteWidth = Width;
			Sprite.SpriteHeight = Height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Scale = ModEntry.Config.GhostBossScale;
			UnhitableHeight = 0;
			HitableHeight = Scale * Height - UnhitableHeight;
			Difficulty = difficulty;
			Health = (int)Math.Round(Health * 20 * Difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(DamageToFarmer * Difficulty);
			moveTowardPlayerThreshold.Value = 20;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			NetFields.AddField(width).AddField(height).AddField(unhitableHeight).AddField(hitableHeight).AddField(difficulty);
		}

		public override void reloadSprite(bool onlyAppearance = false)
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\Ghost")
			{
				SpriteWidth = Width,
				SpriteHeight = Height
			};
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (Health > 0)
			{
				lastGhost = Math.Max(0, lastGhost - time.ElapsedGameTime.Milliseconds);
				lastDebuff = Math.Max(0, lastDebuff - time.ElapsedGameTime.Milliseconds);
				if (ModEntry.WithinAnyPlayerThreshold(this, 10))
				{
					if (lastDebuff == 0f)
					{
						Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(GetBoundingBox().Center, 15f, Player);

						if (ModEntry.IsLessThanHalfHealth(this))
						{
							for (int i = 0; i < 12; i++)
							{
								Vector2 trajectory = ModEntry.VectorFromDegrees(i * 30) * 10f;

								currentLocation.projectiles.Add(new BossProjectile((int)Math.Round(20 * Difficulty), 9, 3, 4, 0f, trajectory.X, trajectory.Y, getStandingPosition(), "", "", "", true, false, currentLocation, this, null, null, "19"));
							}
						}
						else
						{
							currentLocation.projectiles.Add(new BossProjectile((int)Math.Round(20 * Difficulty), 9, 3, 4, 0f, velocityTowardPlayer.X, velocityTowardPlayer.Y, getStandingPosition(), "", "", "", true, false, currentLocation, this, null, null, "19"));
						}
						lastDebuff = Game1.random.Next(3000, 6000);
					}
					if (lastGhost == 0f)
					{
						int ghosts = currentLocation.characters.Count(npc => npc is ToughGhost);
						int dynamicMaxGhosts = ModEntry.IsLessThanHalfHealth(this) ? 2 * maxGhosts: maxGhosts;

						if (ghosts < dynamicMaxGhosts)
						{
							currentLocation.characters.Add(new ToughGhost(Position, Difficulty)
							{
								focusedOnFarmers = true
							});
							lastGhost = Game1.random.Next(3000, 8000);
						}
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

			return new((int)(Position.X - Scale * (Width + widthOffset - xOffset) / 2 * 4f), (int)(Position.Y - (Scale * (Height + heightOffset - yOffset) / 2 - UnhitableHeight) * 4f), (int)(Scale * (Width + widthOffset) * 4f), (int)((Scale * heightOffset + HitableHeight) * 4f));
		}

		public override void drawAboveAllLayers(SpriteBatch b)
		{
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(Width * 2, 21 + yOffset), new Microsoft.Xna.Framework.Rectangle?(Sprite.SourceRect), Color.White, 0f, new Vector2(Width / 2, Width), scale.Value * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : (StandingPixel.Y / 10000f)));
			b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(Width * 2, Width * 4), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + yOffset / 20f * Width / 16, SpriteEffects.None, (StandingPixel.Y - 1) / 10000f);
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
