
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace PlayingCards
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public SButton DealDownModButton { get; set; } = SButton.LeftShift;
        public SButton DealUpModButton { get; set; } = SButton.LeftAlt;
        public KeybindList CreateDeckButton { get; set; } = KeybindList.Parse("LeftShift + F5");
        public KeybindList ShuffleDeckButton { get; set; } = KeybindList.Parse("LeftShift + F6");

    }
}
