using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagnetMod
{
    class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public float MagnetRangeMult { get; set; } = -1;
        public int MagnetSpeedMult { get; set; } = 2;
        public bool NoLootBounce { get; set; } = false;
        public bool NoLootWave { get; set; } = false;
    }
}
