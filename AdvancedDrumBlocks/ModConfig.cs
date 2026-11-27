
using StardewModdingAPI;

namespace AdvancedDrumBlocks
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public SButton IndexModKey { get; set; } = SButton.LeftShift;
        public int CurrentPitch { get; set; } = 0;
    }
}
