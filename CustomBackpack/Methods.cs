using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace CustomBackpack
{
    public partial class ModEntry
    {

        private static bool IsWithinBounds(InventoryMenu instance, int x, int y)
        {
            int first = (instance.actualInventory.Count <= instance.capacity ? 0 :  scrolled.Value * instance.capacity / instance.rows);
            Rectangle rect = new Rectangle(instance.inventory[first].bounds.X, instance.inventory[first].bounds.Y, instance.inventory[first + instance.capacity - 1].bounds.X + instance.inventory[first + instance.capacity - 1].bounds.Width - instance.inventory[first].bounds.X, instance.inventory[first+instance.capacity - 1].bounds.Y + instance.inventory[first+instance.capacity - 1].bounds.Height - instance.inventory[first].bounds.Y);
            return rect.Contains(x, y);
        }

        public static Rectangle GetBounds(InventoryMenu __instance, int i)
        {
            int offset = __instance.capacity >= __instance.actualInventory.Count ? 0 : scrolled.Value;
            int width = __instance.capacity / __instance.rows;
            if (i < offset * width || i >= offset * width + __instance.capacity)
            {
                return new Rectangle();
            }
            else
            {
                return new Rectangle(__instance.xPositionOnScreen + i % width * 64 + __instance.horizontalGap * (i % width), __instance.yPositionOnScreen + (i / width - offset) * (64 + __instance.verticalGap) + (i / width - offset - 1) * 4 - ((((i - offset * width) >= width || !__instance.playerInventory || __instance.verticalGap != 0)) ? 0 : 12), 64, 64);
            }
        }
        private static int GetDownNeighbor(InventoryMenu __instance, int i)
        {
            int width = __instance.capacity / __instance.rows;
            int offset = (__instance.capacity >= __instance.actualInventory.Count ? 0 : scrolled.Value) * width;
            if (i < offset || i >= offset + __instance.capacity)
            {
                return -99999;
            }
            else
            {
                return (i >= offset + width * (__instance.rows - 1)) ? 102 : IDOffset + i + width - offset;
            }
        }

        private static int GetUpNeighbor(InventoryMenu __instance, int i)
        {
            int width = __instance.capacity / __instance.rows;
            int offset = (__instance.capacity >= __instance.actualInventory.Count ? 0 : scrolled.Value) * width;
            if (i < offset || i >= offset + __instance.capacity)
            {
                return -99999;
            }
            else
            {
                return (i < offset + width) ? 12340 + i % width : IDOffset + i - width - offset;
            }
        }
        private static int GetLeftNeighbor(InventoryMenu __instance, int i)
        {
            int width = __instance.capacity / __instance.rows;
            int offset = (__instance.capacity >= __instance.actualInventory.Count ? 0 : scrolled.Value) * width;
            if (i < offset || i >= offset + __instance.capacity)
            {
                return -99999;
            }
            else if(i % width == 0)
            {
                return 107;
            }
            else
            {
                return IDOffset + i - 1 - offset;
            }
        }

        private static int GetRightNeighbor(InventoryMenu __instance, int i)
        {
            int width = __instance.capacity / __instance.rows;
            int offset = (__instance.capacity >= __instance.actualInventory.Count ? 0 : scrolled.Value) * width;
            if (i < offset || i >= offset + __instance.capacity)
            {
                return -99999;
            }
            else if (i % width == width - 1)
            {
                return 106;
            }
            else
            {
                return IDOffset + i + 1 - offset;
            }
        }

        public static bool SetPlayerSlots(int slots, bool force = false)
        {
            if (!Config.ModEnabled || Game1.player is null || slots < 1)
                return false;
            Game1.activeClickableMenu = null;
            if(slots == Game1.player.MaxItems)
            {
                SMonitor.Log($"Player backpack slots already at {slots}");
                return false;
            }

            SMonitor.Log($"Changing player backpack slots from {Game1.player.MaxItems} to {slots}");

            if(slots < Game1.player.MaxItems)
            {
                if (!force)
                {
                    for (int i = Game1.player.Items.Count - 1; i >= slots; i--)
                    {
                        if (Game1.player.Items.Count <= i)
                            break;
                        if (Game1.player.Items[i] is not null)
                        {
                            SMonitor.Log($"Slot {i} isn't empty, aborting");
                            return false;
                        }
                    }
                }
                while (Game1.player.Items.Count > slots)
                {
                    Game1.player.Items.RemoveAt(Game1.player.Items.Count - 1);
                }
            }
            else
            {
                while (Game1.player.Items.Count < slots)
                {
                    Game1.player.Items.Add(null);
                }
            }
            Game1.player.maxItems.Value = slots;
            return true;
        }

        private static void OnHover(ref InventoryMenu __instance, int x, int y)
        {
            if (__instance.capacity >= __instance.actualInventory.Count)
                return;
            if (Game1.input.GetMouseState().ScrollWheelValue != Game1.oldMouseState.ScrollWheelValue)
            {
                if (Game1.oldMouseState.ScrollWheelValue - Game1.input.GetMouseState().ScrollWheelValue > 0)
                {
                    ChangeScroll(__instance, 1);
                }
                else
                {
                    ChangeScroll(__instance, -1);
                }
            }
        }

        public static bool ChangeScroll(InventoryMenu __instance, int change)
        {
            if (change == 0)
                return false;
            if (scrolled.Value + change >= 0 && __instance.actualInventory.Count >= __instance.capacity / __instance.rows * (scrolled.Value + change) + __instance.capacity)
            {
                scrolled.Value += change;
                Game1.playSound("shiny4");
                var offset = __instance.GetOffset();
                for (int i = 0; i < __instance.inventory.Count; i++)
                {
                    __instance.inventory[i].myID = offset > i || offset + __instance.capacity <= i ? -99999 : IDOffset + i - offset;
                    __instance.inventory[i].bounds = GetBounds(__instance, i);
                    __instance.inventory[i].leftNeighborID = GetLeftNeighbor(__instance, i);
                    __instance.inventory[i].rightNeighborID = GetRightNeighbor(__instance, i);
                    __instance.inventory[i].downNeighborID = GetDownNeighbor(__instance, i);
                    __instance.inventory[i].upNeighborID = GetUpNeighbor(__instance, i);
                }
                return true;
            }
            return false;
        }


        public static void DrawUIElements(SpriteBatch b, InventoryMenu __instance)
        {
            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();
            int gridWidth = __instance.capacity / __instance.rows;
            int totalRows = __instance.actualInventory.Count / gridWidth;

            if (totalRows <= __instance.rows)
                return;

            int minScrollWidth = 4;
            int maxScrollWidth = 16;

            var cc1 = __instance.inventory[(scrolled.Value + 1) * gridWidth - 1];
            Point corner1 = cc1.bounds.Location + new Point(cc1.bounds.Width, 0);
            var cc2 = __instance.inventory[(scrolled.Value + __instance.rows) * gridWidth - 1];
            Point corner2 = cc2.bounds.Location + new Point(cc2.bounds.Width, cc2.bounds.Height);
            Point middle = corner1 + new Point(0, (corner2.Y - corner1.Y) / 2);

            scrollArea.Value = new Rectangle(corner1, new Point(24, corner2.Y - corner1.Y));
            if(scrollWidth.Value > 226)
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollArea.Value.X + maxScrollWidth - scrollWidth.Value, scrollArea.Value.Y, scrollWidth.Value, scrollArea.Value.Height, Color.White, 4f, false, -1f);
            else 
                b.Draw(scrollTexture, new Rectangle(scrollArea.Value.X + maxScrollWidth - scrollWidth.Value, scrollArea.Value.Y, scrollWidth.Value, scrollArea.Value.Height), Color.White);


            int scrollIntervals = totalRows - __instance.rows + 1;
            int handleHeight = Math.Max(Config.MinHandleHeight, scrollArea.Value.Height / scrollIntervals);
            int handleOffset = (handleHeight - scrollArea.Value.Height / scrollIntervals) / 2;
            int scrollableHeight = (scrollArea.Value.Height - handleOffset * 2);
            float scrollInterval = scrollableHeight / (float)scrollIntervals;
            int handleY = scrollArea.Value.Y + (int)Math.Round(scrollInterval * scrolled.Value);

            if(scrolled.Value == totalRows - __instance.rows)
            {
                handleY = scrollArea.Value.Y + scrollArea.Value.Height - handleHeight;
            }
            bool inScrollArea = scrollArea.Value.Contains(mouseX, mouseY);

            if (inScrollArea)
            {
                ChangeScroll(__instance, scrollChange.Value);
            }

            if (inScrollArea || scrolling.Value || (Config.ShowArrows && (upArrow.bounds.Contains(mouseX, mouseY) || downArrow.bounds.Contains(mouseX, mouseY))))
            {
                scrollWidth.Value = Math.Min(maxScrollWidth - 4, scrollWidth.Value + 1);
                if (scrolling.Value)
                {
                    int yOffset = Math.Max(Math.Min(mouseY, scrollArea.Value.Y + scrollArea.Value.Height - 1), scrollArea.Value.Y) - scrollArea.Value.Y - handleOffset;
                    ChangeScroll(__instance, (int)(yOffset / scrollInterval) - scrolled.Value);
                }
            }
            else if(scrollWidth.Value > 4)
            {
                scrollWidth.Value = Math.Max(minScrollWidth, scrollWidth.Value - 1);
            }
            if (scrollWidth.Value > 226)
                b.Draw(Game1.mouseCursors, new Rectangle(scrollArea.Value.X + maxScrollWidth - scrollWidth.Value, handleY, scrollWidth.Value, handleHeight), new Rectangle(435, 463, 6, 10), Color.White);
            else
                b.Draw(handleTexture, new Rectangle(scrollArea.Value.X + maxScrollWidth - scrollWidth.Value, handleY, scrollWidth.Value, handleHeight), Color.White);

            if (scrollWidth.Value > maxScrollWidth / 2 && Config.ShowArrows)
            {
                if (scrolled.Value > 0)
                {
                    upArrow.setPosition(corner1.X - 3, corner1.Y - 23);
                    upArrow.draw(b);
                }
                if (scrolled.Value * __instance.capacity / __instance.rows + __instance.capacity < __instance.actualInventory.Count)
                {
                    downArrow.setPosition(corner2.X - 3, corner2.Y - 3);
                    downArrow.draw(b);
                }
            }
            if(__instance is not FullInventoryMenu)
            {
                if (SHelper.Input.IsDown(Config.ShowExpandedButton))
                {
                    OpenFullInventory(__instance);
                    return;
                }
                expandButton.setPosition(corner2.X + 36, corner2.Y - 11);
                expandButton.draw(b);
            }
            if (Game1.oldMouseState.LeftButton == ButtonState.Pressed)
            {
                if (__instance is not FullInventoryMenu && expandButton.containsPoint(mouseX, mouseY))
                {
                    OpenFullInventory(__instance);
                    return;
                }
                else if (Config.ShowArrows && !inScrollArea) 
                {
                    if (pressTime.Value == 0 || (pressTime.Value >= 20 && pressTime.Value % 4 == 0))
                    {
                        if (upArrow.containsPoint(mouseX, mouseY))
                        {
                            ChangeScroll(__instance, -1);
                        }
                        else if (downArrow.containsPoint(mouseX, mouseY))
                        {
                            ChangeScroll(__instance, 1);
                        }
                    }
                    if (pressTime.Value < 20)
                        pressTime.Value++;
                }
            }
            else
            {
                pressTime.Value = 0;
            }
            if (Config.ShowRowNumbers)
            {
                int width = __instance.capacity / __instance.rows;
                for (int i = 0; i < __instance.rows; i++)
                {
                    var cc = __instance.inventory[(scrolled.Value + i) * width];

                    Vector2 toDraw = new Vector2(cc.bounds.X - 8, cc.bounds.Y + 16);

                    var strToDraw = (scrolled.Value + 1 + i) + "";
                    Vector2 strSize = Game1.tinyFont.MeasureString(strToDraw);
                    b.DrawString(Game1.tinyFont, strToDraw, toDraw + new Vector2(-strSize.X / 2f, -strSize.Y), Color.DimGray);
                }
            }
        }

        private static void OpenFullInventory(InventoryMenu __instance)
        {
            Game1.playSound("shwip");
            oldScrolled.Value = scrolled.Value;
            lastMenu.Value = Game1.activeClickableMenu;
            Game1.activeClickableMenu = new FullInventoryPage(__instance, __instance.xPositionOnScreen, __instance.yPositionOnScreen, __instance.width, __instance.height);
        }
    }
}