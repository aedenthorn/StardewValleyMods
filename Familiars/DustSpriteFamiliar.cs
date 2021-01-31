using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using System;
using System.Reflection;

namespace Familiars
{
    public class DustSpriteFamiliar : Familiar
    {
        public DustSpriteFamiliar()
        {
        }


        public DustSpriteFamiliar(Vector2 position, long owner) : base("Dust Spirit", position, new AnimatedSprite(ModEntry.Config.DustTexture))
        {
            Name = "DustSpriteFamiliar";
            ModEntry.SMonitor.Log($"DSF Name: {Name}"); 
            ownerId = owner;
            IsWalkingTowardPlayer = false;
            Sprite.interval = 45f;
            voice = (byte)Game1.random.Next(1, 24);
            HideShadow = true;
            DamageToFarmer = 0;
            farmerPassesThrough = true;
            willDestroyObjectsUnderfoot = false;

            if (ModEntry.Config.DustColorType.ToLower() == "random")
            {
                mainColor = new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
                redColor = new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
                greenColor = new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
                blueColor = new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
            }
            else
            {
                mainColor = ModEntry.Config.DustMainColor;
                redColor = ModEntry.Config.DustRedColor;
                greenColor = ModEntry.Config.DustGreenColor;
                blueColor = ModEntry.Config.DustBlueColor;
            }
            reloadSprite();
        }

