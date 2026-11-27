using StardewModdingAPI;

namespace BuildingShift
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int ShiftAmountNormal { get; set; } = 1;
        public int ShiftAmountMod { get; set; } = 8;
        public SButton ResetKey { get; set; } = SButton.RightShift;
        public SButton LockKey { get; set; } = SButton.L;
        public SButton ShiftDown { get; set; } = SButton.Down;
        public SButton ShiftUp { get; set; } = SButton.Up;
        public SButton ShiftRight { get; set; } = SButton.Right;
        public SButton ShiftLeft { get; set; } = SButton.Left;
        public SButton ModKey { get; set; } = SButton.LeftShift;
    }
}
