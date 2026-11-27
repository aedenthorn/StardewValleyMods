using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace FishTankWallpaper
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public SButton RemoveModKey { get; set; } = SButton.LeftShift;
        public KeybindList MoveUpKey { get; set; } = new(SButton.Up);
        public KeybindList MoveDownKey { get; set; } = new(SButton.Down);
    }
}
