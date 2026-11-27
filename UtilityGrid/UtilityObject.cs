using StardewValley;

namespace UtilityGrid
{
    public class UtilityObject
    {
        public float water;
        public float electric;
        public bool mustBeOn;
        public bool onlyMorning;
        public bool onlyDay;
        public bool onlyNight;
        public bool mustBeFull;
        public bool mustNeedOther;
        public string mustContain;
        public bool mustBeWorking;
        public bool onlyInWater;
        public bool mustHaveSun;
        public bool mustHaveRain;
        public bool mustHaveLightning;
        public int waterChargeCapacity;
        public int electricChargeCapacity;
        public float waterChargeRate;
        public float electricChargeRate;
        public float waterDischargeRate;
        public float electricDischargeRate;
        public bool fillWaterFromRain;
    }
}