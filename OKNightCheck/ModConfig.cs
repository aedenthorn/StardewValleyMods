using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace OKNightCheck
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool ShowQuote { get; set; } = true;
        public bool ShowQuitButton { get; set; } = true;
        public Color QuoteColor { get; set; } = Color.DarkSeaGreen;
        public int QuoteCharPerLine { get; set; } = 50;
        public int LineSpacing { get; set; } = 10;
        public string AuthorPrefix { get; set; } = "-- ";
    }
}
