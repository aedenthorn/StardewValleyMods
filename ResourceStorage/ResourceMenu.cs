using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Object = StardewValley.Object;

namespace ResourceStorage
{
    public class ResourceMenu : IClickableMenu
    {
        public static int scrolled;
        public static int linesPerPage = 17;
        public static int windowWidth = 64 * 24;
        public static ResourceMenu instance;
        public Dictionary<string, long> resources = new();
        public List<Object> resourceList = new();
        //public List<ClickableComponent> allComponents = new();
        public Dictionary<int, ClickableTextureComponent> autoCCs = new();
        public Dictionary<int, ClickableTextureComponent> takeCCs = new();
        public ClickableTextureComponent upCC;
        public ClickableTextureComponent downCC;
        public string hoverText;
        public string hoveredItem;
        public ClickableTextureComponent scrollBar;
        public Rectangle scrollBarRunner;
        public bool scrolling;

        public ResourceMenu() : base(Game1.uiViewport.Width / 2 - (windowWidth + borderWidth * 2) / 2, -borderWidth, windowWidth + borderWidth * 2, Game1.uiViewport.Height, false)
        {
            scrolled = 0;

            RepopulateComponentList();

            exitFunction = emergencyShutDown;

            snapToDefaultClickableComponent();
        }

        public void RepopulateComponentList()
        {
            resourceList.Clear();
            resources = ModEntry.GetFarmerResources(Game1.player);
            foreach (var resource in resources)
            {
                int index = ModEntry.GetIndex(resource.Key);
                if (index < 0)
                    continue;
                Object obj = new Object(index, (int)resource.Value);
                //obj.stack.Value = obj.ParentSheetIndex * 193;
                resourceList.Add(obj);
            }

            int lineHeight = 64;
            linesPerPage = (Game1.uiViewport.Height + 72 - spaceToClearTopBorder * 2 - 108) / lineHeight;

            width = Math.Min(64 * 12, Game1.uiViewport.Width);
            height = Math.Min(Game1.uiViewport.Height + 72, Math.Min(linesPerPage, resourceList.Count) * lineHeight + (borderWidth + spaceToClearTopBorder) * 2 - 32);
            xPositionOnScreen = (Game1.uiViewport.Width - width) / 2;
            yPositionOnScreen = Math.Max(-72, (Game1.uiViewport.Height - height) / 2 - 32);

            //allComponents.Clear();
            autoCCs.Clear();
            takeCCs.Clear();
            
            int count = 0;

            for (int i = scrolled; i < Math.Min(linesPerPage + scrolled, resourceList.Count); i++)
            {
                int xStart = xPositionOnScreen + spaceToClearSideBorder + borderWidth;
                int yStart = yPositionOnScreen + borderWidth + spaceToClearTopBorder - 24 + count * lineHeight;
                int baseID = count * 1000;
                /*
                allComponents.Add(new ClickableComponent(new Rectangle(xStart + 32, yStart + 96, width, lineHeight), resourceList[i].DisplayName, resourceList[i].getDescription())
                {
                    myID = baseID,
                    downNeighborID = baseID + 1000,
                    upNeighborID = baseID - 1000,
                    rightNeighborID = baseID + 2,
                });
                */
                autoCCs[i] = new ClickableTextureComponent("Auto", new Rectangle(xStart, yStart + 104, 36, 36), "", ModEntry.SHelper.Translation.Get("auto"), Game1.mouseCursors, new Rectangle(ModEntry.CanAutoStore(resourceList[i]) ? 236 : 227, 425, 9, 9), 4)
                {
                    myID = baseID,
                    downNeighborID = baseID + 1000,
                    upNeighborID = baseID - 1000,
                    rightNeighborID = baseID + 1,
                };
                takeCCs[i] = new ClickableTextureComponent("Take", new Rectangle(xPositionOnScreen + width - (spaceToClearSideBorder + borderWidth) - 36, yStart + 100, 48, 44), "", ModEntry.SHelper.Translation.Get("take"), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4)
                {
                    myID = baseID + 1,
                    downNeighborID = baseID + 1000,
                    upNeighborID = baseID - 1000,
                    leftNeighborID = baseID,
                };
                count++;
            }

            if (scrolled > 0)
            {
                upCC = new ClickableTextureComponent("Up", new Rectangle(xPositionOnScreen + width + 40, yPositionOnScreen + 84, 40, 44), "", ModEntry.SHelper.Translation.Get("up"), Game1.mouseCursors, new Rectangle(76, 72, 40, 44), 1)
                {
                    myID = -1,
                    leftNeighborID = 0,
                    downNeighborID = -2,
                };

            }
            else
                upCC = null;
            if (count + scrolled < resourceList.Count)
            {
                downCC = new ClickableTextureComponent("Down", new Rectangle(xPositionOnScreen + width + 40, yPositionOnScreen + height - 64, 40, 44), "", ModEntry.SHelper.Translation.Get("down"), Game1.mouseCursors, new Rectangle(12, 76, 40, 44), 1)
                {
                    myID = -2,
                    leftNeighborID = 0,
                    upNeighborID = -1,
                };
            }
            else 
                downCC = null;
            if (resourceList.Count > linesPerPage)
            {
                scrollBar = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 40 + 8, yPositionOnScreen + 132, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
                scrollBarRunner = new Rectangle(scrollBar.bounds.X, scrollBar.bounds.Y, scrollBar.bounds.Width, height - 200);
                float interval = (scrollBarRunner.Height - scrollBar.bounds.Height) / (float)(resourceList.Count - linesPerPage);
                scrollBar.bounds.Y = Math.Min(scrollBarRunner.Y + (int)Math.Round(interval * scrolled), scrollBarRunner.Bottom - scrollBar.bounds.Height);

            }
            populateClickableComponentList();
        }


