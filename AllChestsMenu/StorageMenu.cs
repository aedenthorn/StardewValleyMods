using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AllChestsMenu
{
    public class StorageMenu : IClickableMenu
    {
        public static int scrolled;
        public static int windowWidth = 64 * 26;
        public static StorageMenu instance;
        
        public int xSpace = 64;
        public int scrollInterval = 32;
        public bool focusBottom = false;
        public enum Sort
        {
            LA,
            LD,
            NA,
            ND,
            CA,
            CD,
            IA,
            ID
        }
        public Sort currentSort = Sort.LA;
        public List<StorageData> allStorageList = new List<StorageData>();
        public List<StorageData> storageList = new List<StorageData>();
        public InventoryMenu playerInventoryMenu;
        public bool canScroll;
        public Item heldItem;
        public TemporaryAnimatedSprite poof;
        public List<ClickableComponent> inventoryCells = new List<ClickableComponent>();
        public List<ClickableComponent> sortCCList = new List<ClickableComponent>();
        public List<ClickableTextureComponent> inventoryButtons = new List<ClickableTextureComponent>();
        public List<Rectangle> widgetSources = new List<Rectangle>()
        {
            new Rectangle(257, 284, 16, 16), 
            new Rectangle(162, 440, 16, 16), 
            new Rectangle(420, 457, 14, 14), 
            new Rectangle(420, 471, 14, 14), 
            new Rectangle(240, 320, 16, 16), 
            new Rectangle(653, 205, 44, 44)
        };
        public ClickableTextureComponent trashCan;
        public ClickableTextureComponent organizeButton;
        public TextBox locationText;
        public ClickableComponent lastTopSnappedCC;
        public ClickableComponent locationTextCC;
        public ClickableComponent renameBoxCC;
        public Item hoveredItem;
        public string hoverText;
        public string chestLocation;
        public int hoverAmount;
        public float trashCanLidRotation;
        public int heldMenu = -1;
        public int ccMagnitude = 10000000;
        public int cutoff;
        public string whichLocation;
        public string[] widgetText;
        public Dictionary<string, string> sortNames = new();
        public StorageData targetStorage;
        public StorageData renamingStorage;
        public TextBox renameBox;
        public ClickableTextureComponent okButton;
        public ClickableTextureComponent storeAlikeButton;
        public string chestsAnywhereKey = "Pathoschild.ChestsAnywhere/Name";
        public string filterString;
        public string nameString;
        public string fridgeString;
        private string sortString;

        public StorageMenu() : base(Game1.uiViewport.Width / 2 - (windowWidth + borderWidth * 2) / 2, -borderWidth - 64, windowWidth + borderWidth * 2, Game1.uiViewport.Height + borderWidth * 2 + 64, false)
        {
            currentSort = ModEntry.Config.CurrentSort;
            instance = this;
            cutoff = Game1.uiViewport.Height - 64 * 3 - 8 - borderWidth;

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
            sortString = ModEntry.SHelper.Translation.Get("sort");

            var columns = 12;
            var rows = Math.Min(3, (int)Math.Ceiling(Game1.player.Items.Count / (float)columns));
            var cap = rows * columns;
            
            playerInventoryMenu = new InventoryMenu((Game1.uiViewport.Width - 64 * columns) / 2, Game1.uiViewport.Height - 64 * 3 - borderWidth / 2, false, Game1.player.Items, null, cap, rows);
            SetPlayerInventoryNeighbours();

            trashCan = new ClickableTextureComponent(new Rectangle(playerInventoryMenu.xPositionOnScreen + playerInventoryMenu.width + 64 + 32 + 8, playerInventoryMenu.yPositionOnScreen + 64 + 16, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f, false)
            {
                myID = 4 * ccMagnitude + 2,
                leftNeighborID = 11,
                upNeighborID = 4 * ccMagnitude,
                rightNeighborID = 5 * ccMagnitude
            };
            organizeButton = new ClickableTextureComponent("", new Rectangle(playerInventoryMenu.xPositionOnScreen + playerInventoryMenu.width + 64, playerInventoryMenu.yPositionOnScreen, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), 4f, false)
            {
                myID = 4 * ccMagnitude,
                downNeighborID = 4 * ccMagnitude + 2,
                leftNeighborID = 11,
                rightNeighborID = 4 * ccMagnitude + 1
            };
            storeAlikeButton =  new ClickableTextureComponent("", new Rectangle(playerInventoryMenu.xPositionOnScreen + playerInventoryMenu.width + 64 + 64 + 16, playerInventoryMenu.yPositionOnScreen, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_FillStacks"), Game1.mouseCursors, new Rectangle(103, 469, 16, 16), 4f, false)
            {
                myID = 4 * ccMagnitude + 1,
                downNeighborID = 4 * ccMagnitude + 2,
                leftNeighborID = 4 * ccMagnitude,
                rightNeighborID = 5 * ccMagnitude,
                upNeighborID = 4 * ccMagnitude
            };
            locationText = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = xPositionOnScreen + borderWidth,
                Width = (width - playerInventoryMenu.width) / 2 - borderWidth * 2 - 32,
                Y = cutoff + borderWidth + 32,
                Text = whichLocation
            };
            locationTextCC = new ClickableComponent(new Rectangle(locationText.X, locationText.Y, locationText.Width, locationText.Height), "")
            {
                myID = 2 * ccMagnitude,
                upNeighborID = 1 * ccMagnitude,
                rightNeighborID = 0,
                downNeighborID = 2 * ccMagnitude + 1
            }; 
            renameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = locationText.X,
                Width = locationText.Width,
                Y = locationText.Y + locationText.Height + 48
            };
            renameBoxCC = new ClickableComponent(new Rectangle(renameBox.X, renameBox.Y, renameBox.Width, renameBox.Height), "")
            {
                myID = 2 * ccMagnitude + 1,
                upNeighborID = 2 * ccMagnitude,
                rightNeighborID = 2 * ccMagnitude + 2
            };
            locationText.Selected = false;
            renameBox.Selected = false;
            okButton = new ClickableTextureComponent(new Rectangle(renameBox.X + renameBox.Width + 4, renameBox.Y, 48, 48), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 0.75f, false)
            {
                myID = 2 * ccMagnitude + 2,
                leftNeighborID = 2 * ccMagnitude + 1,
                rightNeighborID = 0
            };
            var s = Enum.GetNames(typeof(Sort));
            for(int i = 0; i < s.Length; i++)
            {
                int row = i % 2;
                string name = s[i];
                sortNames[name] = ModEntry.SHelper.Translation.Get("sort-" + name);
                int idx = 5 * ccMagnitude;
                sortCCList.Add(new ClickableComponent(new Rectangle(organizeButton.bounds.X + 156 + i / 2 * 48 + 32, organizeButton.bounds.Y + 64 + row * 48 + 16, 32, 32), name, name)
                {
                    myID = idx + i,
                    leftNeighborID = i > 2 ? idx + i - 2: 4 * ccMagnitude + 1,
                    rightNeighborID = i < s.Length - 2 ? idx + i + 2 : -1,
                    downNeighborID = row == 0 ? idx + i + 1 : -1,
                    upNeighborID = row == 1 ? idx + i - 1 : -1
                });
            }

            exitFunction = emergencyShutDown;

            PopulateMenus(true);
            snapToDefaultClickableComponent();
        }

        public void PopulateMenus(bool remakeChests = false)
        {
            if (remakeChests || (ModEntry.Config.LimitToCurrentLocation && chestLocation != Game1.currentLocation.Name))
            {
                chestLocation = Game1.currentLocation.Name;
                allStorageList.Clear();
                List<GameLocation> list = new();
                if (ModEntry.Config.LimitToCurrentLocation)
                {
                    list.Add(Game1.currentLocation);
                }
                else
                {
                    list = new(Game1.locations);
                    foreach (var l in list.ToArray())
                    {
                        if (l is BuildableGameLocation)
                        {
                            foreach (var b in (l as BuildableGameLocation).buildings)
                            {
                                if (b is not ShippingBin && b.indoors.Value is not null)
                                    list.Add(b.indoors.Value);
                            }
                        }
                    }
                }
                foreach (var l in list)
                {
                    if (l is FarmHouse)
                    {
                        Chest chest = (l as FarmHouse).fridge.Value;
                        RestoreNulls(chest.items);
                        string key = l.Name;
                        if (!chest.modData.TryGetValue("Pathoschild.ChestsAnywhere/Name", out string chestName) || string.IsNullOrEmpty(chestName))
                        {
                            key += " " + fridgeString;
                            chestName = fridgeString;
                        }
                        else
                        {
                            key += $" {chestName}";
                        }
                        var columns = 12;
                        var rows = Math.Max((int)Math.Ceiling(chest.items.Count / (float)columns), 3);
                        var cap = rows * columns;
                        allStorageList.Add(new StorageData() { chest = chest, name = chestName, location = l.Name, tile = new Vector2(-1, -1), label = key, index = allStorageList.Count });
                    }
                    foreach (var kvp in l.objects.Pairs)
                    {
                        var obj = kvp.Value;
                        Chest chest;
                        if (obj is Chest && (obj as Chest).playerChest.Value && (obj as Chest).CanBeGrabbed)
                        {
                            chest = obj as Chest;
                        }
                        else if (obj.heldObject.Value is Chest)
                        {
                            chest = obj.heldObject.Value as Chest;
                        }
                        else 
                            continue;
                        RestoreNulls(chest.items);
                        string key = $"{l.Name} {kvp.Key.X},{kvp.Key.Y}";
                        if (obj.modData.TryGetValue(chestsAnywhereKey, out string chestName) && !string.IsNullOrEmpty(chestName))
                        {
                            key = $"{chestName} ({key})";
                        }
                        else
                        {
                            chestName = "";
                        }

                        allStorageList.Add(new StorageData() { chest = chest, name = chestName, location = l.Name, tile = new Vector2(kvp.Key.X, kvp.Key.Y), label = key, index = allStorageList.Count });

                    }
                }
                SortAllStorages();
            }

            storageList.Clear();
            int menusAlready = 0;
            int rowsAlready = 0;
            bool even = false;
            int oddRows = 0;
            for (int i = 0; i < allStorageList.Count; i++)
            {
                allStorageList[i].index = i;
                var storage = allStorageList[i];

                if (!string.IsNullOrEmpty(whichLocation) && !storage.label.ToLower().Contains(whichLocation.ToLower()))
                    continue;
                var columns = 12;
                var rows = Math.Max((int)Math.Ceiling(storage.chest.items.Count / (float)columns), 3);
                var cap = rows * columns;
                storage.menu = new InventoryMenu(xPositionOnScreen + borderWidth + (even ? (64 * 13) : 0), yPositionOnScreen - scrolled * scrollInterval + borderWidth + 64 + 64 * rowsAlready + xSpace * (1 + menusAlready), false, storage.chest.items, null, cap, rows);
                if (!even)
                {
                    oddRows = (!storage.collapsed ? storage.menu.rows : 0);
                }
                else
                {
                    rowsAlready += Math.Max((!storage.collapsed ? storage.menu.rows : 0), oddRows);
                    menusAlready++;
                }
                even = !even;
                if (storageList.Count >= 1000)
                {
                    ModEntry.SMonitor.Log("More than 1000 chests. Giving up while we're ahead.", LogLevel.Warn);
                    break;
                }
                storageList.Add(storage);
            }
            inventoryButtons.Clear();
            inventoryCells.Clear();
            
            for (int i = 0; i < storageList.Count; i++)
            {
                var storage = storageList[i];
                var count = storage.menu.inventory.Count;
                var lastCount = i > 0 ? storageList[i - 1].menu.inventory.Count : 0;
                var nextCount = i < storageList.Count - 1 ? storageList[i + 1].menu.inventory.Count : 0;
                var lastLastCount = i > 1 ? storageList[i - 2].menu.inventory.Count : 0;
                var nextNextCount = i < storageList.Count - 2 ? storageList[i + 2].menu.inventory.Count : 0;
                var index = ccMagnitude + i * ccMagnitude / 1000;
                var lastIndex = ccMagnitude + (i - 1) * ccMagnitude / 1000;
                var nextIndex = ccMagnitude + (i + 1) * ccMagnitude / 1000;
                var lastLastIndex = ccMagnitude + (i - 2) * ccMagnitude / 1000;
                var nextNextIndex = ccMagnitude + (i + 2) * ccMagnitude / 1000;
                for (int j = 0; j < count; j++)
                {
                    storage.menu.inventory[j].myID = index + j;
                    if (j % 12 == 0)
                    {
                        if (i > 0 && lastCount > 0)
                        {
                            int row = (int)((j / 12) / (float)(count / 12) * widgetText.Length);
                            storage.menu.inventory[j].leftNeighborID = lastIndex + lastCount + row;
                        }
                        else
                        {
                            storage.menu.inventory[j].leftNeighborID = -1;
                        }
                    }
                    else
                    {
                        storage.menu.inventory[j].leftNeighborID = index + j - 1;
                    }
                    if (j % 12 == 11)
                    {
                        int row = (int)((j / 12) / (float)(count / 12) * widgetText.Length);

                        storage.menu.inventory[j].rightNeighborID = index + count + row;
                    }
                    else
                    {
                        storage.menu.inventory[j].rightNeighborID = index + j + 1;
                    }
                    if (j >= count - 12)
                    {
                        if(i < storageList.Count - 2)
                        {
                            storage.menu.inventory[j].downNeighborID =  + nextNextIndex + j % 12;
                        }
                        else
                        {
                            storage.menu.inventory[j].downNeighborID = -1;
                        }
                    }
                    else
                    {
                        storage.menu.inventory[j].downNeighborID = index + j + 12;
                    }
                    if (j < 12)
                    {
                        if(i > 1)
                        {
                            storage.menu.inventory[j].upNeighborID = lastLastIndex + lastLastCount - (12 - j);
                        }
                        else
                        {
                            storage.menu.inventory[j].upNeighborID = -1;
                        }
                    }
                    else
                    {
                        storage.menu.inventory[j].upNeighborID = index + j - 12;
                    }
                    inventoryCells.Add(storage.menu.inventory[j]);
                }
                storage.inventoryButtons.Clear();
                for (int j = 0; j < widgetText.Length; j++)
                {
                    int row = (int)(j / (float)widgetText.Length * (count / 12));
                    var cc = new ClickableTextureComponent("", GetWidgetRectangle(storage, j), "", widgetText[j], Game1.mouseCursors, widgetSources[j], 32f / widgetSources[j].Width, false)
                    {
                        myID = index + count + j,
                        downNeighborID = j < widgetText.Length - 1 ? index + count + j + 1 : (i < storageList.Count - 2 ? nextNextIndex + nextNextCount : -1),
                        leftNeighborID = index + 11 + row * 12,
                        rightNeighborID = i < storageList.Count - 1 ? nextIndex + row * 12 : -1,
                        upNeighborID = j > 0 ? index + count + j - 1: (i > 1 ? lastLastIndex + lastLastCount + widgetText.Length - 1: -1)
                    };
                    storage.inventoryButtons.Add(cc);
                    inventoryButtons.Add(cc);
                }
            }
            populateClickableComponentList();
        }


        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true);
            for (int i = 0; i < storageList.Count; i++)
            {
                var storage = storageList[i];
                canScroll = storage.menu.yPositionOnScreen + storage.menu.rows * 64 + borderWidth > cutoff;
                if (canScroll && storage.menu.yPositionOnScreen - 48 > cutoff + 64 * 4)
                {
                    break;
                }
                if (i == heldMenu - (storageList[i].index - i))
                    continue;
                SpriteText.drawString(b, storage.label, storage.menu.xPositionOnScreen, storage.menu.yPositionOnScreen - 48);
                if (!storage.collapsed)
                {
                    storage.menu.draw(b);
                    for (int j = 0; j < storage.inventoryButtons.Count; j++)
                    {
                        storage.inventoryButtons[j].draw(b, targetStorage?.index != storage.index && j == storage.inventoryButtons.Count - 1 ? Color.Purple : Color.White, 1);
                    }
                }
            }
            Game1.drawDialogueBox(xPositionOnScreen, cutoff - borderWidth * 2, width, 64 * 4 + borderWidth * 2, false, true, null, false, true);
            playerInventoryMenu.draw(b);
            SpriteText.drawString(b, filterString, locationText.X + 16, locationText.Y - 48);
            locationText.Draw(b);
            if (renamingStorage is not null)
            {
                SpriteText.drawString(b, nameString, renameBox.X + 16, renameBox.Y - 48);
                renameBox.Draw(b);
                okButton.draw(b);
            }
            SpriteText.drawStringHorizontallyCenteredAt(b, sortString, organizeButton.bounds.X + 156 + 32 * 2 + 24 + 32, organizeButton.bounds.Y + 16);
            foreach(var cc in sortCCList)
            {
                b.DrawString(Game1.smallFont, cc.label, cc.bounds.Location.ToVector2() + new Vector2(-1, 1), currentSort.ToString() == cc.label ? Color.Green : Color.Black);
                b.DrawString(Game1.smallFont, cc.label, cc.bounds.Location.ToVector2(), currentSort.ToString() == cc.label ? Color.LightGreen : Color.White);
            }
            trashCan.draw(b);
            organizeButton.draw(b);
            storeAlikeButton.draw(b);

            b.Draw(Game1.mouseCursors, new Vector2(trashCan.bounds.X + 60, trashCan.bounds.Y + 40), new Rectangle?(new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10)), Color.White, trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
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
                heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
            }
            if (heldMenu > -1)
            {
                SpriteText.drawString(b, allStorageList[heldMenu].label, Game1.getOldMouseX(), Game1.getOldMouseY() - 48);
                b.Draw(Game1.staminaRect, new Rectangle(Game1.getOldMouseX(), Game1.getOldMouseY(), 64 * 12, allStorageList[heldMenu].menu.rows * 64), Color.LightGray * 0.5f);
            }
            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            Item held = heldItem;
            Rectangle rect;
            renameBox.Selected = false;
            locationText.Selected = false;
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
                    return;
                }
                if (renamingStorage is not null)
                {
                    renameBox.Update();
                    if (okButton.containsPoint(x, y))
                    {
                        RenameStorage();
                        return;
                    }
                }
                locationText.Update();

                if (trashCan != null && trashCan.containsPoint(x, y) && heldItem != null && heldItem.canBeTrashed())
                {
                    Utility.trashItem(heldItem);
                    heldItem = null;
                    return;
                }
                if (organizeButton.containsPoint(x, y))
                {
                    Game1.playSound("Ship");
                    ItemGrabMenu.organizeItemsInList(Game1.player.Items);
                    return;
                }
                if (storeAlikeButton.containsPoint(x, y))
                {
                    Game1.playSound("Ship");
                    foreach (var s in storageList)
                    {
                        SwapContents(Game1.player.Items, s.chest.items, true);
                    }
                    return;
                }
                foreach (var cc in sortCCList)
                {
                    if(cc.containsPoint(x, y))
                    {
                        Game1.playSound("bigSelect");
                        currentSort = (Sort)Enum.Parse(typeof(Sort), cc.name);
                        ModEntry.Config.CurrentSort = currentSort;
                        ModEntry.SHelper.WriteConfig(ModEntry.Config);
                        SortAllStorages();
                        PopulateMenus();
                        return;
                    }
                }
            }
            else
            {
                for (int i = 0; i < storageList.Count; i++)
                {
                    for(int j = 0; j < storageList[i].inventoryButtons.Count; j++)
                    {
                        if (storageList[i].inventoryButtons[j].containsPoint(x, y))
                        {
                            ClickWidget(storageList[i], j);
                            return;
                        }
                    }
                    rect = new Rectangle(storageList[i].menu.xPositionOnScreen, storageList[i].menu.yPositionOnScreen - 48, (width - borderWidth * 2 - 64) / 2, 48);
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
                    /*
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
                    */
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
                Game1.playSound("shiny4");
                scrolled++;
            }   
            else if (scrolled > 0) 
            {
                Game1.playSound("shiny4");
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
            if (key.Equals(Keys.Delete) && heldItem != null && heldItem.canBeTrashed())
            {
                Utility.trashItem(heldItem);
            }
            if (key.Equals(Keys.Enter) && renameBox.Selected)
            {
                if(renamingStorage is not null)
                {
                    RenameStorage();
                }
                renameBox.Selected = false;
            }
        }


        public override void snapToDefaultClickableComponent()
        {
            if (!Game1.options.snappyMenus || !Game1.options.gamepadControls)
                return;
            if (currentlySnappedComponent == null)
            {
                if (focusBottom)
                {
                    currentlySnappedComponent = getComponentWithID(0);
                }
                else
                {
                    if (lastTopSnappedCC is not null)
                    {
                        currentlySnappedComponent = lastTopSnappedCC;
                        lastTopSnappedCC = null;
                    }
                    else
                    {
                        currentlySnappedComponent = getComponentWithID(ccMagnitude);
                    }
                }
            }


            snapCursorToCurrentSnappedComponent();
        }
        public override void applyMovementKey(int direction)
        {
            if (currentlySnappedComponent != null)
            {
                ClickableComponent next;
                var old = currentlySnappedComponent;
                switch (direction)
                {
                    case 0:
                        next = getComponentWithID(currentlySnappedComponent.upNeighborID);
                        if (focusBottom)
                        {
                            if (currentlySnappedComponent.myID < playerInventoryMenu.inventory.Count)
                            {
                                base.applyMovementKey(direction);
                                SetPlayerInventoryNeighbours();
                                return;
                            }
                        }
                        else
                        {
                            if (next is not null)
                            {
                                if (next.bounds.Y < 0)
                                {
                                    var id = currentlySnappedComponent.myID;
                                    scrolled -= (int)Math.Round(64f / scrollInterval);
                                    PopulateMenus(false);
                                    currentlySnappedComponent = getComponentWithID(id);
                                    snapCursorToCurrentSnappedComponent();
                                    break;
                                }
                            }
                        }
                        if (next is not null)
                        {
                            currentlySnappedComponent = next;
                            snapCursorToCurrentSnappedComponent();
                        }
                        break;
                    case 1:
                        if (currentlySnappedComponent.rightNeighborID != -1)
                        {
                            currentlySnappedComponent = getComponentWithID(currentlySnappedComponent.rightNeighborID);
                            snapCursorToCurrentSnappedComponent();
                        }
                        break;
                    case 2:
                        next = getComponentWithID(currentlySnappedComponent.downNeighborID);
                        if (!focusBottom && next is not null && next.bounds.Y + next.bounds.Height > cutoff)
                        {
                            var id = currentlySnappedComponent.myID;
                            scrolled += (int)Math.Round(64f / scrollInterval);
                            PopulateMenus(false);
                            currentlySnappedComponent = getComponentWithID(id);
                            snapCursorToCurrentSnappedComponent();
                            break;
                        }
                        if(focusBottom && currentlySnappedComponent.myID < playerInventoryMenu.inventory.Count)
                        {
                            base.applyMovementKey(direction);
                            SetPlayerInventoryNeighbours();
                            return;
                        }
                        if (next is not null)
                        {
                            currentlySnappedComponent = next;
                            snapCursorToCurrentSnappedComponent();
                        }
                        break;
                    case 3:
                        if (currentlySnappedComponent.leftNeighborID != -1)
                        {
                            currentlySnappedComponent = getComponentWithID(currentlySnappedComponent.leftNeighborID);
                            snapCursorToCurrentSnappedComponent();
                        }
                        break;
                }
                if (currentlySnappedComponent != old)
                {
                    Game1.playSound("shiny4");
                }
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
                lastTopSnappedCC = getComponentWithID(ccMagnitude);
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
                    return;
                }
                organizeButton.tryHover(x, y, 0.1f);
                if (organizeButton.containsPoint(x, y))
                {
                    hoverText = organizeButton.hoverText;
                    return;
                }
                storeAlikeButton.tryHover(x, y, 0.1f);
                if (storeAlikeButton.containsPoint(x, y))
                {
                    hoverText = storeAlikeButton.hoverText;
                    return;
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
                    }
                    return;
                }
                else
                {
                    trashCanLidRotation = Math.Max(trashCanLidRotation - 0.06544985f, 0f);
                }
                locationText.Hover(x, y);

                foreach (var cc in sortCCList)
                {
                    if (cc.containsPoint(x, y))
                    {
                        hoverText = sortNames[cc.name];
                        return;
                    }
                }
            }
            else
            {
                for (int i = 0; i < storageList.Count; i++)
                {
                    for (int j = 0; j < storageList[i].inventoryButtons.Count; j++)
                    {
                        storageList[i].inventoryButtons[j].tryHover(x, y);
                        if (storageList[i].inventoryButtons[j].containsPoint(x, y))
                        {
                            hoverText = storageList[i].inventoryButtons[j].hoverText;
                            return;
                        }
                    }
                    if (storageList[i].collapsed)
                        continue;
                    item_grab_hovered_item = storageList[i].menu.hover(x, y, heldItem);
                    if (item_grab_hovered_item != null)
                    {
                        hoveredItem = item_grab_hovered_item;
                        return;
                    }
                }
            }


        }
        public override void emergencyShutDown()
        {
            base.emergencyShutDown();
            if (heldItem != null)
            {
                Console.WriteLine("Taking " + heldItem.Name);
                heldItem = Game1.player.addItemToInventory(heldItem);
            }
            if (heldItem != null)
            {
                DropHeldItem();
            }
        }
        public virtual void DropHeldItem()
        {
            if (heldItem == null)
            {
                return;
            }
            Game1.playSound("throwDownITem");
            int drop_direction = Game1.player.facingDirection;
            Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), drop_direction, null, -1);
            heldItem = null;
        }

        public void SwapMenus(int idx1, int idx2)
        {
            if (ModEntry.SHelper.Input.IsDown(ModEntry.Config.ModKey))
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
            while(items.Count % 12 != 0) 
            {
                items.Add(null);
            }
        }
        public void Rename(StorageData storageData)
        {
            renamingStorage = storageData;
            renameBox.Selected = true;
            renameBox.Text = storageData.chest.modData.TryGetValue(chestsAnywhereKey, out string name) ? name : "";
            if (currentlySnappedComponent is not null)
            {
                focusBottom = true;
                lastTopSnappedCC = currentlySnappedComponent;
                currentlySnappedComponent = renameBoxCC;
                snapCursorToCurrentSnappedComponent();
            }
        }

        private void RenameStorage()
        {
            if (string.IsNullOrEmpty(renameBox.Text))
            {
                allStorageList[renamingStorage.index].chest.modData[chestsAnywhereKey] = "";
                allStorageList[renamingStorage.index].label = $"{renamingStorage.location} {(renamingStorage.tile.X > -1 ? renamingStorage.tile.X + "," + renamingStorage.tile.Y : fridgeString)}";
            }
            else
            {
                allStorageList[renamingStorage.index].chest.modData[chestsAnywhereKey] = renameBox.Text;
                allStorageList[renamingStorage.index].label = $"{renameBox.Text} ({renamingStorage.location} {(renamingStorage.tile.X > -1 ? renamingStorage.tile.X + "," + renamingStorage.tile.Y : fridgeString)})";
            }
            renamingStorage = null;
            renameBox.Selected = false;
            Game1.playSound("bigSelect");
            if (lastTopSnappedCC is not null)
            {
                focusBottom = false;
                currentlySnappedComponent = lastTopSnappedCC;
                snapCursorToCurrentSnappedComponent();
            }
            PopulateMenus();
        }

        private void SortAllStorages()
        {
            allStorageList.Sort(delegate (StorageData a, StorageData b)
            {
                string sa;
                string sb;
                int result = 0;
                switch (currentSort)
                {
                    case Sort.LA:
                        if (a.location == b.location)
                            result = a.name.CompareTo(b.name);
                        else
                            result = a.location.CompareTo(b.location);
                        break;
                    case Sort.LD:
                        if (a.location == b.location)
                            result = b.name.CompareTo(a.name);
                        result = b.location.CompareTo(a.location);
                        break;
                    case Sort.NA:
                        sa = a.name;
                        sb = b.name;
                        if (string.IsNullOrEmpty(sa))
                            sa = a.location;
                        if (string.IsNullOrEmpty(sb))
                            sb = b.location;
                        result = sa.CompareTo(sb);
                        break;
                    case Sort.ND:
                        sa = a.name;
                        sb = b.name;
                        if (string.IsNullOrEmpty(sa))
                            sa = a.location;
                        if (string.IsNullOrEmpty(sb))
                            sb = b.location;
                        result = sb.CompareTo(sa);
                        break;
                    case Sort.CA:
                        result = a.chest.items.Count.CompareTo(b.chest.items.Count);
                        break;
                    case Sort.CD:
                        result = b.chest.items.Count.CompareTo(a.chest.items.Count);
                        break;
                    case Sort.IA:
                        result = a.chest.items.Where(i => i is not null).Count().CompareTo(b.chest.items.Where(i => i is not null).Count());
                        break;
                    case Sort.ID:
                        result = b.chest.items.Where(i => i is not null).Count().CompareTo(a.chest.items.Where(i => i is not null).Count());
                        break;
                }
                if(result == 0)
                {
                    result = a.index.CompareTo(b.index);
                }
                return result;
            });
        }
        private void SetPlayerInventoryNeighbours()
        {
            if (playerInventoryMenu.inventory.Count >= 12)
            {
                playerInventoryMenu.inventory[0].leftNeighborID = 2 * ccMagnitude;
                playerInventoryMenu.inventory[11].rightNeighborID = 4 * ccMagnitude;
                if (playerInventoryMenu.inventory.Count >= 24)
                {
                    playerInventoryMenu.inventory[12].leftNeighborID = 2 * ccMagnitude;
                    playerInventoryMenu.inventory[23].rightNeighborID = 4 * ccMagnitude + 1;
                    if (playerInventoryMenu.inventory.Count >= 36)
                    {
                        playerInventoryMenu.inventory[24].leftNeighborID = 2 * ccMagnitude;
                        playerInventoryMenu.inventory[35].rightNeighborID = 4 * ccMagnitude + 1;
                    }
                }
            }
        }
    }
}
