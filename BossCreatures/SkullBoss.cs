using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;

namespace BossCreatures
{
	public class SkullBoss : Bat
	{
		private readonly float difficulty;
		private readonly int width;
		private readonly int height;
		private readonly float unhitableHeight;
		private readonly float hitableHeight;
		private float lastFireball;
		private int burstNo = 0;
		private List<Vector2> previousPositions = new();

		public SkullBoss()
		{
		}

		public SkullBoss(Vector2 position, float difficulty) : base(position, 77377)
		{
			width = ModEntry.Config.SkullBossWidth;
			height = ModEntry.Config.SkullBossHeight;
			Sprite.SpriteWidth = width;
			Sprite.SpriteHeight = height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Scale = ModEntry.Config.SkullBossScale;
			unhitableHeight = 0;
			hitableHeight = Scale * height - unhitableHeight;
			this.difficulty = difficulty;
			Health = (int)Math.Round(Health * 10 * difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(damageToFarmer.Value * difficulty);
			farmerPassesThrough = true;
			moveTowardPlayerThreshold.Value = 20;
		}

		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			base.MovePosition(time, viewport, currentLocation);
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (Health <= 0)
			{
				return;
			}
			faceGeneralDirection(Player.Position, 0, false);
			lastFireball = Math.Max(0f, lastFireball - (float)time.ElapsedGameTime.Milliseconds);
			if (withinPlayerThreshold(10) && lastFireball == 0f)
			{
				Vector2 trajectory = Utility.getVelocityTowardPlayer(Utility.Vector2ToPoint(getStandingPosition()), 8f, Player);

				currentLocation.projectiles.Add(new BasicProjectile((int)Math.Round(20 * difficulty), 10, 3, 4, 0f, trajectory.X, trajectory.Y, getStandingPosition(), "", "", "", true, false, currentLocation, this));
				if (burstNo == 0)
				{
					currentLocation.playSound("fireball");

				}
				if (burstNo >= (Health < MaxHealth / 2?8:4))
				{
					if (Health < MaxHealth / 4)
					{
						lastFireball = Game1.random.Next(800, 1500);
					}
					lastFireball = Game1.random.Next(1500, 3000);
					burstNo = 0;
				}
				else
				{
					burstNo++;
					lastFireball = 100;
				}
				return;
			}
		}

		public override void reloadSprite(bool onlyAppearance = false)
		{
			if (Sprite == null)
			{
				Sprite = new AnimatedSprite("Characters\\Monsters\\Haunted Skull");
			}
			else
			{
				Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			}
			HideShadow = true;
		}


		public override void drawAboveAllLayers(SpriteBatch b)
		{
			if (!Utility.isOnScreen(Position, 128))
			{
				return;
			}
			previousPositions = (List<Vector2>)GetType().BaseType.GetField("previousPositions", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);

			Vector2 pos_offset = Vector2.Zero;

			if (previousPositions.Count > 2)
			{
				pos_offset = Position - previousPositions[1];
			}

			int direction = (Math.Abs(pos_offset.X) > Math.Abs(pos_offset.Y)) ? ((pos_offset.X > 0f) ? 1 : 3) : ((pos_offset.Y < 0f) ? 0 : 2);

			if (direction == -1)
			{
				direction = 2;
			}

			Vector2 offset = new(0f, width / 2 * (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 188.49555921538757));

			b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(width * 2, height * 4), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f * Scale + offset.Y / 20f, SpriteEffects.None, 0.0001f);
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(width * 2 + Game1.random.Next(-6, 7), height * 2 + Game1.random.Next(-6, 7)) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + ((seenPlayer.Value && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), width, height)), Color.Red * 0.44f, 0f, new Vector2(width / 2f, height), Scale * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f - 1f) / 10000f);
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(width * 2 + Game1.random.Next(-6, 7), height * 2 + Game1.random.Next(-6, 7)) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + ((seenPlayer.Value && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), width, height)), Color.Yellow * 0.44f, 0f, new Vector2(width / 2, height), Scale * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f) / 10000f);
			for (int i = previousPositions.Count - 1; i >= 0; i -= 2)
			{
				b.Draw(Sprite.Texture, new Vector2(previousPositions[i].X - Game1.viewport.X, previousPositions[i].Y - Game1.viewport.Y + yJumpOffset) + drawOffset + new Vector2(height * 2, width * 2) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + ((seenPlayer.Value && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), width, height)), Color.White * (0f + 0.125f * i), 0f, new Vector2(width / 2, height), scale.Value * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f - i) / 10000f);
			}
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(width * 2, height * 2) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + ((seenPlayer.Value && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), width, height)), Color.White, 0f, new Vector2(width / 2, height), scale.Value * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f + 1f) / 10000f);
		}

		protected override void updateAnimation(GameTime time)
		{
			if (focusedOnFarmers || withinPlayerThreshold(20) || seenPlayer.Value)
			{
				Sprite.Animate(time, 0, 4, 80f);
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
				if (shakeTimer < 0)
				{
					currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 103, width, height), position.Value + new Vector2(0f, -32f), false, 0.1f, new Color(255, 50, 255) * 0.8f)
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

			return new((int)(Position.X - Scale * (width + widthOffset - xOffset) / 2 * 4f), (int)(Position.Y - (Scale * (height + heightOffset - yOffset) / 2 - unhitableHeight) * 4f), (int)(Scale * (width + widthOffset) * 4f), (int)((Scale * heightOffset + hitableHeight) * 4f));
		}

		public override void shedChunks(int number, float scale)
		{
			Game1.createRadialDebris(currentLocation, Sprite.textureName.Value, new Rectangle(0, height * 4, width, height), height / 2, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)Tile.Y, Color.White, 4f);
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
