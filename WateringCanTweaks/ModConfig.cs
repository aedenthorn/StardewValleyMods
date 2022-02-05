
using StardewModdingAPI;

namespace WateringCanTweaks
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public float ChargedStaminaMult { get; set; } = 1;
        public float VolumeMult { get; set; } = 1;
        public float WaterMult { get; set; } = 1;
        public bool FillAdjacent { get; set; } = true;
    }
}
