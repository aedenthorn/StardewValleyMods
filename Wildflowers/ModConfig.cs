using StardewModdingAPI;
using System.Collections.Generic;

namespace Wildflowers
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool WildFlowersMakeFlowerHoney { get; set; } = true;
        public bool FixFlowerFind { get; set; } = true;
        public int BeeRange { get; set; } = 5;
        public bool WeaponsHarvestFlowers { get; set; } = false;
        public float wildflowerGrowChance { get; set; } = 0.005f;
        public List<string> DisallowNames { get; set; } = new List<string>();
    }
}
