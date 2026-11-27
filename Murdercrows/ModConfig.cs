using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Murdercrows
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public float MaxDistanceSpawn { get; set; } = 25;
        public int MaxSimultaneousMonsters { get; set; } = 20;
        public bool EnableMonsterWaves { get; set; } = true;
    }
}
