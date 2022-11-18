using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace FastForward
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int SpeedMult { get; set; } = 5;
        public SButton ModKey { get; set; } = SButton.RightAlt;
    }
}
