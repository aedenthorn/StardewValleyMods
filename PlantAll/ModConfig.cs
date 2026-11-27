
using StardewModdingAPI;

namespace PlantAll
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool AllowDiagonal { get; set; } = true;
        public SButton ModButton { get; set; } = SButton.LeftShift;
        public SButton StraightModButton { get; set; } = SButton.LeftControl;
        public SButton SprinklerModButton { get; set; } = SButton.LeftAlt;

    }
}
