using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;

namespace DoorKnock
{
    public partial class ModEntry
    {
        //[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performAction), new Type[] { typeof(string[]), typeof(Farmer), typeof(Location) })]
        public static class GameLocation_performAction_Patch
        {
            public static bool Prefix(GameLocation __instance, string[] action, Farmer who, Location tileLocation)
            {
                if (!Config.ModEnabled || !Config.OverrideRebuff || !who.IsLocalPlayer || action?.Length < 2)
                    return true;
                
                if(action[0] == "Door")
                {
                    for (int i = 1; i < action.Length; i++)
                    {
                        string name = action[i];
                        string mailKey = "doorUnlock" + name;
                        if (who.getFriendshipHeartLevelForNPC(name) >= 2 || Game1.player.mailReceived.Contains(mailKey))
                        {
                            return true;
                        }
                        if (name == "Sebastian" && __instance.IsGreenRainingHere() && Game1.year == 1)
                        {
                            return true;
                        }
                    }
                    KnockInteriorDoor(new Vector2(tileLocation.X, tileLocation.Y), action);
                    return false;
                }
                else if(action[0] == "LockedDoorWarp" && action.Length > 3)
                {
                    if (!ArgUtility.TryGetPoint(action, 1, out var tile2, out var error, "Point tile") || !ArgUtility.TryGet(action, 3, out var locationName, out error, true, "string locationName") || !ArgUtility.TryGetInt(action, 4, out var openTime, out error, "int openTime") || !ArgUtility.TryGetInt(action, 5, out var closeTime, out error, "int closeTime") || !ArgUtility.TryGetOptional(action, 6, out var npcName, out error, null, true, "string npcName") || !ArgUtility.TryGetOptionalInt(action, 7, out var minFriendship, out error, 0, "int minFriendship"))
                    {
                        return true;
                    }
                    bool town_key_applies = Game1.player.HasTownKey;
                    if (GameLocation.AreStoresClosedForFestival() && __instance.InValleyContext())
                    {
                        return true;
                    }
                    if (action[3] == "SeedShop" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Wed") && !Utility.HasAnyPlayerSeenEvent("191393") && !town_key_applies)
                    {
                        return true;
                    }
                    if (action[3] == "FishShop" && Game1.player.mailReceived.Contains("willyHours"))
                    {
                        openTime = 800;
                    }
                    if (town_key_applies)
                    {
                        if (town_key_applies && !__instance.InValleyContext())
                        {
                            town_key_applies = false;
                        }
                        if (town_key_applies && __instance is BeachNightMarket && locationName != "FishShop")
                        {
                            town_key_applies = false;
                        }
                    }
                    Friendship friendship;
                    bool canOpenDoor = (town_key_applies || (Game1.timeOfDay >= openTime && Game1.timeOfDay < closeTime)) && (minFriendship <= 0 || __instance.IsWinterHere() || (who.friendshipData.TryGetValue(npcName, out friendship) && friendship.Points >= minFriendship));
                    if (!canOpenDoor)
                    {
                        KnockExteriorDoor(new Vector2(tileLocation.X, tileLocation.Y), action);
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1.drawObjectDialogue), new Type[] { typeof(string) })]
        public static class Game1_drawObjectDialogue_Patch
        {
            public static bool Prefix(string dialogue)
            {
                if (!Config.ModEnabled || !Config.OverrideRebuff)
                    return true;
                Vector2 doorTile = Game1.player.Tile + new Vector2(0, -1);
                var actionString = Game1.currentLocation.doesTileHaveProperty(Game1.player.TilePoint.X, Game1.player.TilePoint.Y - 1, "Action", "Buildings", false);
                if(actionString?.StartsWith("LockedDoorWarp ") != true)
                {
                    doorTile = Game1.player.Tile + new Vector2(0, 1);
                    actionString = Game1.currentLocation.doesTileHaveProperty(Game1.player.TilePoint.X, Game1.player.TilePoint.Y + 1, "Action", "Buildings", false);
                }
                if (actionString?.StartsWith("LockedDoorWarp ") != true)
                    return true;

                string[] action = ArgUtility.SplitBySpace(actionString);

                if (dialogue == Game1.content.LoadString("Strings\\Locations:LockedDoor") || (action.Length > 6 && dialogue == Game1.content.LoadString("Strings\\Locations:LockedDoor_FriendsOnly", Game1.getCharacterFromName(action[6], true, false)?.displayName)))
                {
                    KnockExteriorDoor(doorTile, action);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.ShowLockedDoorMessage))]
        public static class GameLocation_ShowLockedDoorMessage_Patch
        {
            public static bool Prefix(GameLocation __instance)
            {
                if (!Config.ModEnabled || !Config.OverrideRebuff)
                    return true;
                Vector2 doorTile = Game1.player.Tile + new Vector2(0, -1);
                var actionString = Game1.currentLocation.doesTileHaveProperty(Game1.player.TilePoint.X, Game1.player.TilePoint.Y - 1, "Action", "Buildings", false);
                if (actionString?.StartsWith("Door ") != true)
                {
                    doorTile = Game1.player.Tile + new Vector2(0, 1);
                    actionString = Game1.currentLocation.doesTileHaveProperty(Game1.player.TilePoint.X, Game1.player.TilePoint.Y + 1, "Action", "Buildings", false);
                }
                if (actionString?.StartsWith("Door ") != true)
                    return true;
                KnockInteriorDoor(doorTile, ArgUtility.SplitBySpace(actionString));
                return false;
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.Halt))]
        public static class Farmer_Halt_Patch
        {
            public static bool Prefix(Farmer __instance)
            {
                return farmerController.Value == null;
                     
            }
        }

            
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.Update))]
        public static class Farmer_Update_Patch
        {
            public static void Prefix(Farmer __instance, GameTime time)
            {
                if (__instance == Game1.player && farmerController.Value != null)
                {
                    Farmer player = __instance;
                    player.facingDirection.Value = 2;
                    player.setRunning(false, true);
                    player.ignoreCollisions = true;
                    if(!player.movementDirections.Contains(2))
                        player.movementDirections.Add(2);
                }
            }
            public static void Postfix(Farmer __instance, GameTime time)
            {

                if (__instance == Game1.player && farmerController.Value != null)
                {

                    Vector2 start = farmerController.Value.Value;
                    Farmer player = __instance;
                    if (Math.Abs(Vector2.Distance(player.Position, start)) + player.Speed > 64)
                    {
                        farmerController.Value = null;
                        player.ignoreCollisions = false;
                        player.Halt();
                        player.faceDirection(0);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(InteriorDoorDictionary), nameof(InteriorDoorDictionary.ResetLocalState))]
        public static class InteriorDoorDictionary_ResetLocalState_Patch
        {
            public static void Postfix(InteriorDoorDictionary __instance)
            {
                if (!Config.ModEnabled)
                {
                    return;
                }
                foreach (var kvp in __instance.Pairs)
                {
                    if (kvp.Value != true)
                    {
                        try
                        {
                            var door = Game1.currentLocation.interiorDoors.Doors.First(d => d.Position == kvp.Key);
                            Map map = door.Location.Map;
                            if (map == null)
                            {
                                continue;
                            }
                            if (door != null && map.RequireLayer("Buildings").Tiles[kvp.Key.X, kvp.Key.Y] == null)
                            {
                                if (door.Tile == null)
                                {
                                    continue;
                                }
                                map.RequireLayer("Buildings").Tiles[kvp.Key.X, kvp.Key.Y] = door.Tile;
                                door.Location.removeTileProperty(door.Position.X, door.Position.Y, "Back", "TemporaryBarrier");
                                map.RequireLayer("Front").Tiles[kvp.Key.X, kvp.Key.Y - 1] = new StaticTile(map.RequireLayer("Front"), door.Tile.TileSheet, BlendMode.Alpha, door.Tile.TileIndex - door.Tile.TileSheet.SheetWidth);
                                map.RequireLayer("Front").Tiles[kvp.Key.X, kvp.Key.Y - 2] = new StaticTile(map.RequireLayer("Front"), door.Tile.TileSheet, BlendMode.Alpha, door.Tile.TileIndex - door.Tile.TileSheet.SheetWidth * 2);
                            }
                        }
                        catch { }
                    }
                }
            }
        }
    }
}