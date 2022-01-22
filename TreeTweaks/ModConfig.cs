namespace LogSpamFilter
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public float SizeIncreasePerDay { get; set; } = 0.1f;
        public int MaxDaysSizeIncrease { get; set; } = 100;
        public float LootIncreasePerDay { get; set; } = 0.1f;

    }
}
