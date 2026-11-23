
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace SlimeHutchAggro
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool NoAggro { get; set; } = false;
        public bool NoBlockedAggro { get; set; } = true;
    }
}
