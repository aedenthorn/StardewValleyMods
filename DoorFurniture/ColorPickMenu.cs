using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;

namespace DoorFurniture
{
    internal class ColorPickMenu : IClickableMenu
    {
        private Furniture f;
        private ColorPicker picker;
        private bool held;
        private IClickableMenu activeClickableMenu;
        private int holdOffset;
        public ColorPickMenu(Furniture obj, IClickableMenu oldMenu = null)
        {
            f = obj;
            activeClickableMenu = oldMenu;
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
            picker = new ColorPicker("Door", Game1.uiViewport.Width / 2 + width / 2 - SliderBar.defaultWidth - 72, Game1.uiViewport.Height / 2 - 114);
            ModEntry.TryGetDoorData(f, out var data);
            var color = f.modData.TryGetValue(ModEntry.colorKey, out var str) ? Utility.StringToColor(str) ?? data.DefaultColor : data.DefaultColor;
            picker.setColor(color);
        }

        public override void releaseLeftClick(int x, int y)
        {
            holdOffset = 0;
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
                var bounds = AccessTools.FieldRefAccess<ColorPicker, Rectangle>(picker, "bounds");
                int left = bounds.Left;
                int right = bounds.Right - 1;
                if (x < left && left - x < 8)
                {
                    holdOffset = left - x;
                }
                else if (x > right && x - right < 9)
                {
                    holdOffset = right - x;
                }
                ChangeColor(picker.click(x + holdOffset, y));
                held = true;
            }
        }
        public override void leftClickHeld(int x, int y)
        {
            if (held)
            {
                ChangeColor(picker.clickHeld(x + holdOffset, y));
                held = true;
            }
            else
            {
                held = false;
            }
        }
        private void ChangeColor(Color color)
        {
            f.modData[ModEntry.colorKey] = ModEntry.ColorToHexString(color);
        }
        public override void draw(SpriteBatch b)
        {
            if(activeClickableMenu != null)
            {
                activeClickableMenu.draw(b);
            }
            upperRightCloseButton.bounds = new Rectangle(Game1.uiViewport.Width / 2 + width / 2, Game1.uiViewport.Height / 2 - 144, 48, 48);
            base.draw(b);
            drawTextureBox(b, Game1.uiViewport.Width / 2 - width / 2, Game1.uiViewport.Height / 2 - height / 2 - 72, width, height - 24, Color.White);
            //drawTextureBox(b, Game1.uiViewport.Width / 2 - width / 2 + 16, Game1.uiViewport.Height / 2 - width / 2 + 16, 64, 64, Color.White);
            f.drawInMenu(b, new Vector2(Game1.uiViewport.Width / 2 - width / 2 + 16, Game1.uiViewport.Height / 2 - height / 2 - 72 + 16), 2);
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