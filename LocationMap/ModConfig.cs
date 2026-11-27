using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace LocationMap
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool ShowByDefault { get; set; } = false;
        public bool AllowTeleport { get; set; } = false;
        public KeybindList MapKey { get; set; } = new KeybindList(new Keybind(SButton.N));
        public float MapScale { get; set; } = 0.25f;
    }
}
