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
	public class LocationPatches
	{
		private static IMonitor Monitor;

		// call this method from your Entry class
		public static void Initialize(IMonitor monitor)
		{
			Monitor = monitor;
		}


		[HarmonyPatch(typeof(GameLocation), "updateMap")]
		static class GameLocation_updateMap
		{
			static bool Prefix(ref GameLocation __instance, string ___loadedMapPath)
			{
				if(__instance is FarmHouse)
                {
					FarmHouse farmHouse = __instance as FarmHouse;
					ModEntry.ResetSpouses(farmHouse.owner);
					bool showSpouse = ModEntry.spouses.Count > 0 || farmHouse.owner.spouse != null; 
					__instance.mapPath.Value = "Maps\\" + __instance.Name + ((farmHouse.upgradeLevel == 0) ? "" : ((farmHouse.upgradeLevel == 3) ? "2" : string.Concat(farmHouse.upgradeLevel))) + (showSpouse ? "_marriage" : "");

					if (!object.Equals(__instance.mapPath.Value, ___loadedMapPath))
					{
						__instance.reloadMap();
					}
					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(GameLocation), "resetLocalState")]
		static class GameLocation_resetLocalState
		{
			static void Postfix(GameLocation __instance)
			{

				if(__instance is Beach && ModEntry.config.BuyPendantsAnytime)
                {
					FieldRefAccess<Beach,NPC>(__instance as Beach,"oldMariner") = new NPC(new AnimatedSprite("Characters\\Mariner", 0, 16, 32), new Vector2(80f, 5f) * 64f, 2, "Old Mariner", null);
					return;
				}

				if (!(__instance is FarmHouse))
                {
					return;
                }
				ModEntry.PMonitor.Log("reset farm state");

				FarmHouse farmHouse = __instance as FarmHouse;

				ModEntry.ResetSpouseRoles();

                if (!ModEntry.config.BuildAllSpousesRooms)
                {
					return;
                }

				Farmer f = farmHouse.owner;

				if (ModEntry.spouses.ContainsKey("Emily") && f.spouse != "Emily")
                {
					int offset = (ModEntry.spouses.Keys.ToList().IndexOf("Emily") + 1) * 7 * 64;
					Vector2 parrotSpot = new Vector2(2064f+offset, 160f);
					int upgradeLevel = farmHouse.upgradeLevel;
					if (upgradeLevel - 2 <= 1)
					{
						parrotSpot = new Vector2(2448f + offset, 736f);
					}
					farmHouse.temporarySprites.Add(new EmilysParrot(parrotSpot));
				}



				if (farmHouse.upgradeLevel > 3 || ModEntry.spouses.Count == 0)
				{
					return;
				}

				int untitled = 0;
				for(int i = 0; i < farmHouse.map.TileSheets.Count; i++)
                {
					if (farmHouse.map.TileSheets[i].Id == "untitled tile sheet")
						untitled = i;
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

				}
				for (int i = 0; i < 7; i++)
				{
					farmHouse.setMapTileIndex(ox + 29 + i, oy + 11, 0, "Buildings", 0);
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
					farmHouse.setMapTileIndex(ox + 29 + (i * 2 + 1), oy +  10, 53, "Back", untitled);
					farmHouse.setMapTileIndex(ox + 29 + (i * 2), oy +  10, 54, "Back", untitled);
				}
				farmHouse.removeTile(ox + 28, oy + 9, "Front");
				farmHouse.removeTile(ox + 28, oy + 10, "Buildings");
				farmHouse.removeTile(ox + 35, oy +  0, "Front");
				farmHouse.removeTile(ox + 35, oy +  0, "Buildings");
				farmHouse.setMapTileIndex(ox + 35, oy +  10, 53, "Back", untitled);

				int count = 0;

				foreach(string name in ModEntry.spouses.Keys)
                {
					ModEntry.BuildSpouseRoom(farmHouse, name, count++);
				}


				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy +  0, 11, "Buildings", 0);
				for (int i = 0; i < 10; i++)
				{
					farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy +  1 + i, 68, "Buildings", 0);
				}
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy +  10, 130, "Front", 0);
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
				
	}
}