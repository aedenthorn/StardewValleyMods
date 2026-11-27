using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;

namespace FireBreath
{
	public class Fireball : BasicProjectile
	{
		public Fireball(int damageToFarmer, int spriteIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, string collisionSound = null, string bounceSound = null, string firingSound = null, bool explode = false, bool damagesMonsters = false, GameLocation location = null, Character firer = null, onCollisionBehavior collisionBehavior = null, string shotItemId = null) : base(damageToFarmer, spriteIndex, bouncesTillDestruct, tailLength, rotationVelocity, xVelocity, yVelocity, startingPosition, collisionSound, bounceSound, firingSound, explode, damagesMonsters, location, firer, collisionBehavior, shotItemId)
		{
		}

		public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
		{
			if (!damagesMonsters.Value)
				return;

			Farmer playerWhoFiredMe = GetPlayerWhoFiredMe(location);
			explosionAnimation(location);

			if (n is Monster)
			{
				location.damageMonster(n.GetBoundingBox(), damageToFarmer.Value, (int)damageToFarmer.Value + 1, isBomb: false, playerWhoFiredMe, isProjectile: true);
			}
			else if (ModEntry.Config.FireAnnoysNonMonsters)
			{
				n.getHitByPlayer(playerWhoFiredMe, location);
			}
		}
	}
}
