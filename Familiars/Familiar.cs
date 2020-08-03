using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Network;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Familiars
{
	public class Familiar : NPC
	{
		[XmlIgnore]
		public Farmer Player
		{
			get
			{
				return this.findPlayer();
			}
		}

		[XmlIgnore]
		public int DamageToFarmer
		{
			get
			{
				return this.damageToFarmer;
			}
			set
			{
				this.damageToFarmer.Value = value;
			}
		}

		[XmlIgnore]
		public int Health
		{
			get
			{
				return this.health;
			}
			set
			{
				this.health.Value = value;
			}
		}

		[XmlIgnore]
		public int MaxHealth
		{
			get
			{
				return this.maxHealth;
			}
			set
			{
				this.maxHealth.Value = value;
			}
		}

		[XmlIgnore]
		public int ExperienceGained
		{
			get
			{
				return this.experienceGained;
			}
			set
			{
				this.experienceGained.Value = value;
			}
		}

		[XmlIgnore]
		public int Slipperiness
		{
			get
			{
				return this.slipperiness;
			}
			set
			{
				this.slipperiness.Value = value;
			}
		}

		[XmlIgnore]
		public bool focusedOnFarmers
		{
			get
			{
				return this.netFocusedOnFarmers;
			}
			set
			{
				this.netFocusedOnFarmers.Value = value;
			}
		}

		[XmlIgnore]
		public bool wildernessFarmMonster
		{
			get
			{
				return this.netWildernessFarmMonster;
			}
			set
			{
				this.netWildernessFarmMonster.Value = value;
			}
		}

		public Familiar()
		{
		}

		public Familiar(string name, Vector2 position, AnimatedSprite animatedSprite) : this(name, position, 2, animatedSprite)
		{
			base.Breather = false;
			SetScale();
			farmerPassesThrough = true;
			moveTowardPlayerThreshold.Value = 500;
			collidesWithOtherCharacters.Value = false;
			lastPosition = position;
		}

		public FamiliarData SaveData(GameLocation gameLocation)
        {
			return new FamiliarData()
			{
				daysOld = this.daysOld,
				followingOwner = this.followingOwner,
				exp = this.exp,
				ownerId = this.ownerId,
				mainColor = this.mainColor,
				redColor = this.redColor,
				greenColor = this.greenColor,
				blueColor = this.blueColor,
				currentLocation = gameLocation.Name,
				position = this.position,
				baseFrame = this.baseFrame
			};
        }

        protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(new INetSerializable[]
			{
				this.damageToFarmer,
				this.health,
				this.maxHealth,
				this.coinsToDrop,
				this.durationOfRandomMovements,
				this.resilience,
				this.slipperiness,
				this.experienceGained,
				this.jitteriness,
				this.missChance,
				this.isGlider,
				this.mineMonster,
				this.hasSpecialItem,
				this.objectsToDrop,
				this.defaultAnimationInterval,
				this.netFocusedOnFarmers,
				this.netWildernessFarmMonster,
				this.deathAnimEvent,
				this.daysOld,
				this.exp,
				this.trajectoryEvent
			});
			this.position.Field.AxisAlignedMovement = false;
			this.deathAnimEvent.onEvent += this.localDeathAnimation;
			this.trajectoryEvent.onEvent += this.doSetTrajectory;
			this.trajectoryEvent.InterpolationWait = false;
		}
        public override void dayUpdate(int dayOfMonth)
        {
            base.dayUpdate(dayOfMonth);
			daysOld.Value = daysOld.Value + 1;
			SetScale();
		}

        public void SetScale()
        {
			Scale = (float)Math.Min(2, 0.5f + (daysOld * 0.01));
		}

		protected override Farmer findPlayer()
		{
			if (base.currentLocation == null)
			{
				return Game1.player;
			}
			Farmer bestFarmer = Game1.player;
			double bestPriority = double.MaxValue;
			foreach (Farmer f in base.currentLocation.farmers)
			{
				if (!f.hidden)
				{
					double priority = this.findPlayerPriority(f);
					if (priority < bestPriority)
					{
						bestPriority = priority;
						bestFarmer = f;
					}
				}
			}
			return bestFarmer;
		}

		protected virtual double findPlayerPriority(Farmer f)
		{
			return (double)(f.Position - base.Position).LengthSquared();
		}

		public virtual void onDealContactDamage(Farmer who)
		{
		}

		public virtual List<Item> getExtraDropItems()
		{
			return new List<Item>();
		}

		public override bool withinPlayerThreshold()
		{
			return this.focusedOnFarmers || this.withinPlayerThreshold(this.moveTowardPlayerThreshold);
		}

		public override bool IsMonster
		{
			get
			{
				return false;
			}
		}
		public bool isVillager()
		{
			return false;
		}
		public Familiar(string name, Vector2 position, int facingDir, AnimatedSprite animatedSprite) : base(animatedSprite, position, facingDir, name, null)
		{
			
			this.parseMonsterInfo(name);
			base.Breather = false;
		}

		public virtual void drawAboveAllLayers(SpriteBatch b)
		{
		}

		public virtual void drawAboveFrontLayer(SpriteBatch b)
		{
		}

		public override void draw(SpriteBatch b)
		{
			if (!this.isGlider)
			{
				base.draw(b);
			}
		}

		public bool isInvincible()
		{
			return this.invincibleCountdown > 0;
		}

		public void setInvincibleCountdown(int time)
		{
			this.invincibleCountdown = time;
			base.startGlowing(new Color(255, 0, 0), false, 0.25f);
			this.glowingTransparency = 1f;
		}

		protected int maxTimesReachedMineBottom()
		{
			int result = 0;
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				result = Math.Max(result, farmer.timesReachedMineBottom);
			}
			return result;
		}

		protected void parseMonsterInfo(string name)
		{
			string[] monsterInfo = Game1.content.Load<Dictionary<string, string>>("Data\\Monsters")[name].Split(new char[]
			{
				'/'
			});
			this.Health = Convert.ToInt32(monsterInfo[0]);
			this.MaxHealth = this.Health;
			this.DamageToFarmer = Convert.ToInt32(monsterInfo[1]);
			this.coinsToDrop.Value = Game1.random.Next(Convert.ToInt32(monsterInfo[2]), Convert.ToInt32(monsterInfo[3]) + 1);
			this.isGlider.Value = Convert.ToBoolean(monsterInfo[4]);
			this.durationOfRandomMovements.Value = Convert.ToInt32(monsterInfo[5]);
			string[] objectsSplit = monsterInfo[6].Split(new char[]
			{
				' '
			});
			this.objectsToDrop.Clear();
			for (int i = 0; i < objectsSplit.Length; i += 2)
			{
				if (Game1.random.NextDouble() < Convert.ToDouble(objectsSplit[i + 1]))
				{
					this.objectsToDrop.Add(Convert.ToInt32(objectsSplit[i]));
				}
			}
			this.resilience.Value = Convert.ToInt32(monsterInfo[7]);
			this.jitteriness.Value = Convert.ToDouble(monsterInfo[8]);
			base.willDestroyObjectsUnderfoot = false;
			base.moveTowardPlayer(Convert.ToInt32(monsterInfo[9]));
			base.speed = Convert.ToInt32(monsterInfo[10]);
			this.missChance.Value = Convert.ToDouble(monsterInfo[11]);
			this.mineMonster.Value = Convert.ToBoolean(monsterInfo[12]);
			if (this.maxTimesReachedMineBottom() >= 1 && this.mineMonster)
			{
				this.resilience.Value += this.resilience.Value / 2;
				this.missChance.Value *= 2.0;
				this.Health += Game1.random.Next(0, this.Health);
				this.DamageToFarmer += Game1.random.Next(0, this.DamageToFarmer / 2);
				this.coinsToDrop.Value += Game1.random.Next(0, this.coinsToDrop + 1);
				if (Game1.random.NextDouble() < 0.001)
				{
					this.objectsToDrop.Add((Game1.random.NextDouble() < 0.5) ? 72 : 74);
				}
			}
			try
			{
				this.ExperienceGained = Convert.ToInt32(monsterInfo[13]);
			}
			catch (Exception)
			{
				this.ExperienceGained = 1;
			}
			if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
			{
				base.displayName = monsterInfo[monsterInfo.Length - 1];
			}
		}

		public override void reloadSprite()
		{
			this.Sprite = new AnimatedSprite((name == "Junimo" ? "Characters\\" : "Characters\\Monsters\\") + base.Name, 0, 16, 16);
		}

		public virtual void shedChunks(int number)
		{
			this.shedChunks(number, 0.75f);
		}

		public virtual void shedChunks(int number, float scale)
		{
			if (this.Sprite.Texture.Height > this.Sprite.getHeight() * 4)
			{
				Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName, new Microsoft.Xna.Framework.Rectangle(0, this.Sprite.getHeight() * 4 + 16, 16, 16), 8, this.GetBoundingBox().Center.X, this.GetBoundingBox().Center.Y, number, (int)base.getTileLocation().Y, Color.White, 4f * scale);
			}
		}

		public override Rectangle GetBoundingBox()
		{
			Rectangle baseRect = new Rectangle((int)base.Position.X, (int)base.Position.Y, Sprite.SpriteWidth, Sprite.SpriteHeight);
			Rectangle resultRect = new Rectangle((int)baseRect.Center.X - (int)(Sprite.SpriteWidth * scale) / 2, (int)(baseRect.Center.Y - (int)(Sprite.SpriteHeight * scale) / 2), (int)(Sprite.SpriteWidth * scale), (int)(Sprite.SpriteHeight * scale));
			return resultRect;
		}

		public void deathAnimation()
		{
			this.sharedDeathAnimation();
			this.deathAnimEvent.Fire();
		}

		protected virtual void sharedDeathAnimation()
		{
			this.shedChunks(Game1.random.Next(4, 9), 0.75f);
		}

		protected virtual void localDeathAnimation()
		{
		}


		public virtual int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			return this.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, "hitEnemy");
		}

		public int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, string hitSound)
		{
			int actualDamage = Math.Max(1, damage - this.resilience);
			this.slideAnimationTimer = 0;
			if (Game1.random.NextDouble() < this.missChance - this.missChance * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				this.Health -= actualDamage;
				base.currentLocation.playSound(hitSound, NetAudio.SoundContext.Default);
				base.setTrajectory(xTrajectory / 3, yTrajectory / 3);
				if (this.Health <= 0)
				{
					this.deathAnimation();
				}
			}
			return actualDamage;
		}

		public override void setTrajectory(Vector2 trajectory)
		{
			this.trajectoryEvent.Fire(trajectory);
		}

		private void doSetTrajectory(Vector2 trajectory)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (Math.Abs(trajectory.X) > Math.Abs(this.xVelocity))
			{
				this.xVelocity = trajectory.X;
			}
			if (Math.Abs(trajectory.Y) > Math.Abs(this.yVelocity))
			{
				this.yVelocity = trajectory.Y;
			}
		}

		public virtual void behaviorAtGameTick(GameTime time)
		{
			if (this.timeBeforeAIMovementAgain > 0f)
			{
				this.timeBeforeAIMovementAgain -= (float)time.ElapsedGameTime.Milliseconds;
			}
			if (this.Player.isRafting && this.withinPlayerThreshold(4))
			{
				if (Math.Abs(this.Player.GetBoundingBox().Center.Y - this.GetBoundingBox().Center.Y) > 192)
				{
					if (this.Player.GetBoundingBox().Center.X - this.GetBoundingBox().Center.X > 0)
					{
						this.SetMovingLeft(true);
					}
					else
					{
						this.SetMovingRight(true);
					}
				}
				else if (this.Player.GetBoundingBox().Center.Y - this.GetBoundingBox().Center.Y > 0)
				{
					this.SetMovingUp(true);
				}
				else
				{
					this.SetMovingDown(true);
				}
				this.MovePosition(time, Game1.viewport, base.currentLocation);
			}
		}

		public virtual bool passThroughCharacters()
		{
			return false;
		}

		public override bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			return !followingOwner;
		}

		public override void update(GameTime time, GameLocation location)
		{
			if (followingOwner || location.getTileIndexAt(getTileLocationPoint(), "Back") != -1)
				lastPosition = position;
			else
				position.Value = lastPosition;

			if (followingOwner && Vector2.Distance(position, Game1.getFarmer(ownerId).position) > ModEntry.Config.MaxFamiliarDistance)
			{
				position.Value = Game1.getFarmer(ownerId).position;
			}

			if (this is JunimoFamiliar)
            {
				base.update(time, location);
				return;
            }

			this.trajectoryEvent.Poll();
			this.deathAnimEvent.Poll();
			this.position.UpdateExtrapolation((float)(base.speed + base.addedSpeed));
			if (this.invincibleCountdown > 0)
			{
				this.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (this.invincibleCountdown <= 0)
				{
					base.stopGlowing();
				}
			}
			if (location.farmers.Count == 0)
			{
				return;
			}
			base.update(time, location);
			
			if (Game1.IsMasterGame)
			{
				this.behaviorAtGameTick(time);
			}
			
			if(!(this is ButterflyFamiliar))
				this.updateAnimation(time);

			if (this.controller != null && this.withinPlayerThreshold(3))
			{
				this.controller = null;
			}
		}

		protected void resetAnimationSpeed()
		{
			if (!this.ignoreMovementAnimations)
			{
				this.Sprite.interval = (float)this.defaultAnimationInterval - (float)(base.speed + base.addedSpeed - 2) * 20f;
			}
		}

		protected virtual void updateAnimation(GameTime time)
		{
			if (!Game1.IsMasterGame)
			{
				this.updateMonsterSlaveAnimation(time);
			}
			this.resetAnimationSpeed();
		}

		protected override void updateSlaveAnimation(GameTime time)
		{
		}

		protected virtual void updateMonsterSlaveAnimation(GameTime time)
		{
			this.Sprite.animateOnce(time);
		}

		private bool doHorizontalMovement(GameLocation location)
		{
			bool wasAbleToMoveHorizontally = false;
			if (base.Position.X > this.Player.Position.X + 8f || (this.skipHorizontal > 0 && this.Player.getStandingX() < base.getStandingX() - 8))
			{
				base.SetMovingOnlyLeft();
				if (!location.isCollidingPosition(this.nextPosition(3), Game1.viewport, false, this.DamageToFarmer, this.isGlider, this))
				{
					this.MovePosition(Game1.currentGameTime, Game1.viewport, location);
					wasAbleToMoveHorizontally = true;
				}
				else
				{
					this.faceDirection(3);
					if (this.durationOfRandomMovements > 0 && Game1.random.NextDouble() < this.jitteriness)
					{
						if (Game1.random.NextDouble() < 0.5)
						{
							base.tryToMoveInDirection(2, false, this.DamageToFarmer, this.isGlider);
						}
						else
						{
							base.tryToMoveInDirection(0, false, this.DamageToFarmer, this.isGlider);
						}
						this.timeBeforeAIMovementAgain = (float)this.durationOfRandomMovements;
					}
				}
			}
			else if (base.Position.X < this.Player.Position.X - 8f)
			{
				base.SetMovingOnlyRight();
				if (!location.isCollidingPosition(this.nextPosition(1), Game1.viewport, false, this.DamageToFarmer, this.isGlider, this))
				{
					this.MovePosition(Game1.currentGameTime, Game1.viewport, location);
					wasAbleToMoveHorizontally = true;
				}
				else
				{
					this.faceDirection(1);
					if (this.durationOfRandomMovements > 0 && Game1.random.NextDouble() < this.jitteriness)
					{
						if (Game1.random.NextDouble() < 0.5)
						{
							base.tryToMoveInDirection(2, false, this.DamageToFarmer, this.isGlider);
						}
						else
						{
							base.tryToMoveInDirection(0, false, this.DamageToFarmer, this.isGlider);
						}
						this.timeBeforeAIMovementAgain = (float)this.durationOfRandomMovements;
					}
				}
			}
			else
			{
				base.faceGeneralDirection(this.Player.getStandingPosition(), 0, false);
				base.setMovingInFacingDirection();
				this.skipHorizontal = 500;
			}
			return wasAbleToMoveHorizontally;
		}

		private void checkHorizontalMovement(ref bool success, ref bool setMoving, ref bool scootSuccess, Farmer who, GameLocation location)
		{
			if (who.Position.X > base.Position.X + 16f)
			{
				base.SetMovingOnlyRight();
				setMoving = true;
				if (!location.isCollidingPosition(this.nextPosition(1), Game1.viewport, false, this.DamageToFarmer, this.isGlider, this))
				{
					success = true;
				}
				else
				{
					this.MovePosition(Game1.currentGameTime, Game1.viewport, location);
					if (!base.Position.Equals(this.lastPosition))
					{
						scootSuccess = true;
					}
				}
			}
			if (!success && who.Position.X < base.Position.X - 16f)
			{
				base.SetMovingOnlyLeft();
				setMoving = true;
				if (!location.isCollidingPosition(this.nextPosition(3), Game1.viewport, false, this.DamageToFarmer, this.isGlider, this))
				{
					success = true;
					return;
				}
				this.MovePosition(Game1.currentGameTime, Game1.viewport, location);
				if (!base.Position.Equals(this.lastPosition))
				{
					scootSuccess = true;
				}
			}
		}

		private void checkVerticalMovement(ref bool success, ref bool setMoving, ref bool scootSuccess, Farmer who, GameLocation location)
		{
			if (!success && who.Position.Y < base.Position.Y - 16f)
			{
				base.SetMovingOnlyUp();
				setMoving = true;
				if (!location.isCollidingPosition(this.nextPosition(0), Game1.viewport, false, this.DamageToFarmer, this.isGlider, this))
				{
					success = true;
				}
				else
				{
					this.MovePosition(Game1.currentGameTime, Game1.viewport, location);
					if (!base.Position.Equals(this.lastPosition))
					{
						scootSuccess = true;
					}
				}
			}
			if (!success && who.Position.Y > base.Position.Y + 16f)
			{
				base.SetMovingOnlyDown();
				setMoving = true;
				if (!location.isCollidingPosition(this.nextPosition(2), Game1.viewport, false, this.DamageToFarmer, this.isGlider, this))
				{
					success = true;
					return;
				}
				this.MovePosition(Game1.currentGameTime, Game1.viewport, location);
				if (!base.Position.Equals(this.lastPosition))
				{
					scootSuccess = true;
				}
			}
		}

		public override void updateMovement(GameLocation location, GameTime time)
		{
			if (base.IsWalkingTowardPlayer)
			{
				if ((this.moveTowardPlayerThreshold == -1 || this.withinPlayerThreshold()) && this.timeBeforeAIMovementAgain <= 0f && !this.isGlider && location.map.GetLayer("Back").Tiles[(int)this.Player.getTileLocation().X, (int)this.Player.getTileLocation().Y] != null && !location.map.GetLayer("Back").Tiles[(int)this.Player.getTileLocation().X, (int)this.Player.getTileLocation().Y].Properties.ContainsKey("NPCBarrier"))
				{
					if (this.skipHorizontal <= 0)
					{
						if (this.lastPosition.Equals(base.Position) && Game1.random.NextDouble() < 0.001)
						{
							switch (base.FacingDirection)
							{
								case 0:
								case 2:
									if (Game1.random.NextDouble() < 0.5)
									{
										base.SetMovingOnlyRight();
									}
									else
									{
										base.SetMovingOnlyLeft();
									}
									break;
								case 1:
								case 3:
									if (Game1.random.NextDouble() < 0.5)
									{
										base.SetMovingOnlyUp();
									}
									else
									{
										base.SetMovingOnlyDown();
									}
									break;
							}
							this.skipHorizontal = 700;
							return;
						}
						bool success = false;
						bool setMoving = false;
						bool scootSuccess = false;
						if (this.lastPosition.X == base.Position.X)
						{
							this.checkHorizontalMovement(ref success, ref setMoving, ref scootSuccess, this.Player, location);
							this.checkVerticalMovement(ref success, ref setMoving, ref scootSuccess, this.Player, location);
						}
						else
						{
							this.checkVerticalMovement(ref success, ref setMoving, ref scootSuccess, this.Player, location);
							this.checkHorizontalMovement(ref success, ref setMoving, ref scootSuccess, this.Player, location);
						}
						if (!success && !setMoving)
						{
							this.Halt();
							base.faceGeneralDirection(this.Player.getStandingPosition(), 0, false);
						}
						if (success)
						{
							this.skipHorizontal = 500;
						}
						if (scootSuccess)
						{
							return;
						}
					}
					else
					{
						this.skipHorizontal -= time.ElapsedGameTime.Milliseconds;
					}
				}
			}
			else
			{
				this.defaultMovementBehavior(time);
			}
			this.MovePosition(time, Game1.viewport, location);
			if (base.Position.Equals(this.lastPosition) && base.IsWalkingTowardPlayer && this.withinPlayerThreshold())
			{
				this.noMovementProgressNearPlayerBehavior();
			}
		}

		public virtual void noMovementProgressNearPlayerBehavior()
		{
			this.Halt();
			base.faceGeneralDirection(this.Player.getStandingPosition(), 0, false);
		}

		public virtual void defaultMovementBehavior(GameTime time)
		{
			if (Game1.random.NextDouble() < this.jitteriness * 1.8 && this.skipHorizontal <= 0)
			{
				switch (Game1.random.Next(6))
				{
					case 0:
						base.SetMovingOnlyUp();
						return;
					case 1:
						base.SetMovingOnlyRight();
						return;
					case 2:
						base.SetMovingOnlyDown();
						return;
					case 3:
						base.SetMovingOnlyLeft();
						return;
					default:
						this.Halt();
						break;
				}
			}
		}

		public override void Halt()
		{
			int old_speed = base.speed;
			base.Halt();
			base.speed = old_speed;
		}

		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			this.lastPosition = base.Position;
			if (this.xVelocity != 0f || this.yVelocity != 0f)
			{
				if (double.IsNaN((double)this.xVelocity) || double.IsNaN((double)this.yVelocity))
				{
					this.xVelocity = 0f;
					this.yVelocity = 0f;
				}
				Microsoft.Xna.Framework.Rectangle nextPosition = this.GetBoundingBox();
				nextPosition.X += (int)this.xVelocity;
				nextPosition.Y -= (int)this.yVelocity;
				if (!currentLocation.isCollidingPosition(nextPosition, viewport, false, this.DamageToFarmer, this.isGlider, this))
				{
					this.position.X += this.xVelocity;
					this.position.Y -= this.yVelocity;
					if (this.Slipperiness < 1000)
					{
						this.xVelocity -= this.xVelocity / (float)this.Slipperiness;
						this.yVelocity -= this.yVelocity / (float)this.Slipperiness;
						if (Math.Abs(this.xVelocity) <= 0.05f)
						{
							this.xVelocity = 0f;
						}
						if (Math.Abs(this.yVelocity) <= 0.05f)
						{
							this.yVelocity = 0f;
						}
					}
					if (!this.isGlider && this.invincibleCountdown > 0)
					{
						this.slideAnimationTimer -= time.ElapsedGameTime.Milliseconds;
						if (this.slideAnimationTimer < 0 && (Math.Abs(this.xVelocity) >= 3f || Math.Abs(this.yVelocity) >= 3f))
						{
							this.slideAnimationTimer = 100 - (int)(Math.Abs(this.xVelocity) * 2f + Math.Abs(this.yVelocity) * 2f);
							ModEntry.mp.broadcastSprites(currentLocation, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite(6, base.getStandingPosition() + new Vector2(-32f, -32f), Color.White * 0.75f, 8, Game1.random.NextDouble() < 0.5, 20f, 0, -1, -1f, -1, 0)
								{
									scale = 0.75f
								}
							});
						}
					}
				}
				else if (this.isGlider || this.Slipperiness >= 8)
				{
					bool[] array = Utility.horizontalOrVerticalCollisionDirections(nextPosition, this, false);
					if (array[0])
					{
						this.xVelocity = -this.xVelocity;
						this.position.X += (float)Math.Sign(this.xVelocity);
						this.rotation += (float)(3.1415926535897931 + (double)Game1.random.Next(-10, 11) * 3.1415926535897931 / 500.0);
					}
					if (array[1])
					{
						this.yVelocity = -this.yVelocity;
						this.position.Y -= (float)Math.Sign(this.yVelocity);
						this.rotation += (float)(3.1415926535897931 + (double)Game1.random.Next(-10, 11) * 3.1415926535897931 / 500.0);
					}
					if (this.Slipperiness < 1000)
					{
						this.xVelocity -= this.xVelocity / (float)this.Slipperiness / 4f;
						this.yVelocity -= this.yVelocity / (float)this.Slipperiness / 4f;
						if (Math.Abs(this.xVelocity) <= 0.05f)
						{
							this.xVelocity = 0f;
						}
						if (Math.Abs(this.yVelocity) <= 0.051f)
						{
							this.yVelocity = 0f;
						}
					}
				}
				else
				{
					this.xVelocity -= this.xVelocity / (float)this.Slipperiness;
					this.yVelocity -= this.yVelocity / (float)this.Slipperiness;
					if (Math.Abs(this.xVelocity) <= 0.05f)
					{
						this.xVelocity = 0f;
					}
					if (Math.Abs(this.yVelocity) <= 0.05f)
					{
						this.yVelocity = 0f;
					}
				}
				if (this.isGlider)
				{
					return;
				}
			}
			if (this.moveUp)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, false, this.DamageToFarmer, this.isGlider, this)) || this.isCharging)
				{
					this.position.Y -= (float)(base.speed + base.addedSpeed);
					if (!this.ignoreMovementAnimations)
					{
						this.Sprite.AnimateUp(time, 0, "");
					}
					base.FacingDirection = 0;
					this.faceDirection(0);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle tmp = this.nextPosition(0);
					tmp.Width /= 4;
					bool leftCorner = currentLocation.isCollidingPosition(tmp, viewport, false, this.DamageToFarmer, this.isGlider, this);
					tmp.X += tmp.Width * 3;
					bool rightCorner = currentLocation.isCollidingPosition(tmp, viewport, false, this.DamageToFarmer, this.isGlider, this);
					if (leftCorner && !rightCorner && !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, false, this.DamageToFarmer, this.isGlider, this))
					{
						this.position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (rightCorner && !leftCorner && !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, false, this.DamageToFarmer, this.isGlider, this))
					{
						this.position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(this.nextPosition(0), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						this.Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						new Vector2((float)(base.getStandingX() / 64), (float)(base.getStandingY() / 64 - 1));
						if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(0), true))
						{
							currentLocation.playSound("stoneCrack", NetAudio.SoundContext.Default);
							this.position.Y -= (float)(base.speed + base.addedSpeed);
						}
						else
						{
							this.blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					if (this.onCollision != null)
					{
						this.onCollision(currentLocation);
					}
				}
			}
			else if (this.moveRight)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, false, this.DamageToFarmer, this.isGlider, this)) || this.isCharging)
				{
					this.position.X += (float)(base.speed + base.addedSpeed);
					if (!this.ignoreMovementAnimations)
					{
						this.Sprite.AnimateRight(time, 0, "");
					}
					base.FacingDirection = 1;
					this.faceDirection(1);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle tmp2 = this.nextPosition(1);
					tmp2.Height /= 4;
					bool topCorner = currentLocation.isCollidingPosition(tmp2, viewport, false, this.DamageToFarmer, this.isGlider, this);
					tmp2.Y += tmp2.Height * 3;
					bool bottomCorner = currentLocation.isCollidingPosition(tmp2, viewport, false, this.DamageToFarmer, this.isGlider, this);
					if (topCorner && !bottomCorner && !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, false, this.DamageToFarmer, this.isGlider, this))
					{
						this.position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (bottomCorner && !topCorner && !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, false, this.DamageToFarmer, this.isGlider, this))
					{
						this.position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(this.nextPosition(1), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						this.Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						new Vector2((float)(base.getStandingX() / 64 + 1), (float)(base.getStandingY() / 64));
						if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(1), true))
						{
							currentLocation.playSound("stoneCrack", NetAudio.SoundContext.Default);
							this.position.X += (float)(base.speed + base.addedSpeed);
						}
						else
						{
							this.blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					if (this.onCollision != null)
					{
						this.onCollision(currentLocation);
					}
				}
			}
			else if (this.moveDown)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, false, this.DamageToFarmer, this.isGlider, this)) || this.isCharging)
				{
					this.position.Y += (float)(base.speed + base.addedSpeed);
					if (!this.ignoreMovementAnimations)
					{
						this.Sprite.AnimateDown(time, 0, "");
					}
					base.FacingDirection = 2;
					this.faceDirection(2);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle tmp3 = this.nextPosition(2);
					tmp3.Width /= 4;
					bool leftCorner2 = currentLocation.isCollidingPosition(tmp3, viewport, false, this.DamageToFarmer, this.isGlider, this);
					tmp3.X += tmp3.Width * 3;
					bool rightCorner2 = currentLocation.isCollidingPosition(tmp3, viewport, false, this.DamageToFarmer, this.isGlider, this);
					if (leftCorner2 && !rightCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, false, this.DamageToFarmer, this.isGlider, this))
					{
						this.position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (rightCorner2 && !leftCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, false, this.DamageToFarmer, this.isGlider, this))
					{
						this.position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(this.nextPosition(2), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						this.Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						new Vector2((float)(base.getStandingX() / 64), (float)(base.getStandingY() / 64 + 1));
						if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(2), true))
						{
							currentLocation.playSound("stoneCrack", NetAudio.SoundContext.Default);
							this.position.Y += (float)(base.speed + base.addedSpeed);
						}
						else
						{
							this.blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					if (this.onCollision != null)
					{
						this.onCollision(currentLocation);
					}
				}
			}
			else if (this.moveLeft)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, false, this.DamageToFarmer, this.isGlider, this)) || this.isCharging)
				{
					this.position.X -= (float)(base.speed + base.addedSpeed);
					base.FacingDirection = 3;
					if (!this.ignoreMovementAnimations)
					{
						this.Sprite.AnimateLeft(time, 0, "");
					}
					this.faceDirection(3);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle tmp4 = this.nextPosition(3);
					tmp4.Height /= 4;
					bool topCorner2 = currentLocation.isCollidingPosition(tmp4, viewport, false, this.DamageToFarmer, this.isGlider, this);
					tmp4.Y += tmp4.Height * 3;
					bool bottomCorner2 = currentLocation.isCollidingPosition(tmp4, viewport, false, this.DamageToFarmer, this.isGlider, this);
					if (topCorner2 && !bottomCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, false, this.DamageToFarmer, this.isGlider, this))
					{
						this.position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (bottomCorner2 && !topCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, false, this.DamageToFarmer, this.isGlider, this))
					{
						this.position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(this.nextPosition(3), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						this.Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						new Vector2((float)(base.getStandingX() / 64 - 1), (float)(base.getStandingY() / 64));
						if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(3), true))
						{
							currentLocation.playSound("stoneCrack", NetAudio.SoundContext.Default);
							this.position.X -= (float)(base.speed + base.addedSpeed);
						}
						else
						{
							this.blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					if (this.onCollision != null)
					{
						this.onCollision(currentLocation);
					}
				}
			}
			else if (!this.ignoreMovementAnimations)
			{
				if (this.moveUp)
				{
					this.Sprite.AnimateUp(time, 0, "");
				}
				else if (this.moveRight)
				{
					this.Sprite.AnimateRight(time, 0, "");
				}
				else if (this.moveDown)
				{
					this.Sprite.AnimateDown(time, 0, "");
				}
				else if (this.moveLeft)
				{
					this.Sprite.AnimateLeft(time, 0, "");
				}
			}
			if ((this.blockedInterval < 3000 || (float)this.blockedInterval > 3750f) && this.blockedInterval >= 5000)
			{
				base.speed = 4;
				this.isCharging = true;
				this.blockedInterval = 0;
			}
			if (this.DamageToFarmer > 0 && Game1.random.NextDouble() < 0.00033333333333333332)
			{
				if (base.Name.Equals("Shadow Guy") && Game1.random.NextDouble() < 0.3)
				{
					if (Game1.random.NextDouble() < 0.5)
					{
						currentLocation.playSound("grunt", NetAudio.SoundContext.Default);
						return;
					}
					currentLocation.playSound("shadowpeep", NetAudio.SoundContext.Default);
					return;
				}
				else if (!base.Name.Equals("Shadow Girl"))
				{
					if (base.Name.Equals("Ghost"))
					{
						currentLocation.playSound("ghost", NetAudio.SoundContext.Default);
						return;
					}
					if (!base.Name.Contains("Slime"))
					{
						base.Name.Contains("Jelly");
					}
				}
			}
		}

		public const int defaultInvincibleCountdown = 450;

		public const int seekPlayerIterationLimit = 80;

		[XmlElement("damageToFarmer")]
		public readonly NetInt damageToFarmer = new NetInt();

		[XmlElement("health")]
		public readonly NetInt health = new NetInt();

		[XmlElement("maxHealth")]
		public readonly NetInt maxHealth = new NetInt();

		[XmlElement("coinsToDrop")]
		public readonly NetInt coinsToDrop = new NetInt();

		[XmlElement("durationOfRandomMovements")]
		public readonly NetInt durationOfRandomMovements = new NetInt();

		[XmlElement("resilience")]
		public readonly NetInt resilience = new NetInt();

		[XmlElement("slipperiness")]
		public readonly NetInt slipperiness = new NetInt(2);

		[XmlElement("experienceGained")]
		public readonly NetInt experienceGained = new NetInt();

		[XmlElement("jitteriness")]
		public readonly NetDouble jitteriness = new NetDouble();

		[XmlElement("missChance")]
		public readonly NetDouble missChance = new NetDouble();

		[XmlElement("isGlider")]
		public readonly NetBool isGlider = new NetBool();

		[XmlElement("mineMonster")]
		public readonly NetBool mineMonster = new NetBool();

		[XmlElement("hasSpecialItem")]
		public readonly NetBool hasSpecialItem = new NetBool();

		public readonly NetIntList objectsToDrop = new NetIntList();

		protected int skipHorizontal;

		protected int invincibleCountdown;

		[XmlIgnore]
		private bool skipHorizontalUp;

		protected readonly NetInt defaultAnimationInterval = new NetInt(175);

		[XmlIgnore]
		public readonly NetBool netFocusedOnFarmers = new NetBool();

		[XmlIgnore]
		public readonly NetBool netWildernessFarmMonster = new NetBool();

		private readonly NetEvent1Field<Vector2, NetVector2> trajectoryEvent = new NetEvent1Field<Vector2, NetVector2>();

		[XmlIgnore]
		private readonly NetEvent0 deathAnimEvent = new NetEvent0(false);

		protected Familiar.collisionBehavior onCollision;

		private int slideAnimationTimer;

		protected delegate void collisionBehavior(GameLocation location);

		public long ownerId;
		public bool followingOwner = true;

		public readonly NetInt daysOld = new NetInt(0);
		public readonly NetInt exp = new NetInt(0);
		public Color mainColor;
		public Color redColor;
		public Color greenColor;
		public Color blueColor;
        public GameLocation lastLocation;
        public int baseFrame;
		public Vector2 lastPosition;
	}
}
