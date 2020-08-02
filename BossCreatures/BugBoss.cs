using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using System;

namespace BossCreatures
{
    public class BugBoss : Bat
    {
		private float lastFly;
		private float lastDebuff;
		private int MaxFlies;
		public float difficulty;
		private int width;
		private int height;

		public BugBoss() { }

		public BugBoss(Vector2 spawnPos, float _difficulty) : base(spawnPos)
        {
			width = ModEntry.Config.BugBossWidth;
			height = ModEntry.Config.BugBossHeight;
			Sprite.SpriteWidth = width;
			Sprite.SpriteHeight = height;
			this.Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
			Scale = ModEntry.Config.BugBossScale;

			difficulty = _difficulty;

			Health = (int)Math.Round(base.Health * 100 * difficulty);
			MaxHealth = Health;
			DamageToFarmer = (int)Math.Round(base.damageToFarmer * difficulty);
			MaxFlies = 4;


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

					base.currentLocation.projectiles.Add(new DebuffingProjectile(14, 2, 4, 4, 0.196349546f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2((float)this.GetBoundingBox().X, (float)this.GetBoundingBox().Y), base.currentLocation, this));
					this.lastDebuff = Game1.random.Next(3000, 6000);
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
						if(Health < MaxHealth / 2)
						{
							Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(this.GetBoundingBox().Center, 15f, base.Player);
							base.currentLocation.projectiles.Add(new DebuffingProjectile(13, 7, 4, 4, 0.196349546f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2((float)this.GetBoundingBox().X, (float)this.GetBoundingBox().Y), base.currentLocation, this));
						}
						currentLocation.characters.Add(new ToughFly(Position, difficulty)
						{
							focusedOnFarmers = true
						});
						this.lastFly = (float)Game1.random.Next(4000, 8000);
					}
				}
			}
		}
		public override Rectangle GetBoundingBox()
		{
			return new Rectangle((int)(base.Position.X + 8 * Scale), (int)(base.Position.Y + 16 * Scale), (int)(this.Sprite.SpriteWidth * 4 * 3 / 4 * Scale), (int)(32 * Scale));
			Rectangle r = new Rectangle((int)(Position.X - Scale * width / 2), (int)(Position.Y - Scale * height / 2), (int)(Scale * width), (int)(Scale * height));
			return r;
		}
		public override void drawAboveAllLayers(SpriteBatch b)
		{
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(width*2, height*2), new Rectangle?(this.Sprite.SourceRect), (this.shakeTimer > 0) ? Color.Red : Color.White, 0f, new Vector2(width/2, height/2), scale * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.92f);
			b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(width*2, height*2), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, base.wildernessFarmMonster ? 0.0001f : ((float)(base.getStandingY() - 1) / 10000f));
			if (this.isGlowing)
			{
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(width * 2, height * 2), new Rectangle?(this.Sprite.SourceRect), this.glowingColor * this.glowingTransparency, 0f, new Vector2(width/2, height/2), scale * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.99f : ((float)base.getStandingY() / 10000f + 0.001f)));
			}
		}
		public override void shedChunks(int number, float scale)
		{
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName, new Rectangle(0, height*4, width, height), width/2, this.GetBoundingBox().Center.X, this.GetBoundingBox().Center.Y, number, (int)base.getTileLocation().Y, Color.White, 4f);
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