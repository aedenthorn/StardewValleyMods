using StardewModdingAPI;
using System.Collections.Generic;

namespace MusicalPaths
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public SButton ModKey { get; set; } = SButton.LeftAlt;
        public bool ConsumeBlock { get; set; } = true;
        public bool ShowBlockOutLine { get; set; } = true;
        public float BlockOutLineOpacity { get; set; } = 0.5f;
    }
}
