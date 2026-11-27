
using StardewModdingAPI;

namespace FurnitureAdjustment
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool MoveCursor { get; set; } = true;

        public SButton RaiseButton { get; set; } = SButton.NumPad8;
        public SButton LowerButton { get; set; } = SButton.NumPad2;
        public SButton LeftButton { get; set; } = SButton.NumPad4;
        public SButton RightButton { get; set; } = SButton.NumPad6;
        public SButton ModKey { get; set; } = SButton.LeftAlt;
        public int ModSpeed { get; set; } = 5;
        public int MoveSpeed { get; set; } = 10;
    }
}
