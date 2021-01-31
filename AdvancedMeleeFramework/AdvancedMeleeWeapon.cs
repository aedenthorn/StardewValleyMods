using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace AdvancedMeleeFramework
{
    public class AdvancedMeleeWeapon
    {
        public string id = "none";
        public int cooldown = 1500;
        public List<MeleeActionFrame> frames = new List<MeleeActionFrame>();
    }

    public class MeleeActionFrame
    {
        public int frameTicks;
        public WeaponFarmerAnimation animation;
        public WeaponAction action;
        public string sound;
        public List<WeaponProjectile> projectiles = new List<WeaponProjectile>();
        public int relativeFacingDirection = 0;
        public int trajectoryX = 0;
        public int trajectoryY = 0;
    }

    public enum WeaponAction
    {
        NONE,
        NORMAL,
        SPECIAL
    }

    public class WeaponProjectile
    {
        public int damage = 0;
        public int parentSheetIndex = 0;
        public int bouncesTillDestruct = 0;
        public int tailLength = 1;
        public float rotationVelocity = 1;
        public float xVelocity = 0;
        public float yVelocity = -1;
        public float startingPositionX = 0;
        public float startingPositionY = 0;
        public string collisionSound;
        public string firingSound;
        public bool explode = false;
        public bool damagesMonsters = true;
        public bool spriteFromObjectSheet = false;
    }

    public class WeaponFarmerAnimation
    {
        public int frame;
        public float duration;
        public int frames;
    }
}