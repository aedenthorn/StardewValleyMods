using Microsoft.Xna.Framework;

namespace MobileCatalogues
{
    class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public Color BackgroundColor { get; set; } = new Color(41, 57, 106);
        public Color PostBackgroundColor { get; set; } = Color.White;
        public int PostMarginX { get; set; } = 16;
        public int PostMarginY { get;  set; } = 16;
        public int PostHeight { get; set; } = 128;
        public bool EnableCatalogue { get; set; } = true;
        public bool EnableFurnitureCatalogue { get; set; } = true;
        public bool FreeCatalogue { get; set; } = false;
        public bool FreeFurnitureCatalogue { get; set; } = false;
    }
}
