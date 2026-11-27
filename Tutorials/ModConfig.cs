
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Tutorials
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int SidebarWidth { get; set; } = 400;
        public int Height { get; set; } = 1000;
        public KeybindList OpenTutorialKey { get; set; } = new(new Keybind( SButton.LeftControl, SButton.F1));
        public bool Debug { get; set; } = false;
    }
}
