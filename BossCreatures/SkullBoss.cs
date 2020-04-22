using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Projectiles;
using System;
using System.Reflection;

namespace BossCreatures
{
    internal class SkullBoss : Bat
    {
        internal GameLocation currentLocation;
		private float lastFireball;
		private readonly NetEvent0 fireballEvent = new NetEvent0(false);

		public SkullBoss(Vector2 position) : base(position, 77377)
        {
			Health = base.Health * 20;
			MaxHealth = Health;
			Scale = 3f;
            DamageToFarmer = base.damageToFarmer * 2;
			this.moveTowardPlayerThreshold.Value = 20;
		}

		int burstNo = 0;
		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			base.MovePosition(time, viewport, currentLocation);
			if (Health < MaxHealth / 2)
			{
				base.MovePosition(time, viewport, currentLocation);
			}

		}
		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);

			if (Health <= 0)
			{
				return;
			}

			base.faceGeneralDirection(base.Player.Position, 0, false);
			this.lastFireball = Math.Max(0f, this.lastFireball - (float)time.ElapsedGameTime.Milliseconds);
			if (this.withinPlayerThreshold(10) && this.lastFireball == 0f)
			{
				Vector2 trajectory = Utility.getVelocityTowardPlayer(Utility.Vector2ToPoint(base.getStandingPosition()), 8f, base.Player);
				base.currentLocation.projectiles.Add(new BasicProjectile(30, 10, 3, 4, 0f, trajectory.X, trajectory.Y, base.getStandingPosition(), "", "", true, false, base.currentLocation, this, false, null));
				if (burstNo == 0)
				{
					base.currentLocation.playSound("fireball", NetAudio.SoundContext.Default);

				}
				
				if (burstNo >= (Health < MaxHealth / 2?4:2))
				{
					if (Health < MaxHealth / 4)
					{
						this.lastFireball = (float)Game1.random.Next(800, 1500);
					}
					else
					{

					}
					this.lastFireball = (float)Game1.random.Next(1500, 3000);
					burstNo = 0; 
				}
				else
				{
					burstNo++;
					this.lastFireball = 100;
				}
				return;
			}
		}
		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int mHealth = Health;
			int result = base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
			if (mHealth - result <= 0)
			{
				ModEntry.PHelper.Events.Display.RenderedHud -= ModEntry.OnRenderedHud;

				ModEntry.SpawnBossLoot(currentLocation, position.X, position.Y);

				Game1.playSound("Cowboy_Secret");
				ModEntry.RevertMusic();
			}
			return result;
		}
	}
}