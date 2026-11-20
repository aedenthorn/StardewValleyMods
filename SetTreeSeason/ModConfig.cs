
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace SetTreeSeason
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public KeybindList NextSeason { get; set; } = new(SButton.NumPad8);
        public KeybindList PrevSeason { get; set; } = new(SButton.NumPad2);
        public KeybindList ResetSeason { get; set; } = new(SButton.NumPad5);
    }
}
