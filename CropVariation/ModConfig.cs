
using StardewModdingAPI;

namespace CropVariation
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool EnableTrellisResize { get; set; } = true;
        public int ColorVariation { get; set; } = 40;
        public int SizeVariationPercent { get; set; } = 20;
        public int SizeVariationQualityFactor { get; set; } = 100;
        public int ColorVariationQualityFactor { get; set; } = 100;
    }
}
