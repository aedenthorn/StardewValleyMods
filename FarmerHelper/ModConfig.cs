
using StardewModdingAPI;

namespace FarmerHelper
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public string IgnoreHarvestCrops { get; set; } = "";
        public bool IgnoreFlowers { get; set; } = true;
        public bool LabelLatePlanting { get; set; } = true;
        public bool PreventLatePlant { get; set; } = true;
        public bool WarnAboutPlantsUnwateredBeforeSleep { get; set; } = true;
        public bool WarnAboutPlantsUnharvestedBeforeSleep { get; set; } = true;
        public bool WarnAboutAnimalsOutsideBeforeSleep { get; set; } = true;
        public bool WarnAboutAnimalsHungryBeforeSleep { get; set; } = true;
        public bool WarnAboutAnimalsUnharvestedBeforeSleep { get; set; } = true;
        public bool WarnAboutAnimalsNotPetBeforeSleep { get; set; } = true;
        public int DaysPerMonth { get; set; } = 28;

    }
}
