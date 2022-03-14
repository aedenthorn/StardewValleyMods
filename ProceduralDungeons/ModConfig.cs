using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProceduralDungeons
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int MapWidth { get; set; } = 100;
        public int MapHeight { get; set; } = 100;
        public int RoomSizeFactor { get; set; } = 10;
        public int RoomAmountFactor { get; set; } = 50;
    }
}
