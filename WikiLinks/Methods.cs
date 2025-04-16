using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.Objects;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace WikiLinks
{
	public partial class ModEntry
	{
		private static readonly HttpClient httpClient = new();

		public static bool ReceiveOpenWikiPageKeys()
		{
			GameLocation location = Game1.currentLocation;

			if (Game1.activeClickableMenu is not null)
			{
				if (TryOpenPageFromMenu())
					return true;
			}
			else
			{
				if (TryOpenPageFromObject(location))
					return true;
				if (TryOpenPageFromTerrainFeature(location))
					return true;
				if (TryOpenPageFromFurniture(location))
					return true;
				if (TryOpenPageFromNPC(location))
					return true;
				if (TryOpenPageFromFarmAnimal(location))
					return true;
				if (TryOpenPageFromBuilding(location))
					return true;
			}
			return false;
		}

		private static bool TryOpenPageFromMenu()
		{
			if (Game1.activeClickableMenu is InventoryMenu inventoryMenu)
			{
				return TryOpenPageFromInventory(inventoryMenu);
			}
			else if (Game1.activeClickableMenu is StorageContainer storageContainer)
			{
				return TryOpenPageFromInventory(storageContainer.ItemsToGrabMenu) || TryOpenPageFromInventory(storageContainer.inventory);
			}
			else if (Game1.activeClickableMenu is JunimoNoteMenu junimoNoteMenu)
			{
				return TryOpenPageFromInventory(junimoNoteMenu.inventory);
			}
			else if (Game1.activeClickableMenu is QuestContainerMenu questContainerMenu)
			{
				return TryOpenPageFromInventory(questContainerMenu.ItemsToGrabMenu) || TryOpenPageFromInventory(questContainerMenu.inventory);
			}
			else if (Game1.activeClickableMenu is ShopMenu shopMenu)
			{
				return TryOpenPageFromInventory(shopMenu.inventory);
			}
			else if (Game1.activeClickableMenu is ItemGrabMenu itemGrabMenu)
			{
				return TryOpenPageFromInventory(itemGrabMenu.ItemsToGrabMenu) || TryOpenPageFromInventory(itemGrabMenu.inventory);
			}
			else if (Game1.activeClickableMenu is MenuWithInventory menuWithInventory)
			{
				return TryOpenPageFromInventory(menuWithInventory.inventory);
			}
			else if (Game1.activeClickableMenu is CraftingPage craftingPage)
			{
				return TryOpenPageFromInventory(craftingPage.inventory);
			}
			else if (Game1.activeClickableMenu is InventoryPage inventoryPage)
			{
				return TryOpenPageFromInventory(inventoryPage.inventory);
			}
			else if (Game1.activeClickableMenu is GameMenu gameMenu && (gameMenu.currentTab == GameMenu.inventoryTab || gameMenu.currentTab == GameMenu.craftingTab))
			{
				if (gameMenu.currentTab == GameMenu.inventoryTab)
				{
					return TryOpenPageFromInventory((gameMenu.pages[GameMenu.inventoryTab] as InventoryPage).inventory);
				}
				else
				{
					return TryOpenPageFromInventory((gameMenu.pages[GameMenu.craftingTab] as CraftingPage).inventory);
				}
			}
			return true;
		}

		private static bool TryOpenPageFromInventory(InventoryMenu __instance)
		{
			foreach (ClickableComponent clickableComponent in __instance.inventory)
			{
				if (clickableComponent.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
				{
					int slotNumber = Convert.ToInt32(clickableComponent.name);

					if (slotNumber < __instance.actualInventory.Count)
					{
						Item item = __instance.actualInventory[slotNumber];

						if (item is not null)
						{
							if (item.TypeDefinitionId == ItemRegistry.type_object && Game1.objectData.TryGetValue(item.ItemId, out ObjectData objectData))
							{
								OpenPage(objectData.Name);
							}
							else
							{
								OpenPage(item.Name);
							}
							return true;
						}
					}
				}
			}
			return false;
		}

		private static bool TryOpenPageFromObject(GameLocation location)
		{
			if (location.objects.TryGetValue(Game1.currentCursorTile, out Object @object) || location.objects.TryGetValue(Game1.currentCursorTile + new Vector2(0, 1), out @object) && @object.bigCraftable.Value)
			{
				OpenPageForObject(@object);
				return true;
			}
			return false;
		}

		private static void OpenPageForObject(Object @object)
		{
			if (@object.IsBreakableStone())
			{
				string pageName = @object.ItemId switch
				{
					"44" => "Minerals",
					"95" => new Object("909", 1).Name,
					"843" or "844" => new Object("848", 1).Name,
					"25" => new Object("719", 1).Name,
					"75" => new Object("535", 1).Name,
					"76" => new Object("536", 1).Name,
					"56" or "58" or "77" => new Object("537", 1).Name,
					"816" or "817" => new Object("881", 1).Name,
					"818" => new Object("330", 1).Name,
					"819" => new Object("749", 1).Name,
					"8" => new Object("66", 1).Name,
					"10" => new Object("68", 1).Name,
					"12" => new Object("60", 1).Name,
					"14" => new Object("62", 1).Name,
					"6" => new Object("70", 1).Name,
					"4" => new Object("64", 1).Name,
					"2" => new Object("72", 1).Name,
					"845" or "846" or "847" or "670" or "668" => new Object("382", 1).Name,
					"849" or "751" => new Object("378", 1).Name,
					"850" or "290" => new Object("380", 1).Name,
					"BasicCoalNode0" or "BasicCoalNode1" or "VolcanoCoalNode0" or "VolcanoCoalNode1" => new Object("382", 1).Name,
					"VolcanoGoldNode" or "764" => new Object("384", 1).Name,
					"765" => new Object("386", 1).Name,
					"CalicoEggStone_0" or "CalicoEggStone_1" or "CalicoEggStone_2" => new Object("CalicoEgg", 1).Name,
					_ => @object.Name
				};

				OpenPage(pageName);
			}
			else
			{
				OpenPage(@object.Name);
			}
		}

		private static bool TryOpenPageFromTerrainFeature(GameLocation location)
		{
			if (location.terrainFeatures.TryGetValue(Game1.currentCursorTile, out TerrainFeature terrainFeature))
			{
				if (terrainFeature is Tree tree)
				{
					OpenPageForTree(tree);
					return true;
				}
				if (terrainFeature is FruitTree fruitTree)
				{
					OpenPageForFruitTree(fruitTree);
					return true;
				}
				if (terrainFeature is HoeDirt hoeDirt && hoeDirt.crop?.indexOfHarvest?.Value is not null)
				{
					OpenPage(new Object(hoeDirt.crop.indexOfHarvest.Value, 1).Name);
					return true;
				}
			}
			return false;
		}

		private static void OpenPageForTree(Tree tree)
		{
			string pageName = tree.treeType.Value switch
			{
				"1" => "Oak Tree",
				"2" => "Maple Tree",
				"3" => "Pine Tree",
				"6" or "9" => "Palm Tree",
				"7" => "Mushroom Tree",
				"8" => "Mahogany Tree",
				"10" or "11" or "12" => "Green Rain Trees",
				_ => null
			};

			if (pageName is not null)
			{
				OpenPage(pageName);
			}
		}

		private static void OpenPageForFruitTree(FruitTree fruitTree)
		{
			string pageName = fruitTree.treeId.Value switch
			{
				"628" => "Cherry Tree",
				"629" => "Apricot Tree",
				"630" => "Orange Tree",
				"631" => "Peach Tree",
				"632" => "Pomegranate Tree",
				"633" => "Apple Tree",
				"69" => "Banana Tree",
				"835" => "Mango Tree",
				_ => null
			};

			if (pageName is not null)
			{
				OpenPage(pageName);
			}
		}

		private static bool TryOpenPageFromFurniture(GameLocation location)
		{
			foreach (Furniture furniture in location.furniture)
			{
				if (furniture.GetBoundingBox().Contains(Game1.currentCursorTile * 64))
				{
					OpenPage(furniture.Name);
					return true;
				}
			}
			return false;
		}

		private static bool TryOpenPageFromNPC(GameLocation location)
		{
			foreach (NPC npc in location.characters)
			{
				if (npc.Tile == Game1.currentCursorTile || (npc.IsVillager && npc.Tile + new Vector2(0, -1) == Game1.currentCursorTile))
				{
					OpenPageForNPC(npc);
					return true;
				}
			}
			return false;
		}

		private static void OpenPageForNPC(NPC npc)
		{
			if (npc.IsVillager)
			{
				OpenPage(npc.Name);
			}
			else if (npc is Monster monster)
			{
				if (monster is GreenSlime)
				{
					OpenPage("Green Slime");
				}
				else
				{
					OpenPage(npc.Name);
				}
			}
			else if (npc is Pet || npc is Horse)
			{
				OpenPage("Animals");
			}
		}

		private static bool TryOpenPageFromFarmAnimal(GameLocation location)
		{
			foreach (FarmAnimal farmAnimal in location.animals.Values)
			{
				if (farmAnimal.Tile == Game1.currentCursorTile)
				{
					OpenPageForFarmAnimal(farmAnimal);
					return true;
				}
			}
			return false;
		}

		private static void OpenPageForFarmAnimal(FarmAnimal farmAnimal)
		{
			string pageName = farmAnimal.type.Value switch
			{
				"White Chicken" or "Brown Chicken" or "Blue Chicken" => "Chicken",
				"White Cow" or "Brown Cow" => "Cow",
				_ => farmAnimal.type.Value
			};

			OpenPage(pageName);
		}

		private static bool TryOpenPageFromBuilding(GameLocation location)
		{
			Building building = location.getBuildingAt(Game1.currentCursorTile);

			if (building != null)
			{
				OpenPage(building.buildingType.Value);
				return true;
			}
			return false;
		}

		public static async void OpenPage(string page)
		{
			string url = $"https://stardewvalleywiki.com/{page.Replace(' ', '_')}";
			(bool isSuccess, string content) = await FetchUrlContentAsync(url);

			if (isSuccess)
			{
				string localeCode = SHelper.Translation.Locale.Split('-').First();
				string localizedUrl = ExtractLocalizedUrl(content, localeCode) ?? url;

				OpenUrl(localizedUrl);
			}
		}

		private static async Task<(bool isSuccess, string content)> FetchUrlContentAsync(string url)
		{
			try
			{
				HttpResponseMessage response = await httpClient.GetAsync(url);
				string content = await response.Content.ReadAsStringAsync();

				return (response.IsSuccessStatusCode, content);
			}
			catch
			{
				return (false, null);
			}
		}

		private static string ExtractLocalizedUrl(string htmlContent, string localeCode)
		{
			const string regex = "<a href=\"([^\"]*)\"\\s+title=\"[^\"]*\"\\s+lang=\"([^\"]*)\"\\s+hreflang=\"([^\"]*)\"\\s+class=\"([^\"]*)\">";
			MatchCollection matches = Regex.Matches(htmlContent, regex);

			foreach (Match match in matches.Cast<Match>())
			{
				if (match.Groups.Count == 5)
				{
					string hrefValue = match.Groups[1].Value;
					string langValue = match.Groups[2].Value;
					string hreflangValue = match.Groups[3].Value;
					string classValue = match.Groups[4].Value;

					if (langValue.Equals(localeCode) && hreflangValue.Equals(localeCode) && classValue.Equals("interlanguage-link-target"))
					{
						return hrefValue;
					}
				}
			}
			return null;
		}

		private static void OpenUrl(string url)
		{
			SMonitor.Log($"Opening the wiki page: {url}");
			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = url,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error opening link: {ex.Message}");
			}
		}
	}
}
