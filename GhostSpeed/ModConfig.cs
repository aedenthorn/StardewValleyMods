using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace GhostSpeed
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public float SpeedMultiplier { get; set; } = 2.0f;
        public int TilesKnockedBack { get; set; } = 6;
    }
}
