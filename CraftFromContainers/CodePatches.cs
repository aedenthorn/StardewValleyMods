using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;

namespace CraftAndBuildFromContainers
{
	public partial class ModEntry
	{
		public class Inventory_ContainsId_Patch
		{
			public static void Postfix1(Inventory __instance, string itemId, ref bool __result)
			{
				Postfix(__instance, itemId, 1, ref __result);
			}

			public static void Postfix2(Inventory __instance, string itemId, int minimum, ref bool __result)
			{
				Postfix(__instance, itemId, minimum, ref __result);
			}

			private static void Postfix(Inventory __instance, string itemId, int minimum, ref bool __result)
			{
				if (!Config.ModEnabled || __result || __instance != Game1.player.Items || (Config.ToggleButton != SButton.None && !SHelper.Input.IsDown(Config.ToggleButton)) || !IsValidToPull() || itemId == "858" || itemId == "73")
					return;

				__result = DoesFarmerHaveIngredientsInInventoryOrContainers(new List<KeyValuePair<string, int>>() { new(itemId, minimum) });
			}
		}

		public class Inventory_CountId_Patch
		{
			public static void Postfix(Inventory __instance, string itemId, ref int __result)
			{
				if (!Config.ModEnabled || __instance != Game1.player.Items || (Config.ToggleButton != SButton.None && !SHelper.Input.IsDown(Config.ToggleButton)) || !IsValidToPull() || itemId == "858" || itemId == "73")
					return;

				__result += GetItemCountInContainers(itemId);
			}
		}

		public class CraftingRecipe_doesFarmerHaveIngredientsInInventory_Patch
		{
			public static void Postfix(CraftingRecipe __instance, IList<Item> extraToCheck, ref bool __result)
			{
				if (!Config.ModEnabled || __result || (Config.ToggleButton != SButton.None && !SHelper.Input.IsDown(Config.ToggleButton)) || !IsValidToPull())
					return;

				__result = DoesFarmerHaveIngredientsInInventoryOrContainers(__instance.recipeList.ToList(), extraToCheck);
			}
		}

		public class CraftingRecipe_DoesFarmerHaveAdditionalIngredientsInInventory_Patch
		{
			public static void Postfix(List<KeyValuePair<string, int>> additional_recipe_items, IList<Item> extraToCheck, ref bool __result)
			{
				if (!Config.ModEnabled || __result || (Config.ToggleButton != SButton.None && !SHelper.Input.IsDown(Config.ToggleButton)) || !IsValidToPull())
					return;

				__result = DoesFarmerHaveIngredientsInInventoryOrContainers(additional_recipe_items, extraToCheck);
			}
		}

		public class CarpenterMenu_DoesFarmerHaveEnoughResourcesToBuild_Patch
		{
			public static void Postfix(CarpenterMenu __instance, ref bool __result)
			{
				if (!Config.ModEnabled || __result || (Config.ToggleButton != SButton.None && !SHelper.Input.IsDown(Config.ToggleButton)) || !IsValidToPull())
					return;

				__result = DoesFarmerHaveIngredientsInInventoryOrContainers(__instance.ingredients.Select(item => new KeyValuePair<string, int>(item.ItemId, item.Stack)).ToList());
			}
		}

		public class CraftingRecipe_drawRecipeDescription_Patch
		{
			public static void Prefix(CraftingRecipe __instance, ref IList<Item> additional_crafting_items)
			{
				if (!Config.ModEnabled || (Config.ToggleButton != SButton.None && !SHelper.Input.IsDown(Config.ToggleButton)) || !IsValidToPull())
					return;

				AddAdditionalCraftingItemsFromContainers(__instance.recipeList.ToList(), ref additional_crafting_items);
			}
		}

		public class ShopMenu_ConsumeTradeItem_Patch
		{
			public static bool Prefix(string itemId, int count)
			{
				if (!Config.ModEnabled || !IsValidToPull())
					return true;

				ConsumeIngredientsFromInventoryOrContainers(new List<KeyValuePair<string, int>>() { new(itemId, count) });
				return false;
			}
		}

		public class CraftingRecipe_consumeIngredients_Patch
		{
			public static bool Prefix(CraftingRecipe __instance, List<IInventory> additionalMaterials)
			{
				if (!Config.ModEnabled || !IsValidToPull())
					return true;

				ConsumeIngredientsFromInventoryOrContainers(__instance.recipeList.ToList(), additionalMaterials);
				return false;
			}
		}

		public class CraftingRecipe_ConsumeAdditionalIngredients_Patch
		{
			public static bool Prefix(List<KeyValuePair<string, int>> additionalRecipeItems, List<IInventory> additionalMaterials)
			{
				if (!Config.ModEnabled || !IsValidToPull())
					return true;

				ConsumeIngredientsFromInventoryOrContainers(additionalRecipeItems, additionalMaterials);
				return false;
			}
		}

		public class CarpenterMenu_ConsumeResources_Patch
		{
			public static bool Prefix(CarpenterMenu __instance)
			{
				if (!Config.ModEnabled || !IsValidToPull())
					return true;

				ConsumeIngredientsFromInventoryOrContainers(__instance.ingredients.Select(item => new KeyValuePair<string, int>(item.ItemId, item.Stack)).ToList());
				Game1.player.Money -= __instance.currentBuilding.GetData().BuildCost;
				return false;
			}
		}
	}
}
