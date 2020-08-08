namespace UndergroundSecrets
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public float CollapsedFloorMaxPortion { get; set; } = 0.05f;
        public float TilePuzzleChance { get; set; } = 1f;
        public float TrapsMaxPortion { get; set; } = 0.01f;
        public float MushroomTreesMaxPortion { get; set; } = 0.01f;
        public double OfferingPuzzleChance { get; set; } = 0.05f;
    }
}
