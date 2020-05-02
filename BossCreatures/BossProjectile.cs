using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Network;
using StardewValley.Projectiles;

namespace BossCreatures
{
    public class BossProjectile : BasicProjectile
    {
		private bool playSound;
		int debuff;

		public BossProjectile()
		{

		}
		public BossProjectile(int damageToFarmer, int parentSheetIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, string collisionSound, string firingSound, bool explode, bool damagesMonsters = false, GameLocation location = null, Character firer = null, bool spriteFromObjectSheet = false, BasicProjectile.onCollisionBehavior collisionBehavior = null, bool playSound = false, int debuff = -1) : base(damageToFarmer, parentSheetIndex, bouncesTillDestruct, tailLength, rotationVelocity, xVelocity, yVelocity, startingPosition, collisionSound, firingSound, explode, damagesMonsters, location, firer, spriteFromObjectSheet, collisionBehavior)
		{
			this.debuff = debuff;
			this.playSound = playSound;

			if (playSound)
			{
				if (location == null)
				{
					Game1.playSound("debuffSpell");
				}
				else
				{
					location.playSound("debuffSpell", NetAudio.SoundContext.Default);
				}
			}
		}

		public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player)
		{
			base.behaviorOnCollisionWithPlayer(location, player);
			if(debuff > 0)
			{
				Game1.buffsDisplay.addOtherBuff(new Buff(debuff));
				if(playSound)
				{
					location.playSound("debuffHit", NetAudio.SoundContext.Default);
				}
			}
		}
	}
}