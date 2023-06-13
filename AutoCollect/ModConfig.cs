using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace AutoCollect
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public KeybindList ToggleKey { get; set; } = new KeybindList(SButton.NumPad9);
        public int MaxDistance { get; set; } = 1;
    }
}
