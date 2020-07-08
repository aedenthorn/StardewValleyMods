using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Familiars
{
    public class BatFamiliar : Familiar
	{
		public BatFamiliar()
		{
		}

		public BatFamiliar(Vector2 position, Farmer _owner) : base("Bat", position)
		{
			owner = _owner;
			base.Slipperiness = 20 + Game1.random.Next(-5, 6);

			ModEntry.SMonitor.Log($"creating new familiar of type {Name}");
			reloadSprite();

			farmerPassesThrough = true;
			this.Halt();
			base.IsWalkingTowardPlayer = false;
			base.HideShadow = true;
			damageToFarmer.Value = 0;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(new INetSerializable[]
			{
				this.wasHitCounter,
				this.lastHitCounter,
				this.turningRight,
				this.seenPlayer,
				this.cursedDoll,
				this.hauntedSkull
			});
		}

        public override bool IsMonster => true;

        public override void reloadSprite()
		{
			ModEntry.SMonitor.Log($"reloading bat familiar sprite for {Name} {ModEntry.Config.BatTexture}");

			if (this.Sprite == null)
			{
				ModEntry.SMonitor.Log($"creating new sprite");
				this.Sprite = new AnimatedSprite(ModEntry.Config.BatTexture);
			}
			else
			{
				ModEntry.SMonitor.Log($"updating sprite texture");
				this.Sprite.textureName.Value = ModEntry.Config.BatTexture;
			}
			if (!ModEntry.Config.DefaultBatColor)
			{
				typeof(AnimatedSprite).GetField("spriteTexture", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Sprite, Utils.ColorFamiliar(Sprite.Texture, ModEntry.Config.BatMainColor, ModEntry.Config.BatRedColor, ModEntry.Config.BatGreenColor, ModEntry.Config.BatBlueColor));
			}
			base.HideShadow = true;
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			if (who != null)
			{
				return 0;
			}
			return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
		}

		public override void shedChunks(int number, float scale)
		{
			if (this.cursedDoll && this.hauntedSkull)
			{
				Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName, new Rectangle(0, 64, 16, 16), 8, this.GetBoundingBox().Center.X, this.GetBoundingBox().Center.Y, number, (int)base.getTileLocation().Y, Color.White, 4f);
				return;
			}
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName, new Rectangle(0, 384, 64, 64), 32, this.GetBoundingBox().Center.X, this.GetBoundingBox().Center.Y, number, (int)base.getTileLocation().Y, Color.White, scale);
		}
		public override void drawAboveAllLayers(SpriteBatch b)
		{
			if (Utility.isOnScreen(base.Position, 128))
			{
				if (this.cursedDoll)
				{
					if (this.hauntedSkull)
					{
						Vector2 pos_offset = Vector2.Zero;
						if (this.previousPositions.Count > 2)
						{
							pos_offset = base.Position - this.previousPositions[1];
						}
						int direction = (Math.Abs(pos_offset.X) > Math.Abs(pos_offset.Y)) ? ((pos_offset.X > 0f) ? 1 : 3) : ((pos_offset.Y < 0f) ? 0 : 2);
						if (direction == -1)
						{
							direction = 2;
						}
						Vector2 offset = new Vector2(0f, 8f * (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 188.49555921538757));
						b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + offset.Y / 20f, SpriteEffects.None, 0.0001f);
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(32 + Game1.random.Next(-6, 7)), (float)(32 + Game1.random.Next(-6, 7))) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(this.Sprite.Texture, direction * 2 + ((this.seenPlayer && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16)), Color.Red * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.position.Y + 128f - 1f) / 10000f);
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(32 + Game1.random.Next(-6, 7)), (float)(32 + Game1.random.Next(-6, 7))) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(this.Sprite.Texture, direction * 2 + ((this.seenPlayer && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16)), Color.Yellow * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.position.Y + 128f) / 10000f);
						for (int i = this.previousPositions.Count - 1; i >= 0; i -= 2)
						{
							b.Draw(this.Sprite.Texture, new Vector2(this.previousPositions[i].X - (float)Game1.viewport.X, this.previousPositions[i].Y - (float)Game1.viewport.Y + (float)this.yJumpOffset) + this.drawOffset + new Vector2(32f, 32f) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(this.Sprite.Texture, direction * 2 + ((this.seenPlayer && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16)), Color.White * (0f + 0.125f * (float)i), 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.position.Y + 128f - (float)i) / 10000f);
						}
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 32f) + offset, new Rectangle?(Game1.getSourceRectForStandardTileSheet(this.Sprite.Texture, direction * 2 + ((this.seenPlayer && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16)), Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.position.Y + 128f + 1f) / 10000f);
						return;
					}
					Vector2 offset2 = new Vector2(0f, 8f * (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 188.49555921538757));
					b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + offset2.Y / 20f, SpriteEffects.None, 0.0001f);
					b.Draw(Game1.objectSpriteSheet, base.getLocalPosition(Game1.viewport) + new Vector2((float)(32 + Game1.random.Next(-6, 7)), (float)(32 + Game1.random.Next(-6, 7))) + offset2, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 103, 16, 16)), Color.Violet * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.position.Y + 128f - 1f) / 10000f);
					b.Draw(Game1.objectSpriteSheet, base.getLocalPosition(Game1.viewport) + new Vector2((float)(32 + Game1.random.Next(-6, 7)), (float)(32 + Game1.random.Next(-6, 7))) + offset2, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 103, 16, 16)), Color.Lime * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.position.Y + 128f) / 10000f);
					b.Draw(Game1.objectSpriteSheet, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 32f) + offset2, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 103, 16, 16)), new Color(255, 50, 50), 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.position.Y + 128f + 1f) / 10000f);
					return;
				}
				else
				{
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 32f), new Rectangle?(this.Sprite.SourceRect), (this.shakeTimer > 0) ? Color.Red : Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.92f);
					b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, base.wildernessFarmMonster ? 0.0001f : ((float)(base.getStandingY() - 1) / 10000f));
					if (this.isGlowing)
					{
						b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 32f), new Rectangle?(this.Sprite.SourceRect), this.glowingColor * this.glowingTransparency, 0f, new Vector2(8f, 16f), Math.Max(0.2f, this.scale) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.99f : ((float)base.getStandingY() / 10000f + 0.001f)));
					}
				}
			}
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
		}
		public override void behaviorAtGameTick(GameTime time)
		{
			invincibleCountdown = 1000;
			if (this.timeBeforeAIMovementAgain > 0f)
			{
				this.timeBeforeAIMovementAgain -= (float)time.ElapsedGameTime.Milliseconds;
			}
			if (lastHitCounter >= 0)
            {
				lastHitCounter.Value -= time.ElapsedGameTime.Milliseconds;
			}

			chargingMonster = false;
			if(lastHitCounter < 0)
            {
				foreach (NPC npc in currentLocation.characters)
				{
					if (npc is Familiar)
						continue;

					if (npc is Monster && Utils.monstersColliding(this, (Monster)npc))
					{
						currentLocation.damageMonster(GetBoundingBox(), ModEntry.Config.BatMinDamage, ModEntry.Config.BatMaxDamage, false, owner);
						lastHitCounter.Value = 5000;
						chargingMonster = false;
						break;
					}
					else if (npc is Monster && Utils.withinMonsterThreshold(this, (Monster)npc, 2))
					{
						chargingMonster = true;
						if (currentTarget == null || Vector2.Distance(npc.position, position) < Vector2.Distance(currentTarget.position, position))
						{
							currentTarget = (Monster)npc;
						}
					}
				}
			}

			if (this.wasHitCounter >= 0)
			{
				this.wasHitCounter.Value -= time.ElapsedGameTime.Milliseconds;
			}

			if (chargingMonster || (withinPlayerThreshold(10) && followingPlayer))
			{
				this.seenPlayer.Value = true;

				Rectangle targetRect = chargingMonster ? currentTarget.GetBoundingBox() : owner.GetBoundingBox();

				float xSlope = (float)(-(float)(targetRect.Center.X - this.GetBoundingBox().Center.X));
				float ySlope = (float)(targetRect.Center.Y - this.GetBoundingBox().Center.Y);
				float t = Math.Max(1f, Math.Abs(xSlope) + Math.Abs(ySlope));
				if (t < (float)((this.extraVelocity > 0f) ? 192 : 64))
				{
					this.xVelocity = Math.Max(-this.maxSpeed, Math.Min(this.maxSpeed, this.xVelocity * 1.05f));
					this.yVelocity = Math.Max(-this.maxSpeed, Math.Min(this.maxSpeed, this.yVelocity * 1.05f));
				}
				xSlope /= t;
				ySlope /= t;
				if (this.wasHitCounter <= 0)
				{
					this.targetRotation = (float)Math.Atan2((double)(-(double)ySlope), (double)xSlope) - 1.57079637f;
					if ((double)(Math.Abs(this.targetRotation) - Math.Abs(this.rotation)) > 2.748893571891069 && Game1.random.NextDouble() < 0.5)
					{
						this.turningRight.Value = true;
					}
					else if ((double)(Math.Abs(this.targetRotation) - Math.Abs(this.rotation)) < 0.39269908169872414)
					{
						this.turningRight.Value = false;
					}
					if (this.turningRight)
					{
						this.rotation -= (float)Math.Sign(this.targetRotation - this.rotation) * 0.0490873866f;
					}
					else
					{
						this.rotation += (float)Math.Sign(this.targetRotation - this.rotation) * 0.0490873866f;
					}
					this.rotation %= 6.28318548f;
					this.wasHitCounter.Value = 0;
				}
				float maxAccel = Math.Min(5f, Math.Max(1f, 5f - t / 64f / 2f)) + this.extraVelocity;
				xSlope = (float)Math.Cos((double)this.rotation + 1.5707963267948966);
				ySlope = -(float)Math.Sin((double)this.rotation + 1.5707963267948966);
				this.xVelocity += -xSlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				this.yVelocity += -ySlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				if (Math.Abs(this.xVelocity) > Math.Abs(-xSlope * this.maxSpeed))
				{
					this.xVelocity -= -xSlope * maxAccel / 6f;
				}
				if (Math.Abs(this.yVelocity) > Math.Abs(-ySlope * this.maxSpeed))
				{
					this.yVelocity -= -ySlope * maxAccel / 6f;
				}
			}
		}

		protected override void updateAnimation(GameTime time)
		{
			if (base.focusedOnFarmers || this.withinPlayerThreshold(6) || this.seenPlayer)
			{
				this.Sprite.Animate(time, 0, 4, 80f);
				if (this.Sprite.currentFrame % 3 == 0 && Utility.isOnScreen(base.Position, 512) && (this.batFlap == null || !this.batFlap.IsPlaying) && Game1.soundBank != null && base.currentLocation == Game1.currentLocation && !this.cursedDoll)
				{
					batFlap = Game1.soundBank.GetCue("batFlap");
					batFlap.Play();
				}
				if (this.cursedDoll.Value)
				{
					this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.shakeTimer < 0)
					{
						if (!this.hauntedSkull.Value)
						{
							base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 103, 16, 16), this.position + new Vector2(0f, -32f), false, 0.1f, new Color(255, 50, 255) * 0.8f)
							{
								scale = 4f
							});
						}
						this.shakeTimer = 50;
					}
					this.previousPositions.Add(base.Position);
					if (this.previousPositions.Count > 8)
					{
						this.previousPositions.RemoveAt(0);
					}
				}
			}
			else
			{
				this.Sprite.currentFrame = 4;
				this.Halt();
			}
			base.resetAnimationSpeed();
		}


		private readonly NetInt wasHitCounter = new NetInt(0);

		private readonly NetInt lastHitCounter = new NetInt(0);

		private float targetRotation;

		private readonly NetBool turningRight = new NetBool();

		private readonly NetBool seenPlayer = new NetBool();

		private readonly NetBool cursedDoll = new NetBool();

		private readonly NetBool hauntedSkull = new NetBool();

		private ICue batFlap;

		private float extraVelocity;

		private float maxSpeed = 5f;

		private List<Vector2> previousPositions = new List<Vector2>();
		public Monster currentTarget = null;
		private bool chargingMonster;
	}
}
