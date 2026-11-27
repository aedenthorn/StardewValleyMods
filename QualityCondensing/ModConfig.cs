using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace QualityCondensing
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public KeybindList CondenseButton { get; set; } = new (SButton.MouseMiddle);
        public int ToSilver { get; set; } = 3;
        public int ToGold { get; set; } = 3;
        public int ToIridium { get; set; } = 3;
        public string CondenseSound { get; set; } = "coin";
    }
}
