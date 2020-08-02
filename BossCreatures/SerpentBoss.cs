using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;

namespace BossCreatures
{
	public class SerpentBoss : Serpent
    {
		public int timeUntilNextAttack;
		public readonly NetBool firing = new NetBool(false);
		public NetInt attackState = new NetInt();
		public int nextFireTime;
		public int totalFireTime;
		private float difficulty;
		private int width;
		private int height;

		public SerpentBoss() { 
		}

		public SerpentBoss(Vector2 position, float difficulty) : base(position)
        {
			width = ModEntry.Config.SerpentBossWidth;
			height = ModEntry.Config.SerpentBossHeight;
			Sprite.SpriteWidth = width;
			Sprite.SpriteHeight = height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Scale = ModEntry.Config.SerpentBossScale;

			this.difficulty = difficulty;
			Health = (int)Math.Round(base.Health * 15 * difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(base.damageToFarmer * difficulty);
			timeUntilNextAttack = 100;
			this.moveTowardPlayerThreshold.Value = 20;
		}
		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			base.MovePosition(time, viewport, currentLocation);
			if (Health < MaxHealth / 2)
			{
				base.MovePosition(time, viewport, currentLocation);
			}

		}

		protected override void updateAnimation(GameTime time)
        {
            base.updateAnimation(time);
        }

		protected override void initNetFields()
		{
			base.NetFields.AddFields(new INetSerializable[]
			{
				this.attackState,
				this.firing
			});
			base.initNetFields();
		}
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);

			if (Health <= 0)
			{
				return;
			}

			// fire!

			this.timeUntilNextAttack -= time.ElapsedGameTime.Milliseconds;
			if (this.attackState.Value == 0 && this.withinPlayerThreshold(5))
			{
				this.firing.Set(false);
				if (this.timeUntilNextAttack < 0)
				{
					this.timeUntilNextAttack = 0;
					this.attackState.Set(1);
					this.nextFireTime = 50;
					this.totalFireTime = 3000;
					return;
				}
			}
			else if (this.totalFireTime > 0)
			{
				Farmer player = base.Player;
				if (!this.firing)
				{
					if (player != null)
					{
						base.faceGeneralDirection(player.Position, 0, false);
					}
				}
				this.totalFireTime -= time.ElapsedGameTime.Milliseconds;
				if (this.nextFireTime > 0)
				{
					this.nextFireTime -= time.ElapsedGameTime.Milliseconds;
					if (this.nextFireTime <= 0)
					{
						if (!this.firing.Value)
						{
							this.firing.Set(true);
							base.currentLocation.playSound("furnace", NetAudio.SoundContext.Default);
						}
						float fire_angle = 0f;
						Vector2 shot_origin = new Vector2((float)this.GetBoundingBox().Center.X, (float)this.GetBoundingBox().Center.Y);
						base.faceGeneralDirection(player.Position, 0, false);
						switch (this.facingDirection.Value)
						{
							case 0:
								this.yVelocity = -1f;
								shot_origin.Y -= 64f;
								fire_angle = 90f;
								break;
							case 1:
								this.xVelocity = -1f;
								shot_origin.X += 64f;
								fire_angle = 0f;
								break;
							case 2:
								this.yVelocity = 1f;
								fire_angle = 270f;
								break;
							case 3:
								this.xVelocity = 1f;
								shot_origin.X -= 64f;
								fire_angle = 180f;
								break;
						}
						fire_angle += (float)Math.Sin((double)((float)this.totalFireTime / 1000f * 180f) * 3.1415926535897931 / 180.0) * 25f;
						Vector2 shot_velocity = new Vector2((float)Math.Cos((double)fire_angle * 3.1415926535897931 / 180.0), -(float)Math.Sin((double)fire_angle * 3.1415926535897931 / 180.0));
						shot_velocity *= 10f;
						BasicProjectile projectile = new BasicProjectile((int)Math.Round(10 * difficulty), 10, 0, 1, 0.196349546f, shot_velocity.X, shot_velocity.Y, shot_origin, "", "", false, false, base.currentLocation, this, false, null);
						projectile.ignoreTravelGracePeriod.Value = true;
						projectile.maxTravelDistance.Value = 512;
						base.currentLocation.projectiles.Add(projectile);
						if (Health < MaxHealth / 2)
						{
							base.currentLocation.projectiles.Add(new BasicProjectile((int)Math.Round(10 * difficulty), 10, 3, 4, 0f, shot_velocity.X, shot_velocity.Y, shot_origin, "", "", true, false, base.currentLocation, this, false, null));
						}
						this.nextFireTime = 50;
					}
				}
				if (this.totalFireTime <= 0)
				{
					this.totalFireTime = 0;
					this.nextFireTime = 0;
					this.attackState.Set(0);
					this.timeUntilNextAttack = Game1.random.Next(2000, 4000);
				}
			}
		}
		public override void reloadSprite()
		{
			this.Sprite = new AnimatedSprite("Characters\\Monsters\\Serpent");
			this.Sprite.SpriteWidth = width;
			this.Sprite.SpriteHeight = height;
			this.Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			base.HideShadow = true;
		}

		public override Rectangle GetBoundingBox()
		{
			return new Rectangle((int)(base.Position.X + 8 * Scale), (int)base.Position.Y, (int)(this.Sprite.SpriteWidth * 4 * 3 / 4 * Scale), (int)(96 * Scale));
			Rectangle r = new Rectangle((int)(Position.X - Scale * width / 2), (int)(Position.Y - Scale * height / 2), (int)(Scale * width), (int)(Scale * height));
			return r;
		}
		public override void drawAboveAllLayers(SpriteBatch b)
		{
			if (Utility.isOnScreen(base.Position, 128))
			{
				b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(64f, (float)this.GetBoundingBox().Height), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)(base.getStandingY() - 1) / 10000f);
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(width*2, (float)(this.GetBoundingBox().Height / 2)), new Rectangle?(this.Sprite.SourceRect), Color.White, this.rotation, new Vector2(width/2, height/2), scale * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)(base.getStandingY() + 8) / 10000f)));
				if (this.isGlowing)
				{
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(width*2, (float)(this.GetBoundingBox().Height / 2)), new Rectangle?(this.Sprite.SourceRect), this.glowingColor * this.glowingTransparency, this.rotation, new Vector2(width/2, height/2), scale * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)(base.getStandingY() + 8) / 10000f + 0.0001f)));
				}
			}
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