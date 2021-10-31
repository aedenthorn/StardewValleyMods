using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FreeLove
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
        public static bool Farmer_isMarried_Prefix(Farmer __instance, ref bool __result)
        {
            try
            {
                __result = __instance.team.IsMarried(__instance.UniqueMultiplayerID) || Misc.GetSpouses(__instance, 1).Count > 0;
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

        internal static bool Farmer_getSpouse_Prefix(Farmer __instance, ref NPC __result)
        {
            try
            {
                if (ModEntry.tempOfficialSpouse != null && __instance.friendshipData.ContainsKey(ModEntry.tempOfficialSpouse.Name) && __instance.friendshipData[ModEntry.tempOfficialSpouse.Name].IsMarried())
                {
                    __result = ModEntry.tempOfficialSpouse;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_getSpouse_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        internal static bool Farmer_GetSpouseFriendship_Prefix(Farmer __instance, ref Friendship __result)
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
                Monitor.Log($"Failed in {nameof(Farmer_getSpouse_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
    }
}
