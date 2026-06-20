using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace AllChestsMenu
{
	/// <summary>
	/// Menu for consolidating duplicate items from multiple chests into one
	/// </summary>
	public class ConsolidationMenu : IClickableMenu
	{
		private readonly List<ConsolidationItem> items;
		private readonly AllChestsMenu parentMenu;
		private int selectedItemIndex = 0;

		private int itemScroll = 0;
		private int destScroll = 0;

		private ClickableTextureComponent closeButton;
		private ClickableTextureComponent itemScrollUpButton;
		private ClickableTextureComponent itemScrollDownButton;
		private ClickableTextureComponent destScrollUpButton;
		private ClickableTextureComponent destScrollDownButton;

		private const int SlotHeight = 64;
		private const int BorderWidth = 32;

		public ConsolidationMenu(List<ConsolidationItem> consolidationItems, AllChestsMenu parent)
		{
			items = consolidationItems;
			parentMenu = parent;

			int menuWidth = Math.Min(1000, Game1.uiViewport.Width - BorderWidth * 2);
			int menuHeight = Math.Min(600, Game1.uiViewport.Height - BorderWidth * 2);
			xPositionOnScreen = (Game1.uiViewport.Width - menuWidth) / 2;
			yPositionOnScreen = (Game1.uiViewport.Height - menuHeight) / 2;
			width = menuWidth;
			height = menuHeight;

			// Close button
			closeButton = new ClickableTextureComponent(
				"close",
				new Rectangle(xPositionOnScreen + width - 48, yPositionOnScreen + 8, 32, 32),
				"",
				"",
				Game1.mouseCursors,
				new Rectangle(341, 494, 14, 14),
				3f,
				false
			);

            // Up/down buttons for left pane
			itemScrollUpButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + BorderWidth + 4, yPositionOnScreen + BorderWidth + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
			itemScrollDownButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + BorderWidth + 4, yPositionOnScreen + height - BorderWidth - 52, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);

            // Up/down buttons for right pane
			destScrollUpButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width / 2 + 16, yPositionOnScreen + BorderWidth + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
			destScrollDownButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width / 2 + 16, yPositionOnScreen + height - BorderWidth - 52, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
		}

		private int GetMaxItemsVisible()
		{
			// Subtract title heights and buttons offsets
			return (height - BorderWidth * 2 - 80) / (SlotHeight + 8);
		}

		public override void draw(SpriteBatch b)
		{
			// Draw background
			Game1.drawDialogueBox(xPositionOnScreen - BorderWidth, yPositionOnScreen - BorderWidth, width + BorderWidth * 2, height + BorderWidth * 2, false, true);

			// Draw title
			string title = ModEntry.SHelper.Translation.Get("consolidate-menu-title");
			var titleSize = Game1.smallFont.MeasureString(title);
			b.DrawString(Game1.smallFont, title, new Vector2(xPositionOnScreen + width / 2 - titleSize.X / 2, yPositionOnScreen + 8), Game1.textColor);

			int maxVisible = GetMaxItemsVisible();

			// == LEFT PANE: ITEMS ==
			string itemsLabel = ModEntry.SHelper.Translation.Get("item-found-in");
            try { itemsLabel = string.Format(itemsLabel, items.Count); } catch { } 
			b.DrawString(Game1.smallFont, itemsLabel, new Vector2(xPositionOnScreen + BorderWidth + 8, yPositionOnScreen + BorderWidth + 24), Game1.textColor);

            int paneWidth = width / 2 - BorderWidth - 24;
			int startY = yPositionOnScreen + BorderWidth + 64;

			for (int i = 0; i < maxVisible; i++)
			{
				int itemIndex = itemScroll + i;
				if (itemIndex >= items.Count) break;

				var item = items[itemIndex];
				int drawY = startY + i * (SlotHeight + 8);
                Rectangle slotRect = new Rectangle(xPositionOnScreen + BorderWidth + 56, drawY, paneWidth - 64, SlotHeight);
				Rectangle skipRect = new Rectangle(slotRect.Right - 36, slotRect.Y + 16, 32, 32);

				// Draw slot background
				Color bgColor = itemIndex == selectedItemIndex ? Color.Wheat * 0.8f : Color.White * 0.3f;
                bool hovered = slotRect.Contains(Game1.getMouseX(), Game1.getMouseY());
				if (hovered) bgColor = Color.Wheat * 1.0f;
				b.Draw(Game1.staminaRect, slotRect, bgColor);

				// Draw skip button if hovered
				if (hovered || itemIndex == selectedItemIndex)
				{
					bool skipHovered = skipRect.Contains(Game1.getMouseX(), Game1.getMouseY());
					b.Draw(Game1.mouseCursors, skipRect, new Rectangle(341, 494, 14, 14), skipHovered ? Color.White : Color.Gray, 0f, Vector2.Zero, SpriteEffects.None, 1f);
				}

				// Draw item
				if (item.SampleItem != null)
				{
					var itemPos = new Vector2(slotRect.X + 8, slotRect.Y + 8);
					item.SampleItem.drawInMenu(b, itemPos, 1);

					// Draw stack count inside the slot, closer to the item icon
					string countText = $"x{item.TotalStack}";
					var countSize = Game1.smallFont.MeasureString(countText);
					b.DrawString(Game1.smallFont, countText, new Vector2(itemPos.X + 64 + 8, slotRect.Y + 16), Game1.textColor);

					// Draw chest count
					string chestsText = $"{item.ChestsContaining.Count} chests";
					b.DrawString(Game1.smallFont, chestsText, new Vector2(itemPos.X + 64 + 8, slotRect.Y + 36), Game1.textColor * 0.8f);
				}
			}

			if (items.Count > maxVisible)
			{
				if (itemScroll > 0) itemScrollUpButton.draw(b);
				if (itemScroll + maxVisible < items.Count) itemScrollDownButton.draw(b);
			}

			// == RIGHT PANE: DESTINATIONS ==
            if (selectedItemIndex < items.Count)
            {
                var selectedItem = items[selectedItemIndex];
			    string destLabel = ModEntry.SHelper.Translation.Get("select-destination");
			    b.DrawString(Game1.smallFont, destLabel, new Vector2(xPositionOnScreen + width / 2 + 16 + 56, yPositionOnScreen + BorderWidth + 24), Game1.textColor);

                int maxDestVisible = GetMaxItemsVisible();
                for (int i = 0; i < maxDestVisible; i++)
                {
                    int destIndex = destScroll + i;
                    if (destIndex >= selectedItem.ChestsContaining.Count) break;

                    var chest = selectedItem.ChestsContaining[destIndex];
					bool hasSpace = selectedItem.ChestsWithSpace.Contains(chest);

                    int drawY = startY + i * (SlotHeight + 8);
                    Rectangle destRect = new Rectangle(xPositionOnScreen + width / 2 + 16 + 56, drawY, paneWidth - 64, SlotHeight);

                    Color destBgColor = Color.White * 0.3f;
                    if (hasSpace && destRect.Contains(Game1.getMouseX(), Game1.getMouseY())) destBgColor = Color.Wheat * 0.8f;
					if (!hasSpace) destBgColor = Color.Black * 0.3f;

                    b.Draw(Game1.staminaRect, destRect, destBgColor);

                    // Draw chest name
                    string chestLabel = !string.IsNullOrEmpty(chest.label)
                        ? chest.label
                        : $"{chest.location} ({chest.tile.X},{chest.tile.Y})";

					// Calculate item count in this chest
					int chestItemCount = 0;
					if (chest.chest != null && selectedItem.SampleItem != null)
					{
						foreach (var item in chest.chest.Items)
						{
							if (item != null && item.canStackWith(selectedItem.SampleItem))
							{
								chestItemCount += item.Stack;
							}
						}
					}
					
					chestLabel += $" (x{chestItemCount})";
                    
					Color textColor = hasSpace ? Game1.textColor : Game1.textColor * 0.5f;
					b.DrawString(Game1.smallFont, chestLabel, new Vector2(destRect.X + 8, destRect.Y + 16), textColor);

					if (!hasSpace)
					{
						string fullText = ModEntry.SHelper.Translation.Get("not-enough-space");
						var fullSize = Game1.smallFont.MeasureString(fullText);
						// Ensure text doesn't overflow to the right
						float textX = destRect.Right - fullSize.X - 8;
						if (textX < destRect.X + 8) textX = destRect.X + 8; // clamp
						b.DrawString(Game1.smallFont, fullText, new Vector2(textX, destRect.Y + 32), Color.Red * 0.8f);
					}
                }

                if (selectedItem.ChestsContaining.Count > maxDestVisible)
                {
                    if (destScroll > 0) destScrollUpButton.draw(b);
                    if (destScroll + maxDestVisible < selectedItem.ChestsContaining.Count) destScrollDownButton.draw(b);
                }
            }

			// Draw close button
			closeButton.draw(b);

			// Draw mouse cursor
			drawMouse(b);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (closeButton.containsPoint(x, y))
			{
				Game1.playSound("bigDeSelect");
				Game1.activeClickableMenu = parentMenu;
				return;
			}

            int maxVisible = GetMaxItemsVisible();

            // Scroll buttons items
            if (items.Count > maxVisible)
            {
                if (itemScroll > 0 && itemScrollUpButton.containsPoint(x, y)) { itemScroll--; Game1.playSound("shwip"); return; }
                if (itemScroll + maxVisible < items.Count && itemScrollDownButton.containsPoint(x, y)) { itemScroll++; Game1.playSound("shwip"); return; }
            }

            // Scroll buttons dest
            if (selectedItemIndex < items.Count)
            {
                var selectedItem = items[selectedItemIndex];
                if (selectedItem.ChestsContaining.Count > maxVisible)
                {
                    if (destScroll > 0 && destScrollUpButton.containsPoint(x, y)) { destScroll--; Game1.playSound("shwip"); return; }
                    if (destScroll + maxVisible < selectedItem.ChestsContaining.Count && destScrollDownButton.containsPoint(x, y)) { destScroll++; Game1.playSound("shwip"); return; }
                }
            }

            int paneWidth = width / 2 - BorderWidth - 24;
			int startY = yPositionOnScreen + BorderWidth + 64;

			// Check item slots
			for (int i = 0; i < maxVisible; i++)
			{
                int itemIndex = itemScroll + i;
                if (itemIndex >= items.Count) break;

                int drawY = startY + i * (SlotHeight + 8);
                Rectangle slotRect = new Rectangle(xPositionOnScreen + BorderWidth + 56, drawY, paneWidth - 64, SlotHeight);
				Rectangle skipRect = new Rectangle(slotRect.Right - 36, slotRect.Y + 16, 32, 32);

				if (skipRect.Contains(x, y))
				{
					// Skip this item
					items.RemoveAt(itemIndex);
					selectedItemIndex = Math.Min(selectedItemIndex, Math.Max(0, items.Count - 1));
					destScroll = 0;

					if (itemScroll > Math.Max(0, items.Count - GetMaxItemsVisible()))
						itemScroll = Math.Max(0, items.Count - GetMaxItemsVisible());

					Game1.playSound("trashcan");

					if (items.Count == 0)
					{
						Game1.activeClickableMenu = parentMenu;
					}
					return;
				}
				else if (slotRect.Contains(x, y))
				{
					selectedItemIndex = itemIndex;
					destScroll = 0;
					Game1.playSound("tinyWhip");
					return;
				}
			}

			// Check destination slots for selected item
			if (selectedItemIndex < items.Count)
			{
				var selectedItem = items[selectedItemIndex];

				for (int i = 0; i < maxVisible; i++)
				{
                    int destIndex = destScroll + i;
					if (destIndex >= selectedItem.ChestsContaining.Count) break;

                    int drawY = startY + i * (SlotHeight + 8);
                    Rectangle destRect = new Rectangle(xPositionOnScreen + width / 2 + 16 + 56, drawY, paneWidth - 64, SlotHeight);
					
					if (destRect.Contains(x, y))
					{
						var chest = selectedItem.ChestsContaining[destIndex];
						if (selectedItem.ChestsWithSpace.Contains(chest))
						{
							PerformConsolidation(chest);
							Game1.playSound("coin");
							return;
						}
						else
						{
							Game1.playSound("cancel");
							return;
						}
					}
				}
			}
		}

		private void PerformConsolidation(ChestData destination)
		{
			if (selectedItemIndex >= items.Count)
				return;

			var selectedItem = items[selectedItemIndex];
			parentMenu.ConsolidateItem(selectedItem, destination);

			// Remove consolidated item and refresh
			if (selectedItemIndex < items.Count)
			{
				items.RemoveAt(selectedItemIndex);
				selectedItemIndex = Math.Min(selectedItemIndex, Math.Max(0, items.Count - 1));
				destScroll = 0;

                if (itemScroll > Math.Max(0, items.Count - GetMaxItemsVisible()))
					itemScroll = Math.Max(0, items.Count - GetMaxItemsVisible());

				if (items.Count == 0)
				{
					Game1.activeClickableMenu = parentMenu;
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true) { }

		public override void receiveScrollWheelAction(int direction)
		{
			if (items.Count == 0) return;

            int maxVisible = GetMaxItemsVisible();

			if (Game1.getMouseX() < xPositionOnScreen + width / 2)
			{
				// Scroll left pane
				if (direction > 0 && itemScroll > 0)
				{
					itemScroll--;
					Game1.playSound("shiny4");
				}
				else if (direction < 0 && itemScroll + maxVisible < items.Count)
				{
					itemScroll++;
					Game1.playSound("shiny4");
				}
			}
			else
			{
				// Scroll right pane
				var selectedItem = items[selectedItemIndex];
				if (direction > 0 && destScroll > 0)
				{
					destScroll--;
					Game1.playSound("shiny4");
				}
				else if (direction < 0 && destScroll + maxVisible < selectedItem.ChestsContaining.Count)
				{
					destScroll++;
					Game1.playSound("shiny4");
				}
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (key == Keys.Escape || Game1.options.doesInputListContain(Game1.options.menuButton, key))
			{
				Game1.activeClickableMenu = parentMenu;
			}
		}
	}
}
