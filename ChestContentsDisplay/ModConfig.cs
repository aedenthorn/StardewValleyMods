using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace ChestContentsDisplay
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool RequireKeyPress { get; set; } = false;
        public bool ShowTarget { get; set; } = true;
        public int PauseFrames { get; set; } = 30;
        public int Width { get; set; } = 12;
        public KeybindList PressKeys { get; set; } = new KeybindList(SButton.None);
    }
}
