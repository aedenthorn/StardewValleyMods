
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ImportMap
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public KeybindList ImportKey { get; set; } = KeybindList.Parse("LeftShift + F12");

    }
}
