using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace AdvancedCharacterCustomization
{
    public class AdvancedCharacterCustomizationMenu : IClickableMenu
    {
        public CharacterCustomization menu;
        private Farmer farmer;
        private StyleType whichStyle;
        public List<Texture2D> allEntries = new();
        public List<ClickableTextureComponent> entries = new();
        public ClickableTextureComponent  leftButton;
        public ClickableTextureComponent  rightButton;
        public int scrolled = 0;
        private int count;
        private int cols;
        private int oneHeight;
        private int oneWidth;
        private int rows;
        private int fit;
        private object last;
        private int lastIndex;
        public AdvancedCharacterCustomizationMenu(CharacterCustomization m, Farmer f, StyleType w): base(m.xPositionOnScreen, m.yPositionOnScreen, m.width, m.height)
        {
            menu = m;
            farmer = f;
            whichStyle = w;
            count = whichStyle switch
            {
                StyleType.Skin => 24,
                StyleType.Hair => Farmer.GetAllHairstyleIndices().Count,
                StyleType.Shirt => menu.GetValidShirtIds().Count,
                StyleType.Pants => menu.GetValidPantsIds().Count,
                StyleType.Acc => 31,
                _ => 31
            };
            last = GetLast();
            lastIndex = GetLastIndex();
            CreatePreviews();
            cols = 4;
            oneHeight = 192;
            oneWidth = 96;
            rows = height / oneHeight;
            fit = rows * cols;
            scrolled = MathHelper.Clamp(GetLastIndex() / cols, 0, (int)Math.Ceiling((count - fit) / (float)cols));
            RebuildComponents();
        }

        private void CreatePreviews()
        {
            allEntries.Clear();
            for (int i = 0; i < count; i++)
            {
                Texture2D tex = new(Game1.graphics.GraphicsDevice, 64, 128);
                RenderTarget2D renderTarget = new RenderTarget2D(Game1.graphics.GraphicsDevice, 64, 128);
                renderTarget.SetData(new Color[64 * 128]);
                Rectangle destinationRectangle = new Rectangle(0, 0, 64, 128);

                Game1.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
                Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

                var renderBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);

                renderBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
                DoChange(i);
                DrawFarmer(renderBatch, new(0, 0, 64, 128));
                renderBatch.End();

                Game1.graphics.GraphicsDevice.SetRenderTarget(null);
                Color[] data = new Color[renderTarget.Width * renderTarget.Height];
                renderTarget.GetData(data);
                tex.SetData(data);
                allEntries.Add(tex);
            }
            DoChange(last);
        }

        private void RebuildComponents()
        {
            xPositionOnScreen = menu.xPositionOnScreen;
            yPositionOnScreen = menu.yPositionOnScreen;
            width = menu.width;
            height = menu.height;
            int start = cols * scrolled;
            entries.Clear();
            int c = 0;
            var end = Math.Min(start + fit, count);
            for (int i = start; i < end; i++)
            {
                var x = c % cols * width / (cols + 1);
                var y = c / cols * height / (rows + 1);
                entries.Add(new(i.ToString(), new (xPositionOnScreen + 96 + x, yPositionOnScreen + 120 + y, 64, 128), null, i.ToString(), allEntries[i], new(0, 0, 64, 128), 1f));
                c++;
            }

            leftButton = new ClickableTextureComponent("Direction", new Rectangle(xPositionOnScreen + width / 2 - 96, yPositionOnScreen + height, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
            {
                myID = 520,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                leftNeighborImmutable = true,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
            rightButton = new ClickableTextureComponent("Direction", new Rectangle(xPositionOnScreen + width / 2 + 32, yPositionOnScreen + height, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
            {
                myID = 521,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
        }

        private void DrawFarmer(SpriteBatch b, Rectangle bounds)
        {
            farmer.FarmerRenderer.draw(b, farmer.FarmerSprite.CurrentAnimationFrame, farmer.FarmerSprite.CurrentFrame, farmer.FarmerSprite.SourceRect, new Vector2(0, 0), Vector2.Zero, 0.8f, Color.White, 0f, 1f, farmer);
        }

        private void DoChange(object last)
        {

            switch (whichStyle)
            {
                case StyleType.Skin:
                    farmer.changeSkinColor((int)last, true);
                    break;
                case StyleType.Hair:
                    farmer.changeHairStyle((int)last);
                    break;
                case StyleType.Shirt:
                    farmer.changeShirt((string)last);
                    break;
                case StyleType.Pants:
                    farmer.changePantStyle((string)last);
                    break;
                case StyleType.Acc:
                    farmer.changeAccessory((int)last);
                    break;
            }
        }

        private void DoChange(int i)
        {
            switch (whichStyle)
            {
                case StyleType.Skin:
                    farmer.changeSkinColor(i, true);
                    break;
                case StyleType.Hair:
                    farmer.changeHairStyle(Farmer.GetAllHairstyleIndices()[i]);
                    break;
                case StyleType.Shirt:
                    farmer.changeShirt(menu.GetValidShirtIds()[i]);
                    break;
                case StyleType.Pants:
                    farmer.changePantStyle(menu.GetValidPantsIds()[i]);
                    break;
                case StyleType.Acc:
                    farmer.changeAccessory(i - 1);
                    break;
            }
        }

        private object GetLast()
        {
            return whichStyle switch
            {
                StyleType.Skin => farmer.skin.Value,
                StyleType.Hair => farmer.hair.Value,
                StyleType.Shirt => farmer.shirt.Value,
                StyleType.Pants => farmer.pants.Value,
                StyleType.Acc => farmer.accessory.Value,
                _ => farmer.accessory.Value
            };
        }

        private int GetLastIndex()
        {
            return whichStyle switch
            {
                StyleType.Skin => (int)last,
                StyleType.Hair => Farmer.GetAllHairstyleIndices().IndexOf((int)last),
                StyleType.Shirt => menu.GetValidShirtIds().IndexOf((string)last),
                StyleType.Pants => menu.GetValidPantsIds().IndexOf((string)last),
                StyleType.Acc => (int)last,
                _ => 30
            };
        }
        private bool CantScrollDown()
        {
            return (scrolled * cols + fit) / cols >= (int)Math.Ceiling(count / (float)cols);
        }


        public override void draw(SpriteBatch b)
        {

            //menu.draw(b);
            //b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * 0.5f);
            SpriteText.drawStringWithScrollCenteredAt(b, ModEntry.SHelper.Translation.Get(whichStyle.ToString()), xPositionOnScreen + width / 2, yPositionOnScreen);
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true, -1, -1, -1);
            foreach (var cc in entries)
            {
                var bounds = GetRect(cc);
                if (bounds.Contains(Game1.getMouseX(true), Game1.getMouseY(true)))
                {
                    b.Draw(Game1.staminaRect, bounds, Color.White * 0.75f);
                }
                else if (cc.name == lastIndex.ToString())
                {
                    b.Draw(Game1.staminaRect, bounds, Color.White * 0.5f);
                }
                
                cc.draw(b);
            }
            if (scrolled > 0)
            {
                b.Draw(Game1.mouseCursors, new Rectangle(xPositionOnScreen + width - 68, yPositionOnScreen + 104, 24, 24), new Rectangle(421, 459, 12, 12), Color.White);
            }
            if (!CantScrollDown())
            {
                b.Draw(Game1.mouseCursors, new Rectangle(xPositionOnScreen + width - 68, yPositionOnScreen + height - 64, 24, 24), new Rectangle(421, 472, 12, 12), Color.White);
            }
            leftButton.draw(b);
            rightButton.draw(b);
            base.drawMouse(b);
        }

        private Rectangle GetRect(ClickableTextureComponent cc)
        {
            return new Rectangle(cc.bounds.X - 39, cc.bounds.Y - 10, cc.bounds.Width + 78, cc.bounds.Height + 30);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            foreach (var cc in entries)
            {
                if(GetRect(cc).Contains(x, y))
                {
                    var i = int.Parse(cc.name);
                    switch (whichStyle)
                    {
                        case StyleType.Skin:
                            farmer.changeSkinColor(i, true);
                            break;
                        case StyleType.Hair:
                            farmer.changeHairStyle(Farmer.GetAllHairstyleIndices()[i]);
                            break;
                        case StyleType.Shirt:
                            farmer.changeShirt(menu.GetValidShirtIds()[i]);
                            break;
                        case StyleType.Pants:
                            farmer.changePantStyle(menu.GetValidPantsIds()[i]);
                            break;
                        case StyleType.Acc:
                            farmer.changeAccessory(i - 1);
                            break;
                    }
                    if (Game1.activeClickableMenu == this)
                    {
                        Game1.activeClickableMenu = menu;
                    }
                    else if (TitleMenu.subMenu == this)
                    {
                        TitleMenu.subMenu = menu;
                    }
                    Game1.playSound("bigSelect");
                    return;
                }
            }
            int dir;
            if (leftButton.containsPoint(x, y))
            {
                dir = 1;
            }
            else if (rightButton.containsPoint(x, y))
            {
                dir = -1;
            }
            else return;
            farmer.faceDirection((farmer.FacingDirection + dir + 4) % 4);
            farmer.FarmerSprite.StopAnimation();
            farmer.completelyStopAnimatingOrDoingAction();
            Game1.playSound("pickUpItem", null);
            CreatePreviews();
            RebuildComponents();
        }
        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.None)
            {
                return;
            }
            if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose())
            {
                Game1.playSound("bigDeSelect");
                if (Game1.activeClickableMenu == this)
                {
                    Game1.activeClickableMenu = menu;
                }
                else if (TitleMenu.subMenu == this)
                {
                    TitleMenu.subMenu = menu;
                }
                return;
            }
        }
        public override void receiveScrollWheelAction(int direction)
        {
            var newScrolled = scrolled - Math.Sign(direction);
            if (newScrolled < 0)
            {
                return;
            }
            if (newScrolled > scrolled && CantScrollDown())
            {
                return;
            }
            scrolled = newScrolled;
            Game1.playSound("shiny4");
            RebuildComponents();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            menu.gameWindowSizeChanged(oldBounds, newBounds);
            base.gameWindowSizeChanged(oldBounds, newBounds);
            RebuildComponents();
        }
    }
}