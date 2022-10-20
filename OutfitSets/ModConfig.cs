using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace OutfitSets
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int Sets { get; set; } = 6;
        public Color CurrentColor { get; set; } = Color.Brown;
        public Color DefaultColor { get; set; } = Color.White;
    }
}
