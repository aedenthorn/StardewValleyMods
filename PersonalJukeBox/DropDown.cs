using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PersonalJukeBox
{
    public class DropDown
    {
        public const int pixelsHigh = 11;
        public int scrollDelay = 2;

        public List<string> dropDownOptions = new List<string>();
        public List<string> dropDownDisplayOptions = new List<string>();
        public List<string> dropDownNames = new List<string>();
        public List<string> dropDownDisplayNames = new List<string>();

        public int optionOffset;
        
        public int optionScrollTime;

        public int selectedOption;

        public int recentSlotY;

        public int startingSelected;

        public bool clicked;

        public Rectangle dropDownBounds;

        public static Rectangle dropDownBGSource = new Rectangle(433, 451, 3, 3);

        public static Rectangle dropDownButtonSource = new Rectangle(437, 450, 10, 11);
        public static Rectangle starSource = new Rectangle(346, 400, 8, 8);
        public Rectangle bounds;
        public DropDown(List<string> items, List<string> names, int x, int y, int w, int h, int scrollDelay)
        {
            dropDownOptions = items;
            dropDownNames = names;
            bounds = new Rectangle(x, y, w, h);
            RecalculateBounds();
            this.scrollDelay = scrollDelay;
        }

        public virtual void RecalculateBounds()
        {
            dropDownDisplayOptions = dropDownOptions.GetRange(optionOffset, Math.Min(dropDownOptions.Count - optionOffset, 8));
            dropDownDisplayNames = dropDownNames.GetRange(optionOffset, Math.Min(dropDownOptions.Count - optionOffset, 8));

            foreach (string displayed_option in dropDownOptions)
            {
                float text_width = Game1.smallFont.MeasureString(displayed_option).X;
                if (text_width >= bounds.Width - 48)
                {
                    bounds.Width = (int)(text_width + 64f);
                }
            }
            dropDownBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width - 48, bounds.Height * dropDownDisplayOptions.Count);
        }

        public void leftClickHeld(int x, int y)
        {
            clicked = true;
            dropDownBounds.Y = Math.Min(dropDownBounds.Y, Game1.uiViewport.Height - dropDownBounds.Height - recentSlotY);
            selectedOption = (int)Math.Max(Math.Min((float)(y - dropDownBounds.Y) / (float)bounds.Height, (float)(dropDownDisplayOptions.Count - 1)), 0f);
            if(Game1.input.GetMouseState().RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton != ButtonState.Pressed)
            {
                ModEntry.ToggleFavorite(dropDownDisplayOptions[selectedOption]);

            }
            if (y < dropDownBounds.Y && optionOffset > 0)
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
            startingSelected = selectedOption;
            if (!clicked)
            {
                Game1.playSound("shwip", null);
            }
            leftClickHeld(x, y);
        }


        public void leftClickReleased(int x, int y)
        {

            if (clicked)
            {
                Game1.playSound("drumkit6", null);
            }
            clicked = false;
        }


        public void receiveKeyPress(Keys key)
        {
            if (Game1.options.SnappyMenus)
            {
                if (!clicked)
                {
                    if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
                    {
                        selectedOption++;
                        if (selectedOption >= dropDownDisplayOptions.Count)
                        {
                            selectedOption = 0;
                        }
                        //Game1.options.changeDropDownOption(this.whichOption, this.dropDownOptions[this.selectedOption]);
                        return;
                    }
                    if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
                    {
                        selectedOption--;
                        if (selectedOption < 0)
                        {
                            selectedOption = dropDownDisplayOptions.Count - 1;
                        }
                        //Game1.options.changeDropDownOption(this.whichOption, this.dropDownOptions[this.selectedOption]);
                        return;
                    }
                }
                else if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
                {
                    Game1.playSound("shiny4", null);
                    selectedOption++;
                    if (selectedOption >= dropDownDisplayOptions.Count)
                    {
                        selectedOption = 0;
                        return;
                    }
                }
                else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
                {
                    Game1.playSound("shiny4", null);
                    selectedOption--;
                    if (selectedOption < 0)
                    {
                        selectedOption = dropDownDisplayOptions.Count - 1;
                    }
                }
            }
        }

        public void draw(SpriteBatch b, int slotX, int slotY)
        {
            recentSlotY = slotY;
            float alpha = 1f;

            if (clicked)
            {
                IClickableMenu.drawTextureBox(b, Game1.staminaRect, OptionsDropDown.dropDownBGSource, slotX + dropDownBounds.X, slotY + dropDownBounds.Y, dropDownBounds.Width, dropDownBounds.Height, Color.Wheat, 4f, false, 0.97f);
                var list = ModEntry.PlayerList;

                for (int i = 0; i < dropDownDisplayOptions.Count; i++)
                {
                    if (i == selectedOption)
                    {
                        b.Draw(Game1.staminaRect, new Rectangle(slotX + dropDownBounds.X + 4, slotY + dropDownBounds.Y + i * bounds.Height + 4, dropDownBounds.Width - 8, bounds.Height - 8), null, Color.Goldenrod, 0f, Vector2.Zero, SpriteEffects.None, 0.975f);
                    }
                    b.DrawString(Game1.smallFont, dropDownDisplayNames[i], new Vector2((float)(slotX + dropDownBounds.X + 4), (float)(slotY + dropDownBounds.Y + 8 + bounds.Height * i)), Game1.textColor * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.98f);
                    if (list.Contains(dropDownDisplayOptions[i]))
                    {
                        b.Draw(Game1.mouseCursors, new Rectangle(slotX + dropDownBounds.Right - 28, slotY + dropDownBounds.Y + i * bounds.Height + 16, 16, 16), starSource, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.976f);
                    }
                }

                //b.Draw(Game1.mouseCursors, new Vector2((float)(slotX + bounds.X + bounds.Width - 48), (float)(slotY + bounds.Y)), new Rectangle?(OptionsDropDown.dropDownButtonSource), Color.Wheat * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.981f);
                if (optionOffset > 0)
                {
                    b.Draw(Game1.mouseCursors, new Rectangle(slotX + dropDownBounds.Right + 4, slotY + dropDownBounds.Y + 8, 24, 24), new Rectangle(421, 459, 12, 12), Color.White);
                }
                if (optionOffset < dropDownOptions.Count - dropDownDisplayOptions.Count)
                {
                    b.Draw(Game1.mouseCursors, new Rectangle(slotX + dropDownBounds.Right + 4, slotY + dropDownBounds.Y + 12 + (dropDownDisplayOptions.Count - 1) * bounds.Height, 24, 24), new Rectangle(421, 472, 12, 12), Color.White);
                }
                return;
            }
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, OptionsDropDown.dropDownBGSource, slotX + bounds.X, slotY + bounds.Y, bounds.Width - 48, bounds.Height, Color.White * 0.5f, 4f, false, -1f);
            b.DrawString(Game1.smallFont, (selectedOption < dropDownDisplayNames.Count && selectedOption >= 0) ? dropDownDisplayNames[selectedOption] : "", new Vector2((float)(slotX + bounds.X + 4), (float)(slotY + bounds.Y + 8)), Game1.textColor * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.88f);
            b.Draw(Game1.mouseCursors, new Vector2((float)(slotX + bounds.X + bounds.Width - 48), (float)(slotY + bounds.Y)), new Rectangle?(OptionsDropDown.dropDownButtonSource), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
        }
        public string GetCurrentItem()
        {
            if(selectedOption >= 0 && selectedOption < dropDownDisplayOptions.Count)
            {
                return dropDownDisplayOptions[selectedOption];
            }
            selectedOption = 0;
            return dropDownDisplayOptions.Any() ? dropDownDisplayOptions.First() : "";
        }
        public void SetCurrentItem(string item)
        {
            var idx = item is null ? 0 : dropDownOptions.IndexOf(item);
            optionOffset = dropDownOptions.Count > 8 ? Math.Clamp(idx, 0, dropDownOptions.Count - 8) : 0;
            RecalculateBounds();
            selectedOption = idx - optionOffset;
        }
    }
}