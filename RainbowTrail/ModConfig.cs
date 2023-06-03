using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace RainbowTrail
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public KeybindList ToggleKey { get; set; } = new KeybindList(SButton.NumPad1);
        public string EnableSound { get; set; } = "yoba";
        public int MaxLength { get; set; } = 50;
        public int MoveSpeed { get; set; } = 10;
        public int StaminaUse { get; set; } = 0;
    }
}
