using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;

namespace SDIEmily
{
    public class RunningBurst
    {
        public GameLocation currentLocation;
        public Vector2 center;
        public bool isCaster;
        public Color color;

        public int currentTick;
        public int currentAngle;
        public float currentRadius;
        public int currentFrame;
        public MeleeWeapon weapon;

        public RunningBurst(GameLocation currentLocation, Vector2 center, Color color, bool isCaster, Tool weapon)
        {
            this.currentLocation = currentLocation;
            this.center = center;
            this.color = color;
            this.isCaster = isCaster;
            currentRadius = ModEntry.Config.BurstRadius;
            if (weapon is MeleeWeapon)
                this.weapon = weapon as MeleeWeapon;

        }
    }
}