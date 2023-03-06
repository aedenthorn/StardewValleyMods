using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace SeedInfo
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int DaysPerMonth { get; set; } = 28;
        public Color PriceColor { get; set; } = new Color(100, 25, 25);
    }
}
