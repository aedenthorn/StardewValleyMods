using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Network;
using System;

namespace Familiars
{
    public class JunimoFamiliar : Familiar
    {
        public bool EventActor
        {
            get
            {
                return eventActor;
            }
            set
            {
                eventActor.Value = value;
            }
        }

        public JunimoFamiliar()
        {
            forceUpdateTimer = 9999;
        }

        public JunimoFamiliar(Vector2 position, long ownerId) : base("Junimo", position, new AnimatedSprite("Characters\\Junimo", 0, 16, 16))
        {
            friendly.Value = true;
            nextPosition.Value = GetBoundingBox();
            Breather = false;
            speed = 3;
            forceUpdateTimer = 9999;
            collidesWithOtherCharacters.Value = true;
            farmerPassesThrough = true;
            ignoreMovementAnimations = true;

            color.Value = FamiliarsUtils.GetJunimoColor();

        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields(new INetSerializable[]
            {
                friendly,
                holdingStar,
                holdingBundle,
                stayPut,
                eventActor,
                motion,
                nextPosition,
                color,
                bundleColor,
            });
            NetFields.AddFields(new INetSerializable[]
            {
                setReturnToJunimoHutToFetchStarControllerEvent,
                setBringBundleBackToHutControllerEvent,
                setJunimoReachedHutToFetchStarControllerEvent,
                starDoneSpinningEvent,
                returnToJunimoHutToFetchFinalStarEvent
            });
            setReturnToJunimoHutToFetchStarControllerEvent.onEvent += setReturnToJunimoHutToFetchStarController;
            setBringBundleBackToHutControllerEvent.onEvent += setBringBundleBackToHutController;
            setJunimoReachedHutToFetchStarControllerEvent.onEvent += setJunimoReachedHutToFetchStarController;
            starDoneSpinningEvent.onEvent += performStartDoneSpinning;
            returnToJunimoHutToFetchFinalStarEvent.onEvent += returnToJunimoHutToFetchFinalStar;
            position.Field.AxisAlignedMovement = false;
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
            motion.X = xSpeed;
            motion.Y = ySpeed;
        }

        public void setMoving(Vector2 motion)
        {
            this.motion.Value = motion;
        }

        public override void Halt()
        {
            base.Halt();
            motion.Value = Vector2.Zero;
        }

        public void stayStill()
        {
            stayPut.Value = true;
            motion.Value = Vector2.Zero;
        }

        public void allowToMoveAgain()
        {
            stayPut.Value = false;
        }

        private void returnToJunimoHutToFetchFinalStar()
        {
            if (currentLocation == Game1.currentLocation)
            {
                Game1.globalFadeToBlack(new Game1.afterFadeFunction(finalCutscene), 0.005f);
                Game1.freezeControls = true;
                Game1.flashAlpha = 1f;
            }
        }

