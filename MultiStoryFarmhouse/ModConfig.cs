
using System.Collections.Generic;

namespace MultiStoryFarmhouse
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public List<string> FloorNames { get; set; } = new List<string>()
        {
            "ManyRooms",
            "EmptyHall"
        };
        public int MainFloorStairsX { get; set; } = 7;
        public int MainFloorStairsY { get; set; } = 22;
    }
}
