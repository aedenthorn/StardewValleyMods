using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using System;
using StardewValley.Quests;
using Object = StardewValley.Object;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;

namespace WeddingTweaks
{
    public partial class ModEntry
    {
        //[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.resetForPlayerEntry))]
        public class GameLocation_resetForPlayerEntry_Patch
        {
            public static void Prefix(GameLocation __instance)
            {
                if (!Config.EnableMod || !Config.FixWeddingStart)
                    return;
                if (!Game1.eventUp && Game1.weddingsToday.Count > 0 && (Game1.CurrentEvent == null || Game1.CurrentEvent.id != -2) && Game1.currentLocation != null && Game1.currentLocation.Name != "Temp")
                {
                    SMonitor.Log($"Removing preday wedding for today");
                    Game1.weddingsToday.Clear();
                    Game1.weddingToday = false;
                }
            }
        }
        //[HarmonyPatch(typeof(Game1), nameof(Game1.prepareSpouseForWedding))]
        public class Game1_prepareSpouseForWedding_Patch
        {
            public static void Prefix(Farmer farmer)
            {
                if (!Config.EnableMod || !Config.FixWeddingStart)
                    return;
                farmer.friendshipData[farmer.spouse].Status = FriendshipStatus.Engaged;
            }
        }
    }
}