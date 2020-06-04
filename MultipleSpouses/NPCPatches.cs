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

namespace MultipleSpouses
{
	public class NPCPatches
	{
		private static IMonitor Monitor;

		// call this method from your Entry class
		public static void Initialize(IMonitor monitor)
		{
			Monitor = monitor;
		}

		[HarmonyPatch(typeof(NPC), "setUpForOutdoorPatioActivity")]
		static class NPC_setUpForOutdoorPatioActivity
		{
			static bool Prefix(NPC __instance)
			{
				if(ModEntry.outdoorSpouse != __instance.Name)
                {
					return false;
				}
				ModEntry.PMonitor.Log("is outdoor spouse: " + __instance.Name);
				return true;
			}
		}

		[HarmonyPatch(typeof(NPC), "checkAction")]
		static class NPC_checkAction
		{
			static void Prefix(ref NPC __instance, ref Farmer who, ref string __state)
			{
				if(who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsMarried())
                {
					__state = who.spouse;
					who.spouse = __instance.Name;
				}
				else
                {
					__state = null;
                }
			}
			static void Postfix(ref NPC __instance, ref Farmer who, string __state)
			{
				if(__state != null)
                {
					who.spouse = __state;
					__instance.hasBeenKissedToday.Value = false;
				}
			}
		}


		[HarmonyPatch(typeof(NPC), "marriageDuties")]
		static class NPC_marriageDuties
		{
			static void Postfix(NPC __instance)
			{
				ModEntry.PMonitor.Log("marriage duties for "+__instance.Name);
				if (!(__instance.currentLocation is FarmHouse))
                {
					return;
                }
				ModEntry.PMonitor.Log("in farm house");
				__instance.shouldPlaySpousePatioAnimation.Value = false;

				Farmer spouse = __instance.getSpouse();
				FarmHouse farmHouse = Game1.getLocationFromName(spouse.homeLocation.Value) as FarmHouse;
				Vector2 spot = (farmHouse.upgradeLevel == 1) ? new Vector2(32f, 5f) : new Vector2(38f, 14f);

				if (ModEntry.kitchenSpouse == __instance.Name)
                {
					ModEntry.PMonitor.Log($"{__instance.Name} is in kitchen");
					__instance.setTilePosition(farmHouse.getKitchenStandingSpot());
					return;
				}
				else if (ModEntry.bedSpouse == __instance.Name)
				{
					ModEntry.PMonitor.Log($"{__instance.Name} is in bed");
					__instance.setTilePosition(farmHouse.getSpouseBedSpot(__instance.getSpouse().spouse));
					return;
				}

                if (!ModEntry.config.BuildAllSpousesRooms && spouse.spouse != __instance.Name)
                {
					__instance.setTilePosition(farmHouse.getRandomOpenPointInHouse(Game1.random));
					return;
				}

				int offset = 0;
				if (spouse.spouse != __instance.Name)
                {
					int idx = ModEntry.spouses.Keys.ToList().IndexOf(__instance.Name);
					offset = 7 * (idx + 1);
				}
				__instance.setTilePosition((int)spot.X + offset, (int)spot.Y);
				__instance.faceDirection(Game1.random.Next(0, 4));
			}
		}
				
		[HarmonyPatch(typeof(NPC), "getSpouse")]
		static class NPC_getSpouse
		{
			static bool Prefix(NPC __instance, ref Farmer __result)
			{
				foreach (Farmer f in Game1.getAllFarmers())
                {
					if(f.friendshipData.ContainsKey(__instance.Name) && f.friendshipData[__instance.Name].IsMarried())
                    {
						__result = f;
						return false;
					}
				}
				return true;
			}
		}
		
		[HarmonyPatch(typeof(NPC), "isMarried")]
		static class NPC_isMarried
		{
			static bool Prefix(NPC __instance, ref bool __result)
			{
				__result = false;
				if (!__instance.isVillager())
				{
					return false;
				}
				foreach (Farmer f in Game1.getAllFarmers())
                {
					if(f.friendshipData.ContainsKey(__instance.Name) && f.friendshipData[__instance.Name].IsMarried())
                    {
						__result = true;
						return false;
					}
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(NPC), "isMarriedOrEngaged")]
		static class NPC_isMarriedOrEngaged
		{
			static bool Prefix(NPC __instance, ref bool __result)
			{
				__result = false;
				if (!__instance.isVillager())
				{
					return false;
				}
				foreach (Farmer f in Game1.getAllFarmers())
                {
					if(f.friendshipData.ContainsKey(__instance.Name) && (f.friendshipData[__instance.Name].IsMarried() || f.friendshipData[__instance.Name].IsEngaged()))
                    {
						__result = true;
						return false;
					}
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(NPC), "tryToReceiveActiveObject")]
		static class tryToReceiveActiveObject
		{
			static bool Prefix(NPC __instance, ref Farmer who, string __state)
			{
				try
				{
					if (who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsMarried())
					{
						who.spouse = __instance.Name;
					}

					if (who.ActiveObject.ParentSheetIndex == 460)
					{
						if (!__instance.datable || __instance.isMarriedOrEngaged() || (who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < 1500))
						{
							if (Game1.random.NextDouble() < 0.5)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", __instance.displayName));
								return false;
							}
							__instance.CurrentDialogue.Push(new Dialogue((__instance.Gender == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3970") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3971"), __instance));
							Game1.drawDialogue(__instance);
							return false;
						}
						else if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < 2500)
						{
							if (!who.friendshipData[__instance.Name].ProposalRejected)
							{
								__instance.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3972") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3973"), __instance));
								Game1.drawDialogue(__instance);
								who.changeFriendship(-20, __instance);
								who.friendshipData[__instance.Name].ProposalRejected = true;
								return false;
							}
							__instance.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3974") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3975"), __instance));
							Game1.drawDialogue(__instance);
							who.changeFriendship(-50, __instance);
							return false;
						}
						else
						{
							if (!__instance.datable || who.houseUpgradeLevel >= 1)
							{
								typeof(NPC).GetMethod("engagementResponse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { who, false });
								return false;
							}
							if (Game1.random.NextDouble() < 0.5)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", __instance.displayName));
								return false;
							}
							__instance.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3972"), __instance));
							Game1.drawDialogue(__instance);
							return false;
						}
					}
					else
					{
						return true;
					}
				}
				catch (Exception ex)
				{
					Monitor.Log($"Failed in {nameof(tryToReceiveActiveObject)}:\n{ex}", LogLevel.Error);
					return true;
				}
			}
		}
	}
}