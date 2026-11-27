
using StardewModdingAPI;

namespace ModularBuildings
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public SButton ModKey { get; set; } = SButton.LeftShift;
        public SButton BuildKey { get; set; } = SButton.MouseLeft;
    }
}
