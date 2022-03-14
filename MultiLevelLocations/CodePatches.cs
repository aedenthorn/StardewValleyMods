using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using System.Text.RegularExpressions;

namespace MultiLevelLocations
{
    public partial class ModEntry
    {
        public static void GameLocation_updateMap_Prefix(GameLocation __instance)
        {
            if (!Config.EnableMod || __instance.mapPath.Value == null || __instance != Game1.player.currentLocation)
                return;

            string newMapPath = Regex.Replace(__instance.mapPath.Value, "_[0-9]+$", "") + (lastPlayerLevel > 1 ? "_" + lastPlayerLevel : "");
            __instance.mapPath.Value = newMapPath;

        }
    }
}