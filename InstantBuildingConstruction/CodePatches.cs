using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Network.NetEvents;

namespace InstantBuildingConstructionAndUpgrade
{
	public partial class ModEntry
	{
		public class GameLocation_houseUpgradeAccept_Patch
		{
			public static bool Prefix()
			{
				if (!Config.ModEnabled)
					return true;

				switch (Game1.player.HouseUpgradeLevel)
				{
					case 0:
						if (Config.FreeConstructionAndUpgrade || (Game1.player.Money >= 10000 && Game1.player.Items.ContainsId("(O)388", 450)))
						{
							if (!Config.FreeConstructionAndUpgrade)
							{
								Game1.player.Money -= 10000;
								Game1.player.Items.ReduceId("(O)388", 450);
							}
							if (!Config.InstantFarmhouseUpgrade)
							{
								Game1.player.daysUntilHouseUpgrade.Value = 3;
								Game1.RequireCharacter("Robin").setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted");
								Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
								Game1.Multiplayer.globalChatInfoMessage("HouseUpgrade", Game1.player.Name, Lexicon.getTokenizedPossessivePronoun(Game1.player.IsMale));
							}
							else
							{
								FinishHouseUpgrade();
							}
						}
						else if (Game1.player.Money < 10000)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood1"));     
						}
						break;
					case 1:
						if (Config.FreeConstructionAndUpgrade || (Game1.player.Money >= 65000 && Game1.player.Items.ContainsId("(O)709", 100)))
						{
							if (!Config.FreeConstructionAndUpgrade)
							{
								Game1.player.Money -= 65000;
								Game1.player.Items.ReduceId("(O)709", 100);
							}
							if (!Config.InstantFarmhouseUpgrade)
							{
								Game1.player.daysUntilHouseUpgrade.Value = 3;
								Game1.RequireCharacter("Robin").setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted");              
								Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
								Game1.Multiplayer.globalChatInfoMessage("HouseUpgrade", Game1.player.Name, Lexicon.getTokenizedPossessivePronoun(Game1.player.IsMale));
							}
							else
							{
								FinishHouseUpgrade();
							}
						}
						else if (Game1.player.Money < 65000)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood2", "100"));
						}
						break;
					case 2:
						if (Config.FreeConstructionAndUpgrade || (Game1.player.Money >= 100000))
						{
							if (!Config.FreeConstructionAndUpgrade)
							{
								Game1.player.Money -= 100000;
							}
							if (!Config.InstantFarmhouseUpgrade)
							{
								Game1.player.daysUntilHouseUpgrade.Value = 3;
								Game1.RequireCharacter("Robin").setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted");
								Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
								Game1.Multiplayer.globalChatInfoMessage("HouseUpgrade", Game1.player.Name, Lexicon.getTokenizedPossessivePronoun(Game1.player.IsMale));
							}
							else
							{
								FinishHouseUpgrade();
							}
						}
						else if (Game1.player.Money < 100000)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
						}
						break;
				}
				return false;
			}

			private static void FinishHouseUpgrade()
			{
				Game1.playSound("achievement");
				FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(Game1.player);
				homeOfFarmer.moveObjectsForHouseUpgrade(Game1.player.HouseUpgradeLevel + 1);
				Game1.player.HouseUpgradeLevel++;
				Game1.player.daysUntilHouseUpgrade.Value = -1;
				homeOfFarmer.setMapForUpgradeLevel(Game1.player.HouseUpgradeLevel);
				Game1.stats.checkForBuildingUpgradeAchievements();
				Game1.player.autoGenerateActiveDialogueEvent("houseUpgrade_" + Game1.player.HouseUpgradeLevel);
			}
		}

		public class GameLocation_communityUpgradeAccept_Patch
		{
			public static bool Prefix()
			{
				if (!Config.ModEnabled)
					return true;

				if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					if (Config.FreeConstructionAndUpgrade || (Game1.player.Money >= 500000 && Game1.player.Items.ContainsId("(O)388", 950)))
					{
						if (!Config.FreeConstructionAndUpgrade)
						{
							Game1.player.Money -= 500000;
							Game1.player.Items.ReduceId("(O)388", 950);
						}
						if (!Config.InstantCommunityUpgrade)
						{
							Game1.RequireLocation<Town>("Town").daysUntilCommunityUpgrade.Value = 3;
							Game1.RequireCharacter("Robin").setNewDialogue("Data\\ExtraDialogue:Robin_PamUpgrade_Accepted");
							Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
							Game1.Multiplayer.globalChatInfoMessage("CommunityUpgrade", Game1.player.Name);
						}
						else
						{
							FinishCommunityUpgradeUpgrade();
						}
					}
					else if (Game1.player.Money < 500000)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_NotEnoughWood3"));
					}
				}
				else if (!Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
				{
					if (Config.FreeConstructionAndUpgrade || (Game1.player.Money >= 300000))
					{
						if (!Config.FreeConstructionAndUpgrade)
						{
							Game1.player.Money -= 300000;
						}
						if (!Config.InstantCommunityUpgrade)
						{
							Game1.RequireLocation<Town>("Town").daysUntilCommunityUpgrade.Value = 3;
							Game1.RequireCharacter("Robin").setNewDialogue("Data\\ExtraDialogue:Robin_HouseUpgrade_Accepted");
							Game1.drawDialogue(Game1.getCharacterFromName("Robin"));
							Game1.Multiplayer.globalChatInfoMessage("CommunityUpgrade", Game1.player.Name);
						}
						else
						{
							FinishCommunityUpgradeUpgrade();
						}
					}
					else if (Game1.player.Money < 300000)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
					}
				}
				return false;
			}

			private static void FinishCommunityUpgradeUpgrade()
			{
				Game1.playSound("achievement");
				if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "pamHouseUpgrade", MailType.Received, add: true);
					Game1.player.changeFriendship(1000, Game1.getCharacterFromName("Pam"));
				}
				else
				{
					Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "communityUpgradeShortcuts", MailType.Received, add: true);
				}
			}
		}

		public class GameLocation_buildStructure_Patch
		{
			public static void Postfix1(Building building, bool __result)
			{
				Postfix(building, __result);
			}

			public static void Postfix2(Building constructed, bool __result)
			{
				Postfix(constructed, __result);
			}

			public static void Postfix(Building building, bool __result)
			{
				if (__result && !building.isUnderConstruction())
				{
					Game1.player.team.constructedBuildings.Add(building.buildingType.Value);
				}
			}
		}

		public class Building_FinishConstruction_Patch
		{
			public static void Postfix(Building __instance, bool onGameStart)
			{
				if (!onGameStart)
				{
					Game1.player.team.constructedBuildings.Add(__instance.buildingType.Value);
				}
			}
		}
	}
}
