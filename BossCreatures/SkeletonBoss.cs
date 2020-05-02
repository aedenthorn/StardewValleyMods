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

namespace BossCreatures
{
	public class SkeletonBoss : Skeleton
	{
		private float difficulty;
		private readonly NetBool throwing = new NetBool();
		private bool spottedPlayer;
		private int controllerAttemptTimer;
		private int throwTimer = 0;
		private int throwBurst = 10;
		private int throws = 0;
		private int width;
		private int height;

		public SkeletonBoss() {
		}

		public SkeletonBoss(Vector2 spawnPos, float difficulty) : base(spawnPos)
        {
			width = ModEntry.Config.SkeletonBossWidth;
			height = ModEntry.Config.SkeletonBossHeight;
			Sprite.SpriteWidth = width;
			Sprite.SpriteHeight = height;
			Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));

			this.difficulty = difficulty;

			Health = (int)Math.Round(base.Health * 20 * difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(base.damageToFarmer * 2 * difficulty);

			Scale = ModEntry.Config.SkeletonBossScale;
			this.moveTowardPlayerThreshold.Value = 20;
		}
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.throwing);
			this.position.Field.AxisAlignedMovement = true;
		}
		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			base.MovePosition(time, viewport, currentLocation);
			base.MovePosition(time, viewport, currentLocation);
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			if (!this.throwing)
			{
				this.throwTimer -= time.ElapsedGameTime.Milliseconds;
				base.behaviorAtGameTick(time);
			}
			if (Health <= 0)
			{
				return;
			}
			if (!this.spottedPlayer && !base.wildernessFarmMonster && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, base.getTileLocation(), base.Player.getTileLocation(), 8))
			{
				this.controller = new PathFindController(this, base.currentLocation, new Point(base.Player.getStandingX() / 64, base.Player.getStandingY() / 64), Game1.random.Next(4), null, 200);
				this.spottedPlayer = true;
				base.facePlayer(base.Player);
				//base.currentLocation.playSound("skeletonStep", NetAudio.SoundContext.Default);
				base.IsWalkingTowardPlayer = true;
			}
			else if (this.throwing)
			{
				if (this.invincibleCountdown > 0)
				{
					this.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
					if (this.invincibleCountdown <= 0)
					{
						base.stopGlowing();
					}
				}
				this.Sprite.Animate(time, 20, 5, 150f);
				if (this.Sprite.currentFrame == 24)
				{

					this.throwing.Value = false;
					this.Sprite.currentFrame = 0;
					this.faceDirection(2);
					Vector2 v = Utility.getVelocityTowardPlayer(new Point((int)base.Position.X, (int)base.Position.Y), 8f, base.Player);
					if(Health < MaxHealth / 2)
					{
						if(throws == 0)
						{
							base.currentLocation.playSound("fireball", NetAudio.SoundContext.Default);
						}
						base.currentLocation.projectiles.Add(new BasicProjectile(DamageToFarmer, 4, 0, 0, 0.196349546f, v.X, v.Y, new Vector2(base.Position.X, base.Position.Y), "", "", false, false, base.currentLocation, this, false, null));
						base.currentLocation.projectiles.Add(new BasicProjectile(DamageToFarmer, 10, 0, 4, 0.196349546f, v.X, v.Y, new Vector2(base.Position.X, base.Position.Y), "", "", true, false, base.currentLocation, this, false, null));
						if (++throws > throwBurst *2)
						{
							throwTimer = 1000;
							throws = 0;
						}
						else
						{
							throwTimer = 100;
						}
					}
					else
					{
						base.currentLocation.projectiles.Add(new BasicProjectile(DamageToFarmer, 4, 0, 0, 0.196349546f, v.X, v.Y, new Vector2(base.Position.X, base.Position.Y), "skeletonHit", "skeletonStep", false, false, base.currentLocation, this, false, null));
						if (++throws > throwBurst)
						{
							throwTimer = 1000;
							throws = 0;
						}
						else
						{
							throwTimer = 10;
						}

					}
				}
			}
			else if (this.spottedPlayer && this.controller == null && Game1.random.NextDouble() < 0.5 && !base.wildernessFarmMonster && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, base.getTileLocation(), base.Player.getTileLocation(), 8) && throwTimer <= 0)
			{
				this.throwing.Value = true;
				this.Sprite.currentFrame = 20;
				//base.shake(750);
			}
			else if (this.withinPlayerThreshold(20))
			{
				this.controller = null;
			}
			else if (this.spottedPlayer && this.controller == null && this.controllerAttemptTimer <= 0)
			{
				this.controller = new PathFindController(this, base.currentLocation, new Point(base.Player.getStandingX() / 64, base.Player.getStandingY() / 64), Game1.random.Next(4), null, 200);
				base.facePlayer(base.Player);
				this.controllerAttemptTimer = 2000;
			}
			else if (base.wildernessFarmMonster)
			{
				this.spottedPlayer = true;
				base.IsWalkingTowardPlayer = true;
			}
			this.controllerAttemptTimer -= time.ElapsedGameTime.Milliseconds;
			
		}

		public override void Halt()
		{

		}
		public override void shedChunks(int number)
		{
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName, new Rectangle(0, height*4, width, width), 8, this.GetBoundingBox().Center.X, this.GetBoundingBox().Center.Y, number, (int)base.getTileLocation().Y, Color.White, 4f);
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