using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace TreasureChestsExpanded
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int TreasureBaseValue { get; set; } = 100;
        public float TreasureIncreaseRate { get; set; } = 0.3f;
        public int ItemMinValue { get; set; } = 20;
        public int ItemMaxValue { get; set; } = -1;
        public bool IncludeCoins { get; set; } = true;
        public int BaseCoinsMin { get; set; } = 100;
        public int BaseCoinsMax { get; set; } = 500;
        public float CoinsIncreaseRate { get; set; } = 0.3f;
        public bool IncludeHats { get; set; } = true;
        public bool IncludeRings { get; set; } = true;
        public bool IncludePants { get; set; } = true;
        public bool IncludeShirts { get; set; } = true;
        public bool IncludeBoots { get; set; } = true;
        public bool IncludeWeapons { get; set; } = true;
        public bool IncludeBigCraftables { get; set; } = true;
        public bool IncludeRelics { get; set; } = true;
        public bool IncludeSeeds { get; set; } = true;
        public bool IncludeMinerals { get; set; } = true;
        public bool IncludeFood { get; set; } = false;
        public bool IncludeFish { get; set; } = false;
        public bool IncludeBasicObjects { get; set; } = false;
    }
}
