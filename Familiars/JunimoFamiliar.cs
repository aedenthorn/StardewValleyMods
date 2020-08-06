using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Policy;

namespace Familiars
{
    public class JunimoFamiliar : Familiar
	{
		public bool EventActor
		{
			get
			{
				return this.eventActor;
			}
			set
			{
				this.eventActor.Value = value;
			}
		}

		public JunimoFamiliar()
		{
			this.forceUpdateTimer = 9999;
		}

		public JunimoFamiliar(Vector2 position, long ownerId) : base("Junimo", position, new AnimatedSprite("Characters\\Junimo", 0, 16, 16))
		{
			this.friendly.Value = true;
			this.nextPosition.Value = this.GetBoundingBox();
			base.Breather = false;
			base.speed = 3;
			this.forceUpdateTimer = 9999;
			this.collidesWithOtherCharacters.Value = true;
			this.farmerPassesThrough = true;
			ignoreMovementAnimations = true;

			color.Value = FamiliarsUtils.GetJunimoColor();

		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(new INetSerializable[]
			{
				this.friendly,
				this.holdingStar,
				this.holdingBundle,
				this.stayPut,
				this.eventActor,
				this.motion,
				this.nextPosition,
				this.color,
				this.bundleColor,
			});
			base.NetFields.AddFields(new INetSerializable[]
			{
				this.setReturnToJunimoHutToFetchStarControllerEvent,
				this.setBringBundleBackToHutControllerEvent,
				this.setJunimoReachedHutToFetchStarControllerEvent,
				this.starDoneSpinningEvent,
				this.returnToJunimoHutToFetchFinalStarEvent
			});
			this.setReturnToJunimoHutToFetchStarControllerEvent.onEvent += this.setReturnToJunimoHutToFetchStarController;
			this.setBringBundleBackToHutControllerEvent.onEvent += this.setBringBundleBackToHutController;
			this.setJunimoReachedHutToFetchStarControllerEvent.onEvent += this.setJunimoReachedHutToFetchStarController;
			this.starDoneSpinningEvent.onEvent += this.performStartDoneSpinning;
			this.returnToJunimoHutToFetchFinalStarEvent.onEvent += this.returnToJunimoHutToFetchFinalStar;
			this.position.Field.AxisAlignedMovement = false;
		}
		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			ignoreMovementAnimations = true;
			base.MovePosition(time, viewport, currentLocation);
		}
		public override bool canPassThroughActionTiles()
		{
			return false;
		}

