using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace MapTeleport
{
	public partial class ModEntry
	{
		public class MapPage_receiveLeftClick_Patch
		{
			public static bool Prefix(MapPage __instance, int x, int y)
			{
				if (!Config.ModEnabled || Game1.eventUp || Game1.isFestival())
					return false;

				return !CheckClickableComponents(__instance.points.Values.ToList(), x, y);
			}
		}

		public static bool CheckClickableComponents(List<ClickableComponent> components, int x, int y)
		{
			if (components is not null)
			{
				// Sort boundries so that the function will warp to the smallest overlapping area
				components.Sort(delegate (ClickableComponent a, ClickableComponent b)
				{
					return (a.bounds.Height * a.bounds.Width).CompareTo(b.bounds.Height * b.bounds.Width);
				});
				foreach (ClickableComponent component in components)
				{
					if (component.containsPoint(x, y))
					{
						string[] array = component?.name.Split('/');

						if (array.Length >= 2)
						{
							string id = array[1];
							string locationName = GetLocationName(array[0], id);
							GameLocation location = Game1.getLocationFromName(locationName);

							if (location is not null)
							{
								Point tilePoint = GetTilePoint(locationName, id);

								if (tilePoint != new Point(-1, -1))
								{
									SMonitor.Log($"Teleporting to {locationName} - {id} ({tilePoint.X}, {tilePoint.Y})", LogLevel.Debug);
									Game1.activeClickableMenu?.exitThisMenu(true);
									Game1.warpFarmer(location.NameOrUniqueName, tilePoint.X, tilePoint.Y, false);
									return true;
								}
							}
						}
					}
				}
			}
			return false;
		}

		private static string GetLocationName(string locationName, string id)
		{
			locationName = GetLocationNameVanilla(locationName, id);

			if (CompatibilityUtility.IsSVELoaded)
			{
				locationName = GetLocationNameSVE(locationName, id, locationName);
			}
			if (CompatibilityUtility.IsESLoaded)
			{
				locationName = GetLocationNameES(locationName, id, locationName);
			}
			return locationName;
		}

		private static string GetLocationNameVanilla(string locationName, string id)
		{
			return (locationName, id) switch
			{
				("SecretWoods", _) => "Woods",
				("GingerIsland", _) => "IslandSouth",
				("East", _) => "IslandEast",
				("North", _) => "IslandNorth",
				("South", _) => "IslandSouth",
				("SouthEast", _) => "IslandSouthEast",
				("West", _) => "IslandWest",
				_ => locationName
			};
		}

		private static string GetLocationNameSVE(string locationName, string id, string value)
		{
			return (locationName, id) switch
			{
				("Mountain", "AdventureGuild") => "Custom_AdventurerSummit",
				("Mountain", "Mines") => "Custom_AdventurerSummit",
				("FlashShifter.StardewValleyExpandedCP_TownMap_GrandpasShed", "FlashShifter.StardewValleyExpandedCP_Town_GrandpasShed") => "Custom_GrandpasShedOutside",
				("FlashShifter.StardewValleyExpandedCP_TownMap_Andys", "FlashShifter.StardewValleyExpandedCP_Town_Andys") => "Forest",
				("FlashShifter.StardewValleyExpandedCP_TownMap_Fairhaven", "FlashShifter.StardewValleyExpandedCP_Town_Fairhaven") => "Forest",
				("FlashShifter.StardewValleyExpandedCP_TownMap_SophiasHouse", "FlashShifter.StardewValleyExpandedCP_Town_SophiasHouse") => "Custom_BlueMoonVineyard",
				("FlashShifter.StardewValleyExpandedCP_TownMap_Bluemoon", "FlashShifter.StardewValleyExpandedCP_Town_Bluemoon") => "Custom_BlueMoonVineyard",
				("FlashShifter.StardewValleyExpandedCP_TownMap_EmeraldFarm", "FlashShifter.StardewValleyExpandedCP_Town_EmeraldFarm") => "Railroad",
				("FlashShifter.StardewValleyExpandedCP_TownMap_JenkinsResidence", "FlashShifter.StardewValleyExpandedCP_Town_JenkinsResidence") => "Town",
				("FlashShifter.StardewValleyExpandedCP_TownMap_OldCommunityGarden", "FlashShifter.StardewValleyExpandedCP_Town_OldCommunityGarden") => "Custom_Garden",
				("FlashShifter.StardewValleyExpandedCP_TownMap_ShearwaterBridge", "FlashShifter.StardewValleyExpandedCP_Town_ShearwaterBridge") => "Custom_ShearwaterBridge",
				("Custom_ScarlettsHouse", "Custom_ScarlettsHouse") => "Custom_GrampletonSuburbs",
				("Custom_HighlandsOutpost", "Custom_HighlandsOutpost") => "Custom_Highlands",
				("Custom_DesertRailway", "Custom_DesertRailway") => "Custom_CrimsonBadlands",
				("Custom_CastleVillageEntrance", "Custom_CastleVillageEntrance") => "Custom_CrimsonBadlands",
				("Custom_FirstSlashGuild", "Custom_FirstSlashGuild") => "Custom_FableReef",
				_ => value
			};
		}

		private static string GetLocationNameES(string locationName, string id, string value)
		{
			return (locationName, id) switch
			{
				("ScarpCrossing", "JessieHouse") => "EastScarp_ClearingHouse",
				("ScarpCrossing", "ScarpCrossing") => "EastScarp_ClearingHouse",
				("NoraandToriHouse", "NoraandToriHouse") => "EastScarp_Village",
				("MainScarp", "AideenHouse") => "EastScarp_Village",
				("MainScarp", "ScarpeInn") => "EastScarp_Village",
				("MainScarp", "ScarpePond") => "EastScarp_Village",
				("MainScarp", "VetHouse") => "EastScarp_Village",
				("MainScarp", "JacobBarn") => "EastScarp_Village",
				("LionsMane", "LionsMane") => "EastScarp_Village",
				("RodneysSilo", "RodneysSilo") => "EastScarp_Village",
				("MainScarp", "PlumFarm") => "EastScarp_Village",
				("MainScarp", "Underscarp") => "EastScarp_Village",
				("MainScarp", "Tidepools") => "EastScarp_Village",
				("MainScarp", "Lighthouse") => "EastScarp_Village",
				("MateoHome", "MateoHome") => "EastScarp_Village",
				("AbandonedMine", "OrchardHouse") => "EastScarp_MineEntrance",
				("CherryOrchard", "OrchardHouse") => "EastScarp_Orchard",
				("CherryOrchard", "SmugglerShack") => "EastScarp_Orchard",
				_ => value
			};
		}

		private static Point GetTilePoint(string locationName, string id)
		{
			Point tilePoint = GetTilePointVanilla(locationName, id);

			if (CompatibilityUtility.IsSVELoaded)
			{
				tilePoint = GetTilePointSVE(locationName, id, tilePoint);
			}
			if (CompatibilityUtility.IsESLoaded)
			{
				tilePoint = GetTilePointES(locationName, id, tilePoint);
			}
			return tilePoint;
		}

		private static Point GetTilePointVanilla(string locationName, string id)
		{
			return (locationName, id) switch
			{
				("Backwoods", _) => new Point(18, 18),
				("Beach", "ElliottCabin") => new Point(49, 11),
				("Beach", "LonelyStone") => new Point(11, 25),
				("Beach", "FishShop_DefaultHours") or ("Beach", "FishShop_ExtendedHours") => new Point(30, 34),
				("Beach", _) => new Point(20, 4),
				("BeachNightMarket", "Submarine") => new Point(5, 35),
				("BeachNightMarket", "MermaidHouse") => new Point(58, 32),
				("BeachNightMarket", _) => new Point(20, 4),
				("BusStop", _) => new Point(14, 23),
				("Desert", _) => new Point(35, 43),
				("Farm", _) => GetHomeTilePoint(),
				("Forest", "MarnieRanch") => new Point(90, 16),
				("Forest", "LeahCottage") => new Point(104, 33),
				("Forest", "WizardTower") => new Point(5, 27),
				("Forest", "AbandonedHouse") => new Point(34, 96),
				("Forest", "SewerPipe") => new Point(94, 100),
				("Forest", _) => new Point(35, 43),
				("Mountain", "AdventureGuild") => new Point(76, 9),
				("Mountain", "Carpenter") => new Point(12, 26),
				("Mountain", "Mines") => new Point(54, 6),
				("Mountain", "Quarry") => new Point(124, 12),
				("Mountain", "Tent") => new Point(29, 8),
				("Mountain", _) => new Point(31, 20),
				("Railroad", "Spa") => new Point(10, 57),
				("Railroad", _) => new Point(29, 58),
				("Woods", _) => new Point(8, 9),
				("Town", "AlexHouse") => new Point(57, 64),
				("Town", "Blacksmith") => new Point(94, 82),
				("Town", "CommunityCenter") or ("Town", "MovieTheater_JojaRoute") => new Point(52, 20),
				("Town", "Graveyard") => new Point(47, 88),
				("Town", "HaleyHouse") => new Point(20, 89),
				("Town", "Hospital") => new Point(36, 56),
				("Town", "JojaMart_Open") or ("Town", "JojaMart_Abandoned") or ("Town", "MovieTheater_CommunityCenterRoute") => new Point(95, 51),
				("Town", "ManorHouse") => new Point(58, 86),
				("Town", "Museum") => new Point(101, 90),
				("Town", "PamHouse") or ("Town", "Trailer") => new Point(72, 69),
				("Town", "PierreStore_InitialHours") or ("Town", "PierreStore_ExtendedHours") => new Point(43, 57),
				("Town", "Saloon") => new Point(45, 71),
				("Town", "SamHouse") => new Point(10, 86),
				("Town", "Sewer") => new Point(34, 97),
				("Town", _) => new Point(29, 67),
				("IslandEast", "JungleHut") => new Point(22, 11),
				("IslandEast", _) => new Point(21, 37),
				("IslandNorth", "DigSite") => new Point(5, 49),
				("IslandNorth", "FieldOffice") => new Point(46, 47),
				("IslandNorth", "Trader") => new Point(36, 72),
				("IslandNorth", "Volcano") => new Point(40, 24),
				("IslandNorth", _) => new Point(36, 88),
				("IslandSouth", "Resort") => new Point(19, 25),
				("IslandSouth", _) => new Point(21, 43),
				("IslandSouthEast", "PirateCove") => new Point(29, 19),
				("IslandSouthEast", _) => new Point(19, 24),
				("IslandWest", "BirdieShack") => new Point(19, 57),
				("IslandWest", "GourmandCave") => new Point(96, 34),
				("IslandWest", "QiWalnutRoom") => new Point(20, 23),
				("IslandWest", "Shipwreck") => new Point(58, 92),
				("IslandWest", "TigerSlimeGrove") => new Point(38, 28),
				("IslandWest", _) => new Point(77, 40),
				_ => new Point(-1, -1)
			};
		}

		private static Point GetTilePointSVE(string locationName, string id, Point value)
		{
			return (locationName, id) switch
			{
				("Forest", "WizardTower") => new Point(9, 21),
				("Custom_AdventurerSummit", "AdventureGuild") => new Point(32, 22),
				("Custom_AdventurerSummit", "Mines") => new Point(19, 16),
				("Custom_GrandpasShedOutside", "FlashShifter.StardewValleyExpandedCP_Town_GrandpasShed") => new Point(22, 17),
				("Forest", "FlashShifter.StardewValleyExpandedCP_Town_Andys") => new Point(62, 67),
				("Forest", "FlashShifter.StardewValleyExpandedCP_Town_Fairhaven") => new Point(68, 73),
				("Custom_BlueMoonVineyard", "FlashShifter.StardewValleyExpandedCP_Town_SophiasHouse") => new Point(28, 32),
				("Custom_BlueMoonVineyard", "FlashShifter.StardewValleyExpandedCP_Town_Bluemoon") => new Point(28, 48),
				("Railroad", "FlashShifter.StardewValleyExpandedCP_Town_EmeraldFarm") => new Point(38, 51),
				("Town", "FlashShifter.StardewValleyExpandedCP_Town_JenkinsResidence") => new Point(59, 52),
				("Custom_Garden", "FlashShifter.StardewValleyExpandedCP_Town_OldCommunityGarden") => new Point(22, 27),
				("Custom_ShearwaterBridge", "FlashShifter.StardewValleyExpandedCP_Town_ShearwaterBridge") => new Point(30, 20),
				("Custom_GrampletonSuburbs", "Custom_ScarlettsHouse") => new Point(36, 17),
				("Custom_GrampletonSuburbsTrainStation", "Custom_GrampletonSuburbsTrainStation") => new Point(30, 15),
				("Custom_Highlands", "Custom_HighlandsOutpost") => new Point(129, 113),
				("Custom_HighlandsCavern", "Custom_HighlandsCavern") => new Point(120, 145),
				("Custom_CastleVillageOutpost", "Custom_CastleVillageOutpost") => new Point(31, 28),
				("Custom_CrimsonBadlands", "Custom_DesertRailway") => new Point(228, 84),
				("Custom_CrimsonBadlands", "Custom_CastleVillageEntrance") => new Point(147, 15),
				("Custom_IridiumQuarry", "Custom_IridiumQuarry") => new Point(68, 17),
				("Custom_TreasureCave", "Custom_TreasureCave") => new Point(10, 12),
				("Custom_FableReef", "Custom_FirstSlashGuild") => new Point(43, 44),
				_ => value
			};
		}

		private static Point GetTilePointES(string locationName, string id, Point value)
		{
			return (locationName, id) switch
			{
				("EastScarp_ClearingHouse", "JessieHouse") => new Point(24, 9),
				("EastScarp_ClearingHouse", "ScarpCrossing") => new Point(23, 20),
				("EastScarp_Village", "NoraandToriHouse") => new Point(4, 62),
				("EastScarp_Village", "AideenHouse") => new Point(16, 49),
				("EastScarp_Village", "ScarpeInn") => new Point(42, 66),
				("EastScarp_Village", "ScarpePond") => new Point(26, 72),
				("EastScarp_Village", "VetHouse") => new Point(14, 24),
				("EastScarp_Village", "JacobBarn") => new Point(14, 12),
				("EastScarp_Village", "LionsMane") => new Point(57, 5),
				("EastScarp_Village", "RodneysSilo") => new Point(88, 32),
				("EastScarp_Village", "PlumFarm") => new Point(73, 43),
				("EastScarp_Village", "Underscarp") => new Point(44, 86),
				("EastScarp_Village", "Tidepools") => new Point(50, 112),
				("EastScarp_Village", "Lighthouse") => new Point(71, 118),
				("EastScarp_Village", "MateoHome") => new Point(93, 94),
				("EastScarp_MineEntrance", "OrchardHouse") => new Point(22, 10),
				("EastScarp_Orchard", "OrchardHouse") => new Point(33, 14),
				("EastScarp_Orchard", "SmugglerShack") => new Point(18, 65),
				_ => value
			};
		}

		private static Point GetHomeTilePoint()
		{
			string cabinName = Game1.player.homeLocation.Value;

			if (!Context.IsMainPlayer && cabinName is not null)
			{
				foreach (Building building in Game1.getFarm().buildings)
				{
					if (building.indoors.Value?.uniqueName.Value == cabinName)
					{
						int tileX = building.tileX.Value + building.humanDoor.X;
						int tileY = building.tileY.Value + building.humanDoor.Y + 1;

						return new Point(tileX, tileY);
					}
				}
			}
			return Game1.getFarm().GetMainFarmHouseEntry();
		}
	}
}
