using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;

namespace SDIEmily
{
    internal class RunningSkill
    {
        public GameLocation currentLocation;
        public Vector2 center;
        public bool isCaster;
        
        public Vector2 currentStartPos;
        public Vector2 currentEndPos;
        public Vector2 currentPos;
        public int currentLoop;
        public int currentTick;
        public int totalTicks;
        public int currentFrame;
        public MeleeWeapon weapon;


        public RunningSkill(GameLocation currentLocation, Vector2 center, bool isCaster, Tool weapon)
        {
            this.currentLocation = currentLocation;
            this.center = center;
            this.isCaster = isCaster;
            if(weapon is MeleeWeapon)
                this.weapon = weapon as MeleeWeapon;

        }
    }
}