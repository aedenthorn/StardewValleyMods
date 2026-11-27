using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace LauncherDrawer
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool CustomPosition { get; set; } = false;
        public int X { get; set; } = -1;
        public int Y { get; set; } = -1;
        public int MaxEntries { get; set; } = -1;
        public int DrawerSpeed { get; set; } = 10;
        public string OpenSound { get; set; } = "doorCreak";
        public string CloseSound { get; set; } = "doorCreakReverse";
        public string KeybindSound { get; set; } = "coin";
        public string LinkSound { get; set; } = "coin";
        public KeybindList DrawerKey { get; set; } = new(SButton.NumPad5);
        public KeybindList HideKey { get; set; } = new(SButton.MouseMiddle);
        public HashSet<string> HideList { get; set; } = new();
        public List<string> Keybinds { get; set; } = new()
        {
            "GMCM|Generic Mod Config Menu|F12"
        };
        public List<string> Links { get; set; } = new()
        {
            "Wiki|Stardew Wiki|https://stardewvalleywiki.com"
        };
    }
}
