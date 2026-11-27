using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixtyNine
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public float MaxDistanceNice { get; set; } = 256f;
    }
}
