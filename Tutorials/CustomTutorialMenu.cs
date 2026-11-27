using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Tiles;

namespace Tutorials
{
    internal class CustomTutorialMenu : IClickableMenu
    {
        private TutorialData tutorial;
        private string tutorialKey;
        private List<ClickableTextureComponent> pageDots = new();
        private ClickableTextureComponent texture;
        private ClickableTextureComponent leftArrow;
        private ClickableTextureComponent rightArrow;
        private List<ClickableComponent> sidebarEntries = new();
        private Dictionary<string, List<string>> categorizedTutorials = new();
        private List<string> orderedCategories = new();
        private string currentCategory;
        private int scrolled;
        private int maxEntries;
        private int currentPage;
        private int ticks;
        private int size;
        private int sidebarWidth;
        private bool openingTutorial;
        private bool keyPress;

        public CustomTutorialMenu(string key = null, string cat = null, List<string> cats = null) : base(Game1.uiViewport.Width / 2 - (600 + borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + borderWidth * 2) / 2 - 192, 600 + borderWidth * 2, 600 + borderWidth * 2 + 192, true)
        {
            string thisCat = key == null ? null : ModEntry.TutorialDict[key].Category;
            foreach (var kvp in ModEntry.TutorialDict)
            {
                if (cats?.Contains(kvp.Value.Category) == false && (key == null || kvp.Value.Category != thisCat))
                    continue;
                if (!categorizedTutorials.ContainsKey(kvp.Value.Category))
                {
                    categorizedTutorials[kvp.Value.Category] = new();
                }
                categorizedTutorials[kvp.Value.Category].Add(kvp.Key);
            }
            foreach(var k in categorizedTutorials.Keys)
            {
                categorizedTutorials[k].Sort();
            }
            orderedCategories.AddRange(categorizedTutorials.Keys);
            orderedCategories.Sort();
            if(key != null)
            {
                openingTutorial = true;
                tutorialKey = key;
                tutorial = ModEntry.TutorialDict[key];
                currentCategory = tutorial.Category;
            }
            else if(cat != null)
            {
                openingTutorial = true;
                tutorialKey = categorizedTutorials[cat].First();
                tutorial = ModEntry.TutorialDict[tutorialKey];
                currentCategory = cat;
            }
            Recalibrate();
        }
        private void Recalibrate()
        {
            sidebarWidth = ModEntry.Config.SidebarWidth;
            size = ModEntry.Config.Height;
            height = size + 64;
            width = size + sidebarWidth;
            
            xPositionOnScreen = Game1.uiViewport.Width / 2 - width / 2;
            yPositionOnScreen = Game1.uiViewport.Height / 2 - height / 2 - 32;

            int entryHeight = 48;
            int entryWidth = sidebarWidth - 18;
            maxEntries = size / entryHeight;
            int count = 0;
            int xStart = xPositionOnScreen + 36;
            int yStart = yPositionOnScreen + 96;

            sidebarEntries.Clear();
            foreach(var c in orderedCategories)
            {
                count++;
                if (count - scrolled >= maxEntries && !openingTutorial)
                    break;
                if (count > scrolled || openingTutorial)
                {
                    var num = sidebarEntries.Count;
                    sidebarEntries.Add(new(new(xStart, yStart + sidebarEntries.Count * entryHeight, entryWidth, entryHeight), "cat|" + c)
                    {
                        myID = 1000 + num,
                        downNeighborID = 1000 + num + 1,
                        upNeighborID = 1000 + num - 1,
                        rightNeighborID = 2000
                    });
                }
                if(currentCategory == c)
                {
                    foreach(var t in categorizedTutorials[c])
                    {
                        count++;
                        if (count - scrolled >= maxEntries && !openingTutorial)
                            goto breakout;
                        if (count > scrolled || openingTutorial)
                        {
                            if(openingTutorial && t == tutorialKey)
                            {
                                openingTutorial = false;
                                while(count >= maxEntries + scrolled)
                                {
                                    scrolled++;
                                } 
                                Recalibrate();
                                return;
                            }
                            if (count > scrolled || openingTutorial)
                            {
                                var num = sidebarEntries.Count;
                                sidebarEntries.Add(new(new(xStart, yStart + sidebarEntries.Count * entryHeight, entryWidth, entryHeight), t)
                                {
                                    myID = 1000 + num,
                                    downNeighborID = 1000 + num + 1,
                                    upNeighborID = 1000 + num - 1,
                                    rightNeighborID = 2000
                                });
                            }
                        }
                    }
                }
            }
        breakout:
            upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 36, yPositionOnScreen + 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false)
            {
                myID = 9175502
            };

