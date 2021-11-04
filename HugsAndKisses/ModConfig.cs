
namespace HugsAndKisses
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;

        public bool RoommateKisses { get; set; } = true;
        public int MinHeartsForKiss { get; set; } = 9;
        public bool UnlimitedDailyKisses { get; set; } = true;
        public bool AllowPlayerSpousesToKiss { get; set; } = true;
        public float SpouseKissChance0to1 { get; set; } = 0.1f;
        public string CustomKissSound { get; set; } = "";
        public string CustomHugSound { get; set; } = "";
        public string CustomKissFrames { get; set; } = "Sam:36,Penny:35";
        public float MaxDistanceToKiss { get; set; } = 200f;
        public double MinSpouseKissIntervalSeconds { get; set; } = 10;
        public bool AllowNPCRelativesToHug { get; set; } = true;
        public bool AllowNPCSpousesToKiss { get; set; } = true;
        public bool AllowRelativesToKiss { get; set; } = false;
        public bool AllowNonDateableNPCsToHugAndKiss { get; set; } = false;

        //public bool RemoveSpouseOrdinaryDialogue { get; set; } = false;
    }
}
