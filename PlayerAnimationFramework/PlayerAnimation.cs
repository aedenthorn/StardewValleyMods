using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace PlayerAnimationFramework
{
    public class PlayerAnimation
    {
        public string chatTrigger;
        public string keyTrigger;
        public List<PlayerAnimationFrame> animations = new List<PlayerAnimationFrame>();
    }

    public class PlayerAnimationFrame
    {
        public float jitter;
        public float jump;
        public int frame = -1;
        public int facing = -1;
        public string sound;
        public int length;
        public bool secondaryArm;
        public bool flip;
    }
}