using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;

namespace Swim
{
    public class BigFishie : DinoMonster
	{
		public BigFishie() : base()
		{
		}

		public BigFishie(Vector2 position) : base(position)
		{
			Sprite.LoadTexture("Fishies/" + ModEntry.bigFishTextures[Game1.random.Next(ModEntry.bigFishTextures.Count)]);
			Scale = 0.5f + (float)Game1.random.NextDouble()/4f;
			DamageToFarmer = 0;
			moveTowardPlayerThreshold.Value = 50;
			Slipperiness = 24 + Game1.random.Next(10);
			collidesWithOtherCharacters.Value = false;
			farmerPassesThrough = true;
		}
		public override void drawAboveAllLayers(SpriteBatch b)
		{
			invincibleCountdown = 1000;
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.IsWalkingTowardPlayer = false;
			this.nextChangeDirectionTime -= time.ElapsedGameTime.Milliseconds;
			this.nextWanderTime -= time.ElapsedGameTime.Milliseconds;
			if (this.nextChangeDirectionTime < 0)
			{
				this.nextChangeDirectionTime = Game1.random.Next(500, 1000);
				int facingDirection = base.FacingDirection;
				this.facingDirection.Value = (this.facingDirection.Value + (Game1.random.Next(0, 3) - 1) + 4) % 4;
			}
			if (this.nextWanderTime < 0)
			{
				if (this.wanderState)
				{
					this.nextWanderTime = Game1.random.Next(1000, 2000);
				}
				else
				{
					this.nextWanderTime = Game1.random.Next(1000, 3000);
				}
				this.wanderState = !this.wanderState;
			}
			if (this.wanderState)
			{
				this.moveLeft = (this.moveUp = (this.moveRight = (this.moveDown = false)));
				base.tryToMoveInDirection(this.facingDirection.Value, false, base.DamageToFarmer, this.isGlider);
			}
		}
	}
}
