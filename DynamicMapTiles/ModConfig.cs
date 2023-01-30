using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace DynamicMapTiles
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool IsDebug { get; set; } = false;
        public bool TriggerDuringEvents { get; set; } = false;

    }
}
