using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace SDIEmily
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int MaxSkillRange { get; set; } = 256;
        public int MaxSkillDistanceOffLookAxis { get; set; } = 64;
    }
}
