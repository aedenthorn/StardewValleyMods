using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace LikeADuckToWater
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public float MaxDistance { get; set; } = 20;
        public int FriendshipGain { get; set; } = 15;
    }
}
