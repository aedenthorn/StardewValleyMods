using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace WeddingTweaks
{
    public static class FarmerPatches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }

        public static bool Farmer_getChildren_Prefix(Farmer __instance, ref List<Child> __result)
        {
            try
            {

                if (ModEntry.startingLoadActors && Environment.StackTrace.Contains("command_loadActors") && !Environment.StackTrace.Contains("addActor") && !Environment.StackTrace.Contains("Dialogue") && !Environment.StackTrace.Contains("checkForSpecialCharacters") && Game1Patches.lastGotCharacter != null && __instance != null)
                {
                    __result = Utility.getHomeOfFarmer(__instance)?.getChildren()?.FindAll(c => c.displayName.EndsWith($"({Game1Patches.lastGotCharacter})")) ?? new List<Child>();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_getChildren_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
    }
}
