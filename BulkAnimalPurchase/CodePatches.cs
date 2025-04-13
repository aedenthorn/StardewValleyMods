using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.FarmAnimals;
using StardewValley.Menus;
using static StardewValley.BellsAndWhistles.SpriteText;
using Object = StardewValley.Object;

namespace BulkAnimalPurchase
{
	public partial class ModEntry
	{
		private static readonly PerScreen<List<string>> alternatePurchaseTypes = new(() => new());
		private static readonly PerScreen<ClickableTextureComponent> minusButton = new(() => null);
		private static readonly PerScreen<ClickableTextureComponent> plusButton = new(() => null);
		private static readonly PerScreen<int> animalsToBuy = new(() => 0);

		internal static List<string> AlternatePurchaseTypes
		{
			get => alternatePurchaseTypes.Value;
			set => alternatePurchaseTypes.Value = value;
		}

		internal static ClickableTextureComponent MinusButton
		{
			get => minusButton.Value;
			set => minusButton.Value = value;
		}

		internal static ClickableTextureComponent PlusButton
		{
			get => plusButton.Value;
			set => plusButton.Value = value;
		}

		internal static int AnimalsToBuy
		{
			get => animalsToBuy.Value;
			set => animalsToBuy.Value = value;
		}

		public class PurchaseAnimalsMenu_Patch
		{
			public static void Postfix(PurchaseAnimalsMenu __instance)
			{
				if (!Config.EnableMod)
					return;

				AnimalsToBuy = 1;
				MinusButton = new ClickableTextureComponent("BAPMod_minus", new Rectangle(__instance.xPositionOnScreen + __instance.width - 180, __instance.yPositionOnScreen + __instance.height + 16, 64, 64), null, "", Game1.mouseCursors, OptionsPlusMinus.minusButtonSource, 4f, false)
				{
					myID = 200,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
				PlusButton = new ClickableTextureComponent("BAPMod_plus", new Rectangle(__instance.xPositionOnScreen + __instance.width - 80, __instance.yPositionOnScreen + __instance.height + 16, 64, 64), null, "", Game1.mouseCursors, OptionsPlusMinus.plusButtonSource, 4f, false)
				{
					myID = 201,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
			}
		}

		public class Game1_drawDialogueBox_Patch
		{
			private static readonly PerScreen<bool> skip = new(() => false);

			internal static bool Skip
			{
				get => skip.Value;
				set => skip.Value = value;
			}

			public static void Prefix()
			{
				if (!Config.EnableMod || Game1.activeClickableMenu is not PurchaseAnimalsMenu purchaseAnimalsMenu || Game1.IsFading() || Skip || AccessTools.FieldRefAccess<PurchaseAnimalsMenu, bool>(Game1.activeClickableMenu as PurchaseAnimalsMenu, "onFarm") || AccessTools.FieldRefAccess<PurchaseAnimalsMenu, bool>(Game1.activeClickableMenu as PurchaseAnimalsMenu, "namingAnimal"))
					return;

				string amountText = SHelper.Translation.Get("amount");
				Vector2 amountTextSize = Game1.dialogueFont.MeasureString(amountText);
				int maxAmountTextWidth = purchaseAnimalsMenu.width - 236;
				float amountTextScale = 1f;
				string animalsToBuyText = AnimalsToBuy.ToString();
				Vector2 animalsToBuyTextTextSize = Game1.dialogueFont.MeasureString(animalsToBuyText);

				if (amountTextSize.X > maxAmountTextWidth)
				{
					amountTextScale = maxAmountTextWidth / amountTextSize.X;
				}
				Skip = true;
				Game1.drawDialogueBox(purchaseAnimalsMenu.xPositionOnScreen, purchaseAnimalsMenu.yPositionOnScreen + purchaseAnimalsMenu.height - 100, purchaseAnimalsMenu.width, 200, false, true, null, false, true, -1, -1, -1);
				Skip = false;
				Utility.drawTextWithShadow(Game1.spriteBatch, amountText, Game1.dialogueFont, new Vector2(purchaseAnimalsMenu.xPositionOnScreen + 40, purchaseAnimalsMenu.yPositionOnScreen + purchaseAnimalsMenu.height + 34 - amountTextSize.Y * amountTextScale / 2), Game1.textColor, amountTextScale);
				MinusButton.draw(Game1.spriteBatch, Color.White * (AnimalsToBuy > 1 ? 1f : 0.5f), 0.86f + MinusButton.bounds.Y / 20000f);
				Utility.drawTextWithShadow(Game1.spriteBatch, AnimalsToBuy + "", Game1.dialogueFont, new Vector2(purchaseAnimalsMenu.xPositionOnScreen + purchaseAnimalsMenu.width - 116 - animalsToBuyTextTextSize.X / 2, purchaseAnimalsMenu.yPositionOnScreen + purchaseAnimalsMenu.height + 34 - animalsToBuyTextTextSize.Y / 2), Game1.textColor, 1f);
				PlusButton.draw(Game1.spriteBatch, Color.White * (AnimalsToBuy < 99 ? 1f : 0.5f), 0.86f + PlusButton.bounds.Y / 20000f);
			}
		}

		public class PurchaseAnimalsMenu_draw_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				SMonitor.Log($"Transpiling PurchaseAnimalsMenu.draw");
				List<CodeInstruction> list = instructions.ToList();

				for (int i = 0; i < list.Count - 5; i++)
				{
					if (list[i].opcode.Equals(OpCodes.Stloc_S) && list[i].operand is LocalBuilder localBuilder && localBuilder.LocalIndex.Equals(4))
					{
						list.Insert(i, new CodeInstruction(OpCodes.Call, typeof(PurchaseAnimalsMenu_draw_Patch).GetMethod(nameof(AddXLeftToAddToString))));
						i++;
					}
					else if (list[i].opcode.Equals(OpCodes.Ldarg_0) && list[i + 1].opcode.Equals(OpCodes.Ldfld) && list[i + 1].operand.Equals(typeof(IClickableMenu).GetField(nameof(IClickableMenu.height))) && list[i + 2].opcode.Equals(OpCodes.Add) && list[i + 3].opcode.Equals(OpCodes.Ldc_I4_S) && list[i + 3].operand.Equals((sbyte)-32))
					{
						list.Insert(i, new CodeInstruction(OpCodes.Call, typeof(PurchaseAnimalsMenu_draw_Patch).GetMethod(nameof(AddYOffsetToHoveredFarmAnimal))));
						i++;
					}
					else if (list[i].opcode.Equals(OpCodes.Ldstr) && list[i].operand.Equals("Truffle Pig") && list[i + 6].opcode.Equals(OpCodes.Call) && list[i + 6].operand.Equals(typeof(SpriteText).GetMethod(nameof(drawStringWithScrollBackground), BindingFlags.Public | BindingFlags.Static)))
					{
						List<CodeInstruction> replacementInstructions = new()
						{
							new(OpCodes.Call, typeof(PurchaseAnimalsMenu_draw_Patch).GetMethod(nameof(SetScrollBackgroundPlaceHolderWidthText))) { labels = list[i].labels }
						};

						replacementInstructions.AddRange(list.GetRange(i + 1, 4));
						replacementInstructions.Add(new CodeInstruction(OpCodes.Call, typeof(PurchaseAnimalsMenu_draw_Patch).GetMethod(nameof(SetScrollBackgroundScrollTextAlignment))));
						list.InsertRange(i, replacementInstructions);
						i += replacementInstructions.Count;
						list.RemoveRange(i, 6);
					}
					else if (list[i].opcode.Equals(OpCodes.Ldarg_0) && list[i + 1].opcode.Equals(OpCodes.Ldfld) && list[i + 1].operand.Equals(typeof(IClickableMenu).GetField(nameof(IClickableMenu.height))) && list[i + 2].opcode.Equals(OpCodes.Add) && list[i + 3].opcode.Equals(OpCodes.Ldc_I4_S) && list[i + 3].operand.Equals((sbyte)64))
					{
						list.Insert(i, new CodeInstruction(OpCodes.Call, typeof(PurchaseAnimalsMenu_draw_Patch).GetMethod(nameof(AddYOffsetToHoveredFarmAnimal))));
						i++;
					}
					else if (list[i].opcode.Equals(OpCodes.Ldstr) && list[i].operand.Equals("$99999999g") && list[i + 16].opcode.Equals(OpCodes.Call) && list[i + 16].operand.Equals(typeof(SpriteText).GetMethod(nameof(drawStringWithScrollBackground), BindingFlags.Public | BindingFlags.Static)))
					{
						List<CodeInstruction> replacementInstructions = list.GetRange(i, 15);

						replacementInstructions.Add(new(OpCodes.Call, typeof(PurchaseAnimalsMenu_draw_Patch).GetMethod(nameof(SetScrollBackgroundScrollTextAlignment))));
						list.InsertRange(i, replacementInstructions);
						i += replacementInstructions.Count;
						list.RemoveRange(i, 16);
					}
				}
				return list;
			}

