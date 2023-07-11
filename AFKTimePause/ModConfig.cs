using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace AFKTimePause
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool FreezeGame { get; set; } = true;
        public int ticksTilAFK { get; set; } = 3600;
        public bool ShowAFKText { get; set; } = false;
        public bool WakeOnMouseMove { get; set; } = true;
        public string AFKText { get; set; } = "AFK";
    }
}
