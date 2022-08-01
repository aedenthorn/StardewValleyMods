using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace SDVExplorer.UI
{
	public class FieldDropDown : FieldElement
	{
		public FieldDropDown(string label, object obj, List<object> hier, int x = -1, int y = -1) : base(label, obj, hier, x, y, (int)Game1.smallFont.MeasureString("Windowed Borderless Mode   ").X + 48, 44)
		{
			RecalculateBounds();
		}

		public virtual void RecalculateBounds()
		{
			foreach (string displayed_option in this.dropDownDisplayOptions)
			{
				float text_width = Game1.smallFont.MeasureString(displayed_option).X;
				if (text_width >= (float)(this.bounds.Width - 48))
				{
					this.bounds.Width = (int)(text_width + 64f);
				}
			}
			this.dropDownBounds = new Rectangle(this.bounds.X, this.bounds.Y, this.bounds.Width - 48, this.bounds.Height * this.dropDownOptions.Count);
		}

		public override void leftClickHeld(int x, int y)
		{
			if (!this.greyedOut)
			{
				base.leftClickHeld(x, y);
				this.clicked = true;
				this.dropDownBounds.Y = Math.Min(this.dropDownBounds.Y, Game1.uiViewport.Height - this.dropDownBounds.Height - this.recentSlotY);
				if (!Game1.options.SnappyMenus)
				{
					this.selectedOption = (int)Math.Max(Math.Min((float)(y - this.dropDownBounds.Y) / (float)this.bounds.Height, (float)(this.dropDownOptions.Count - 1)), 0f);
				}
			}
		}

		public override void receiveLeftClick(int x, int y)
		{
			if (!this.greyedOut)
			{
				base.receiveLeftClick(x, y);
				this.startingSelected = this.selectedOption;
				if (!this.clicked)
				{
					Game1.playSound("shwip");
				}
				this.leftClickHeld(x, y);
				FieldDropDown.selected = this;
			}
		}

		public override void leftClickReleased(int x, int y)
		{
			if (!this.greyedOut && this.dropDownOptions.Count > 0)
			{
				base.leftClickReleased(x, y);
				if (this.clicked)
				{
					Game1.playSound("drumkit6");
				}
				this.clicked = false;
				FieldDropDown.selected = this;
				if (this.dropDownBounds.Contains(x, y) || (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse))
				{
				}
				else
				{
					this.selectedOption = this.startingSelected;
				}
				OptionsDropDown.selected = null;
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (Game1.options.SnappyMenus && !this.greyedOut)
			{
				if (!this.clicked)
				{
					if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
					{
						this.selectedOption++;
						if (this.selectedOption >= this.dropDownOptions.Count)
						{
							this.selectedOption = 0;
						}
						FieldDropDown.selected = this;
						FieldDropDown.selected = null;
						return;
					}
					if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
					{
						this.selectedOption--;
						if (this.selectedOption < 0)
						{
							this.selectedOption = this.dropDownOptions.Count - 1;
						}
						FieldDropDown.selected = this;
						FieldDropDown.selected = null;
						return;
					}
				}
				else if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
				{
					Game1.playSound("shiny4");
					this.selectedOption++;
					if (this.selectedOption >= this.dropDownOptions.Count)
					{
						this.selectedOption = 0;
						return;
					}
				}
				else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
				{
					Game1.playSound("shiny4");
					this.selectedOption--;
					if (this.selectedOption < 0)
					{
						this.selectedOption = this.dropDownOptions.Count - 1;
					}
				}
			}
		}

		public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			this.recentSlotY = slotY;
			base.draw(b, slotX, slotY, context);
			float alpha = this.greyedOut ? 0.33f : 1f;
			if (this.clicked)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, FieldDropDown.dropDownBGSource, slotX + this.dropDownBounds.X, slotY + this.dropDownBounds.Y, this.dropDownBounds.Width, this.dropDownBounds.Height, Color.White * alpha, 4f, false, 0.97f);
				for (int i = 0; i < this.dropDownDisplayOptions.Count; i++)
				{
					if (i == this.selectedOption)
					{
						b.Draw(Game1.staminaRect, new Rectangle(slotX + this.dropDownBounds.X, slotY + this.dropDownBounds.Y + i * this.bounds.Height, this.dropDownBounds.Width, this.bounds.Height), new Rectangle?(new Rectangle(0, 0, 1, 1)), Color.Wheat, 0f, Vector2.Zero, SpriteEffects.None, 0.975f);
					}
					b.DrawString(Game1.smallFont, this.dropDownDisplayOptions[i], new Vector2((float)(slotX + this.dropDownBounds.X + 4), (float)(slotY + this.dropDownBounds.Y + 8 + this.bounds.Height * i)), Game1.textColor * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.98f);
				}
				b.Draw(Game1.mouseCursors, new Vector2((float)(slotX + this.bounds.X + this.bounds.Width - 48), (float)(slotY + this.bounds.Y)), new Rectangle?(FieldDropDown.dropDownButtonSource), Color.Wheat * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.981f);
				return;
			}
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, FieldDropDown.dropDownBGSource, slotX + this.bounds.X, slotY + this.bounds.Y, this.bounds.Width - 48, this.bounds.Height, Color.White * alpha, 4f, false, -1f);
			b.DrawString(Game1.smallFont, (this.selectedOption < this.dropDownDisplayOptions.Count && this.selectedOption >= 0) ? this.dropDownDisplayOptions[this.selectedOption] : "", new Vector2((float)(slotX + this.bounds.X + 4), (float)(slotY + this.bounds.Y + 8)), Game1.textColor * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.88f);
			b.Draw(Game1.mouseCursors, new Vector2((float)(slotX + this.bounds.X + this.bounds.Width - 48), (float)(slotY + this.bounds.Y)), new Rectangle?(FieldDropDown.dropDownButtonSource), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		}

		static FieldDropDown()
		{
		}

		public const int pixelsHigh = 11;

		[InstancedStatic]
		public static FieldDropDown selected;

		public List<string> dropDownOptions = new List<string>();

		public List<string> dropDownDisplayOptions = new List<string>();

		public int selectedOption;

		public int recentSlotY;

		public int startingSelected;

		private bool clicked;

		public Rectangle dropDownBounds;

		public static Rectangle dropDownBGSource = new Rectangle(433, 451, 3, 3);

		public static Rectangle dropDownButtonSource = new Rectangle(437, 450, 10, 11);
	}
}
