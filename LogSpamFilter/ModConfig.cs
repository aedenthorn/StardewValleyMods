namespace LogSpamFilter
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool IsDebug { get; set; } = false;
        public string AllowList { get; set; } = "Log Spam Filter";
        public int MSSpawnThrottle { get; set; } = 1000;
        public int MSBetweenMessages { get; set; } = 0;
        public int MSBetweenIdenticalMessages { get; set; } = 1000;
        public int MSBetweenSimilarMessages { get; set; } = 0;
        public int PercentSimilarity { get; set; } = 80;

    }
}
