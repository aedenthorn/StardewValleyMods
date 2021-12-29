
using StardewModdingAPI;

namespace PetBed
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool IsBed { get; set; } = true;

        public int BedChance { get; set; } = 100;
        public string IndoorBedName { get; set; } = "Ped Bed";
        public string OutdoorBedName { get; set; } = "Ped Bed";
        public string IndoorBedOffset { get; set; } = "0,0";
        public string OutdoorBedOffset { get; set; } = "0,0";
    }
}
