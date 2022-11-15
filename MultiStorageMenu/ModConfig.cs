using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace MultiStorageMenu
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int ChestRows { get; set; } = 3;
        public SButton ModKey { get; set; } = SButton.LeftShift;
        public SButton ModKey2 { get; set; } = SButton.LeftControl;
        public SButton MenuKey { get; set; } = SButton.F2;

    }
}
