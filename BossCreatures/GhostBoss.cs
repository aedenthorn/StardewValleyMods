using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BossCreatures
{
    internal class GhostBoss : Ghost
    {
        private float lastGhost;
        private float lastDebuff;
        private int MaxGhosts;
        private float difficulty;
        private int width;
        private int height;

        public GhostBoss() { }
        public GhostBoss(Vector2 spawnPos, float difficulty) : base(spawnPos)
        {
            width = ModEntry.Config.GhostBossWidth;
            height = ModEntry.Config.GhostBossHeight;
            Sprite.SpriteWidth = width;
            Sprite.SpriteHeight = height;
            Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
            Scale = ModEntry.Config.GhostBossScale;

            this.difficulty = difficulty;

            Health = (int)Math.Round(Health * 20 * difficulty);
            MaxHealth = Health;
            DamageToFarmer = (int)Math.Round(DamageToFarmer * difficulty);
            MaxGhosts = 4;

            moveTowardPlayerThreshold.Value = 20;
        }

        public override void reloadSprite()
        {
            Sprite = new AnimatedSprite("Characters\\Monsters\\Ghost");
            Sprite.LoadTexture(ModEntry.GetBossTexture(GetType()));
        }

        public override void behaviorAtGameTick(GameTime time)
        {
            base.behaviorAtGameTick(time);


            if (Health <= 0)
            {
                return;
            }

            lastGhost = Math.Max(0f, lastGhost - (float)time.ElapsedGameTime.Milliseconds);
            lastDebuff = Math.Max(0f, lastDebuff - (float)time.ElapsedGameTime.Milliseconds);

            if (withinPlayerThreshold(10))
            {
                if(lastDebuff == 0f)
                {
                    Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(GetBoundingBox().Center, 15f, Player);
                    if (ModEntry.IsLessThanHalfHealth(this))
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            Vector2 trajectory = ModEntry.VectorFromDegree(i * 30) * 10f;
                            currentLocation.projectiles.Add(new BossProjectile((int)Math.Round(20 * difficulty), 9, 3, 4, 0f, trajectory.X, trajectory.Y, getStandingPosition(), "", "", true, false, currentLocation, this, false, null, 19));
                        }
                    }
                    else
                    {
                        currentLocation.projectiles.Add(new BossProjectile((int)Math.Round(20 * difficulty), 9, 3, 4, 0f, velocityTowardPlayer.X, velocityTowardPlayer.Y, getStandingPosition(), "", "", true, false, currentLocation, this, false, null, 19));
                    }


                    lastDebuff = Game1.random.Next(3000, 6000);
                }
                if (lastGhost == 0f)
                {
                    int ghosts = 0;
                    using (List<NPC>.Enumerator enumerator = currentLocation.characters.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            NPC j = enumerator.Current;
                            if (j is ToughGhost)
                            {
                                ghosts++;
                            }
                        }
                    }
                    if (ghosts < (Health < MaxHealth / 2 ? MaxGhosts * 2 : MaxGhosts))
                    {
                        GameLocation aLocation = currentLocation;
                        currentLocation.characters.Add(new ToughGhost(Position, difficulty)
                        {
                            focusedOnFarmers = true
                        });
                        lastGhost = (float)Game1.random.Next(3000, 80000);
                    }
                }
            }
        }
        public override Rectangle GetBoundingBox()
        {
            return new Rectangle((int)(Position.X + 8 * Scale), (int)(Position.Y + 16 * Scale), (int)(Sprite.SpriteWidth * 4 * 3 / 4 * Scale), (int)(32 * Scale));
            // Rectangle r = new Rectangle((int)(Position.X - Scale * width / 2), (int)(Position.Y - Scale * height / 2), (int)(Scale * width), (int)(Scale * height));
            // return r;
        }
        public override void drawAboveAllLayers(SpriteBatch b)
        {
            int offset  = (int)GetType().BaseType.GetField("yOffset", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);

            b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(width*2, (float)(21 + offset)), new Microsoft.Xna.Framework.Rectangle?(Sprite.SourceRect), Color.White, 0f, new Vector2(width/2, width), scale * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : (getStandingY() / 10000f)));
            b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(width*2, width*4), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f + offset / 20f * width/16, SpriteEffects.None, (getStandingY() - 1) / 10000f);
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