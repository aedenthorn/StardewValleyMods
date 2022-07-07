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
        public static string GetCoopName()
        {
            if (!Config.ModEnabled || coopName is null)
                return "Abigail";
            return coopName;
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performAction))]
        public class GameLocation_performAction_Patch
        {
            public static bool Prefix(GameLocation __instance, string action, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || action == null || !who.IsLocalPlayer || !SHelper.Input.IsDown(Config.ModKey))
                    return true;
                if (MineShaft.lowestLevelReached < (Game1.player.currentLocation.Name == "SkullCave" ? 121 : 1))
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
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !who.IsLocalPlayer || !SHelper.Input.IsDown(Config.ModKey))
                    return true;
                Tile tile = __instance.map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
                if (tile == null || tile.TileIndex != 115)
                    return true;
                Game1.activeClickableMenu = new BetterElevatorMenu();
                __result = true;
                return false;
            }
        }
    }
}