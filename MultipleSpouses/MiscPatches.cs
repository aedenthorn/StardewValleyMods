using Harmony;
using static Harmony.AccessTools;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Reflection;
using xTile.Dimensions;
using System.IO;
using StardewValley.BellsAndWhistles;
using xTile.Tiles;
using System.Linq;
using StardewValley.Network;
using System.Text.RegularExpressions;

namespace MultipleSpouses
{
	public static class MiscPatches
	{
		private static IMonitor Monitor;

		// call this method from your Entry class
		public static void Initialize(IMonitor monitor)
		{
			Monitor = monitor;
		}
		public static bool Farmer_doDivorce_Prefix(ref Farmer __instance)
		{
			try
			{
				__instance.divorceTonight.Value = false;
				if (!__instance.isMarried() || ModEntry.spouseToDivorce == null)
				{
					ModEntry.PMonitor.Log("Tried to divorce but not married!");
					return false;
				}

				string key = ModEntry.spouseToDivorce;


				ModEntry.PMonitor.Log($"Divorcing {key}");
				if (__instance.friendshipData.ContainsKey(key))
				{
					if (ModEntry.config.FriendlyDivorce)
					{
						__instance.friendshipData[key].Points = Math.Max(2000, __instance.friendshipData[key].Points);
						__instance.friendshipData[key].Status = FriendshipStatus.Friendly;
					}
					else
					{
						__instance.friendshipData[key].Points = 0;
						__instance.friendshipData[key].Status = FriendshipStatus.Divorced;
					}
					__instance.friendshipData[key].RoommateMarriage = false;
					NPC ex = Game1.getCharacterFromName(key);
					ex.PerformDivorce();
					Game1.player.Money -= 50000;
					ModEntry.ResetSpouses(__instance);
					ModEntry.PHelper.Content.InvalidateCache("Maps/FarmHouse1_marriage");
					ModEntry.PHelper.Content.InvalidateCache("Maps/FarmHouse2_marriage");
					typeof(FarmHouse).GetMethod("resetLocalState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Utility.getHomeOfFarmer(__instance), new object[] { });
					Utility.getHomeOfFarmer(__instance).showSpouseRoom();
					Game1.getFarm().addSpouseOutdoorArea(__instance.spouse == null ? "" : __instance.spouse);
				}

				ModEntry.spouseToDivorce = null;
				return false;
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(Farmer_doDivorce_Prefix)}:\n{ex}", LogLevel.Error);
			}
			return true;
		}

	}
}
