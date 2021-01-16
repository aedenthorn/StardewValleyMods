using StardewModdingAPI;

namespace ExpertSitting
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool AllowMapSit { get; set; } = true;
        public string[] SeatTypes { get; set; } = {
            "Stone",
            "Log Section",
            "Ornamental Hay Bale",
        };
        public SButton MapSitModKey { get; set; } = SButton.LeftShift;
    }
}
