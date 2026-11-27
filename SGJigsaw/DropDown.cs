using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace SGJigsaw
{
    public class DropDown
    {
        public const int pixelsHigh = 11;
        public int scrollDelay = 2;

        public List<string> dropDownOptions = new List<string>();
        public List<string> dropDownDisplayOptions = new List<string>();

        public int optionOffset;
        
        public int optionScrollTime;

        public int selectedOption;

        public int recentSlotY;

        public int startingSelected;

        public bool clicked;

        public Rectangle dropDownBounds;

        public static Rectangle dropDownBGSource = new Rectangle(433, 451, 3, 3);

        public static Rectangle dropDownButtonSource = new Rectangle(437, 450, 10, 11);
        public Rectangle bounds;
        public DropDown(List<string> items, int x, int y, int w, int h, int scrollDelay)
        {
            dropDownOptions = items;
            bounds = new Rectangle(x, y, w, h);
            RecalculateBounds();
            this.scrollDelay = scrollDelay;
        }

        public virtual void RecalculateBounds()
        {
            dropDownDisplayOptions = dropDownOptions.GetRange(optionOffset, Math.Min(dropDownOptions.Count - optionOffset, 8));

            foreach (string displayed_option in this.dropDownOptions)
            {
                float text_width = Game1.smallFont.MeasureString(displayed_option).X;
                if (text_width >= (float)(this.bounds.Width - 48))
                {
                    this.bounds.Width = (int)(text_width + 64f);
                }
            }
            this.dropDownBounds = new Rectangle(this.bounds.X, this.bounds.Y, this.bounds.Width - 48, this.bounds.Height * this.dropDownDisplayOptions.Count);
        }

        public void leftClickHeld(int x, int y)
        {
            this.clicked = true;
            this.dropDownBounds.Y = Math.Min(this.dropDownBounds.Y, Game1.uiViewport.Height - this.dropDownBounds.Height - this.recentSlotY);
            this.selectedOption = (int)Math.Max(Math.Min((float)(-16 + y - this.dropDownBounds.Y) / (float)this.bounds.Height, (float)(this.dropDownDisplayOptions.Count - 1)), 0f);
            if(y < dropDownBounds.Y && optionOffset > 0)
            {
                if(optionScrollTime++ > scrollDelay)
                {
                    optionOffset--;
                    optionScrollTime = 0;
                    RecalculateBounds();
                }
            }
            else if(y > dropDownBounds.Y + dropDownBounds.Height && optionOffset < dropDownOptions.Count - 8)
            {
                if (optionScrollTime++ > scrollDelay)
                {
                    optionOffset++;
                    optionScrollTime = 0;
                    RecalculateBounds();
                }
            }
            else
            {
                optionScrollTime = 0;
            }
        }
        public void receiveLeftClick(int x, int y)
        {
            this.startingSelected = this.selectedOption;
            if (!this.clicked)
            {
                Game1.playSound("shwip", null);
            }
            this.leftClickHeld(x, y);
        }


        public void leftClickReleased(int x, int y)
        {

            if (this.clicked)
            {
                Game1.playSound("drumkit6", null);
            }
            this.clicked = false;
        }


        public void receiveKeyPress(Keys key)
        {
            if (Game1.options.SnappyMenus)
            {
                if (!this.clicked)
                {
                    if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
                    {
                        this.selectedOption++;
                        if (this.selectedOption >= this.dropDownDisplayOptions.Count)
                        {
                            this.selectedOption = 0;
                        }
                        //Game1.options.changeDropDownOption(this.whichOption, this.dropDownOptions[this.selectedOption]);
                        return;
                    }
                    if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
                    {
                        this.selectedOption--;
                        if (this.selectedOption < 0)
                        {
                            this.selectedOption = this.dropDownDisplayOptions.Count - 1;
                        }
                        //Game1.options.changeDropDownOption(this.whichOption, this.dropDownOptions[this.selectedOption]);
                        return;
                    }
                }
                else if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
                {
                    Game1.playSound("shiny4", null);
                    this.selectedOption++;
                    if (this.selectedOption >= this.dropDownDisplayOptions.Count)
                    {
                        this.selectedOption = 0;
                        return;
                    }
                }
                else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
                {
                    Game1.playSound("shiny4", null);
                    this.selectedOption--;
                    if (this.selectedOption < 0)
                    {
                        this.selectedOption = this.dropDownDisplayOptions.Count - 1;
                    }
                }
            }
        }

        public void draw(SpriteBatch b, int slotX, int slotY)
        {
            this.recentSlotY = slotY;
            float alpha = 1f;
            if (this.clicked)
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, OptionsDropDown.dropDownBGSource, slotX + this.dropDownBounds.X, slotY + this.dropDownBounds.Y, this.dropDownBounds.Width, this.dropDownBounds.Height, Color.White * alpha, 4f, false, 0.97f);
                for (int i = 0; i < this.dropDownDisplayOptions.Count; i++)
                {
                    if (i == this.selectedOption)
                    {
                        b.Draw(Game1.staminaRect, new Rectangle(slotX + this.dropDownBounds.X, slotY + this.dropDownBounds.Y + i * this.bounds.Height, this.dropDownBounds.Width, this.bounds.Height), new Rectangle?(new Rectangle(0, 0, 1, 1)), Color.Wheat, 0f, Vector2.Zero, SpriteEffects.None, 0.975f);
                    }
                    b.DrawString(Game1.smallFont, this.dropDownDisplayOptions[i], new Vector2((float)(slotX + this.dropDownBounds.X + 4), (float)(slotY + this.dropDownBounds.Y + 8 + this.bounds.Height * i)), Game1.textColor * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.98f);
                }
                b.Draw(Game1.mouseCursors, new Vector2((float)(slotX + this.bounds.X + this.bounds.Width - 48), (float)(slotY + this.bounds.Y)), new Rectangle?(OptionsDropDown.dropDownButtonSource), Color.Wheat * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.981f);
                return;
            }
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, OptionsDropDown.dropDownBGSource, slotX + this.bounds.X, slotY + this.bounds.Y, this.bounds.Width - 48, this.bounds.Height, Color.White * alpha, 4f, false, -1f);
            b.DrawString(Game1.smallFont, (this.selectedOption < this.dropDownDisplayOptions.Count && this.selectedOption >= 0) ? this.dropDownDisplayOptions[this.selectedOption] : "", new Vector2((float)(slotX + this.bounds.X + 4), (float)(slotY + this.bounds.Y + 8)), Game1.textColor * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.88f);
            b.Draw(Game1.mouseCursors, new Vector2((float)(slotX + this.bounds.X + this.bounds.Width - 48), (float)(slotY + this.bounds.Y)), new Rectangle?(OptionsDropDown.dropDownButtonSource), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
        }
        public string GetCurrentItem()
        {
            return dropDownDisplayOptions[selectedOption];
        }
        public void SetCurrentItem(string item)
        {
            var idx = dropDownOptions.IndexOf(item);
            optionOffset = dropDownOptions.Count > 8 ? Math.Clamp(idx, 0, dropDownOptions.Count - 8) : 0;
            RecalculateBounds();
            selectedOption = idx - optionOffset;
        }
    }
}