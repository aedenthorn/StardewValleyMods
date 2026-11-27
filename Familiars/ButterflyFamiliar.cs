using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Familiars
{
    public class ButterflyFamiliar : Familiar
    {
        public ButterflyFamiliar()
        {
        }


        public ButterflyFamiliar(Vector2 position, long _owner, bool existing = false) : base("Butterfly", position, new AnimatedSprite(ModEntry.Config.ButterflyTexture, 0, 16, 16))
        {
            Name = "ButterflyFamiliar";
            ownerId = _owner;
            flapSpeed = Game1.random.Next(35, 55);

            base.Slipperiness = 20 + Game1.random.Next(-5, 6);
            farmerPassesThrough = true;
            Halt();
            base.IsWalkingTowardPlayer = false;
            base.HideShadow = true;
            ignoreMovementAnimations = true;
            damageToFarmer.Value = 0;

            if (!existing)
            {

                if (Game1.random.NextDouble() < 0.5)
                {
                    baseFrame = (Game1.random.NextDouble() < 0.5) ? (Game1.random.Next(3) * 3 + 160) : (Game1.random.Next(3) * 3 + 180);
                }
                else
                {
                    baseFrame = (Game1.random.NextDouble() < 0.5) ? (Game1.random.Next(3) * 4 + 128) : (Game1.random.Next(3) * 4 + 148);
                    summerButterfly = true;
                }
                Sprite.currentFrame = baseFrame;
                sprite.Value.loop = false;

                if (ModEntry.Config.ButterflyColorType.ToLower() == "random")
                {
                    mainColor = new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
                    redColor = new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
                    greenColor = new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
                    blueColor = new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
                }
                else
                {
                    mainColor = ModEntry.Config.ButterflyMainColor;
                    redColor = ModEntry.Config.ButterflyRedColor;
                    greenColor = ModEntry.Config.ButterflyGreenColor;
                    blueColor = ModEntry.Config.ButterflyBlueColor;
                }
                reloadSprite();
            }
        }

        public override void reloadSprite()
        {
            if (Sprite == null)
            {
                Sprite = new AnimatedSprite(ModEntry.Config.ButterflyTexture, baseFrame, 16, 16);
            }
            else
            {
                Sprite.textureName.Value = ModEntry.Config.ButterflyTexture;
            }
            if (ModEntry.Config.DustColorType.ToLower() != "default")
            {
                Sprite.spriteTexture = FamiliarsUtils.ColorFamiliar(Sprite.Texture, mainColor, redColor, greenColor, blueColor);
            }
        }
        public override void draw(SpriteBatch b)
        {

        }
        public override void draw(SpriteBatch b, float alpha)
        {

        }

        public override void update(GameTime time, GameLocation location)
        {
            if (followingOwner || location.getTileIndexAt(getTileLocationPoint(), "Back") != -1)
                lastPosition = position;
            else
                position.Value = lastPosition;

            flapTimer -= time.ElapsedGameTime.Milliseconds;
            if (flapTimer <= 0 && sprite.Value.CurrentAnimation == null)
            {
                if (summerButterfly)
                {
                    Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                    {
                        new FarmerSprite.AnimationFrame(baseFrame + 1, flapSpeed),
                        new FarmerSprite.AnimationFrame(baseFrame + 2, flapSpeed),
                        new FarmerSprite.AnimationFrame(baseFrame + 3, flapSpeed),
                        new FarmerSprite.AnimationFrame(baseFrame + 2, flapSpeed),
                        new FarmerSprite.AnimationFrame(baseFrame + 1, flapSpeed),
                        new FarmerSprite.AnimationFrame(baseFrame, flapSpeed, false, false, new AnimatedSprite.endOfAnimationBehavior(doneWithFlap), false)
                    });
                }
                else
                {
                    Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                    {
                        new FarmerSprite.AnimationFrame(baseFrame + 1, (int)(flapSpeed * 1.5f)),
                        new FarmerSprite.AnimationFrame(baseFrame + 2, (int)(flapSpeed * 1.5f)),
                        new FarmerSprite.AnimationFrame(baseFrame + 1, (int)(flapSpeed * 1.5f)),
                        new FarmerSprite.AnimationFrame(baseFrame, (int)(flapSpeed * 1.5f), false, false, new AnimatedSprite.endOfAnimationBehavior(doneWithFlap), false)
                    });
                }
            }
            base.update(time, location);
        }
        protected override void updateMonsterSlaveAnimation(GameTime time)
        {
        }

        public override void drawAboveAllLayers(SpriteBatch b)
        {
            Sprite.currentFrame = Math.Max(Sprite.CurrentFrame, baseFrame);
            if(Sprite.currentAnimationIndex  > (summerButterfly ? 3 : 2))
            {
                position.Value += new Vector2(0, -4);
            }
            else
            {
                position.Value += new Vector2(0, 2);
            }
            Sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(-64f, -128f + yJumpOffset + yOffset)), position.Y / 10000f, 0, 0, Color.White, flip, 4f * scale, 0f, false);
        }

        public override Rectangle GetBoundingBox()
        {
            Vector2 pos = position + new Vector2(-64f, -128f + yJumpOffset + yOffset);
            return new Rectangle((int)pos.X - 16, (int)pos.Y - 16, 32, 32);
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            base.NetFields.AddFields(new INetSerializable[]
            {
            wasHitCounter,
            lastBuff,
            turningRight,
            seenPlayer,
            cursedDoll,
            hauntedSkull
            });
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
        }



        public override void behaviorAtGameTick(GameTime time)
        {
            invincibleCountdown = 1000;
            if (timeBeforeAIMovementAgain > 0f)
            {
                timeBeforeAIMovementAgain -= time.ElapsedGameTime.Milliseconds;
            }
            if (lastBuff >= 0)
            {
                lastBuff.Value -= time.ElapsedGameTime.Milliseconds;
            }

            if (wasHitCounter >= 0)
            {
                wasHitCounter.Value -= time.ElapsedGameTime.Milliseconds;
            }

            if (followingOwner)
            {

                Vector2 center = Position + new Vector2(8, 8);
                Vector2 playerCenter = GetOwner().position + new Vector2(64, 92);
                if (Vector2.Distance(playerCenter, center) > 256)
                {
                    Position = Vector2.Distance(playerCenter, center) * 0.03f * Vector2.Normalize(playerCenter - center) + center - new Vector2(8,8);

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
                if (lastBuff <= 0 && Vector2.Distance(GetOwner().getTileLocation(), getTileLocation()) < 3)
                {
                    if (Game1.random.NextDouble() < BuffChance())
                    {
                        if (ModEntry.Config.ButterflySoundEffects)
                            Game1.playSound("yoba");
                        BuffsDisplay buffsDisplay = Game1.buffsDisplay;
                        Buff buff2 = GetBuff();
                        buffsDisplay.addOtherBuff(buff2);
                        AddExp(1);
                        lastBuff.Value = GetBuffInterval();
                    }
                    else
                        lastBuff.Value = 1000;
                }
            }
        }

        private Buff GetBuff()
        {
            int buffAmount = (int)Math.Ceiling(Math.Sqrt(exp) / 10f);
            int which = Game1.random.Next(10);
            int[] buffs = new int[10];
            for(int i = 0; i < 10; i++)
            {
                buffs[i] = which == i ? buffAmount : 0;

            }

            Buff buff = new Buff(buffs[0], buffs[1], buffs[2], buffs[3], buffs[4], buffs[5], buffs[6], 0, 0, buffs[7], buffs[8], buffs[9], BuffDuration(), "ButterflyFamiliar", ModEntry.SHelper.Translation.Get("ButterflyFamiliar"));
            buff.which = 4200 + which;
            return buff;
        }

        private int BuffDuration()
        {
            return (int)Math.Sqrt(exp) * 10;
        }

        private int GetBuffInterval()
        {
            return (int)((10000 - (int)Math.Sqrt(exp)) * ModEntry.Config.ButterflyBuffIntervalMult);
        }

        private int Buff()
        {
            return (int)Math.Ceiling(Math.Sqrt(exp / 10f));
        }

        private double BuffChance()
        {
            return (0.01 + Math.Sqrt(exp) * 0.001) * ModEntry.Config.ButterflyBuffChanceMult;
        }

        private Farmer GetOwner()
        {
            return Game1.getFarmer(ownerId);
        }

        private void AddExp(int v)
        {
            exp.Value += v;
        }

        protected override void updateAnimation(GameTime time)
        {
        }
        public void doneWithFlap(Farmer who)
        {
            flapTimer = 50 + Game1.random.Next(-5, 6);
        }
        private readonly NetInt wasHitCounter = new NetInt(0);

        private readonly NetInt lastBuff = new NetInt(0);

        private float targetRotation;

        private readonly NetBool turningRight = new NetBool();

        private readonly NetBool seenPlayer = new NetBool();

        private readonly NetBool cursedDoll = new NetBool();

        private readonly NetBool hauntedSkull = new NetBool();


        private float extraVelocity;

        private float maxSpeed = 1.5f;

        public Monster currentTarget = null;
        private int flapTimer;
        private int flapSpeed = 50;
        private bool summerButterfly;
        public bool stayInbounds;
    }
}
