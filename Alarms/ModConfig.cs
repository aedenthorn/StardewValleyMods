using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace Alarms
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public string DefaultSound { get; set; } = "rooster";
        public KeybindList MenuButton { get; set; } = new KeybindList(new Keybind(SButton.LeftShift, SButton.OemPipe));
    }
}
