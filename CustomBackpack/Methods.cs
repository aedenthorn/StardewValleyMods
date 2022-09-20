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
            int first = (instance.actualInventory.Count <= instance.capacity ? 0 :  scrolled * instance.capacity / instance.rows);
            Rectangle rect = new Rectangle(instance.inventory[first].bounds.X, instance.inventory[first].bounds.Y, instance.inventory[first + instance.capacity - 1].bounds.X + instance.inventory[first + instance.capacity - 1].bounds.Width - instance.inventory[first].bounds.X, instance.inventory[first+instance.capacity - 1].bounds.Y + instance.inventory[first+instance.capacity - 1].bounds.Height - instance.inventory[first].bounds.Y);
            return rect.Contains(x, y);
        }

        public static Rectangle GetBounds(InventoryMenu __instance, int i)
        {
            int offset = __instance.capacity >= __instance.actualInventory.Count ? 0 : scrolled;
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
            if ((Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadUp) && !Game1.oldPadState.IsButtonDown(Buttons.DPadUp)) || (Game1.input.GetKeyboardState().IsKeyDown(Keys.Up) && !Game1.oldKBState.IsKeyDown(Keys.Up)))
            {
                 ChangeScroll(__instance, -1);
            }
            else if ((Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadDown) && !Game1.oldPadState.IsButtonDown(Buttons.DPadDown)) || (Game1.input.GetKeyboardState().IsKeyDown(Keys.Down) && !Game1.oldKBState.IsKeyDown(Keys.Down)))
            {
                 ChangeScroll(__instance, 1);
            }
            else if (Game1.input.GetMouseState().ScrollWheelValue != Game1.oldMouseState.ScrollWheelValue)
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

        private static bool ChangeScroll(InventoryMenu __instance, int v)
        {
            if (v == 0)
                return false;
            if (scrolled + v >= 0 && __instance.actualInventory.Count >= __instance.capacity / __instance.rows * (scrolled + v) + __instance.capacity)
            {
                scrolled += v;
                Game1.playSound("shiny4");
                int width = __instance.capacity / __instance.rows;
                for (int i = 0; i < __instance.inventory.Count; i++)
                {
                    __instance.inventory[i].bounds = GetBounds(__instance, i);
                    __instance.inventory[i].downNeighborID = (i >= __instance.capacity + width * (scrolled - 1)) ? 102 : 90000 + (i + width);
                    __instance.inventory[i].upNeighborID = (i < width * (scrolled + 1)) ? (12340 + i - width * scrolled) : 90000 + (i - width);
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

            var cc1 = __instance.inventory[(scrolled + 1) * gridWidth - 1];
            Point corner1 = cc1.bounds.Location + new Point(cc1.bounds.Width, 0);
            var cc2 = __instance.inventory[(scrolled + __instance.rows) * gridWidth - 1];
            Point corner2 = cc2.bounds.Location + new Point(cc2.bounds.Width, cc2.bounds.Height);
            Point middle = corner1 + new Point(0, (corner2.Y - corner1.Y) / 2);

            scrollArea = new Rectangle(corner1, new Point(24, corner2.Y - corner1.Y));
            if(scrollWidth > 226)
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollArea.X + maxScrollWidth - scrollWidth, scrollArea.Y, scrollWidth, scrollArea.Height, Color.White, 4f, false, -1f);
            else 
                b.Draw(scrollTexture, new Rectangle(scrollArea.X + maxScrollWidth - scrollWidth, scrollArea.Y, scrollWidth, scrollArea.Height), Color.White);


            int scrollIntervals = totalRows - __instance.rows + 1;
            int handleHeight = Math.Max(Config.MinHandleHeight, scrollArea.Height / scrollIntervals);
            int handleOffset = (handleHeight - scrollArea.Height / scrollIntervals) / 2;
            int scrollableHeight = (scrollArea.Height - handleOffset * 2);
            float scrollInterval = scrollableHeight / (float)scrollIntervals;
            int handleY = scrollArea.Y + (int)Math.Round(scrollInterval * scrolled);

            if(scrolled == totalRows - __instance.rows)
            {
                handleY = scrollArea.Y + scrollArea.Height - handleHeight;
            }
            bool inScrollArea = scrollArea.Contains(mouseX, mouseY);

            if (inScrollArea)
            {
                ChangeScroll(__instance, scrollChange);
            }

            if (inScrollArea || scrolling || (Config.ShowArrows && (upArrow.bounds.Contains(mouseX, mouseY) || downArrow.bounds.Contains(mouseX, mouseY))))
            {
                scrollWidth = Math.Min(maxScrollWidth - 4, scrollWidth + 1);
                if (scrolling)
                {
                    int yOffset = Math.Max(Math.Min(mouseY, scrollArea.Y + scrollArea.Height - 1), scrollArea.Y) - scrollArea.Y - handleOffset;
                    ChangeScroll(__instance, (int)(yOffset / scrollInterval) - scrolled);
                }
            }
            else if(scrollWidth > 4)
            {
                scrollWidth = Math.Max(minScrollWidth, scrollWidth - 1);
            }
            if (scrollWidth > 226)
                b.Draw(Game1.mouseCursors, new Rectangle(scrollArea.X + maxScrollWidth - scrollWidth, handleY, scrollWidth, handleHeight), new Rectangle(435, 463, 6, 10), Color.White);
            else
                b.Draw(handleTexture, new Rectangle(scrollArea.X + maxScrollWidth - scrollWidth, handleY, scrollWidth, handleHeight), Color.White);

            if (scrollWidth > maxScrollWidth / 2 && Config.ShowArrows)
            {
                if (scrolled > 0)
                {
                    upArrow.setPosition(corner1.X - 3, corner1.Y - 23);
                    upArrow.draw(b);
                }
                if (scrolled * __instance.capacity / __instance.rows + __instance.capacity < __instance.actualInventory.Count)
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
                    if (pressTime == 0 || (pressTime >= 20 && pressTime % 4 == 0))
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
                    if (pressTime < 20)
                        pressTime++;
                }
            }
            else
            {
                pressTime = 0;
            }
            if (Config.ShowRowNumbers)
            {
                int width = __instance.capacity / __instance.rows;
                for (int i = 0; i < __instance.rows; i++)
                {
                    var cc = __instance.inventory[(scrolled + i) * width];

                    Vector2 toDraw = new Vector2(cc.bounds.X - 8, cc.bounds.Y + 16);

                    var strToDraw = (scrolled + 1 + i) + "";
                    Vector2 strSize = Game1.tinyFont.MeasureString(strToDraw);
                    b.DrawString(Game1.tinyFont, strToDraw, toDraw + new Vector2(-strSize.X / 2f, -strSize.Y), Color.DimGray);
                }
            }
        }

        private static void OpenFullInventory(InventoryMenu __instance)
        {
            Game1.playSound("shwip");
            oldScrolled = scrolled;
            lastMenu = Game1.activeClickableMenu;
            Game1.activeClickableMenu = new FullInventoryPage(__instance, __instance.xPositionOnScreen, __instance.yPositionOnScreen, __instance.width, __instance.height);
        }
    }
}