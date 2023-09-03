using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;

namespace BossCreatures
{
    public class SlimeBoss : BigSlime
    {
        public int timeUntilNextAttack;
        public readonly NetBool firing = new NetBool(false);
        public NetInt attackState = new NetInt();
        public int nextFireTime;
        public int totalFireTime;
        private float difficulty;
        private int width;
        private int height;
        private int j = 0;

        public SlimeBoss() { 
        }

        public SlimeBoss(Vector2 position, float difficulty) : base(position, 121)
        {
            width = ModEntry.Config.SlimeBossWidth;
            height = ModEntry.Config.SlimeBossHeight;
            Sprite.SpriteWidth = width;
            Sprite.SpriteHeight = height;
            Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
            Sprite.UpdateSourceRect();

            Scale = ModEntry.Config.SlimeBossScale;

            this.difficulty = difficulty;
            Health = (int)Math.Round(Health * 10 * difficulty);
            MaxHealth = Health;
            DamageToFarmer = (int)Math.Round(damageToFarmer * 2 * difficulty);
            timeUntilNextAttack = 100;
            moveTowardPlayerThreshold.Value = 20;

            //this.willDestroyObjectsUnderfoot = true;
        }

        public override void reloadSprite()
        {
            typeof(Monster).GetMethod("reloadSprite").Invoke(this, new object[] { });
            Sprite.SpriteWidth = width;
            Sprite.SpriteHeight = height;
            Sprite.interval = 300f;
            Sprite.ignoreStopAnimation = true;
            ignoreMovementAnimations = true;
            HideShadow = true;
            Sprite.framesPerAnimation = 8;
        }

        protected override void updateAnimation(GameTime time)
        {
            base.updateAnimation(time);
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
        public override void behaviorAtGameTick(GameTime time)
        {
            base.behaviorAtGameTick(time);

            if (Health <= 0)
            {
                return;
            }

            // fire!

            timeUntilNextAttack -= time.ElapsedGameTime.Milliseconds;

            List<Farmer> farmers = new List<Farmer>();
            FarmerCollection.Enumerator enumerator = currentLocation.farmers.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.currentLocation == currentLocation && enumerator.Current.GetBoundingBox().Intersects(GetBoundingBox()))
                {
                    enumerator.Current.takeDamage((int)Math.Round(20 * difficulty), true, null);
                    totalFireTime = 0;
                    nextFireTime = 10;
                    attackState.Set(0);
                    timeUntilNextAttack = Game1.random.Next(1000, 2000);
                }
            }

            if (attackState.Value == 0 && withinPlayerThreshold(20))
            {
                firing.Set(false);
                if (timeUntilNextAttack < 0)
                {
                    timeUntilNextAttack = 0;
                    attackState.Set(1);
                    nextFireTime = 50;
                    totalFireTime = 3000;
                    return;
                }
            }
            else if (totalFireTime > 0)
            {
                Farmer player = Player;
                if (!firing.Value)
                {
                    if (player != null)
                    {
                        faceGeneralDirection(player.Position, 0, false);
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
                        }
                        float fire_angle = 0f;
                        Vector2 shot_origin = new Vector2((float)GetBoundingBox().Center.X, (float)GetBoundingBox().Center.Y);
                        faceGeneralDirection(player.Position, 0, false);
                        switch (facingDirection.Value)
                        {
                            case 0:
                                fire_angle = 90f;
                                break;
                            case 1:
                                fire_angle = 0f;
                                break;
                            case 2:
                                fire_angle = 270f;
                                break;
                            case 3:
                                fire_angle = 180f;
                                break;
                        }
                        fire_angle += (float)Math.Sin((double)((float)totalFireTime / 1000f * 180f) * 3.1415926535897931 / 180.0) * 25f;
                        Vector2 shot_velocity = new Vector2((float)Math.Cos((double)fire_angle * 3.1415926535897931 / 180.0), -(float)Math.Sin((double)fire_angle * 3.1415926535897931 / 180.0));
                        shot_velocity *= 5f;

                        for (int i = 0; i < 8; i++)
                        {

                            bool one = i < 4;
                            bool two = i % 4 < 2;
                            bool three = i % 2 == 0;

                            Vector2 v = new Vector2((three ? shot_velocity.X : shot_velocity.Y) * (one ? -1 : 1), (three ? shot_velocity.Y : shot_velocity.X) * (two ? -1 : 1));
                            //v = ModEntry.RotateVector(v, j);
                            BasicProjectile projectile = new BossProjectile((int)(5 * difficulty), 766, 0, 1, 0.196349546f, v.X, v.Y, shot_origin, "", "", false, false, currentLocation, this, true, null, 13, true);
                            projectile.IgnoreLocationCollision = true;
                            projectile.ignoreTravelGracePeriod.Value = true;
                            projectile.maxTravelDistance.Value = 512;
                            currentLocation.projectiles.Add(projectile);

                            if (!ModEntry.IsLessThanHalfHealth(this))
                            {
                                i++;
                            }

                        }
                        if (ModEntry.IsLessThanHalfHealth(this))
                        {
                            j += 1;
                        }
                        j %= 360;


                        nextFireTime = 20;
                    }
                }
                if (totalFireTime <= 0)
                {
                    totalFireTime = 0;
                    nextFireTime = 20;
                    attackState.Set(0);

                    timeUntilNextAttack = 0;
                }
            }
        }
        public override Rectangle GetBoundingBox()
        {
            return new Rectangle((int)(Position.X + 8 * Scale), (int)(Position.Y + 16 * Scale), (int)(Sprite.SpriteWidth * 4 * 3 / 4 * Scale), (int)(32 * Scale));
            // Rectangle r = new Rectangle((int)(Position.X - Scale*width/2), (int)(Position.Y - Scale * height/2), (int)(Scale*width), (int)(Scale*height));
            // return r;
        }

        public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
        {
            int result = base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
            if (Health <= 0)
            {
                ModEntry.BossDeath(currentLocation, this, difficulty);
            }
            else
            {
                if(Game1.random.NextDouble() < 0.5f)
                    currentLocation.characters.Add(new GreenSlime(Position, (int)(120*difficulty)));

                currentLocation.characters[currentLocation.characters.Count - 1].setTrajectory(xTrajectory / 8 + Game1.random.Next(-20, 20), yTrajectory / 8 + Game1.random.Next(-20, 20));
                currentLocation.characters[currentLocation.characters.Count - 1].willDestroyObjectsUnderfoot = false;
                currentLocation.characters[currentLocation.characters.Count - 1].moveTowardPlayer(20);
            }
            ModEntry.MakeBossHealthBar(Health, MaxHealth);
            return result;
        }
    }
}