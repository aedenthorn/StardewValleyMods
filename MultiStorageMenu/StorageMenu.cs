using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace MultiStorageMenu
{
    public class StorageMenu : IClickableMenu
    {
        public static int scrolled;
        public static int windowWidth = 64 * 25;
        public int xSpace = 64;
        public int scrollInterval = 32;

        public List<StorageData> storageList = new List<StorageData>();
        public InventoryMenu playerInventoryMenu;
        public bool canScroll;
        private Item heldItem;
        private TemporaryAnimatedSprite poof;
        private ClickableTextureComponent trashCan;
        private Item hoveredItem;
        private string hoverText;
        private int hoverAmount;
        private float trashCanLidRotation;
        private int heldMenu = -1;
        private int cutoff;

        public StorageMenu() : base(Game1.uiViewport.Width / 2 - (windowWidth + borderWidth * 2) / 2, -borderWidth - 64, 64 * 25 + 4 + borderWidth * 2, Game1.viewport.Height + borderWidth * 2 + 64, false)
        {
            cutoff = Game1.viewport.Height - 64 * 3 - 8 - borderWidth;

            var columns = 12;
            var rows = Math.Min(3, (int)Math.Ceiling(Game1.player.Items.Count / (float)columns));
            var cap = rows * columns;
            playerInventoryMenu = new InventoryMenu((Game1.viewport.Width - 64 * columns) / 2, Game1.viewport.Height - 64 * 3 - borderWidth / 2, false, Game1.player.Items, null, cap, rows);
            this.trashCan = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 192 - 32 - IClickableMenu.borderWidth - 104, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f, false)
            {
                myID = 5948,
                downNeighborID = 4857,
                leftNeighborID = 12,
                upNeighborID = 106
            };
            PopulateMenus(true);
            snapToDefaultClickableComponent();
        }

        private void PopulateMenus(bool remakeChests = false)
        {

            if (remakeChests)
            {
                storageList.Clear();
                foreach (var l in Game1.locations)
                {
                    if(l is FarmHouse)
                    {
                        Chest f = (l as FarmHouse).fridge.Value;
                        if (!f.modData.TryGetValue("Pathoschild.ChestsAnywhere/Name", out string key))
                        {
                            key = $"{l.Name} Fridge";
                        }
                        storageList.Add(new StorageData() { chest = f, id = key });

                    }
                    foreach (var kvp in l.objects.Pairs)
                    {
                        var obj = kvp.Value;
                        if (obj is Chest && (obj as Chest).playerChest.Value)
                        {
                            if(!obj.modData.TryGetValue("Pathoschild.ChestsAnywhere/Name", out string key))
                            {
                                key = $"{l.Name} {kvp.Key.X},{kvp.Key.Y}";
                            } 
                            storageList.Add(new StorageData() { chest = obj as Chest, id = key });
                        }
                    }
                }
            }
            int menusAlready = 0;
            int rowsAlready = 0;
            bool even = false;
            int oddRows = 0;
            for(int i = 0; i < storageList.Count; i++)
            {
                var storage = storageList[i];
                var columns = 12;
                var rows = Math.Max((int)Math.Ceiling(storage.chest.items.Count / (float)columns), 3);
                var cap = rows * columns;
                while (cap > storage.chest.items.Count)
                {
                    storage.chest.items.Add(null);
                }
                storageList[i].menu = new InventoryMenu(xPositionOnScreen + borderWidth + (even ? (64 * 13) : 0), yPositionOnScreen - scrolled * scrollInterval + borderWidth + 64 + 64 * rowsAlready + xSpace * (1 + menusAlready), false, storage.chest.items, null, cap, rows);
                if (!even)
                {
                    oddRows = (!storage.collapsed ? rows : 0);
                }
                else
                {
                    rowsAlready += Math.Max((!storage.collapsed ? rows : 0), oddRows);
                    menusAlready++;
                }
                even = !even;
            }
        }


        public override void snapToDefaultClickableComponent()
        {
            if (!Game1.options.snappyMenus || !Game1.options.gamepadControls)
                return;
            if(currentlySnappedComponent == null)
                currentlySnappedComponent = getComponentWithID(900);
            snapCursorToCurrentSnappedComponent();
        }
        public override void applyMovementKey(int direction)
        {
            if(currentlySnappedComponent != null)
            {
            }
            base.applyMovementKey(direction);
        }
        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true);
            for(int i = 0; i < storageList.Count; i++)
            {
                var storage = storageList[i];
                canScroll = storage.menu.yPositionOnScreen + storage.menu.rows * 64 + borderWidth > cutoff;
                if(canScroll && storage.menu.yPositionOnScreen - 48 > cutoff + 64 * 4)
                {
                    break;
                }
                if (i == heldMenu)
                    continue;
                SpriteText.drawString(b, storage.id, storage.menu.xPositionOnScreen, storage.menu.yPositionOnScreen - 48);
                if(!storage.collapsed)
                    storage.menu.draw(b);
            }
            Game1.drawDialogueBox(xPositionOnScreen, cutoff - borderWidth * 2, width, 64 * 4 + borderWidth * 2, false, true, null, false, true);
            playerInventoryMenu.draw(b);
            if (this.hoverText != null && this.hoveredItem == null)
            {
                if (this.hoverAmount > 0)
                {
                    IClickableMenu.drawToolTip(b, this.hoverText, "", null, true, -1, 0, -1, -1, null, this.hoverAmount);
                }
                else
                {
                    IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, -1, -1, -1, -1, 1f, null, null);
                }
            }
            if (this.hoveredItem != null)
            {
                IClickableMenu.drawToolTip(b, this.hoveredItem.getDescription(), this.hoveredItem.DisplayName, this.hoveredItem, this.heldItem != null, -1, 0, -1, -1, null, -1);
            }
            if (this.heldItem != null)
            {
                this.heldItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 8), (float)(Game1.getOldMouseY() + 8)), 1f);
            }
            if (heldMenu > -1)
            {
                SpriteText.drawString(b, storageList[heldMenu].id, Game1.getOldMouseX(), Game1.getOldMouseY() - 48);
                b.Draw(Game1.staminaRect, new Rectangle(Game1.getOldMouseX(), Game1.getOldMouseY(), 64 * 12, storageList[heldMenu].menu.rows * 64), Color.LightGray * 0.5f);

            }
            trashCan.draw(b);
            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            Item held = heldItem;
            if (y >= cutoff)
            {
                if(heldMenu> -1)
                {
                    return;
                }
                heldItem = playerInventoryMenu.leftClick(x, y, heldItem, false);
                if (heldItem != held)
                {
                    if (heldItem != null)
                        Game1.playSound("bigSelect");
                    else
                        Game1.playSound("bigDeSelect");
                }
            }
            else
            {
                for (int i = 0; i < storageList.Count; i++)
                {
                    var rect = new Rectangle(storageList[i].menu.xPositionOnScreen, storageList[i].menu.yPositionOnScreen - 48, (width - borderWidth * 2 - 64) / 2, 48);
                    if (rect.Contains(new Point(x, y)) || (heldMenu > -1 && storageList[i].menu.isWithinBounds(x, y)))
                    {
                        if (heldMenu > -1)
                        {
                            SwapMenus(i, heldMenu);
                            Game1.playSound("bigDeSelect");
                        }
                        else
                        {
                            heldMenu = i;
                            Game1.playSound("bigSelect");
                        }
                        return;
                    }
                    if (storageList[i].collapsed || heldMenu > -1)
                        continue;
                    heldItem = storageList[i].menu.leftClick(x, y, heldItem, false);
                    if (heldItem != held)
                    {
                        if (heldItem != null)
                            Game1.playSound("bigSelect");
                        else
                            Game1.playSound("bigDeSelect");
                        return;
                    }
                }
            }

        }

        private void SwapMenus(int idx1, int idx2)
        {
            StorageData storageData = storageList[idx1];
            storageList[idx1] = storageList[idx2];
            storageList[idx2] = storageData;
            PopulateMenus();
            heldMenu = -1;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (heldMenu > -1)
                return;
            Item held = heldItem;
            if(y >= cutoff)
            {
                heldItem = playerInventoryMenu.rightClick(x, y, heldItem, false);
                if (heldItem != held)
                {
                    if (heldItem != null)
                        Game1.playSound("bigSelect");
                    else
                        Game1.playSound("bigDeSelect");
                    return;
                }
            }
            else
            {
                for (int i = 0; i < storageList.Count; i++)
                {
                    var rect = new Rectangle(storageList[i].menu.xPositionOnScreen, storageList[i].menu.yPositionOnScreen - 48, (width - borderWidth * 2 - 64) / 2, 48);
                    if (rect.Contains(new Point(x, y)))
                    {
                        storageList[i].collapsed = !storageList[i].collapsed;
                        Game1.playSound("shiny4");
                        PopulateMenus(false);
                        return;
                    }
                    if (storageList[i].collapsed)
                        continue;

                    heldItem = storageList[i].menu.rightClick(x, y, heldItem, false);
                    if (heldItem != held)
                    {
                        if (heldItem != null)
                            Game1.playSound("bigSelect");
                        else
                            Game1.playSound("bigDeSelect");
                        return;
                    }
                }

            }
        }
        public override void receiveScrollWheelAction(int direction)
        {
            if (Game1.getMousePosition().Y >= cutoff)
            {
                return;
            }
            scrollInterval = 64;
            if(direction < 0)
            {
                if (!canScroll) 
                    return;
                scrolled++;
            }   
            else if (scrolled > 0) 
            {
                scrolled--;
            }
            PopulateMenus(false);
        }
        public override void receiveKeyPress(Keys key)
        {
            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                base.applyMovementKey(key);
            }
            if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose())
            {
                base.exitThisMenu(true);
            }
            else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.heldItem != null)
            {
                Game1.setMousePosition(this.trashCan.bounds.Center);
            }
            if (key == Keys.Delete && this.heldItem != null && this.heldItem.canBeTrashed())
            {
                Utility.trashItem(this.heldItem);
                this.heldItem = null;
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);
            if (this.poof != null && this.poof.update(time))
            {
                this.poof = null;
            }
        }

        public override void performHoverAction(int x, int y)
        {
            if (heldMenu > -1)
                return;

            this.hoveredItem = null;
            this.hoverText = "";
            base.performHoverAction(x, y);
            Item item_grab_hovered_item;
            if (Game1.getMousePosition().Y >= cutoff)
            {
                item_grab_hovered_item = playerInventoryMenu.hover(x, y, heldItem);
                if (item_grab_hovered_item != null)
                {
                    this.hoveredItem = item_grab_hovered_item;
                }
            }
            else
            {
                for (int i = 0; i < storageList.Count; i++)
                {
                    if (storageList[i].collapsed)
                        continue;
                    item_grab_hovered_item = storageList[i].menu.hover(x, y, this.heldItem);
                    if (item_grab_hovered_item != null)
                    {
                        this.hoveredItem = item_grab_hovered_item;
                    }
                }
            }
            this.hoverAmount = 0;
            if (this.trashCan != null)
            {
                if (this.trashCan.containsPoint(x, y))
                {
                    if (this.trashCanLidRotation <= 0f)
                    {
                        Game1.playSound("trashcanlid");
                    }
                    this.trashCanLidRotation = Math.Min(this.trashCanLidRotation + 0.06544985f, 1.57079637f);
                    if (this.heldItem != null && Utility.getTrashReclamationPrice(this.heldItem, Game1.player) > 0)
                    {
                        this.hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
                        this.hoverAmount = Utility.getTrashReclamationPrice(this.heldItem, Game1.player);
                        return;
                    }
                }
                else
                {
                    this.trashCanLidRotation = Math.Max(this.trashCanLidRotation - 0.06544985f, 0f);
                }
            }
        }
    }
}