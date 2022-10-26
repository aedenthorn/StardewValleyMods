using Microsoft.Xna.Framework;

namespace Quotes
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool EnableApp { get; set; } = true;
        public bool ShowDailyQuote { get; set; } = true;
        public Color QuoteColor { get; set; } = Color.DarkSeaGreen;
        public int QuoteCharPerLine { get; set; } = 50;
        public int LineSpacing { get; set; } = 10;
        public float QuoteDurationPerLineMult { get; set; } = 1f;
        public int QuoteFadeMult { get; set; } = 1;
        public string AuthorPrefix { get; set; } = "-- ";
        public bool ClickToDispelQuote { get; set; } = true;
        public bool RandomQuote { get; set; } = false;
        public int QuoteWidth { get; set; } = 1000;
    }
}
