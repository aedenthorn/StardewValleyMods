using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using static StardewValley.Menus.CarpenterMenu;
using static StardewValley.Menus.LoadGameMenu;

namespace NoMoney
{
	public partial class ModEntry
    {
        public static ClickableTextureComponent moneyComponent;
        public static ClickableTextureComponent moneyComponentX;

        [HarmonyPatch(typeof(SaveFileSlot), "drawSlotMoney")]
        public static class SaveFileSlot_drawSlotMoney_Patch
        {
            public static bool Prefix(SpriteBatch b, int i, SaveFileSlot __instance, LoadGameMenu ___menu)
            {
                if (!__instance.Farmer.modData.ContainsKey(modKey) && !Config.EnableGlobally)
                    return true;
                string cashText = SHelper.Translation.Get("no-money-title");
                int moneyWidth = (int)Game1.dialogueFont.MeasureString(cashText).X;
                Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(___menu.slotButtons[i].bounds.X + ___menu.width - 192 - 100 - moneyWidth), (float)(___menu.slotButtons[i].bounds.Y + 64 + 44)), new Rectangle(193, 373, 9, 9), Color.White, 0f, Vector2.Zero, 4f, false, 1f, -1, -1, 0.35f);
                Vector2 position = new Vector2((float)(___menu.slotButtons[i].bounds.X + ___menu.width - 192 - 60 - moneyWidth), (float)(___menu.slotButtons[i].bounds.Y + 64 + 44));
                Utility.drawTextWithShadow(b, cashText, Game1.dialogueFont, position, Game1.textColor * __instance.getSlotAlpha(), 1f, -1f, -1, -1, 1f, 3);
                return false;
            }
        }

        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.getSellPrice))]
        public static class FarmAnimal_getSellPrice_Patch
        {
            public static bool Prefix(ref int __result)
            {
                if (!IsEnabled)
                    return true;
                __result = 0;
                return false;
            }
        }

        [HarmonyPatch(typeof(CarpenterMenu.BlueprintEntry), nameof(CarpenterMenu.BlueprintEntry.BuildCost))]
        [HarmonyPatch(MethodType.Getter)]
        public static class CarpenterMenu_BlueprintEntry_BuildCost_Get_Patch
        {
            public static bool Prefix(ref int __result)
            {
                if (!IsEnabled)
                    return true;
                __result = 0;
                return false;
            }
        }

        [HarmonyPatch(typeof(Stats), nameof(Stats.checkForMoneyAchievements))]
        public static class Stats_checkForMoneyAchievements_Patch
        {
            public static bool Prefix()
            {
                return !IsEnabled;
            }
        }

        [HarmonyPatch(typeof(DayTimeMoneyBox), "updatePosition")]
        public static class DayTimeMoneyBox_updatePosition_Patch
        {
            public static void Postfix(DayTimeMoneyBox __instance)
            {
                if (!IsEnabled)
                    return;
                __instance.questButton.bounds = new Rectangle(__instance.xPositionOnScreen + 212, __instance.yPositionOnScreen + 164, 44, 46);
            }
        }

        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.drawMoneyBox))]
        public static class DayTimeMoneyBox_drawMoneyBox_Patch
        {
            public static bool Prefix()
            {
                return !IsEnabled;
            }
        }
        [HarmonyPatch(typeof(Utility), nameof(Utility.drawTextWithShadow), new Type[] { typeof(SpriteBatch), typeof(string), typeof(SpriteFont), typeof(Vector2), typeof(Color), typeof(float), typeof(float), typeof(int), typeof(int), typeof(float), typeof(int) })]
        public static class Utility_drawTextWithShadow_Patch
        {
            public static bool Prefix(string text)
            {
                return !IsEnabled || Game1.activeClickableMenu is not GameMenu gm || gm.GetCurrentPage() is not InventoryPage || (!text.StartsWith(Game1.content.LoadString("Strings\\UI:Inventory_CurrentFunds" + (Game1.player.useSeparateWallets ? "_Separate" : ""), Utility.getNumberWithCommas(Game1.player.Money))) && !text.StartsWith(Game1.content.LoadString("Strings\\UI:Inventory_TotalEarnings" + (Game1.player.useSeparateWallets ? "_Separate" : ""), Utility.getNumberWithCommas((int)Game1.player.totalMoneyEarned))));
            }
        }
        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.draw), new Type[] { typeof(SpriteBatch) })]
        public static class DayTimeMoneyBox_draw_Patch
        {
            public static void Prefix(DayTimeMoneyBox __instance, ref int ___goldCoinTimer, ref string ___goldCoinString)
            {
                if (!IsEnabled)
                    return;
                ___goldCoinTimer = 0;
                ___goldCoinString = null;
            }
        }
        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.gotGoldCoin))]
        public static class DayTimeMoneyBox_gotGoldCoin_Patch
        {
            public static bool Prefix()
            {
                return !IsEnabled;
            }
        }
        [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.drawCurrency))]
        public static class ShopMenu_drawCurrency_Patch
        {
            public static bool Prefix()
            {
                return !IsEnabled;
            }
        }
        [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.draw), new Type[] { typeof(SpriteBatch) })]
        public static class ShopMenu_draw_Patch
        {
            public static void Prefix(ShopMenu __instance)
            {
                if (!IsEnabled) 
                    return;
                __instance.hoverPrice = 0;
            }
        }
        [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.setItemPriceAndStock))]
        public static class ShopMenu_setItemPriceAndStock_Patch
        {
            public static void Postfix(ShopMenu __instance)
            {
                if (!IsEnabled)
                    return;
                foreach (var key in __instance.itemPriceAndStock.Keys)
                {
                    __instance.itemPriceAndStock[key].Price = 0;
                }
            }
        }
        [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.AddForSale))]
        public static class ShopMenu_AddForSale_Patch
        {
            public static void Prefix(ISalable item, ref ItemStockInformation stock)
            {
                if (!IsEnabled)
                    return;
                if(stock is not null)
                    stock.Price = 0;
                    
            }
        }
        public static bool salePrice_Prefix(ref int __result)
        {
            if (!IsEnabled)
                return true;
            __result = 0;
            return false;
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.Money))]
        [HarmonyPatch(MethodType.Getter)]
        public static class Farmer_Money_Get_Patch
        {
            public static bool Prefix(ref int __result)
            {
                if (!IsEnabled)
                    return true;
                __result = int.MaxValue;
                return false;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.totalMoneyEarned))]
        [HarmonyPatch(MethodType.Getter)]
        public static class Farmer_totalMoneyEarned_Get_Patch
        {
            public static bool Prefix(ref uint __result)
            {
                if (!IsEnabled)
                    return true;
                __result = uint.MaxValue;
                return false;
            }
        }



        [HarmonyPatch(typeof(CharacterCustomization), "ResetComponents")]
        public static class CharacterCustomization_ResetComponents_Patch
        {

            public static void Postfix(CharacterCustomization __instance)
            {
                if (!Config.ModEnabled)
                    return;
                var pos = new Rectangle(__instance.xPositionOnScreen - 32, __instance.yPositionOnScreen + 80, 32, 32);
                moneyComponent = new ClickableTextureComponent(pos, Game1.mouseCursors, new Rectangle(0, 384, 16, 16), 2);
                moneyComponentX = new ClickableTextureComponent(new Rectangle(pos.X + 4, pos.Y + 4, 24, 24), Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 2);
            }
        }

        [HarmonyPatch(typeof(CharacterCustomization), nameof(CharacterCustomization.draw), new Type[] { typeof(SpriteBatch) })]
        public static class CharacterCustomization_draw_Patch
        {
            public static void Prefix(CharacterCustomization __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled)
                    return;
                moneyComponent.draw(b);
                if (IsEnabled)
                {
                    moneyComponentX.draw(b);
                }
            }
        }
        [HarmonyPatch(typeof(CharacterCustomization), "receiveLeftClick")]
        public class CharacterCustomization_receiveLeftClick_Patch
        {
            public static bool Prefix(CharacterCustomization __instance, int x, int y, bool playSound)
            {
                if (!Config.ModEnabled || !moneyComponent.containsPoint(x, y))
                    return true;
                ToggleEnabled();
                if(playSound)
                    Game1.playSound("grassyStep");
                return false;
            }
        }
        [HarmonyPatch(typeof(CharacterCustomization), "performHoverAction")]
        public class CharacterCustomization_performHoverAction_Patch
        {
            public static bool Prefix(CharacterCustomization __instance, int x, int y, ref string ___hoverText, ref string ___hoverTitle)
            {
                if (!Config.ModEnabled || !moneyComponent.containsPoint(x, y))
                    return true;

                ___hoverTitle = SHelper.Translation.Get("no-money-title");
                ___hoverText = IsEnabled ? SHelper.Translation.Get("disable") : SHelper.Translation.Get("enable"); 
                return false;
            }
        }
    }
}
