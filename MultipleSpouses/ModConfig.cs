using Netcode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleSpouses
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool BuyPendantsAnytime { get; set; } = false;
        public bool BuildAllSpousesRooms { get; set; } = true;
        public float MaxDistanceToKiss { get; set; } = 200f;
        public double MinSpouseKissInterval { get; set; } = 5;
        public float SpouseKissChance { get; set; } = 0.5f;
        public bool AllowSpousesToKiss { get; set; } = true;
    }
}
