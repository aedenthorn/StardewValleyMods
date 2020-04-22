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
    internal class GhostBoss : Ghost
    {
		private float lastGhost;
		private float lastDebuff;
		private int MaxGhosts;
		private float difficulty;

		public GhostBoss(Vector2 spawnPos, float difficulty) : base(spawnPos)
        {
			this.difficulty = difficulty;

			Health = (int)Math.Round(base.Health * 20 * difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(base.damageToFarmer * difficulty);
			MaxGhosts = 5;

			Scale = 3f;
			this.moveTowardPlayerThreshold.Value = 20;
		}
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
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

					base.currentLocation.projectiles.Add(new DebuffingProjectile(19, 7, 4, 4, 0.196349546f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2((float)this.GetBoundingBox().X, (float)this.GetBoundingBox().Y), base.currentLocation, this));
					this.lastDebuff = Game1.random.Next(5000, 10000);
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
						this.lastGhost = (float)Game1.random.Next(300, 600);
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
				ModEntry.PHelper.Events.Display.RenderedHud -= ModEntry.OnRenderedHud;

				ModEntry.SpawnBossLoot(currentLocation, position.X, position.Y, difficulty);

				Game1.playSound("Cowboy_Secret");
				ModEntry.RevertMusic();
			}
			return result;
		}
	}
}