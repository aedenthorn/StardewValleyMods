using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;

namespace BossCreatures
{
	public class SkullBoss : Bat
	{
		private readonly NetInt width = new();
		private readonly NetInt height = new();
		private readonly NetFloat unhitableHeight = new();
		private readonly NetFloat hitableHeight = new();
		private readonly NetFloat difficulty = new();
		private const int maxBurst = 4;
		private float lastFireball;
		private int burstNo = 0;
		private List<Vector2> previousPositions = new();

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

		public SkullBoss()
		{
		}

		public SkullBoss(Vector2 position, float difficulty) : base(position, 77377)
		{
			Width = ModEntry.Config.SkullBossWidth;
			Height = ModEntry.Config.SkullBossHeight;
			Sprite.SpriteWidth = Width;
			Sprite.SpriteHeight = Height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Scale = ModEntry.Config.SkullBossScale;
			UnhitableHeight = 0;
			HitableHeight = Scale * Height - UnhitableHeight;
			Difficulty = difficulty;
			Health = (int)Math.Round(Health * 10 * Difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(damageToFarmer.Value * Difficulty);
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
			if (Sprite is null)
			{
				Sprite = new AnimatedSprite("Characters\\Monsters\\Haunted Skull")
				{
					SpriteWidth = Width,
					SpriteHeight = Height
				};
			}
			else
			{
				Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			}
			HideShadow = true;
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (Health > 0)
			{
				faceGeneralDirection(Player.Position, 0, false);
				lastFireball = Math.Max(0f, lastFireball - time.ElapsedGameTime.Milliseconds);
				if (ModEntry.WithinAnyPlayerThreshold(this, 10) && lastFireball == 0f)
				{
					Vector2 trajectory = Utility.getVelocityTowardPlayer(Utility.Vector2ToPoint(getStandingPosition()), 8f, Player);
					int dynamicMaxBurst = ModEntry.IsLessThanHalfHealth(this) ? 2 * maxBurst: maxBurst;

					currentLocation.projectiles.Add(new BasicProjectile((int)Math.Round(20 * Difficulty), 10, 3, 4, 0f, trajectory.X, trajectory.Y, getStandingPosition(), "", "", "", true, false, currentLocation, this));
					if (burstNo < dynamicMaxBurst)
					{
						if (burstNo == 0)
						{
							currentLocation.playSound("fireball");
						}
						burstNo++;
						lastFireball = 100;
					}
					else
					{
						if (!ModEntry.IsLessThanQuarterHealth(this))
						{
							lastFireball = Game1.random.Next(1500, 3000);
						}
						else
						{
							lastFireball = Game1.random.Next(800, 1500);
						}
						burstNo = 0;
					}
				}
			}
		}

		protected override void updateAnimation(GameTime time)
		{
			if (focusedOnFarmers || seenPlayer.Value || ModEntry.WithinAnyPlayerThreshold(this, 20))
			{
				Sprite.Animate(time, 0, 4, 80f);
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
				if (shakeTimer < 0)
				{
					currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 103, Width, Height), position.Value + new Vector2(0f, -32f), false, 0.1f, new Color(255, 50, 255) * 0.8f)
					{
						scale = 4f
					});
					shakeTimer = 50;
				}
				previousPositions.Add(Position);
				if (previousPositions.Count > 8)
				{
					previousPositions.RemoveAt(0);
				}
			}
			resetAnimationSpeed();
		}

		public override Rectangle GetBoundingBox()
		{
			const float xOffset = 3f;
			const float yOffset = -7f;
			const float widthOffset = 0f;
			const float heightOffset = -1f;

			return new((int)(Position.X - Scale * (Width + widthOffset - xOffset) / 2 * 4f), (int)(Position.Y - (Scale * (Height + heightOffset - yOffset) / 2 - UnhitableHeight) * 4f), (int)(Scale * (Width + widthOffset) * 4f), (int)((Scale * heightOffset + HitableHeight) * 4f));
		}

		public override void drawAboveAllLayers(SpriteBatch b)
		{
			if (Utility.isOnScreen(Position, 128))
			{
				previousPositions = (List<Vector2>)GetType().BaseType.GetField("previousPositions", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);

				Vector2 pos_offset = Vector2.Zero;

				if (previousPositions.Count > 2)
				{
					pos_offset = Position - previousPositions[1];
				}

				int direction = (Math.Abs(pos_offset.X) > Math.Abs(pos_offset.Y)) ? ((pos_offset.X > 0f) ? 1 : 3) : ((pos_offset.Y < 0f) ? 0 : 2);
				Vector2 offset = new(0f, Width / 2 * (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 188.49555921538757));

				if (direction == -1)
				{
					direction = 2;
				}
				b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(Width * 2, Height * 4), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f * Scale + offset.Y / 20f, SpriteEffects.None, 0.0001f);
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(Width * 2 + Game1.random.Next(-6, 7), Height * 2 + Game1.random.Next(-6, 7)) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + ((seenPlayer.Value && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), Width, Height)), Color.Red * 0.44f, 0f, new Vector2(Width / 2f, Height), Scale * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f - 1f) / 10000f);
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(Width * 2 + Game1.random.Next(-6, 7), Height * 2 + Game1.random.Next(-6, 7)) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + ((seenPlayer.Value && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), Width, Height)), Color.Yellow * 0.44f, 0f, new Vector2(Width / 2, Height), Scale * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f) / 10000f);
				for (int i = previousPositions.Count - 1; i >= 0; i -= 2)
				{
					b.Draw(Sprite.Texture, new Vector2(previousPositions[i].X - Game1.viewport.X, previousPositions[i].Y - Game1.viewport.Y + yJumpOffset) + drawOffset + new Vector2(Height * 2, Width * 2) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + ((seenPlayer.Value && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), Width, Height)), Color.White * (0f + 0.125f * i), 0f, new Vector2(Width / 2, Height), scale.Value * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f - i) / 10000f);
				}
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(Width * 2, Height * 2) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + ((seenPlayer.Value && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), Width, Height)), Color.White, 0f, new Vector2(Width / 2, Height), scale.Value * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f + 1f) / 10000f);
			}
		}

		public override void shedChunks(int number, float scale)
		{
			Game1.createRadialDebris(currentLocation, Sprite.textureName.Value, new Rectangle(0, Height * 4, Width, Height), Height / 2, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)Tile.Y, Color.White, 4f);
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
