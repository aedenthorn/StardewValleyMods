using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewVN
{
    public class VNSelectMenu : IClickableMenu
    {
        public virtual List<VisualNovelData> MenuSlots
        {
            get
            {
                return menuSlots;
            }
            set
            {
                menuSlots = value;
            }
        }

        public VNSelectMenu(string filter = null)
            : base(Game1.uiViewport.Width / 2 - (1100 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1100 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, false)
        {

            upArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 16, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f, false)
            {
                myID = 800,
                downNeighborID = 801,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                region = 902
            };
            downArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f, false)
            {
                myID = 801,
                upNeighborID = 800,
                leftNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = -99998,
                region = 902
            };
            scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
            scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 64 - upArrow.bounds.Height - 28);

            for (int i = 0; i < 4; i++)
            {
                slotButtons.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + 16 + i * (height / 4), width - 32, height / 4 + 4), i.ToString() ?? "")
                {
                    myID = i,
                    region = 900,
                    downNeighborID = ((i < 3) ? (-99998) : (-7777)),
                    upNeighborID = ((i > 0) ? (-99998) : (-7777)),
                    rightNeighborID = -99998,
                    fullyImmutable = true
                });
            }
            MenuSlots = ModEntry.vnDict.Values.ToList();

            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                populateClickableComponentList();
                snapToDefaultClickableComponent();
            }
            UpdateButtons();
        }
        public virtual void UpdateButtons()
        {
            for (int i = 0; i < slotButtons.Count; i++)
            {
                if (currentItemIndex + i < MenuSlots.Count)
                {
                    slotButtons[i].visible = true;

                }
                else
                {
                    slotButtons[i].visible = false;

                }
            }
        }


        public override void snapToDefaultClickableComponent()
        {
            snapCursorToCurrentSnappedComponent();
        }

        protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            if (direction != 0)
            {
                if (direction == 2 && currentItemIndex < Math.Max(0, MenuSlots.Count - 4))
                {
                    downArrowPressed();
                    currentlySnappedComponent = base.getComponentWithID(3);
                    snapCursorToCurrentSnappedComponent();
                    return;
                }
            }
            else if (currentItemIndex > 0)
            {
                upArrowPressed();
                currentlySnappedComponent = base.getComponentWithID(0);
                snapCursorToCurrentSnappedComponent();
            }
        }

        /// <inheritdoc />
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            xPositionOnScreen = (newBounds.Width - width) / 2;
            yPositionOnScreen = (newBounds.Height - (height + 32)) / 2;
            upArrow.bounds.X = xPositionOnScreen + width + 16;
            upArrow.bounds.Y = yPositionOnScreen + 16;
            downArrow.bounds.X = xPositionOnScreen + width + 16;
            downArrow.bounds.Y = yPositionOnScreen + height - 64;
            scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
            scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 64 - upArrow.bounds.Height - 28);
            for (int i = 0; i < slotButtons.Count; i++)
            {
                slotButtons[i].bounds.X = xPositionOnScreen + 16;
                slotButtons[i].bounds.Y = yPositionOnScreen + 16 + i * (height / 4);
            }
            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                int id = ((currentlySnappedComponent != null) ? currentlySnappedComponent.myID : 81114);
                populateClickableComponentList();
                currentlySnappedComponent = base.getComponentWithID(id);
                snapCursorToCurrentSnappedComponent();
            }
        }

        /// <inheritdoc />
        public override void performHoverAction(int x, int y)
        {
            hoverText = "";
            base.performHoverAction(x, y);

            upArrow.tryHover(x, y, 0.1f);
            downArrow.tryHover(x, y, 0.1f);
            scrollBar.tryHover(x, y, 0.1f);

            if (scrolling)
            {
                return;
            }
            for (int i = 0; i < slotButtons.Count; i++)
            {
                if (currentItemIndex + i < MenuSlots.Count && slotButtons[i].containsPoint(x, y))
                {
                    if (slotButtons[i].scale == 1f)
                    {
                        Game1.playSound("Cowboy_gunshot", null);
                    }
                    slotButtons[i].scale = Math.Min(slotButtons[i].scale + 0.03f, 1.1f);
                }
                else
                {
                    slotButtons[i].scale = Math.Max(1f, slotButtons[i].scale - 0.03f);
                }
            }
        }

        /// <inheritdoc />
        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (scrolling)
            {
                int y2 = scrollBar.bounds.Y;
                scrollBar.bounds.Y = Math.Min(yPositionOnScreen + height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, yPositionOnScreen + upArrow.bounds.Height + 20));
                float percentage = (float)(y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
                currentItemIndex = Math.Min(MenuSlots.Count - 4, Math.Max(0, (int)((float)MenuSlots.Count * percentage)));
                setScrollBarToCurrentIndex();
                if (y2 != scrollBar.bounds.Y)
                {
                    Game1.playSound("shiny4", null);
                }
            }
        }

        /// <inheritdoc />
        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            scrolling = false;
        }

        protected void setScrollBarToCurrentIndex()
        {
            if (MenuSlots.Count > 0)
            {
                scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, MenuSlots.Count - 4 + 1) * currentItemIndex + upArrow.bounds.Bottom + 4;
                if (currentItemIndex == MenuSlots.Count - 4)
                {
                    scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
                }
            }
            UpdateButtons();
        }

        /// <inheritdoc />
        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && currentItemIndex > 0)
            {
                upArrowPressed();
                return;
            }
            if (direction < 0 && currentItemIndex < Math.Max(0, MenuSlots.Count - 4))
            {
                downArrowPressed();
            }
        }

        private void downArrowPressed()
        {
            downArrow.scale = downArrow.baseScale;
            currentItemIndex++;
            Game1.playSound("shwip", null);
            setScrollBarToCurrentIndex();
        }

        private void upArrowPressed()
        {
            upArrow.scale = upArrow.baseScale;
            currentItemIndex--;
            Game1.playSound("shwip", null);
            setScrollBarToCurrentIndex();
        }


        /// <inheritdoc />
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (timerToLoad > 0 || loading)
            {
                return;
            }
            base.receiveLeftClick(x, y, playSound);
            if (downArrow.containsPoint(x, y) && currentItemIndex < Math.Max(0, MenuSlots.Count - 4))
            {
                downArrowPressed();
            }
            else if (upArrow.containsPoint(x, y) && currentItemIndex > 0)
            {
                upArrowPressed();
            }
            else if (scrollBar.containsPoint(x, y))
            {
                scrolling = true;
            }
            else if (!downArrow.containsPoint(x, y) && x > xPositionOnScreen + width && x < xPositionOnScreen + width + 128 && y > yPositionOnScreen && y < yPositionOnScreen + height)
            {
                scrolling = true;
                leftClickHeld(x, y);
                releaseLeftClick(x, y);
            }

            for (int j = 0; j < slotButtons.Count; j++)
            {
                if (slotButtons[j].containsPoint(x, y) && j < MenuSlots.Count)
                {
                    Game1.playSound("select", null);

                    TitleMenu.subMenu = new VisualNovelMenu(MenuSlots[currentItemIndex + j]);
                    return;
                }
            }
            currentItemIndex = Math.Max(0, Math.Min(MenuSlots.Count - 4, currentItemIndex));
        }

        protected virtual void saveFileScanComplete()
        {
            Game1.game1.ResetGameStateOnTitleScreen();
        }



        protected virtual void drawSlotBackground(SpriteBatch b, int i)
        {
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), slotButtons[i].bounds.X, slotButtons[i].bounds.Y, slotButtons[i].bounds.Width, slotButtons[i].bounds.Height, Color.White, 4f, false, -1f);
        }

        protected virtual void drawBefore(SpriteBatch b)
        {
        }


        /// <inheritdoc />
        public override void draw(SpriteBatch b)
        {
            drawBefore(b);

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), xPositionOnScreen, yPositionOnScreen, width, height + 32, Color.White, 4f, true, -1f);

            upArrow.draw(b);
            downArrow.draw(b);
            if (MenuSlots.Count > 4)
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f, false, -1f);
                scrollBar.draw(b);
            }
            base.draw(b); 
            for (int i = 0; i < slotButtons.Count; i++)
            {
                if (currentItemIndex + i < MenuSlots.Count)
                {
                    MenuSlots[currentItemIndex + i].drawAction.Invoke(b, slotButtons[i].bounds);
                }
            }
            if (hoverText.Length > 0)
            {
                IClickableMenu.drawHoverText(b, hoverText, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
            }
            if (Game1.activeClickableMenu == this && (!Game1.options.SnappyMenus || currentlySnappedComponent != null))
            {
                base.drawMouse(b, false, loading ? 1 : (-1));
            }
            drawn = true;
        }


        public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
        {
            return (a.region == 901 && b.region != 901 && direction == 2 && b.myID != 81114) || ((a.region != 901 || direction != 3 || b.region == 900) && (direction != 1 || a.region != 900 || b.region == 901) && (a.region == 903 || b.region != 903) && ((direction != 0 && direction != 2) || a.myID != 81114 || b.region != 902) && base.IsAutomaticSnapValid(direction, a, b));
        }

        protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
        {
            return false;
        }

        protected const int CenterOffset = 0;

        public const int region_upArrow = 800;

        public const int region_downArrow = 801;

        public const int region_okDelete = 802;

        public const int region_cancelDelete = 803;

        public const int region_slots = 900;

        public const int region_deleteButtons = 901;

        public const int region_navigationButtons = 902;

        public const int region_deleteConfirmations = 903;

        public const int itemsPerPage = 4;

        public List<ClickableComponent> slotButtons = new List<ClickableComponent>();


        public int currentItemIndex;

        public int timerToLoad;

        public int selected = -1;

        public int selectedForDelete = -1;

        public ClickableTextureComponent upArrow;

        public ClickableTextureComponent downArrow;

        public ClickableTextureComponent scrollBar;



        public ClickableComponent backButton;

        public bool scrolling;


        protected List<GamesGameData> menuSlots = new List<GamesGameData>();

        private Rectangle scrollBarRunner;

        protected string hoverText = "";

        public bool loading;

        public bool drawn;


        private int _updatesSinceLastDeleteConfirmScreen;




    }
}
