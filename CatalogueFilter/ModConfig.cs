using StardewModdingAPI;
using System.Collections.Generic;

namespace CatalogueFilter
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool ShowLabel { get; set; } = true;
        public int LabelColor { get; set; } = 4;
    }
}
