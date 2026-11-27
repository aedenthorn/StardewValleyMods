using Microsoft.Xna.Framework;

namespace MobileArcade
{
    class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public Color BackgroundColor { get; set; } = new Color(41, 57, 106);
        public Color PostBackgroundColor { get; set; } = Color.White;
        public int PostMarginX { get; set; } = 16;
        public int PostMarginY { get;  set; } = 16;
        public int PostHeight { get; set; } = 128;
    }
}
