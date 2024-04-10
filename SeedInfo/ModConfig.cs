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
        public bool DisplayMead { get; set; } = true;
        public bool DisplayCrop {  get; set; } = true;
        public bool DisplayPickle { get; set; } = true;
        public bool DisplayKeg { get; set; } = true;
        public bool DisplayDehydrator { get; set; } = true;
        public bool DivideDehydrate {  get; set; } = false;
    }
}
