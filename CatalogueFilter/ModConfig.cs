using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace CatalogueFilter
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool ShowLabel { get; set; } = true;
        public Color LabelColor { get; set; } = Color.White; // Color for the text that says "Filter" in the shop menu
    }
}
