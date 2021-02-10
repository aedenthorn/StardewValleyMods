using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomLocks
{
    public class ModConfig
    {
        public bool Enabled { get; set; } = true;
        public bool AllowSeedShopWed { get; set; } = true;
        public bool AllowOutsideTime { get; set; } = true;
        public bool AllowStrangerHomeEntry { get; set; } = false;
        public bool AllowStrangerRoomEntry { get; set; } = false;
        public bool AllowAdventureGuildEntry { get; set; } = false;
        public bool IgnoreEvents { get; set; } = false;
    }
}
