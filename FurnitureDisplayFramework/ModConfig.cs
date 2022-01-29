
using StardewModdingAPI;

namespace FurnitureDisplayFramework
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;

        public SButton PlaceKey { get; set; } = SButton.MouseLeft;
        public SButton TakeKey { get; set; } = SButton.MouseRight;
    }
}
