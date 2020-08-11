using Microsoft.Xna.Framework;

namespace MobileCatalogues
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool EnableCatalogue { get; set; } = true;
        public bool EnableFurnitureCatalogue { get; set; } = true;
        public bool EnableSeedCatalogue { get; set; } = true;
        public bool EnableTravelingCatalogue { get; set; } = true;
        public bool LimitTravelingCatalogToInTown { get; set; } = true;
        public bool EnableDesertCatalogue { get; set; } = true;
        public bool LimitDesertCatalogToBusFixed { get; set; } = true;
        public bool EnableHatCatalogue { get; set; } = true;
        public bool RequireCataloguePurchase { get; set; } = true;
        public int PriceCatalogue { get; set; } = 30000;
        public int PriceFurnitureCatalogue { get; set; } = 100000;
        public int PriceSeedCatalogue { get; set; } = 300000;
        public int PriceTravelingCatalogue { get; set; } = 500000;
        public int PriceDesertCatalogue { get; set; } = 500000;
        public int PriceHatCatalogue { get; set; } = 10000;
        public bool FreeCatalogue { get; set; } = false;
        public bool FreeFurnitureCatalogue { get; set; } = false;
        public bool FreeSeedCatalogue { get; set; } = false;
        public bool FreeDesertCatalogue { get; set; } = false;
        public bool FreeTravelingCatalogue { get; set; } = false;
        public bool FreeHatCatalogue { get; set; } = false;
        public string SeedsToInclude { get; set; } = "season";
        public float PriceMult { get; set; } = 1f;
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
    }
}
