
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace AdvancedMenuPositioning
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public KeybindList MoveKeys { get; set; } = new KeybindList(new Keybind(SButton.LeftShift, SButton.MouseLeft));
        public KeybindList DetachKeys { get; set; } = new KeybindList(new Keybind(SButton.LeftShift, SButton.X));
        public KeybindList CloseKeys { get; set; } = new KeybindList(new Keybind(SButton.LeftShift, SButton.Z));
        public bool StrictKeybindings { get; set; } = true;
    }
}