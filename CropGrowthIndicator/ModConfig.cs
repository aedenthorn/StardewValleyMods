
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace CropGrowthIndicator
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool ShowDaysInCurrentPhase { get; set; } = true;
        public string ReadyText { get; set; } = "Ready!";
        public Color CurrentGrowthColor { get; set; } = Color.Yellow;
        public Color TotalGrowthColor { get; set; } = Color.LightGreen;
    }
}
