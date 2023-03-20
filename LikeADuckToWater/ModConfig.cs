using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace LikeADuckToWater
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool SwimAfterAutoPet { get; set; } = true;
        public bool EatBeforeSwimming { get; set; } = true;
        public float MaxDistance { get; set; } = 20;
        public float ChancePerTick { get; set; } = 0.03f;
        public int FriendshipGain { get; set; } = 10;
    }
}
