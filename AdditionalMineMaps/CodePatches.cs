using HarmonyLib;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AdditionalMineMaps
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(MineShaft), nameof(MineShaft.loadLevel))]
        public class MineShaft_loadLevel_Patch
        {
            public static void Prefix(MineShaft __instance)
            {
                __instance.modData.Remove(mapPathKey);
                if (!Config.ModEnabled || mapDict.Count == 0)
                    return;
                var level = __instance.mineLevel;
                if (level % (__instance.getMineArea(-1) == 121 ? 5 : 10) == 0)
                {
                    return;
                }
                var list = new List<MapData>();
                foreach(var data in mapDict.Values)
                {
                    if(data.minLevel <= level && (data.maxLevel >= level || data.maxLevel < 0))
                    {
                        list.Add(data);
                    }
                }
                if (!list.Any())
                    return;
                var count = list.Count;
                if (Config.AllowVanillaMaps)
                {
                    if(__instance.getMineArea(-1) == 121)
                        count += 32;
                    else
                        count += 37;
                    if (Game1.random.Next(count) >= mapDict.Count)
                        return;
                }
                string map = list[Game1.random.Next(list.Count)].mapPath;
                __instance.modData[mapPathKey] = map;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.updateMap))]
        public class GameLocation_updateMap_Patch
        {
            public static void Prefix(GameLocation __instance)
            {
                if (!Config.ModEnabled || __instance is not MineShaft || !__instance.modData.TryGetValue(mapPathKey, out string mapPath) || __instance.mapPath.Value == mapPath)
                    return;
                __instance.mapPath.Value = mapPath;
            }
        }
    }
}