using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace SDVExplorer.UI
{
    public class FieldElement
    {
        public List<object> hierarchy;
        public object instance;
		public const int defaultX = 8;

		public const int defaultY = 4;

		public const int defaultPixelWidth = 9;

		public Rectangle bounds;

		public string label;

		public bool greyedOut;

		public Vector2 labelOffset = Vector2.Zero;

		public FieldElement(string label, object obj, List<object> hier)
		{
			instance = obj;
			this.label = label;
			this.bounds = new Rectangle(32, 16, 36, 36);
		}
		public FieldElement(string label, object obj, List<object> hier, int x, int y, int width, int height)
		{
			hierarchy = hier;
			instance = obj;

			if (x == -1)
			{
				x = 32;
			}
			if (y == -1)
			{
				y = 16;
			}
			this.bounds = new Rectangle(x, y, width, height);
			this.label = label;
		}

		public FieldElement(string label, object obj, List<object> hier, Rectangle bounds)
		{
			instance = obj;

			this.label = label;
			this.bounds = bounds;
		}
		public virtual void receiveLeftClick(int x, int y)
		{
		}

		public virtual void leftClickHeld(int x, int y)
		{
		}

		public virtual void leftClickReleased(int x, int y)
		{
		}

		public virtual void receiveKeyPress(Keys key)
		{
		}

		public virtual void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			int label_start_x = slotX + this.bounds.X + this.bounds.Width + 8 + (int)this.labelOffset.X;
			int label_start_y = slotY + this.bounds.Y + (int)this.labelOffset.Y;
			string displayed_text = this.label;
			SpriteFont font = Game1.dialogueFont;
			if (context != null)
			{
				int max_width = context.width - 64;
				int menu_start_x = context.xPositionOnScreen;
				if (font.MeasureString(this.label).X + (float)label_start_x > (float)(max_width + menu_start_x))
				{
					int allowed_space = max_width + menu_start_x - label_start_x;
					font = Game1.smallFont;
					displayed_text = Game1.parseText(this.label, font, allowed_space);
					label_start_y -= (int)((font.MeasureString(displayed_text).Y - font.MeasureString("T").Y) / 2f);
				}
			}
			Utility.drawTextWithShadow(b, displayed_text, font, new Vector2((float)label_start_x, (float)label_start_y), this.greyedOut ? (Game1.textColor * 0.33f) : Game1.textColor, 1f, 0.1f, -1, -1, 1f, 3);
		}
	}
}