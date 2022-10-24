using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace AFKTimePause
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int SecondsTilAFK { get; set; } = 60;
        public bool ShowAFKText { get; set; } = false;
        public bool ShowOKMenu { get; set; } = true;
        public string AFKText { get; set; } = "AFK";
    }
}
