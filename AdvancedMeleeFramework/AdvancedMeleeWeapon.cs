using System.Collections.Generic;

namespace AdvancedMeleeFramework
{
    public class AdvancedMeleeWeapon
    {
        public string id = "none";
        public int type = 0;
        public List<AdvancedEnchantmentData> enchantments = new List<AdvancedEnchantmentData>();
        public int skillLevel = 0;
        public int cooldown = 1500;
        public List<MeleeActionFrame> frames = new List<MeleeActionFrame>();
        public Dictionary<string, string> config = new Dictionary<string, string>();
    }

    public class MeleeActionFrame
    {
        public int frameTicks;
        public bool? invincible = null;
        public SpecialEffect special = null;
        public WeaponFarmerAnimation animation;
        public WeaponAction action;
        public string sound;
        public List<AdvancedWeaponProjectile> projectiles = new List<AdvancedWeaponProjectile>();
        public int relativeFacingDirection = 0;
        public float trajectoryX = 0;
        public float trajectoryY = 0;
        public Dictionary<string, string> config = new Dictionary<string, string>();
    }

    public class SpecialEffect
    {
        public string name;
        public Dictionary<string, string> parameters;
        public Dictionary<string, string> config = new Dictionary<string, string>();
    }
    public class AdvancedEnchantmentData
    {
        public string name;
        public string type;
        public Dictionary<string, string> parameters;
        public Dictionary<string, string> config = new Dictionary<string, string>();
    }

    public enum WeaponAction
    {
        NONE,
        NORMAL,
        SPECIAL
    }

    public class AdvancedWeaponProjectile
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
        public Dictionary<string, string> config = new Dictionary<string, string>();
    }

    public class WeaponFarmerAnimation
    {
        public int frame;
        public float duration;
        public int frames;
    }
}