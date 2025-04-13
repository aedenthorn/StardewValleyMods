using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.SpecialOrders;
using Object = StardewValley.Object;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace MoolahMoneyMod
{
	public partial class ModEntry
	{
		public class Farmer__money_Setter_Patch
		{
			public static void Prefix(Farmer __instance, ref int value)
			{
				if (!Config.ModEnabled)
					return;

				BigInteger moolah = 0;

				if (__instance.modData.TryGetValue(moolahKey, out string moolahString))
				{
					moolah = BigInteger.Parse(moolahString);
				}
				value = StoreOverflowAndClampMoney(__instance, value + moolah);
			}
		}

		public class Farmer_addUnearnedMoney_Patch
		{
			public static bool Prefix(Farmer __instance, ref int money)
			{
				if (!Config.ModEnabled)
					return true;

				BigInteger total = GetMoolah(__instance) + money;

				if (total > int.MaxValue)
				{
					SetMoolah(__instance, total);
					return false;
				}
				return true;
			}
		}

		public class Game1_newDayAfterFade_Patch
		{
			public static void Prefix()
			{
				if (!Config.ModEnabled || (!Game1.player.useSeparateWallets && !Game1.player.IsMainPlayer))
					return;

				BigInteger previous = GetMoolah(Game1.player);
				BigInteger total = new(0);

				shippingBin.Value = Game1.getFarm().getShippingBin(Game1.player).ToArray();
				foreach (Item item in shippingBin.Value)
				{
					if (item is not null)
					{
						BigInteger item_value = 0;

						if (item is Object obj)
						{
							item_value = new BigInteger(obj.sellToStorePrice()) * item.Stack;
							total += item_value;
						}
						Game1.player.displayedShippedItems.Add(item);
						if (Game1.player.team.specialOrders is not null)
						{
							foreach (SpecialOrder order in Game1.player.team.specialOrders)
							{
								order.onItemShipped?.Invoke(Game1.player, item, item_value > int.MaxValue ? int.MaxValue : (int)item_value);
							}
						}
					}
				}
				SMonitor.Log($"Made {total} moolah today");
				SetMoolah(Game1.player, total + previous);
				Game1.getFarm().getShippingBin(Game1.player).Clear();
			}
		}

		public class DayTimeMoneyBox_drawMoneyBox_Patch
		{
			public static bool Prefix(DayTimeMoneyBox __instance, SpriteBatch b, int overrideX, int overrideY)
			{
				if (!Config.ModEnabled || Game1.player is null)
					return true;

				BigInteger moolah = GetMoolah(Game1.player);
				int extraDigits = moolah.ToString().Length - 8;

				AccessTools.Method(typeof(DayTimeMoneyBox), "updatePosition").Invoke(__instance, Array.Empty<object>());
				b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : overrideX, overrideY - 172) : __instance.position) + new Vector2(28 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0), 172 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), new Rectangle?(new Rectangle(340, 472, 65, 17)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
				if (extraDigits > 0)
				{
					b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : overrideX, overrideY - 172) : __instance.position) + new Vector2((float)(28 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)) - extraDigits * 24, 172 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), new Rectangle?(new Rectangle(340, 472, 11, 17)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
					for (int i = 0; i < extraDigits; i++)
					{
						b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : overrideX, overrideY - 172) : __instance.position) + new Vector2((float)(68 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)) - (i + 1) * 24, 180 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), new Rectangle?(new Rectangle(356, 474, 6, 15)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
					}
				}
				DrawMoneyDial(__instance.moneyDial, b, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : overrideX, overrideY - 172) : __instance.position) + new Vector2((float)(68 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), 196 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), moolah);
				if (__instance.moneyShakeTimer > 0)
				{
					__instance.moneyShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				}
				return false;
			}
		}

		public class InventoryPage_draw_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				SMonitor.Log($"Transpiling InventoryPage.draw");
				List<CodeInstruction> list = instructions.ToList();

				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].opcode == OpCodes.Callvirt && list[i].operand is MethodInfo info && info == AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.Money)) && list[i + 1].opcode == OpCodes.Call && list[i + 1].operand is MethodInfo info1 && info1 == AccessTools.Method(typeof(Utility), nameof(Utility.getNumberWithCommas)))
					{
						list[i + 1].operand = AccessTools.Method(typeof(ModEntry), nameof(GetMoney));
						list.RemoveAt(i--);
					}
					else if (list[i].opcode == OpCodes.Callvirt && list[i].operand is MethodInfo info2 && info2 == AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.totalMoneyEarned)) && list[i + 1].opcode == OpCodes.Call && list[i + 1].operand is MethodInfo info3 && info3 == AccessTools.Method(typeof(Utility), nameof(Utility.getNumberWithCommas)))
					{
						list[i + 1].operand = AccessTools.Method(typeof(ModEntry), nameof(GetTotalMoneyEarned));
						list.RemoveAt(i--);
					}
				}
				return list;
			}
		}

		public class MoneyDial_draw_Patch
		{
			public static bool Prefix()
			{
				if (!Config.ModEnabled || !Environment.StackTrace.Contains("ShippingMenu"))
					return true;

				return false;
			}
		}

		public class ShippingMenu_Patch
		{
			public static void Prefix(ShippingMenu __instance)
			{
				if (!Config.ModEnabled)
					return;

				categoryTotals.Value = new() { 0, 0, 0, 0, 0, 0 };
				itemValues.Value = new();
				singleItemValues.Value = new();
				moneyDialDataList.Value = new() { new(), new(), new(), new(), new(), new() };
				foreach (Item item in Game1.player.displayedShippedItems)
				{
					if (item is Object obj)
					{
						int category = __instance.getCategoryIndexForObject(obj);
						int sell_to_store_price = obj.sellToStorePrice(-1L);
						BigInteger price = new BigInteger(sell_to_store_price) * obj.Stack;

						categoryTotals.Value[category] += price;
						categoryTotals.Value[5] += price;
						itemValues.Value[item] = price;
						singleItemValues.Value[item] = sell_to_store_price;
					}
				}
			}
		}

		public class ShippingMenu_draw_Patch
		{
			public static void Postfix(ShippingMenu __instance, SpriteBatch b, int ___introTimer, int ___itemSlotWidth, List<MoneyDial> ___categoryDials, List<List<Item>> ___categoryItems, bool ___outro)
			{
				if (!Config.ModEnabled || ___outro)
					return;

				if (__instance.currentPage == -1)
				{
					int languageSpecificOffset = (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru) ? 64 : 0;
					int index = 0;

					foreach (ClickableTextureComponent category in __instance.categories)
					{
						if (___introTimer < 2500 - index * 500)
						{
							Vector2 vector = category.getVector2() + new Vector2(12 - languageSpecificOffset, -8f);

							for (int n = Math.Min(0, 6 - categoryTotals.Value[index].ToString().Length); n < 0; n++)
							{
								b.Draw(Game1.mouseCursors, vector + new Vector2(-___itemSlotWidth + languageSpecificOffset - 192 - 24 + n * 6 * 4, 12f), new Rectangle(355, 476, 7, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
							}
							DrawMoneyDial(___categoryDials[index], b, vector + new Vector2(-___itemSlotWidth + languageSpecificOffset - 192 - 72 + 4, 20f), categoryTotals.Value[index], index);
						}
						index++;
					}
				}
				else
				{
					int width = Math.Min(__instance.width, 1280);
					int height = Math.Min(__instance.height, 920);
					int x = Game1.uiViewport.Width / 2 - width / 2;
					int y = Game1.uiViewport.Height / 2 - height / 2;
					Vector2 location = new(x + 32, y + 32);

					IClickableMenu.drawTextureBox(b, x, y, width, height, Color.White);
					for (int i = __instance.currentTab * __instance.itemsPerCategoryPage; i < __instance.currentTab * __instance.itemsPerCategoryPage + __instance.itemsPerCategoryPage; i++)
					{
						if (___categoryItems[__instance.currentPage].Count > i)
						{
							Item item = ___categoryItems[__instance.currentPage][i];
							string singleItemValueText = item.DisplayName + " x" + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", singleItemValues.Value[item]);
							string itemValueText = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", GetNumberWithSeparator(itemValues.Value[item]));
							int textPositionX = (int)location.X + width - 64 - SpriteText.getWidthOfString(itemValueText);

							item.drawInMenu(b, location, 1f, 1f, 1f, StackDrawType.Draw);
							while (SpriteText.getWidthOfString(singleItemValueText + itemValueText) < width - 192)
							{
								singleItemValueText += " .";
							}
							if (SpriteText.getWidthOfString(singleItemValueText + itemValueText) >= width)
							{
								singleItemValueText = singleItemValueText.Remove(singleItemValueText.Length - 1);
							}
							SpriteText.drawString(b, singleItemValueText, (int)location.X + 64 + 12, (int)location.Y + 12);
							SpriteText.drawString(b, itemValueText, textPositionX, (int)location.Y + 12);
							location.Y += 68f;
						}
					}
				}
				if (!Game1.options.SnappyMenus || (___introTimer <= 0 && !___outro))
				{
					Game1.mouseCursorTransparency = 1f;
					__instance.drawMouse(b);
				}
			}
		}

		public class LocalizedContentManager_LoadString_Patch
		{
			public static void Prefix1(string path, ref object sub1)
			{
				object sub2 = null;
				object sub3 = null;

				Prefix(path, ref sub1, ref sub2, ref sub3);
			}

			public static void Prefix2(string path, ref object sub1, ref object sub2)
			{
				object sub3 = null;

				Prefix(path, ref sub1, ref sub2, ref sub3);
			}

			public static void Prefix3(string path, ref object sub1, ref object sub2, ref object sub3)
			{
				Prefix(path, ref sub1, ref sub2, ref sub3);
			}

			public static void Prefix4(string path, ref object[] substitutions)
			{
				if (substitutions is not null)
				{
					object sub1 = substitutions.Length > 0 ? substitutions[0] : null;
					object sub2 = substitutions.Length > 1 ? substitutions[1] : null;
					object sub3 = substitutions.Length > 2 ? substitutions[2] : null;

					Prefix(path, ref sub1, ref sub2, ref sub3);
				}
			}

			public static void Prefix(string path, ref object sub1, ref object sub2, ref object sub3)
			{
				if (!Config.ModEnabled || string.IsNullOrEmpty(Config.Separator))
					return;

				if ((sub1 is int || sub1 is BigInteger) && (path == "Strings\\UI:AnimalQuery_Sell" || path == "Strings\\UI:Inventory_CurrentFunds" || path == "Strings\\UI:Inventory_TotalEarnings" || path == "Strings\\UI:Inventory_CurrentFunds_Separate" || path == "Strings\\UI:Inventory_TotalEarnings_Separate" || path == "Strings\\UI:LetterViewer_MoneyIncluded" || path == "Strings\\UI:ItemList_ItemsLostValue" || path == "Strings\\Locations:BusStop_BuyTicketToDesert" || path == "Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse2" || path == "Strings\\Locations:BuyTicket" || path == "Strings\\StringsFromMaps:MovieTheater_CranePlay" || path == "Strings\\1_6_Strings:GoldenParrot_Yes" || path == "Strings\\1_6_Strings:Joja_Debt_Notice" || path == "Strings\\StringsFromCSFiles:Event.cs.1058" || path == "Strings\\StringsFromCSFiles:Event.cs.1068" || path == "Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020" || path == "Strings\\StringsFromCSFiles:FishingQuest.cs.13248" || path == "Strings\\StringsFromCSFiles:FishingQuest.cs.13274" || path == "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13607"))
				{
					sub1 = CheckIntToString(sub1.ToString());
				}
				if ((sub2 is int || sub2 is BigInteger) && (path == "Strings\\UI:Chat_SeparatedWallets" || path == "Strings\\Locations:MineCart_DestinationWithPrice"))
				{
					sub2 = CheckIntToString(sub2.ToString());
				}
				if ((sub3 is int || sub3 is BigInteger) && (path == "Strings\\UI:Chat_SentMoney"))
				{
					sub3 = CheckIntToString(sub3.ToString());
				}
			}
		}

		public class Utility_getNumberWithCommas_Patch
		{
			public static bool Prefix(int number, ref string __result)
			{
				if (!Config.ModEnabled || string.IsNullOrEmpty(Config.Separator))
					return true;

				__result = GetNumberWithSeparator(number);
				return false;
			}
		}

		public class IClickableMenu_drawHoverText_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				SMonitor.Log($"Transpiling IClickableMenu.drawHoverText");
				List<CodeInstruction> list =instructions.ToList();

				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].opcode == OpCodes.Call && list[i].operand is MethodInfo info && info == AccessTools.Method(typeof(int), nameof(int.ToString)))
					{
						list.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(CheckIntToString))));
						i++;
					}
				}
				return list;
			}
		}

		public class ShopMenu_draw_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				SMonitor.Log($"Transpiling ShopMenu.draw");
				List<CodeInstruction> list = instructions.ToList();

				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].opcode == OpCodes.Call && list[i].operand is MethodInfo info && info == AccessTools.Method(typeof(int), nameof(int.ToString)))
					{
						list.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(CheckIntToString))));
						i++;
					}
				}
				return list;
			}
		}

		public class SaveFileSlot_drawSlotMoney_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				SMonitor.Log($"Transpiling SaveFileSlot.drawSlotMoney");
				List<CodeInstruction> list = instructions.ToList();

				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].opcode == OpCodes.Callvirt && list[i].operand is MethodInfo info && info == AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.Money)) && list[i + 1].opcode == OpCodes.Call && list[i + 1].operand is MethodInfo info1 && info1 == AccessTools.Method(typeof(Utility), nameof(Utility.getNumberWithCommas)))
					{
						list[i + 1].operand = AccessTools.Method(typeof(ModEntry), nameof(GetMoney));
						list.RemoveAt(i--);
					}
				}
				return list;
			}
		}
	}
}
