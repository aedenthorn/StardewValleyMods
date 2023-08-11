using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace CropWateringBubbles
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool RequireKeyPress { get; set; } = false;
        public bool OnlyWhenWatering { get; set; } = false;
        public bool IncludeGiantable { get; set; } = true;
        public int RepeatInterval { get; set; } = 3;
        public int OpacityPercent { get; set; } = 75;
        public int SizePercent { get; set; } = 100;
        public KeybindList PressKeys { get; set; } = new KeybindList(new Keybind(SButton.LeftShift, SButton.OemPeriod));
    }
}
