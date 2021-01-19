namespace OverworldChests
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool AllowIndoorSpawns { get; set; } = true;
        public string OnlyAllowLocations { get; set; } = "";
        public string DisallowLocations { get; set; } = "";
        public int RespawnInterval { get; set; } = 7;
        public float ChestDensity { get; set; } = 0.001f;
        public int ChestsExtentedMaxLevel { get; set; } = 20;
        public float ChestsExtendedRarity { get; set; } = 0.1f;
    }
}
