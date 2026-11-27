using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace MovieTheatreTweaks
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool RemoveCraneMan { get; set; } = true;
        public int CloseTime { get; set; } = 2100;
        public int OpenTime { get; set; } = 900;
    }
}
