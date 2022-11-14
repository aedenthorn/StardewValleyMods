using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace MultiStorageMenu
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public SButton ModKey { get; set; } = SButton.LeftShift;
        public SButton MenuKey { get; set; } = SButton.F2;

    }
}
