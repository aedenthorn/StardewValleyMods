using StardewModdingAPI;

namespace StackedItemIcons
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int MinForDoubleStack { get; set; } = 10;
        public int MinForTripleStack { get; set; } = 100;
        public int Spacing { get; set; } = 10;
    }
}
