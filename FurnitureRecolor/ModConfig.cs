
using StardewModdingAPI;

namespace FurnitureRecolor
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;

        public SButton RedButton { get; set; } = SButton.NumPad1;
        public SButton GreenButton { get; set; } = SButton.NumPad7;
        public SButton BlueButton { get; set; } = SButton.NumPad9;
        public SButton ResetButton { get; set; } = SButton.NumPad3;

        public SButton ModKey { get; set; } = SButton.LeftAlt;
    }
}
