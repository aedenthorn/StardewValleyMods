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
        public bool intro = true;
        public Color color;

        public Vector2 currentStartPos;
        public Vector2 currentEndPos;
        public Vector2 currentPos;
        public int currentLoop = -1;
        public int currentTick;
        public int totalTicks;
        public int currentFrame;
        public MeleeWeapon weapon;


        public RunningSkill(GameLocation currentLocation, Vector2 farmerPos, Vector2 center, Color color, bool isCaster, Tool weapon)
        {
            this.currentLocation = currentLocation;
            currentStartPos = farmerPos - new Vector2(0, 64);
            this.color = color;
            this.center = center;
            this.isCaster = isCaster;
            if(weapon is MeleeWeapon)
                this.weapon = weapon as MeleeWeapon;

        }
    }
}