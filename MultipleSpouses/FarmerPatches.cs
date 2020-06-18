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
        private static IModHelper Helper;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
        }
        public static bool Farmer_doDivorce_Prefix(ref Farmer __instance)
        {
            try
            {
                __instance.divorceTonight.Value = false;
                if (!__instance.isMarried() || ModEntry.spouseToDivorce == null)
                {
                    Monitor.Log("Tried to divorce but no spouse to divorce!");
                    return false;
                }

                string key = ModEntry.spouseToDivorce;

                int points = 2000;
                if(ModEntry.divorceHeartsLost < 0)
                {
                    points = 0;
                }
                else
                {
                    points -= ModEntry.divorceHeartsLost * 250;
                }

                if (__instance.friendshipData.ContainsKey(key))
                {
                    Monitor.Log($"Divorcing {key}");
                    __instance.friendshipData[key].Points = Math.Min(2000, Math.Max(0,points));
                    Monitor.Log($"Resulting points: {__instance.friendshipData[key].Points}");

                    __instance.friendshipData[key].Status = points < 1000 ? FriendshipStatus.Divorced : FriendshipStatus.Friendly;
                    Monitor.Log($"Resulting friendship status: {__instance.friendshipData[key].Status}");

                    __instance.friendshipData[key].RoommateMarriage = false;

                    NPC ex = Game1.getCharacterFromName(key);
                    ex.PerformDivorce();
                    if(ModEntry.officialSpouse == key)
                    {
                        ModEntry.officialSpouse = null;
                    }
                    if(__instance.spouse == key)
                    {
                        __instance.spouse = null;
                    }
                    Misc.ResetSpouses(__instance);
                    Helper.Content.InvalidateCache("Maps/FarmHouse1_marriage");
                    Helper.Content.InvalidateCache("Maps/FarmHouse2_marriage");
                    Maps.BuildSpouseRooms(Utility.getHomeOfFarmer(Game1.player));
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

        public static bool Farmer_isMarried_Prefix(Farmer __instance, ref bool __result)
        {
            try
            {
                __result = __instance.team.IsMarried(__instance.UniqueMultiplayerID) || (ModEntry.spouses.Count > 0 || __instance.spouse != null);
                return false;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_isMarried_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
    }
}
