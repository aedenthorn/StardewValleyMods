using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ToggleFullScreen
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public KeybindList ToggleButtons { get; set; } = new KeybindList(new Keybind(SButton.LeftAlt, SButton.Enter));
        public int LastWindowedWidth { get; set; } = -1;
        public int LastWindowedHeight { get; set; } = -1;
    }
}
