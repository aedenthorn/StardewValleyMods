using StardewModdingAPI;

namespace SimpleBackup
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool AutoSaveDaily { get; set; } = false;
        public int AutoSaveOnDayOfWeek { get; set; } = 7;
        public int AutoSaveOnDayOfMonth { get; set; } = 1;
        public int MaxDaysOldToKeep { get; set; } = 1;
        public SButton SaveButton { get; set; } = SButton.None;
    }
}
