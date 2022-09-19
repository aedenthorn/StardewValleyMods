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

        public FullInventoryMenu(InventoryMenu menu): base(menu.xPositionOnScreen, menu.yPositionOnScreen, true, menu.actualInventory, menu.highlightMethod, Math.Min(Game1.player.Items.Count, (Game1.game1.uiScreen.Height - 80) / 68 * 12), Math.Min(Game1.player.Items.Count / 12, (Game1.game1.uiScreen.Height - 80) / 68))
        {
			var y = Math.Max(56, yPositionOnScreen - 32 *  (capacity / 12 - 6));
			var diff = yPositionOnScreen - y;
			yPositionOnScreen = y;
			for(int i = 0; i < inventory.Count; i++)
            {
				inventory[i].bounds.Y -= diff;
            }
			height = Math.Min(Game1.game1.uiScreen.Height + 96, 86 + capacity / 12 * 68 + 80);
		}
		public override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
		{
			Game1.drawDialogueBox(xPositionOnScreen - 36, yPositionOnScreen - 136, width + 76, height, false, true);
			for (int i = 0; i < inventory.Count; i++)
			{
				if (_iconShakeTimer.ContainsKey(i) && Game1.currentGameTime.TotalGameTime.TotalSeconds >= _iconShakeTimer[i])
				{
					_iconShakeTimer.Remove(i);
				}
			}
			Color tint = (red == -1) ? Color.White : new Color((int)Utility.Lerp((float)red, (float)Math.Min(255, red + 150), 0.65f), (int)Utility.Lerp((float)green, (float)Math.Min(255, green + 150), 0.65f), (int)Utility.Lerp((float)blue, (float)Math.Min(255, blue + 150), 0.65f));
			Texture2D texture = (red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture;
			for (int j = 0; j < capacity; j++)
			{
				Vector2 toDraw = new Vector2((float)(xPositionOnScreen + j % (capacity / rows) * 64 + horizontalGap * (j % (capacity / rows))), (float)(yPositionOnScreen + j / (capacity / rows) * (64 + verticalGap) + (j / (capacity / rows) - 1) * 4 - ((j >= capacity / rows || !playerInventory || verticalGap != 0) ? 0 : 12)));
				b.Draw(texture, toDraw, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1)), tint, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.5f);
				if ((playerInventory || showGrayedOutSlots) && j >= Game1.player.maxItems.Value)
				{
					b.Draw(texture, toDraw, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 57, -1, -1)), tint * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.5f);
				}
				if (!Game1.options.gamepadControls && j < 12 && playerInventory)
				{
					string strToDraw = (j == 9) ? "0" : ((j == 10) ? "-" : ((j == 11) ? "=" : ((j + 1).ToString() ?? "")));
					Vector2 strSize = Game1.tinyFont.MeasureString(strToDraw);
					b.DrawString(Game1.tinyFont, strToDraw, toDraw + new Vector2(32f - strSize.X / 2f, -strSize.Y), (j == Game1.player.CurrentToolIndex) ? Color.Red : Color.DimGray);
				}
			}
			for (int k = 0; k < capacity; k++)
			{
				Vector2 toDraw2 = new Vector2((float)(xPositionOnScreen + k % (capacity / rows) * 64 + horizontalGap * (k % (capacity / rows))), (float)(yPositionOnScreen + k / (capacity / rows) * (64 + verticalGap) + (k / (capacity / rows) - 1) * 4 - ((k >= capacity / rows || !playerInventory || verticalGap != 0) ? 0 : 12)));
				if (actualInventory.Count > k && actualInventory.ElementAt(k) != null)
				{
					bool highlight = highlightMethod(actualInventory[k]);
					if (_iconShakeTimer.ContainsKey(k))
					{
						toDraw2 += 1f * new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
					}
					actualInventory[k].drawInMenu(b, toDraw2, (inventory.Count > k) ? inventory[k].scale : 1f, (!highlightMethod(actualInventory[k])) ? 0.25f : 1f, 0.865f, StackDrawType.Draw, Color.White, highlight);
				}
			}
		}
    }
}