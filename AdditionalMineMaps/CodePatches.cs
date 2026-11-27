using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Netcode;
using StardewValley;
using StardewValley.Locations;

namespace AdditionalMineMaps
{
	public partial class ModEntry
	{
		public class MineShaft_loadLevel_Patch
		{
			public static void Prefix(MineShaft __instance)
			{
				__instance.modData.Remove(mapPathKey);
				__instance.modData.Remove(mapTypeKey);
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
				var randomMap = list[Game1.random.Next(list.Count)];

                string map = randomMap.mapPath;
				__instance.modData[mapPathKey] = map;
				__instance.modData[mapTypeKey] = randomMap.mapType;
			}
		}

		public class GameLocation_updateMap_Patch
		{
			public static void Prefix(GameLocation __instance)
			{
				if (!Config.ModEnabled || __instance is not MineShaft ms || !__instance.modData.TryGetValue(mapPathKey, out string mapPath))
					return;
				if(__instance.mapPath.Value != mapPath)
					__instance.mapPath.Value = mapPath;
				if (__instance.modData[mapTypeKey] == "T")
					AccessTools.FieldRefAccess<MineShaft, NetBool>(ms, "netIsTreasureRoom").Value = true;
			}
		}
		public class MineShaft_isQuarryArea_Get_Patch
		{
			public static bool Prefix(MineShaft __instance, ref bool __result)
			{
				if (!Config.ModEnabled || __instance is not MineShaft ms || !__instance.modData.TryGetValue(mapTypeKey, out string mapType) || mapType != "Q")
					return true;
				__result = true;
				return false;
			}
		}
		public class MineShaft_isDinoArea_Get_Patch
        {
			public static bool Prefix(MineShaft __instance, ref bool __result)
			{
				if (!Config.ModEnabled || __instance is not MineShaft ms || !__instance.modData.TryGetValue(mapTypeKey, out string mapType) || mapType != "D")
					return true;
				__result = true;
				return false;
			}
		}
		public class MineShaft_isMonsterArea_Get_Patch
        {
			public static bool Prefix(MineShaft __instance, ref bool __result)
			{
				if (!Config.ModEnabled || __instance is not MineShaft ms || !__instance.modData.TryGetValue(mapTypeKey, out string mapType) || mapType != "M")
					return true;
				__result = true;
				return false;
			}
		}
		public class MineShaft_isSlimeArea_Get_Patch
        {
			public static bool Prefix(MineShaft __instance, ref bool __result)
			{
				if (!Config.ModEnabled || __instance is not MineShaft ms || !__instance.modData.TryGetValue(mapTypeKey, out string mapType) || mapType != "S")
					return true;
				__result = true;
				return false;
			}
		}
	}
}
