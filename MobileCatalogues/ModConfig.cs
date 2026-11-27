using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MobileCatalogues
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool RequireCataloguePurchase { get; set; } = true;
        public Dictionary <string, int> CataloguePrices { get; set; }
        public int DefaultPrice { get; set; } = 10000;
        public int AppHeaderHeight { get; set; } = 32;
        public int AppRowHeight { get; set; } = 32;
        public Color BackgroundColor { get; set; } = Color.White;
        public Color HighlightColor { get; set; } = new Color(230,230,255);
        public Color GreyedColor { get; set; } = new Color(230, 230, 230);
        public Color HeaderColor { get; set; } = new Color(100, 100, 200);
        public Color TextColor { get; set; } = Color.Black;
        public Color HeaderTextColor { get; set; } = Color.White;
        public int MarginX { get; set; } = 4;
        public int MarginY { get; set; } = 4;
        public float HeaderTextScale { get; set; } = 0.5f;
        public float TextScale { get; set; } = 0.5f;
        public List<string> AllCatalogues { get; set; } = new();
        public List<string> ShownCatalogues { get; set; } = new();
        public List<string> HiddenCatalogues { get; set; } = new();
    }
}
