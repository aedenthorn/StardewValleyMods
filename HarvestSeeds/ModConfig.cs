
using StardewModdingAPI;

namespace HarvestSeeds
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool RegrowableSeeds { get; set; } = false;
        public int SeedChance { get; set; } = 10;
        public int MinSeeds { get; set; } = 1;
        public int MaxSeeds { get; set; } = 2;

    }
}
