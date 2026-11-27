using StardewModdingAPI;

namespace QuickLoad
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public SButton Hotkey { get; set; } = SButton.F7;
        public string SaveFolder { get; set; }
        public bool UseLastLoaded { get; set; } = true;
    }
}
