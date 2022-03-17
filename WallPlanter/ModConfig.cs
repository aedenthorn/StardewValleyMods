
using StardewModdingAPI;

namespace WallPlanter
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public SButton ModKey { get; set; } = SButton.LeftShift;
        public SButton UpButton { get; set; } = SButton.Up;
        public SButton DownButton { get; set; } = SButton.Down;
        public int OffsetY { get; set; } = 64;
        public int InnerOffsetY { get; set; } = 0;
    }
}
