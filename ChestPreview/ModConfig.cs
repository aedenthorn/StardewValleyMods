using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace ChestPreview
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool ShowWhenFacing { get; set; } = true;
        public bool ShowWhenHover { get; set; } = true;
        public SButton ModKey { get; set; } = SButton.LeftAlt;
    }
}
