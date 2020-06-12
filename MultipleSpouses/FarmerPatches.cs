using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Reflection;

namespace MultipleSpouses
{
    public static class FarmerPatches
    {
        private static IMonitor Monitor;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }
        public static bool Farmer_doDivorce_Prefix(ref Farmer __instance)
        {
            try
            {
                __instance.divorceTonight.Value = false;
                if (!__instance.isMarried() || ModEntry.spouseToDivorce == null)
                {
                    ModEntry.PMonitor.Log("Tried to divorce but not married!");
                    return false;
                }

                string key = ModEntry.spouseToDivorce;


                ModEntry.PMonitor.Log($"Divorcing {key}");
                if (__instance.friendshipData.ContainsKey(key))
                {
                    if (ModEntry.config.FriendlyDivorce)
                    {
                        __instance.friendshipData[key].Points = Math.Max(2000, __instance.friendshipData[key].Points);
                        __instance.friendshipData[key].Status = FriendshipStatus.Friendly;
                    }
                    else
                    {
                        __instance.friendshipData[key].Points = 0;
                        __instance.friendshipData[key].Status = FriendshipStatus.Divorced;
                    }
                    __instance.friendshipData[key].RoommateMarriage = false;
                    NPC ex = Game1.getCharacterFromName(key);
                    ex.PerformDivorce();
                    ModEntry.ResetSpouses(__instance);
                    ModEntry.PHelper.Content.InvalidateCache("Maps/FarmHouse1_marriage");
                    ModEntry.PHelper.Content.InvalidateCache("Maps/FarmHouse2_marriage");
                    Utility.getHomeOfFarmer(__instance).resetForPlayerEntry();
                    Utility.getHomeOfFarmer(__instance).showSpouseRoom();
                    Game1.getFarm().addSpouseOutdoorArea(__instance.spouse == null ? "" : __instance.spouse);
                }

                ModEntry.spouseToDivorce = null;
                return false;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_doDivorce_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static bool GameLocation_performTouchAction_Prefix(GameLocation __instance, string fullActionString, Vector2 playerStandingPosition)
        {
            try
            {
                string[] acta = fullActionString.Split(' ');
                string text = acta[0];
                if (text == "Sleep" && Game1.timeOfDay == 600)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_performTouchAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

    }
}
