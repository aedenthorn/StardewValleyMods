using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using System;
using xTile.Dimensions;

namespace CustomLocks
{
    public static class ObjectPatches
    {
        private static IMonitor Monitor;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

		public static bool GameLocation_performAction_Prefix(GameLocation __instance, string action, Farmer who, Location tileLocation)
		{
			try
			{
				if (action != null && who.IsLocalPlayer)
				{
					string[] actionParams = action.Split(new char[]
					{
					' '
					});
					string text = actionParams[0];
					if (text == "WizardHatch" && (!who.friendshipData.ContainsKey("Wizard") || who.friendshipData["Wizard"].Points < 1000) && ModEntry.Config.AllowStrangerRoomEntry)
					{
						__instance.playSoundAt("doorClose", new Vector2((float)tileLocation.X, (float)tileLocation.Y), NetAudio.SoundContext.Default);
						Game1.warpFarmer("WizardHouseBasement", 4, 4, true);
						return false;
					}
					else if (text == "Door" && actionParams.Length > 1 && (!Game1.eventUp || ModEntry.Config.IgnoreEvents))
					{
						bool unlocked = false;
						for (int i = 1; i < actionParams.Length; i++)
						{
							if (who.getFriendshipHeartLevelForNPC(actionParams[i]) >= 2 || Game1.player.mailReceived.Contains("doorUnlock" + actionParams[i]))
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
		public static bool GameLocation_performTouchAction_Prefix(GameLocation __instance, string fullActionString, Vector2 playerStandingPosition)
		{
			try
			{
				if (Game1.eventUp && !ModEntry.Config.IgnoreEvents)
				{
					return false;
				}
				string[] acta = fullActionString.Split(' ');
				string text = acta[0];
				if (text == "Door" && ModEntry.Config.AllowStrangerRoomEntry)
				{
					int i = 1;
					while (i < acta.Length)
					{
						if (Game1.player.getFriendshipHeartLevelForNPC(acta[i]) >= 2)
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
				Monitor.Log($"Failed in {nameof(GameLocation_performAction_Prefix)}:\n{ex}", LogLevel.Error);
			}
			return true;
		}

		public static bool GameLocation_lockedDoorWarp(GameLocation __instance, string[] actionParams)
	    {
            try
            {
				if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) && Utility.getStartTimeOfFestival() < 1900)
				{
				}
				else if (actionParams[3].Equals("SeedShop") && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Wed") && !Utility.HasAnyPlayerSeenEvent(191393))
				{
					if (ModEntry.Config.AllowSeedShopWed)
					{
						ModEntry.DoWarp(actionParams, __instance);
						return false;
					}
				}
				else if (
					(Game1.timeOfDay < Convert.ToInt32(actionParams[4]) || Game1.timeOfDay >= Convert.ToInt32(actionParams[5]) && ModEntry.Config.AllowOutsideTime) 
					&& 
					(actionParams.Length >= 7 && !Game1.currentSeason.Equals("winter") && (!Game1.player.friendshipData.ContainsKey(actionParams[6]) || Game1.player.friendshipData[actionParams[6]].Points < Convert.ToInt32(actionParams[7])) && ModEntry.Config.AllowStrangerHomeEntry)
				)
				{
					// both outside time and stranger
					ModEntry.DoWarp(actionParams, __instance);
					return false;
				}
				else if (
					(Game1.timeOfDay >= Convert.ToInt32(actionParams[4]) && Game1.timeOfDay < Convert.ToInt32(actionParams[5]))
					&&
					(actionParams.Length >= 7 && !Game1.currentSeason.Equals("winter") && (!Game1.player.friendshipData.ContainsKey(actionParams[6]) || Game1.player.friendshipData[actionParams[6]].Points < Convert.ToInt32(actionParams[7])) && ModEntry.Config.AllowStrangerHomeEntry)
				)
				{
					// inside time and stranger
					ModEntry.DoWarp(actionParams, __instance);
					return false;
				}
				else if (
					(Game1.timeOfDay < Convert.ToInt32(actionParams[4]) || Game1.timeOfDay >= Convert.ToInt32(actionParams[5]) && ModEntry.Config.AllowOutsideTime)
					&&
					(actionParams.Length < 7 || Game1.currentSeason.Equals("winter") || (Game1.player.friendshipData.ContainsKey(actionParams[6]) && Game1.player.friendshipData[actionParams[6]].Points >= Convert.ToInt32(actionParams[7])))
				)
				{
					// outside time and not stranger
					ModEntry.DoWarp(actionParams, __instance);
					return false;
				}
				else if (actionParams.Length < 7 && ModEntry.Config.AllowOutsideTime)
                {
					ModEntry.DoWarp(actionParams, __instance);
					return false;
				}
				else if (ModEntry.Config.AllowStrangerHomeEntry)
                {
					ModEntry.DoWarp(actionParams, __instance);
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