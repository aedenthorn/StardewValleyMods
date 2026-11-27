using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
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
                return findPlayer();
            }
        }

        [XmlIgnore]
        public int DamageToFarmer
        {
            get
            {
                return damageToFarmer;
            }
            set
            {
                damageToFarmer.Value = value;
            }
        }

        [XmlIgnore]
        public int Health
        {
            get
            {
                return health;
            }
            set
            {
                health.Value = value;
            }
        }

        [XmlIgnore]
        public int MaxHealth
        {
            get
            {
                return maxHealth;
            }
            set
            {
                maxHealth.Value = value;
            }
        }

        [XmlIgnore]
        public int ExperienceGained
        {
            get
            {
                return experienceGained;
            }
            set
            {
                experienceGained.Value = value;
            }
        }

        [XmlIgnore]
        public int Slipperiness
        {
            get
            {
                return slipperiness;
            }
            set
            {
                slipperiness.Value = value;
            }
        }

        [XmlIgnore]
        public bool focusedOnFarmers
        {
            get
            {
                return netFocusedOnFarmers;
            }
            set
            {
                netFocusedOnFarmers.Value = value;
            }
        }

        [XmlIgnore]
        public bool wildernessFarmMonster
        {
            get
            {
                return netWildernessFarmMonster;
            }
            set
            {
                netWildernessFarmMonster.Value = value;
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
            willDestroyObjectsUnderfoot = false;
        }

        public FamiliarData SaveData(GameLocation gameLocation)
        {
            return new FamiliarData()
            {
                daysOld = daysOld,
                followingOwner = followingOwner,
                exp = exp,
                ownerId = ownerId,
                mainColor = mainColor,
                redColor = redColor,
                greenColor = greenColor,
                blueColor = blueColor,
                currentLocation = gameLocation.Name,
                position = position,
                baseFrame = baseFrame,
                color = color
            };
        }

        public override void performTenMinuteUpdate(int timeOfDay, GameLocation l)
        {

        }

        protected override void initNetFields()
        {
            base.initNetFields();
            base.NetFields.AddFields(new INetSerializable[]
            {
                damageToFarmer,
                health,
                maxHealth,
                coinsToDrop,
                durationOfRandomMovements,
                resilience,
                slipperiness,
                experienceGained,
                jitteriness,
                missChance,
                isGlider,
                mineMonster,
                hasSpecialItem,
                objectsToDrop,
                defaultAnimationInterval,
                netFocusedOnFarmers,
                netWildernessFarmMonster,
                deathAnimEvent,
                daysOld,
                exp,
                trajectoryEvent
            });
            position.Field.AxisAlignedMovement = false;
            deathAnimEvent.onEvent += localDeathAnimation;
            trajectoryEvent.onEvent += doSetTrajectory;
            trajectoryEvent.InterpolationWait = false;
        }
        public override void dayUpdate(int dayOfMonth)
        {
            base.dayUpdate(dayOfMonth);
            daysOld.Value = daysOld.Value + 1;
            SetScale();
        }

        public void SetScale()
        {
            Scale = (float)Math.Min(ModEntry.Config.MaxScale, ModEntry.Config.StartScale + (daysOld * ModEntry.Config.ScalePerDay));
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
                    double priority = findPlayerPriority(f);
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
            return focusedOnFarmers || withinPlayerThreshold(moveTowardPlayerThreshold);
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
            
            parseMonsterInfo(name);
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
            if (!isGlider)
            {
                base.draw(b);
            }
        }

        public bool isInvincible()
        {
            return invincibleCountdown > 0;
        }

        public void setInvincibleCountdown(int time)
        {
            invincibleCountdown = time;
            base.startGlowing(new Color(255, 0, 0), false, 0.25f);
            glowingTransparency = 1f;
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
            Health = Convert.ToInt32(monsterInfo[0]);
            MaxHealth = Health;
            DamageToFarmer = Convert.ToInt32(monsterInfo[1]);
            coinsToDrop.Value = Game1.random.Next(Convert.ToInt32(monsterInfo[2]), Convert.ToInt32(monsterInfo[3]) + 1);
            isGlider.Value = Convert.ToBoolean(monsterInfo[4]);
            durationOfRandomMovements.Value = Convert.ToInt32(monsterInfo[5]);
            string[] objectsSplit = monsterInfo[6].Split(new char[]
            {
                ' '
            });
            objectsToDrop.Clear();
            for (int i = 0; i < objectsSplit.Length; i += 2)
            {
                if (Game1.random.NextDouble() < Convert.ToDouble(objectsSplit[i + 1]))
                {
                    objectsToDrop.Add(Convert.ToInt32(objectsSplit[i]));
                }
            }
            resilience.Value = Convert.ToInt32(monsterInfo[7]);
            jitteriness.Value = Convert.ToDouble(monsterInfo[8]);
            base.willDestroyObjectsUnderfoot = false;
            base.moveTowardPlayer(Convert.ToInt32(monsterInfo[9]));
            base.speed = Convert.ToInt32(monsterInfo[10]);
            missChance.Value = Convert.ToDouble(monsterInfo[11]);
            mineMonster.Value = Convert.ToBoolean(monsterInfo[12]);
            if (maxTimesReachedMineBottom() >= 1 && mineMonster)
            {
                resilience.Value += resilience.Value / 2;
                missChance.Value *= 2.0;
                Health += Game1.random.Next(0, Health);
                DamageToFarmer += Game1.random.Next(0, DamageToFarmer / 2);
                coinsToDrop.Value += Game1.random.Next(0, coinsToDrop + 1);
                if (Game1.random.NextDouble() < 0.001)
                {
                    objectsToDrop.Add((Game1.random.NextDouble() < 0.5) ? 72 : 74);
                }
            }
            try
            {
                ExperienceGained = Convert.ToInt32(monsterInfo[13]);
            }
            catch (Exception)
            {
                ExperienceGained = 1;
            }
            if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
            {
                base.displayName = monsterInfo[monsterInfo.Length - 1];
            }
        }

        public override void reloadSprite()
        {
            Sprite = new AnimatedSprite((name == "Junimo" ? "Characters\\" : "Characters\\Monsters\\") + base.Name, 0, 16, 16);
        }

        public virtual void shedChunks(int number)
        {
            shedChunks(number, 0.75f);
        }

        public virtual void shedChunks(int number, float scale)
        {
            if (Sprite.Texture.Height > Sprite.getHeight() * 4)
            {
                Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Microsoft.Xna.Framework.Rectangle(0, Sprite.getHeight() * 4 + 16, 16, 16), 8, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)base.getTileLocation().Y, Color.White, 4f * scale);
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
            sharedDeathAnimation();
            deathAnimEvent.Fire();
        }

        protected virtual void sharedDeathAnimation()
        {
            shedChunks(Game1.random.Next(4, 9), 0.75f);
        }

        protected virtual void localDeathAnimation()
        {
        }


        public virtual int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
        {
            if (ModEntry.Config.Invincible)
                return 0;
            return takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, "hitEnemy");
        }

        public int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, string hitSound)
        {
            int actualDamage = Math.Max(1, damage - resilience);
            slideAnimationTimer = 0;
            if (Game1.random.NextDouble() < missChance - missChance * addedPrecision)
            {
                actualDamage = -1;
            }
            else
            {
                Health -= actualDamage;
                base.currentLocation.playSound(hitSound, NetAudio.SoundContext.Default);
                base.setTrajectory(xTrajectory / 3, yTrajectory / 3);
                if (Health <= 0)
                {
                    deathAnimation();
                }
            }
            return actualDamage;
        }

        public override void setTrajectory(Vector2 trajectory)
        {
            trajectoryEvent.Fire(trajectory);
        }

        private void doSetTrajectory(Vector2 trajectory)
        {
            if (!Game1.IsMasterGame)
            {
                return;
            }
            if (Math.Abs(trajectory.X) > Math.Abs(xVelocity))
            {
                xVelocity = trajectory.X;
            }
            if (Math.Abs(trajectory.Y) > Math.Abs(yVelocity))
            {
                yVelocity = trajectory.Y;
            }
        }

        public virtual void behaviorAtGameTick(GameTime time)
        {
            if (timeBeforeAIMovementAgain > 0f)
            {
                timeBeforeAIMovementAgain -= (float)time.ElapsedGameTime.Milliseconds;
            }
            if (Player.isRafting && withinPlayerThreshold(4))
            {
                if (Math.Abs(Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y) > 192)
                {
                    if (Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X > 0)
                    {
                        SetMovingLeft(true);
                    }
                    else
                    {
                        SetMovingRight(true);
                    }
                }
                else if (Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y > 0)
                {
                    SetMovingUp(true);
                }
                else
                {
                    SetMovingDown(true);
                }
                MovePosition(time, Game1.viewport, base.currentLocation);
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

            trajectoryEvent.Poll();
            deathAnimEvent.Poll();
            position.UpdateExtrapolation((float)(base.speed + base.addedSpeed));
            if (invincibleCountdown > 0)
            {
                invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
                if (invincibleCountdown <= 0)
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
                behaviorAtGameTick(time);
            }
            
            if(!(this is ButterflyFamiliar))
                updateAnimation(time);

            if (controller != null && withinPlayerThreshold(3))
            {
                controller = null;
            }
        }

        protected void resetAnimationSpeed()
        {
            if (!ignoreMovementAnimations)
            {
                Sprite.interval = (float)defaultAnimationInterval - (float)(base.speed + base.addedSpeed - 2) * 20f;
            }
        }

        protected virtual void updateAnimation(GameTime time)
        {
            if (!Game1.IsMasterGame)
            {
                updateMonsterSlaveAnimation(time);
            }
            resetAnimationSpeed();
        }

        protected override void updateSlaveAnimation(GameTime time)
        {
        }

        protected virtual void updateMonsterSlaveAnimation(GameTime time)
        {
            Sprite.animateOnce(time);
        }

        private bool doHorizontalMovement(GameLocation location)
        {
            bool wasAbleToMoveHorizontally = false;
            if (base.Position.X > Player.Position.X + 8f || (skipHorizontal > 0 && Player.getStandingX() < base.getStandingX() - 8))
            {
                base.SetMovingOnlyLeft();
                if (!location.isCollidingPosition(nextPosition(3), Game1.viewport, false, DamageToFarmer, isGlider, this))
                {
                    MovePosition(Game1.currentGameTime, Game1.viewport, location);
                    wasAbleToMoveHorizontally = true;
                }
                else
                {
                    faceDirection(3);
                    if (durationOfRandomMovements > 0 && Game1.random.NextDouble() < jitteriness)
                    {
                        if (Game1.random.NextDouble() < 0.5)
                        {
                            base.tryToMoveInDirection(2, false, DamageToFarmer, isGlider);
                        }
                        else
                        {
                            base.tryToMoveInDirection(0, false, DamageToFarmer, isGlider);
                        }
                        timeBeforeAIMovementAgain = (float)durationOfRandomMovements;
                    }
                }
            }
            else if (base.Position.X < Player.Position.X - 8f)
            {
                base.SetMovingOnlyRight();
                if (!location.isCollidingPosition(nextPosition(1), Game1.viewport, false, DamageToFarmer, isGlider, this))
                {
                    MovePosition(Game1.currentGameTime, Game1.viewport, location);
                    wasAbleToMoveHorizontally = true;
                }
                else
                {
                    faceDirection(1);
                    if (durationOfRandomMovements > 0 && Game1.random.NextDouble() < jitteriness)
                    {
                        if (Game1.random.NextDouble() < 0.5)
                        {
                            base.tryToMoveInDirection(2, false, DamageToFarmer, isGlider);
                        }
                        else
                        {
                            base.tryToMoveInDirection(0, false, DamageToFarmer, isGlider);
                        }
                        timeBeforeAIMovementAgain = (float)durationOfRandomMovements;
                    }
                }
            }
            else
            {
                base.faceGeneralDirection(Player.getStandingPosition(), 0, false);
                base.setMovingInFacingDirection();
                skipHorizontal = 500;
            }
            return wasAbleToMoveHorizontally;
        }

        private void checkHorizontalMovement(ref bool success, ref bool setMoving, ref bool scootSuccess, Farmer who, GameLocation location)
        {
            if (who.Position.X > base.Position.X + 16f)
            {
                base.SetMovingOnlyRight();
                setMoving = true;
                if (!location.isCollidingPosition(nextPosition(1), Game1.viewport, false, DamageToFarmer, isGlider, this))
                {
                    success = true;
                }
                else
                {
                    MovePosition(Game1.currentGameTime, Game1.viewport, location);
                    if (!base.Position.Equals(lastPosition))
                    {
                        scootSuccess = true;
                    }
                }
            }
            if (!success && who.Position.X < base.Position.X - 16f)
            {
                base.SetMovingOnlyLeft();
                setMoving = true;
                if (!location.isCollidingPosition(nextPosition(3), Game1.viewport, false, DamageToFarmer, isGlider, this))
                {
                    success = true;
                    return;
                }
                MovePosition(Game1.currentGameTime, Game1.viewport, location);
                if (!base.Position.Equals(lastPosition))
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
                if (!location.isCollidingPosition(nextPosition(0), Game1.viewport, false, DamageToFarmer, isGlider, this))
                {
                    success = true;
                }
                else
                {
                    MovePosition(Game1.currentGameTime, Game1.viewport, location);
                    if (!base.Position.Equals(lastPosition))
                    {
                        scootSuccess = true;
                    }
                }
            }
            if (!success && who.Position.Y > base.Position.Y + 16f)
            {
                base.SetMovingOnlyDown();
                setMoving = true;
                if (!location.isCollidingPosition(nextPosition(2), Game1.viewport, false, DamageToFarmer, isGlider, this))
                {
                    success = true;
                    return;
                }
                MovePosition(Game1.currentGameTime, Game1.viewport, location);
                if (!base.Position.Equals(lastPosition))
                {
                    scootSuccess = true;
                }
            }
        }

        public override void updateMovement(GameLocation location, GameTime time)
        {
            if (base.IsWalkingTowardPlayer)
            {
                if ((moveTowardPlayerThreshold == -1 || withinPlayerThreshold()) && timeBeforeAIMovementAgain <= 0f && !isGlider && location.map.GetLayer("Back").Tiles[(int)Player.getTileLocation().X, (int)Player.getTileLocation().Y] != null && !location.map.GetLayer("Back").Tiles[(int)Player.getTileLocation().X, (int)Player.getTileLocation().Y].Properties.ContainsKey("NPCBarrier"))
                {
                    if (skipHorizontal <= 0)
                    {
                        if (lastPosition.Equals(base.Position) && Game1.random.NextDouble() < 0.001)
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
                            skipHorizontal = 700;
                            return;
                        }
                        bool success = false;
                        bool setMoving = false;
                        bool scootSuccess = false;
                        if (lastPosition.X == base.Position.X)
                        {
                            checkHorizontalMovement(ref success, ref setMoving, ref scootSuccess, Player, location);
                            checkVerticalMovement(ref success, ref setMoving, ref scootSuccess, Player, location);
                        }
                        else
                        {
                            checkVerticalMovement(ref success, ref setMoving, ref scootSuccess, Player, location);
                            checkHorizontalMovement(ref success, ref setMoving, ref scootSuccess, Player, location);
                        }
                        if (!success && !setMoving)
                        {
                            Halt();
                            base.faceGeneralDirection(Player.getStandingPosition(), 0, false);
                        }
                        if (success)
                        {
                            skipHorizontal = 500;
                        }
                        if (scootSuccess)
                        {
                            return;
                        }
                    }
                    else
                    {
                        skipHorizontal -= time.ElapsedGameTime.Milliseconds;
                    }
                }
            }
            else
            {
                defaultMovementBehavior(time);
            }
            MovePosition(time, Game1.viewport, location);
            if (base.Position.Equals(lastPosition) && base.IsWalkingTowardPlayer && withinPlayerThreshold())
            {
                noMovementProgressNearPlayerBehavior();
            }
        }

        public virtual void noMovementProgressNearPlayerBehavior()
        {
            Halt();
            base.faceGeneralDirection(Player.getStandingPosition(), 0, false);
        }

        public virtual void defaultMovementBehavior(GameTime time)
        {
            if (Game1.random.NextDouble() < jitteriness * 1.8 && skipHorizontal <= 0)
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
                        Halt();
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
            lastPosition = base.Position;
            if (xVelocity != 0f || yVelocity != 0f)
            {
                if (double.IsNaN((double)xVelocity) || double.IsNaN((double)yVelocity))
                {
                    xVelocity = 0f;
                    yVelocity = 0f;
                }
                Microsoft.Xna.Framework.Rectangle nextPosition = GetBoundingBox();
                nextPosition.X += (int)xVelocity;
                nextPosition.Y -= (int)yVelocity;
                if (!currentLocation.isCollidingPosition(nextPosition, viewport, false, DamageToFarmer, isGlider, this))
                {
                    position.X += xVelocity;
                    position.Y -= yVelocity;
                    if (Slipperiness < 1000)
                    {
                        xVelocity -= xVelocity / (float)Slipperiness;
                        yVelocity -= yVelocity / (float)Slipperiness;
                        if (Math.Abs(xVelocity) <= 0.05f)
                        {
                            xVelocity = 0f;
                        }
                        if (Math.Abs(yVelocity) <= 0.05f)
                        {
                            yVelocity = 0f;
                        }
                    }
                    if (!isGlider && invincibleCountdown > 0)
                    {
                        slideAnimationTimer -= time.ElapsedGameTime.Milliseconds;
                        if (slideAnimationTimer < 0 && (Math.Abs(xVelocity) >= 3f || Math.Abs(yVelocity) >= 3f))
                        {
                            slideAnimationTimer = 100 - (int)(Math.Abs(xVelocity) * 2f + Math.Abs(yVelocity) * 2f);
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
                else if (isGlider || Slipperiness >= 8)
                {
                    bool[] array = Utility.horizontalOrVerticalCollisionDirections(nextPosition, this, false);
                    if (array[0])
                    {
                        xVelocity = -xVelocity;
                        position.X += (float)Math.Sign(xVelocity);
                        rotation += (float)(3.1415926535897931 + (double)Game1.random.Next(-10, 11) * 3.1415926535897931 / 500.0);
                    }
                    if (array[1])
                    {
                        yVelocity = -yVelocity;
                        position.Y -= (float)Math.Sign(yVelocity);
                        rotation += (float)(3.1415926535897931 + (double)Game1.random.Next(-10, 11) * 3.1415926535897931 / 500.0);
                    }
                    if (Slipperiness < 1000)
                    {
                        xVelocity -= xVelocity / (float)Slipperiness / 4f;
                        yVelocity -= yVelocity / (float)Slipperiness / 4f;
                        if (Math.Abs(xVelocity) <= 0.05f)
                        {
                            xVelocity = 0f;
                        }
                        if (Math.Abs(yVelocity) <= 0.051f)
                        {
                            yVelocity = 0f;
                        }
                    }
                }
                else
                {
                    xVelocity -= xVelocity / (float)Slipperiness;
                    yVelocity -= yVelocity / (float)Slipperiness;
                    if (Math.Abs(xVelocity) <= 0.05f)
                    {
                        xVelocity = 0f;
                    }
                    if (Math.Abs(yVelocity) <= 0.05f)
                    {
                        yVelocity = 0f;
                    }
                }
                if (isGlider)
                {
                    return;
                }
            }
            if (moveUp)
            {
                if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(nextPosition(0), viewport, false, DamageToFarmer, isGlider, this)) || isCharging)
                {
                    position.Y -= (float)(base.speed + base.addedSpeed);
                    if (!ignoreMovementAnimations)
                    {
                        Sprite.AnimateUp(time, 0, "");
                    }
                    base.FacingDirection = 0;
                    faceDirection(0);
                }
                else
                {
                    Microsoft.Xna.Framework.Rectangle tmp = nextPosition(0);
                    tmp.Width /= 4;
                    bool leftCorner = currentLocation.isCollidingPosition(tmp, viewport, false, DamageToFarmer, isGlider, this);
                    tmp.X += tmp.Width * 3;
                    bool rightCorner = currentLocation.isCollidingPosition(tmp, viewport, false, DamageToFarmer, isGlider, this);
                    if (leftCorner && !rightCorner && !currentLocation.isCollidingPosition(nextPosition(1), viewport, false, DamageToFarmer, isGlider, this))
                    {
                        position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
                    }
                    else if (rightCorner && !leftCorner && !currentLocation.isCollidingPosition(nextPosition(3), viewport, false, DamageToFarmer, isGlider, this))
                    {
                        position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
                    }
                    if (!currentLocation.isTilePassable(nextPosition(0), viewport) || !base.willDestroyObjectsUnderfoot)
                    {
                        Halt();
                    }
                    else if (base.willDestroyObjectsUnderfoot)
                    {
                        new Vector2((float)(base.getStandingX() / 64), (float)(base.getStandingY() / 64 - 1));
                        if (currentLocation.characterDestroyObjectWithinRectangle(nextPosition(0), true))
                        {
                            currentLocation.playSound("stoneCrack", NetAudio.SoundContext.Default);
                            position.Y -= (float)(base.speed + base.addedSpeed);
                        }
                        else
                        {
                            blockedInterval += time.ElapsedGameTime.Milliseconds;
                        }
                    }
                    if (onCollision != null)
                    {
                        onCollision(currentLocation);
                    }
                }
            }
            else if (moveRight)
            {
                if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(nextPosition(1), viewport, false, DamageToFarmer, isGlider, this)) || isCharging)
                {
                    position.X += (float)(base.speed + base.addedSpeed);
                    if (!ignoreMovementAnimations)
                    {
                        Sprite.AnimateRight(time, 0, "");
                    }
                    base.FacingDirection = 1;
                    faceDirection(1);
                }
                else
                {
                    Microsoft.Xna.Framework.Rectangle tmp2 = nextPosition(1);
                    tmp2.Height /= 4;
                    bool topCorner = currentLocation.isCollidingPosition(tmp2, viewport, false, DamageToFarmer, isGlider, this);
                    tmp2.Y += tmp2.Height * 3;
                    bool bottomCorner = currentLocation.isCollidingPosition(tmp2, viewport, false, DamageToFarmer, isGlider, this);
                    if (topCorner && !bottomCorner && !currentLocation.isCollidingPosition(nextPosition(2), viewport, false, DamageToFarmer, isGlider, this))
                    {
                        position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
                    }
                    else if (bottomCorner && !topCorner && !currentLocation.isCollidingPosition(nextPosition(0), viewport, false, DamageToFarmer, isGlider, this))
                    {
                        position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
                    }
                    if (!currentLocation.isTilePassable(nextPosition(1), viewport) || !base.willDestroyObjectsUnderfoot)
                    {
                        Halt();
                    }
                    else if (base.willDestroyObjectsUnderfoot)
                    {
                        new Vector2((float)(base.getStandingX() / 64 + 1), (float)(base.getStandingY() / 64));
                        if (currentLocation.characterDestroyObjectWithinRectangle(nextPosition(1), true))
                        {
                            currentLocation.playSound("stoneCrack", NetAudio.SoundContext.Default);
                            position.X += (float)(base.speed + base.addedSpeed);
                        }
                        else
                        {
                            blockedInterval += time.ElapsedGameTime.Milliseconds;
                        }
                    }
                    if (onCollision != null)
                    {
                        onCollision(currentLocation);
                    }
                }
            }
            else if (moveDown)
            {
                if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(nextPosition(2), viewport, false, DamageToFarmer, isGlider, this)) || isCharging)
                {
                    position.Y += (float)(base.speed + base.addedSpeed);
                    if (!ignoreMovementAnimations)
                    {
                        Sprite.AnimateDown(time, 0, "");
                    }
                    base.FacingDirection = 2;
                    faceDirection(2);
                }
                else
                {
                    Microsoft.Xna.Framework.Rectangle tmp3 = nextPosition(2);
                    tmp3.Width /= 4;
                    bool leftCorner2 = currentLocation.isCollidingPosition(tmp3, viewport, false, DamageToFarmer, isGlider, this);
                    tmp3.X += tmp3.Width * 3;
                    bool rightCorner2 = currentLocation.isCollidingPosition(tmp3, viewport, false, DamageToFarmer, isGlider, this);
                    if (leftCorner2 && !rightCorner2 && !currentLocation.isCollidingPosition(nextPosition(1), viewport, false, DamageToFarmer, isGlider, this))
                    {
                        position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
                    }
                    else if (rightCorner2 && !leftCorner2 && !currentLocation.isCollidingPosition(nextPosition(3), viewport, false, DamageToFarmer, isGlider, this))
                    {
                        position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
                    }
                    if (!currentLocation.isTilePassable(nextPosition(2), viewport) || !base.willDestroyObjectsUnderfoot)
                    {
                        Halt();
                    }
                    else if (base.willDestroyObjectsUnderfoot)
                    {
                        new Vector2((float)(base.getStandingX() / 64), (float)(base.getStandingY() / 64 + 1));
                        if (currentLocation.characterDestroyObjectWithinRectangle(nextPosition(2), true))
                        {
                            currentLocation.playSound("stoneCrack", NetAudio.SoundContext.Default);
                            position.Y += (float)(base.speed + base.addedSpeed);
                        }
                        else
                        {
                            blockedInterval += time.ElapsedGameTime.Milliseconds;
                        }
                    }
                    if (onCollision != null)
                    {
                        onCollision(currentLocation);
                    }
                }
            }
            else if (moveLeft)
            {
                if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(nextPosition(3), viewport, false, DamageToFarmer, isGlider, this)) || isCharging)
                {
                    position.X -= (float)(base.speed + base.addedSpeed);
                    base.FacingDirection = 3;
                    if (!ignoreMovementAnimations)
                    {
                        Sprite.AnimateLeft(time, 0, "");
                    }
                    faceDirection(3);
                }
                else
                {
                    Microsoft.Xna.Framework.Rectangle tmp4 = nextPosition(3);
                    tmp4.Height /= 4;
                    bool topCorner2 = currentLocation.isCollidingPosition(tmp4, viewport, false, DamageToFarmer, isGlider, this);
                    tmp4.Y += tmp4.Height * 3;
                    bool bottomCorner2 = currentLocation.isCollidingPosition(tmp4, viewport, false, DamageToFarmer, isGlider, this);
                    if (topCorner2 && !bottomCorner2 && !currentLocation.isCollidingPosition(nextPosition(2), viewport, false, DamageToFarmer, isGlider, this))
                    {
                        position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
                    }
                    else if (bottomCorner2 && !topCorner2 && !currentLocation.isCollidingPosition(nextPosition(0), viewport, false, DamageToFarmer, isGlider, this))
                    {
                        position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
                    }
                    if (!currentLocation.isTilePassable(nextPosition(3), viewport) || !base.willDestroyObjectsUnderfoot)
                    {
                        Halt();
                    }
                    else if (base.willDestroyObjectsUnderfoot)
                    {
                        new Vector2((float)(base.getStandingX() / 64 - 1), (float)(base.getStandingY() / 64));
                        if (currentLocation.characterDestroyObjectWithinRectangle(nextPosition(3), true))
                        {
                            currentLocation.playSound("stoneCrack", NetAudio.SoundContext.Default);
                            position.X -= (float)(base.speed + base.addedSpeed);
                        }
                        else
                        {
                            blockedInterval += time.ElapsedGameTime.Milliseconds;
                        }
                    }
                    if (onCollision != null)
                    {
                        onCollision(currentLocation);
                    }
                }
            }
            else if (!ignoreMovementAnimations)
            {
                if (moveUp)
                {
                    Sprite.AnimateUp(time, 0, "");
                }
                else if (moveRight)
                {
                    Sprite.AnimateRight(time, 0, "");
                }
                else if (moveDown)
                {
                    Sprite.AnimateDown(time, 0, "");
                }
                else if (moveLeft)
                {
                    Sprite.AnimateLeft(time, 0, "");
                }
            }
            if ((blockedInterval < 3000 || (float)blockedInterval > 3750f) && blockedInterval >= 5000)
            {
                base.speed = 4;
                isCharging = true;
                blockedInterval = 0;
            }
            if (DamageToFarmer > 0 && Game1.random.NextDouble() < 0.00033333333333333332)
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
        public readonly NetColor color = new NetColor();
    }
}
