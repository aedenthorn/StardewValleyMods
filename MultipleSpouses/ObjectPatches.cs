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

namespace MultipleSpouses
{
	public class ObjectPatches
	{
		private static IMonitor Monitor;

		// call this method from your Entry class
		public static void Initialize(IMonitor monitor)
		{
			Monitor = monitor;
		}

		
		[HarmonyPatch(typeof(GameLocation), "resetLocalState")]
		static class FarmHouse_loadSpouseRoom
		{
			static void Postfix(GameLocation __instance)
			{
				if(__instance is Beach && ModEntry.config.BuyPendantsAnytime)
                {
					FieldRefAccess<Beach,NPC>(__instance as Beach,"oldMariner") = new NPC(new AnimatedSprite("Characters\\Mariner", 0, 16, 32), new Vector2(80f, 5f) * 64f, 2, "Old Mariner", null);
					return;
				}

				if (!ModEntry.config.BuildAllSpousesRooms || !(__instance is FarmHouse))
                {
					return;
                }

				FarmHouse farmHouse = __instance as FarmHouse;

				Farmer f = farmHouse.owner;

				if (farmHouse.upgradeLevel > 3)
				{
					return;
				}
				int ox = 0;
				int oy = 0;
				if (farmHouse.upgradeLevel > 1)
				{
					ox = 6;
					oy = 9;
				}

				for (int i = 0; i < 6; i++)
				{

					farmHouse.setMapTileIndex(ox + 29 + i, oy +  11, 0, "Buildings", 0);
				}
				for (int i = 0; i < 7; i++)
				{
					farmHouse.removeTile(ox + 29 + i, oy +  9, "Front");
					farmHouse.removeTile(ox + 29 + i, oy +  10, "Buildings");
					farmHouse.setMapTileIndex(ox + 28 + i, oy +  10, 165, "Front", 0);
					farmHouse.removeTile(ox + 29 + i, oy +  10, "Back");
				}
				for (int i = 0; i < 8; i++)
				{
					farmHouse.setMapTileIndex(ox + 28 + i, oy +  10, 165, "Front", 0);
				}
				for (int i = 0; i < 10; i++)
				{
					farmHouse.removeTile(ox + 35, oy +  0 + i, "Buildings");
					farmHouse.removeTile(ox + 35, oy +  0 + i, "Front");
				}
				for (int i = 0; i < 3; i++)
				{
					farmHouse.setMapTileIndex(ox + 29 + (i * 2 + 1), oy +  10, 53, "Back", 4);
					farmHouse.setMapTileIndex(ox + 29 + (i * 2), oy +  10, 54, "Back", 4);
				}
				farmHouse.removeTile(ox + 28, oy + 9, "Front");
				farmHouse.removeTile(ox + 28, oy + 10, "Buildings");
				farmHouse.removeTile(ox + 35, oy +  0, "Front");
				farmHouse.removeTile(ox + 35, oy +  0, "Buildings");
				farmHouse.setMapTileIndex(ox + 35, oy +  10, 53, "Back", 4);

				int count = 0;

				foreach (string name in f.friendshipData.Keys)
				{
					if (f.friendshipData[name].IsMarried() && farmHouse.owner.spouse != name)
					{
						ModEntry.BuildSpouseRoom(farmHouse, name, count++);
					}
				}

				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy +  0, 11, "Buildings", 0);
				for (int i = 0; i < 10; i++)
				{
					farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy +  1 + i, 68, "Buildings", 0);
				}
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy +  10, 130, "Front", 0);
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


		[HarmonyPatch(typeof(Beach), "checkAction")]
		static class Beach_checkAction
		{
			static bool Prefix(Beach __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result, NPC ___oldMariner)
			{
				if (___oldMariner != null && ___oldMariner.getTileX() == tileLocation.X && ___oldMariner.getTileY() == tileLocation.Y)
				{
					string playerTerm = Game1.content.LoadString("Strings\\Locations:Beach_Mariner_Player_" + (who.IsMale ? "Male" : "Female"));
					if (who.specialItems.Contains(460) && !Utility.doesItemWithThisIndexExistAnywhere(460, false))
					{
						for (int i = who.specialItems.Count - 1; i >= 0; i--)
						{
							if (who.specialItems[i] == 460)
							{
								who.specialItems.RemoveAt(i);
							}
						}
					}
					if (who.specialItems.Contains(460))
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerHasItem", playerTerm)));
					}
					else if (who.hasAFriendWithHeartLevel(10, true) && who.houseUpgradeLevel == 0)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerNotUpgradedHouse", playerTerm)));
					}
					else if (who.hasAFriendWithHeartLevel(10, true))
					{
						Response[] answers = new Response[]
						{
						new Response("Buy", Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerBuyItem_AnswerYes")),
						new Response("Not", Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerBuyItem_AnswerNo"))
						};
						__instance.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerBuyItem_Question", playerTerm)), answers, "mariner");
					}
					else
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerNoRelationship", playerTerm)));
					}
					__result = true;
					return false;
				}
				return true;
			}
		}
				
		[HarmonyPatch(typeof(NPC), "marriageDuties")]
		static class NPC_marriageDuties
		{
			static void Postfix(NPC __instance)
			{
				if(!(__instance.currentLocation is FarmHouse))
                {
					return;
                }
				Farmer spouse = __instance.getSpouse();
				FarmHouse farmHouse = Game1.getLocationFromName(spouse.homeLocation.Value) as FarmHouse;
				if (farmHouse.isTileOccupied(new Vector2((float)(__instance.Position.X / 64), (float)(__instance.Position.Y / 64))))
                {
					Point clearPos = farmHouse.getRandomOpenPointInHouse(Game1.random);
					if(clearPos != Point.Zero)
                    {
						__instance.setTilePosition(clearPos);
					}
					return;
				}
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