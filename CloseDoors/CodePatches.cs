using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Network;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CloseDoors
{
    public partial class ModEntry
    {
        public static Dictionary<GameLocation, Dictionary<Character, Point>> doorDict = new();
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool) })]
        public class GameLocation_isCollidingPosition_Patch
        {
            public static void Postfix(GameLocation __instance, Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile, bool ignoreCharacterRequirement)
            {
                if (!Config.ModEnabled)
                    return;
                if (!isFarmer)
                {
                    if (((character != null) ? character.controller : null) != null && character.FacingDirection % 2 == 0)
                    {
                        Layer buildings_layer = __instance.map.RequireLayer("Buildings");
                        Point tileLocation = character.FacingDirection == 2 ? new Point(position.Center.X / 64, position.Bottom / 64) : new Point(position.Center.X / 64, position.Top / 64);
                        if (IsDoorOpen(__instance, tileLocation))
                        {
                            if(!doorDict.TryGetValue(__instance, out var dict))
                            {
                                doorDict[__instance] = new();
                                dict = new();
                            }
                            dict[character] = tileLocation;
                        }
                        else if(doorDict.TryGetValue(__instance, out var dict) && dict.TryGetValue(character, out var point) && point == (character.FacingDirection == 2 ? tileLocation + new Point(0, -2) : tileLocation + new Point(0, 2)))
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
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performAction), new Type[] { typeof(string[]), typeof(Farmer), typeof(Location) })]
        public class GameLocation_performAction_Patch
        {
            public static void Prefix(string[] action, Farmer who, Location tileLocation)
            {
                if (!Config.ModEnabled)
                    return;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.openDoor))]
        public class GameLocation_openDoor_Patch
        {
            public static void Postfix(Location tileLocation, bool playSound)
            {
                if (!Config.ModEnabled)
                    return;
            }
        }
        [HarmonyPatch(typeof(InteriorDoor), "openDoorTiles")]
        public class InteriorDoor_openDoorTiles_Patch
        {
            public static bool Prefix(InteriorDoor __instance)
            {

                if (!Config.ModEnabled)
                    return true;
                return true;
            }
            public static void Postfix(InteriorDoor __instance)
            {

                if (!Config.ModEnabled)
                    return;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
            {
                if (!Config.ModEnabled)
                    return true;
                if (!string.IsNullOrEmpty(__instance.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings")))
                    return true;
                var tilePoint = new Point(tileLocation.X, tileLocation.Y);
                if (TryCloseDoor(__instance, tilePoint))
                {
                    return false;
                }
                return true;
            }

        }

    }
}