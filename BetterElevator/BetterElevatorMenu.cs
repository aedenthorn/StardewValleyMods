using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace BetterElevator
{
    public class BetterElevatorMenu : IClickableMenu
    {
		public int levelToGoto;
		public BetterElevatorMenu() : base(0, 0, 0, 0, true)
		{
			width = (484 + borderWidth * 2);
			height = 54 + borderWidth * 3;
			xPositionOnScreen = Game1.uiViewport.Width / 2 - width / 2;
			yPositionOnScreen = Game1.uiViewport.Height / 2 - height / 2;
            initializeUpperRightCloseButton();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (isWithinBounds(x, y))
			{

				base.receiveLeftClick(x, y, true);
				return;
			}
			Game1.exitActiveMenu();
		}
        public override void receiveKeyPress(Keys key)
        {
            switch (key)
            {
				case Keys.D0:
				case Keys.NumPad0:
					AddNumber(0);
					break;
				case Keys.D1:
				case Keys.NumPad1:
					AddNumber(1);
					break;
				case Keys.D2:
				case Keys.NumPad2:
					AddNumber(2);
					break;
				case Keys.D3:
				case Keys.NumPad3:
					AddNumber(3);
					break;
				case Keys.D4:
				case Keys.NumPad4:
					AddNumber(4);
					break;
				case Keys.D5:
				case Keys.NumPad5:
					AddNumber(5);
					break;
				case Keys.D6:
				case Keys.NumPad6:
					AddNumber(6);
					break;
				case Keys.D7:
				case Keys.NumPad7:
					AddNumber(7);
					break;
				case Keys.D8:
				case Keys.NumPad8:
					AddNumber(8);
					break;
				case Keys.D9:
				case Keys.NumPad9:
					AddNumber(9);
					break;
				case Keys.Enter:
					if(levelToGoto > 0)
                    {
						if (IsSkullCave())
							levelToGoto += 120;
						Game1.player.currentLocation.playSound("stairsdown");
						ModEntry.SMonitor.Log($"Entering mine level {levelToGoto}");
						Game1.enterMine(levelToGoto);
					}
					Game1.exitActiveMenu();
					break;
				case Keys.Back:
					if(levelToGoto > 0)
                    {
						string ls = levelToGoto.ToString();
						if (ls.Length == 1) 
						{
							levelToGoto = 0;
						}
                        else
                        {
							ls = ls.Substring(0, ls.Length - 1);
							levelToGoto = int.Parse(ls);
                        }
					}
					break;

			}
            base.receiveKeyPress(key);
        }

        private void AddNumber(int n)
        {
			if (n == 0 && levelToGoto == 0)
				return;
			string ls = (levelToGoto == 0 ? "" : levelToGoto.ToString())  + n;
			int newLevel;
			try
			{
				newLevel = int.Parse(ls);
			}
			catch (OverflowException)
			{
				newLevel = int.MaxValue;
			}
			if (IsSkullCave())
			{
				if (!ModEntry.Config.Unrestricted)
				{
                    newLevel = Math.Min(Math.Max(MineShaft.lowestLevelReached - 120, 0), newLevel);
                }
				if (newLevel == 77377 - 120)
				{
					newLevel++;
				}
				if (newLevel > int.MaxValue - 120)
				{
					newLevel = int.MaxValue - 120;
				}
            }
			else
			{
                if (!ModEntry.Config.Unrestricted)
                {
                    newLevel = Math.Min(Math.Min(MineShaft.lowestLevelReached, 120), newLevel);
                }
				if (newLevel > 120)
				{
					newLevel = 120;
				}
            }
			levelToGoto = newLevel;
		}

		private bool IsSkullCave()
		{
			return ((Game1.player.currentLocation is MineShaft && (Game1.player.currentLocation as MineShaft).getMineArea(-1) == 121) || Game1.player.currentLocation.Name == "SkullCave");

        }

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public static int drawTicks;

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen - 64 + 8, width + 21, height + 64, false, true, null, false, true, -1, -1, -1);
			base.draw(b);

            int lowestLevel = MineShaft.lowestLevelReached;
            if (IsSkullCave()) { 
				lowestLevel = Math.Max(0, lowestLevel - 120);
            }
            else
            {
                lowestLevel = Math.Min(120, lowestLevel);
            }

            string level = (levelToGoto > 0 ?  levelToGoto.ToString() : "");
			
			int blinkRate = 16;
			drawTicks++;
			if (drawTicks < blinkRate)
				level += "_";
			else if (drawTicks > blinkRate * 2)
				drawTicks = 0;

			int offset = 48;
			b.DrawString(Game1.dialogueFont, string.Format(ModEntry.SHelper.Translation.Get("level-reached-x"), lowestLevel), new Vector2(xPositionOnScreen + offset, yPositionOnScreen + offset), Game1.textColor);
			b.DrawString(Game1.dialogueFont, string.Format(ModEntry.SHelper.Translation.Get("enter-level")) + " " + level, new Vector2(xPositionOnScreen + offset, yPositionOnScreen + offset * 2), Game1.textColor);
			drawMouse(b, false, -1);

		}
	}
}