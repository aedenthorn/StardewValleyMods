using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Inventories;

namespace AdvancedCooking
{
	public partial class ModEntry
	{
		class IClickableMenu_populateClickableComponentList_Patch
		{
			public static void Postfix(IClickableMenu __instance)
			{
				if (!Config.ModEnabled || __instance is not CraftingPage craftingPage || !craftingPage.cooking)
					return;

				AttachClickableComponents(craftingPage);
				AssignClickableComponentNeighbors(craftingPage);
			}
		}

		class CraftingPage_Patch
		{
			public static void Prefix(CraftingPage __instance, int x, ref int y, bool cooking, List<IInventory> materialContainers, ref int height)
			{
				if (!Config.ModEnabled || !cooking)
					return;

				Reset();
				Initialize(__instance, x, ref y, materialContainers, ref height);
			}

			public static void Postfix(CraftingPage __instance, bool cooking)
			{
				if (!Config.ModEnabled || !cooking)
					return;

				AttachClickableComponents(__instance);
				AssignClickableComponentNeighbors(__instance);
				UpdateActualInventory(__instance);
			}
		}

		class CraftingPage_receiveLeftClick_Patch
		{
			public static void Prefix(CraftingPage __instance, ref Item ___heldItem, int x, int y)
			{
				if (!Config.ModEnabled || !__instance.cooking)
					return;

				if (CanCook && CookButton is not null && CookButton.containsPoint(x, y))
				{
					TryCook(__instance, ref ___heldItem);
					return;
				}
				if (__instance._materialContainers is not null && __instance._materialContainers.Count > 0)
				{
					if (FridgeLeftButton.containsPoint(x, y))
					{
						Game1.playSound("pickUpItem");
						FridgeIndex--;
						if (FridgeIndex < -1)
						{
							FridgeIndex = __instance._materialContainers.Count - 1;
						}
						UpdateActualInventory(__instance);
						return;
					}
					if (FridgeRightButton.containsPoint(x, y))
					{
						Game1.playSound("pickUpItem");
						FridgeIndex++;
						if (FridgeIndex >= __instance._materialContainers.Count)
						{
							FridgeIndex = -1;
						}
						UpdateActualInventory(__instance);
						return;
					}
				}
				if (IngredientMenu.isWithinBounds(x, y) && ___heldItem is not Tool)
				{
					___heldItem = IngredientMenu.leftClick(x, y, ___heldItem, true);
					UpdateCookableRecipes();
				}
			}
		}

		class CraftingPage_receiveRightClick_Patch
		{
			public static void Prefix(CraftingPage __instance, ref Item ___heldItem, int x, int y)
			{
				if (!Config.ModEnabled || !__instance.cooking)
					return;

				if (CanCook && CookButton is not null && CookButton.containsPoint(x, y))
				{
					TryCook(__instance, ref ___heldItem);
					return;
				}
				if (IngredientMenu.isWithinBounds(x, y) && ___heldItem is not Tool)
				{
					___heldItem = IngredientMenu.rightClick(x, y, ___heldItem, true);
					UpdateCookableRecipes();
				}
			}
		}

		class CraftingPage_gameWindowSizeChanged_Patch
		{
			public static void Postfix(CraftingPage __instance)
			{
				if (!Config.ModEnabled || !__instance.cooking)
					return;

				if (FridgeLeftButton is not null && FridgeRightButton is not null)
				{
					FridgeLeftButton.bounds = new Rectangle(__instance.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Config.YOffset - 36, 64, 64);
					FridgeRightButton.bounds = new Rectangle(__instance.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 318, __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Config.YOffset - 36, 64, 64);
				}
				IngredientMenu.SetPosition(__instance.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Config.YOffset + 84);
				CookButton.bounds = new Rectangle(__instance.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 * 11 + 8, __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Config.YOffset + 80, 64, 64);
				AssignClickableComponentNeighbors(__instance);
			}
		}

		class CraftingPage_emergencyShutDown_Patch
		{
			public static void Prefix(CraftingPage __instance)
			{
				if (!Config.ModEnabled || !__instance.cooking)
					return;

				HandleBeforeCleanup(__instance);
			}
		}

		class Game1_drawDialogueBox_Patch
		{
			public static void Postfix()
			{
				if (!Config.ModEnabled || Game1.activeClickableMenu is not CraftingPage craftingPage || !craftingPage.cooking)
					return;

				int xStart = Game1.activeClickableMenu.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth;
				int yStart = Game1.activeClickableMenu.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + Config.YOffset;
				string whichFridge = FridgeIndex >= 0 ? string.Format(SHelper.Translation.Get("fridge-x"), FridgeIndex + 1) : SHelper.Translation.Get("player");

				typeof(IClickableMenu).GetMethod("drawHorizontalPartition", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Game1.activeClickableMenu, new object[] { Game1.spriteBatch, yStart, false, -1, -1, -1 });
				Utility.drawTextWithShadow(Game1.spriteBatch, SHelper.Translation.Get("ingredients") + (Config.ShowProductInfo && CookableRecipes.Count > 0 ? " " + string.Format(CookableRecipes.Count == 1 ? SHelper.Translation.Get("will-cook-1") : SHelper.Translation.Get("will-cook-x"), CookableRecipes.Count) : ""), Game1.smallFont, new Vector2(xStart, yStart + 46), Color.Black, 1f, -1f, -1, -1, 1f, 3);
				IngredientMenu.draw(Game1.spriteBatch);
				CookButton.draw(Game1.spriteBatch, CanCook ? Color.White : (Color.Gray * 0.8f), 0.88f);
				if (FridgeLeftButton is not null && FridgeRightButton is not null)
				{
					FridgeLeftButton.draw(Game1.spriteBatch);
					FridgeRightButton.draw(Game1.spriteBatch);
					Utility.drawTextWithShadow(Game1.spriteBatch, whichFridge, Game1.smallFont, new Vector2(xStart + 88, yStart - 20), Color.Black, 1f, -1f, -1, -1, 1f, 3);
				}
				if (Config.ShowCookTooltip && CanCook && CookButton.containsPoint(Game1.getMouseX(), Game1.getMouseY()) && CookableRecipes.Count > 0)
				{
					StringBuilder tooltip = new();
					List<(Item, int)> values = CookableRecipes.Values.ToList();

					if (!SHelper.Input.IsDown(Config.CookAllModKey))
					{
						tooltip.Append(string.Format(SHelper.Translation.Get("x-of-y"), 1, Config.ShowProductsInTooltip ? values[0].Item1.DisplayName : "???"));
					}
					else
					{
						for (int i = 0; i < values.Count; i++)
						{
							if (i > 0)
							{
								tooltip.Append('\n');
							}
							if (i < Config.MaxItemsInTooltip)
							{
								tooltip.Append(string.Format(SHelper.Translation.Get("x-of-y"), values[i].Item2, Config.ShowProductsInTooltip ? values[i].Item1.DisplayName : "???"));
							}
							else
							{
								tooltip.Append(string.Format(SHelper.Translation.Get("plus-x-more"), CookableRecipes.Keys.Count - i));
								break;
							}
						}
					}
					IClickableMenu.drawHoverText(Game1.spriteBatch, tooltip, Game1.smallFont, 0, 0, -1, SHelper.Translation.Get("cook"));
				}
			}
		}
	}
}
