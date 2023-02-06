
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace SprinklerMod
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public SButton SprinkleButton { get; set; } = SButton.MouseRight;
        public SButton SprinkleAllButton { get; set; } = SButton.Enter;
        public Dictionary<string, int> SprinklerRadii { get; set; } = new()
        {
            { "Sprinkler", 0 },
            { "Quality Sprinkler", 1 },
            { "Iridium Sprinkler", 2 }
        };
    }
}
