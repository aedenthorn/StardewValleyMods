
using StardewModdingAPI;

namespace FurnitureDisplayFramework
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;

        public SButton PlaceKey { get; set; } = SButton.MouseLeft;
        public SButton TakeKey { get; set; } = SButton.MouseRight;
        public SButton PlaceModKey { get; set; } = SButton.LeftShift;
        public SButton TakeModKey { get; set; } = SButton.LeftShift;
        public SButton PlaceAllOneModKey { get; set; } = SButton.LeftAlt;
        public bool PlaceAllByDefault { get; set; } = false;
    }
}
