
using StardewModdingAPI;

namespace Chess
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool FreeMode { get; set; } = false;
        public SButton ModeKey { get; set; } = SButton.H;
        public SButton SetupKey { get; set; } = SButton.J;
        public SButton SwapKey { get; set; } = SButton.K;
        public SButton ClearKey { get; set; } = SButton.L;
    }
}
