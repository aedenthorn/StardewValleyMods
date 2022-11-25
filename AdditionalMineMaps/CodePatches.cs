using HarmonyLib;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace AdditionalMineMaps
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.updateMap))]
        public class GameLocation_updateMap_Patch
        {
            public static void Prefix(GameLocation __instance)
            {
                if(!Config.ModEnabled || __instance is not MineShaft || mapList.Length == 0)
                    return;
                int level = (__instance as MineShaft).loadedMapNumber;
                if(level % 5 == 0)
                {
                    return;
                }
                int count = mapList.Length;
                if (Config.AllowVanillaMaps)
                {
                    if((__instance as MineShaft).getMineArea(-1) == 121)
                        count += 32;
                    else
                        count += 37;
                    if (Game1.random.Next(count) >= mapList.Length)
                        return;
                }
                string map = mapList[Game1.random.Next(mapList.Length)];
                SMonitor.Log($"Loading custom map {map}");
                __instance.mapPath.Value = map;
            }
        }
    }
}