using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmPlots
{
    public class FarmPlotsMenu : IClickableMenu
    {

        public static int windowWidth = 64 * 16;

        public int perRow = 8;
        public int rows = 3;
        public int lineHeight = 72;
        public int maxHeight;
        public int seedScroll;
        public int fertScroll;

        public List<ClickableTextureComponent> tabs = new List<ClickableTextureComponent>();
        public List<ParsedItemData>[] seedList = new List<ParsedItemData>[4];
        public List<ParsedItemData> fertilizerList = new List<ParsedItemData>();
        public List<ClickableTextureComponent> seedButtons = new List<ClickableTextureComponent>();
        public List<ClickableTextureComponent> fertilizerButtons = new List<ClickableTextureComponent>();
        public ClickableTextureComponent harvestButton;
        public ClickableTextureComponent tillButton;
        public ClickableTextureComponent buyButton;
        public ClickableTextureComponent activeButton;
        public ClickableTextureComponent updateButton;
        
        public Rectangle seedRect;
        public Rectangle fertRect;
        
        public int[] heights = new int[4];
        public int currentSeason;

        public int baseID = 1000;

        public string hoverText;
        public string hoveredItem;

        public FarmPlotsMenu() : base(0, -borderWidth, windowWidth + borderWidth * 2, Game1.uiViewport.Height, false)
        {
            width = 64 * 8 + borderWidth * 2 + 40;
            if(ModEntry.currentPlot.Value == null && ModEntry.TryGetAutoPlots(Game1.currentLocation, out var list))
            {
                foreach(var p in list)
                {
                    if (p.tiles.Contains(Game1.player.Tile))
                    {
                        ModEntry.currentPlot.Value = p;
                        break;
                    }
                }
            }
            seedList = new List<ParsedItemData>[]
            {
                new(),
                new(),
                new(),
                new()
            };

            foreach (var d in Game1.cropData)
            {
                foreach (var s in d.Value.Seasons)
                {
                    for(int i = 0; i < 5; i++)
                    {
                        ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(O)" + d.Key);
                        seedList[(int)s].Add(itemData);
                    }
                }
            }

            fertilizerList.Clear();
            foreach (var d in Game1.objectData.Where(d => d.Value.Category == -19))
            {
                for (int i = 0; i < 5; i++)
                {
                    ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(O)" + d.Key);
                    fertilizerList.Add(itemData);
                }
            }
            heights = new int[4];
            perRow = 8;
            lineHeight = 72;
            maxHeight = 0;
            foreach (var s in Enum.GetValues(typeof(Season)))
            {
                var h = 400 + rows * 2 * lineHeight;
                if (h > maxHeight)
                    maxHeight = h;
                heights[(int)s] = h;
            }

            ScrollToSelected();
            RepopulateComponentList();
            exitFunction = emergencyShutDown;

            snapToDefaultClickableComponent();
        }

        public void RepopulateComponentList()
        {
            height = heights[currentSeason];
            yPositionOnScreen = Game1.uiViewport.Height / 2 - maxHeight / 2;

            int xStart = xPositionOnScreen + spaceToClearSideBorder + borderWidth;
            int yStart = yPositionOnScreen + borderWidth + spaceToClearTopBorder + 24;

            tabs.Clear();
            foreach (int s in Enum.GetValues(typeof(Season)))
            {
                tabs.Add(new ClickableTextureComponent("season" + s, new Rectangle(xPositionOnScreen + borderWidth - 4 + 48 * s, yPositionOnScreen + borderWidth + 56, 48, 32), "", Utility.getSeasonNameFromNumber(s), Game1.mouseCursors, new Rectangle(406, 441 + 8 * s, 12, 8), 4f)
                {
                    myID = baseID + s,
                    rightNeighborID = baseID + s + 1,
                    leftNeighborID = baseID + s - 1,
                    downNeighborID = baseID + 1000
                });
            }

            seedButtons.Clear();
            for (int i = seedScroll * perRow; i < seedList[currentSeason].Count && i < (seedScroll + rows) * perRow; i++)
            {
                int iOff = i - seedScroll * perRow;
                var s = seedList[currentSeason][i];
                int xoff = xStart + 64 * (iOff % perRow);
                int yoff = yStart + lineHeight * (iOff / perRow);
                seedButtons.Add(new ClickableTextureComponent(s.ItemId, new Rectangle(xoff, yoff, 64, 64), "", s.DisplayName, s.GetTexture(), s.GetSourceRect(), 4)
                {
                    myID = baseID + 1000 + iOff,
                    upNeighborID = baseID,
                    leftNeighborID = baseID + 1000 - 1,
                    rightNeighborID = baseID + 1000 + 1,
                    downNeighborID = baseID + 2000
                });
            }

            seedRect = new Rectangle(xStart, yStart, 64 * perRow, lineHeight * rows);

            fertilizerButtons = new();
            for (int j = fertScroll * perRow; j < fertilizerList.Count && j < (fertScroll + rows) * perRow; j++)
            {
                int jOff = j - fertScroll * perRow;

                var itemData = fertilizerList[j];
                int xoff = xStart + 64 * (jOff % perRow);
                int yoff = yStart + lineHeight * (jOff / perRow) + lineHeight * (rows + 1);

                fertilizerButtons.Add(new ClickableTextureComponent(itemData.ItemId, new Rectangle(xoff, yoff, 64, 64), "", itemData.DisplayName, itemData.GetTexture(), itemData.GetSourceRect(), 4)
                {
                    myID = baseID + 2000 + jOff,
                    upNeighborID = baseID + 1000,
                    leftNeighborID = baseID + 2000 - 1,
                    rightNeighborID = baseID + 2000 + 1,
                    downNeighborID = baseID + 3000
                });
            }

            fertRect = new Rectangle(xStart, yStart + lineHeight * (rows + 1), 64 * perRow, lineHeight * rows);


            int buttonWidth = 64;
            int buttonStartY = yStart + lineHeight * (rows * 2 + 2);

            harvestButton = new ClickableTextureComponent("harvest", new Rectangle(xStart, buttonStartY, 44, 44), "", ModEntry.SHelper.Translation.Get("Harvest"), Game1.mouseCursors, new Rectangle(76, 73, 40, 44), 1)
            {
                myID = baseID + 3000,
                upNeighborID = baseID + 2000,
                rightNeighborID = baseID + 3001,
            };
            tillButton = new ClickableTextureComponent("till", new Rectangle(xStart + buttonWidth, buttonStartY, 48, 48), "", ModEntry.SHelper.Translation.Get("Till"), ModEntry.SHelper.GameContent.Load<Texture2D>("TileSheets/tools"), new Rectangle(81, 34, 15, 15), 3)
            {
                myID = baseID + 3001,
                upNeighborID = baseID + 2000,
                leftNeighborID = baseID + 3000,
                rightNeighborID = baseID + 3002,
            };
            buyButton = new ClickableTextureComponent("buy", new Rectangle(xStart + buttonWidth * 2, buttonStartY, 48, 48), "", ModEntry.SHelper.Translation.Get("Buy"), Game1.mouseCursors, new Rectangle(280, 412, 15, 14), 3)
            {
                myID = baseID + 3002,
                upNeighborID = baseID + 2000,
                leftNeighborID = baseID + 3001,
                rightNeighborID = baseID + 3003,
            };
            activeButton = new ClickableTextureComponent("active", new Rectangle(xStart + buttonWidth * 3, buttonStartY, 48, 48), "", ModEntry.SHelper.Translation.Get("Active"), Game1.mouseCursors, new Rectangle(ModEntry.currentPlot.Value?.active[currentSeason] == true ? 236 : 227, 425, 9, 9), 5f)
            {
                myID = baseID + 3003,
                upNeighborID = baseID + 2000,
                leftNeighborID = baseID + 3002,
                rightNeighborID = baseID + 3004,
            };
            updateButton = new ClickableTextureComponent("update", new Rectangle(xStart + buttonWidth * 4, buttonStartY, 48, 48), "", ModEntry.SHelper.Translation.Get("Update"), Game1.mouseCursors, new Rectangle(447, 96, 32, 32), 1.5f)
            {
                myID = baseID + 3004,
                upNeighborID = baseID + 2000,
                leftNeighborID = baseID + 3003,
            };
            populateClickableComponentList();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            RepopulateComponentList();
        }

        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true);

            if(ModEntry.currentPlot.Value == null)
            {
                b.DrawString(Game1.dialogueFont, ModEntry.SHelper.Translation.Get("Select"), new Vector2(xPositionOnScreen + 40, yPositionOnScreen + height / 2 - 32), Color.Black);
                Game1.mouseCursorTransparency = 1f;
                drawMouse(b);
                return;
            }

            b.Draw(Game1.mouseCursors, new Rectangle(xPositionOnScreen + borderWidth - 8, yPositionOnScreen + 128, width - borderWidth *2 + 16, 24), new Rectangle?(new Rectangle(275, 313, 1, 6)), Color.White);

            //b.DrawString(Game1.dialogueFont, ModEntry.SHelper.Translation.Get("AutoPlot"), new Vector2(xPositionOnScreen + 40, yPositionOnScreen + 96), Color.Black);
            foreach (var s in seedButtons)
            {
                s.draw(b,(s.name == ModEntry.currentPlot.Value?.seeds[currentSeason] ? Color.White : Color.Gray * 0.5f), 0.86f);
            }
            if (seedButtons.Count < seedList[currentSeason].Count)
            {
                int xStart = xPositionOnScreen + width - borderWidth - 8;
                int yStart = yPositionOnScreen + borderWidth + spaceToClearTopBorder + 24;
                int totalRows = (int)Math.Ceiling(seedList[currentSeason].Count / (float)perRow);
                int height = (int)Math.Ceiling((float)(rows * lineHeight) / (totalRows - rows + 1));
                b.Draw(Game1.staminaRect, new Rectangle(xStart, yStart, 8, rows * lineHeight), Color.Gray);
                b.Draw(Game1.staminaRect, new Rectangle(xStart, yStart + seedScroll * height, 8, height), Color.DarkOrange);
            }
            if(fertilizerButtons.Count < fertilizerList.Count)
            {
                int xStart = xPositionOnScreen + width - borderWidth - 8;
                int yStart = yPositionOnScreen + borderWidth + spaceToClearTopBorder + 24 + lineHeight * (rows + 1);
                int totalRows = (int)Math.Ceiling(fertilizerList.Count / (float)perRow);
                int height = (int)Math.Ceiling((float)(rows * lineHeight) / (totalRows - rows + 1));
                b.Draw(Game1.staminaRect, new Rectangle(xStart, yStart, 8, rows * lineHeight), Color.Gray);
                b.Draw(Game1.staminaRect, new Rectangle(xStart, yStart + fertScroll * height, 8, height), Color.DarkOrange);
            }
            foreach (var s in fertilizerButtons)
            {
                s.draw(b, s.name == ModEntry.currentPlot.Value?.fertilizers[currentSeason] ? Color.White : Color.Gray * 0.5f, 0.86f);
            }
            foreach (var s in tabs)
            {
                s.draw(b, ("season" + currentSeason == s.name ? Color.White : Color.Gray * 0.5f), 0.86f);
            }
            harvestButton.draw(b, (ModEntry.currentPlot.Value?.harvest[currentSeason] == true ? Color.White : Color.Gray * 0.5f), 0.86f);
            tillButton.draw(b, (ModEntry.currentPlot.Value?.till[currentSeason] == true ? Color.White : Color.Gray * 0.5f), 0.86f);
            buyButton.draw(b, (ModEntry.currentPlot.Value?.buy[currentSeason] == true ? Color.White : Color.Gray * 0.5f), 0.86f);
            activeButton.draw(b);
            
            if(currentSeason == (int)Game1.season)
                updateButton.draw(b);

            if (hoverText != null && hoveredItem == null)
            {
                drawHoverText(b, hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null);
            }
            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (ModEntry.currentPlot.Value == null)
                return;
            for (int i = 0; i < seedButtons.Count; i++)
            {

                if (seedButtons[i].containsPoint(x, y))
                {
                    ModEntry.currentPlot.Value.seeds[currentSeason] = seedList[currentSeason][i + seedScroll * perRow].ItemId;
                    Game1.playSound("bigSelect");
                    SaveToLocation();
                    return;
                }
            }
            for (int i = 0; i < fertilizerButtons.Count; i++)
            {

                if (fertilizerButtons[i].containsPoint(x, y))
                {
                    ModEntry.currentPlot.Value.fertilizers[currentSeason] = fertilizerList[i + fertScroll * perRow].ItemId;
                    Game1.playSound("bigSelect");
                    SaveToLocation();
                    return;
                }
            }
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].containsPoint(x, y))
                {
                    currentSeason = i;
                    Game1.playSound("bigSelect");
                    ScrollToSelected();
                    RepopulateComponentList();
                    return;
                }
            }
            if(harvestButton.containsPoint(x, y))
            {
                ModEntry.currentPlot.Value.harvest[currentSeason] = !ModEntry.currentPlot.Value.harvest[currentSeason];
                Game1.playSound("bigSelect");
                SaveToLocation();
                return;
            }
            if(tillButton.containsPoint(x, y))
            {
                ModEntry.currentPlot.Value.till[currentSeason] = !ModEntry.currentPlot.Value.till[currentSeason];
                Game1.playSound("bigSelect");
                SaveToLocation();
                return;
            }
            if(buyButton.containsPoint(x, y))
            {
                ModEntry.currentPlot.Value.buy[currentSeason] = !ModEntry.currentPlot.Value.buy[currentSeason];
                Game1.playSound("bigSelect");
                SaveToLocation();
                return;
            }
            if(activeButton.containsPoint(x, y))
            {
                ModEntry.currentPlot.Value.active[currentSeason] = !ModEntry.currentPlot.Value.active[currentSeason];
                Game1.playSound("bigSelect");
                SaveToLocation();
                RepopulateComponentList();
                return;
            }
            if(updateButton.containsPoint(x, y) && currentSeason == (int)Game1.season)
            {
                ModEntry.ActivatePlot(Game1.currentLocation, ModEntry.currentPlot.Value);
                Game1.playSound("dirtyHit");
                return;
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            base.receiveRightClick(x, y, playSound);
        }


        public override void receiveKeyPress(Keys key)
        {
            bool close = Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose();
            if (close)
            {
                exitThisMenu(true);
            }
            else if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                applyMovementKey(key);
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (seedButtons.Count < seedList[currentSeason].Count && seedRect.Contains(Game1.getMousePosition(true)))
            {
                if (direction < 0 && seedScroll < Math.Ceiling(seedList[currentSeason].Count / (float)perRow) - rows)
                {
                    Game1.playSound("shiny4", null);
                    seedScroll++;
                    RepopulateComponentList();
                }
                else if (direction > 0 && seedScroll > 0)
                {
                    Game1.playSound("shiny4", null);
                    seedScroll--;
                    RepopulateComponentList();
                }
            }
            else if (fertilizerButtons.Count < fertilizerList.Count && fertRect.Contains(Game1.getMousePosition(true)))
            {
                if (direction < 0 && fertScroll < Math.Ceiling(fertilizerList.Count / (float)perRow) - rows)
                {
                    Game1.playSound("shiny4", null);
                    fertScroll++;
                    RepopulateComponentList();
                }
                else if (direction > 0 && fertScroll > 0)
                {
                    Game1.playSound("shiny4", null);
                    fertScroll--;
                    RepopulateComponentList();
                }
            }
            base.receiveScrollWheelAction(direction);
        }


        public override void snapToDefaultClickableComponent()
        {
            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                currentlySnappedComponent = this.getComponentWithID(0);
                snapCursorToCurrentSnappedComponent();
            }
        }

        public override void applyMovementKey(int direction)
        {

            if (currentlySnappedComponent != null)
            {
                ClickableComponent next = null;
                switch (direction)
                {
                    case 0:
                        next = getComponentWithID(currentlySnappedComponent.upNeighborID);
                        break;
                    case 1:
                        next = getComponentWithID(currentlySnappedComponent.rightNeighborID);
                        break;
                    case 2:
                        next = getComponentWithID(currentlySnappedComponent.downNeighborID);
                        break;
                    case 3:
                        next = getComponentWithID(currentlySnappedComponent.leftNeighborID);
                        break;
                }
                if (next is not null)
                {
                    Game1.playSound("shiny4");
                    currentlySnappedComponent = next;
                    snapCursorToCurrentSnappedComponent();
                }
            }
        }
        private void ScrollToSelected()
        {
            if (ModEntry.currentPlot.Value == null)
                return;
            if (ModEntry.currentPlot.Value.seeds[currentSeason] is string id && seedList[currentSeason].Count > rows * perRow)
            {
                seedScroll = seedList[currentSeason].FindIndex(d => d.ItemId == id) / perRow;
            }
            else
            {
                seedScroll = 0;
            }
            if (ModEntry.currentPlot.Value.fertilizers[currentSeason] is string fid && fertilizerList.Count > rows * perRow)
            {
                fertScroll = fertilizerList.FindIndex(d => d.ItemId == fid) / perRow;
            }
            else
            {
                fertScroll = 0;
            }
        }

        private void SaveToLocation()
        {
            Game1.currentLocation.modData[ModEntry.plotsKey] = JsonConvert.SerializeObject(ModEntry.locationDict[Game1.currentLocation]);
        }


        public override void update(GameTime time)
        {
            base.update(time);
        }

        public override void performHoverAction(int x, int y)
        {
            hoveredItem = null;
            hoverText = "";
            base.performHoverAction(x, y);

            for (int i = 0; i < seedButtons.Count; i++)
            {
                if (seedButtons[i].containsPoint(x, y))
                {
                    hoverText = seedButtons[i].hoverText;
                    return;
                }
            }
            for (int i = 0; i < fertilizerButtons.Count; i++)
            {
                if (fertilizerButtons[i].containsPoint(x, y))
                {
                    hoverText = fertilizerButtons[i].hoverText;
                    return;
                }
            }
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].containsPoint(x, y))
                {
                    hoverText = tabs[i].hoverText;
                    return;
                }
            }
            if(harvestButton.containsPoint(x, y))
            {
                hoverText = harvestButton.hoverText;
                return;
            }
            if(tillButton.containsPoint(x, y))
            {
                hoverText = tillButton.hoverText;
                return;
            }
            if(buyButton.containsPoint(x, y))
            {
                hoverText = buyButton.hoverText;
                return;
            }
            if(activeButton.containsPoint(x, y))
            {
                hoverText = activeButton.hoverText;
                return;
            }
            if(updateButton.containsPoint(x, y))
            {
                hoverText = updateButton.hoverText;
                return;
            }
        }

        public override void emergencyShutDown()
        {
            base.emergencyShutDown();
        }

    }
}