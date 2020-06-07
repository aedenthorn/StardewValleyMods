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
        public int MinPointsToMarry { get; set; } = 2500;
        public int MinPointsToDate { get; set; } = 2000;
        public int DaysUntilMarriage { get; set; } = 3;
        public bool FriendlyDivorce { get; set; } = true;
        public bool BuildAllSpousesRooms { get; set; } = true;
        public int HallTileOdd { get; set; } = 265;
        public int HallTileEven { get; set; } = 266;
        public bool AllowSpousesToKiss { get; set; } = true;
        public float SpouseKissChance { get; set; } = 0.5f;
        public bool RealKissSound { get; set; } = false;
        public float MaxDistanceToKiss { get; set; } = 200f;
        public double MinSpouseKissInterval { get; set; } = 5;
        public double BabyRequestChance { get; set; } = 0.05f;
        public bool AllowGayPregnancies { get; set; } = true;
        public float FemaleBabyChance { get; set; } = 0.5f;
        public int PregnancyDays { get; set; } = 14;
        public int MaxChildren { get; set; } = 2;
    }
}
