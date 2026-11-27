using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace CraftAndBuildFromContainers
{
	public partial class ModEntry
	{
		public static List<IInventory> cachedContainers;

		private static bool IsValidToPull()
		{
			if (Config.EnableEverywhere)
				return true;
			if (Config.EnableForShopTrading && Game1.activeClickableMenu is ShopMenu)
				return true;
			if (Config.EnableForCrafting && Game1.activeClickableMenu is GameMenu && (Game1.activeClickableMenu as GameMenu).GetCurrentPage() is CraftingPage)
				return true;
			if (Config.EnableForBuilding && Game1.activeClickableMenu is CarpenterMenu)
				return true;
			return false;
		}

		public static List<IInventory> GetContainers()
		{
			//if (cachedContainers is not null)
				//return cachedContainers;

			List<IInventory> list = new();

			void AddContainersFromLocation(GameLocation location)
			{
				if (location is Farm farm && Config.IncludeShippingBin)
				{
					if (Config.UnrestrictedShippingBin)
					{
						list.Add(farm.getShippingBin(Game1.player));
					}
					else
					{
						list.Add(new LastShippedInventory());
					}
				}
				if (location is FarmHouse && Config.IncludeFridge)
				{
					FarmHouse farmhouse = location as FarmHouse;

					if (farmhouse.upgradeLevel > 0)
					{
						list.Add(farmhouse.fridge.Value.GetItemsForPlayer());
					}
				}
				foreach (Object obj in location.objects.Values)
				{
					if (obj is not null)
					{
						if (obj is Chest objAsChest && objAsChest.playerChest.Value && objAsChest.CanBeGrabbed && (!objAsChest.fridge.Value || Config.IncludeMiniFridges) && (objAsChest.SpecialChestType != Chest.SpecialChestTypes.MiniShippingBin || Config.IncludeMiniShippingBins) && (objAsChest.SpecialChestType != Chest.SpecialChestTypes.JunimoChest || Config.IncludeJunimoChests))
						{
							list.Add(objAsChest.GetItemsForPlayer());
						}
						else if (obj.heldObject.Value is Chest objHeldObjectAsChest && Config.IncludeAutoGrabbers)
						{
							list.Add(objHeldObjectAsChest.GetItemsForPlayer());
						}
					}
				}
			}

			foreach (GameLocation location in Game1.locations)
			{
				AddContainersFromLocation(location);
				if (location.IsBuildableLocation())
				{
					foreach (Building building in location.buildings)
					{
						if (building.indoors.Value is not null)
						{
							AddContainersFromLocation(building.indoors.Value);
						}
					}
				}
			}
			cachedContainers = list;
			return cachedContainers;
		}

		#pragma warning disable CS0618
		private static bool DoesFarmerHaveIngredientsInInventoryOrContainers(List<KeyValuePair<string, int>> recipeList, IList<Item> extraToCheck = null)
		{
			foreach (KeyValuePair<string, int> recipe in recipeList)
			{
				int value = recipe.Value;

				value -= Game1.player.getItemCount(recipe.Key);
				if (value <= 0)
				{
					continue;
				}
				if (extraToCheck != null)
				{
					value -= Game1.player.getItemCountInList(extraToCheck, recipe.Key);
					if (value <= 0)
					{
						continue;
					}
				}
				value -= GetItemCountInContainers(recipe.Key);
				if (value <= 0)
				{
					continue;
				}
				return false;
			}
			return true;
		}

		private static int GetItemCountInContainers(string itemId)
		{
			List<IInventory> containers = GetContainers();
			int value = 0;

			foreach (IInventory items in containers)
			{
				value += Game1.player.getItemCountInList(items, itemId);
			}
			return value;
		}

		public static void AddAdditionalCraftingItemsFromContainers(List<KeyValuePair<string, int>> recipeList, ref IList<Item> additionalCraftingItems)
		{
			List<IInventory> containers = GetContainers();

			additionalCraftingItems ??= new List<Item>();
			foreach (KeyValuePair<string, int> recipe in recipeList)
			{
				foreach (IInventory items in containers)
				{
					int value = Game1.player.getItemCountInList(items, recipe.Key);

					if (value <= 0)
					{
						continue;
					}
					additionalCraftingItems.Add(new Object(recipe.Key, value));
				}
			}
		}
		#pragma warning restore CS0618

		private static void ConsumeIngredientsFromInventoryOrContainers(List<KeyValuePair<string, int>> recipeItems, List<IInventory> additionalMaterials = null)
		{
			foreach (KeyValuePair<string, int> recipe in recipeItems)
			{
				int value = recipe.Value;

				value -= ConsumeFromPlayerInventory(recipe.Key, value);
				if (value <= 0)
				{
					continue;
				}
				if (additionalMaterials != null)
				{
					value -= ConsumeFromAdditionalMaterials(additionalMaterials, recipe.Key, value);
					if (value <= 0)
					{
						continue;
					}
				}
				value -= ConsumeFromContainers(recipe.Key, value);
				if (value <= 0)
				{
					continue;
				}
				SMonitor.Log($"Missing {value} of item {recipe.Key}.");
			}
			ClearShippingBinNulls();
		}

		private static int ConsumeFromPlayerInventory(string itemId, int count)
		{
			itemId = ItemRegistry.QualifyItemId(itemId);
			if (itemId == "(O)73")
			{
				Game1.netWorldState.Value.GoldenWalnuts = Math.Max(0, Game1.netWorldState.Value.GoldenWalnuts - count);
				return count;
			}
			else if (itemId == "(O)858")
			{
				Game1.player.QiGems = Math.Max(0, Game1.player.QiGems - count);
				return count;
			}
			else
			{
				return Game1.player.Items.ReduceId(itemId, count);
			}
		}

		private static int ConsumeFromAdditionalMaterials(List<IInventory> additionalMaterials, string itemId, int count)
		{
			int consumedCount = 0;

			foreach (IInventory items in additionalMaterials)
			{
				int value = items.ReduceId(itemId, count);

				consumedCount += value;
				count -= value;
				if (count <= 0)
				{
					break;
				}
			}
			return consumedCount;
		}

		private static int ConsumeFromContainers(string itemId, int count)
		{
			List<IInventory> containers = GetContainers();
			int consumedCount = 0;

			foreach (IInventory items in containers)
			{
				int value = (items as LastShippedInventory)?.ReduceId(itemId, count) ?? items.ReduceId(itemId, count);

				consumedCount += value;
				count -= value;
				if (count <= 0)
				{
					break;
				}
			}
			return consumedCount;
		}

		private static void ClearShippingBinNulls()
		{
			IInventory shippingBin = Game1.getFarm().getShippingBin(Game1.player);

			for (int i = shippingBin.Count - 1; i >= 0; i--)
			{
				if (shippingBin[i] is null || shippingBin[i].Stack <= 0)
				{
					shippingBin.RemoveAt(i);
				}
			}
		}
	}
}
