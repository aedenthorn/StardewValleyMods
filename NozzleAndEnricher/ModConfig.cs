
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace NozzleAndEnricher
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public int NozzleBonus { get; set; } = 1;
        public SButton ModKey { get; set; } = SButton.LeftAlt;

    }
}
