using HarmonyLib;
using StardewValley;
using StardewValley.Objects;

namespace FurniturePlacementTweaks
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Furniture), nameof(Furniture.canBeRemoved))]
        public class Furniture_canBeRemoved_Patch
        {
            public static bool Prefix(Furniture __instance, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || Config.AllowPickupIfUnder || __instance.furniture_type.Value != 12)
                    return true;
                foreach(Furniture f in who.currentLocation.furniture)
                {
                    if (f != __instance && f.furniture_type.Value != 12 && f.boundingBox.Value.Intersects(__instance.boundingBox.Value))
                    {
                        __result = false;
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Furniture), nameof(Furniture.AllowPlacementOnThisTile))]
        public class Furniture_AllowPlacementOnThisTile_Patch
        {
            public static bool Prefix(Furniture __instance, ref bool __result)
            {
                if (!Config.ModEnabled || !Config.AllowOverlapPlacement)
                    return true;
                __result = true;
                return false;
            }
        }
    }
}