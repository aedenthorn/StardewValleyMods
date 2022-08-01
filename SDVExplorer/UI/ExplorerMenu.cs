using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace SDVExplorer.UI
{
	public class ExplorerMenu : IClickableMenu
	{
		public ExplorerMenu(int x, int y, int width, int height) : base(x, y, width, height, false)
		{
			upArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f, false);
			downArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f, false);
			scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
			scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 128 - upArrow.bounds.Height - 8);
			for (int i = 0; i < itemsPerPage; i++)
			{
				optionSlots.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + 80 + 4 + i * ((height - 128) / itemsPerPage) + 16, width - 32, (height - 128) / itemsPerPage + 4), i.ToString() ?? "")
				{
					myID = i,
					downNeighborID = ((i < 6) ? (i + 1) : -7777),
					upNeighborID = ((i > 0) ? (i - 1) : -7777),
					fullyImmutable = true
				});
			}
			options = ModEntry.LoadFields(Game1.game1, new List<object>());
		}

		public override bool readyToClose()
		{
			return lastRebindTick != Game1.ticks && base.readyToClose();
		}

		public override void snapToDefaultClickableComponent()
		{
			base.snapToDefaultClickableComponent();
			currentlySnappedComponent = getComponentWithID(1);
			snapCursorToCurrentSnappedComponent();
		}

		public override void applyMovementKey(int direction)
		{
			if (IsDropdownActive())
			{
				if (optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count && options[currentItemIndex + optionsSlotHeld] is FieldDropDown && direction != 2)
				{
					return;
				}
			}
			else
			{
				base.applyMovementKey(direction);
			}
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			base.customSnapBehavior(direction, oldRegion, oldID);
			if (oldID == 6 && direction == 2 && currentItemIndex < Math.Max(0, options.Count - itemsPerPage))
			{
				downArrowPressed();
				Game1.playSound("shiny4");
				return;
			}
			if (oldID == 0 && direction == 0)
			{
				if (currentItemIndex > 0)
				{
					upArrowPressed();
					Game1.playSound("shiny4");
					return;
				}
				currentlySnappedComponent = getComponentWithID(12346);
				if (currentlySnappedComponent != null)
				{
					currentlySnappedComponent.downNeighborID = 0;
				}
				snapCursorToCurrentSnappedComponent();
			}
		}

		private void setScrollBarToCurrentIndex()
		{
			if (options.Count > 0)
			{
				scrollBar.bounds.Y = (int)Math.Floor(((float)scrollBarRunner.Height - scrollBar.bounds.Height) / Math.Max(1, options.Count - itemsPerPage + 1) * currentItemIndex) + upArrow.bounds.Bottom + 4;
				if (scrollBar.bounds.Y > downArrow.bounds.Y - scrollBar.bounds.Height - 4)
				{
					scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
				}
			}
		}

		public override void snapCursorToCurrentSnappedComponent()
		{
			if (currentlySnappedComponent == null || currentlySnappedComponent.myID >= options.Count)
			{
				if (currentlySnappedComponent != null)
				{
					base.snapCursorToCurrentSnappedComponent();
				}
				return;
			}
			FieldDropDown dropdown = options[currentlySnappedComponent.myID + currentItemIndex] as FieldDropDown;
			if (dropdown != null)
			{
				Game1.setMousePosition(currentlySnappedComponent.bounds.Left + dropdown.bounds.Right - 32, currentlySnappedComponent.bounds.Center.Y - 4);
				return;
			}
			Game1.setMousePosition(currentlySnappedComponent.bounds.Left + 48, currentlySnappedComponent.bounds.Center.Y - 12);
		}

		public override void leftClickHeld(int x, int y)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			base.leftClickHeld(x, y);
			if (scrolling)
			{
				int y2 = scrollBar.bounds.Y;
				scrollBar.bounds.Y = Math.Min(yPositionOnScreen + height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, yPositionOnScreen + upArrow.bounds.Height + 20));
				float percentage = (float)(y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
				currentItemIndex = Math.Min(options.Count - itemsPerPage, Math.Max(0, (int)((float)options.Count * percentage)));
				setScrollBarToCurrentIndex();
				if (y2 != scrollBar.bounds.Y)
				{
					Game1.playSound("shiny4");
					return;
				}
			}
			else if (optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count)
			{
				options[currentItemIndex + optionsSlotHeld].leftClickHeld(x - optionSlots[optionsSlotHeld].bounds.X, y - optionSlots[optionsSlotHeld].bounds.Y);
			}
		}

		public override ClickableComponent getCurrentlySnappedComponent()
		{
			return currentlySnappedComponent;
		}

		public override void setCurrentlySnappedComponentTo(int id)
		{
			currentlySnappedComponent = getComponentWithID(id);
			snapCursorToCurrentSnappedComponent();
		}

		public override void receiveKeyPress(Keys key)
		{
			if ((optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count) || (Game1.options.snappyMenus && Game1.options.gamepadControls))
			{
				if (currentlySnappedComponent != null && Game1.options.snappyMenus && Game1.options.gamepadControls && options.Count > currentItemIndex + currentlySnappedComponent.myID && currentItemIndex + currentlySnappedComponent.myID >= 0)
				{
					options[currentItemIndex + currentlySnappedComponent.myID].receiveKeyPress(key);
				}
				else if (options.Count > currentItemIndex + optionsSlotHeld && currentItemIndex + optionsSlotHeld >= 0)
				{
					options[currentItemIndex + optionsSlotHeld].receiveKeyPress(key);
				}
			}
			base.receiveKeyPress(key);
		}

		public override void receiveScrollWheelAction(int direction)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			if (IsDropdownActive())
			{
				return;
			}
			base.receiveScrollWheelAction(direction);
			if (direction > 0 && currentItemIndex > 0)
			{
				upArrowPressed();
				Game1.playSound("shiny4");
			}
			else if (direction < 0 && currentItemIndex < Math.Max(0, options.Count - itemsPerPage))
			{
				downArrowPressed();
				Game1.playSound("shiny4");
			}
			if (Game1.options.SnappyMenus)
			{
				snapCursorToCurrentSnappedComponent();
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			base.releaseLeftClick(x, y);
			if (optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count)
			{
				options[currentItemIndex + optionsSlotHeld].leftClickReleased(x - optionSlots[optionsSlotHeld].bounds.X, y - optionSlots[optionsSlotHeld].bounds.Y);
			}
			optionsSlotHeld = -1;
			scrolling = false;
		}

		public bool IsDropdownActive()
		{
			return optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count && options[currentItemIndex + optionsSlotHeld] is FieldDropDown;
		}

		private void downArrowPressed()
		{
			if (IsDropdownActive())
			{
				return;
			}
			UnsubscribeFromSelectedTextbox();
			downArrow.scale = downArrow.baseScale;
			currentItemIndex++;
			setScrollBarToCurrentIndex();
		}

		public virtual void UnsubscribeFromSelectedTextbox()
		{
			if (Game1.keyboardDispatcher.Subscriber != null)
			{
				foreach (FieldElement option in options)
				{
					if (option is FieldTextEntry && Game1.keyboardDispatcher.Subscriber == (option as FieldTextEntry).textBox)
					{
						Game1.keyboardDispatcher.Subscriber = null;
						break;
					}
				}
			}
		}

		public void preWindowSizeChange()
		{
            _lastSelectedIndex = ((getCurrentlySnappedComponent() != null) ? getCurrentlySnappedComponent().myID : -1);
            _lastCurrentItemIndex = currentItemIndex;
		}

		public void postWindowSizeChange()
		{
			if (Game1.options.SnappyMenus)
			{
				Game1.activeClickableMenu.setCurrentlySnappedComponentTo(_lastSelectedIndex);
			}
			currentItemIndex = _lastCurrentItemIndex;
			setScrollBarToCurrentIndex();
		}

		private void upArrowPressed()
		{
			if (IsDropdownActive())
			{
				return;
			}
			UnsubscribeFromSelectedTextbox();
			upArrow.scale = upArrow.baseScale;
			currentItemIndex--;
			setScrollBarToCurrentIndex();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			if (downArrow.containsPoint(x, y) && currentItemIndex < Math.Max(0, options.Count - itemsPerPage))
			{
				downArrowPressed();
				Game1.playSound("shwip");
			}
			else if (upArrow.containsPoint(x, y) && currentItemIndex > 0)
			{
				upArrowPressed();
				Game1.playSound("shwip");
			}
			else if (scrollBar.containsPoint(x, y))
			{
				scrolling = true;
			}
			else if (!downArrow.containsPoint(x, y) && x > xPositionOnScreen + width && x < xPositionOnScreen + width + 128 && y > yPositionOnScreen && y < yPositionOnScreen + height)
			{
				scrolling = true;
				leftClickHeld(x, y);
				releaseLeftClick(x, y);
			}
			currentItemIndex = Math.Max(0, Math.Min(options.Count - itemsPerPage, currentItemIndex));
			UnsubscribeFromSelectedTextbox();
			for (int i = 0; i < optionSlots.Count; i++)
			{
				if (optionSlots[i].bounds.Contains(x, y) && currentItemIndex + i < options.Count && options[currentItemIndex + i].bounds.Contains(x - optionSlots[i].bounds.X, y - optionSlots[i].bounds.Y))
				{
					options[currentItemIndex + i].receiveLeftClick(x - optionSlots[i].bounds.X, y - optionSlots[i].bounds.Y);
					optionsSlotHeld = i;
					return;
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			for (int i = 0; i < optionSlots.Count; i++)
			{
				if (currentItemIndex >= 0 && currentItemIndex + i < options.Count && options[currentItemIndex + i].bounds.Contains(x - optionSlots[i].bounds.X, y - optionSlots[i].bounds.Y))
				{
					Game1.SetFreeCursorDrag();
					break;
				}
			}
			if (scrollBarRunner.Contains(x, y))
			{
				Game1.SetFreeCursorDrag();
			}
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			hoverText = "";
			upArrow.tryHover(x, y, 0.1f);
			downArrow.tryHover(x, y, 0.1f);
			scrollBar.tryHover(x, y, 0.1f);
			bool flag = scrolling;
		}

		public override void draw(SpriteBatch b)
		{
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen - 100, width, 200, false, true, null, false, true, -1, -1, -1);
			string hierString = "Game1.game1";
			object obj = Game1.game1;
			foreach(var i in ModEntry.currentHeirarchy)
            {
				if (i is FieldInfo)
				{
					var r = AccessTools.Field(obj.GetType(), (i as FieldInfo).Name);
					obj = r.GetValue(obj);
					hierString += " - " + r.Name;
				}
				else if (i is PropertyInfo)
				{
					var r = AccessTools.Property(obj.GetType(), (i as PropertyInfo).Name);
					obj = r.GetValue(obj);
					hierString += " - " + r.Name;
				}
			}
			b.DrawString(Game1.dialogueFont, hierString, new Vector2(xPositionOnScreen + 48, yPositionOnScreen + 16), Game1.textColor);
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true, -1, -1, -1);
			for (int i = 0; i < optionSlots.Count; i++)
			{
				if (currentItemIndex >= 0 && currentItemIndex + i < options.Count)
				{
					options[currentItemIndex + i].draw(b, optionSlots[i].bounds.X, optionSlots[i].bounds.Y, this);
				}
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			if (!GameMenu.forcePreventClose)
			{
				upArrow.draw(b);
				downArrow.draw(b);
				if (options.Count > itemsPerPage)
				{
                    drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f, false, -1f);
					scrollBar.draw(b);
				}
			}
			if (!hoverText.Equals(""))
			{
                drawHoverText(b, hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, -1, -1, -1, -1, 1f, null, null);
			}
			if (!GameMenu.forcePreventClose && shouldDrawCloseButton())
			{
				base.draw(b);
			}
			if (!Game1.options.hardwareCursor)
			{
				base.drawMouse(b, true, -1);
			}
		}

		public int itemsPerPage = 10;

		private string hoverText = "";

		public List<ClickableComponent> optionSlots = new List<ClickableComponent>();

		public int currentItemIndex;

		private ClickableTextureComponent upArrow;

		private ClickableTextureComponent downArrow;

		private ClickableTextureComponent scrollBar;

		private bool scrolling;

		public List<FieldElement> options = new List<FieldElement>();

		private Rectangle scrollBarRunner;

		protected static int _lastSelectedIndex;

		protected static int _lastCurrentItemIndex;

		public int lastRebindTick = -1;

		private int optionsSlotHeld = -1;
	}

}
