using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace ResourceStorage
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public string AutoStore { get; set; } = "92,330,388,390,709,771";
        public Keybind ModKeyMax = new Keybind(SButton.LeftShift);
    }
}
