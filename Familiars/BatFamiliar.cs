using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Familiars
{
    public class BatFamiliar : Familiar
    {
        public BatFamiliar()
        {
        }

        public BatFamiliar(Vector2 position, long _owner) : base("Bat", position, new AnimatedSprite(ModEntry.Config.BatTexture))
        {
            Name = "BatFamiliar";
            ownerId = _owner;

            Slipperiness = 20 + Game1.random.Next(-5, 6);
            farmerPassesThrough = true;
            Halt();
            IsWalkingTowardPlayer = false;
            HideShadow = true;
            damageToFarmer.Value = 0;

            if (ModEntry.Config.BatColorType.ToLower() == "random")
            {
                mainColor = new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
                redColor = new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
                greenColor = new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
                blueColor = new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
            }
            else
            {
                mainColor = ModEntry.Config.BatMainColor;
                redColor = ModEntry.Config.BatRedColor;
                greenColor = ModEntry.Config.BatGreenColor;
                blueColor = ModEntry.Config.BatBlueColor;
            }

            reloadSprite();
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields(new INetSerializable[]
            {
                wasHitCounter,
                lastHitCounter,
                lastScreechCounter,
                turningRight,
                seenPlayer,
                cursedDoll,
                hauntedSkull
            });
        }


        public override void reloadSprite()
        {
            ModEntry.SMonitor.Log($"reloading bat familiar sprite for {Name} {ModEntry.Config.BatTexture}");

            if (Sprite == null)
            {
                ModEntry.SMonitor.Log($"creating new sprite");
                Sprite = new AnimatedSprite(ModEntry.Config.BatTexture);
            }
            else
            {
                ModEntry.SMonitor.Log($"updating sprite texture");
                Sprite.textureName.Value = ModEntry.Config.BatTexture;
            }
            if (ModEntry.Config.BatColorType.ToLower() != "default")
            {
                Sprite.spriteTexture = FamiliarsUtils.ColorFamiliar(Sprite.Texture, mainColor, redColor, greenColor, blueColor);
            }
            HideShadow = true;
        }

        public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
        {
            if (who != null)
            {
                return 0;
            }
            return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
        }

        public override void shedChunks(int number, float scale)
        {
            Game1.createRadialDebris(currentLocation, Sprite.textureName, new Rectangle(0, 384, 64, 64), 32, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)getTileLocation().Y, Color.White, scale);
        }
        public override void drawAboveAllLayers(SpriteBatch b)
        {
            if (Utility.isOnScreen(Position, 128))
            {

                b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 32f), new Rectangle?(Sprite.SourceRect), (shakeTimer > 0) ? Color.Red : Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.92f);
                b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f * scale, SpriteEffects.None, wildernessFarmMonster ? 0.0001f : ((float)(getStandingY() - 1) / 10000f));
                if (isGlowing)
                {
                    b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 32f), new Rectangle?(Sprite.SourceRect), glowingColor * glowingTransparency, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.99f : ((float)getStandingY() / 10000f + 0.001f)));
                }
            }
        }

        public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
        {
            base.drawAboveAlwaysFrontLayer(b);
        }
        public override void behaviorAtGameTick(GameTime time)
        {
            invincibleCountdown = 1000;
            if (timeBeforeAIMovementAgain > 0f)
            {
                timeBeforeAIMovementAgain -= (float)time.ElapsedGameTime.Milliseconds;
            }
            if (lastHitCounter >= 0)
            {
                lastHitCounter.Value -= time.ElapsedGameTime.Milliseconds;
            }
            if (lastScreechCounter >= 0)
            {
                lastScreechCounter.Value -= time.ElapsedGameTime.Milliseconds;
            }
            if (lastScreechCounter < 0 && GetBoundingBox().Intersects(GetOwner().GetBoundingBox()))
            {
                if(ModEntry.Config.BatSoundEffects)
                    currentLocation.playSound("batScreech", NetAudio.SoundContext.Default);
                lastScreechCounter.Value = 10000;
            }

            chargingMonster = false;
            if(lastHitCounter < 0 && !(currentLocation is SlimeHutch))
            {
                foreach (NPC npc in currentLocation.characters)
                {
                    if (npc is Familiar)
                        continue;

                    if (npc is Monster && FamiliarsUtils.monstersColliding(this, (Monster)npc))
                    {
                        if (BaseDamage() >= 0)
                        {
                            int damageAmount = Game1.random.Next(BaseDamage(), BaseDamage() * 2 + 1);
                            damageAmount = (npc as Monster).takeDamage(damageAmount, 0, 0, false, 0, GetOwner());
                            if((npc as Monster).Health <= 0)
                            {
                                AddExp(10);
                            }
                            else
                            {
                                AddExp(1);
                            }
                        }
                        lastHitCounter.Value = AttackInterval();
                        chargingMonster = false;
                        break;
                    }
                    else if (npc is Monster && FamiliarsUtils.withinMonsterThreshold(this, (Monster)npc, 2))
                    {
                        chargingMonster = true;
                        if (currentTarget == null || Vector2.Distance(npc.position, position) < Vector2.Distance(currentTarget.position, position))
                        {
                            currentTarget = (Monster)npc;
                        }
                    }
                }
            }

            if (wasHitCounter >= 0)
            {
                wasHitCounter.Value -= time.ElapsedGameTime.Milliseconds;
            }

            if (chargingMonster || followingOwner)
            {
                seenPlayer.Value = true;

                Vector2 center = Position + new Vector2(8, 8);
                Vector2 playerCenter = GetOwner().position + new Vector2(64, 92);
                if (Vector2.Distance(playerCenter, center) > 256)
                {
                    Position = Vector2.Distance(playerCenter, center) * 0.03f * Vector2.Normalize(playerCenter - center) + center - new Vector2(8, 8);

                }

                float xSlope = (float)(-(float)(playerCenter.X - center.X));
                float ySlope = (float)(playerCenter.Y - center.Y);
                float t = Math.Max(1f, Math.Abs(xSlope) + Math.Abs(ySlope));
                if (t < (float)((extraVelocity > 0f) ? 192 : 64))
                {
                    xVelocity = Math.Max(-maxSpeed, Math.Min(maxSpeed, xVelocity * 1.05f));
                    yVelocity = Math.Max(-maxSpeed, Math.Min(maxSpeed, yVelocity * 1.05f));
                }
                xSlope /= t;
                ySlope /= t;
                if (wasHitCounter <= 0)
                {
                    targetRotation = (float)Math.Atan2((double)(-(double)ySlope), (double)xSlope) - 1.57079637f;
                    if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) > 2.748893571891069 && Game1.random.NextDouble() < 0.5)
                    {
                        turningRight.Value = true;
                    }
                    else if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) < 0.39269908169872414)
                    {
                        turningRight.Value = false;
                    }
                    if (turningRight)
                    {
                        rotation -= (float)Math.Sign(targetRotation - rotation) * 0.0490873866f;
                    }
                    else
                    {
                        rotation += (float)Math.Sign(targetRotation - rotation) * 0.0490873866f;
                    }
                    rotation %= 6.28318548f;
                    wasHitCounter.Value = 0;
                }
                float maxAccel = Math.Min(5f, Math.Max(1f, 5f - t / 64f / 2f)) + extraVelocity;
                xSlope = (float)Math.Cos((double)rotation + 1.5707963267948966);
                ySlope = -(float)Math.Sin((double)rotation + 1.5707963267948966);
                xVelocity += -xSlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
                yVelocity += -ySlope * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
                if (Math.Abs(xVelocity) > Math.Abs(-xSlope * maxSpeed))
                {
                    xVelocity -= -xSlope * maxAccel / 6f;
                }
                if (Math.Abs(yVelocity) > Math.Abs(-ySlope * maxSpeed))
                {
                    yVelocity -= -ySlope * maxAccel / 6f;
                }
            }
        }

        private Farmer GetOwner()
        {
            return Game1.getFarmer(ownerId);
        }

        private void AddExp(int v)
        {
            exp.Value += v;
        }

        private int AttackInterval()
        {
            return (int)(Math.Max(500, 5000 - (int)Math.Sqrt(exp)) * ModEntry.Config.BatAttackIntervalMult);
        }

        private int BaseDamage()
        {
            return (int)(Math.Sqrt(exp) * ModEntry.Config.BatDamageMult);
        }

        protected override void updateAnimation(GameTime time)
        {
            if (followingOwner)
            {
                Sprite.Animate(time, 0, 4, 80f);
                if (Sprite.currentFrame % 3 == 0 && Utility.isOnScreen(Position, 512) && (batFlap == null || !batFlap.IsPlaying) && Game1.soundBank != null && currentLocation == Game1.currentLocation && !cursedDoll)
                {
                    batFlap = Game1.soundBank.GetCue("batFlap");
                    //batFlap.Play();
                }
                if (cursedDoll.Value)
                {
                    shakeTimer -= time.ElapsedGameTime.Milliseconds;
                    if (shakeTimer < 0)
                    {
                        if (!hauntedSkull.Value)
                        {
                            currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 103, 16, 16), position + new Vector2(0f, -32f), false, 0.1f, new Color(255, 50, 255) * 0.8f)
                            {
                                scale = 4f
                            });
                        }
                        shakeTimer = 50;
                    }
                    previousPositions.Add(Position);
                    if (previousPositions.Count > 8)
                    {
                        previousPositions.RemoveAt(0);
                    }
                }
            }
            else
            {
                Sprite.currentFrame = 4;
                Halt();
            }
            resetAnimationSpeed();
        }

        private readonly NetInt wasHitCounter = new NetInt(0);

        private readonly NetInt lastHitCounter = new NetInt(0);
        private readonly NetInt lastScreechCounter = new NetInt(0);

        private float targetRotation;

        private readonly NetBool turningRight = new NetBool();

        private readonly NetBool seenPlayer = new NetBool();

        private readonly NetBool cursedDoll = new NetBool();

        private readonly NetBool hauntedSkull = new NetBool();

        private ICue batFlap;

        private float extraVelocity;

        private float maxSpeed = 5f;

        private List<Vector2> previousPositions = new List<Vector2>();
        public Monster currentTarget = null;
        private bool chargingMonster;
    }
}
