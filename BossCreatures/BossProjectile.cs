using Microsoft.Xna.Framework;
using xTile.Dimensions;
using StardewValley;
using StardewValley.Projectiles;

namespace BossCreatures
{
	public class BossProjectile : BasicProjectile
	{
		private readonly bool pullPlayerIn;
		private Vector2 startingPosition;

		public BossProjectile()
		{
		}

		public BossProjectile(int damageToFarmer, int parentSheetIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, string collisionSound, string bounceSound, string firingSound, bool explode, bool damagesMonsters = false, GameLocation location = null, Character firer = null, onCollisionBehavior collisionBehavior = null, string shotItemId = null, string debuff = null, bool pullIn = false) : base(damageToFarmer, parentSheetIndex, bouncesTillDestruct, tailLength, rotationVelocity, xVelocity, yVelocity, startingPosition, collisionSound, bounceSound, firingSound, explode, damagesMonsters, location, firer, collisionBehavior, shotItemId)
		{
			this.debuff.Value = debuff;
			this.startingPosition = startingPosition;
			pullPlayerIn = pullIn;
		}

		public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player)
		{
			base.behaviorOnCollisionWithPlayer(location, player);
			if (debuff.Value is not null)
			{
				player.applyBuff(new Buff(debuff.Value));
			}
			if (pullPlayerIn)
			{
				Vector2 newPosition = player.Position + (startingPosition - player.Position) * 0.02f;

				if (location.isTilePassable(new Location((int)(newPosition.X / 64), (int)(newPosition.Y / 64)), Game1.viewport))
				{
					player.Position = newPosition;
				}
			}
		}
	}
}
