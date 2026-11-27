namespace FruitTreeShaker
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int DaysRipeBeforeFalling { get; set; } = 5;
        public bool RandomDrops { get; set; } = false;
        public int MaxDaysRipeBeforeFalling { get; set; } = 8;
        public bool DropAllInStorm { get; set; } = true;
        public bool FarmOnly { get; set; } = true;
        public bool ShakePalmTrees { get; set; } = false;
        public bool ShakeNormalTrees { get; set; } = false;
    }
}
