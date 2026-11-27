using StardewModdingAPI;

namespace LadderSpawnOnEntry
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool EnableForDangerous { get; set; } = true;
        public SButton ToggleKey { get; set; } = SButton.None;
    }
}
