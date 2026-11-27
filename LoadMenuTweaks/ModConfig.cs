
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace LoadMenuTweaks
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int MaxTrees { get; set; } = 20;
        public int TreeChancePercent { get; set; } = 5;
        public int TreeGrowthStage { get; set; } = 5;
        public int ObjectChancePercent { get; set; } = 2;
    }
}
