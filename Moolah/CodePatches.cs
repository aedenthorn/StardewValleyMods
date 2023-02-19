using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Object = StardewValley.Object;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Moolah
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Farmer), nameof(Farmer._money))]
        [HarmonyPatch(MethodType.Setter)]
        public class Farmer__money_Setter_Patch
        {
            public static void Prefix(Farmer __instance, ref int value)
            {
                if (!Config.EnableMod)
                    return;
                BigInteger moocha = 0;
                if (__instance.modData.TryGetValue(moochaKey, out string moochaString))
                    moocha = BigInteger.Parse(moochaString);
                value = AdjustMoney(__instance, value + moocha);

                //SMonitor.Log($"Total money: {value + moocha}");
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.addUnearnedMoney))]
        public class Farmer_addUnearnedMoney_Patch
        {
            public static bool Prefix(Farmer __instance, ref int money)
            {
                if (!Config.EnableMod)
                    return true;
                BigInteger total = GetTotalMoolah(__instance) + money;
                if(total > maxValue)
                {
                    __instance._money = AdjustMoney(__instance, total);
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Game1), "_newDayAfterFade")]
        public class Game1_newDayAfterFade_Patch
        {
            public static void Prefix()
            {
                if (!Config.EnableMod || (!Game1.player.useSeparateWallets && !Game1.player.IsMainPlayer))
                    return;

                shippingBin.Value = Game1.getFarm().getShippingBin(Game1.player).ToArray();
                BigInteger previous = GetTotalMoolah(Game1.player);
                BigInteger total = new(0);
                if (Config.Debug && false)
                {
                    Object obj = new Object(64, 999, false, 2000000000);
                    List<Object> list = new();
                    for (int i = 0; i < 100; i++)
                    {
                        list.Add(obj);
                    }
                    shippingBin.Value = list.ToArray();
                }
                foreach (Item item in shippingBin.Value)
                {
                    BigInteger item_value = 0;
                    if (item is Object)
                    {
                        item_value = new BigInteger((item as Object).sellToStorePrice(-1L)) * item.Stack;
                        total += item_value;
                    }
                    Game1.player.displayedShippedItems.Add(item);
                    if (Game1.player.team.specialOrders is not null)
                    {
                        foreach (SpecialOrder order in Game1.player.team.specialOrders)
                        {
                            if (order.onItemShipped != null)
                            {
                                order.onItemShipped(Game1.player, item, item_value > int.MaxValue ? int.MaxValue : (int)item_value);
                            }
                        }
                    }
                }
                SMonitor.Log($"Made {total} moolah today");
                Game1.player.Money = AdjustMoney(Game1.player, total + previous);
                Game1.getFarm().getShippingBin(Game1.player).Clear();
            }
        }
        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.drawMoneyBox))]
        public class DayTimeMoneyBox_drawMoneyBox_Patch
        {
            public static bool Prefix(DayTimeMoneyBox __instance, SpriteBatch b, int overrideX, int overrideY)
            {
                if (!Config.EnableMod || Game1.player is null)
                    return true;

				BigInteger moocha = GetTotalMoolah(Game1.player);

                AccessTools.Method(typeof(DayTimeMoneyBox), "updatePosition").Invoke(__instance, new object[] { });
                b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : ((float)overrideX), (float)(overrideY - 172)) : __instance.position) + new Vector2((float)(28 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), (float)(172 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0))), new Rectangle?(new Rectangle(340, 472, 65, 17)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);

				int extraDigits = moocha.ToString().Length - 8;
				if(extraDigits > 0)
                {
					b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : ((float)overrideX), (float)(overrideY - 172)) : __instance.position) + new Vector2((float)(28 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)) - (extraDigits) * 24, (float)(172 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0))), new Rectangle?(new Rectangle(340, 472, 11, 17)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
					for (int i = 0; i < extraDigits; i++)
                    {
						b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : ((float)overrideX), (float)(overrideY - 172)) : __instance.position) + new Vector2((float)(68 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)) - (i + 1) * 24, (float)(180 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0))), new Rectangle?(new Rectangle(356, 474, 6, 15)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
					}
				}

				DrawMoneyDial(__instance.moneyDial, b, ((overrideY != -1) ? new Vector2((overrideX == -1) ? __instance.position.X : ((float)overrideX), (float)(overrideY - 172)) : __instance.position) + new Vector2((float)(68 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), (float)(196 + ((__instance.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0))), moocha, -1);
                if (__instance.moneyShakeTimer > 0)
                {
                    __instance.moneyShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.draw))]
        public class InventoryPage_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling InventoryPage.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.Money)) && codes[i + 1].opcode == OpCodes.Call && codes[i + 1].operand is MethodInfo && (MethodInfo)codes[i + 1].operand == AccessTools.Method(typeof(Utility), nameof(Utility.getNumberWithCommas)))
                    {
                        SMonitor.Log("Switching shown money");
                        codes[i + 1].operand = AccessTools.Method(typeof(InventoryPage_draw_Patch), nameof(InventoryPage_draw_Patch.GetMoney));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }

            private static string GetMoney(int money)
            {
                var m = GetTotalMoolah(Game1.player);
                if (m.ToString().Length < 10)
                    return Utility.getNumberWithCommas((int)m);
                return string.Format("{0:#.##E+0}", m);
            }
        }
        [HarmonyPatch(typeof(MoneyDial), nameof(MoneyDial.draw))]
        public class MoneyDial_draw_Patch
        {
            public static bool Prefix(MoneyDial __instance)
            {
                if (!Config.EnableMod || string.IsNullOrEmpty(Config.Separator) || !Environment.StackTrace.Contains("ShippingMenu"))
                    return true;
                return false;

            }
        }
        [HarmonyPatch(typeof(ShippingMenu), new Type[] { typeof(IList<Item>) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class ShippingMenu_Patch
        {
            public static void Prefix(ShippingMenu __instance)
            {
                if (!Config.EnableMod)
                    return;

                categoryTotals.Value = new() { 0, 0, 0, 0, 0, 0 };
                itemValues.Value = new();
                singleItemValues.Value = new();
                moneyDialDataList.Value = new() { new(), new(), new(), new(), new(), new() };

                foreach (var j in Game1.player.displayedShippedItems)
                {
                    if (j is Object)
                    {
                        Object o = j as Object;
                        int category = __instance.getCategoryIndexForObject(o);
                        int sell_to_store_price = o.sellToStorePrice(-1L);
                        BigInteger price = new BigInteger(sell_to_store_price) * o.Stack;
                        categoryTotals.Value[category] += price;
                        categoryTotals.Value[5] += price;
                        itemValues.Value[j] = price;
                        singleItemValues.Value[j] = sell_to_store_price;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(ShippingMenu), nameof(ShippingMenu.draw))]
        public class ShippingMenun_draw_Patch
        {
            public static void Prefix(ShippingMenu __instance, List<List<Item>> ___categoryItems, ref List<Item> __state)
            {
                if (!Config.EnableMod || __instance.currentPage == -1)
                    return;
                __state = ___categoryItems[__instance.currentPage].ToList();
                ___categoryItems[__instance.currentPage].Clear();
            }
            public static void Postfix(ShippingMenu __instance, SpriteBatch b, int ___introTimer, int ___itemSlotWidth, int ___categoryLabelsWidth, List<MoneyDial> ___categoryDials, List<List<Item>> ___categoryItems, bool ___outro, List<Item> __state)
            {
                if (!Config.EnableMod || ___outro)
                    return;
                if(__instance.currentPage == -1)
                {
                    int i = 0;
                    foreach (ClickableTextureComponent c in __instance.categories)
                    {
                        if (___introTimer < 2500 - i * 500)
                        {
                            var ct = categoryTotals.Value[i];
                            Vector2 start = c.getVector2() + new Vector2(12f, -8f);
                            for (int j = Math.Min(-4, -(ct.ToString().Length - 6)); j < 0; j++)
                            {
                                b.Draw(Game1.mouseCursors, start + new Vector2((float)(-(float)___itemSlotWidth - 192 - 24 + j * 6 * 4), 12f), new Rectangle?(new Rectangle(355, 476, 7, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
                            }
                            DrawMoneyDial(___categoryDials[i], b, start + new Vector2((float)(-(float)___itemSlotWidth - 192 - 72 + 4), 20f), ct, i);
                        }
                        i++;
                    }
                }
                else
                {
                    ___categoryItems[__instance.currentPage] = __state;
                    Vector2 position = new Vector2((float)(__instance.xPositionOnScreen + 32), (float)(__instance.yPositionOnScreen + 32));
                    for (int k = __instance.currentTab * __instance.itemsPerCategoryPage; k < __instance.currentTab * __instance.itemsPerCategoryPage + __instance.itemsPerCategoryPage; k++)
                    {
                        if (___categoryItems[__instance.currentPage].Count > k)
                        {
                            Item item = ___categoryItems[__instance.currentPage][k];
                            item.drawInMenu(b, position, 1f, 1f, 1f, StackDrawType.Draw);
                            bool show_single_item_price = true;
                            if (LocalizedContentManager.CurrentLanguageLatin)
                            {
                                string line_string;
                                if (show_single_item_price)
                                {
                                    line_string = item.DisplayName + " x" + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", singleItemValues.Value[item]);
                                }
                                else
                                {
                                    line_string = item.DisplayName + ((item.Stack > 1) ? (" x" + item.Stack.ToString()) : "");
                                }
                                SpriteText.drawString(b, line_string, (int)position.X + 64 + 12, (int)position.Y + 12, 999999, -1, 999999, 1f, 0.88f, false, -1, "", -1, SpriteText.ScrollTextAlignment.Left);
                                string dots = ".";
                                for (int l = 0; l < __instance.width - 96 - SpriteText.getWidthOfString(line_string + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", itemValues.Value[item]), 999999); l += SpriteText.getWidthOfString(" .", 999999))
                                {
                                    dots += " .";
                                }
                                SpriteText.drawString(b, dots, (int)position.X + 80 + SpriteText.getWidthOfString(line_string, 999999), (int)position.Y + 8, 999999, -1, 999999, 1f, 0.88f, false, -1, "", -1, SpriteText.ScrollTextAlignment.Left);
                                SpriteText.drawString(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", itemValues.Value[item]), (int)position.X + __instance.width - 64 - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", itemValues.Value[item]), 999999), (int)position.Y + 12, 999999, -1, 999999, 1f, 0.88f, false, -1, "", -1, SpriteText.ScrollTextAlignment.Left);
                            }
                            else
                            {
                                string dotsAndName;
                                if (show_single_item_price)
                                {
                                    dotsAndName = item.DisplayName + " x" + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", singleItemValues.Value[item]);
                                }
                                else
                                {
                                    dotsAndName = item.DisplayName + ((item.Stack > 1) ? (" x" + item.Stack.ToString()) : ".");
                                }
                                string qtyTxt = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", itemValues.Value[item]);
                                int qtyPosX = (int)position.X + __instance.width - 64 - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", itemValues.Value[item]), 999999);
                                SpriteText.getWidthOfString(dotsAndName + qtyTxt, 999999);
                                while (SpriteText.getWidthOfString(dotsAndName + qtyTxt, 999999) < 1123)
                                {
                                    dotsAndName += " .";
                                }
                                if (SpriteText.getWidthOfString(dotsAndName + qtyTxt, 999999) >= 1155)
                                {
                                    dotsAndName = dotsAndName.Remove(dotsAndName.Length - 1);
                                }
                                SpriteText.drawString(b, dotsAndName, (int)position.X + 64 + 12, (int)position.Y + 12, 999999, -1, 999999, 1f, 0.88f, false, -1, "", -1, SpriteText.ScrollTextAlignment.Left);
                                SpriteText.drawString(b, qtyTxt, qtyPosX, (int)position.Y + 12, 999999, -1, 999999, 1f, 0.88f, false, -1, "", -1, SpriteText.ScrollTextAlignment.Left);
                            }
                            position.Y += 68f;
                        }
                    }
                }
                if (!Game1.options.SnappyMenus || (___introTimer <= 0 && !___outro))
                {
                    Game1.mouseCursorTransparency = 1f;
                    __instance.drawMouse(b, false, -1);
                }
            }
        }
        [HarmonyPatch(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string), typeof(object) })]
        public class LocalizedContentManager_LoadString_Patch
        {
            public static void Prefix(string path, ref object sub1)
            {
                if (!Config.EnableMod || string.IsNullOrEmpty(Config.Separator) || path != "Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020" || (sub1 is not int && sub1 is not BigInteger))
                    return;
                sub1 = CheckIntToString(sub1.ToString());
            }
        }
        [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.drawHoverText), new Type[] { typeof(SpriteBatch), typeof(StringBuilder), typeof(SpriteFont), typeof(int), typeof(int), typeof(int), typeof(string), typeof(int), typeof(string[]), typeof(Item), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(float), typeof(CraftingRecipe), typeof(IList<Item>) })]
        public class IClickableMenu_drawHoverText_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling IClickableMenu.drawHoverText");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(int), nameof(int.ToString)))
                    {
                        SMonitor.Log("Checking int to string");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.CheckIntToString))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }

        }
        [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.draw))]
        public class ShopMenu_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling ShopMenu.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(int), nameof(int.ToString)))
                    {
                        SMonitor.Log("Checking int to string");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.CheckIntToString))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }

        }

    }
}