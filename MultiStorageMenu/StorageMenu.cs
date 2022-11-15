using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.SDKs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime;
using static System.Net.Mime.MediaTypeNames;
using Object = StardewValley.Object;

namespace MultiStorageMenu
{
    public class StorageMenu : IClickableMenu
    {
        public static int scrolled;
        public static int windowWidth = 64 * 25;
        public static StorageMenu instance;
        
        public int xSpace = 64;
        public int scrollInterval = 32;

        public List<StorageData> allStorageList = new List<StorageData>();
        public List<StorageData> storageList = new List<StorageData>();
        public InventoryMenu playerInventoryMenu;
        public bool canScroll;
        public Item heldItem;
        public TemporaryAnimatedSprite poof;
        public ClickableTextureComponent trashCan;
        private ClickableTextureComponent organizeButton;
        public TextBox locationText;
        public ClickableComponent locationTextCC;
        public Item hoveredItem;
        public string hoverText;
        public int hoverAmount;
        public float trashCanLidRotation;
        public int heldMenu = -1;
        public int cutoff;
        public string whichLocation;
        public string[] widgetText;
        public StorageData targetStorage;
        public StorageData renamingStorage;
        public TextBox renameBox;
        public ClickableTextureComponent okButton;
        private ClickableTextureComponent storeAlikeButton;
        public string chestsAnywhereKey = "Pathoschild.ChestsAnywhere/Name";
        public string filterString;
        public string nameString;
        public string fridgeString;
        public string storeSimilarString;

