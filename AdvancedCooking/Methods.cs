using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace AdvancedCooking
{
	public partial class ModEntry
	{
		private static readonly PerScreen<List<IInventory>> containers = new(() => null);
		private static readonly PerScreen<ClickableTextureComponent> fridgeLeftButton = new(() => null);
		private static readonly PerScreen<ClickableTextureComponent> fridgeRightButton = new(() => null);
		private static readonly PerScreen<ClickableTextureComponent> cookButton = new(() => null);
		private static readonly PerScreen<int> fridgeIndex = new(() => 0);
		private static readonly PerScreen<InventoryMenu> ingredientMenu = new(() => null);
		private static readonly PerScreen<bool> canCook = new(() => false);
		private static readonly PerScreen<Dictionary<string, (Item, int)>> remainingIngredients = new(() => null);
		private static readonly PerScreen<Dictionary<CraftingRecipe, (Item, int)>> cookableRecipes = new(() => null);

		internal static List<IInventory> Containers
		{
			get => containers.Value;
			set => containers.Value = value;
		}

		internal static ClickableTextureComponent FridgeLeftButton
		{
			get => fridgeLeftButton.Value;
			set => fridgeLeftButton.Value = value;
		}

		internal static ClickableTextureComponent FridgeRightButton
		{
			get => fridgeRightButton.Value;
			set => fridgeRightButton.Value = value;
		}

		internal static ClickableTextureComponent CookButton
		{
			get => cookButton.Value;
			set => cookButton.Value = value;
		}

		internal static int FridgeIndex
		{
			get => fridgeIndex.Value;
			set => fridgeIndex.Value = value;
		}

		internal static InventoryMenu IngredientMenu
		{
			get => ingredientMenu.Value;
			set => ingredientMenu.Value = value;
		}

		internal static bool CanCook
		{
			get => canCook.Value;
			set => canCook.Value = value;
		}

		internal static Dictionary<string, (Item, int)> RemainingIngredients
		{
			get => remainingIngredients.Value;
			set => remainingIngredients.Value = value;
		}

		internal static Dictionary<CraftingRecipe, (Item, int)> CookableRecipes
		{
			get => cookableRecipes.Value;
			set => cookableRecipes.Value = value;
		}

		private static void Reset()
		{
			Containers = null;
			FridgeLeftButton = null;
			FridgeRightButton = null;
			CookButton = null;
			FridgeIndex = -1;
			IngredientMenu = null;
			CanCook = false;
			RemainingIngredients = null;
			CookableRecipes = null;
		}

		private static void Initialize(CraftingPage __instance, int x, ref int y, List<IInventory> materialContainers, ref int height)
		{
			y -= 86;
			height += 172;
			Containers = materialContainers;
			if (Containers is not null && Containers.Count > 0)
			{
				FridgeLeftButton = new ClickableTextureComponent("FridgeLeft", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, y + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Config.YOffset - 36, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = 631,
					upNeighborID = 24,
					leftNeighborID = -99998,
					rightNeighborID = 632,
					downNeighborID = -99998
				};
				FridgeRightButton = new ClickableTextureComponent("FridgeRight", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 318, y + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Config.YOffset - 36, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 632,
					upNeighborID = 29,
					leftNeighborID = 631,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
			}
			IngredientMenu = new InventoryMenu(x + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, y + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Config.YOffset + 84, false, new Item[11], InventoryMenu.highlightAllItems, 11, 1, 0, 0, true);
			CookButton = new ClickableTextureComponent(new Rectangle(x + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 * 11 + 8, y + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Config.YOffset + 80, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				myID = 102,
				upNeighborID = 35,
				rightNeighborID = -99998,
				leftNeighborID = 46,
				downNeighborID = -99998
			};
			UpdateCookableRecipes();
			__instance.behaviorBeforeCleanup = HandleBeforeCleanup;
		}

		private static void AttachClickableComponents(CraftingPage __instance)
		{
			if (FridgeLeftButton is not null && FridgeRightButton is not null)
			{
				__instance.allClickableComponents.Add(FridgeLeftButton);
				__instance.allClickableComponents.Add(FridgeRightButton);
			}
			for (int i = 0; i < IngredientMenu.inventory.Count; i++)
			{
				IngredientMenu.inventory[i].myID = 36 + i;
				__instance.allClickableComponents.Add(IngredientMenu.inventory[i]);
			}
			__instance.allClickableComponents.Add(CookButton);
		}

		private static void AssignClickableComponentNeighbors(CraftingPage __instance)
		{
			foreach (ClickableComponent clickableComponent in __instance.allClickableComponents)
			{
				if (clickableComponent.myID == 24)
				{
					clickableComponent.downNeighborID = FridgeLeftButton is not null ? 631 : clickableComponent.myID + 12;
					clickableComponent.downNeighborImmutable = true;
				}
				if (clickableComponent.myID == 29)
				{
					clickableComponent.downNeighborID = FridgeRightButton is not null ? 632 : clickableComponent.myID + 12;
					clickableComponent.downNeighborImmutable = true;
				}
				if ((25 <= clickableComponent.myID && clickableComponent.myID <= 28) || (30 <= clickableComponent.myID && clickableComponent.myID <= 34))
				{
					clickableComponent.downNeighborID = clickableComponent.myID + 12;
					clickableComponent.downNeighborImmutable = true;
				}
				if (clickableComponent.myID == 35)
				{
					clickableComponent.downNeighborID = 102;
					clickableComponent.downNeighborImmutable = true;
				}
				if (clickableComponent.myID == 36)
				{
					clickableComponent.upNeighborID = FridgeLeftButton is not null ? 631 : clickableComponent.myID - 12;
					clickableComponent.upNeighborImmutable = true;
				}
				if (clickableComponent.myID == 41)
				{
					clickableComponent.upNeighborID = FridgeLeftButton is not null ? 632 : clickableComponent.myID - 12;
					clickableComponent.upNeighborImmutable = true;
				}
				if ((37 <= clickableComponent.myID && clickableComponent.myID <= 40) || (42 <= clickableComponent.myID && clickableComponent.myID <= 46))
				{
					clickableComponent.upNeighborID = clickableComponent.myID - 12;
					clickableComponent.upNeighborImmutable = true;
				}
				if (clickableComponent.myID == 36)
				{
					clickableComponent.rightNeighborID = clickableComponent.myID + 1;
					clickableComponent.rightNeighborImmutable = true;
				}
				if (37 <= clickableComponent.myID && clickableComponent.myID <= 45)
				{
					clickableComponent.rightNeighborID = clickableComponent.myID + 1;
					clickableComponent.rightNeighborImmutable = true;
					clickableComponent.leftNeighborID = clickableComponent.myID - 1;
					clickableComponent.leftNeighborImmutable = true;
				}
				if (clickableComponent.myID == 46)
				{
					clickableComponent.rightNeighborID = 102;
					clickableComponent.rightNeighborImmutable = true;
					clickableComponent.leftNeighborID = clickableComponent.myID - 1;
					clickableComponent.leftNeighborImmutable = true;
				}
				if (clickableComponent.myID == 107)
				{
					clickableComponent.downNeighborID = 36;
					clickableComponent.downNeighborImmutable = true;
				}
				if (clickableComponent.myID == 106)
				{
					clickableComponent.downNeighborID = 102;
					clickableComponent.downNeighborImmutable = true;
				}
			}
		}

		private static void UpdateActualInventory(CraftingPage __instance)
		{
			IInventory list = FridgeIndex < 0 ? Game1.player.Items : __instance._materialContainers[Math.Min(__instance._materialContainers.Count - 1 ,FridgeIndex)];

			for (int i = 0; i < Game1.player.maxItems.Value; i++)
			{
				if (list.Count <= i)
				{
					list.Add(null);
				}
			}
			__instance.inventory.actualInventory = list;
			__instance.inventory.highlightMethod = item => item is not Tool;
		}

		private static void TryCook(CraftingPage __instance, ref Item heldItem)
		{
			bool isFail = CookableRecipes.Count <= 0;
			int seasoningQuantity = Math.Min(CookableRecipes.Sum(recipe => recipe.Value.Item2), IngredientMenu.actualInventory.Where(ingredient => ingredient is not null && ingredient.QualifiedItemId.Equals("(O)917")).Sum(seasoning => seasoning.Stack));
			bool seasoned = seasoningQuantity > 0;
			List<string> newRecipesLearned = new();

			if (!isFail && !SHelper.Input.IsDown(Config.CookAllModKey))
			{
				(CraftingRecipe recipe, (Item item, _)) = CookableRecipes.First();

				RemainingIngredients = GetIngredients();
				foreach ((string recipeKey, int requiredQuantity) in recipe.recipeList)
				{
					foreach ((string ingredientKey, (Item ingredient, int quantity)) in RemainingIngredients)
					{
						if (IsMatchingIngredient(ingredient, recipeKey))
						{
							int remainingQuantity = Math.Max(0, quantity - requiredQuantity);
							int remainingRequired = requiredQuantity - (quantity - remainingQuantity);

							RemainingIngredients[ingredientKey] = (ingredient, remainingQuantity);
							recipe.recipeList[recipeKey] = remainingRequired;
						}
					}
				}
				CookableRecipes = new()
				{
					{ recipe, (new Object(item.ItemId, 1), 1) }
				};
				seasoningQuantity = Math.Min(seasoningQuantity, 1);
			}
			if (isFail ? Config.ConsumeIngredientsOnFail : Config.ConsumeExtraIngredientsOnSucceed)
			{
				IngredientMenu.actualInventory = new Item[11];
			}
			else
			{
				foreach (string key in RemainingIngredients.Keys)
				{
					if (key.Equals("(O)917"))
					{
						RemainingIngredients[key] = (RemainingIngredients[key].Item1, RemainingIngredients[key].Item2 - seasoningQuantity);
						break;
					}
				}
				for (int i = 0; i < IngredientMenu.actualInventory.Count; i++)
				{
					if (IngredientMenu.actualInventory[i] is not null && RemainingIngredients.ContainsKey(IngredientMenu.actualInventory[i].QualifiedItemId))
					{
						IngredientMenu.actualInventory[i].Stack = Math.Min(RemainingIngredients[IngredientMenu.actualInventory[i].QualifiedItemId].Item2, IngredientMenu.actualInventory[i].maximumStackSize());
						RemainingIngredients[IngredientMenu.actualInventory[i].QualifiedItemId] = (RemainingIngredients[IngredientMenu.actualInventory[i].QualifiedItemId].Item1, RemainingIngredients[IngredientMenu.actualInventory[i].QualifiedItemId].Item2 - IngredientMenu.actualInventory[i].Stack);
						if (IngredientMenu.actualInventory[i].Stack <= 0)
						{
							IngredientMenu.actualInventory[i] = null;
						}
					}
				}
			}
			if (isFail && Config.ConsumeIngredientsOnFail && Config.GiveTrashOnFail)
			{
				CookableRecipes = new()
				{
					{ new CraftingRecipe(string.Empty, true), (new Object("168", 1), 1) }
				};
			}
			foreach ((CraftingRecipe recipe, (Item item, int quantity)) in CookableRecipes)
			{
				int remainingQuantity = quantity;

				while (remainingQuantity > 0)
				{
					int stackQuantity = Math.Min(seasoningQuantity > 0 ? Math.Min(seasoningQuantity, remainingQuantity) : remainingQuantity, item.maximumStackSize());
					Object obj = seasoningQuantity > 0 ? new(item.ItemId, stackQuantity, false, -1, 2) : new(item.ItemId, stackQuantity);
					bool canStackWith = heldItem is not null && heldItem.getOne().canStackWith(obj);
					bool canStackWithinMaxStackSize = canStackWith && heldItem.Stack + obj.Stack <= heldItem.maximumStackSize();

					if (canStackWith && !canStackWithinMaxStackSize && heldItem.Stack != heldItem.maximumStackSize())
					{
						stackQuantity = heldItem.maximumStackSize() - heldItem.Stack;
						obj = new(item.ItemId, stackQuantity);
						canStackWith = heldItem is not null && heldItem.getOne().canStackWith(obj);
						canStackWithinMaxStackSize = canStackWith && heldItem.Stack + obj.Stack <= heldItem.maximumStackSize();
					}

					bool canHold = heldItem is null || canStackWithinMaxStackSize;

					if (Config.HoldCookedItem && !canHold)
					{
						heldItem = Utility.addItemToThisInventoryList(heldItem, __instance.inventory.actualInventory, 36);
						canStackWith = heldItem is not null && heldItem.getOne().canStackWith(obj);
						canStackWithinMaxStackSize = canStackWith && heldItem.Stack + obj.Stack <= heldItem.maximumStackSize();
						canHold = heldItem is null || canStackWithinMaxStackSize;
					}
					if (Config.HoldCookedItem && canHold)
					{
						if (canStackWithinMaxStackSize)
						{
							heldItem.Stack += obj.Stack;
						}
						else
						{
							heldItem = obj;
						}
					}
					else
					{
						AddItemToThisInventoryListOrDrop(obj, __instance.inventory.actualInventory, 36);
					}
					if (!isFail)
					{
						if (Config.LearnUnknownRecipes && !Game1.player.cookingRecipes.ContainsKey(recipe.name))
						{
							Game1.player.cookingRecipes.Add(recipe.name, 0);
							newRecipesLearned.Add(recipe.DisplayName);
						}
						Game1.player.NotifyQuests(quest => quest.OnRecipeCrafted(recipe, obj));
						if (!Game1.player.recipesCooked.TryGetValue(obj.ItemId, out int value))
						{
							value = 0;
						}
						Game1.player.recipesCooked[obj.ItemId] = value + obj.Stack;
					}
					seasoningQuantity -= obj.Stack;
					remainingQuantity -= stackQuantity;
				}
			}
			if (isFail)
			{
				Game1.addHUDMessage(new HUDMessage(SHelper.Translation.Get("cooking-failed"), 3));
				Game1.playSound("cancel");
			}
			else
			{
				if (newRecipesLearned.Count > 0)
				{
					foreach (string recipeDisplayName in newRecipesLearned)
					{
						Game1.showGlobalMessage(string.Format(SHelper.Translation.Get("new-recipe-x"), recipeDisplayName));
					}
					Game1.playSound("yoba");
					__instance.RepositionElements();
				}
				if (seasoned)
				{
					Game1.playSound("breathin");
				}
				Game1.playSound("coin");
			}
			Game1.stats.checkForCookingAchievements();
			UpdateCookableRecipes();
		}

		private static void UpdateCookableRecipes()
		{
			if (Game1.activeClickableMenu is not CraftingPage)
			{
				CookableRecipes = new();
				return;
			}
			CanCook = IngredientMenu.actualInventory.Any(ingredient => ingredient is not null);
			(CookableRecipes, RemainingIngredients) = CalculateCookables(GetIngredients());
		}

		private static Dictionary<string, (Item, int)> GetIngredients()
		{
			Dictionary<string, (Item, int)> ingredients = new();

			foreach (Item item in IngredientMenu.actualInventory)
			{
				if (item is not null)
				{
					if (!ingredients.ContainsKey(item.QualifiedItemId))
					{
						ingredients.Add(item.QualifiedItemId, (item, item.Stack));
					}
					else
					{
						ingredients[item.QualifiedItemId] = (ingredients[item.QualifiedItemId].Item1, ingredients[item.QualifiedItemId].Item2 + item.Stack);
					}
				}
			}
			return ingredients;
		}

		private static (Dictionary<CraftingRecipe, (Item, int)>, Dictionary<string, (Item, int)>) CalculateCookables(Dictionary<string, (Item, int)> ingredients)
		{
			// Filter and group the possible recipes by the number of required ingredients
			IEnumerable<IGrouping<int, CraftingRecipe>> groupedRecipes = CraftingRecipe.cookingRecipes
				.Select(recipe => new CraftingRecipe(recipe.Key, true))
				.Where(recipe => (Config.AllowUnknownRecipes || Game1.player.cookingRecipes.ContainsKey(recipe.name)) && recipe.recipeList.All(req => ingredients
					.Where(pair => IsMatchingIngredient(pair.Value.Item1, req.Key))
					.Sum(pair => pair.Value.Item2) >= req.Value))
				.GroupBy(recipe => recipe.recipeList.Count)
				.OrderByDescending(group => group.Key);
			Dictionary<CraftingRecipe, (Item, int)> result = new();

			// Iterate over each group of recipes
			foreach (IGrouping<int, CraftingRecipe> group in groupedRecipes)
			{
				// List where each sublist contains recipes sharing common ingredients
				List<List<CraftingRecipe>> ingredientGroups = new();

				foreach (CraftingRecipe recipe in group)
				{
					bool addedToExistingGroup = false;

					// Check if the recipe shares any common ingredients with existing groups
					foreach (List<CraftingRecipe> ingredientGroup in ingredientGroups)
					{
						if (ingredientGroup.Any(r => recipe.recipeList.Keys.Intersect(r.recipeList.Keys).Any()))
						{
							ingredientGroup.Add(recipe);
							addedToExistingGroup = true;
							break;
						}
					}
					// If the recipe doesn't share ingredients with any group, create a new group
					if (!addedToExistingGroup)
					{
						ingredientGroups.Add(new List<CraftingRecipe> { recipe });
					}
				}
				// Calculate the maximum number of dishes for each ingredient group
				foreach (List<CraftingRecipe> ingredientGroup in ingredientGroups)
				{
					Dictionary<string, int> totalGroupRequirements = new();

					// Combine the ingredient requirements for all recipes in the group
					foreach (CraftingRecipe recipe in ingredientGroup)
					{
						foreach (KeyValuePair<string, int> ingredient in recipe.recipeList)
						{
							if (totalGroupRequirements.ContainsKey(ingredient.Key))
							{
								totalGroupRequirements[ingredient.Key] += ingredient.Value;
							}
							else
							{
								totalGroupRequirements[ingredient.Key] = ingredient.Value;
							}
						}
					}

					int maxGroupCraftable = int.MaxValue;

					// Calculate the maximum number of dishes that can be cooked for the group
					foreach (KeyValuePair<string, int> requirement in totalGroupRequirements)
					{
						int availableQuantity = ingredients
							.Where(pair => IsMatchingIngredient(pair.Value.Item1, requirement.Key))
							.Sum(pair => pair.Value.Item2);

						if (availableQuantity < requirement.Value)
						{
							maxGroupCraftable = 0;
							break;
						}

						int maxForIngredient = availableQuantity / requirement.Value;

						if (maxForIngredient < maxGroupCraftable)
						{
							maxGroupCraftable = maxForIngredient;
						}
					}

					void CalculateMaximumDishesForGroupRecipes(int max)
					{
						// Process each recipe in the group and calculate how many dishes can be cooked
						foreach (CraftingRecipe recipe in ingredientGroup)
						{
							int maxCraftable = max;

							foreach (KeyValuePair<string, int> ingredient in recipe.recipeList)
							{
								int available = ingredients
									.Where(pair => IsMatchingIngredient(pair.Value.Item1, ingredient.Key))
									.Sum(pair => pair.Value.Item2);
								int craftable = available / ingredient.Value;

								maxCraftable = Math.Min(maxCraftable, craftable);
							}
							// If the recipe can be cooked, add it to the result and update available ingredients
							if (maxCraftable > 0)
							{
								Item item = recipe.createItem();

								// Subtract ingredients used from available ingredients
								foreach (KeyValuePair<string, int> ingredient in recipe.recipeList)
								{
									int needed = ingredient.Value * maxCraftable;

									foreach (KeyValuePair<string, (Item, int)> pair in ingredients.Where(pair => IsMatchingIngredient(pair.Value.Item1, ingredient.Key)).ToList())
									{
										if (pair.Value.Item2 >= needed)
										{
											ingredients[pair.Key] = (pair.Value.Item1, pair.Value.Item2 - needed);
											break;
										}
										else
										{
											needed -= pair.Value.Item2;
											ingredients[pair.Key] = (pair.Value.Item1, 0);
										}
									}
								}
								result[recipe] = result.ContainsKey(recipe) ? (result[recipe].Item1, result[recipe].Item2 + maxCraftable) : (item, maxCraftable);
							}
						}
					}

					CalculateMaximumDishesForGroupRecipes(maxGroupCraftable);
					CalculateMaximumDishesForGroupRecipes(int.MaxValue);
				}
			}
			return (result, ingredients);
		}

		private static bool IsMatchingIngredient(Item item, string key)
		{
			if (item is not Object obj || obj.bigCraftable.Value)
			{
				return false;
			}
			return obj.ItemId.ToString() == key || obj.Category.ToString() == key || CraftingRecipe.isThereSpecialIngredientRule(obj, key);
		}

		private static bool AddItemToThisInventoryListOrDrop(Item item, IList<Item> list, int listMaxSpace = -1)
		{
			if (item is not null)
			{
				item = Utility.addItemToThisInventoryList(item, list, listMaxSpace);
				if (item is not null)
				{
					Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
					return false;
				}
			}
			return true;
		}

		private static void HandleBeforeCleanup(IClickableMenu __instance)
		{
			if (__instance is not CraftingPage craftingPage || !craftingPage.cooking)
				return;

			if (craftingPage.heldItem is not null)
			{
				AddItemToThisInventoryListOrDrop(craftingPage.heldItem, craftingPage.inventory.actualInventory, 36);
			}
			foreach (Item ingredient in IngredientMenu.actualInventory)
			{
				if (ingredient is not null)
				{
					AddItemToThisInventoryListOrDrop(ingredient, craftingPage.inventory.actualInventory, 36);
				}
			}
			IngredientMenu.actualInventory = null;
		}
	}
}
