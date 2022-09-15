using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace FarmAnimalHarvestHelper
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int MaxWaitHour { get; set; } = 1200;
        public Vector2 FirstSlotTile { get; set; } = new Vector2(8, 4);
    }
}
