using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace GardenPotTweaks
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool EnableHoney { get; set; } = true;
        public bool FixFlowerFind { get; set; } = true;
        public bool EnableSprinklering { get; set; } = true;
        public bool EnableAncientSeeds { get; set; } = false;
        public bool EnableMoving { get; set; } = true;
    }
}
