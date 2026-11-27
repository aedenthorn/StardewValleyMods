
namespace MultipleSpouses
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool BuyPendantsAnytime { get; set; } = false;
        public int MinPointsToMarry { get; set; } = 2500;
        public int MinPointsToDate { get; set; } = 2000;
        public int DaysUntilMarriage { get; set; } = 3;
        public bool AllSpousesWearMarriageClothesAtWeddings { get; set; } = false;
        public bool AllSpousesJoinWeddings { get; set; } = true;
        public bool FriendlyDivorce { get; set; } = true;
        public bool ComplexDivorce { get; set; } = true;
        public bool PreventHostileDivorces { get; set; } = false;
        public bool RoommateRomance { get; set; } = true;
        public bool RomanceAllVillagers { get; set; } = false;

        public int PercentChanceForSpouseInBed { get; set; } = 25;
        public int PercentChanceForSpouseInKitchen { get; set; } = 25;
        public int PercentChanceForSpouseAtPatio { get; set; } = 25;
        public int MaxGiftsPerDay { get; set; } = 1;
        //public bool RemoveSpouseOrdinaryDialogue { get; set; } = false;
        public int MinHeartsForKiss { get; set; } = 9;
        public bool UnlimitedDailyKisses { get; set; } = true;
        public bool AllowSpousesToKiss { get; set; } = true;
        public float SpouseKissChance { get; set; } = 0.1f;
        public bool RealKissSound { get; set; } = true;
        public float MaxDistanceToKiss { get; set; } = 200f;
        public double MinSpouseKissInterval { get; set; } = 20;
        public bool PreventRelativesFromKissing { get; set; } = true;

        public double BabyRequestChance { get; set; } = 0.05f;
        public bool AllowGayPregnancies { get; set; } = true;
        public float FemaleBabyChance { get; set; } = 0.5f;
        public int PregnancyDays { get; set; } = 14;
        public int MaxChildren { get; set; } = 2;
        public bool ChildrenHaveHairOfSpouse { get; set; } = true;
        public int ChildGrowthMultiplier { get; set; } = 1;
        public bool ShowParentNames { get; set; } = false;

        public bool DisableCustomSpousesRooms { get; set; } = false;
        public bool BuildAllSpousesRooms { get; set; } = true;
        public string SpouseRoomOrder { get; set; } = "";
        public string SpouseSleepOrder { get; set; } = "";
        public int ExistingSpouseRoomOffsetX { get; set; } = 0;
        public int ExistingSpouseRoomOffsetY { get; set; } = 0;
        public bool CustomBed { get; set; } = true;
        public int BedWidth { get; set; } = 5;
        public float SheetTransparency { get; set; } = 1f;
        public int ExtraKidsRoomWidth { get; set; } = 0;
        public int ExtraCribs { get; set; } = 0;
    }
}
