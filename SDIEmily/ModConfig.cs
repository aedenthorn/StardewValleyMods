using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace SDIEmily
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int MaxSkillRange { get; set; } = 256;
        public int MaxSkillDistanceOffLookAxis { get; set; } = 64;
        public int SkillRadius { get; set; } = 128;
        public int SkillHits { get; set; } = 5;
        public float SkillSpeed { get; set; } = 1f;
        public float SkillDamageMult { get; set; } = 1f;
    }
}
