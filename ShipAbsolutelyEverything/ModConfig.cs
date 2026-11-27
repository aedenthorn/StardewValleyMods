
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley.Objects;
using StardewValley.Quests;
using System.Collections.Generic;
using System.Net;

namespace ShipAbsolutelyEverything
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public List<string> IncludeTypes { get; set; } = new List<string>()
        {
            "Basic",
            "Arch",
            "Minerals",
            "Crafting",
            "Fish",
            "Cooking",
            "Seeds"
        };
        public List<string> ExcludedItems { get; set; } = new List<string>()
        {
            
        };
    }
}
