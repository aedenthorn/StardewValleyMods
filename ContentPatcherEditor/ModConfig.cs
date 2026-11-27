using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace ContentPatcherEditor
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool ShowButton { get; set; } = true;
        public bool Backup { get; set; } = true;
        public bool OpenModsFolderAfterZip { get; set; } = true;
        public string ModsFolder { get; set; } = "";
        public KeybindList MenuButton { get; set; } = new KeybindList(new Keybind(SButton.LeftShift, SButton.OemCloseBrackets));
    }
}
