
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley.Objects;
using StardewValley.Quests;
using System.Collections.Generic;
using System.Net;

namespace PushInChairs
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public KeybindList ToggleKey { get; set; } = new KeybindList(new Keybind(SButton.LeftShift, SButton.MouseLeft));
    }
    
}
