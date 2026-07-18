
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace ChestSigns
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public SButton ModKey { get; set; } = SButton.LeftAlt;

    }
}
