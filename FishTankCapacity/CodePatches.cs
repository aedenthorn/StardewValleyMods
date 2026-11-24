using HarmonyLib;
using StardewValley.Objects;
using System;

namespace FishTankCapacity
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(FishTankFurniture), nameof(FishTankFurniture.GetCapacityForCategory))]
        public static class FishTankFurniture_GetCapacityForCategory_Patch
        {
            public static void Postfix(FishTankFurniture __instance, FishTankFurniture.FishTankCategories category, ref int __result)
            {
                if(!Config.ModEnabled)
                    return;
                if (Config.UnlimitedCapacity)
                {
                    __result = int.MaxValue;
                }
                else if(Config.CustomCapacities.TryGetValue(__instance.ItemId, out var cap))
                {
                    __result = cap;
                }
                else
                {
                    __result = (int)Math.Round(__result * Config.CapacityMultiplier);
                }
            }
        }
    }
}