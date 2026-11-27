using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;

namespace BossCreatures
{
	public class BugBoss : Bat
	{
		private readonly NetInt width = new();
		private readonly NetInt height = new();
		private readonly NetFloat unhitableHeight = new();
		private readonly NetFloat hitableHeight = new();
		private readonly NetFloat difficulty = new();
		private const int MaxFlies = 4;
		private float lastFly;
		private float lastDebuff;

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

		public BugBoss()
		{
		}

		public BugBoss(Vector2 spawnPos, float difficulty) : base(spawnPos)
		{
			Width = ModEntry.Config.BugBossWidth;
			Height = ModEntry.Config.BugBossHeight;
			Sprite.SpriteWidth = Width;
			Sprite.SpriteHeight = Height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Scale = ModEntry.Config.BugBossScale;
			UnhitableHeight = 0;
			HitableHeight = Scale * Height - UnhitableHeight;
			Difficulty = difficulty;
			Health = (int)Math.Round(Health * 100 * Difficulty);
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
			base.reloadSprite(onlyAppearance);
			Sprite = new AnimatedSprite("Characters\\Monsters\\Armored Bug")
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
				lastFly = Math.Max(0f, lastFly - time.ElapsedGameTime.Milliseconds);
				lastDebuff = Math.Max(0f, lastDebuff - time.ElapsedGameTime.Milliseconds);
				if (ModEntry.WithinAnyPlayerThreshold(this, 10))
				{
					if (lastDebuff == 0f)
					{
						Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(GetBoundingBox().Center, 15f, Player);

						currentLocation.projectiles.Add(new DebuffingProjectile("14", 2, 4, 4, 0.196349546f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2(GetBoundingBox().X, GetBoundingBox().Y), currentLocation, this));
						lastDebuff = Game1.random.Next(3000, 6000);
					}
					if (lastFly == 0f)
					{
						int flies = currentLocation.characters.Count(npc => npc is ToughFly);
						int dynamicMaxFlies = ModEntry.IsLessThanHalfHealth(this) ? 2 * MaxFlies: MaxFlies;

						if (flies < dynamicMaxFlies)
						{
							if (ModEntry.IsLessThanHalfHealth(this))
							{
								Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(GetBoundingBox().Center, 15f, Player);

								currentLocation.projectiles.Add(new DebuffingProjectile("13", 7, 4, 4, 0.196349546f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2(GetBoundingBox().X, GetBoundingBox().Y), currentLocation, this));
							}
							currentLocation.characters.Add(new ToughFly(Position, Difficulty)
							{
								focusedOnFarmers = true
							});
							lastFly = Game1.random.Next(4000, 8000);
						}
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

			return new((int)(Position.X - Scale * (Width + widthOffset - xOffset) / 2 * 4f), (int)(Position.Y - (Scale * (Height + heightOffset - yOffset) / 2 - UnhitableHeight) * 4f), (int)(Scale * (Width + widthOffset) * 4f), (int)((Scale * heightOffset + HitableHeight) * 4f));
		}

		public override void drawAboveAllLayers(SpriteBatch b)
		{
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(Width * 2, Height * 2), new Rectangle?(Sprite.SourceRect), (shakeTimer > 0) ? Color.Red : Color.White, 0f, new Vector2(Width / 2, Height / 2), scale.Value * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.92f);
			b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(Width * 2, Height * 2), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, wildernessFarmMonster ? 0.0001f : ((StandingPixel.Y - 1) / 10000f));
			if (isGlowing)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(Width * 2, Height * 2), new Rectangle?(Sprite.SourceRect), glowingColor * glowingTransparency, 0f, new Vector2(Width / 2, Height / 2), scale.Value * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.99f : (StandingPixel.Y / 10000f + 0.001f)));
			}
		}

		public override void shedChunks(int number, float scale)
		{
			Game1.createRadialDebris(currentLocation, Sprite.textureName.Value, new Rectangle(0, Height * 4, Width, Height), Width / 2, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)Tile.Y, Color.White, 4f);
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
