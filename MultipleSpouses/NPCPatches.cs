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
	public static class NPCPatches
	{
		private static IMonitor Monitor;

		// call this method from your Entry class
		public static void Initialize(IMonitor monitor)
		{
			Monitor = monitor;
		}

		public static bool NPC_setUpForOutdoorPatioActivity_Prefix(NPC __instance)
		{
			if (ModEntry.outdoorSpouse != __instance.Name)
			{
				return false;
			}
			ModEntry.PMonitor.Log("is outdoor spouse: " + __instance.Name);
			return true;
		}

		public static void NPC_checkAction_Prefix(ref NPC __instance, ref Farmer who, ref string __state)
		{
			if (who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsMarried())
			{
				__state = who.spouse;
				who.spouse = __instance.Name;
			}
			else
			{
				__state = null;
			}
		}

		public static void NPC_checkAction_Postfix(ref NPC __instance, ref Farmer who, string __state)
		{
			if (__state != null)
			{
				who.spouse = __state;
				__instance.hasBeenKissedToday.Value = false;
			}
		}


		public static void NPC_marriageDuties_Postfix(NPC __instance)
		{
			ModEntry.PMonitor.Log("marriage duties for " + __instance.Name);
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

		public static bool NPC_getSpouse_Prefix(NPC __instance, ref Farmer __result)
		{
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.friendshipData.ContainsKey(__instance.Name) && f.friendshipData[__instance.Name].IsMarried())
				{
					__result = f;
					return false;
				}
			}
			return true;
		}

		public static bool NPC_isMarried_Prefix(NPC __instance, ref bool __result)
		{
			__result = false;
			if (!__instance.isVillager())
			{
				return false;
			}
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.friendshipData.ContainsKey(__instance.Name) && f.friendshipData[__instance.Name].IsMarried())
				{
					__result = true;
					return false;
				}
			}
			return true;
		}

		public static bool NPC_isMarriedOrEngaged_Prefix(NPC __instance, ref bool __result)
		{
			__result = false;
			if (!__instance.isVillager())
			{
				return false;
			}
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.friendshipData.ContainsKey(__instance.Name) && (f.friendshipData[__instance.Name].IsMarried() || f.friendshipData[__instance.Name].IsEngaged()))
				{
					__result = true;
					return false;
				}
			}
			return true;
		}

		public static bool NPC_tryToReceiveActiveObject_Prefix(NPC __instance, ref Farmer who, string __state)
		{
			if (who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsMarried())
			{
				who.spouse = __instance.Name;
				GameLocation l = Game1.getLocationFromName(Game1.player.homeLocation);
				if (l is FarmHouse)
				{
					(l as FarmHouse).showSpouseRoom();
					l.resetForPlayerEntry();
				}
			}

			if (who.ActiveObject.ParentSheetIndex == 458)
			{
				if (!__instance.datable)
				{
					if (Game1.random.NextDouble() < 0.5)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3955", __instance.displayName));
						return false;
					}
					__instance.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3956") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3957"), __instance));
					Game1.drawDialogue(__instance);
					return false;
				}
				else
				{
					if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsDating())
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:AlreadyDatingBouquet", __instance.displayName));
						return false;
					}
					if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsDivorced())
					{
						__instance.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Divorced_bouquet"), __instance));
						Game1.drawDialogue(__instance);
						return false;
					}
					if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < 1000)
					{
						__instance.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3958") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3959"), __instance));
						Game1.drawDialogue(__instance);
						return false;
					}
					if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < 2000)
					{
						__instance.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3960") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3961"), __instance));
						Game1.drawDialogue(__instance);
						return false;
					}
					Friendship friendship = who.friendshipData[__instance.Name];
					if (!friendship.IsDating())
					{
						friendship.Status = FriendshipStatus.Dating;
						Multiplayer mp = ModEntry.PHelper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
						mp.globalChatInfoMessage("Dating", new string[]
						{
									Game1.player.Name,
									__instance.displayName
						});
					}
					__instance.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3962") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3963"), __instance));
					who.changeFriendship(25, __instance);
					who.reduceActiveItemByOne();
					who.completelyStopAnimatingOrDoingAction();
					__instance.doEmote(20, true);
					Game1.drawDialogue(__instance);
					return false;
				}
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
	}
}
