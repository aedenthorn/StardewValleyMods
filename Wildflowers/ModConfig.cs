using StardewModdingAPI;
using System.Collections.Generic;

namespace Wildflowers
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool WildFlowersMakeFlowerHoney { get; set; } = true;
        public float wildflowerGrowChance { get; set; } = 0.05f;
        public List<string> DisallowNames { get; set; } = new List<string>();
    }
}
