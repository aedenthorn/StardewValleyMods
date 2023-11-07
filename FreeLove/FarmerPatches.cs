using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeLove
{
    public static class FarmerPatches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
        }
        public static bool Farmer_doDivorce_Prefix(ref Farmer __instance)
        {
            try
            {
                Monitor.Log("Trying to divorce");
                __instance.divorceTonight.Value = false;
                if (!__instance.isMarriedOrRoommates() || ModEntry.spouseToDivorce == null)
                {
                    Monitor.Log("Tried to divorce but no spouse to divorce!");
                    return false;
                }

                string key = ModEntry.spouseToDivorce;

                int points = 2000;
                if (ModEntry.divorceHeartsLost < 0)
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
                    __instance.friendshipData[key].Points = Math.Min(2000, Math.Max(0, points));
                    Monitor.Log($"Resulting points: {__instance.friendshipData[key].Points}");

                    __instance.friendshipData[key].Status = points < 1000 ? FriendshipStatus.Divorced : FriendshipStatus.Friendly;
                    Monitor.Log($"Resulting friendship status: {__instance.friendshipData[key].Status}");

                    __instance.friendshipData[key].RoommateMarriage = false;

                    NPC ex = Game1.getCharacterFromName(key);
                    ex.PerformDivorce();
                    if (__instance.spouse == key)
                    {
                        __instance.spouse = null;
                    }
                    ModEntry.currentSpouses.Remove(__instance.UniqueMultiplayerID);
                    ModEntry.currentUnofficialSpouses.Remove(__instance.UniqueMultiplayerID);
                    ModEntry.ResetSpouses(__instance);
                    Helper.GameContent.InvalidateCache("Maps/FarmHouse1_marriage");
                    Helper.GameContent.InvalidateCache("Maps/FarmHouse2_marriage");

                    Monitor.Log($"New spouse: {__instance.spouse}, married {__instance.isMarriedOrRoommates()}");

                    Utility.getHomeOfFarmer(__instance).showSpouseRoom();
                    Utility.getHomeOfFarmer(__instance).setWallpapers();
                    Utility.getHomeOfFarmer(__instance).setFloors();

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
                __result = __instance.team.IsMarried(__instance.UniqueMultiplayerID) || ModEntry.GetSpouses(__instance, true).Count > 0;
                return false;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_isMarried_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static bool Farmer_checkAction_Prefix(Farmer __instance, Farmer who, GameLocation location, ref bool __result)
        {
            try
            {
                if (who.isRidingHorse())
                {
                    who.Halt();
                }
                if (__instance.hidden.Value)
                {
                    return true;
                }
                if (Game1.CurrentEvent == null && who.CurrentItem != null && who.CurrentItem.ParentSheetIndex == 801 && !__instance.isEngaged() && !who.isEngaged()) { 
                    who.Halt();
                    who.faceGeneralDirection(__instance.getStandingPosition(), 0, false);
                    string question2 = Game1.content.LoadString("Strings\\UI:AskToMarry_" + (__instance.IsMale ? "Male" : "Female"), __instance.Name);
                    location.createQuestionDialogue(question2, location.createYesNoResponses(), delegate (Farmer _, string answer)
                    {
                        if (answer == "Yes")
                        {
                            who.team.SendProposal(__instance, ProposalType.Marriage, who.CurrentItem.getOne());
                            Game1.activeClickableMenu = new PendingProposalDialog();
                        }
                    }, null);
                    __result = true;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_checkAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
        public static bool skipSpouse = false;
        public static void Farmer_spouse_Postfix(Farmer __instance, ref string __result)
        {
            if (skipSpouse)
                return;
            try
            {
                skipSpouse = true;
                if (ModEntry.tempOfficialSpouse != null && __instance.friendshipData.ContainsKey(ModEntry.tempOfficialSpouse.Name) && __instance.friendshipData[ModEntry.tempOfficialSpouse.Name].IsMarried())
                {
                    __result = ModEntry.tempOfficialSpouse.Name;
                }
                else
                {
                    var spouses = ModEntry.GetSpouses(__instance, true);
                    string aspouse = null;
                    foreach(var spouse in spouses)
                    {
                        if (aspouse is null)
                            aspouse = spouse.Key;
                        if (__instance.friendshipData.TryGetValue(spouse.Key, out var f) && f.IsEngaged())
                        {
                            __result = spouse.Key;
                            break;
                        }
                    }
                    if(__result is null && aspouse is not null)
                    {
                        __result = aspouse;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_spouse_Postfix)}:\n{ex}", LogLevel.Error);
            }
            skipSpouse = false;
        }

        public static void Farmer_getSpouse_Postfix(Farmer __instance, ref NPC __result)
        {
            try
            {

                if (ModEntry.tempOfficialSpouse != null && __instance.friendshipData.ContainsKey(ModEntry.tempOfficialSpouse.Name) && __instance.friendshipData[ModEntry.tempOfficialSpouse.Name].IsMarried())
                {
                    __result = ModEntry.tempOfficialSpouse;
                }
                else
                {
                    var spouses = ModEntry.GetSpouses(__instance, true);
                    NPC aspouse = null;
                    foreach (var spouse in spouses)
                    {
                        if (aspouse is null)
                            aspouse = spouse.Value;
                        if (__instance.friendshipData[spouse.Key].IsEngaged())
                        {
                            __result = spouse.Value;
                            break;
                        }
                    }
                    if (__result is null && aspouse is not null)
                    {
                        __result = aspouse;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_getSpouse_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static bool Farmer_GetSpouseFriendship_Prefix(Farmer __instance, ref Friendship __result)
        {
            try
            {

                if (ModEntry.tempOfficialSpouse != null && __instance.friendshipData.ContainsKey(ModEntry.tempOfficialSpouse.Name) && __instance.friendshipData[ModEntry.tempOfficialSpouse.Name].IsMarried())
                {
                    __result = __instance.friendshipData[ModEntry.tempOfficialSpouse.Name];
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_GetSpouseFriendship_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
        internal static bool Farmer_getChildren_Prefix(Farmer __instance, ref List<Child> __result)
        {
            try
            {

                if (EventPatches.startingLoadActors && Environment.StackTrace.Contains("command_loadActors") && !Environment.StackTrace.Contains("addActor") && !Environment.StackTrace.Contains("Dialogue") && !Environment.StackTrace.Contains("checkForSpecialCharacters") && Game1Patches.lastGotCharacter != null && __instance != null)
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
