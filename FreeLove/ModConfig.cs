
namespace FreeLove
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool BuyPendantsAnytime { get; set; } = false;
        public int MinPointsToMarry { get; set; } = 2500;
        public int MinPointsToDate { get; set; } = 2000;
        public int DaysUntilMarriage { get; set; } = 3;
        public bool PreventHostileDivorces { get; set; } = true;
        public bool ComplexDivorce { get; set; } = true;
        public bool RoommateRomance { get; set; } = true;
        public bool RomanceAllVillagers { get; set; } = false;
        public bool ShowParentNames { get; set; } = false;
        public string SpouseSleepOrder { get; set; } = "";
        public int PercentChanceForSpouseInBed { get; set; } = 25;
        public int PercentChanceForSpouseInKitchen { get; set; } = 25;
        public int PercentChanceForSpouseAtPatio { get; set; } = 25;


        //public bool RemoveSpouseOrdinaryDialogue { get; set; } = false;
    }
}
