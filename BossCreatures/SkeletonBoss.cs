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

namespace BossCreatures
{
    public class SkeletonBoss : Skeleton
    {
        private float difficulty;
        private readonly NetBool throwing = new NetBool();
        private bool spottedPlayer;
        private int controllerAttemptTimer;
        private int throwTimer = 0;
        private int throwBurst = 10;
        private int throws = 0;
        private int width;
        private int height;

        public SkeletonBoss() {
        }

        public SkeletonBoss(Vector2 spawnPos, float difficulty) : base(spawnPos)
        {
            width = ModEntry.Config.SkeletonBossWidth;
            height = ModEntry.Config.SkeletonBossHeight;
            Sprite.SpriteWidth = width;
            Sprite.SpriteHeight = height;
            Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));

            this.difficulty = difficulty;

            Health = (int)Math.Round(Health * 20 * difficulty);
            MaxHealth = Health;
            DamageToFarmer = (int)Math.Round(damageToFarmer * 2 * difficulty);

            Scale = ModEntry.Config.SkeletonBossScale;
            moveTowardPlayerThreshold.Value = 20;
        }
        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddField(throwing);
            position.Field.AxisAlignedMovement = true;
        }
        public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
        {
            base.MovePosition(time, viewport, currentLocation);
            base.MovePosition(time, viewport, currentLocation);
        }

        public override void behaviorAtGameTick(GameTime time)
        {
            if (!throwing.Value)
            {
                throwTimer -= time.ElapsedGameTime.Milliseconds;
                base.behaviorAtGameTick(time);
            }
            if (Health <= 0)
            {
                return;
            }
            if (!spottedPlayer && !wildernessFarmMonster && Utility.doesPointHaveLineOfSightInMine(currentLocation, getTileLocation(), Player.getTileLocation(), 8))
            {
                controller = new PathFindController(this, currentLocation, new Point(Player.getStandingX() / 64, Player.getStandingY() / 64), Game1.random.Next(4), null, 200);
                spottedPlayer = true;
                facePlayer(Player);
                //base.currentLocation.playSound("skeletonStep", NetAudio.SoundContext.Default);
                IsWalkingTowardPlayer = true;
            }
            else if (throwing)
            {
                if (invincibleCountdown > 0)
                {
                    invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
                    if (invincibleCountdown <= 0)
                    {
                        stopGlowing();
                    }
                }
                Sprite.Animate(time, 20, 5, 150f);
                if (Sprite.currentFrame == 24)
                {

                    throwing.Value = false;
                    Sprite.currentFrame = 0;
                    faceDirection(2);
                    Vector2 v = Utility.getVelocityTowardPlayer(new Point((int)Position.X, (int)Position.Y), 8f, Player);
                    if(Health < MaxHealth / 2)
                    {
                        if(throws == 0)
                        {
                            currentLocation.playSound("fireball", NetAudio.SoundContext.Default);
                        }
                        currentLocation.projectiles.Add(new BasicProjectile(DamageToFarmer, 4, 0, 0, 0.196349546f, v.X, v.Y, new Vector2(Position.X, Position.Y), "", "", false, false, currentLocation, this, false, null));
                        currentLocation.projectiles.Add(new BasicProjectile(DamageToFarmer, 10, 0, 4, 0.196349546f, v.X, v.Y, new Vector2(Position.X, Position.Y), "", "", true, false, currentLocation, this, false, null));
                        if (++throws > throwBurst *2)
                        {
                            throwTimer = 1000;
                            throws = 0;
                        }
                        else
                        {
                            throwTimer = 100;
                        }
                    }
                    else
                    {
                        currentLocation.projectiles.Add(new BasicProjectile(DamageToFarmer, 4, 0, 0, 0.196349546f, v.X, v.Y, new Vector2(Position.X, Position.Y), "skeletonHit", "skeletonStep", false, false, currentLocation, this, false, null));
                        if (++throws > throwBurst)
                        {
                            throwTimer = 1000;
                            throws = 0;
                        }
                        else
                        {
                            throwTimer = 10;
                        }

                    }
                }
            }
            else if (spottedPlayer && controller == null && Game1.random.NextDouble() < 0.5 && !wildernessFarmMonster && Utility.doesPointHaveLineOfSightInMine(currentLocation, getTileLocation(), Player.getTileLocation(), 8) && throwTimer <= 0)
            {
                throwing.Value = true;
                Sprite.currentFrame = 20;
                //base.shake(750);
            }
            else if (withinPlayerThreshold(20))
            {
                controller = null;
            }
            else if (spottedPlayer && controller == null && controllerAttemptTimer <= 0)
            {
                controller = new PathFindController(this, currentLocation, new Point(Player.getStandingX() / 64, Player.getStandingY() / 64), Game1.random.Next(4), null, 200);
                facePlayer(Player);
                controllerAttemptTimer = 2000;
            }
            else if (wildernessFarmMonster)
            {
                spottedPlayer = true;
                IsWalkingTowardPlayer = true;
            }
            controllerAttemptTimer -= time.ElapsedGameTime.Milliseconds;
            
        }
        public override Rectangle GetBoundingBox()
        {
            Rectangle r = new Rectangle((int)(Position.X - Scale * width / 2), (int)(Position.Y - Scale * height / 2), (int)(Scale * width), (int)(Scale * height));
            return r;
        }
        public override void Halt()
        {

        }
        public override void shedChunks(int number)
        {
            Game1.createRadialDebris(currentLocation, Sprite.textureName.Value, new Rectangle(0, height*4, width, width), 8, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)getTileLocation().Y, Color.White, 4f);
        }

        public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
        {
            int result = base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
            if (Health <= 0)
            {
                ModEntry.BossDeath(currentLocation, this, difficulty);
            }
            ModEntry.MakeBossHealthBar(Health, MaxHealth);
            return result;
        }
    }
}