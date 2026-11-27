using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.SDKs;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace ContentPatcherEditor
{
    public class ContentPatcherMenu : IClickableMenu
    {
        public static int scrolled;
        public static int linesPerPage = 17;
        public static int windowWidth = 64 * 24;
        public static ContentPatcherMenu instance;
        public List<ContentPatcherPack> contentPatcherPacks = new();
        public List<ClickableComponent> allComponents = new();
        public ClickableTextureComponent addCC;
        public ClickableTextureComponent upCC;
        public ClickableTextureComponent downCC;
        public string hoverText;
        public string hoveredItem;
        private ClickableTextureComponent scrollBar;
        private Rectangle scrollBarRunner;
        private bool scrolling;

        public ContentPatcherMenu() : base(Game1.uiViewport.Width / 2 - (windowWidth + borderWidth * 2) / 2, -borderWidth, windowWidth + borderWidth * 2, Game1.uiViewport.Height, false)
        {
            scrolled = 0;
            string folder = ModEntry.Config.ModsFolder.Trim();
            if (string.IsNullOrEmpty(folder))
                folder = Path.Combine(Constants.GamePath, "Mods");
            
            foreach (var dir in Directory.GetDirectories(folder, "*", SearchOption.AllDirectories))
            {
                if (File.Exists(Path.Combine(dir, "manifest.json")) && File.Exists(Path.Combine(dir, "content.json")))
                {
                    try
                    {
                        MyManifest manifest = JsonConvert.DeserializeObject<MyManifest>(File.ReadAllText(Path.Combine(dir, "manifest.json")));
                        if (manifest.ContentPackFor?.UniqueID == "Pathoschild.ContentPatcher")
                        {
                            var pack = new ContentPatcherPack()
                            {
                                directory = dir,
                                manifest = manifest,
                                content = JsonConvert.DeserializeObject<ContentPatcherContent>(File.ReadAllText(Path.Combine(dir, "content.json"))),
                            };
                            if(pack.content is null)
                            {
                                ModEntry.SMonitor.Log($"Error loading mod at {dir}: \n\ncontent.json is null", LogLevel.Error);
                                continue;
                            }
                            ModEntry.RebuildLists(pack);
                            contentPatcherPacks.Add(pack);
                        }
                    }
                    catch(Exception ex)
                    {
                        ModEntry.SMonitor.Log($"Error loading mod at {dir}: \n\n{ex}", LogLevel.Error);
                    }
                }
            }

            RepopulateComponentList();

            exitFunction = emergencyShutDown;

            snapToDefaultClickableComponent();
        }

        private void RepopulateComponentList()
        {
            width = Math.Min(64 * 24, Game1.viewport.Width);
            height = Game1.viewport.Height + 72;
            xPositionOnScreen = (Game1.viewport.Width - width) / 2;
            yPositionOnScreen = -72;
            linesPerPage = (height - spaceToClearTopBorder * 2 - 108) / 64;

            allComponents.Clear();
            
            int count = 0;
            int lineHeight = 64;
            int dowWidth = 56;
            int weekWidth = dowWidth * 7 + 8;
            int domWidth = 40;
            int monthWidth = domWidth * 7 + 12;

            for (int i = scrolled; i < Math.Min(linesPerPage + scrolled, contentPatcherPacks.Count); i++)
            {
                int xStart = xPositionOnScreen + spaceToClearSideBorder + borderWidth;
                int yStart = yPositionOnScreen + borderWidth + spaceToClearTopBorder - 24 + count * lineHeight;
                int baseID = count * 1000;
                
                allComponents.Add(new ClickableComponent(new Rectangle(xStart, yStart + 96, width, lineHeight), contentPatcherPacks[i].manifest.Name, contentPatcherPacks[i].manifest.UniqueID)
                {
                    myID = baseID,
                    downNeighborID = baseID + 1000,
                    upNeighborID = baseID - 1000,
                    rightNeighborID = count < linesPerPage ? -1 : -2
                });
                count++;
            }
            addCC = new ClickableTextureComponent("Add", new Rectangle(xPositionOnScreen + width - 100, yPositionOnScreen - 96 + height, 56, 56), "", ModEntry.SHelper.Translation.Get("add"), Game1.mouseCursors, new Rectangle(1, 412, 14, 14), 4)
            {
                myID = count * 1000,
                upNeighborID = (count - 1) * 1000,
                rightNeighborID = -2,
            };
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
            if (count + scrolled < contentPatcherPacks.Count)
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
            if (contentPatcherPacks.Count > linesPerPage)
            {
                scrollBar = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 40 + 8, yPositionOnScreen + 132, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
                scrollBarRunner = new Rectangle(scrollBar.bounds.X, scrollBar.bounds.Y, scrollBar.bounds.Width, height - 200);
                float interval = (scrollBarRunner.Height - scrollBar.bounds.Height) / (float)(contentPatcherPacks.Count - linesPerPage);
                scrollBar.bounds.Y = Math.Min(scrollBarRunner.Y + (int)Math.Round(interval * scrolled), scrollBarRunner.Bottom - scrollBar.bounds.Height);

            }
            populateClickableComponentList();
        }


        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true);
            SpriteText.drawStringHorizontallyCenteredAt(b, ModEntry.SHelper.Translation.Get("content-packs"), Game1.viewport.Width / 2, yPositionOnScreen + spaceToClearTopBorder + borderWidth / 2);
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + borderWidth + spaceToClearTopBorder + 48, width - 64, 16), new Rectangle(40, 16, 1, 16), Color.White);
            int count = 0;
            foreach(var set in allComponents)
            {
                b.DrawString(Game1.dialogueFont, set.name, new Vector2(set.bounds.X, set.bounds.Y), Color.Black);
                count++;
            }
            addCC.draw(b);
            upCC?.draw(b);
            downCC?.draw(b);
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + spaceToClearSideBorder + 16, yPositionOnScreen + height - 112, width - 64, 8), new Rectangle(40, 16, 1, 16), Color.White);
            if (scrollBar is not null)
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f, true, -1f);
                scrollBar.draw(b);
            }
            if (hoverText != null && hoveredItem == null)
            {
                drawHoverText(b, hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, -1, -1, -1, -1, 1f, null, null);
            }
            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            for(int i = 0; i < allComponents.Count; i++)
            {
                var set = allComponents[i];
                if(set.containsPoint(x, y))
                {
                    Game1.playSound("bigSelect");
                    if(Game1.activeClickableMenu is TitleMenu)
                    {
                        TitleMenu.subMenu = new ContentPackMenu(contentPatcherPacks.First(p => p.manifest.UniqueID == set.label));
                    }
                    else
                    {
                        Game1.activeClickableMenu = new ContentPackMenu(contentPatcherPacks.First(p => p.manifest.UniqueID == set.label));
                    }
                    return;
                }
            }
            if(addCC.containsPoint(x, y))
            {
                Game1.playSound("bigSelect");
                ModEntry.CreateNewContentPatcherPack();
                RepopulateComponentList();
                return;
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

        private bool DoScroll(int direction)
        {
            if (direction < 0 && scrolled < contentPatcherPacks.Count - linesPerPage)
            {
                Game1.playSound("shiny4");
                scrolled++;
            }
            else if (direction > 0 && scrolled > 0)
            {
                Game1.playSound("shiny4");
                scrolled--;
            }
            else
            {
                return false;
            }
            return true;
        }

        public override void receiveKeyPress(Keys key)
        {
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

            for (int i = 0; i < allComponents.Count; i++)
            {
                var set = allComponents[i];
                if(set.containsPoint(x, y))
                    hoverText = set.label;

            }
            if (addCC.containsPoint(x, y))
            {
                hoverText = addCC.hoverText;
                return;
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
                float interval = (scrollBarRunner.Height - scrollBar.bounds.Height) / (float)(contentPatcherPacks.Count - linesPerPage);

                float percent = (y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
                int which = (int)Math.Round(scrollBarRunner.Height / interval * percent);

                int newScroll = Math.Max(0, Math.Min(contentPatcherPacks.Count - linesPerPage, which));
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
            scrolled = Math.Min(scrolled, contentPatcherPacks.Count - ((Game1.viewport.Height + 72 - spaceToClearTopBorder * 2 - 108) / 64));
            RepopulateComponentList();
        }

    }
}
