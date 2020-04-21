using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using System;

namespace BossCreatures
{
    internal class BugBoss : Bat
    {
        private string defaultMusic;
		private float lastFly;
		private float lastDebuff;
		private int MaxFlies = 5;

		public BugBoss(Vector2 spawnPos, string music) : base(spawnPos)
        {
            this.defaultMusic = music;
			this.Sprite.LoadTexture("Characters\\Monsters\\Bug");
			this.Sprite.SpriteHeight = 16;
			Health = base.Health * 150;
			MaxHealth = Health;
			Scale = 3f;
			DamageToFarmer = base.damageToFarmer * 2;
			this.moveTowardPlayerThreshold.Value = 20;
		}
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);

			if (Health <= 0)
			{
				return;
			}

			this.lastFly = Math.Max(0f, this.lastFly - (float)time.ElapsedGameTime.Milliseconds);
			this.lastDebuff = Math.Max(0f, this.lastDebuff - (float)time.ElapsedGameTime.Milliseconds);

			if (withinPlayerThreshold(10))
			{
				if(lastDebuff == 0f)
				{
					Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(this.GetBoundingBox().Center, 15f, base.Player);

					base.currentLocation.projectiles.Add(new DebuffingProjectile(14, 7, 4, 4, 0.196349546f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2((float)this.GetBoundingBox().X, (float)this.GetBoundingBox().Y), base.currentLocation, this));
					this.lastDebuff = Game1.random.Next(1500, 3000);
				}
				if (lastFly == 0f)
				{
					int flies = 0;
					using (NetCollection<NPC>.Enumerator enumerator = currentLocation.characters.GetEnumerator())
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
					if (flies < (Health < MaxHealth / 2 ? this.MaxFlies * 2 : this.MaxFlies))
					{
						currentLocation.characters.Add(new ToughFly(Position)
						{
							currentLocation = base.currentLocation
						});
						if (Health < MaxHealth / 2)
						{
							this.lastFly = (float)Game1.random.Next(150, 300);
						}
						else
						{
							this.lastFly = (float)Game1.random.Next(300, 600);
						}
					}
				}
			}
		}
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int mHealth = Health;
			int result = base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
			if (mHealth - result <= 0)
			{
				ModEntry.PMonitor.Log("Boss dead", LogLevel.Alert);
				ModEntry.PHelper.Events.Display.RenderedHud -= ModEntry.OnRenderedHud;

				ModEntry.SpawnBossLoot(currentLocation, position.X, position.Y);

				Game1.playSound("Cowboy_Secret");
				if (!(currentLocation is MineShaft))
				{
					ModEntry.PMonitor.Log("resetting music to " + defaultMusic, LogLevel.Alert);
					Game1.changeMusicTrack(defaultMusic, false);
				}
			}
			return result;
		}
	}
}