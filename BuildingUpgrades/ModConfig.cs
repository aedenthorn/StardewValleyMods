using StardewModdingAPI;
using System.Collections.Generic;

namespace BuildingUpgrades
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool AllowOverlapPlacement { get; set; } = true;
        public bool AllowPickupIfUnder { get; set; } = false;
    }
}
