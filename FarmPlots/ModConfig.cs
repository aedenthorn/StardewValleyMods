using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace FarmPlots
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public KeybindList MenuKey { get; set; } = new KeybindList(new Keybind(SButton.LeftShift, SButton.F));
        public SButton CreateKey { get; set; } = SButton.MouseLeft;
        public SButton DeleteKey { get; set; } = SButton.MouseRight;
    }
}
