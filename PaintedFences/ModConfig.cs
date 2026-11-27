using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace PaintedFences
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool Debug { get; set; } = false;
        public SButton ModKey { get; set; } = SButton.LeftShift;
        public SButton BulkModKey { get; set; } = SButton.LeftControl;
        public SButton DeleteKey { get; set; } = SButton.Delete;
    }
}
