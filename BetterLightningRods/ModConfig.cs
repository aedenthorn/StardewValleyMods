
using StardewModdingAPI;

namespace BetterLightningRods
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool UniqueCheck { get; set; } = true;
        public bool OnlyCheckEmpty { get; set; } = false;
        public int RodsToCheck { get; set; } = 2;
        public float LightningChance { get; set; } = 13f;
        public bool Astraphobia { get; set; } = false;
    }
}
