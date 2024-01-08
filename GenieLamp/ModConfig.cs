using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace GenieLamp
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public string LampItem { get; set; } = "Golden Mask";
        public string MenuSound { get; set; } = "cowboy_explosion";
        public string WishSound { get; set; } = "yoba";
        public int WishesPerItem { get; set; } = 3;
    }
}