            int space = 48;
            pageDots.Clear();
            if (tutorial is null)
                return;
            for (int i = 0; i < tutorial.Frames.Count; i++)
            {
                int pos = xPositionOnScreen + sidebarWidth +  size / 2 - tutorial.Frames.Count * space / 2 + i * space;
                pageDots.Add(new(new(pos, yPositionOnScreen + height - 80, 28, 36), Game1.mouseCursors, new(129 + (i == currentPage ? 8 : 0), 338, 7, 9), 4f)
                {
                    myID = 2000 + i,
                    leftNeighborID = i == 0 ? 1000 : 2000 + i - 1,
                    rightNeighborID = 2000 + i + 1,
                });
            }
            //leftArrow = new(new(xPositionOnScreen - 44, yPositionOnScreen + height / 2, 44, 40), Game1.mouseCursors, new(8, 268, 44, 40), 1f);
            //rightArrow = new(new(xPositionOnScreen + width, yPositionOnScreen + height / 2, 44, 40), Game1.mouseCursors, new(12, 204, 44, 40), 1f);

            var d = tutorial.Frames[currentPage];
            if (d.Texture is string tstr)
            {
                var tex = ModEntry.SHelper.GameContent.Load<Texture2D>(tstr);
                Rectangle source = d.StartRect ?? new(0, 0, tex.Width, tex.Height);
                float x = xPositionOnScreen + sidebarWidth + 50;
                float y = yPositionOnScreen + 100;
                float w = size - 86;
                float h = w * 9 / 16;
                float xr = source.Width / w; // 200 / 600
                float yr = source.Height / h; // 80 / 300
                float scale = 1f;
                if (xr > yr)
                {
                    scale /= xr;
                    y += h / 2;
                    y -= (source.Height * scale) / 2;
                }
                else
                {
                    scale /= yr;
                    x += w / 2;
                    x -= (source.Width * scale) / 2;
                }
                texture = new(new Rectangle((int)x, (int)y, (int)w, (int)h), tex, tutorial.Frames[currentPage].StartRect ?? new(), scale);
            }
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            Recalibrate();
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if(upperRightCloseButton.containsPoint(x, y))
            {
                exitThisMenu();
            }
            foreach(var cc in sidebarEntries)
            {
                if (cc.containsPoint(x, y)) 
                {
                    if (cc.name.StartsWith("cat|"))
                    {
                        var cat = cc.name.Substring(4);
                        currentCategory = currentCategory == cat ? null : cat;
                        Game1.playSound("shwip");
                    }
                    else
                    {
                        tutorialKey = cc.name;
                        tutorial = ModEntry.TutorialDict[cc.name];
                        ticks = 0;
                        currentPage = 0;
                        Game1.playSound("bigSelect");
                    }
                    Recalibrate();
                    return;
                }
            }
            for(int i = 0; i < pageDots.Count; i++)
            {
                if (pageDots[i].containsPoint(x, y))
                {
                    Game1.playSound("shwip");
                    currentPage = i;
                    ticks = 0;
                    Recalibrate();
                }
            }
        }
        public override void receiveKeyPress(Keys key)
        {
            if(Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
            {
                currentPage--;
                if (currentPage < 0)
                    currentPage = tutorial.Frames.Count - 1;
                Game1.playSound("shwip");
                ticks = 0;
                Recalibrate();
                return;
            }
            if(Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
            {
                currentPage++;
                if (currentPage >= tutorial.Frames.Count)
                    currentPage = 0;
                Game1.playSound("shwip");
                ticks = 0;
                Recalibrate();
                return;
            }
            if(Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
            {
                keyPress = true;
                receiveScrollWheelAction(1);
                return;
            }
            if(Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
            {
                keyPress = true;
                receiveScrollWheelAction(-1);
                return;
            }
            base.receiveKeyPress(key);
        }
        public override void performHoverAction(int x, int y)
        {
        }
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false, true, -1, -1, -1); 
            
            b.Draw(Game1.mouseCursors, new Rectangle(xPositionOnScreen + sidebarWidth + 14 , yPositionOnScreen + 96, 36, height - 128), new Rectangle?(new Rectangle(278, 324, 9, 1)), Color.White);
            var yPosition = yPositionOnScreen + size * 9 / 16 + 32;
            //b.Draw(Game1.menuTexture, new Vector2((float)this.xPositionOnScreen + sidebarWidth, (float)yPosition), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 4, -1, -1)), Color.White);
            b.Draw(Game1.menuTexture, new Rectangle(this.xPositionOnScreen + sidebarWidth + 46, yPosition, size - 78, 64), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 6, -1, -1)), Color.White);
            //b.Draw(Game1.menuTexture, new Vector2((float)(this.xPositionOnScreen + sidebarWidth + this.size - 64), (float)yPosition), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 7, -1, -1)), Color.White);
           
            upperRightCloseButton.draw(b);
            
            if(tutorial != null)
            {
                if(tutorial.Frames.Count > 1)
                {
                    foreach (var cc in pageDots)
                    {
                        cc.draw(b);
                    }
                }
                if (tutorial.Frames[currentPage].Frames > 1 && tutorial.Frames[currentPage].StartRect is Rectangle r)
                {
                    ticks++;
                    var which = ticks / tutorial.Frames[currentPage].FrameRate;
                    if (which >= tutorial.Frames[currentPage].Frames)
                    {
                        which = 0;
                        ticks = 0;
                    }
                    int startX = r.Width * which;
                    int startY = 0;
                    while(startX >= texture.texture.Width)
                    {
                        startX -= texture.texture.Width;
                        startY += r.Height;
                    }
                    texture.sourceRect = new(r.Location + new Point(startX, startY), r.Size);
                }
                texture?.draw(b);

                if (tutorial.Frames[currentPage].Text is string str)
                {
                    SpriteText.drawString(b, str, xPositionOnScreen + sidebarWidth + 80, yPosition + 64, 999999, size - spaceToClearSideBorder * 8, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
                }
            }
            foreach (var cc in sidebarEntries)
            {
                if (cc.name.StartsWith("cat|"))
                {
                    if (cc.bounds.Contains(Game1.getMousePosition()))
                    {
                        b.Draw(Game1.staminaRect, cc.bounds, Color.WhiteSmoke * 0.5f);
                    }
                    if (ModEntry.CatDict.TryGetValue(cc.name.Substring(4), out var catName))
                    {
                        Utility.drawTextWithShadow(b, catName, Game1.dialogueFont, new Vector2(cc.bounds.X + 8, cc.bounds.Y + 4), Game1.textColor);
                    }
                }
                else
                {
                    var font = Game1.smallFont;
                    string title = ModEntry.TutorialDict[cc.name].Title;
                    if (cc.bounds.Contains(Game1.getMousePosition()))
                    {
                        b.Draw(Game1.staminaRect, new Rectangle(cc.bounds.Location, new Point((int)Math.Max(font.MeasureString(title).X + 48, cc.bounds.Width), cc.bounds.Height)), Color.WhiteSmoke * 0.5f);
                    }
                    else
                    {
                        if (cc.name == tutorialKey)
                        {
                            b.Draw(Game1.staminaRect, cc.bounds, Color.LightGoldenrodYellow * 0.5f);
                        }
                        if(font.MeasureString(title).X > cc.bounds.Width - 48) 
                        {
                            while (font.MeasureString(title).X > cc.bounds.Width - 48)
                            {
                                title = title.Substring(0, title.Length - 1);
                            }
                            title += "...";
                        }
                    }
                    Utility.drawTextWithShadow(b, title, font, new Vector2(cc.bounds.X + 24, cc.bounds.Y + 8), Game1.textColor);
                }
            }
            //leftArrow.draw(b);
            //rightArrow.draw(b);
            drawMouse(b, false, -1);
        }
        public override void receiveScrollWheelAction(int direction)
        {
            if (keyPress || new Rectangle(xPositionOnScreen, yPositionOnScreen, sidebarWidth, height).Contains(Game1.getMousePosition(true)))
            {
                keyPress = false;
                if (direction > 0 && scrolled > 0)
                {
                    Game1.playSound("shiny4");
                    scrolled--;
                    Recalibrate();
                }
                else if (direction < 0 && scrolled <= categorizedTutorials.Count + (currentCategory == null ? 0 : categorizedTutorials[currentCategory].Count) - maxEntries)
                {
                    Game1.playSound("shiny4");
                    scrolled++;
                    Recalibrate();
                }
            }
        }
    }
}