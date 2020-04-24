using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomMonsterFloors
{
    public class ModConfig 
    {
        public int PercentChanceMonsterFloor { get; set; } = 20;
        public string SlimeDinoMonsterSplitPercents { get; set; } = "33:33:34";
        public string SlimeMonsterSplitPercents { get; set; } = "50:50";
        public int MinFloorsBetweenMonsterFloors { get; set; } = 4;
    }
}
