using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace FlowerColors
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool HoverToShowCombined { get; set; } = false;
        public bool CombineOnOrganize { get; set; } = true;
        public bool AppendCombinedNumberToName { get; set; } = true;
        public bool FixSliderBar { get; set; } = true;
        public bool FixWhiteFlowerDrawing { get; set; } = true;
        public KeybindList CombineButton { get; set; } = new (new Keybind(SButton.MouseMiddle));
        public KeybindList CopyButton { get; set; } = new (new Keybind(SButton.LeftControl, SButton.C));
        public KeybindList PasteButton { get; set; } = new (new Keybind(SButton.LeftControl, SButton.V));
        public KeybindList PickButton { get; set; } = new (new Keybind(SButton.LeftControl, SButton.X));
        public SButton PrismaticModKey { get; set; } = SButton.LeftShift;
        public SButton ScrollModKey { get; set; } = SButton.None;
    }
}
