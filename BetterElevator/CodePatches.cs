using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using xTile.Dimensions;
using xTile.Tiles;

namespace BetterElevator
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performAction))]
        public class GameLocation_performAction_Patch
        {
            public static bool Prefix(GameLocation __instance, string action, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || action == null || !who.IsLocalPlayer || !SHelper.Input.IsDown(Config.ModKey))
                    return true;
                if (!Config.Unrestricted && MineShaft.lowestLevelReached < (Game1.player.currentLocation.Name == "SkullCave" ? 121 : 1))
                {
                    return true;
                }

                string[] actionParams = action.Split(' ');
                string text = actionParams[0];
                if (text == "SkullDoor")
                {
                    if (!who.hasSkullKey || !who.hasUnlockedSkullDoor)
                        return true;
                }
                else if (text == "Mine" && actionParams.Length > 1 && actionParams[1] == "77377")
                {
                    return true;
                }
                else if (text != "Mine")
                {
                    return true;
                }
                Game1.activeClickableMenu = new BetterElevatorMenu();
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(MineShaft), nameof(MineShaft.checkAction))]
        public class MineShaft_checkAction_Patch
        {
            public static bool Prefix(MineShaft __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !who.IsLocalPlayer)
                    return true;
                Tile tile = __instance.map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
                if (tile == null)
                    return true;
                if (tile.TileIndex == 115)
                {
                    if (!SHelper.Input.IsDown(Config.ModKey))
                        return true;
                    if (__instance.mineLevel == 77377)
                        return true;
                    Game1.activeClickableMenu = new BetterElevatorMenu();
                    __result = true;
                    return false;
                }
                if (tile.TileIndex == 173)
                {
                    if (__instance.mineLevel == 77376)
                    {
                        Game1.enterMine(__instance.mineLevel + 2);
                        __instance.playSound("stairsdown");
                        __result = true;
                        return false;
                    }
                    if (__instance.mineLevel == int.MaxValue)
                    {
                        Game1.enterMine(__instance.mineLevel);
                        __instance.playSound("stairsdown");
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(MineShaft), nameof(MineShaft.shouldCreateLadderOnThisLevel))]
        public class MineShaft_shouldCreateLadderOnThisLevel_Patch
        {
            public static void Postfix(MineShaft __instance, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return;
                if (__instance.mineLevel == int.MaxValue)
                    __result = false;
            }
        }
    }
}