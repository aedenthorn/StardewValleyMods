using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace CropHarvestBubbles
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool RequireKeyPress { get; set; } = false;
        public bool IgnoreFlowers { get; set; } = true;
        public int OpacityPercent { get; set; } = 75;
        public int SizePercent { get; set; } = 100;
        public KeybindList PressKeys { get; set; } = new KeybindList(SButton.None);
    }
}
