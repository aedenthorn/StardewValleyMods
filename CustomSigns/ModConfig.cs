
using StardewModdingAPI;

namespace CustomSigns
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public SButton ModKey { get; set; } = SButton.LeftShift;
        public SButton ResetKey { get; set; } = SButton.F5;
    }
}