        public StorageMenu() : base(Game1.uiViewport.Width / 2 - (windowWidth + borderWidth * 2) / 2, -borderWidth - 64, 64 * 26 + 4 + borderWidth * 2, Game1.viewport.Height + borderWidth * 2 + 64, false)
        {
            instance = this;
            cutoff = Game1.viewport.Height - 64 * 3 - 8 - borderWidth;

            widgetText = new string[]{
                ModEntry.SHelper.Translation.Get("open"), 
                Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"), 
                ModEntry.SHelper.Translation.Get("put"), 
                ModEntry.SHelper.Translation.Get("take"), 
                ModEntry.SHelper.Translation.Get("rename"), 
                ModEntry.SHelper.Translation.Get("target")
            };

            filterString = ModEntry.SHelper.Translation.Get("filter");
            nameString = ModEntry.SHelper.Translation.Get("name");
            fridgeString = ModEntry.SHelper.Translation.Get("fridge");
            storeSimilarString = ModEntry.SHelper.Translation.Get("store-similar");

            var columns = 12;
            var rows = Math.Min(3, (int)Math.Ceiling(Game1.player.Items.Count / (float)columns));
            var cap = rows * columns;
            playerInventoryMenu = new InventoryMenu((Game1.viewport.Width - 64 * columns) / 2, Game1.viewport.Height - 64 * 3 - borderWidth / 2, false, Game1.player.Items, null, cap, rows);
            trashCan = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 128, yPositionOnScreen + height - 32 - borderWidth - 104, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f, false)
            {
                myID = 5948,
                downNeighborID = 4857,
                leftNeighborID = 12,
                upNeighborID = 106
            };
            organizeButton = new ClickableTextureComponent("", new Rectangle(playerInventoryMenu.xPositionOnScreen + playerInventoryMenu.width + 64, playerInventoryMenu.yPositionOnScreen, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), 4f, false)
            {
                myID = 106,
                downNeighborID = 105,
                leftNeighborID = 11,
                upNeighborID = 898
            };
            storeAlikeButton =  new ClickableTextureComponent("", new Rectangle(playerInventoryMenu.xPositionOnScreen + playerInventoryMenu.width + 64, playerInventoryMenu.yPositionOnScreen + 64, 64, 64), "", storeSimilarString, Game1.mouseCursors, new Rectangle(419, 456, 14, 14), 4f, false)
            {
                myID = 106,
                downNeighborID = 105,
                leftNeighborID = 11,
                upNeighborID = 898
            };
            locationText = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = xPositionOnScreen + borderWidth,
                Width = (width - playerInventoryMenu.width) / 2 - borderWidth * 2 - 64,
                Y = cutoff + borderWidth + 32,
                Text = whichLocation
            };
            locationTextCC = new ClickableComponent(new Rectangle(locationText.X, locationText.Y, 192, 64), "")
            {
                myID = 538,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            }; 
            renameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = locationText.X,
                Width = locationText.Width,
                Y = locationText.Y + locationText.Height + 48,
            }; 
            okButton = new ClickableTextureComponent(new Rectangle(renameBox.X + renameBox.Width + 4, renameBox.Y, 48, 48), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 0.75f, false)
            {
                myID = 101,
                upNeighborID = 108
            };
            PopulateMenus(true);
            snapToDefaultClickableComponent();
        }

        public void PopulateMenus(bool remakeChests = false)
        {
            if (remakeChests)
            {
                allStorageList.Clear();
                foreach (var l in Game1.locations)
                {
                    if(l is FarmHouse)
                    {
                        Chest f = (l as FarmHouse).fridge.Value;
                        string key = l.Name;
                        if (!f.modData.TryGetValue("Pathoschild.ChestsAnywhere/Name", out string name) || string.IsNullOrEmpty(name))
                        {
                            key += " " + fridgeString;
                        }
                        else
                        {
                            key += $" {name}";
                        }
                        allStorageList.Add(new StorageData() { chest = f, location = l.Name, id = key, index = allStorageList.Count });
                    }
                    foreach (var kvp in l.objects.Pairs)
                    {
                        var obj = kvp.Value;
                        if (obj is Chest && (obj as Chest).playerChest.Value && (obj as Chest).CanBeGrabbed)
                        {
                            string key = $"{l.Name} {kvp.Key.X},{kvp.Key.Y}";
                            if (obj.modData.TryGetValue(chestsAnywhereKey, out string name) && !string.IsNullOrEmpty(name))
                            {
                                key = $"{name} ({key})";
                            }

                            allStorageList.Add(new StorageData() { chest = obj as Chest, location = l.Name, id = key, index = allStorageList.Count });
                        }
                    }
                }
            }
            storageList.Clear();
            int menusAlready = 0;
            int rowsAlready = 0;
            bool even = false;
            int oddRows = 0;
            for(int i = 0; i < allStorageList.Count; i++)
            {
                var storage = allStorageList[i];

                if (!string.IsNullOrEmpty(whichLocation) && !storage.id.ToLower().Contains(whichLocation.ToLower()))
                    continue;
                RestoreNulls(storage.chest.items);
                var columns = 12;
                var rows = Math.Max((int)Math.Ceiling(storage.chest.items.Count / (float)columns), 3);
                var cap = rows * columns;
                while (cap > storage.chest.items.Count)
                {
                    storage.chest.items.Add(null);
                }
                storage.menu = new InventoryMenu(xPositionOnScreen + borderWidth + (even ? (64 * 13) : 0), yPositionOnScreen - scrolled * scrollInterval + borderWidth + 64 + 64 * rowsAlready + xSpace * (1 + menusAlready), false, storage.chest.items, null, cap, rows);
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
                storageList.Add(storage);
            }
        }

        public void RestoreNulls(IList<Item> items)
        {
            while (items.Count < 12 * ModEntry.Config.ChestRows)
            {
                items.Add(null);
            }
            while (items.Count > 12 * ModEntry.Config.ChestRows && items.Contains(null))
            {
                items.Remove(null);
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
                if (!storage.collapsed)
                {
                    storage.menu.draw(b);
                    b.Draw(Game1.mouseCursors, GetWidgetRectangle(storage, 0), new Rectangle(257, 284, 16, 16), Color.White);
                    b.Draw(Game1.mouseCursors, GetWidgetRectangle(storage, 1), new Rectangle(162, 440, 16, 16), Color.White);
                    b.Draw(Game1.mouseCursors, GetWidgetRectangle(storage, 2), new Rectangle(420, 457, 14, 14), Color.White);
                    b.Draw(Game1.mouseCursors, GetWidgetRectangle(storage, 3), new Rectangle(420, 471, 14, 14), Color.White);
                    b.Draw(Game1.mouseCursors, GetWidgetRectangle(storage, 4), new Rectangle(240, 320, 16, 16), Color.White);
                    b.Draw(Game1.mouseCursors, GetWidgetRectangle(storage, 5), new Rectangle(653, 205, 44, 44), targetStorage?.index == storage.index ? Color.White : Color.Purple);
                }
            }
            Game1.drawDialogueBox(xPositionOnScreen, cutoff - borderWidth * 2, width, 64 * 4 + borderWidth * 2, false, true, null, false, true);
            playerInventoryMenu.draw(b);
            SpriteText.drawString(b, filterString, locationText.X + 16, locationText.Y - 48);
            locationText.Draw(b);
            if(renamingStorage is not null)
            {
                SpriteText.drawString(b, nameString, renameBox.X + 16, renameBox.Y - 48);
                renameBox.Draw(b);
                okButton.draw(b);
            }
            
            trashCan.draw(b);
            organizeButton.draw(b);
            storeAlikeButton.draw(b);

            b.Draw(Game1.mouseCursors, new Vector2((float)(trashCan.bounds.X + 60), (float)(trashCan.bounds.Y + 40)), new Rectangle?(new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10)), Color.White, trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
            Game1.spriteBatch.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + 16, -4, 24, 16), new Rectangle(16, 16, 24, 16), Color.White);
            Game1.spriteBatch.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + width - 32, -4, 16, 16), new Rectangle(225, 16, 16, 16), Color.White);
            Game1.spriteBatch.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + 40, -4, width - 72, 16), new Rectangle(40, 16, 1, 16), Color.White);
            if (hoverText != null && hoveredItem == null)
            {
                if (hoverAmount > 0)
                {
                    drawToolTip(b, hoverText, "", null, true, -1, 0, -1, -1, null, hoverAmount);
                }
                else
                {
                    drawHoverText(b, hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, -1, -1, -1, -1, 1f, null, null);
                }
            }
            if (hoveredItem != null)
            {
                drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem, heldItem != null, -1, 0, -1, -1, null, -1);
            }
            if (heldItem != null)
            {
                heldItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 8), (float)(Game1.getOldMouseY() + 8)), 1f);
            }
            if (heldMenu > -1)
            {
                SpriteText.drawString(b, allStorageList[heldMenu].id, Game1.getOldMouseX(), Game1.getOldMouseY() - 48);
                b.Draw(Game1.staminaRect, new Rectangle(Game1.getOldMouseX(), Game1.getOldMouseY(), 64 * 12, allStorageList[heldMenu].menu.rows * 64), Color.LightGray * 0.5f);

            }
            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            Item held = heldItem;
            if (y >= cutoff)
            {
                if (heldMenu> -1)
                {
                    return;
                }
                heldItem = playerInventoryMenu.leftClick(x, y, heldItem, false);
                if (heldItem != held)
                {
                    if (heldItem != null)
                    {
                        Game1.playSound("bigSelect");
                        if(targetStorage is not null)
                        {
                            heldItem = AddItem(targetStorage.chest.items, heldItem);
                        }
                    }
                    else
                        Game1.playSound("bigDeSelect");
                }
                if(renamingStorage is not null)
                {
                    renameBox.Update();
                    if(okButton.containsPoint(x, y))
                    {
                        renamingStorage.chest.modData[chestsAnywhereKey] = renameBox.Text;
                        renamingStorage = null;
                        renameBox.Selected = false;
                        Game1.playSound("bigSelect");
                        PopulateMenus(true);
                    }
                }

                locationText.Update();
                if (this.trashCan != null && this.trashCan.containsPoint(x, y) && this.heldItem != null && this.heldItem.canBeTrashed())
                {
                    Utility.trashItem(this.heldItem);
                    this.heldItem = null;
                }
                if (organizeButton.containsPoint(x, y))
                {
                    Game1.playSound("Ship");
                    ItemGrabMenu.organizeItemsInList(Game1.player.Items);
                }
                if (storeAlikeButton.containsPoint(x, y))
                {
                    Game1.playSound("Ship");
                    foreach (var s in storageList)
                    {
                        SwapContents(Game1.player.Items, s.chest.items, true);
                    }
                    
                }
            }
            else
            {
                for (int i = 0; i < storageList.Count; i++)
                {
                    for(int j = 0; j < widgetText.Length; j++)
                    {
                        if (GetWidgetRectangle(storageList[i], j).Contains(new Point(x, y)))
                        {
                            ClickWidget(storageList[i], j);
                            return;
                        }
                    }
                    var rect = new Rectangle(storageList[i].menu.xPositionOnScreen, storageList[i].menu.yPositionOnScreen - 48, (width - borderWidth * 2 - 64) / 2, 48);
                    if (rect.Contains(new Point(x, y)) || (heldMenu > -1 && storageList[i].menu.isWithinBounds(x, y)))
                    {
                        if (heldMenu > -1)
                        {
                            SwapMenus(heldMenu, storageList[i].index);
                            Game1.playSound("bigDeSelect");
                        }
                        else
                        {
                            heldMenu = storageList[i].index;
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
                        {
                            Game1.playSound("bigSelect");
                            if (ModEntry.SHelper.Input.IsDown(ModEntry.Config.ModKey))
                            {
                                heldItem = AddItem(Game1.player.Items, heldItem);
                            }
                        }
                        else
                            Game1.playSound("bigDeSelect");
                        return;
                    }
                }
            }

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
                        PopulateMenus();
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
                applyMovementKey(key);
            }
            if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
            {
                exitThisMenu(true);
            }
            else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && heldItem != null)
            {
                Game1.setMousePosition(trashCan.bounds.Center);
            }
            if (key == Keys.Delete && heldItem != null && heldItem.canBeTrashed())
            {
                Utility.trashItem(heldItem);
                heldItem = null;
            }
            if (key.Equals(Keys.Delete) && this.heldItem != null && this.heldItem.canBeTrashed())
            {
                Utility.trashItem(this.heldItem);
            }
            if (key.Equals(Keys.Enter) && renameBox.Selected)
            {
                if(renamingStorage is not null)
                {
                    renamingStorage.chest.modData[chestsAnywhereKey] = renameBox.Text;
                    renamingStorage = null;
                    renameBox.Selected = false;
                    Game1.playSound("bigSelect");
                    PopulateMenus(true);
                }
                renameBox.Selected = false;
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);
            if(whichLocation?.ToLower() != locationText.Text.ToLower())
            {
                whichLocation = locationText.Text;
                scrolled = 0;
                PopulateMenus(false);
            }
            if (poof != null && poof.update(time))
            {
                poof = null;
            }
        }

        public override void performHoverAction(int x, int y)
        {
            if (heldMenu > -1)
                return;

            hoveredItem = null;
            hoverText = "";
            base.performHoverAction(x, y);
            Item item_grab_hovered_item;
            if (Game1.getMousePosition().Y >= cutoff)
            {
                item_grab_hovered_item = playerInventoryMenu.hover(x, y, heldItem);
                if (item_grab_hovered_item != null)
                {
                    hoveredItem = item_grab_hovered_item;
                }
                organizeButton.tryHover(x, y, 0.1f);
                if (organizeButton.containsPoint(x, y))
                {
                    hoverText = organizeButton.hoverText;
                }
                storeAlikeButton.tryHover(x, y, 0.1f);
                if (storeAlikeButton.containsPoint(x, y))
                {
                    hoverText = storeAlikeButton.hoverText;
                }
                hoverAmount = 0;

                if (trashCan.containsPoint(x, y))
                {
                    if (trashCanLidRotation <= 0f)
                    {
                        Game1.playSound("trashcanlid");
                    }
                    trashCanLidRotation = Math.Min(trashCanLidRotation + 0.06544985f, 1.57079637f);
                    if (heldItem != null && Utility.getTrashReclamationPrice(heldItem, Game1.player) > 0)
                    {
                        hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
                        hoverAmount = Utility.getTrashReclamationPrice(heldItem, Game1.player);
                        return;
                    }
                }
                else
                {
                    trashCanLidRotation = Math.Max(trashCanLidRotation - 0.06544985f, 0f);
                }
                locationText.Hover(x, y);
            }
            else
            {
                for (int i = 0; i < storageList.Count; i++)
                {
                    for (int j = 0; j < widgetText.Length; j++)
                    {
                        if (GetWidgetRectangle(storageList[i], j).Contains(new Point(x, y)))
                        {
                            hoverText = widgetText[j];
                            return;
                        }
                    }
                    if (storageList[i].collapsed)
                        continue;
                    item_grab_hovered_item = storageList[i].menu.hover(x, y, heldItem);
                    if (item_grab_hovered_item != null)
                    {
                        hoveredItem = item_grab_hovered_item;
                    }
                }
            }


        }

        public void SwapMenus(int idx1, int idx2)
        {
            if (ModEntry.SHelper.Input.IsDown(SButton.LeftShift))
            {
                SwapContents(allStorageList[idx1].chest.items, allStorageList[idx2].chest.items);
                heldMenu = -1;
                return;
            }
            StorageData storageData = allStorageList[idx1];
            allStorageList[idx1] = allStorageList[idx2];
            allStorageList[idx1].index = idx1;
            allStorageList[idx2] = storageData;
            allStorageList[idx2].index = idx2;
            if(targetStorage is not null)
            {
                if (targetStorage.index == idx1)
                {
                    targetStorage = allStorageList[idx1];
                }
                else if (targetStorage.index == idx2)
                {
                    targetStorage = allStorageList[idx2];
                }
            }
            PopulateMenus();
            heldMenu = -1;
        }

        public void SwapContents(IList<Item> storageData1, IList<Item> storageData2, bool same = false)
        {
            if(!same)
                same = ModEntry.SHelper.Input.IsDown(ModEntry.Config.ModKey2);
            for (int i = 0; i < storageData1.Count; i++)
            {
                var item = storageData1[i];
                if (item is null)
                    continue;
                if (same)
                {
                    bool contains = false;
                    foreach (var m in storageData2)
                    {
                        if (m is not null && m.Name == item.Name)
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains)
                        continue;
                }
                var newItem = AddItem(storageData2, item);
                if (newItem is null)
                {
                    storageData1[i] = null;
                }
                else
                {
                    storageData1[i].Stack = newItem.Stack;
                }
            }
        }

        public Item AddItem(IList<Item> item_list, Item item)
        {
            for (int i = 0; i < item_list.Count; i++)
            {
                if (item_list[i] != null && item_list[i].canStackWith(item))
                {
                    item.Stack = item_list[i].addToStack(item);
                    if (item.Stack <= 0)
                    {
                        return null;
                    }
                }
            }
            for(int i = 0; i < item_list.Count; i++)
            {
                if (item_list[i] is null)
                {
                    item_list[i] = item;
                    return null;
                }
            }
            return item;
        }
        public Rectangle GetWidgetRectangle(StorageData storage, int v)
        {
            return new Rectangle(storage.menu.xPositionOnScreen + storage.menu.width + 4, storage.menu.yPositionOnScreen + v * 33, 32, 32);
        }
        public void ClickWidget(StorageData storageData, int idx)
        {
            switch (idx)
            {
                case 0:
                    Game1.playSound("bigSelect");
                    storageData.chest.ShowMenu();
                    break;
                case 1:
                    Game1.playSound("Ship");
                    ItemGrabMenu.organizeItemsInList(storageData.chest.items);
                    break;
                case 2:
                    Game1.playSound("stoneStep");
                    SwapContents(Game1.player.Items, storageData.chest.items);
                    break;
                case 3:
                    Game1.playSound("stoneStep");
                    SwapContents(storageData.chest.items, Game1.player.Items);
                    break;
                case 4:
                    Game1.playSound("bigSelect");
                    Rename(storageData);
                    break;
                case 5:
                    if(targetStorage?.index == storageData.index) 
                    {
                        Game1.playSound("bigDeSelect");
                        targetStorage = null;
                    }
                    else
                    {
                        Game1.playSound("bigSelect");
                        targetStorage = storageData;
                    }
                    break;

            }
        }

        public void Rename(StorageData storageData)
        {
            renamingStorage = storageData;
            renameBox.Selected = true;
            renameBox.Text = storageData.chest.modData.TryGetValue(chestsAnywhereKey, out string name) ? name : "";
        }
    }
}