using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace SDVExplorer
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public KeybindList MenuKeys { get; set; } = KeybindList.Parse("LeftShift + F12");
    }
}
