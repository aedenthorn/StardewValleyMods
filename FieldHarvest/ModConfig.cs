
using StardewModdingAPI;

namespace FieldHarvest
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool AllowDiagonal { get; set; } = false;
        public bool OnlySameSeed { get; set; } = true;
        public bool AutoCollect { get; set; } = false;

        public SButton ModButton { get; set; } = SButton.LeftShift;

    }
}
