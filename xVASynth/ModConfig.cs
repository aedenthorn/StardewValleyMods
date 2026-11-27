
using StardewModdingAPI;

namespace GroundhogDay
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;

        public SButton ToggleModKey { get; set; } = SButton.Pause;
    }
}
