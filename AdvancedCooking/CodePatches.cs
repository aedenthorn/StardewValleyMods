using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedCooking
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {

        private static void CraftingPage_Prefix(CraftingPage __instance, int x, int y, bool cooking, List<Chest> material_containers, ref int height)
        {
            isCookingMenu = false;
            containers = null;
            ingredientMenu = null;
            if (cooking)
            {
                height += 172;
                containers = material_containers;
                ingredientMenu = new InventoryMenu(x + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, y + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Config.YOffset + 84, false, ingredients, null, 11, 1, 0, 0, true);
                isCookingMenu = true;
                cookButton = new ClickableTextureComponent(new Rectangle(x + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 * 11, y + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Config.YOffset + 80, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
                {
                    myID = 102,
                    rightNeighborID = -99998,
                    leftNeighborID = 106
                };
                int labelWidth = (int)Game1.smallFont.MeasureString(SHelper.Translation.Get("fridge-x")).X;

                fridgeLeftButton = new ClickableTextureComponent("FridgeLeft", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth - 8, y + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Config.YOffset - 36, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
                {
                    myID = 631,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                };
                fridgeRightButton = new ClickableTextureComponent("FridgeRight", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 128 + labelWidth,  y + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Config.YOffset - 36, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
                {
                    myID = 632,
                    upNeighborID = -99998,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    downNeighborID = -99998
                };
            }
        }
        private static void CraftingPage_receiveLeftClick_Prefix(CraftingPage __instance, ref Item ___heldItem, int x, int y)
        {
            if (!Config.EnableMod || Game1.activeClickableMenu is not CraftingPage || !AccessTools.FieldRefAccess<CraftingPage, bool>(Game1.activeClickableMenu as CraftingPage, "cooking") || ___heldItem is Tool)
                return;
            if (cookButton != null && cookButton.containsPoint(x, y))
            {
                TryCookRecipe(__instance, ref ___heldItem);
                return;
            }
            if (fridgeLeftButton.containsPoint(x, y))
            {
                Game1.playSound("pickUpItem");
                fridgeIndex--;
                if(fridgeIndex < -1)
                    fridgeIndex = __instance._materialContainers.Count - 1;
                UpdateActualInventory(__instance);
                return;
            }
            if (fridgeRightButton.containsPoint(x, y))
            {
                Game1.playSound("pickUpItem");
                fridgeIndex++;
                if (fridgeIndex >= __instance._materialContainers.Count)
                    fridgeIndex = -1;
                UpdateActualInventory(__instance);
                return;
            }
            ___heldItem = ingredientMenu.leftClick(x, y, ___heldItem, true);
        }
        private static void Game1_drawDialogueBox_Postfix()
        {
            if (!Config.EnableMod || Game1.activeClickableMenu is not CraftingPage || !AccessTools.FieldRefAccess<CraftingPage, bool>(Game1.activeClickableMenu as CraftingPage, "cooking"))
                return;
            if(currentCookables == null)
                UpdateCurrentCookables();

            int xStart = Game1.activeClickableMenu.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth;
            int yStart = Game1.activeClickableMenu.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + Config.YOffset;
            string whichFridge = fridgeIndex >= 0 ? string.Format(SHelper.Translation.Get("fridge-x"), fridgeIndex+1) : SHelper.Translation.Get("player"); 
            Utility.drawTextWithShadow(Game1.spriteBatch, whichFridge, Game1.smallFont, new Vector2(xStart + 88, yStart - 20), Color.Black, 1f, -1f, -1, -1, 1f, 3);
            AccessTools.Method(typeof(IClickableMenu), "drawHorizontalPartition").Invoke(Game1.activeClickableMenu, new object[] { Game1.spriteBatch, yStart, false, -1, -1, -1 });
            Utility.drawTextWithShadow(Game1.spriteBatch, SHelper.Translation.Get("ingredients") + (Config.ShowProductInfo && currentCookables.Count > 0 ? " " + string.Format(currentCookables.Count == 1 ? SHelper.Translation.Get("will-cook-1") : SHelper.Translation.Get("will-cook-x"), currentCookables.Count) : ""), Game1.smallFont, new Vector2(xStart, yStart + 46), Color.Black, 1f, -1f, -1, -1, 1f, 3);
            ingredientMenu.draw(Game1.spriteBatch);
            cookButton.draw(Game1.spriteBatch);
            fridgeLeftButton.draw(Game1.spriteBatch);
            fridgeRightButton.draw(Game1.spriteBatch);
            if (Config.ShowCookTooltip && cookButton.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
            {
                DrawCookButtonTooltip();
            }
        }


        private static void InventoryMenu_Prefix(ref IList<Item> actualInventory)
        {
            if (!Config.EnableMod || !isCookingMenu || containers == null || containers.Count == 0 || containers[0] == null)
                return;

            var list = fridgeIndex < 0 ? Game1.player.Items : containers[Math.Min(containers.Count - 1, fridgeIndex)].items;

            for (int i = 0; i < Game1.player.maxItems.Value; i++)
            {
                if (list.Count <= i)
                    list.Add(null);
            }
            actualInventory = list;
            isCookingMenu = false;
        }
        private static void CookingMenu_DrawActualInventory_Postfix(ItemGrabMenu __instance, SpriteBatch b)
        {
            if (!Config.EnableMod)
                return;
            if (currentCookables == null)
                UpdateCurrentCookables();

            int xStart = Game1.activeClickableMenu.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth;
            int yStart = Game1.activeClickableMenu.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + Config.YOffset;
            string whichFridge = fridgeIndex >= 0 ? string.Format(SHelper.Translation.Get("fridge-x"), fridgeIndex + 1) : SHelper.Translation.Get("player");
            Utility.drawTextWithShadow(Game1.spriteBatch, whichFridge, Game1.smallFont, new Vector2(xStart + 88, yStart - 20), Color.Black, 1f, -1f, -1, -1, 1f, 3);
            AccessTools.Method(typeof(IClickableMenu), "drawHorizontalPartition").Invoke(Game1.activeClickableMenu, new object[] { Game1.spriteBatch, yStart, false, -1, -1, -1 });
            Utility.drawTextWithShadow(Game1.spriteBatch, SHelper.Translation.Get("ingredients") + (Config.ShowProductInfo && currentCookables.Count > 0 ? " " + string.Format(currentCookables.Count == 1 ? SHelper.Translation.Get("will-cook-1") : SHelper.Translation.Get("will-cook-x"), currentCookables.Count) : ""), Game1.smallFont, new Vector2(xStart, yStart + 46), Color.Black, 1f, -1f, -1, -1, 1f, 3);
            ingredientMenu.draw(Game1.spriteBatch);
            cookButton.draw(Game1.spriteBatch);
            fridgeLeftButton.draw(Game1.spriteBatch);
            fridgeRightButton.draw(Game1.spriteBatch);
            if (Config.ShowCookTooltip && cookButton.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
            {
                DrawCookButtonTooltip();
            }
        }
    }
}