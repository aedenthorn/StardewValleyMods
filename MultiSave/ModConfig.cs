using StardewModdingAPI;

namespace MultiSave
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool AutoSaveDaily { get; set; } = true;
        public int AutoSaveOnDayOfWeek { get; set; } = 0;
        public int AutoSaveOnDayOfMonth { get; set; } = 0;
        public int MaxDaysOldToKeep { get; set; } = 7;
        public SButton SaveButton { get; set; } = SButton.None;
    }
}
