using StardewValley;

namespace UtilityGrid
{
    public class UtilityObjectInstance
    {
        public UtilityObjectInstance(UtilityObject template, Object obj)
        {
            Template = template;
            WorldObject = obj;
        }

        public float CurrentPowerDiff { get; set; }
        public UtilityObject Template { get; }
        public Object WorldObject { get; }
    }
}