			public static string AddXLeftToAddToString(string str)
			{
				if (!Config.EnableMod)
					return str;

				return $"{str} {string.Format(SHelper.Translation.Get("x-left-to-add"), AnimalsToBuy)}";
			}

			public static int AddYOffsetToHoveredFarmAnimal(int y)
			{
				if (!Config.EnableMod)
					return y;

				return y + 100;
			}

			public static string SetScrollBackgroundPlaceHolderWidthText()
			{
				if (!Config.EnableMod)
					return "Truffle Pig";

				return "XXXXXXXXXXXXXXX";
			}

			public static ScrollTextAlignment SetScrollBackgroundScrollTextAlignment()
			{
				if (!Config.EnableMod)
					return ScrollTextAlignment.Left;

				return ScrollTextAlignment.Center;
			}
		}

		public class PurchaseAnimalsMenu_performHoverAction_Patch
		{
			public static bool Prefix(PurchaseAnimalsMenu __instance, int x, int y, bool ___freeze, bool ___onFarm, bool ___namingAnimal)
			{
				if (!Config.EnableMod || Game1.IsFading() || ___freeze || ___onFarm || ___namingAnimal)
					return true;

				if (MinusButton is not null && MinusButton.containsPoint(x, y) && AnimalsToBuy > 1)
				{
					__instance.hovered = MinusButton;
				}
				if (PlusButton is not null && PlusButton.containsPoint(x, y))
				{
					__instance.hovered = PlusButton;
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

				if (AnimalsToBuy <= 1)
				{
					return true;
				}
				AnimalsToBuy--;
				Game1.addHUDMessage(new HUDMessage(__instance.animalBeingPurchased.isMale() ? Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11311", __instance.animalBeingPurchased.displayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11314", __instance.animalBeingPurchased.displayName), 1));
				__instance.namingAnimal = false;
				__instance.textBox.Selected = false;
				__instance.textBox.OnEnterPressed -= __instance.textBoxEvent;
				__instance.animalBeingPurchased = new FarmAnimal(AlternatePurchaseTypes.Any() ? Game1.random.ChooseFrom(AlternatePurchaseTypes) : __instance.animalBeingPurchased.type.Value, Game1.Multiplayer.getNewID(), __instance.animalBeingPurchased.ownerID.Value);
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
					if (MinusButton is not null && MinusButton.containsPoint(x, y) && AnimalsToBuy > 1)
					{
						Game1.playSound("smallSelect");
						AnimalsToBuy--;
						return false;
					}
					if (PlusButton is not null && PlusButton.containsPoint(x, y) && AnimalsToBuy < 99)
					{
						Game1.playSound("smallSelect");
						AnimalsToBuy++;
						return false;
					}
				}
				else
				{
					foreach (ClickableTextureComponent item in __instance.animalsToPurchase)
					{
						if (__instance.readOnly || !item.containsPoint(x, y) || (item.item as Object).Type is not null)
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
								AlternatePurchaseTypes.Clear();
								if (Game1.farmAnimalData.TryGetValue(__instance.animalBeingPurchased.type.Value, out FarmAnimalData value))
								{
									if (value.AlternatePurchaseTypes is not null)
									{
										foreach (AlternatePurchaseAnimals alternatePurchaseType in value.AlternatePurchaseTypes)
										{
											if (GameStateQuery.CheckConditions(alternatePurchaseType.Condition, null, null, null, null, null, new HashSet<string> { "RANDOM" }))
											{
												AlternatePurchaseTypes.AddRange(alternatePurchaseType.AnimalIds);
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

				___priceOfAnimal /= AnimalsToBuy;
				SMonitor.Log($"Price of animal: {___priceOfAnimal}x{AnimalsToBuy}");
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
				__result *= AnimalsToBuy;
			}
		}

		public class SpriteText_drawStringWithScrollBackground_Patch
		{
			public static void Prefix(ref string s, ref string placeHolderWidthText)
			{
				if (!Config.EnableMod || Game1.activeClickableMenu is not PurchaseAnimalsMenu || AnimalsToBuy <= 1 || (placeHolderWidthText != "Golden Chicken" && placeHolderWidthText != "Truffle Pig"))
					return;

				s += " x" + AnimalsToBuy;
				placeHolderWidthText += " x" + AnimalsToBuy;
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
