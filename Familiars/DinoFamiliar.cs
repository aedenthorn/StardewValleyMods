using Familiars;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StardewValley.Monsters
{
    public class DinoFamiliar : Familiar
    {

        public Monster currentTarget = null;
        private bool chargingMonster;
        private Color color;
        public int timeUntilNextAttack;

        protected bool _hasPlayedFireSound;

        public readonly NetBool firing = new NetBool(false);

        public NetInt attackState = new NetInt();

        public int nextFireTime;

        public int totalFireTime;

        public int nextChangeDirectionTime;

        public int nextWanderTime;

        public bool wanderState;


        public enum AttackState
        {
            None,
            Fireball,
            Charge
        }

        public DinoFamiliar()
        {
        }

        public DinoFamiliar(Vector2 position, long owner) : base("Pepper Rex", position, new AnimatedSprite(ModEntry.Config.DinoTexture))
        {
            Name = "DinoFamiliar";
            ownerId = owner;
            Sprite.SpriteWidth = 32;
            Sprite.SpriteHeight = 32;
            Sprite.UpdateSourceRect();
            timeUntilNextAttack = 2000;
            nextChangeDirectionTime = Game1.random.Next(1000, 3000);
            nextWanderTime = Game1.random.Next(1000, 2000);
            damageToFarmer.Value = 0;
            farmerPassesThrough = true;

            if (ModEntry.Config.DinoColorType.ToLower() == "random")
            {
                mainColor = new Color(Game1.random.Next(256),Game1.random.Next(256),Game1.random.Next(256));
                redColor = new Color(Game1.random.Next(256),Game1.random.Next(256),Game1.random.Next(256));
                greenColor = new Color(Game1.random.Next(256),Game1.random.Next(256),Game1.random.Next(256));
                blueColor = new Color(Game1.random.Next(256),Game1.random.Next(256),Game1.random.Next(256));
            }
            else 
            {
                mainColor = ModEntry.Config.DinoMainColor;
                redColor = ModEntry.Config.DinoRedColor;
                greenColor = ModEntry.Config.DinoGreenColor;
                blueColor = ModEntry.Config.DinoBlueColor;
            }

            reloadSprite();
        }

        protected override void initNetFields()
        {
            NetFields.AddFields(new INetSerializable[]
            {
                attackState,
                firing
            });
            base.initNetFields();
        }

        public override void reloadSprite()
        {
            if (Sprite == null)
            {
                Sprite = new AnimatedSprite(ModEntry.Config.DinoTexture);
            }
            else
            {
                Sprite.textureName.Value = ModEntry.Config.DinoTexture;
            }
            if (ModEntry.Config.DinoColorType.ToLower() != "default")
            {
                Sprite.spriteTexture = FamiliarsUtils.ColorFamiliar(Sprite.Texture, mainColor, redColor, greenColor, blueColor);
            }
            Sprite.SpriteWidth = 32;
            Sprite.SpriteHeight = 32;
            Sprite.UpdateSourceRect();
            HideShadow = true;
        }
        public override void draw(SpriteBatch b)
        {
            if (!IsInvisible && Utility.isOnScreen(Position, 128))
            {
                b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(56f, (float)(16 + yJumpOffset)), new Rectangle?(Sprite.SourceRect), Color.White, rotation, new Vector2(16f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
                if (isGlowing)
                {
                    b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(56f, (float)(16 + yJumpOffset)), new Rectangle?(Sprite.SourceRect), glowingColor * glowingTransparency, 0f, new Vector2(16f, 16f), 4f * Math.Max(0.2f, scale), flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f + 0.001f)));
                }
            }
        }
        public override Rectangle GetBoundingBox()
        {
            Rectangle baseRect = new Rectangle((int)Position.X + 8, (int)Position.Y, Sprite.SpriteWidth * 4 * 3 / 4, 64);
            if(scale >= 1)
                return baseRect;
            return new Rectangle((int)baseRect.Center.X - (int)(Sprite.SpriteWidth * 4 * 3 / 4 * scale) / 2, (int)(baseRect.Center.Y - 32 * scale), (int)(Sprite.SpriteWidth * 4 * 3 / 4 * scale), (int)(64*scale));
        }

        public override List<Item> getExtraDropItems()
        {
            List<Item> extra_items = new List<Item>();
            if (Game1.random.NextDouble() < 0.10000000149011612)
            {
                extra_items.Add(new Object(107, 1, false, -1, 0));
            }
            else
            {
                extra_items.Add(Utility.GetRandom<Item>(new List<Item>
                {
                    new Object(580, 1, false, -1, 0),
                    new Object(583, 1, false, -1, 0),
                    new Object(584, 1, false, -1, 0)
                }, null));
            }
            return extra_items;
        }

        protected override void sharedDeathAnimation()
        {
            currentLocation.playSound("skeletonDie", NetAudio.SoundContext.Default);
            currentLocation.playSound("grunt", NetAudio.SoundContext.Default);
            for (int i = 0; i < 16; i++)
            {
                Game1.createRadialDebris(currentLocation, Sprite.textureName, new Rectangle(64, 128, 16, 16), 16, (int)Utility.Lerp((float)GetBoundingBox().Left, (float)GetBoundingBox().Right, (float)Game1.random.NextDouble()), (int)Utility.Lerp((float)GetBoundingBox().Bottom, (float)GetBoundingBox().Top, (float)Game1.random.NextDouble()), 1, (int)getTileLocation().Y, Color.White, 4f);
            }
        }

        protected override void localDeathAnimation()
        {
            Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, Position, Color.HotPink, 10, false, 100f, 0, -1, -1f, -1, 0)
            {
                holdLastFrame = true,
                alphaFade = 0.01f,
                interval = 70f
            }, currentLocation, 8, 96, 64);
        }

        public override void behaviorAtGameTick(GameTime time)
        {
            invincibleCountdown = 1000;

            chargingMonster = false;
            if(!(currentLocation is SlimeHutch))
            {
                foreach (NPC npc in currentLocation.characters)
                {
                    if (npc is Familiar)
                        continue;

                    if (npc is Monster && FamiliarsUtils.withinMonsterThreshold(this, (Monster)npc, 2))
                    {
                        chargingMonster = true;
                        if (currentTarget == null || Vector2.Distance(npc.position, position) < Vector2.Distance(currentTarget.position, position))
                        {
                            currentTarget = (Monster)npc;
                        }
                    }
                }
            }

            if (attackState.Value == 1)
            {
                IsWalkingTowardPlayer = false;
                Halt();
            }
            else if (withinPlayerThreshold() && followingOwner)
            {
                if (withinPlayerThreshold(1))
                    Halt();
                else
                    IsWalkingTowardPlayer = true;
            }
            else
            {
                IsWalkingTowardPlayer = false;
                nextChangeDirectionTime -= time.ElapsedGameTime.Milliseconds;
                nextWanderTime -= time.ElapsedGameTime.Milliseconds;
                if (nextChangeDirectionTime < 0)
                {
                    nextChangeDirectionTime = Game1.random.Next(500, 1000);
                    facingDirection.Value = (facingDirection.Value + (Game1.random.Next(0, 3) - 1) + 4) % 4;
                }
                if (nextWanderTime < 0)
                {
                    if (wanderState)
                    {
                        nextWanderTime = Game1.random.Next(1000, 2000);
                    }
                    else
                    {
                        nextWanderTime = Game1.random.Next(1000, 3000);
                    }
                    wanderState = !wanderState;
                }
                if (wanderState)
                {
                    moveLeft = (moveUp = (moveRight = (moveDown = false)));
                    tryToMoveInDirection(facingDirection.Value, false, DamageToFarmer, isGlider);
                }
            }
            timeUntilNextAttack -= time.ElapsedGameTime.Milliseconds;
            if (attackState.Value == 0 && chargingMonster)
            {
                firing.Set(false);
                if (timeUntilNextAttack < 0)
                {
                    timeUntilNextAttack = 0;
                    attackState.Set(1);
                    nextFireTime = 500;
                    totalFireTime = 3000;
                    if (ModEntry.Config.DinoSoundEffects)
                        currentLocation.playSound("croak", NetAudio.SoundContext.Default);
                    return;
                }
            }
            else if (totalFireTime > 0)
            {
                if (!firing)
                {
                    if (currentTarget != null)
                    {
                        faceGeneralDirection(currentTarget.Position, 0, false);
                    }
                }
                totalFireTime -= time.ElapsedGameTime.Milliseconds;
                if (nextFireTime > 0)
                {
                    nextFireTime -= time.ElapsedGameTime.Milliseconds;
                    if (nextFireTime <= 0)
                    {
                        if (!firing.Value)
                        {
                            firing.Set(true);
                            currentLocation.playSound("furnace", NetAudio.SoundContext.Default);
                        }
                        float fire_angle = 0f;
                        Vector2 shot_origin = new Vector2((float)GetBoundingBox().Center.X - 32f * scale, (float)GetBoundingBox().Center.Y - 32f * scale);
                        switch (facingDirection.Value)
                        {
                            case 0:
                                yVelocity = -1f;
                                shot_origin.Y -= 74f * scale;
                                fire_angle = 90f;
                                break;
                            case 1:
                                xVelocity = -1f;
                                shot_origin.X += 74f * scale;
                                fire_angle = 0f;
                                break;
                            case 2:
                                yVelocity = 1f;
                                fire_angle = 270f;
                                shot_origin.Y += 74 * scale;
                                break;
                            case 3:
                                xVelocity = 1f;
                                shot_origin.X -= 74f * scale;
                                fire_angle = 180f;
                                break;
                        }
                        fire_angle += (float)Math.Sin((double)((float)totalFireTime / 1000f * 180f) * 3.1415926535897931 / 180.0) * 25f;
                        Vector2 shot_velocity = new Vector2((float)Math.Cos((double)fire_angle * 3.1415926535897931 / 180.0), -(float)Math.Sin((double)fire_angle * 3.1415926535897931 / 180.0));
                        shot_velocity *= 10f;
                        BasicProjectile projectile = new BasicProjectile(GetDamage(), 10, 0, 1, 0.196349546f, shot_velocity.X, shot_velocity.Y, shot_origin, "", "", false, true, currentLocation, this, false, null);
                        projectile.ignoreTravelGracePeriod.Value = true;
                        projectile.maxTravelDistance.Value = GetFireDistance();
                        currentLocation.projectiles.Add(projectile);
                        nextFireTime = 50;
                    }
                }
                if (totalFireTime <= 0)
                {
                    AddExp(1);
                    totalFireTime = 0;
                    nextFireTime = 0;
                    attackState.Set(0);
                    timeUntilNextAttack = Game1.random.Next(1000, 2000);
                }
            }
        }

        private int GetFireDistance()
        {
            return (int)(Math.Sqrt(exp) * ModEntry.Config.DinoFireDistanceMult);
        }

        private int GetDamage()
        {
            return (int)(Math.Sqrt(exp) * ModEntry.Config.DinoDamageMult);
        }

        private void AddExp(int v)
        {
            exp.Value += v;
        }
        protected override void updateAnimation(GameTime time)
        {
            int direction_offset = 0;
            if (FacingDirection == 2)
            {
                direction_offset = 0;
            }
            else if (FacingDirection == 1)
            {
                direction_offset = 4;
            }
            else if (FacingDirection == 0)
            {
                direction_offset = 8;
            }
            else if (FacingDirection == 3)
            {
                direction_offset = 12;
            }
            if (attackState.Value != 1)
            {
                if (isMoving() || wanderState)
                {
                    if (FacingDirection == 0)
                    {
                        Sprite.AnimateUp(time, 0, "");
                        return;
                    }
                    if (FacingDirection == 3)
                    {
                        Sprite.AnimateLeft(time, 0, "");
                        return;
                    }
                    if (FacingDirection == 1)
                    {
                        Sprite.AnimateRight(time, 0, "");
                        return;
                    }
                    if (FacingDirection == 2)
                    {
                        Sprite.AnimateDown(time, 0, "");
                        return;
                    }
                }
                else
                {
                    if (FacingDirection == 0)
                    {
                        Sprite.AnimateUp(time, 0, "");
                    }
                    else if (FacingDirection == 3)
                    {
                        Sprite.AnimateLeft(time, 0, "");
                    }
                    else if (FacingDirection == 1)
                    {
                        Sprite.AnimateRight(time, 0, "");
                    }
                    else if (FacingDirection == 2)
                    {
                        Sprite.AnimateDown(time, 0, "");
                    }
                    Sprite.StopAnimation();
                }
                return;
            }
            if (firing.Value)
            {
                Sprite.CurrentFrame = 16 + direction_offset;
                return;
            }
            Sprite.CurrentFrame = 17 + direction_offset;
        }

        protected override void updateMonsterSlaveAnimation(GameTime time)
        {
            int direction_offset = 0;
            if (FacingDirection == 2)
            {
                direction_offset = 0;
            }
            else if (FacingDirection == 1)
            {
                direction_offset = 4;
            }
            else if (FacingDirection == 0)
            {
                direction_offset = 8;
            }
            else if (FacingDirection == 3)
            {
                direction_offset = 12;
            }
            if (attackState.Value != 1)
            {
                if (isMoving())
                {
                    if (FacingDirection == 0)
                    {
                        Sprite.AnimateUp(time, 0, "");
                        return;
                    }
                    if (FacingDirection == 3)
                    {
                        Sprite.AnimateLeft(time, 0, "");
                        return;
                    }
                    if (FacingDirection == 1)
                    {
                        Sprite.AnimateRight(time, 0, "");
                        return;
                    }
                    if (FacingDirection == 2)
                    {
                        Sprite.AnimateDown(time, 0, "");
                        return;
                    }
                }
                else
                {
                    Sprite.StopAnimation();
                }
                return;
            }
            if (firing.Value)
            {
                Sprite.CurrentFrame = 16 + direction_offset;
                return;
            }
            Sprite.CurrentFrame = 17 + direction_offset;
        }
        public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
        {
            if (who != null)
            {
                return 0;
            }
            return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
        }
        public override bool withinPlayerThreshold()
        {
            if (currentLocation != null && !currentLocation.farmers.Any())
            {
                return false;
            }
            Vector2 tileLocationOfPlayer = GetOwner().getTileLocation();
            Vector2 tileLocationOfMonster = getTileLocation();
            return Math.Abs(tileLocationOfMonster.X - tileLocationOfPlayer.X) <= (float)moveTowardPlayerThreshold && Math.Abs(tileLocationOfMonster.Y - tileLocationOfPlayer.Y) <= (float)moveTowardPlayerThreshold;
        }
        private Farmer GetOwner()
        {
            return Game1.getFarmer(ownerId);
        }

    }
}
