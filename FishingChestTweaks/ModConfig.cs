namespace FishingChestTweaks
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool AutoCollect { get; set; } = true;
        public bool DropIfFull { get; set; } = false;
        public bool NoReduction { get; set; } = true;
        public bool ChestWithoutFish { get; set; } = true;
        public string ChestMovement { get; set; } = "Random";
        public string GoldenChestMovement { get; set; } = "Legend";
    }
}
