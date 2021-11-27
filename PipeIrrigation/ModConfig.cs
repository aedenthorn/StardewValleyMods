
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace PipeIrrigation
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool WaterSurroundingTiles { get; set; } = true;
        public bool ShowSprinklerAnimations { get; set; } = true;
        public bool ShowWateredTilesLabelOnGrid { get; set; } = true;
        public int PercentWaterPerTile { get; set; } = 25;
    }
}