		public override bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			return true;
		}

		public override bool canTalk()
		{
			return false;
		}

		public void setMoving(int xSpeed, int ySpeed)
		{
			this.motion.X = (float)xSpeed;
			this.motion.Y = (float)ySpeed;
		}

		public void setMoving(Vector2 motion)
		{
			this.motion.Value = motion;
		}

		public override void Halt()
		{
			base.Halt();
			this.motion.Value = Vector2.Zero;
		}

		public void stayStill()
		{
			this.stayPut.Value = true;
			this.motion.Value = Vector2.Zero;
		}

		public void allowToMoveAgain()
		{
			this.stayPut.Value = false;
		}

		private void returnToJunimoHutToFetchFinalStar()
		{
			if (base.currentLocation == Game1.currentLocation)
			{
				Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.finalCutscene), 0.005f);
				Game1.freezeControls = true;
				Game1.flashAlpha = 1f;
			}
		}

		public void returnToJunimoHutToFetchStar(GameLocation location)
		{
			base.currentLocation = location;
			this.friendly.Value = true;
			if (((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areAllAreasComplete())
			{
				this.returnToJunimoHutToFetchFinalStarEvent.Fire();
				this.collidesWithOtherCharacters.Value = false;
				this.farmerPassesThrough = false;
				this.stayStill();
				this.faceDirection(0);
				GameLocation cc = Game1.getLocationFromName("CommunityCenter");
				if (!Game1.player.mailReceived.Contains("ccIsComplete"))
				{
					Game1.player.mailReceived.Add("ccIsComplete");
				}
				if (Game1.currentLocation.Equals(cc))
				{
					(cc as CommunityCenter).addStarToPlaque();
					return;
				}
			}
			else
			{
				DelayedAction.textAboveHeadAfterDelay((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\Characters:JunimoTextAboveHead1") : Game1.content.LoadString("Strings\\Characters:JunimoTextAboveHead2"), this, Game1.random.Next(3000, 6000));
				this.setReturnToJunimoHutToFetchStarControllerEvent.Fire();
				if (ModEntry.Config.JunimoSoundEffects)
					location.playSound("junimoMeep1", NetAudio.SoundContext.Default);
				this.collidesWithOtherCharacters.Value = false;
				this.farmerPassesThrough = false;
				this.holdingBundle.Value = true;
				base.speed = 3;
			}
		}

		private void setReturnToJunimoHutToFetchStarController()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			this.controller = new PathFindController(this, base.currentLocation, new Point(25, 10), 0, new PathFindController.endBehavior(this.junimoReachedHutToFetchStar));
		}

		private void finalCutscene()
		{
			this.collidesWithOtherCharacters.Value = false;
			this.farmerPassesThrough = false;
			(Game1.getLocationFromName("CommunityCenter") as CommunityCenter).prepareForJunimoDance();
			Game1.player.Position = new Vector2(29f, 11f) * 64f;
			Game1.player.completelyStopAnimatingOrDoingAction();
			Game1.player.faceDirection(3);
			Game1.UpdateViewPort(true, new Point(Game1.player.getStandingX(), Game1.player.getStandingY()));
			Game1.viewport.X = Game1.player.getStandingX() - Game1.viewport.Width / 2;
			Game1.viewport.Y = Game1.player.getStandingY() - Game1.viewport.Height / 2;
			Game1.viewportTarget = Vector2.Zero;
			Game1.viewportCenter = new Point(Game1.player.getStandingX(), Game1.player.getStandingY());
			Game1.moveViewportTo(new Vector2(32.5f, 6f) * 64f, 2f, 999999, null, null);
			Game1.globalFadeToClear(new Game1.afterFadeFunction(this.goodbyeDance), 0.005f);
			Game1.pauseTime = 1000f;
			Game1.freezeControls = true;
		}

		public void bringBundleBackToHut(Color bundleColor, GameLocation location)
		{
			base.currentLocation = location;
			if (!this.holdingBundle)
			{
				base.Position = Utility.getRandomAdjacentOpenTile(Game1.player.getTileLocation(), location) * 64f;
				int iter = 0;
				while (location.isCollidingPosition(this.GetBoundingBox(), Game1.viewport, this) && iter < 5)
				{
					base.Position = Utility.getRandomAdjacentOpenTile(Game1.player.getTileLocation(), location) * 64f;
					iter++;
				}
				if (iter >= 5)
				{
					return;
				}
				if (Game1.random.NextDouble() < 0.25)
				{
					DelayedAction.textAboveHeadAfterDelay((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\Characters:JunimoThankYou1") : Game1.content.LoadString("Strings\\Characters:JunimoThankYou2"), this, Game1.random.Next(3000, 6000));
				}
				this.bundleColor.Value = bundleColor;
				this.setBringBundleBackToHutControllerEvent.Fire();
				this.collidesWithOtherCharacters.Value = false;
				this.farmerPassesThrough = false;
				this.holdingBundle.Value = true;
				base.speed = 3;
			}
		}

		private void setBringBundleBackToHutController()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			this.controller = new PathFindController(this, base.currentLocation, new Point(25, 10), 0, new PathFindController.endBehavior(this.junimoReachedHutToReturnBundle));
		}

		private void junimoReachedHutToReturnBundle(Character c, GameLocation l)
		{
			base.currentLocation = l;
			this.holdingBundle.Value = false;
			this.collidesWithOtherCharacters.Value = true;
			this.farmerPassesThrough = true;
			l.playSound("Ship", NetAudio.SoundContext.Default);
		}

		private void junimoReachedHutToFetchStar(Character c, GameLocation l)
		{
			base.currentLocation = l;
			this.holdingStar.Value = true;
			this.holdingBundle.Value = false;
			base.speed = 3;
			this.collidesWithOtherCharacters.Value = false;
			this.farmerPassesThrough = false;
			this.setJunimoReachedHutToFetchStarControllerEvent.Fire();
			l.playSound("dwop", NetAudio.SoundContext.Default);
			this.farmerPassesThrough = false;
		}

		private void setJunimoReachedHutToFetchStarController()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			this.controller = new PathFindController(this, base.currentLocation, new Point(32, 9), 2, new PathFindController.endBehavior(this.placeStar));
		}

		private void placeStar(Character c, GameLocation l)
		{
			base.currentLocation = l;
			this.collidesWithOtherCharacters.Value = false;
			this.farmerPassesThrough = true;
			this.holdingStar.Value = false;
			l.playSound("tinyWhip", NetAudio.SoundContext.Default);
			this.friendly.Value = true;
			base.speed = 3;
			ModEntry.mp.broadcastSprites(l, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(this.Sprite.textureName, new Rectangle(0, 109, 16, 19), 40f, 8, 10, base.Position + new Vector2(0f, -64f), false, false, 1f, 0f, Color.White, 4f * this.scale, 0f, 0f, 0f, false)
				{
					endFunction = new TemporaryAnimatedSprite.endBehavior(this.starDoneSpinning),
					motion = new Vector2(0.22f, -2f),
					acceleration = new Vector2(0f, 0.01f),
					id = 777f
				}
			});
		}

		private void goodbyeDance()
		{
			Game1.player.faceDirection(3);
			(Game1.getLocationFromName("CommunityCenter") as CommunityCenter).junimoGoodbyeDance();
		}

		private void starDoneSpinning(int extraInfo)
		{
			this.starDoneSpinningEvent.Fire();
			(base.currentLocation as CommunityCenter).addStarToPlaque();
		}

		private void performStartDoneSpinning()
		{
			if (Game1.currentLocation is CommunityCenter)
			{
				Game1.playSound("yoba");
				Game1.flashAlpha = 1f;
				Game1.playSound("yoba");
			}
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			if (this.textAboveHeadTimer > 0 && this.textAboveHead != null)
			{
				Vector2 local = Game1.GlobalToLocal(new Vector2((float)base.getStandingX(), (float)base.getStandingY() - 128f + (float)this.yJumpOffset));
				if (this.textAboveHeadStyle == 0)
				{
					local += new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
				}
				SpriteText.drawStringWithScrollCenteredAt(b, this.textAboveHead, (int)local.X, (int)local.Y, "", this.textAboveHeadAlpha, this.textAboveHeadColor, 1, (float)(base.getTileY() * 64) / 10000f + 0.001f + (float)base.getTileX() / 10000f, true);
			}
		}

		protected override void updateSlaveAnimation(GameTime time)
		{
			if (this.holdingStar || this.holdingBundle)
			{
				this.Sprite.Animate(time, 44, 4, 200f);
				return;
			}
			if (!this.position.IsInterpolating())
			{
				this.Sprite.Animate(time, 8, 4, 100f);
				return;
			}
			if (base.FacingDirection == 1)
			{
				this.flip = false;
				this.Sprite.Animate(time, 16, 8, 50f);
				return;
			}
			if (base.FacingDirection == 3)
			{
				this.Sprite.Animate(time, 16, 8, 50f);
				this.flip = true;
				return;
			}
			if (base.FacingDirection == 0)
			{
				this.Sprite.Animate(time, 32, 8, 50f);
				return;
			}
			this.Sprite.Animate(time, 0, 8, 50f);
		}

        public override void behaviorAtGameTick(GameTime time)
        {
        }

        public override void updateMovement(GameLocation location, GameTime time)
        {
        }

        protected override void updateAnimation(GameTime time)
        {
        }
        protected override void updateMonsterSlaveAnimation(GameTime time)
        {
        }

        public override void update(GameTime time, GameLocation location)
		{
			base.currentLocation = location;
			base.update(time, location);
			this.forceUpdateTimer = 99999;

			if (this.eventActor)
			{
				return;
			}
			soundTimer--;
			this.farmerCloseCheckTimer -= time.ElapsedGameTime.Milliseconds;

			if (followingOwner)
			{
				this.farmerCloseCheckTimer = 100;
				if (this.holdingStar.Value)
				{
					this.setJunimoReachedHutToFetchStarController();
				}
				else
				{
					Farmer f = Game1.getFarmer(ownerId);
					if (f != null && f.currentLocation == currentLocation)
					{
						if (Vector2.Distance(base.Position, f.Position) > (float)(base.speed * 4))
						{
							if (this.motion.Equals(Vector2.Zero) && soundTimer <= 0)
							{
								this.jump((int)(5 + Game1.random.Next(1, 4) * scale));
								location.localSound("junimoMeep1");
								soundTimer = 400;
							}
							if (Game1.random.NextDouble() < 0.007)
							{
								this.jumpWithoutSound((int)(5 + Game1.random.Next(1, 4) * scale));
							}
							this.setMoving(Utility.getVelocityTowardPlayer(new Point((int)base.Position.X, (int)base.Position.Y), (float)base.speed, f));
						}
						else
						{
							this.motion.Value = Vector2.Zero;
						}
					}
					else
					{
						this.motion.Value = Vector2.Zero;
					}
				}
			}
			if (!base.IsInvisible && this.controller == null)
			{
				Farmer f = Game1.getFarmer(ownerId);
				this.nextPosition.Value = this.GetBoundingBox();
				this.nextPosition.X += (int)this.motion.X;
				if (!location.isCollidingPosition(this.nextPosition, Game1.viewport, this))
				{
					this.position.X += (float)((int)this.motion.X);
				}
				this.nextPosition.X -= (int)this.motion.X;
				this.nextPosition.Y += (int)this.motion.Y;
				if (!location.isCollidingPosition(this.nextPosition, Game1.viewport, this))
				{
					this.position.Y += (float)((int)this.motion.Y);
				}

				lastHeal -= time.ElapsedGameTime.Milliseconds;

				if (Vector2.Distance(f.getTileLocation(), getTileLocation()) < 3 && lastHeal < 0 && Game1.random.NextDouble() < HealChance())
				{
					bool heal = Game1.random.NextDouble() < 0.5;
					location.temporarySprites.Add(new TemporaryAnimatedSprite((Game1.random.NextDouble() < 0.5) ? 10 : 11, base.Position, (heal ? Color.Red : Color.LawnGreen), 8, false, 100f, 0, -1, -1f, -1, 0)
					{
						motion = this.motion.Value / 4f,
						alphaFade = 0.01f,
						layerDepth = 0.8f,
						scale = 0.75f,
						alpha = 0.75f
					});
					AddExp(1);
					if (heal && f.health < f.maxHealth)
                    {
						f.health += HealAmount();
					}
					else if (!heal && f.stamina < f.MaxStamina)
                    {
						f.stamina += HealAmount();
                    }
					ModEntry.SMonitor.Log($"Junimo Heal {HealAmount()}, exp {exp}");
					lastHeal = HealInterval();
				}
			}
			if (this.controller == null && this.motion.Equals(Vector2.Zero))
			{
				this.Sprite.Animate(time, 8, 4, 100f);
				return;
			}
			if (this.holdingStar || this.holdingBundle)
			{
				this.Sprite.Animate(time, 44, 4, 200f);
				return;
			}
			if (this.moveRight || (Math.Abs(this.motion.X) > Math.Abs(this.motion.Y) && this.motion.X > 0f))
			{
				this.flip = false;
				this.Sprite.Animate(time, 16, 8, 50f);
				return;
			}
			if (this.moveLeft || (Math.Abs(this.motion.X) > Math.Abs(this.motion.Y) && this.motion.X < 0f))
			{
				this.Sprite.Animate(time, 16, 8, 50f);
				this.flip = true;
				return;
			}
			if (this.moveUp || (Math.Abs(this.motion.Y) > Math.Abs(this.motion.X) && this.motion.Y < 0f))
			{
				this.Sprite.Animate(time, 32, 8, 50f);
				return;
			}
			this.Sprite.Animate(time, 0, 8, 50f);
		}
		private void AddExp(int v)
		{
			exp.Value += v;
		}
		private int HealAmount()
        {
			return (int)(Math.Ceiling(Math.Sqrt(exp/10f)) * ModEntry.Config.JunimoHealAmountMult);
        }

        private double HealChance()
        {
			return (0.001 + Math.Sqrt(exp) * 0.001) * ModEntry.Config.JunimoHealChanceMult;
        }

        private int HealInterval()
        {
			return (int)Math.Round(Math.Max(1000, 10000 - Math.Sqrt(exp)) * ModEntry.Config.JunimoHealIntervalMult);
        }

        public override void draw(SpriteBatch b, float alpha = 1f)
		{
			if (!base.IsInvisible)
			{
				this.Sprite.UpdateSourceRect();
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(this.Sprite.SpriteWidth * 4 / 2), (float)this.Sprite.SpriteHeight * 3f / 4f * 4f / (float)Math.Pow((double)(this.Sprite.SpriteHeight / 16), 2.0) + (float)this.yJumpOffset - 8f) + ((this.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero) + new Vector2(0,-24), new Rectangle?(this.Sprite.SourceRect), this.color.Value * alpha, this.rotation, new Vector2((float)(this.Sprite.SpriteWidth * 4 / 2), (float)(this.Sprite.SpriteHeight * 4) * 3f / 4f) / 4f, Math.Max(0.2f, this.scale) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.991f : ((float)base.getStandingY() / 10000f)));

				if (this.holdingStar)
				{
					b.Draw(this.Sprite.Texture, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2(8f, -64f * this.scale + 4f + (float)this.yJumpOffset)), new Rectangle?(new Rectangle(0, 109, 16, 19)), Color.White * alpha, 0f, Vector2.Zero, 4f * this.scale, SpriteEffects.None, base.Position.Y / 10000f + 0.0001f);
					return;
				}
				if (this.holdingBundle)
				{
					b.Draw(this.Sprite.Texture, Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2(8f, -64f * this.scale + 20f + (float)this.yJumpOffset)), new Rectangle?(new Rectangle(0, 96, 16, 13)), this.bundleColor.Value * alpha, 0f, Vector2.Zero, 4f * this.scale, SpriteEffects.None, base.Position.Y / 10000f + 0.0001f);
				}
			}
		}

		public readonly NetBool friendly = new NetBool();

		public readonly NetBool holdingStar = new NetBool();

		public readonly NetBool holdingBundle = new NetBool();

		public readonly NetBool stayPut = new NetBool();

		public new readonly NetBool eventActor = new NetBool();

		private readonly NetVector2 motion = new NetVector2(Vector2.Zero);

		private new readonly NetRectangle nextPosition = new NetRectangle();

		private readonly NetColor bundleColor = new NetColor();

		private readonly NetEvent0 setReturnToJunimoHutToFetchStarControllerEvent = new NetEvent0(false);

		private readonly NetEvent0 setBringBundleBackToHutControllerEvent = new NetEvent0(false);

		private readonly NetEvent0 setJunimoReachedHutToFetchStarControllerEvent = new NetEvent0(false);

		private readonly NetEvent0 starDoneSpinningEvent = new NetEvent0(false);

		private readonly NetEvent0 returnToJunimoHutToFetchFinalStarEvent = new NetEvent0(false);

		private int farmerCloseCheckTimer = 100;

		private static int soundTimer;
        private int lastHeal;
    }
}