        public override void reloadSprite()
        {
            if (Sprite == null)
            {
                Sprite = new AnimatedSprite(ModEntry.Config.DustTexture);
            }
            else
            {
                Sprite.textureName.Value = ModEntry.Config.DustTexture;
            }
            if (ModEntry.Config.DustColorType.ToLower() != "default")
            {
                Sprite.spriteTexture = FamiliarsUtils.ColorFamiliar(Sprite.Texture, mainColor, redColor, greenColor, blueColor);
            }
        }
        public override void draw(SpriteBatch b)
        {
            if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
            {
                b.Draw(Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(64 + yJumpOffset)), new Rectangle?(Sprite.SourceRect), Color.White, rotation, new Vector2(8f, 16f), new Vector2(scale + (float)Math.Max(-0.1, (double)(yJumpOffset + 32) / 128.0), scale - Math.Max(-0.1f, (float)yJumpOffset / 256f)) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)base.getStandingY() / 10000f)));
                if (isGlowing)
                {
                    b.Draw(Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(64 + yJumpOffset)), new Rectangle?(Sprite.SourceRect), glowingColor * glowingTransparency, rotation, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.99f : ((float)base.getStandingY() / 10000f + 0.001f)));
                }
                b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 80f), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), (4f + (float)yJumpOffset / 64f) * scale, SpriteEffects.None, (float)(base.getStandingY() - 1) / 10000f);
            }
        }
        protected override void sharedDeathAnimation()
        {
        }
        protected override void localDeathAnimation()
        {
            if (ModEntry.Config.DustSoundEffects)
                currentLocation.localSound("dustMeep");
            base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position, new Color(50, 50, 80), 10, false, 100f, 0, -1, -1f, -1, 0));
            base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-32, 32)), new Color(50, 50, 80), 10, false, 100f, 0, -1, -1f, -1, 0)
            {
                delayBeforeAnimationStart = 150,
                scale = 0.5f
            });
            base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-32, 32)), new Color(50, 50, 80), 10, false, 100f, 0, -1, -1f, -1, 0)
            {
                delayBeforeAnimationStart = 300,
                scale = 0.5f
            });
            base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2((float)Game1.random.Next(-32, 32), (float)Game1.random.Next(-32, 32)), new Color(50, 50, 80), 10, false, 100f, 0, -1, -1f, -1, 0)
            {
                delayBeforeAnimationStart = 450,
                scale = 0.5f
            });
        }
        public override void shedChunks(int number, float scale)
        {
            Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(0, 16, 16, 16), 8, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)base.getTileLocation().Y, Color.White, (base.Health <= 0) ? 4f : 2f);
        }


        public void offScreenBehavior(Character c, GameLocation l)
        {
        }
        protected override void updateAnimation(GameTime time)
        {
            Sprite.AnimateDown(time, 0, "");
            if (yJumpOffset == 0)
            {
                jumpWithoutSound((int)(5 + Game1.random.Next(1, 4) * scale));
                yJumpVelocity = (int)(5 + Game1.random.Next(1, 4) * scale);
                if (Game1.random.NextDouble() < 0.1 && (meep == null || !meep.IsPlaying) && Utility.isOnScreen(base.Position, 64) && Game1.soundBank != null && Game1.currentLocation == base.currentLocation)
                {
                    if (ModEntry.Config.DustSoundEffects)
                    {
                        meep = Game1.soundBank.GetCue("dustMeep");
                        meep.SetVariable("Pitch", (int)(voice * 100) + Game1.random.Next(-100, 100));
                        meep.Play();
                    }
                }
            }
            base.resetAnimationSpeed();
        }

        public override void behaviorAtGameTick(GameTime time)
        {
            invincibleCountdown = 1000;

            if (timeBeforeAIMovementAgain > 0f)
            {
                timeBeforeAIMovementAgain -= (float)time.ElapsedGameTime.Milliseconds;
            }

            if (yJumpOffset == 0)
            {
                if (Game1.random.NextDouble() < DestroyRockChance())
                {
                    ModEntry.SHelper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue().broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite[]
                    {
                        new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 128, 64, 64), 40f, 4, 0, base.getStandingPosition(), false, false)
                    });
                    foreach (Vector2 v in Utility.getAdjacentTileLocations(base.getTileLocation()))
                    {
                        if (currentLocation is MineShaft && currentLocation.objects.ContainsKey(v) && base.currentLocation.objects[v].Name.Equals("Stone"))
                        {
                            AddExp(1);
                            currentLocation.destroyObject(v, null);
                        }
                    }
                    yJumpVelocity *= 2f;
                }
                if (!chargingFarmer && !chargingMonster)
                {
                    xVelocity = (float)Game1.random.Next(-20, 21) / 5f;
                }
            }
            if (lastHitCounter >= 0)
            {
                lastHitCounter.Value -= time.ElapsedGameTime.Milliseconds;
            }

            chargingMonster = false;
            if(lastHitCounter < 0)
            {
                foreach (NPC npc in currentLocation.characters)
                {
                    if (npc is Familiar)
                        continue;
                    if (npc is Monster && FamiliarsUtils.monstersColliding(this, (Monster)npc) && Game1.random.NextDouble() < StealChance())
                    {
                        ModEntry.SMonitor.Log("Stealing loot");
                        FamiliarsUtils.monsterDrop(this, (Monster)npc, GetOwner());
                        lastHitCounter.Value = StealInterval();
                        chargingMonster = false;
                        AddExp(1);
                        break;
                    }
                    else if (npc is Monster && FamiliarsUtils.withinMonsterThreshold(this, (Monster)npc, 5))
                    {
                        chargingMonster = true;
                        if (currentTarget == null || Vector2.Distance(npc.position, position) < Vector2.Distance(currentTarget.position, position))
                        {
                            currentTarget = (Monster)npc;
                        }
                    }
                }
            }

            if (chargingMonster && currentTarget != null)
            {
                base.Slipperiness = 10;

                Vector2 v2 = FamiliarsUtils.getAwayFromNPCTrajectory(GetBoundingBox(), currentTarget);
                xVelocity += -v2.X / 150f + ((Game1.random.NextDouble() < 0.01) ? ((float)Game1.random.Next(-50, 50) / 10f) : 0f);
                if (Math.Abs(xVelocity) > 5f)
                {
                    xVelocity = (float)(Math.Sign(xVelocity) * 5);
                }
                yVelocity += -v2.Y / 150f + ((Game1.random.NextDouble() < 0.01) ? ((float)Game1.random.Next(-50, 50) / 10f) : 0f);
                if (Math.Abs(yVelocity) > 5f)
                {
                    yVelocity = (float)(Math.Sign(yVelocity) * 5);
                }
                return;
            }

            chargingFarmer = false;
            if (!followingOwner)
                return;
            if (!seenFarmer && withinPlayerThreshold())
            {
                seenFarmer = true;
                return;
            }
            if (seenFarmer && controller == null && !runningAwayFromFarmer)
            {
                base.addedSpeed = 2;
                controller = new PathFindController(this, base.currentLocation, new PathFindController.isAtEnd(Utility.isOffScreenEndFunction), -1, false, new PathFindController.endBehavior(offScreenBehavior), 350, Point.Zero, true);
                runningAwayFromFarmer = true;
                return;
            }
            if (controller == null && runningAwayFromFarmer)
            {
                chargingFarmer = true;
            }

            if (chargingFarmer)
            {
                base.Slipperiness = 10;
                Vector2 v2 = Utility.getAwayFromPlayerTrajectory(GetBoundingBox(), GetOwner());
                xVelocity += -v2.X / 150f + ((Game1.random.NextDouble() < 0.01) ? ((float)Game1.random.Next(-50, 50) / 10f) : 0f);
                if (Math.Abs(xVelocity) > 5f)
                {
                    xVelocity = (float)(Math.Sign(xVelocity) * 5);
                }
                yVelocity += -v2.Y / 150f + ((Game1.random.NextDouble() < 0.01) ? ((float)Game1.random.Next(-50, 50) / 10f) : 0f);
                if (Math.Abs(yVelocity) > 5f)
                {
                    yVelocity = (float)(Math.Sign(yVelocity) * 5);
                }
                if (Game1.random.NextDouble() < 0.0001)
                {
                    controller = new PathFindController(this, base.currentLocation, new Point((int)GetOwner().getTileLocation().X, (int)GetOwner().getTileLocation().Y), Game1.random.Next(4), null, 300);
                    chargingFarmer = false;
                    return;
                }
            }
        }
        public override Rectangle GetBoundingBox()
        {
            Vector2 pos = position + new Vector2(0, (float)(32 + yJumpOffset));
            Rectangle baseRect = new Rectangle((int)pos.X, (int)pos.Y, Sprite.SpriteWidth * 4 * 3 / 4, 32);
            if (scale >= 1)
                return baseRect;
            return new Rectangle((int)baseRect.Center.X - (int)(Sprite.SpriteWidth * 4 * 3 / 4 * scale) / 2, (int)(baseRect.Center.Y - 16 * scale), (int)(Sprite.SpriteWidth * 4 * 3 / 4 * scale), (int)(32 * scale));
        }

        private double DestroyRockChance()
        {
            return 0.01 + (0.001 * Math.Sqrt(exp));
        }

        private int StealInterval()
        {
            return 10000 - exp;
        }

        private double StealChance()
        {
            return 0.001 + (0.001 * Math.Sqrt(exp)) * ModEntry.Config.DustSpriteStealChanceMult;
        }

        private void AddExp(int v)
        {
            exp.Value += v;
        }
        public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
        {
            if(who != null)
            {
                return 0;
            }
            return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
        }

        public override bool withinPlayerThreshold()
        {
            if (base.currentLocation != null && !base.currentLocation.farmers.Any())
            {
                return false;
            }
            Vector2 tileLocationOfPlayer = GetOwner().getTileLocation();
            Vector2 tileLocationOfMonster = base.getTileLocation();
            return Math.Abs(tileLocationOfMonster.X - tileLocationOfPlayer.X) <= (float)moveTowardPlayerThreshold && Math.Abs(tileLocationOfMonster.Y - tileLocationOfPlayer.Y) <= (float)moveTowardPlayerThreshold;
        }
        private Farmer GetOwner()
        {
            return Game1.getFarmer(ownerId);
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields(new INetSerializable[]
            {
                lastHitCounter,
            });
        }
        private readonly NetInt lastHitCounter = new NetInt(0);

        public Monster currentTarget = null;
        private bool chargingMonster;
        private Color color;
        private bool seenFarmer;
        private bool runningAwayFromFarmer;
        private bool chargingFarmer;
        private ICue meep;
        public byte voice;

    }
}
