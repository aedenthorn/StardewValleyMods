
using StardewModdingAPI;

namespace SittingAction
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public SButton SuppressKey { get; set; } = SButton.LeftShift;
    }
}
