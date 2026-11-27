
using StardewModdingAPI;

namespace PetBed
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool IndoorIsBed { get; set; } = true;
        public bool OutdoorIsBed { get; set; } = true;

        public int BedChance { get; set; } = 100;
        public string IndoorBedName { get; set; } = "Pet Bed";
        public string OutdoorBedName { get; set; } = "Pet Bed";
        public string IndoorBedOffset { get; set; } = "0,0";
        public string OutdoorBedOffset { get; set; } = "0,0";
    }
}
