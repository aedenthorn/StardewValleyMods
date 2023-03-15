using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;
using xTile.Tiles;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace LikeADuckToWater
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.updatePerTenMinutes))]
        public class FarmAnimal_MovePosition_Patch
        {
            public static void Postfix(FarmAnimal __instance)
            {
                if (!Config.ModEnabled || __instance.controller is not null || __instance.currentLocation != Game1.getFarm() || __instance.currentLocation.waterTiles is null || __instance.modData.ContainsKey(swamTodayKey) || !__instance.CanSwim() || __instance.isSwimming.Value || !__instance.wasPet.Value || __instance.fullness.Value < 195)
                    return;
                Stopwatch s = new Stopwatch();
                s.Start();
                TryMoveToWater(__instance, __instance.currentLocation);
                s.Stop();
                SMonitor.Log($"Trying to move took {s.ElapsedMilliseconds}ms");
            }
        }
        
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool) })]
        public class GameLocation_isCollidingPosition_Patch
        {
            public static bool Prefix(GameLocation __instance, Rectangle position, Character character, ref bool __result)
            {
                if (!isCollidingWater(__instance, character, position.X / 64, position.Y / 64))
                    return false;
                return true;
            }
        }
        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.dayUpdate))]
        public class FarmAnimal_dayUpdate_Patch
        {
            public static void Postfix(FarmAnimal __instance)
            {
                __instance.modData.Remove(swamTodayKey);
            }
        }
        [HarmonyPatch(typeof(Farm), nameof(Farm.DayUpdate))]
        public class Farm_DayUpdate_Patch
        {
            public static void Postfix(Farm __instance)
            {
                RebuildHopSpots(__instance);
                

            }
        }

    }
}