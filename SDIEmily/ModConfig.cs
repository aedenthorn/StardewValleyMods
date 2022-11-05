using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace SDIEmily
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int MaxSkillRange { get; set; } = 256;
        public int MaxBurstRange { get; set; } = 512;
        public int MaxSkillDistanceOffLookAxis { get; set; } = 64;
        public int MaxBurstDistanceOffLookAxis { get; set; } = 64;
        public int SkillRadius { get; set; } = 128;
        public int BurstRadius { get; set; } = 512;
        public int BurstPullSpeed { get; set; } = 128;
        public int SkillHits { get; set; } = 5;
        public float SkillSpeed { get; set; } = 1f;
        public float BurstSpeed { get; set; } = 1f;
        public int BurstEndRadius { get; set; } = 16;
        public float SkillDamageMult { get; set; } = 0.2f;
        public float BurstDamageMult { get; set; } = 0.2f;
    }
}
