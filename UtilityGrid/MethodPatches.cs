using Microsoft.Xna.Framework;
using StardewValley;

namespace UtilityGrid
{
    public partial class ModEntry
    {
        // dga_add "aedenthorn.WaterPumpDGA/Bronze Water Pump" 1
        public static bool Utility_playerCanPlaceItemHere_Prefix(GameLocation location, Item item, int x, int y, ref bool __result)
        {
            //SMonitor.Log($"placing {item.Name}, {objectDict.ContainsKey(item.Name)}");
            if (!Config.EnableMod || !objectDict.ContainsKey(item.Name) || !(location is Farm) || !objectDict[item.Name].onlyInWater || location.Objects.ContainsKey(new Vector2(x, y)))
                return true;
            __result = location.isWaterTile(x / 64, y / 64);
            return false;
        }
        public static void Object_IsSprinkler_Postfix(Object __instance, ref bool __result)
        {
            //SMonitor.Log($"placing {item.Name}, {objectDict.ContainsKey(item.Name)}");
            if (!Config.EnableMod || !__result || !objectDict.ContainsKey(__instance.Name))
                return;
            __result = IsObjectPowered(__instance.TileLocation, objectDict[__instance.Name]);

        }
        public static void Object_updateWhenCurrentLocation_Prefix(Object __instance)
        {
            //SMonitor.Log($"placing {item.Name}, {objectDict.ContainsKey(item.Name)}");
            if (!Config.EnableMod || !objectDict.ContainsKey(__instance.Name))
                return;
            __instance.IsOn = IsObjectPowered(__instance.TileLocation, objectDict[__instance.Name]);

        }
        public static Object preItem = null;
        public static int minutesUntilReady = 0;
        public static void Object_DayUpdate_Prefix(Object __instance, ref bool __state)
        {
            //SMonitor.Log($"placing {item.Name}, {objectDict.ContainsKey(item.Name)}");
            if (!Config.EnableMod || !objectDict.ContainsKey(__instance.Name))
                return;
            if(!IsObjectPowered(__instance.TileLocation, objectDict[__instance.Name]))
            {
                preItem = __instance.heldObject.Value;
                minutesUntilReady = __instance.MinutesUntilReady;
                __instance.heldObject.Value = null;
                __state = true;
            }

        }
        public static void Object_DayUpdate_Postfix(Object __instance, bool __state)
        {
            //SMonitor.Log($"placing {item.Name}, {objectDict.ContainsKey(item.Name)}");
            if (!__state)
                return;
            __instance.MinutesUntilReady = minutesUntilReady;
            __instance.heldObject.Value = preItem;
        }
    }
}