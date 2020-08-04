using Microsoft.Xna.Framework;

namespace MobileTelevision
{
    class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool EnableCatalogue { get; set; } = true;
        public bool EnableFurnitureCatalogue { get; set; } = true;
        public bool EnableSeedCatalogue { get; set; } = true;
        public bool FreeCatalogue { get; set; } = false;
        public bool FreeFurnitureCatalogue { get; set; } = false;
        public bool FreeSeedCatalogue { get; set; } = false;
        public string SeedsToInclude { get; set; } = "season";
    }
}
