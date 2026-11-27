using Microsoft.Xna.Framework;

namespace Swim
{
    public class SwimmerData
    {
        public int oxygen;
        public bool isJumping;
        public bool isUnderwater;
        public Vector2 startJumpLoc;
        public Vector2 endJumpLoc;
        public ulong lastJump = 0;
        public int ticksUnderwater = 0;
        public int ticksWearingScubaGear = 0;
        public int bubbleOffset = 0;
        public int lastBreatheSound;
        public bool surfacing;
        public bool swimSuitAlways;
        public bool readyToSwim = true;
    }
}