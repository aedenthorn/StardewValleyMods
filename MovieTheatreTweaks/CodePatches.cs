using HarmonyLib;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using System;
using xTile.Dimensions;

namespace MovieTheatreTweaks
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.answerDialogueAction))]
        public class GameLocation_answerDialogueAction_Patch
        {
            public static bool Prefix(GameLocation __instance, string questionAndAnswer)
            {
                if (!Config.ModEnabled || questionAndAnswer != "WatchMovieSpendTicket_Yes")
                    return true;

                __instance.performAction("Theatre_Doors_Yes", Game1.player, new Location(0,0));

                return false;
            }
        }

        [HarmonyPatch(typeof(MovieTheater), "addSpecificRandomNPC")]
        public class MovieTheater_addSpecificRandomNPC_Patch
        {
            public static bool Prefix(int whichRandomNPC)
            {
                if (!Config.ModEnabled || whichRandomNPC != 0 || !Config.RemoveCraneMan)
                    return true;
                return false;
            }
        }     
         
        [HarmonyPatch(typeof(MovieTheater), nameof(MovieTheater.performAction))]
        public class MovieTheater_performAction_Patch
        {
            public static bool Prefix(MovieTheater __instance, NetInt ____currentState, ref string action, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                if (action == "Theater_Doors")
                {
                    __result = true;
                    ShowMovieDialogue(__instance, who);
                    return false;
                }
                if (action == "Theatre_Doors_Yes")
                {
                    if(____currentState.Value == 0)
                    {
                        Game1.player.removeItemsFromInventory(809, 1);
                    }
                    action = "Theater_Doors";
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performAction))]
        public class GameLocation_performAction_Patch
        {
            public static bool Prefix(GameLocation __instance, ref string action, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                string[] actionParams = action.Split(' ', StringSplitOptions.None);
                string text = actionParams[0];
                if (text == "Theater_Entrance"
                    && !Game1.isFestival()
                    && Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater")
                    && Game1.timeOfDay <= Config.CloseTime
                    && Game1.timeOfDay >= Config.OpenTime)
                {
                    __result = true;
                    EnterTheatre(__instance);
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(MovieTheater), "_Leave")]
        public class MovieTheater__Leave_Patch
        {
            public static void Postfix()
            {
                if (!Config.ModEnabled)
                    return;
                Game1.player.lastSeenMovieWeek.Value = -1;
            }
        }

    }
}