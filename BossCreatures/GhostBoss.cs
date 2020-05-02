using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using System;
using System.Reflection;

namespace BossCreatures
{
    public class GhostBoss : Ghost
    {
		private float lastGhost;
		private float lastDebuff;
		private int MaxGhosts;
		private float difficulty;
		private int width;
		private int height;

		public GhostBoss() { }
		public GhostBoss(Vector2 spawnPos, float difficulty) : base(spawnPos)
        {
			width = ModEntry.Config.GhostBossWidth;
			height = ModEntry.Config.GhostBossHeight;
			Sprite.SpriteWidth = width;
			Sprite.SpriteHeight = height;
			this.Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Scale = ModEntry.Config.GhostBossScale;

			this.difficulty = difficulty;

			Health = (int)Math.Round(base.Health * 20 * difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(base.damageToFarmer * difficulty);
			MaxGhosts = 4;

			this.moveTowardPlayerThreshold.Value = 20;
		}
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);


			if (Health <= 0)
			{
				return;
			}

			this.lastGhost = Math.Max(0f, this.lastGhost - (float)time.ElapsedGameTime.Milliseconds);
			this.lastDebuff = Math.Max(0f, this.lastDebuff - (float)time.ElapsedGameTime.Milliseconds);

			if (withinPlayerThreshold(10))
			{
				if(lastDebuff == 0f)
				{
					Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(this.GetBoundingBox().Center, 15f, base.Player);
					if (ModEntry.IsLessThanHalfHealth(this))
					{
						for (int i = 0; i < 12; i++)
						{
							Vector2 trajectory = ModEntry.VectorFromDegree(i * 30) * 10f;
							currentLocation.projectiles.Add(new BossProjectile((int)Math.Round(20 * difficulty), 9, 3, 4, 0f, trajectory.X, trajectory.Y, getStandingPosition(), "", "", true, false, currentLocation, this, false, null, false, 19));
						}
					}
					else
					{
						currentLocation.projectiles.Add(new BossProjectile((int)Math.Round(20 * difficulty), 9, 3, 4, 0f, velocityTowardPlayer.X, velocityTowardPlayer.Y, getStandingPosition(), "", "", true, false, currentLocation, this, false, null, false, 19));
					}


					this.lastDebuff = Game1.random.Next(3000, 6000);
				}
				if (lastGhost == 0f)
				{
					int ghosts = 0;
					using (NetCollection<NPC>.Enumerator enumerator = currentLocation.characters.GetEnumerator())
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
					if (ghosts < (Health < MaxHealth / 2 ? this.MaxGhosts * 2 : this.MaxGhosts))
					{
						currentLocation.characters.Add(new ToughGhost(Position, difficulty)
						{
							currentLocation = base.currentLocation
						});
						this.lastGhost = (float)Game1.random.Next(1000, 2000);
					}
				}
			}
		}
		public override void drawAboveAllLayers(SpriteBatch b)
		{
			int offset  = (int)GetType().BaseType.GetField("yOffset", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);

			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(width*2, (float)(21 + offset)), new Microsoft.Xna.Framework.Rectangle?(this.Sprite.SourceRect), Color.White, 0f, new Vector2(width/2, width), scale * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : (getStandingY() / 10000f)));
			b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(width*2, width*4), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + offset / 20f * width/16, SpriteEffects.None, (getStandingY() - 1) / 10000f);
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int result = base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
			if (Health <= 0)
			{
				ModEntry.BossDeath(currentLocation, position, difficulty);

			}
			ModEntry.MakeBossHealthBar(Health, MaxHealth);
			return result;
		}
	}
}