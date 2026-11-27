using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using xTile.Dimensions;

namespace CustomLocks
{
    public static class CustomLocksPatches
    {
        private static IMonitor Monitor;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public static bool Mountain_checkAction_Prefix(Mountain __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            if (!ModEntry.Config.Enabled)
                return true;

            try
            {
                if (__instance.map.GetLayer("Buildings").Tiles[tileLocation] != null)
                {
                    int tileIndex = __instance.map.GetLayer("Buildings").Tiles[tileLocation].TileIndex;
                    if (tileIndex == 1136 && !who.mailReceived.Contains("guildMember") && !who.hasQuest("16") && ModEntry.Config.AllowAdventureGuildEntry)
                    {
                        if (__instance.map.GetLayer("Buildings").Tiles[tileLocation].Properties.TryGetValue("Action", out xTile.ObjectModel.PropertyValue propertyValue))
                        {
                            string[] actionParams = propertyValue.ToString().Split(' ');
                            if ((Game1.timeOfDay < Convert.ToInt32(actionParams[4]) || Game1.timeOfDay >= Convert.ToInt32(actionParams[5])) && !ModEntry.Config.AllowOutsideTime)
                            {
                                string sub1 = Game1.getTimeOfDayString(Convert.ToInt32(actionParams[4])).Replace(" ", "");
                                string sub2 = Game1.getTimeOfDayString(Convert.ToInt32(actionParams[5])).Replace(" ", "");
                                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_OpenRange", sub1, sub2));
                            }
                            else
                            {
                                __instance.playSound("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
                                Game1.warpFarmer("AdventureGuild", 6, 19, false);
                            }
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Mountain_checkAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
        public static bool GameLocation_performAction_Prefix(GameLocation __instance, string[] action, Farmer who, Location tileLocation)
        {
            if (!ModEntry.Config.Enabled)
                return true;

            try
            {
                if (action != null && who.IsLocalPlayer)
                {
                    string text = action[0];
                    if (text == "WizardHatch" && (!who.friendshipData.ContainsKey("Wizard") || who.friendshipData["Wizard"].Points < 1000) && ModEntry.Config.AllowStrangerRoomEntry)
                    {
                        __instance.playSound("doorClose", new Vector2(tileLocation.X, tileLocation.Y));
                        Game1.warpFarmer("WizardHouseBasement", 4, 4, true);
                        return false;
                    }
                    else if (text == "Door" && action.Length > 1 && (!Game1.eventUp || ModEntry.Config.IgnoreEvents))
                    {
                        bool unlocked = false;
                        for (int i = 1; i < action.Length; i++)
                        {
                            if (who.getFriendshipHeartLevelForNPC(action[i]) >= 2 || Game1.player.mailReceived.Contains("doorUnlock" + action[i]))
                            {
                                unlocked = true;
                            }
                        }
                        if(!unlocked && ModEntry.Config.AllowStrangerRoomEntry)
                        {
                            Rumble.rumble(0.1f, 100f);
                            __instance.openDoor(tileLocation, true);
                            return false;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_performAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
        public static bool GameLocation_performTouchAction_Prefix(GameLocation __instance, string[] action, Vector2 playerStandingPosition)
        {
            if (!ModEntry.Config.Enabled)
                return true;

            try
            {
                if (Game1.eventUp && !ModEntry.Config.IgnoreEvents)
                {
                    return false;
                }
                string text = action[0];
                if (text == "Door" && ModEntry.Config.AllowStrangerRoomEntry)
                {
                    int i = 1;
                    while (i < action.Length)
                    {
                        if (Game1.player.getFriendshipHeartLevelForNPC(action[i]) >= 2)
                        {
                            return true;
                        }
                        i++;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_performTouchAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static bool GameLocation_lockedDoorWarp(GameLocation __instance, Point tile, string locationName, int openTime, int closeTime, string npcName, int minFriendship)
        {
            if (!ModEntry.Config.Enabled)
                return true;

            try
            {
                bool town_key_applies = Game1.player.HasTownKey;
                if (locationName == "FishShop" && Game1.player.mailReceived.Contains("willyHours"))
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
                if (GameLocation.AreStoresClosedForFestival() && __instance.InValleyContext())
                {
                    var x = 1;
                }
                else if (locationName.Equals("SeedShop") && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Wed") && !Utility.HasAnyPlayerSeenEvent("191393"))
                {
                    if (ModEntry.Config.AllowSeedShopWed)
                    {
                        if ((Game1.timeOfDay < openTime || Game1.timeOfDay >= closeTime) && !ModEntry.Config.AllowOutsideTime)
                        {
                            string sub1 = Game1.getTimeOfDayString(openTime).Replace(" ", "");
                            string sub2 = Game1.getTimeOfDayString(closeTime).Replace(" ", "");
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_OpenRange", sub1, sub2));
                        }
                        else
                        {
                            ModEntry.DoWarp(__instance, tile, locationName);
                        }
                        return false;
                    }
                }
                else if (
                    ((!town_key_applies && (Game1.timeOfDay < openTime || Game1.timeOfDay >= closeTime)) && ModEntry.Config.AllowOutsideTime) 
                    && 
                    (minFriendship > 0 && __instance.IsWinterHere() && (!Game1.player.friendshipData.ContainsKey(npcName) || Game1.player.friendshipData[npcName].Points < minFriendship) && ModEntry.Config.AllowStrangerHomeEntry)
                )
                {
                    // both outside time and stranger
                    ModEntry.DoWarp(__instance, tile, locationName);
                    return false;
                }
                else if (
                    (town_key_applies || (Game1.timeOfDay >= openTime && Game1.timeOfDay < closeTime))
                    &&
                    (minFriendship > 0 && __instance.IsWinterHere() && (!Game1.player.friendshipData.ContainsKey(npcName) || Game1.player.friendshipData[npcName].Points < minFriendship) && ModEntry.Config.AllowStrangerHomeEntry)
                )
                {
                    // inside time and stranger
                    ModEntry.DoWarp(__instance, tile, locationName);
                    return false;
                }
                else if (
                    (!town_key_applies && (Game1.timeOfDay < openTime || Game1.timeOfDay >= closeTime) && ModEntry.Config.AllowOutsideTime)
                    &&
                    ((minFriendship <= 0 || __instance.IsWinterHere()) || (Game1.player.friendshipData.TryGetValue(npcName, out var f) && f.Points >= minFriendship))
                )
                {
                    // outside time and not stranger
                    ModEntry.DoWarp(__instance, tile, locationName);
                    return false;
                }
                else if (minFriendship <= 0 && ModEntry.Config.AllowOutsideTime)
                {
                    ModEntry.DoWarp(__instance, tile, locationName);
                    return false;
                }
                else if (ModEntry.Config.AllowStrangerHomeEntry)
                {
                    if ((Game1.timeOfDay < openTime || Game1.timeOfDay >= closeTime) && !ModEntry.Config.AllowOutsideTime)
                    {
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
                    }
                    else
                    {
                        ModEntry.DoWarp(__instance, tile, locationName);
                    }
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_lockedDoorWarp)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }
    }
}