using StardewModdingAPI;
using System.Collections.Generic;

namespace CustomPaintings
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public Dictionary<int, string> PaintingPaths { get; set; } = new Dictionary<int, string>();
        public SButton ModKey { get; set; } = SButton.LeftAlt;
    }
}
