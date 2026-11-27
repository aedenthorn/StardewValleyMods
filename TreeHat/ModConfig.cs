
using StardewModdingAPI;

namespace TreeHat
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool AllowOthersToPick { get; set; } = false;
        public SButton CheatButton { get; set; } = SButton.F15;

    }
}
