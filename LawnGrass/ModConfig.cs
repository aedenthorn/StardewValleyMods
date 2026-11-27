
using StardewModdingAPI;

namespace LawnGrass
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public int MaxDailyGrowth { get; set; } = 1;
        public float GrowChance { get; set; } = 0.5f;
        public float SproutChance { get; set; } = 1f;
        public SButton ModKey { get; set; } = SButton.LeftControl;
        public bool LawnByDefault { get; set; } = true;
        public bool AllGrassIsLawn { get; set; } = false;
        public bool ReturnGrassStarter { get; set; } = true;
        public bool ProtectNonLawn { get; set; } = true;
        public bool TrufflesInGrass { get; set; } = true;
        public bool MatchMapGrass { get; set; } = false;
        public int MapGrassTileIndex { get; set; } = 175;
    }
}
