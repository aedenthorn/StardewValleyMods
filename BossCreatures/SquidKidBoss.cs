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
using System.Reflection;

namespace BossCreatures
{
	public class SquidKidBoss : SquidKid
	{
		private float difficulty;
		private float lastIceBall;
		private float lastLightning = 2000f;
		private bool startedLightning = false;
		private Vector2 playerPosition;
		private int width;
		private int height;

		public SquidKidBoss() {
		}

		public SquidKidBoss(Vector2 spawnPos, float _difficulty) : base(spawnPos)
        {
			width = ModEntry.Config.SquidKidBossWidth;
			height = ModEntry.Config.SquidKidBossHeight;
			Sprite.SpriteWidth = width;
			Sprite.SpriteHeight = height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));

			this.difficulty = _difficulty;
			Health = (int)Math.Round(base.Health * 1500 * difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(damageToFarmer * 2 * difficulty);

			Scale = ModEntry.Config.SquidKidBossScale;
			this.moveTowardPlayerThreshold.Value = 20;
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
			typeof(SquidKidBoss).BaseType.GetField("lastFireball", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this as SquidKid,1000f);
			base.behaviorAtGameTick(time);
			if (Health <= 0)
			{
				return;
			}

			if (this.withinPlayerThreshold(20))
			{
				this.lastIceBall = Math.Max(0f, this.lastIceBall - (float)time.ElapsedGameTime.Milliseconds);
				this.lastLightning = Math.Max(0f, this.lastLightning - (float)time.ElapsedGameTime.Milliseconds);

				if (!startedLightning && lastLightning < (ModEntry.IsLessThanHalfHealth(this) ? 500f : 1000f))
				{
					startedLightning = true;

					List<Farmer> farmers = new List<Farmer>();
					FarmerCollection.Enumerator enumerator = currentLocation.farmers.GetEnumerator();
					while (enumerator.MoveNext())
					{
						farmers.Add(enumerator.Current);
					}
					playerPosition = farmers[Game1.random.Next(0, farmers.Count)].position;

					Microsoft.Xna.Framework.Rectangle lightningSourceRect = new Rectangle(0, 0, 16, 16);
					float markerScale = 8f;
					Vector2 drawPosition = playerPosition + new Vector2(-16*markerScale/2 + 32f,-16*markerScale/ 2 + 32f);

					Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\Projectiles", lightningSourceRect, 9999f, 1, 999, drawPosition, false, Game1.random.NextDouble() < 0.5, (playerPosition.Y + 32f) / 10000f + 0.001f, 0.025f, Color.White, markerScale, 0f, 0f, 0f, false)
					{
						light = true,
						lightRadius = 2f,
						delayBeforeAnimationStart = 200,
						lightcolor = Color.Black
					});
				}

				if (lastLightning == 0f)
				{
					startedLightning = false;
					lightningStrike(playerPosition);
					lastLightning = Game1.random.Next(1000, 2000) * (ModEntry.IsLessThanHalfHealth(this) ? 1 : 2);
				}
				if (lastIceBall == 0f)
				{
					Vector2 trajectory = ModEntry.VectorFromDegree(Game1.random.Next(0,360)) * 10f;
					currentLocation.projectiles.Add(new BossProjectile((int)Math.Round(20 * difficulty), 9, 3, 4, 0f, trajectory.X, trajectory.Y, getStandingPosition(), "", "", true, false, currentLocation, this, false, null, 19));

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
						setTrajectory((int)Utility.getVelocityTowardPlayer(Utility.Vector2ToPoint(base.getStandingPosition()), 8f, base.Player).X, (int)(-(int)Utility.getVelocityTowardPlayer(Utility.Vector2ToPoint(base.getStandingPosition()), 8f, base.Player).Y));
					}
				}

			}
		}

		private void lightningStrike(Vector2 playerLocation)
		{
			Farm.LightningStrikeEvent lightningEvent = new Farm.LightningStrikeEvent();
			lightningEvent.bigFlash = true;
			lightningEvent.createBolt = true;
			lightningEvent.boltPosition = playerLocation + new Vector2(32f, 32f);
			Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
			Game1.playSound("thunder");
			Utility.drawLightningBolt(lightningEvent.boltPosition, currentLocation);

			List<Farmer> farmers = new List<Farmer>();
			FarmerCollection.Enumerator enumerator = currentLocation.farmers.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.currentLocation == currentLocation && enumerator.Current.GetBoundingBox().Intersects(new Rectangle((int)Math.Round(playerLocation.X - 32), (int)Math.Round(playerLocation.Y - 32), 64, 64)))
				{
					enumerator.Current.takeDamage((int)Math.Round(20 * difficulty), true, null);
				}
			}


		}
		public override Rectangle GetBoundingBox()
		{
			Rectangle r = new Rectangle((int)(Position.X - Scale * width / 2), (int)(Position.Y - Scale * height / 2), (int)(Scale * width), (int)(Scale * height));
			return r;
		}
		public override void drawAboveAllLayers(SpriteBatch b)
		{
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(width*2, (float)(21 + this.yOffset)), new Rectangle?(this.Sprite.SourceRect), Color.White, 0f, new Vector2(width/2, height), scale * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)base.getStandingY() / 10000f)));
			b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(width*2, height*4), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + (float)this.yOffset / 20f, SpriteEffects.None, (float)(base.getStandingY() - 1) / 10000f);
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