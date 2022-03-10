
using StardewModdingAPI;

namespace RobinWorkHours
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int StartTime { get; set; } = 900;
        public int EndTime { get; set; } = 1700;
        public int FarmTravelTime { get; set; } = 160;
        public int TownTravelTime { get; set; } = 120;
        public int BackwoodsTravelTime { get; set; } = 170;

    }
}
