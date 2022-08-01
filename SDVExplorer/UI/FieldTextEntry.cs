using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace SDVExplorer.UI
{
	public class FieldTextEntry : FieldElement
	{
		public FieldTextEntry(string label, object obj, List<object> hier, int x = -1, int y = -1) : base(label, obj, hier, x, y, (int)Game1.smallFont.MeasureString("Windowed Borderless Mode   ").X + 48, 44)
		{
			this.textBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Color.Black);
			this.textBox.Width = this.bounds.Width;
		}

		public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			this.textBox.X = slotX + this.bounds.Left - 8;
			this.textBox.Y = slotY + this.bounds.Top;
			this.textBox.Draw(b, true);
			base.draw(b, slotX, slotY, context);
		}

		public override void receiveLeftClick(int x, int y)
		{
			this.textBox.SelectMe();
			this.textBox.Update();
		}

		public const int pixelsHigh = 11;

		public TextBox textBox;
	}
}
