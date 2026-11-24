using StardewModdingAPI;
using System.Collections.Generic;

namespace FishTankCapacity
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool UnlimitedCapacity { get; set; } = true;
        public SButton AddAnyItemModKey { get; set; } = SButton.LeftShift;
        public float CapacityMultiplier { get; set; } = 1;
        public Dictionary<string, int> CustomCapacities { get; set; } = new() { {"PutAquariumItemIdHere", 42 } };
    }
}
