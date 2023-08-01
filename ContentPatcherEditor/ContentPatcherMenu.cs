using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
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

namespace ContentPatcherEditor
{
    public class ContentPatcherMenu : IClickableMenu
    {
        public static int scrolled;
        public static int setsPerPage = 18;
        public static int windowWidth = 64 * 24;
        public static ContentPatcherMenu instance;
        public List<ContentPatcherPack> contentPatcherPacks = new();
        public List<ClickableComponent> allComponents = new();
        public ClickableTextureComponent addCC;
        public ClickableTextureComponent upCC;
        public ClickableTextureComponent downCC;
        public string hoverText;
        public string hoveredItem;

        public ContentPatcherMenu() : base(Game1.uiViewport.Width / 2 - (windowWidth + borderWidth * 2) / 2, -borderWidth, windowWidth + borderWidth * 2, Game1.uiViewport.Height, false)
        {
            setsPerPage = 18;
            width = 64 * 24;
            RepopulateComponentList();

            exitFunction = emergencyShutDown;

            snapToDefaultClickableComponent();
        }

        private void RepopulateComponentList()
        {
            contentPatcherPacks.Clear();
            allComponents.Clear();
            
            int count = 0;
            int setHeight = 64;
            int lineHeight = 96;
            int clockWidth = 144;
            int seasonWidth = 108;
            int dowWidth = 56;
            int weekWidth = dowWidth * 7 + 8;
            int domWidth = 40;
            int monthWidth = domWidth * 7 + 12;
            Texture2D textBox = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
            foreach(var dir in Directory.GetDirectories(Path.Combine(Constants.GamePath, "Mods"), "*", SearchOption.AllDirectories))
            {
                if(File.Exists(Path.Combine(dir, "manifest.json")) && File.Exists(Path.Combine(dir, "content.json")))
                {
                    MyManifest manifest = JsonConvert.DeserializeObject<MyManifest>(File.ReadAllText(Path.Combine(dir, "manifest.json")));
                    if(manifest.ContentPackFor?.UniqueID == "Pathoschild.ContentPatcher")
                    {
                        contentPatcherPacks.Add(new ContentPatcherPack()
                        {
                            directory = dir,
                            manifest = manifest,
                            content = JsonConvert.DeserializeObject<ContentPatcherContent>(File.ReadAllText(Path.Combine(dir, "content.json")))
                        });
                    }
                }
            }
            for (int i = scrolled; i < Math.Min(setsPerPage + scrolled, contentPatcherPacks.Count); i++)
            {
                int xStart = xPositionOnScreen + spaceToClearSideBorder + borderWidth;
                int yStart = yPositionOnScreen + borderWidth + spaceToClearTopBorder - 24 + count * setHeight;
                int baseID = count * 1000;
                allComponents.Add(new ClickableComponent(new Rectangle(xStart, yStart, width, setHeight), contentPatcherPacks[i].manifest.Name, contentPatcherPacks[i].manifest.UniqueID )
                {
                    myID = baseID,
                    downNeighborID = baseID + 1000,
                    upNeighborID = baseID - 1000,
                    rightNeighborID = count < setsPerPage ? -1 : -2
                });
                count++;
            }
            addCC = new ClickableTextureComponent("Add", new Rectangle(xPositionOnScreen + width - 128, yPositionOnScreen - 128 + height, 56, 56), "", ModEntry.SHelper.Translation.Get("add"), Game1.mouseCursors, new Rectangle(1, 412, 14, 14), 4)
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
            populateClickableComponentList();
        }

        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true);
            int count = 0;
            foreach(var set in allComponents)
            {
                b.DrawString(Game1.dialogueFont, set.name, new Vector2(set.bounds.X, set.bounds.Y), Color.Black);
                count++;
            }
            addCC.draw(b);
            upCC?.draw(b);
            downCC?.draw(b);
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
                    Game1.activeClickableMenu = new ContentPackMenu(contentPatcherPacks.First(p => p.manifest.UniqueID == set.label));
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
            if (direction < 0 && scrolled < contentPatcherPacks.Count - setsPerPage)
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

    }
}
