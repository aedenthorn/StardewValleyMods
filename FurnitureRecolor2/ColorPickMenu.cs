using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;
using System.Collections.Generic;

namespace FurnitureRecolor
{
    internal class ColorPickMenu : IClickableMenu
    {
        private Furniture f;
        private List<ColorPicker> pickers = new();
        private bool held;
        private IClickableMenu activeClickableMenu;
        private int holdOffset;
        private List<Color> colorList = new();
        private List<Color> originalColorList = new();
        private List<ClickableComponent> previews = new();
        public ColorPickMenu(Furniture obj, List<Color> list, IClickableMenu oldMenu = null)
        {
            previews = new();
            pickers = new();
            f = obj;
            activeClickableMenu = oldMenu;

            if(obj.modData.TryGetValue(ModEntry.colorsKey, out var str))
            {
                List<Color> newList = new();

                ModEntry.MakeColorList(newList, str);
                if(newList.Count == list.Count)
                {
                    colorList = newList;
                }
                else
                {
                    colorList = list;
                }
            }
            else
            {
                colorList = list;
            }
            originalColorList = list;
            RecreatePickers();
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            RecreatePickers();
            //base.gameWindowSizeChanged(oldBounds, newBounds);
        }

        private void RecreatePickers()
        {
            height = colorList.Count * 128;
            width = 600;
            xPositionOnScreen = Game1.uiViewport.Width / 2 - width / 2;
            yPositionOnScreen = Game1.uiViewport.Height / 2 - height / 2;
            var top = Game1.uiViewport.Height / 2 - height / 2;
            upperRightCloseButton = new ClickableTextureComponent(new Rectangle(Game1.uiViewport.Width / 2 + width / 2, top, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false);
            pickers.Clear();
            for(int i = 0; i < colorList.Count; i++)
            {
                var picker = new ColorPicker("Furniture"+i, Game1.uiViewport.Width / 2 + width / 2 - SliderBar.defaultWidth - 72, top + 14 + i * 128);
                previews.Add(new ClickableComponent(new Rectangle(Game1.uiViewport.Width / 2, top + 14 + i * 128, 64, 64), $"preview{i}", null));
                var color = colorList[i];
                picker.setColor(color);
                pickers.Add(picker);
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            holdOffset = 0;
            foreach(var picker in pickers)
            {
                picker.releaseClick();
            }
            held = false;
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if(upperRightCloseButton.containsPoint(x, y))
            {
                base.receiveLeftClick(x, y, playSound);
                return;
            }
            for(int i = 0; i < previews.Count; i++)
            {
                if (previews[i].containsPoint(x, y))
                {
                    pickers[i].setColor(originalColorList[i]);
                    ChangeColor(originalColorList[i], i);
                    return;
                }
            }
            var bounds = AccessTools.FieldRefAccess<ColorPicker, Rectangle>(pickers[0], "bounds");
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

            for (int i = 0; i < pickers.Count; i++)
            {
                if (pickers[i].containsPoint(x + holdOffset, y))
                {
                    ChangeColor(pickers[i].click(x + holdOffset, y), i);
                }
            }
            held = true;

        }
        public override void leftClickHeld(int x, int y)
        {
            if (held)
            {
                for(int i = 0; i < pickers.Count; i++)
                {
                    if (pickers[i].containsPoint(x + holdOffset, y))
                    {
                        ChangeColor(pickers[i].clickHeld(x + holdOffset, y), i);
                    }
                }
                held = true;
            }
            else
            {
                held = false;
            }
        }
        private void ChangeColor(Color color, int i)
        {
            colorList[i] = color;
            List<string> colors = new();
            foreach (Color c in colorList) 
            {
                colors.Add($"{c.R},{c.G},{c.B}");
            }
            f.modData[ModEntry.colorsKey] = string.Join(";", colors);
        }
        public override void draw(SpriteBatch b)
        {
            if(activeClickableMenu != null)
            {
                activeClickableMenu.draw(b);
            }
            upperRightCloseButton.bounds = new Rectangle(Game1.uiViewport.Width / 2 + width / 2, yPositionOnScreen, 48, 48);
            base.draw(b);

            drawTextureBox(b, Game1.uiViewport.Width / 2 - width / 2, Game1.uiViewport.Height / 2 - height / 2 - 8, width, height - 24, Color.White);
            //drawTextureBox(b, Game1.uiViewport.Width / 2 - width / 2 + 16, Game1.uiViewport.Height / 2 - width / 2 + 16, 64, 64, Color.White);
            ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(f.QualifiedItemId);

            var sourceRect = itemData.GetSourceRect(0, null);
            f.drawInMenu(b, new Vector2(xPositionOnScreen + width / 4, yPositionOnScreen + height / 2 - 48), 4, 1, 1, StackDrawType.Hide, Color.White, true);
            for(int i = 0; i < pickers.Count; i++)
            {
                pickers[i].draw(b);
                b.Draw(Game1.staminaRect, previews[i].bounds, pickers[i].getSelectedColor());
            }
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