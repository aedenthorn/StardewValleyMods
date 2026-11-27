using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;

namespace CustomBackpack
{
    public class FullInventoryMenu : InventoryMenu
	{
		public int oldScrolled;

		public FullInventoryMenu(InventoryMenu menu): base(menu.xPositionOnScreen, menu.yPositionOnScreen, true, menu.actualInventory, menu.highlightMethod, Math.Min(Game1.player.Items.Count, (Game1.game1.uiScreen.Height - 80) / 68 * 12), Math.Min(Game1.player.Items.Count / 12, (Game1.game1.uiScreen.Height - 80) / 68))
		{
			
			height = Math.Min(Game1.game1.uiScreen.Height + 96, 86 + Game1.player.Items.Count / 12 * 68 + 80);
			var y = Math.Max(56, Game1.game1.uiScreen.Height / 2 - height / 2 + 96);
			var diff = yPositionOnScreen - y;
			yPositionOnScreen = y;
			for(int i = 0; i < inventory.Count; i++)
            {
				inventory[i].bounds.Y -= diff;
            }
		}
		public override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
		{
			Game1.drawDialogueBox(xPositionOnScreen - 62, yPositionOnScreen - 136, width + 120, height + 4, false, true);
			for (int i = 0; i < inventory.Count; i++)
			{
				if (_iconShakeTimer.ContainsKey(i) && Game1.currentGameTime.TotalGameTime.TotalSeconds >= _iconShakeTimer[i])
				{
					_iconShakeTimer.Remove(i);
				}
			}
			Color tint = (red == -1) ? Color.White : new Color((int)Utility.Lerp(red, Math.Min(255, red + 150), 0.65f), (int)Utility.Lerp(green, Math.Min(255, green + 150), 0.65f), (int)Utility.Lerp(blue, Math.Min(255, blue + 150), 0.65f));
			Texture2D texture = (red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture;
			int perRow = capacity / rows;
			int first = capacity < actualInventory.Count ? ModEntry.scrolled.Value * perRow : 0;
			for (int i = first; i < Math.Min(actualInventory.Count, first + capacity); i++)
			{
				Vector2 toDraw = new Vector2(xPositionOnScreen + i % perRow * 64 + horizontalGap * (i % perRow), ModEntry.GetBounds(this, i).Y);
				b.Draw(texture, toDraw, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1)), tint, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.5f);
				if ((playerInventory || showGrayedOutSlots) && i >= Game1.player.maxItems.Value)
				{
					b.Draw(texture, toDraw, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 57, -1, -1)), tint * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.5f);
				}
				if (!Game1.options.gamepadControls && i < 12 && playerInventory)
				{
					string strToDraw = (i == 9) ? "0" : ((i == 10) ? "-" : ((i == 11) ? "=" : ((i + 1).ToString() ?? "")));
					Vector2 strSize = Game1.tinyFont.MeasureString(strToDraw);
					b.DrawString(Game1.tinyFont, strToDraw, toDraw + new Vector2(32f - strSize.X / 2f, -strSize.Y), (i == Game1.player.CurrentToolIndex) ? Color.Red : Color.DimGray);
				}
			}
			for (int i = first; i < Math.Min(actualInventory.Count, first + capacity); i++)
			{
				Vector2 toDraw2 = new Vector2(xPositionOnScreen + i % perRow * 64 + horizontalGap * (i % perRow), ModEntry.GetBounds(this, i).Y);
				if (actualInventory.Count > i && actualInventory.ElementAt(i) != null)
				{
					bool highlight = highlightMethod(actualInventory[i]);
					if (_iconShakeTimer.ContainsKey(i))
					{
						toDraw2 += 1f * new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
					}
					actualInventory[i].drawInMenu(b, toDraw2, (inventory.Count > i) ? inventory[i].scale : 1f, (!highlightMethod(actualInventory[i])) ? 0.25f : 1f, 0.865f, StackDrawType.Draw, Color.White, highlight);
				}
			}
			ModEntry.DrawUIElements(b, this);
		}
	}
}