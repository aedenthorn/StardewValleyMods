using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using System;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CloseDoors
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        public class GameLocation_isCollidingPosition_Patch
        {
            public static void Postfix(GameLocation __instance, Rectangle position, bool isFarmer, Character character)
            {
                if (!Config.ModEnabled)
                    return;
                if (!isFarmer)
                {
                    if (((character != null) ? character.controller : null) != null)
                    {
                        Layer buildings_layer = __instance.map.RequireLayer("Buildings");
                        Point tileLocation = GetMovingTile(character.FacingDirection, position);
                        if (IsDoorOpen(__instance, tileLocation))
                        {
                            if(!doorDict.TryGetValue(__instance, out var dict))
                            {
                                doorDict[__instance] = new();
                                dict = new();
                            }
                            dict[character] = tileLocation;
                        }
                        else if(doorDict.TryGetValue(__instance, out var dict) && dict.TryGetValue(character, out var point) && IsRecentDoorPoint(character, tileLocation, point))
                        {
                            dict.Remove(character);
                            if(TryCloseDoor(__instance, point))
                            {
                            }
                        }
                        else if(character.FacingDirection == 0)
                        {
                            Tile tile = buildings_layer.Tiles[tileLocation.X, tileLocation.Y];
                            if (tile != null && tile.Properties.ContainsKey("Action"))
                            {
                                __instance.openDoor(new Location(tileLocation.X, tileLocation.Y), Game1.currentLocation.Equals(__instance));
                            }
                            else
                            {
                                tileLocation = new Point(position.Center.X / 64, position.Top / 64);
                                tile = buildings_layer.Tiles[tileLocation.X, tileLocation.Y];
                                if (tile != null && tile.Properties.ContainsKey("Action"))
                                {
                                    __instance.openDoor(new Location(tileLocation.X, tileLocation.Y), Game1.currentLocation.Equals(__instance));
                                }
                            }
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static void Postfix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || __result)
                    return;
                if (!string.IsNullOrEmpty(__instance.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings")))
                    return;
                if (__instance.isCharacterAtTile(new Vector2(tileLocation.X, tileLocation.Y))?.IsVillager == true)
                    return;
                var tilePoint = new Point(tileLocation.X, tileLocation.Y);
                if (TryCloseDoor(__instance, tilePoint))
                {
                    __result = true;
                }
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isActionableTile))]
        public class GameLocation_isActionableTile_Patch
        {
            public static bool Prefix(GameLocation __instance, int xTile, int yTile, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                foreach (var d in __instance.interiorDoors.Doors)
                {
                    if (d.Position.X == xTile && d.Position.Y == yTile && d.Value)
                    {
                        __result = true;
                        return false;
                    }
                }
                return true;
            }

        }

    }
}