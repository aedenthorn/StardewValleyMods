using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;

namespace FlowerColors
{
    internal class ColorPickMenu : IClickableMenu
    {
        private ColoredObject obj;
        private Crop crop;
        private ColorPicker picker;
        private bool held;
        private IClickableMenu activeClickableMenu;
        public ColorPickMenu(object o, IClickableMenu oldMenu = null)
        {
            activeClickableMenu = oldMenu;
            if (o is ColoredObject c)
            {
                obj = c;
            }
            else if (o is Crop cr)
            {
                obj = new ColoredObject(cr.indexOfHarvest.Value, 1, cr.tintColor.Value);
                crop = cr;
            }
            width = SliderBar.defaultWidth + 160;
            height = 144;
            RecreatePicker();
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            RecreatePicker();
            base.gameWindowSizeChanged(oldBounds, newBounds);
        }

        private void RecreatePicker()
        {
            upperRightCloseButton = new ClickableTextureComponent(new Rectangle(Game1.uiViewport.Width / 2 + width / 2, Game1.uiViewport.Height / 2 - 64, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false);
            picker = new ColorPicker("Flower", Game1.uiViewport.Width / 2 + width / 2 - SliderBar.defaultWidth - 72, Game1.uiViewport.Height / 2 - 114);

            picker.setColor(obj.color.Value);
        }

        public override void releaseLeftClick(int x, int y)
        {
            picker.releaseClick();
            held = false;
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if(upperRightCloseButton.containsPoint(x, y))
            {
                base.receiveLeftClick(x, y, playSound);
            }
            else
            {
                ChangeColor(picker.click(x, y));
                held = true;
            }
        }
        public override void leftClickHeld(int x, int y)
        {
            if (held)
            {
                ChangeColor(picker.clickHeld(x, y));
                held = true;
            }
            else
            {
                held = false;
            }
        }
        private void ChangeColor(Color color)
        {
            if (crop != null)
            {
                crop.tintColor.Value = color;
            }
            obj.color.Value = color;
        }
        public override void draw(SpriteBatch b)
        {
            if(activeClickableMenu != null)
            {
                activeClickableMenu.draw(b);
            }
            upperRightCloseButton.bounds = new Rectangle(Game1.uiViewport.Width / 2 + width / 2, Game1.uiViewport.Height / 2 - 144, 48, 48);
            base.draw(b);
            drawTextureBox(b, Game1.uiViewport.Width / 2 - width / 2, Game1.uiViewport.Height / 2 - width / 2, width, height - 24, Color.White);
            //drawTextureBox(b, Game1.uiViewport.Width / 2 - width / 2 + 16, Game1.uiViewport.Height / 2 - width / 2 + 16, 64, 64, Color.White);
            obj.drawInMenu(b, new Vector2(Game1.uiViewport.Width / 2 - width / 2 + 16, Game1.uiViewport.Height / 2 - width / 2 + 28), 1);
            picker.draw(b);
            drawMouse(b);
        }
        public override bool readyToClose()
        {
            if(activeClickableMenu != null)
            {
                Game1.activeClickableMenu = activeClickableMenu;
                Game1.playSound("bigDeSelect");
                return false;
            }
            return base.readyToClose();
        }
    }
}