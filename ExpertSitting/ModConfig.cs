namespace ExpertSitting
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool AllowMapSit { get; set; } = false;
        public string[] SeatTypes { get; set; } = {
            "Stone",
            "Log Section",
            "Ornamental Hay Bale",
        };
    }
}
