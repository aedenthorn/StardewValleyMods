using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace ThrowableBombs
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool FreezeGame { get; set; } = true;
        public int SecondsTilAFK { get; set; } = 60;
        public bool ShowAFKText { get; set; } = false;
        public string AFKText { get; set; } = "AFK";
    }
}
