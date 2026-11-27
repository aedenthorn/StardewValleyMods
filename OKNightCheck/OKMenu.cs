using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace OKNightCheck
{
	public class OKMenu : ShippingMenu
	{
		private ClickableTextureComponent quitButton;

        public OKMenu() : base(new List<Item>())
		{
			if (ModEntry.quotesAPI is not null)
            {
				quote = ModEntry.quotesAPI.GetRandomQuoteAndAuthor(true);
			}

			_activated = false;
			if (!Game1.wasRainingYesterday)
			{
				Game1.changeMusicTrack(Game1.currentSeason.Equals("summer") ? "nightTime" : "none", false, Game1.MusicContext.Default);
			}
			plusButtonWidth = 40;
			itemSlotWidth = 96;
			itemAndPlusButtonWidth = plusButtonWidth + itemSlotWidth + 8;
			centerX = Game1.uiViewport.Width / 2;
			centerY = Game1.uiViewport.Height / 2;
			_hasFinished = false;
			Rectangle okRect = new Rectangle(centerX + totalWidth / 2 - itemAndPlusButtonWidth + 32, centerY + 300 - 64, 64, 64);
			okButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11382"), okRect, null, Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11382"), Game1.mouseCursors, new Rectangle(128, 256, 64, 64), 1f, false)
			{
				myID = 101,
			};
            if (ModEntry.Config.ShowQuitButton)
            {
                Rectangle quitRect = new Rectangle(0, 0, 60, 60);
                quitButton = new ClickableTextureComponent("Quit", quitRect, null, "Quit", Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 5, false)
                {
                    myID = 102,
                    rightNeighborID = 101
                };
                okButton.leftNeighborID = 102;
            }
            backButton = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + height - 64, 48, 44), null, "", Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 103,
				rightNeighborID = -7777
			};
			forwardButton = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width - 32 - 48, yPositionOnScreen + height - 64, 48, 44), null, "", Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 102,
				leftNeighborID = 103
			};
			if (Game1.dayOfMonth == 25 && Game1.currentSeason.Equals("winter"))
			{
				Vector2 startingPosition = new Vector2((float)Game1.uiViewport.Width, (float)Game1.random.Next(0, 200));
				Rectangle sourceRect = new Rectangle(640, 800, 32, 16);
				int loops = 1000;
				TemporaryAnimatedSprite t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 80f, 2, loops, startingPosition, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, true);
				t.motion = new Vector2(-4f, 0f);
				t.delayBeforeAnimationStart = 3000;
				animations.Add(t);
			}
			RepositionItems();
            populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				snapToDefaultClickableComponent();
			}
		}

		public new void RepositionItems()
		{
			centerX = Game1.uiViewport.Width / 2;
			centerY = Game1.uiViewport.Height / 2;
			if (dayPlaqueY < 0)
			{
				dayPlaqueY = -64;
			}
			backButton.bounds.X = xPositionOnScreen + 32;
			backButton.bounds.Y = yPositionOnScreen + height - 64;
			forwardButton.bounds.X = xPositionOnScreen + width - 32 - 48;
			forwardButton.bounds.Y = yPositionOnScreen + height - 64;
			Rectangle okRect = new Rectangle(centerX + totalWidth / 2 - itemAndPlusButtonWidth + 32, centerY + 300 - 64, 64, 64);
			okButton.bounds = okRect;
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(101);
			snapCursorToCurrentSnappedComponent();
		}


		public override void applyMovementKey(int direction)
		{
			if (!CanReceiveInput())
			{
				return;
			}
			base.applyMovementKey(direction);
		}

		public override void performHoverAction(int x, int y)
		{
			if (!CanReceiveInput())
			{
				return;
			}
			base.performHoverAction(x, y);
			okButton.tryHover(x, y, 0.1f);
			backButton.tryHover(x, y, 0.5f);
			forwardButton.tryHover(x, y, 0.5f);
		}

		public new bool CanReceiveInput()
		{
			return introTimer <= 0 && saveGameMenu == null && !outro;
		}

		public override void receiveKeyPress(Keys key)
		{
			if (!CanReceiveInput())
			{
				return;
			}
			if (introTimer <= 0 && !Game1.options.gamepadControls && (key.Equals(Keys.Escape) || Game1.options.doesInputListContain(Game1.options.menuButton, key)))
			{
				receiveLeftClick(okButton.bounds.Center.X, okButton.bounds.Center.Y, true);
				return;
			}
			if (introTimer <= 0 && (!Game1.options.gamepadControls || !Game1.options.doesInputListContain(Game1.options.menuButton, key)))
			{
				base.receiveKeyPress(key);
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (!CanReceiveInput())
			{
				return;
			}
			base.receiveGamePadButton(b);
			if ((b == Buttons.Start || b == Buttons.B) && !outro)
			{
				if (introTimer <= 0)
				{
					okClicked();
					return;
				}
				introTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds * 2;
			}
		}

		private void okClicked()
		{
			outro = true;
			outroFadeTimer = 800;
			Game1.playSound("bigDeSelect");
			Game1.changeMusicTrack("none", false, Game1.MusicContext.Default);
		}
		
		private void quitClicked()
		{
			Game1.changeMusicTrack("none", false, Game1.MusicContext.Default);
            if (Game1.options.optionsDirty)
            {
                Game1.options.SaveDefaultOptions();
            }
            Game1.playSound("bigDeSelect");
            Game1.ExitToTitle(null);
        }

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!CanReceiveInput())
			{
				return;
			}
			if (outro && !savedYet)
			{
				SaveGameMenu saveGameMenu = this.saveGameMenu;
				return;
			}
			if (savedYet)
			{
				return;
			}
			base.receiveLeftClick(x, y, playSound);
			if (introTimer <= 0)
			{
				if(okButton.containsPoint(x, y))
					okClicked();
				if(quitButton.containsPoint(x, y))
                    quitClicked();
			}
			if (Game1.dayOfMonth == 28 && timesPokedMoon <= 10 && new Rectangle(Game1.uiViewport.Width - 176, 4, 172, 172).Contains(x, y))
			{
				moonShake = 100;
				timesPokedMoon++;
				if (timesPokedMoon > 10)
				{
					Game1.playSound("shadowDie");
					return;
				}
				Game1.playSound("thudStep");
				return;
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
            initialize(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height, false);
			RepositionItems();
		}


		public override void update(GameTime time)
		{
			base.update(time);
			if (!this._activated)
			{
				this._activated = true;
				Game1.player.team.endOfNightStatus.UpdateState("shipment");
			}
			if (!this._hasFinished)
			{
				if (this.saveGameMenu != null)
				{
					this.saveGameMenu.update(time);
					if (this.saveGameMenu.quit)
					{
						this.saveGameMenu = null;
						this.savedYet = true;
					}
				}
				this.weatherX += (float)time.ElapsedGameTime.Milliseconds * 0.03f;
				for (int i = this.animations.Count - 1; i >= 0; i--)
				{
					if (this.animations[i].update(time))
					{
						this.animations.RemoveAt(i);
					}
				}
				if (this.outro)
				{
					if (this.outroFadeTimer > 0)
					{
						this.outroFadeTimer -= time.ElapsedGameTime.Milliseconds;
					}
					else if (this.outroFadeTimer <= 0 && this.dayPlaqueY < this.centerY - 64)
					{
						if (this.animations.Count > 0)
						{
							this.animations.Clear();
						}
						this.dayPlaqueY += (int)Math.Ceiling((double)((float)time.ElapsedGameTime.Milliseconds * 0.35f));
						if (this.dayPlaqueY >= this.centerY - 64)
						{
							this.outroPauseBeforeDateChange = 700;
						}
					}
					else if (this.outroPauseBeforeDateChange > 0)
					{
						this.outroPauseBeforeDateChange -= time.ElapsedGameTime.Milliseconds;
						if (this.outroPauseBeforeDateChange <= 0)
						{
							this.newDayPlaque = true;
							Game1.playSound("newRecipe");
							if (!Game1.currentSeason.Equals("winter") && Game1.game1.IsMainInstance)
							{
								DelayedAction.playSoundAfterDelay(Game1.IsRainingHere(null) ? "rainsound" : "rooster", 1500, null, -1);
							}
							this.finalOutroTimer = 2000;
							this.animations.Clear();
							if (!this.savedYet)
							{
								if (this.saveGameMenu == null)
								{
									this.saveGameMenu = new SaveGameMenu();
								}
								return;
							}
						}
					}
					else if (this.finalOutroTimer > 0 && this.savedYet)
					{
						this.finalOutroTimer -= time.ElapsedGameTime.Milliseconds;
						if (this.finalOutroTimer <= 0)
						{
							this._hasFinished = true;
						}
					}
				}
				if (this.introTimer >= 0)
				{
					int num = this.introTimer;
					this.introTimer -= time.ElapsedGameTime.Milliseconds * ((Game1.oldMouseState.LeftButton == ButtonState.Pressed) ? 3 : 1);
				}
				else if (Game1.dayOfMonth != 28 && !this.outro)
				{
					if (!Game1.wasRainingYesterday)
					{
						Vector2 startingPosition = new Vector2((float)Game1.uiViewport.Width, (float)Game1.random.Next(200));
						Rectangle sourceRect = new Rectangle(640, 752, 16, 16);
						int rows = Game1.random.Next(1, 4);
						if (Game1.random.NextDouble() < 0.001)
						{
							bool flip = Game1.random.NextDouble() < 0.5;
							if (Game1.random.NextDouble() < 0.5)
							{
								this.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(640, 826, 16, 8), 40f, 4, 0, new Vector2((float)Game1.random.Next(this.centerX * 2), (float)Game1.random.Next(this.centerY)), false, flip)
								{
									rotation = 3.14159274f,
									scale = 4f,
									motion = new Vector2((float)(flip ? -8 : 8), 8f),
									local = true
								});
							}
							else
							{
								this.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(258, 1680, 16, 16), 40f, 4, 0, new Vector2((float)Game1.random.Next(this.centerX * 2), (float)Game1.random.Next(this.centerY)), false, flip)
								{
									scale = 4f,
									motion = new Vector2((float)(flip ? -8 : 8), 8f),
									local = true
								});
							}
						}
						else if (Game1.random.NextDouble() < 0.0002)
						{
							startingPosition = new Vector2((float)Game1.uiViewport.Width, (float)Game1.random.Next(4, 256));
							TemporaryAnimatedSprite bird = new TemporaryAnimatedSprite("", new Rectangle(0, 0, 1, 1), 9999f, 1, 10000, startingPosition, false, false, 0.01f, 0f, Color.White * (0.25f + (float)Game1.random.NextDouble()), 4f, 0f, 0f, 0f, true);
							bird.motion = new Vector2(-0.25f, 0f);
							this.animations.Add(bird);
						}
						else if (Game1.random.NextDouble() < 5E-05)
						{
							startingPosition = new Vector2((float)Game1.uiViewport.Width, (float)(Game1.uiViewport.Height - 192));
							for (int j = 0; j < rows; j++)
							{
								TemporaryAnimatedSprite bird2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, (float)Game1.random.Next(60, 101), 4, 100, startingPosition + new Vector2((float)((j + 1) * Game1.random.Next(15, 18)), (float)((j + 1) * -20)), false, false, 0.01f, 0f, Color.Black, 4f, 0f, 0f, 0f, true);
								bird2.motion = new Vector2(-1f, 0f);
								this.animations.Add(bird2);
								bird2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, (float)Game1.random.Next(60, 101), 4, 100, startingPosition + new Vector2((float)((j + 1) * Game1.random.Next(15, 18)), (float)((j + 1) * 20)), false, false, 0.01f, 0f, Color.Black, 4f, 0f, 0f, 0f, true);
								bird2.motion = new Vector2(-1f, 0f);
								this.animations.Add(bird2);
							}
						}
						else if (Game1.random.NextDouble() < 1E-05)
						{
							sourceRect = new Rectangle(640, 784, 16, 16);
							TemporaryAnimatedSprite t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 75f, 4, 1000, startingPosition, false, false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, true);
							t.motion = new Vector2(-3f, 0f);
							t.yPeriodic = true;
							t.yPeriodicLoopTime = 1000f;
							t.yPeriodicRange = 8f;
							t.shakeIntensity = 0.5f;
							this.animations.Add(t);
						}
					}
					this.smokeTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.smokeTimer <= 0)
					{
						this.smokeTimer = 50;
						this.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(684, 1075, 1, 1), 1000f, 1, 1000, new Vector2(188f, (float)(Game1.uiViewport.Height - 128 + 20)), false, false)
						{
							color = (Game1.wasRainingYesterday ? Color.SlateGray : Color.White),
							scale = 4f,
							scaleChange = 0f,
							alphaFade = 0.0025f,
							motion = new Vector2(0f, (float)(-(float)Game1.random.Next(25, 75)) / 100f / 4f),
							acceleration = new Vector2(-0.001f, 0f)
						});
					}
				}
				if (this.moonShake > 0)
				{
					this.moonShake -= time.ElapsedGameTime.Milliseconds;
				}
				return;
			}
			if (Game1.PollForEndOfNewDaySync())
			{
				base.exitThisMenu(false);
				return;
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (Game1.wasRainingYesterday)
			{
				b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle?(new Rectangle(639, 858, 1, 184)), Game1.currentSeason.Equals("winter") ? Color.LightSlateGray : (Color.SlateGray * (1f - (float)introTimer / 3500f)));
				b.Draw(Game1.mouseCursors, new Rectangle(2556, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle?(new Rectangle(639, 858, 1, 184)), Game1.currentSeason.Equals("winter") ? Color.LightSlateGray : (Color.SlateGray * (1f - (float)introTimer / 3500f)));
				for (int x = -244; x < Game1.uiViewport.Width + 244; x += 244)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)x + weatherX / 2f % 244f, 32f), new Rectangle?(new Rectangle(643, 1142, 61, 53)), Color.DarkSlateGray * 1f * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
				b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(Game1.uiViewport.Height - 192)), new Rectangle?(new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 48)), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.25f) : new Color(30, 62, 50)) * (0.5f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(2556f, (float)(Game1.uiViewport.Height - 192)), new Rectangle?(new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 48)), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.25f) : new Color(30, 62, 50)) * (0.5f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(Game1.uiViewport.Height - 128)), new Rectangle?(new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 32)), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.5f) : new Color(30, 62, 50)) * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(2556f, (float)(Game1.uiViewport.Height - 128)), new Rectangle?(new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 32)), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.5f) : new Color(30, 62, 50)) * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(160f, (float)(Game1.uiViewport.Height - 128 + 16 + 8)), new Rectangle?(new Rectangle(653, 880, 10, 10)), Color.White * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				for (int x2 = -244; x2 < Game1.uiViewport.Width + 244; x2 += 244)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)x2 + weatherX % 244f, -32f), new Rectangle?(new Rectangle(643, 1142, 61, 53)), Color.SlateGray * 0.85f * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
				}
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in animations)
				{
					temporaryAnimatedSprite.draw(b, true, 0, 0, 1f);
				}
				for (int x3 = -244; x3 < Game1.uiViewport.Width + 244; x3 += 244)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)x3 + weatherX * 1.5f % 244f, -128f), new Rectangle?(new Rectangle(643, 1142, 61, 53)), Color.LightSlateGray * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
				}
			}
			else
			{
				b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle?(new Rectangle(639, 858, 1, 184)), Color.White * (1f - (float)introTimer / 3500f));
				b.Draw(Game1.mouseCursors, new Rectangle(2556, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle?(new Rectangle(639, 858, 1, 184)), Color.White * (1f - (float)introTimer / 3500f));
				b.Draw(Game1.mouseCursors, new Vector2(0f, 0f), new Rectangle?(new Rectangle(0, 1453, 639, 195)), Color.White * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(2556f, 0f), new Rectangle?(new Rectangle(0, 1453, 639, 195)), Color.White * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				if (Game1.dayOfMonth == 28)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(Game1.uiViewport.Width - 176), 4f) + ((moonShake > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle?(new Rectangle(642, 835, 43, 43)), Color.White * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					if (timesPokedMoon > 10)
					{
						b.Draw(Game1.mouseCursors, new Vector2((float)(Game1.uiViewport.Width - 136), 48f) + ((moonShake > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle?(new Rectangle(685, 844 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 4000.0 < 200.0 || (Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 8000.0 > 7600.0 && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 8000.0 < 7800.0)) ? 21 : 0), 19, 21)), Color.White * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					}
				}
				b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(Game1.uiViewport.Height - 192)), new Rectangle?(new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 48)), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.25f) : new Color(0, 20, 40)) * (0.65f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(2556f, (float)(Game1.uiViewport.Height - 192)), new Rectangle?(new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 48)), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.25f) : new Color(0, 20, 40)) * (0.65f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(Game1.uiViewport.Height - 128)), new Rectangle?(new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 32)), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.5f) : new Color(0, 32, 20)) * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(2556f, (float)(Game1.uiViewport.Height - 128)), new Rectangle?(new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 32)), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.5f) : new Color(0, 32, 20)) * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(160f, (float)(Game1.uiViewport.Height - 128 + 16 + 8)), new Rectangle?(new Rectangle(653, 880, 10, 10)), Color.White * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			if (!outro && !Game1.wasRainingYesterday)
			{
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite2 in animations)
				{
					temporaryAnimatedSprite2.draw(b, true, 0, 0, 1f);
				}
			}
			if (introTimer <= 0)
			{
				okButton.draw(b);
				if (ModEntry.Config.ShowQuitButton)
				{
					quitButton.draw(b);

				}
				okButton.draw(b);

				if(ModEntry.quotesAPI is not null && quote is not null)
                {

					int lineSpacing = Game1.dialogueFont.LineSpacing + ModEntry.Config.LineSpacing;

					for (int i = 0; i < quote.Length - 1; i++)
					{
						b.DrawString(Game1.dialogueFont, quote[i], new Vector2(Game1.viewport.Width / 2 - (ModEntry.Config.QuoteCharPerLine * 10), Game1.viewport.Height / 2 - ((quote.Length - 1) / 2) * lineSpacing + lineSpacing * i), ModEntry.Config.QuoteColor);
					}
					if (quote[quote.Length - 1] != null)
						b.DrawString(Game1.dialogueFont, ModEntry.Config.AuthorPrefix + quote[quote.Length - 1], new Vector2(Game1.viewport.Width / 2 + (ModEntry.Config.QuoteCharPerLine * 10) - (quote[quote.Length - 1].Length * 20 + ModEntry.Config.AuthorPrefix.Length * 20), Game1.viewport.Height / 2 - ((quote.Length - 1) / 2) * lineSpacing + lineSpacing * (quote.Length - 1)), ModEntry.Config.QuoteColor);
				}
			}
			if (outro)
			{
				b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle?(new Rectangle(639, 858, 1, 184)), Color.Black * (1f - (float)outroFadeTimer / 800f));
				SpriteText.drawStringWithScrollCenteredAt(b, newDayPlaque ? Utility.getDateString(0) : Utility.getYesterdaysDate(), Game1.uiViewport.Width / 2, dayPlaqueY, "", 1f, -1, 0, 0.88f, false);
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite3 in animations)
				{
					temporaryAnimatedSprite3.draw(b, true, 0, 0, 1f);
				}
				if (finalOutroTimer > 0 || _hasFinished)
				{
					b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle?(new Rectangle(0, 0, 1, 1)), Color.Black * (1f - (float)finalOutroTimer / 2000f));
				}
			}
			if (saveGameMenu != null)
			{
				saveGameMenu.draw(b);
			}
			if (!Game1.options.SnappyMenus || (introTimer <= 0 && !outro))
			{
				Game1.mouseCursorTransparency = 1f;
				base.drawMouse(b, false, -1);
			}
		}

		private int plusButtonWidth;

		private int itemSlotWidth;

		private int itemAndPlusButtonWidth;

		private int totalWidth;

		private int centerX;

		private int centerY;

		private int introTimer = 3500;

		private int outroFadeTimer;

		private int outroPauseBeforeDateChange;

		private int finalOutroTimer;

		private int smokeTimer;

		private int dayPlaqueY;

		private int moonShake = -1;

		private int timesPokedMoon;

		private float weatherX;

		private bool outro;

		private bool newDayPlaque;

		private bool savedYet;


		private SaveGameMenu saveGameMenu;
        private string[] quote;
    }
}