using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace AllChestsMenu
{
	public class ChestMenu : InventoryMenu
	{
		const int columns = 12;

		public ChestMenu(int xPosition, int yPosition, bool playerInventory, IList<Item> actualInventory = null, highlightThisItem highlightMethod = null, int capacity = -1, int rows = 3, int horizontalGap = 0, int verticalGap = 0, bool drawSlots = true) : base(xPosition, yPosition, playerInventory, actualInventory, highlightMethod, capacity, rows, horizontalGap, verticalGap, drawSlots)
		{
			initialize(xPositionOnScreen, yPositionOnScreen, 64 * columns, 64 * rows + 16);
			inventory.Clear();
			for (int j = 0; j < capacity; j++)
			{
				int rowIndex = j / columns;
				int columnIndex = j % columns;
				int num = (!playerInventory) ? ((j >= capacity - columns) ? (-99998) : (j + columns)) : ((j < actualInventory.Count - columns) ? (j + columns) : ((j < actualInventory.Count - 3 && actualInventory.Count >= 36) ? (-99998) : ((j % 12 < 2) ? 102 : 101)));

				inventory.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + columnIndex * 64 + horizontalGap * columnIndex, yPositionOnScreen + rowIndex * (64 + verticalGap) + 4, 64, 64), j.ToString() ?? "")
				{
					myID = j,
					leftNeighborID = (j % columns != 0) ? (j - 1) : 107,
					rightNeighborID = ((j + 1) % columns != 0) ? (j + 1) : 106,
					downNeighborID = num,
					upNeighborID = (j < columns) ? (12340 + j) : (j - columns),
					region = 9000,
					upNeighborImmutable = true,
					downNeighborImmutable = true,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
			}
		}

		public override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
		{
			for (int i = 0; i < inventory.Count; i++)
			{
				if (_iconShakeTimer.TryGetValue(i, out double value) && Game1.currentGameTime.TotalGameTime.TotalSeconds >= value)
				{
					_iconShakeTimer.Remove(i);
				}
			}

			Color color = (red == -1) ? Color.White : new Color((int)Utility.Lerp(red, Math.Min(255, red + 150), 0.65f), (int)Utility.Lerp(green, Math.Min(255, green + 150), 0.65f), (int)Utility.Lerp(blue, Math.Min(255, blue + 150), 0.65f));
			Texture2D texture = (red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture;

			if (drawSlots)
			{
				for (int j = 0; j < capacity; j++)
				{
					int rowIndex = j / columns;
					int columnIndex = j % columns;
					Vector2 vector = new(xPositionOnScreen + columnIndex * 64 + horizontalGap * columnIndex, yPositionOnScreen + rowIndex * (64 + verticalGap) + 4);

					b.Draw(texture, vector, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.5f);
					if ((playerInventory || showGrayedOutSlots) && j >= Game1.player.maxItems.Value)
					{
						b.Draw(texture, vector, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 57), color * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.5f);
					}
					if (!Game1.options.gamepadControls && j < 12 && playerInventory)
					{
						string text = j switch
						{
							11 => "=",
							10 => "-",
							9 => "0",
							_ => (j + 1).ToString() ?? "",
						};
						Vector2 vector2 = Game1.tinyFont.MeasureString(text);

						b.DrawString(Game1.tinyFont, text, vector + new Vector2(32f - vector2.X / 2f, 0f - vector2.Y), (j == Game1.player.CurrentToolIndex) ? Color.Red : Color.DimGray);
					}
				}
				for (int k = 0; k < capacity; k++)
				{
					int rowIndex = k / columns;
					int columnIndex = k % columns;
					Vector2 location = new(xPositionOnScreen + columnIndex * 64 + horizontalGap * columnIndex, yPositionOnScreen + rowIndex * (64 + verticalGap) + 4);

					if (actualInventory.Count > k && actualInventory[k] != null)
					{
						bool drawShadow = highlightMethod(actualInventory[k]);

						if (_iconShakeTimer.ContainsKey(k))
						{
							location += 1f * new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
						}
						actualInventory[k].drawInMenu(b, location, (inventory.Count > k) ? inventory[k].scale : 1f, (!highlightMethod(actualInventory[k])) ? 0.25f : 1f, 0.865f, StackDrawType.Draw, Color.White, drawShadow);
					}
				}
				return;
			}
			for (int l = 0; l < capacity; l++)
			{
				int rowIndex = l / columns;
				int columnIndex = l % columns;
				Vector2 location2 = new(xPositionOnScreen + columnIndex * 64 + horizontalGap * columnIndex, yPositionOnScreen + rowIndex * (64 + verticalGap) + 4);

				if (actualInventory.Count > l && actualInventory[l] != null)
				{
					bool flag = highlightMethod(actualInventory[l]);

					if (_iconShakeTimer.ContainsKey(l))
					{
						location2 += 1f * new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
					}
					actualInventory[l].drawInMenu(b, location2, (inventory.Count > l) ? inventory[l].scale : 1f, (!flag) ? 0.25f : 1f, 0.865f, StackDrawType.Draw, Color.White, flag);
				}
			}
		}

		public new List<Vector2> GetSlotDrawPositions()
		{
			List<Vector2> list = new();

			for (int i = 0; i < capacity; i++)
			{
				int rowIndex = i / columns;
				int columnIndex = i % columns;

				list.Add(new Vector2(xPositionOnScreen + columnIndex * 64 + horizontalGap * columnIndex, yPositionOnScreen + rowIndex * (64 + verticalGap) + 4));
			}
			return list;
		}

		public new List<ClickableComponent> GetBorder(BorderSide side)
		{
			List<ClickableComponent> list = new();

			switch (side)
			{
				case BorderSide.Bottom:
				{
					for (int l = 0; l < inventory.Count; l++)
					{
						if (l >= actualInventory.Count - columns)
						{
							list.Add(inventory[l]);
						}
					}
					break;
				}
				case BorderSide.Top:
				{
					for (int j = 0; j < inventory.Count; j++)
					{
						if (j < columns)
						{
							list.Add(inventory[j]);
						}
					}
					break;
				}
				case BorderSide.Left:
				{
					for (int k = 0; k < inventory.Count; k++)
					{
						if (k % columns == 0)
						{
							list.Add(inventory[k]);
						}
					}
					break;
				}
				case BorderSide.Right:
				{
					for (int i = 0; i < inventory.Count; i++)
					{
						if (i % columns == columns - 1)
						{
							list.Add(inventory[i]);
						}
					}
					break;
				}
			}
			return list;
		}

		public Item LeftClick(int x, int y, Item toPlace, bool playSound = true, bool isShippingBinChest = false, bool isMiniShippingBinChest = false)
		{
			if ((isShippingBinChest || isMiniShippingBinChest) && toPlace is not null && !Utility.highlightShippableObjects(toPlace))
				return toPlace;

			foreach (ClickableComponent item in inventory)
			{
				if (!item.containsPoint(x, y))
				{
					continue;
				}

				int num = Convert.ToInt32(item.name);

				if (num >= actualInventory.Count || (actualInventory[num] != null && !highlightMethod(actualInventory[num]) && !actualInventory[num].canStackWith(toPlace)))
				{
					continue;
				}
				if (actualInventory[num] != null)
				{
					if (toPlace != null)
					{
						if (isShippingBinChest && !ModEntry.Config.UnrestrictedShippingBin)
						{
							continue;
						}
						if (playSound)
						{
							Game1.playSound("stoneStep");
						}
						return Utility.addItemToInventory(toPlace, num, actualInventory, onAddItem);
					}
					if (playSound)
					{
						Game1.playSound(moveItemSound);
					}
					return Utility.removeItemFromInventory(num, actualInventory);
				}
				if (toPlace != null)
				{
					if (playSound)
					{
						Game1.playSound("stoneStep");
					}
					return Utility.addItemToInventory(toPlace, num, actualInventory, onAddItem);
				}
			}
			return toPlace;
		}
	}
}
