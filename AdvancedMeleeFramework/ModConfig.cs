using StardewModdingAPI;

namespace AdvancedMeleeFramework
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public SButton ReloadButton { get; set; } = SButton.NumPad0;
        public bool RequireModKey { get; set; } = false;
        public SButton ModKey { get; set; } = SButton.LeftShift;
    }
}
