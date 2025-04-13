using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.FarmAnimals;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace BulkAnimalPurchase
{
	public partial class ModEntry
	{
		private static readonly List<string> alternatePurchaseTypes = new();

		public class PurchaseAnimalsMenu_Patch
		{
			public static void Prefix()
			{
				if (!Config.EnableMod)
					return;

				Point start = new Point(Game1.uiViewport.Width / 2 + PurchaseAnimalsMenu.menuWidth / 2 - IClickableMenu.borderWidth * 2, (Game1.uiViewport.Height - PurchaseAnimalsMenu.menuHeight - IClickableMenu.borderWidth * 2) / 4) + new Point(-100, PurchaseAnimalsMenu.menuHeight + 120);

				animalsToBuy = 1;
				minusButton = new ClickableTextureComponent("BAPMod_minus", new Rectangle(start, new Point(64, 64)), null, "", Game1.mouseCursors, OptionsPlusMinus.minusButtonSource, 4f, false)
				{
					myID = 200,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
				plusButton = new ClickableTextureComponent("BAPMod_plus", new Rectangle(start + new Point(100, 0), new Point(64, 64)), null, "", Game1.mouseCursors, OptionsPlusMinus.plusButtonSource, 4f, false)
				{
					myID = 201,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
			}
		}

		private static bool skip = false;

		public class Game1_drawDialogueBox_Patch
		{
			public static void Prefix()
			{
				if (!Config.EnableMod || Game1.activeClickableMenu is not PurchaseAnimalsMenu || Game1.IsFading() || skip || AccessTools.FieldRefAccess<PurchaseAnimalsMenu, bool>(Game1.activeClickableMenu as PurchaseAnimalsMenu, "onFarm") || AccessTools.FieldRefAccess<PurchaseAnimalsMenu, bool>(Game1.activeClickableMenu as PurchaseAnimalsMenu, "namingAnimal"))
					return;

				IClickableMenu menu = Game1.activeClickableMenu;
				SpriteBatch b = Game1.spriteBatch;

				skip = true;
				Game1.drawDialogueBox(menu.xPositionOnScreen, menu.yPositionOnScreen + menu.height - 100, menu.width, 200, false, true, null, false, true, -1, -1, -1);
				skip = false;
				Utility.drawTextWithShadow(b, SHelper.Translation.Get("amount"), Game1.dialogueFont, new Vector2(menu.xPositionOnScreen + 40, menu.yPositionOnScreen + menu.height + 10), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				minusButton.draw(b);
				Utility.drawTextWithShadow(b, animalsToBuy + "", Game1.dialogueFont, new Vector2(menu.xPositionOnScreen + menu.width - 116 - Game1.dialogueFont.MeasureString(animalsToBuy + "").X / 2, menu.yPositionOnScreen + menu.height + 10), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				plusButton.draw(b);
			}
		}

		public class PurchaseAnimalsMenu_draw_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				SMonitor.Log($"Transpiling PurchaseAnimalsMenu.draw");
				List<CodeInstruction> codes = instructions.ToList();
				bool found = false;

				for (int i = 0; i < codes.Count; i++)
				{
					if (found && codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string),typeof(object),typeof(object) }))
					{
						SMonitor.Log("Adding to string result");
						codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, typeof(PurchaseAnimalsMenu_draw_Patch).GetMethod(nameof(AddToString))));
						break;
					}
					else if (!found && codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11355")
					{
						SMonitor.Log("found string 11355");
						found = true;
					}
				}
				return codes;
			}

			public static string AddToString(string str)
			{
				if (!Config.EnableMod)
					return str;

				return str + " " + string.Format(SHelper.Translation.Get("x-left-to-add"), animalsToBuy);
			}
		}

		public class PurchaseAnimalsMenu_performHoverAction_Patch
		{
			public static bool Prefix(PurchaseAnimalsMenu __instance, int x, int y, bool ___freeze, bool ___onFarm, bool ___namingAnimal)
			{
				if (!Config.EnableMod || Game1.IsFading() || ___freeze || ___onFarm || ___namingAnimal)
					return true;

				if (minusButton != null && minusButton.containsPoint(x, y) && animalsToBuy > 1)
				{
					__instance.hovered = minusButton;
				}
				if (plusButton != null && plusButton.containsPoint(x, y))
				{
					__instance.hovered = plusButton;
				}
				return true;
			}
		}

		public class PurchaseAnimalsMenu_setUpForReturnAfterPurchasingAnimal_Patch
		{
			public static bool Prefix(PurchaseAnimalsMenu __instance)
			{
				if (!Config.EnableMod)
					return true;

				ApplyConfiguration(__instance.animalBeingPurchased);

				if (animalsToBuy <= 1)
				{
					return true;
				}
				animalsToBuy--;
				Game1.addHUDMessage(new HUDMessage(__instance.animalBeingPurchased.isMale() ? Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11311", __instance.animalBeingPurchased.displayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11314", __instance.animalBeingPurchased.displayName), 1));
				__instance.namingAnimal = false;
				__instance.textBox.Selected = false;
				__instance.textBox.OnEnterPressed -= __instance.textBoxEvent;
				__instance.animalBeingPurchased = new FarmAnimal(alternatePurchaseTypes.Any() ? Game1.random.ChooseFrom(alternatePurchaseTypes) : __instance.animalBeingPurchased.type.Value, Game1.Multiplayer.getNewID(), __instance.animalBeingPurchased.ownerID.Value);
				SMonitor.Log($"next animal type: {__instance.animalBeingPurchased.type}; price {__instance.priceOfAnimal}, funds left {Game1.player.Money}");
				return false;
			}
		}

		public class PurchaseAnimalsMenu_receiveLeftClick_Patch
		{
			public static bool Prefix(PurchaseAnimalsMenu __instance, int x, int y, ref int __state)
			{
				__state = __instance.priceOfAnimal;
				if (!Config.EnableMod || Game1.IsFading() || __instance.freeze || __instance.namingAnimal)
					return true;

				if (!__instance.onFarm)
				{
					if (minusButton != null && minusButton.containsPoint(x, y) && animalsToBuy > 1)
					{
						Game1.playSound("smallSelect");
						animalsToBuy--;
						return false;
					}
					if (plusButton != null && plusButton.containsPoint(x, y))
					{
						Game1.playSound("smallSelect");
						animalsToBuy++;
						return false;
					}
				}
				else
				{
					foreach (ClickableTextureComponent item in __instance.animalsToPurchase)
					{
						if (__instance.readOnly || !item.containsPoint(x, y) || (item.item as StardewValley.Object).Type != null)
						{
							continue;
						}
						if (Game1.player.Money >= item.item.salePrice())
						{
							if (!SHelper.ModRegistry.IsLoaded("aedenthorn.LivestockChoices"))
							{
								string type = __instance.animalBeingPurchased.type.Value;

								if (type.EndsWith(" Chicken") && !type.Equals("Void Chicken") && !type.Equals("Golden Chicken"))
								{
									type = "Chicken";
								}
								else if (type.EndsWith(" Cow"))
								{
									type = "Cow";
								}
								alternatePurchaseTypes.Clear();
								if (Game1.farmAnimalData.TryGetValue(__instance.animalBeingPurchased.type.Value, out FarmAnimalData value))
								{
									if (value.AlternatePurchaseTypes is not null)
									{
										foreach (AlternatePurchaseAnimals alternatePurchaseType in value.AlternatePurchaseTypes)
										{
											if (GameStateQuery.CheckConditions(alternatePurchaseType.Condition, null, null, null, null, null, new HashSet<string> { "RANDOM" }))
											{
												alternatePurchaseTypes.AddRange(alternatePurchaseType.AnimalIds);
											}
										}
									}
								}
							}
						}
					}
				}
				return true;
			}

			public static void Postfix(int __state, ref int ___priceOfAnimal)
			{
				if (!Config.EnableMod || __state == ___priceOfAnimal)
					return;

				___priceOfAnimal /= animalsToBuy;
				SMonitor.Log($"Price of animal: {___priceOfAnimal}x{animalsToBuy}");
			}
		}

		public class Item_salePrice_Patch
		{
			public static void Postfix(Object __instance, ref int __result)
			{
				if (!Config.EnableMod || Game1.activeClickableMenu is not PurchaseAnimalsMenu)
					return;

				__result = __instance.Name switch
				{
					"White Chicken" => Config.ChickenPrice,
					"Duck" => Config.DuckPrice,
					"Rabbit" => Config.RabbitPrice,
					"White Cow" => Config.CowPrice,
					"Goat" => Config.GoatPrice,
					"Sheep" => Config.SheepPrice,
					"Pig" => Config.PigPrice,
					_ => __result
				};
				__result *= animalsToBuy;
			}
		}

		public class SpriteText_drawStringWithScrollBackground_Patch
		{
			public static void Prefix(ref string s, ref string placeHolderWidthText)
			{
				if (!Config.EnableMod || Game1.activeClickableMenu is not PurchaseAnimalsMenu || animalsToBuy <= 1 || (placeHolderWidthText != "Golden Chicken" && placeHolderWidthText != "Truffle Pig"))
					return;

				s += " x" + animalsToBuy;
				placeHolderWidthText += " x" + animalsToBuy;
			}
		}

		public class AnimalHouse_addNewHatchedAnimal_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				SMonitor.Log($"Transpiling AnimalHouse.addNewHatchedAnimal");
				List<CodeInstruction> codes = instructions.ToList();

				for (int i = 0; i < codes.Count; i++)
				{
					if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(AnimalHouse), nameof(AnimalHouse.adoptAnimal), new Type[] { typeof(FarmAnimal) }))
					{
						codes.Insert(i, new CodeInstruction(OpCodes.Call, typeof(ModEntry).GetMethod(nameof(ApplyConfiguration))));
						i++;
					}
				}
				return codes;
			}
		}
	}
}
