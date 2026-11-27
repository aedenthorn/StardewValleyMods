using LightSwitches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using static System.Net.Mime.MediaTypeNames;

namespace LightSwitches
{
    internal class ColorPickMenu : IClickableMenu
    {
        private Furniture f;
        private ColorPicker picker;
        private Color lastColor;
        private bool held;
        private TextBox onTime;
        private TextBox offTime;
        private ClickableComponent onTimeCC;
        private ClickableComponent offTimeCC;
        private string onText;
        private string offText;
        public ColorPickMenu(Furniture _f)
        {
            f = _f;
            width = SliderBar.defaultWidth + 160;
            height = 320;
            if (!f.modData.TryGetValue(ModEntry.colorKey, out var str))
            {
                str = "#FFFFFF";
                f.modData[ModEntry.colorKey] = str;
            }
            lastColor = Utility.StringToColor(str) ?? Color.White;
            RecreateElements();
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            RecreateElements();
            base.gameWindowSizeChanged(oldBounds, newBounds);
        }

        private void RecreateElements()
        {
            upperRightCloseButton = new ClickableTextureComponent(new Rectangle(Game1.uiViewport.Width / 2 + width / 2, Game1.uiViewport.Height / 2 - 64, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false);
            picker = new ColorPicker("Color", Game1.uiViewport.Width / 2 + width / 2 - SliderBar.defaultWidth - 72, Game1.uiViewport.Height / 2 - 114);
            picker.setColor(Utility.StringToColor(f.modData[ModEntry.colorKey]) ?? Color.White);
            onTime = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = Game1.uiViewport.Width / 2 - width / 2 + 8,
                Y = Game1.uiViewport.Height / 2,
                Width = width - 24,
                Text = f.modData.TryGetValue(ModEntry.onTimeKey, out var str) ? str : null
            };
            onTimeCC = new(new(onTime.X, onTime.Y, onTime.Width, onTime.Height), "onTimeCC");

            offTime = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = onTime.X,
                Y = onTime.Y + onTime.Height + 44,
                Width = onTime.Width,
                Text = f.modData.TryGetValue(ModEntry.offTimeKey, out var str2) ? str2 : null
            };
            offTimeCC = new(new(offTime.X, offTime.Y, offTime.Width, offTime.Height), "offTimeCC");
        }

        public override void receiveKeyPress(Keys key)
        {
            if(key == Keys.Tab || key == Keys.Up || key == Keys.Down)
            {
                if (onTime.Selected)
                {
                    offTime.SelectMe();
                }
                else if (offTime.Selected)
                {
                    onTime.SelectMe();
                }
            }

            base.receiveKeyPress(key);
        }

        public override void releaseLeftClick(int x, int y)
        {
            picker.releaseClick();
            held = false;
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            onTime.Update();
            offTime.Update();
            if (upperRightCloseButton.containsPoint(x, y))
            {
                base.receiveLeftClick(x, y, playSound);
            }
            else
            {
                ChangeColor(picker.click(x, y));
                held = true;
            }
        }
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            onTime.Update();
            offTime.Update();
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
            lastColor = color;
            f.modData[ModEntry.colorKey] = ModEntry.ColorToHexString(color);
            if(f.Location != null && f.modData.TryGetValue(ModEntry.onOffKey, out var on) && on == "on")
            {
                f.Location.modData[ModEntry.colorKey] = ModEntry.ColorToHexString(color);
            }
        }
        public override void draw(SpriteBatch b)
        {
            upperRightCloseButton.bounds = new Rectangle(Game1.uiViewport.Width / 2 + width / 2, Game1.uiViewport.Height / 2 - 144, 48, 48);
            base.draw(b);
            drawTextureBox(b, Game1.uiViewport.Width / 2 - width / 2, Game1.uiViewport.Height / 2 - width / 2, width, height - 24, Color.White);
            
            b.Draw(Game1.staminaRect, new Rectangle(Game1.uiViewport.Width / 2 - width / 2 + 24, Game1.uiViewport.Height / 2 - width / 2 + 36, 48, 48), null, lastColor);
            picker.draw(b);
            b.DrawString(Game1.smallFont, ModEntry.SHelper.Translation.Get("on-time"), new(onTime.X + 8, onTime.Y - 32), Game1.textColor);
            b.DrawString(Game1.smallFont, ModEntry.SHelper.Translation.Get("off-time"), new(offTime.X + 8, offTime.Y - 32), Game1.textColor);
            onTime.Draw(b);
            offTime.Draw(b);
            drawMouse(b);
        }
        public override void update(GameTime time)
        {
            if (onText != onTime.Text)
            {
                onText = onTime.Text;
                if(int.TryParse(onText, out var i) && i % 100 < 60 && i >= 600)
                {
                    f.modData[ModEntry.onTimeKey] = i.ToString();
                }
                else
                {
                    f.modData.Remove(ModEntry.onTimeKey);
                }
            }
            if (offText != offTime.Text)
            {
                offText = offTime.Text;
                if (int.TryParse(offText, out var i) && i % 100 < 60 && i >= 600)
                {
                    f.modData[ModEntry.offTimeKey] = i.ToString();
                }
                else
                {
                    f.modData.Remove(ModEntry.offTimeKey);
                }
            }
        }
    }
}