        public void returnToJunimoHutToFetchStar(GameLocation location)
        {
            currentLocation = location;
            friendly.Value = true;
            if (((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areAllAreasComplete())
            {
                returnToJunimoHutToFetchFinalStarEvent.Fire();
                collidesWithOtherCharacters.Value = false;
                farmerPassesThrough = false;
                stayStill();
                faceDirection(0);
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
                setReturnToJunimoHutToFetchStarControllerEvent.Fire();
                if (ModEntry.Config.JunimoSoundEffects)
                    location.playSound("junimoMeep1", NetAudio.SoundContext.Default);
                collidesWithOtherCharacters.Value = false;
                farmerPassesThrough = false;
                holdingBundle.Value = true;
                speed = 3;
            }
        }

        private void setReturnToJunimoHutToFetchStarController()
        {
            if (!Game1.IsMasterGame)
            {
                return;
            }
            controller = new PathFindController(this, currentLocation, new Point(25, 10), 0, new PathFindController.endBehavior(junimoReachedHutToFetchStar));
        }

        private void finalCutscene()
        {
            collidesWithOtherCharacters.Value = false;
            farmerPassesThrough = false;
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
            Game1.globalFadeToClear(new Game1.afterFadeFunction(goodbyeDance), 0.005f);
            Game1.pauseTime = 1000f;
            Game1.freezeControls = true;
        }

        public void bringBundleBackToHut(Color bundleColor, GameLocation location)
        {
            currentLocation = location;
            if (!holdingBundle)
            {
                Position = Utility.getRandomAdjacentOpenTile(Game1.player.getTileLocation(), location) * 64f;
                int iter = 0;
                while (location.isCollidingPosition(GetBoundingBox(), Game1.viewport, this) && iter < 5)
                {
                    Position = Utility.getRandomAdjacentOpenTile(Game1.player.getTileLocation(), location) * 64f;
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
                setBringBundleBackToHutControllerEvent.Fire();
                collidesWithOtherCharacters.Value = false;
                farmerPassesThrough = false;
                holdingBundle.Value = true;
                speed = 3;
            }
        }

        private void setBringBundleBackToHutController()
        {
            if (!Game1.IsMasterGame)
            {
                return;
            }
            controller = new PathFindController(this, currentLocation, new Point(25, 10), 0, new PathFindController.endBehavior(junimoReachedHutToReturnBundle));
        }

        private void junimoReachedHutToReturnBundle(Character c, GameLocation l)
        {
            currentLocation = l;
            holdingBundle.Value = false;
            collidesWithOtherCharacters.Value = true;
            farmerPassesThrough = true;
            l.playSound("Ship", NetAudio.SoundContext.Default);
        }

        private void junimoReachedHutToFetchStar(Character c, GameLocation l)
        {
            currentLocation = l;
            holdingStar.Value = true;
            holdingBundle.Value = false;
            speed = 3;
            collidesWithOtherCharacters.Value = false;
            farmerPassesThrough = false;
            setJunimoReachedHutToFetchStarControllerEvent.Fire();
            l.playSound("dwop", NetAudio.SoundContext.Default);
            farmerPassesThrough = false;
        }

        private void setJunimoReachedHutToFetchStarController()
        {
            if (!Game1.IsMasterGame)
            {
                return;
            }
            controller = new PathFindController(this, currentLocation, new Point(32, 9), 2, new PathFindController.endBehavior(placeStar));
        }

        private void placeStar(Character c, GameLocation l)
        {
            currentLocation = l;
            collidesWithOtherCharacters.Value = false;
            farmerPassesThrough = true;
            holdingStar.Value = false;
            l.playSound("tinyWhip", NetAudio.SoundContext.Default);
            friendly.Value = true;
            speed = 3;
            ModEntry.mp.broadcastSprites(l, new TemporaryAnimatedSprite[]
            {
                new TemporaryAnimatedSprite(Sprite.textureName, new Rectangle(0, 109, 16, 19), 40f, 8, 10, Position + new Vector2(0f, -64f), false, false, 1f, 0f, Color.White, 4f * scale, 0f, 0f, 0f, false)
                {
                    endFunction = new TemporaryAnimatedSprite.endBehavior(starDoneSpinning),
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
            starDoneSpinningEvent.Fire();
            (currentLocation as CommunityCenter).addStarToPlaque();
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
            if (textAboveHeadTimer > 0 && textAboveHead != null)
            {
                Vector2 local = Game1.GlobalToLocal(new Vector2(getStandingX(), getStandingY() - 128f + yJumpOffset));
                if (textAboveHeadStyle == 0)
                {
                    local += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
                }
                SpriteText.drawStringWithScrollCenteredAt(b, textAboveHead, (int)local.X, (int)local.Y, "", textAboveHeadAlpha, textAboveHeadColor, 1, getTileY() * 64 / 10000f + 0.001f + getTileX() / 10000f, true);
            }
        }

        protected override void updateSlaveAnimation(GameTime time)
        {
            if (holdingStar || holdingBundle)
            {
                Sprite.Animate(time, 44, 4, 200f);
                return;
            }
            if (!position.IsInterpolating())
            {
                Sprite.Animate(time, 8, 4, 100f);
                return;
            }
            if (FacingDirection == 1)
            {
                flip = false;
                Sprite.Animate(time, 16, 8, 50f);
                return;
            }
            if (FacingDirection == 3)
            {
                Sprite.Animate(time, 16, 8, 50f);
                flip = true;
                return;
            }
            if (FacingDirection == 0)
            {
                Sprite.Animate(time, 32, 8, 50f);
                return;
            }
            Sprite.Animate(time, 0, 8, 50f);
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
            currentLocation = location;
            base.update(time, location);
            forceUpdateTimer = 99999;

            if (eventActor)
            {
                return;
            }
            soundTimer--;
            farmerCloseCheckTimer -= time.ElapsedGameTime.Milliseconds;

            if (followingOwner)
            {
                farmerCloseCheckTimer = 100;
                if (holdingStar.Value)
                {
                    setJunimoReachedHutToFetchStarController();
                }
                else
                {
                    Farmer f = Game1.getFarmer(ownerId);
                    if (f != null && f.currentLocation == currentLocation)
                    {
                        if (Vector2.Distance(Position, f.Position) > speed * 4 + 32)
                        {
                            if (motion.Equals(Vector2.Zero) && soundTimer <= 0)
                            {
                                jump((int)(5 + Game1.random.Next(1, 4) * scale));
                                location.localSound("junimoMeep1");
                                soundTimer = 400;
                            }
                            if (Game1.random.NextDouble() < 0.007)
                            {
                                jumpWithoutSound((int)(5 + Game1.random.Next(1, 4) * scale));
                            }
                            setMoving(Utility.getVelocityTowardPlayer(new Point((int)Position.X, (int)Position.Y), speed, f));
                        }
                        else
                        {
                            motion.Value = Vector2.Zero;
                        }
                    }
                    else
                    {
                        motion.Value = Vector2.Zero;
                    }
                }
            }
            if (!IsInvisible && controller == null)
            {
                Farmer f = Game1.getFarmer(ownerId);
                nextPosition.Value = GetBoundingBox();
                nextPosition.X += (int)motion.X;
                if (!location.isCollidingPosition(nextPosition, Game1.viewport, this))
                {
                    position.X += (int)motion.X;
                }
                nextPosition.X -= (int)motion.X;
                nextPosition.Y += (int)motion.Y;
                if (!location.isCollidingPosition(nextPosition, Game1.viewport, this))
                {
                    position.Y += (int)motion.Y;
                }

                lastHeal -= time.ElapsedGameTime.Milliseconds;

                if (Vector2.Distance(f.getTileLocation(), getTileLocation()) < 3 && lastHeal < 0 && Game1.random.NextDouble() < HealChance())
                {
                    bool heal = Game1.random.NextDouble() < 0.5;
                    location.temporarySprites.Add(new TemporaryAnimatedSprite((Game1.random.NextDouble() < 0.5) ? 10 : 11, Position, (heal ? Color.Red : Color.LawnGreen), 8, false, 100f, 0, -1, -1f, -1, 0)
                    {
                        motion = motion.Value / 4f,
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
            if (controller == null && motion.Equals(Vector2.Zero))
            {
                Sprite.Animate(time, 8, 4, 100f);
                return;
            }
            if (holdingStar || holdingBundle)
            {
                Sprite.Animate(time, 44, 4, 200f);
                return;
            }
            if (moveRight || (Math.Abs(motion.X) > Math.Abs(motion.Y) && motion.X > 0f))
            {
                flip = false;
                Sprite.Animate(time, 16, 8, 50f);
                return;
            }
            if (moveLeft || (Math.Abs(motion.X) > Math.Abs(motion.Y) && motion.X < 0f))
            {
                Sprite.Animate(time, 16, 8, 50f);
                flip = true;
                return;
            }
            if (moveUp || (Math.Abs(motion.Y) > Math.Abs(motion.X) && motion.Y < 0f))
            {
                Sprite.Animate(time, 32, 8, 50f);
                return;
            }
            Sprite.Animate(time, 0, 8, 50f);
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
            if (!IsInvisible)
            {
                Sprite.UpdateSourceRect();
                b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(Sprite.SpriteWidth * 4 / 2, Sprite.SpriteHeight * 3f / 4f * 4f / (float)Math.Pow(Sprite.SpriteHeight / 16, 2.0) + yJumpOffset - 8f) + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero) + new Vector2(0,-24), new Rectangle?(Sprite.SourceRect), color.Value * alpha, rotation, new Vector2(Sprite.SpriteWidth * 4 / 2, Sprite.SpriteHeight * 4 * 3f / 4f) / 4f, Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : (getStandingY() / 10000f)));

                if (holdingStar)
                {
                    b.Draw(Sprite.Texture, Game1.GlobalToLocal(Game1.viewport, Position + new Vector2(8f, -64f * scale + 4f + yJumpOffset)), new Rectangle?(new Rectangle(0, 109, 16, 19)), Color.White * alpha, 0f, Vector2.Zero, 4f * scale, SpriteEffects.None, Position.Y / 10000f + 0.0001f);
                    return;
                }
                if (holdingBundle)
                {
                    b.Draw(Sprite.Texture, Game1.GlobalToLocal(Game1.viewport, Position + new Vector2(8f, -64f * scale + 20f + yJumpOffset)), new Rectangle?(new Rectangle(0, 96, 16, 13)), bundleColor.Value * alpha, 0f, Vector2.Zero, 4f * scale, SpriteEffects.None, Position.Y / 10000f + 0.0001f);
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