        public override void draw(SpriteBatch b)
        {
            ModEntry.gameMenu.draw(b);
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true);
            SpriteText.drawStringHorizontallyCenteredAt(b, ModEntry.SHelper.Translation.Get("resources"), Game1.uiViewport.Width / 2, yPositionOnScreen + spaceToClearTopBorder + borderWidth / 2);
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + borderWidth + spaceToClearTopBorder + 48, width - 64, 16), new Rectangle(40, 16, 1, 16), Color.White);
            int count = 0;
            for (int i = scrolled; i < Math.Min(linesPerPage + scrolled, resourceList.Count); i++)
            {
                autoCCs[i].draw(b);
                takeCCs[i].draw(b);
                resourceList[i].drawInMenu(b, new Vector2(autoCCs[i].bounds.Right + 8, autoCCs[i].bounds.Y - 16), 1, 1, 1, StackDrawType.Hide);
                b.DrawString(Game1.dialogueFont, resourceList[i].DisplayName, new Vector2(autoCCs[i].bounds.Right + 80, autoCCs[i].bounds.Y - 4), Color.Black);
                float width = Game1.dialogueFont.MeasureString(resourceList[i].Stack + "").X;
                b.DrawString(Game1.dialogueFont, resourceList[i].Stack + "", new Vector2(takeCCs[i].bounds.X - width - 8, autoCCs[i].bounds.Y - 4), Color.Black);
                count++;
            }
            upCC?.draw(b);
            downCC?.draw(b);
            if (scrollBar is not null)
            {
                drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f, true, -1f);
                scrollBar.draw(b);
            }
            if (!string.IsNullOrEmpty(hoverText) && hoveredItem == null)
            {
                drawHoverText(b, hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, -1, -1, -1, -1, 1f, null, null);
            }
            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            for (int i = scrolled; i < Math.Min(linesPerPage + scrolled, resourceList.Count); i++)
            {
                if (autoCCs[i].containsPoint(x, y))
                {
                    List<string> list = ModEntry.Config.AutoStore.Split(',').ToList();

                    if (ModEntry.CanAutoStore(resourceList[i]))
                    {
                        ModEntry.SMonitor.Log($"Removing {resourceList[i].DisplayName} from autostore list");
                        Game1.playSound("drumkit6");

                        for (int j = 0; j < list.Count; j++)
                        {
                            if (list[j].Trim().ToLower() == resourceList[i].Name.ToLower() || (int.TryParse(list[j], out int index) && index == resourceList[i].ParentSheetIndex))
                            {
                                list.RemoveAt(j);
                                break;
                            }
                        }
                    }
                    else
                    {
                        ModEntry.SMonitor.Log($"Adding {resourceList[i].DisplayName} to autostore list");
                        Game1.playSound("drumkit6");

                        list.Add(resourceList[i].Name);
                    }
                    ModEntry.Config.AutoStore = string.Join(",", list);
                    ModEntry.SMonitor.Log($"New autostore list: {ModEntry.Config.AutoStore}");
                    ModEntry.SHelper.WriteConfig(ModEntry.Config);
                    RepopulateComponentList();
                    return;
                }
                if (takeCCs[i].containsPoint(x, y))
                {
                    int stack = 1;
                    if (ModEntry.SHelper.Input.IsDown(ModEntry.Config.ModKey1))
                    {
                        stack = Math.Min(Math.Min(resourceList[i].Stack, resourceList[i].maximumStackSize()), ModEntry.Config.ModKey1Amount);
                    }
                    else if (ModEntry.SHelper.Input.IsDown(ModEntry.Config.ModKey2))
                    {
                        stack = Math.Min(Math.Min(resourceList[i].Stack, resourceList[i].maximumStackSize()), ModEntry.Config.ModKey2Amount);
                    }
                    else if (ModEntry.SHelper.Input.IsDown(ModEntry.Config.ModKey3))
                    {
                        stack = Math.Min(Math.Min(resourceList[i].Stack, resourceList[i].maximumStackSize()), ModEntry.Config.ModKey3Amount);
                    }

                    Object obj = new Object(resourceList[i].ParentSheetIndex, stack);
                    if (Game1.objectInformation.TryGetValue(obj.ParentSheetIndex, out string data) && Game1.player.addItemToInventoryBool(obj))
                    {
                        Game1.playSound("Ship");
                        ModEntry.ModifyResourceLevel(Game1.player, ModEntry.GetIdString(data), -stack);
                        RepopulateComponentList();
                    }
                    else
                    {
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                    }
                    return;
                }
            }
            if (upCC?.containsPoint(x, y) == true)
            {
                if (DoScroll(1))
                {
                    RepopulateComponentList();
                }
                return;
            }
            if (downCC?.containsPoint(x, y) == true)
            {
                if (DoScroll(-1))
                {
                    RepopulateComponentList();
                }
                return;
            }
            if (scrollBar?.containsPoint(x, y) == true)
            {
                scrolling = true;
                return;
            }
        }


        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            base.receiveRightClick(x, y, playSound);
        }
        public override void receiveScrollWheelAction(int direction)
        {
            DoScroll(direction);
            RepopulateComponentList();
        }

        public bool DoScroll(int direction)
        {
            if (direction < 0 && scrolled < resourceList.Count - linesPerPage)
            {
                Game1.playSound("shiny4");
                scrolled++;
                RepopulateComponentList();
            }
            else if (direction > 0 && scrolled > 0)
            {
                Game1.playSound("shiny4");
                scrolled--;
                RepopulateComponentList();
            }
            else
            {
                return false;
            }
            return true;
        }

        public override void receiveKeyPress(Keys key)
        {
            if((Game1.options.doesInputListContain(Game1.options.menuButton, key) || key == (Keys)ModEntry.Config.ResourcesKey) && readyToClose())
            {
                exitThisMenu();
                Game1.activeClickableMenu = ModEntry.gameMenu;
                return;
            }
            base.receiveKeyPress(key);
        }


        public override void snapToDefaultClickableComponent()
        {
            if(Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                currentlySnappedComponent = getComponentWithID(0);
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
                if (next is null && (currentlySnappedComponent.myID % 1000 == 0))
                {
                    if (direction == 0)
                    {
                        DoScroll(1);
                        next = getComponentWithID(currentlySnappedComponent.upNeighborID);
                    }
                    else if (direction == 2)
                    {
                        DoScroll(-1);
                        next = getComponentWithID(currentlySnappedComponent.downNeighborID);
                    }
                }
                if (next is not null)
                {
                    Game1.playSound("shiny4");
                    currentlySnappedComponent = next;
                    snapCursorToCurrentSnappedComponent();
                }
            }
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

            foreach (var cc in autoCCs.Values)
            {
                if (cc.containsPoint(x, y))
                {
                    hoverText = cc.hoverText;
                    return;
                }
            }
            foreach (var cc in takeCCs.Values) 
            {
                if (cc.containsPoint(x, y))
                {
                    hoverText = cc.hoverText;
                    return;
                }
            }
            if (upCC?.containsPoint(x, y) == true)
            {
                hoverText = upCC.hoverText;
                return;
            }
            if (downCC?.containsPoint(x, y) == true)
            {
                hoverText = downCC.hoverText;
                return;
            }
        }
        public override void emergencyShutDown()
        {
            base.emergencyShutDown();
        }
        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (scrolling)
            {
                float interval = (scrollBarRunner.Height - scrollBar.bounds.Height) / (float)(resourceList.Count - linesPerPage);

                float percent = (y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
                int which = (int)Math.Round(scrollBarRunner.Height / interval * percent);

                int newScroll = Math.Max(0, Math.Min(resourceList.Count - linesPerPage, which));
                if (newScroll != scrolled)
                {
                    Game1.playSound("shiny4");
                    scrolled = newScroll;
                    RepopulateComponentList();
                }
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            scrolling = false;
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            scrolled = Math.Min(scrolled, resourceList.Count - ((Game1.uiViewport.Height + 72 - spaceToClearTopBorder * 2 - 108) / 64));
            RepopulateComponentList();
        }

    }
}
