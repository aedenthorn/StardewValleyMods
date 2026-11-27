using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using Object = StardewValley.Object;

namespace ParticleEffects
{
    public partial class ModEntry { 

        private static void ShowFarmerParticleEffect(SpriteBatch b, Farmer instance, string key, ParticleEffectData ped)
        {
            if (!farmerEffectDict.TryGetValue(instance.UniqueMultiplayerID, out EntityParticleData entityParticleData))
            {
                entityParticleData = new EntityParticleData();
                farmerEffectDict[instance.UniqueMultiplayerID] = entityParticleData;
            }
            if (!entityParticleData.particleDict.TryGetValue(key, out var particleList))
            {
                particleList = new List<ParticleData>();
                farmerEffectDict[instance.UniqueMultiplayerID].particleDict[key] = particleList;
            }
            ShowParticleEffect(b, particleList, ped, instance.GetBoundingBox().Center.ToVector2() + new Vector2(ped.fieldOffsetX, ped.fieldOffsetY), instance.getDrawLayer());
            farmerEffectDict[instance.UniqueMultiplayerID] = entityParticleData;
        }
        private static void ShowNPCParticleEffect(SpriteBatch b, NPC instance, string key, ParticleEffectData ped)
        {
            if (!npcEffectDict.TryGetValue(instance.Name, out EntityParticleData entityParticleData))
            {
                entityParticleData = new EntityParticleData();
                npcEffectDict[instance.Name] = entityParticleData;
            }
            if (!entityParticleData.particleDict.TryGetValue(key, out var particleList))
            {
                particleList = new List<ParticleData>();
                npcEffectDict[instance.Name].particleDict[key] = particleList;
            }
            ShowParticleEffect(b, particleList, ped, instance.GetBoundingBox().Center.ToVector2() + new Vector2(ped.fieldOffsetX, ped.fieldOffsetY), 1f);
            npcEffectDict[instance.Name] = entityParticleData;
        }
        private static void ShowObjectParticleEffect(SpriteBatch b, Object instance, int x, int y, string key, ParticleEffectData ped)
        {
            var oKey = instance.Name + "|" + x + "," + y;
            if (!objectEffectDict.TryGetValue(oKey, out EntityParticleData entityParticleData))
            {
                entityParticleData = new EntityParticleData();
                objectEffectDict[oKey] = entityParticleData;
            }
            if (!entityParticleData.particleDict.TryGetValue(key, out var particleList))
            {
                particleList = new List<ParticleData>();
                objectEffectDict[oKey].particleDict[key] = particleList;
            }
            ShowParticleEffect(b, particleList, ped, instance.getBoundingBox(new Vector2(x, y)).Center.ToVector2() + new Vector2(ped.fieldOffsetX, ped.fieldOffsetY), Math.Max(0f, ((y + 1) * 64 - 24) / 10000f) + x * 1E-05f);
            objectEffectDict[oKey] = entityParticleData;
        }
        private static void ShowLocationParticleEffect(SpriteBatch b, GameLocation instance, ParticleEffectData ped)
        {
            if (!locationEffectDict.TryGetValue(instance.Name, out EntityParticleData entityParticleData))
            {
                entityParticleData = new EntityParticleData();
                locationEffectDict[instance.Name] = entityParticleData;
            }
            List<ParticleData> particleList;
            if (!entityParticleData.particleDict.TryGetValue(ped.key, out particleList))
            {
                particleList = new List<ParticleData>();
                locationEffectDict[instance.Name].particleDict[ped.key] = particleList;
            }
            ShowParticleEffect(b, particleList, ped, new Vector2(ped.fieldOffsetX, ped.fieldOffsetY), 1f);
            locationEffectDict[instance.Name] = entityParticleData;
        }
        private static void ShowScreenParticleEffect(SpriteBatch b, ParticleEffectData ped)
        {
            List<ParticleData> particleList;
            if (!screenEffectDict.particleDict.TryGetValue(ped.key, out particleList))
            {
                particleList = new List<ParticleData>();
                screenEffectDict.particleDict[ped.key] = particleList;
            }
            ShowParticleEffect(b, particleList, ped, new Vector2(ped.fieldOffsetX, ped.fieldOffsetY), 1f, true);
        }
        private static void ShowParticleEffect(SpriteBatch b, List<ParticleData> particleList, ParticleEffectData ped, Vector2 center, float drawDepth, bool screen = false)
        {
            for (int i = particleList.Count - 1; i >= 0; i--)
            {
                particleList[i].age++;
                if (particleList[i].age > particleList[i].lifespan)
                {
                    particleList.RemoveAt(i);
                    continue;
                }
                Vector2 direction = particleList[i].direction;
                if(direction == Vector2.Zero)
                {
                    if (ped.movementType.Contains(" "))
                    {
                        var split = ped.movementType.Split(' ');
                        if (split.Length == 2 
                            && float.TryParse(split[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float x) 
                            && float.TryParse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float y)
                        )
                        { 
                            direction = new Vector2(x, y);
                            direction.Normalize();
                        }
                    }
                    else
                    {
                        switch (ped.movementType)
                        {
                            case "away":
                                direction = (center - particleList[i].position) * new Vector2(-1, -1);
                                direction.Normalize();
                                break;
                            case "towards":
                                direction = center - particleList[i].position;
                                direction.Normalize();
                                break;
                            case "up":
                                direction = new Vector2(0, -1);
                                break;
                            case "down":
                                direction = new Vector2(0, 1);
                                break;
                            case "left":
                                direction = new Vector2(-1, 0);
                                break;
                            case "right":
                                direction = new Vector2(1, 0);
                                break;
                            case "random":
                                direction = new Vector2((float)Game1.random.NextDouble() - 0.5f, (float)Game1.random.NextDouble() - 0.5f);
                                direction.Normalize();
                                break;
                        }
                    }
                    particleList[i].direction = direction;
                }
                particleList[i].position += particleList[i].direction * (ped.movementSpeed + ped.acceleration * particleList[i].age);
                if (IsOutOfBounds(particleList[i], ped, center) )
                {
                    particleList.RemoveAt(i);
                    continue;
                }
                particleList[i].rotation += particleList[i].rotationRate;
            }
            if(particleList.Count < ped.maxParticles && Game1.random.NextDouble() < ped.particleChance)
            {
                var particle = new ParticleData();
                particle.lifespan = Game1.random.Next(ped.minLifespan, ped.maxLifespan + 1);
                particle.scale = ped.minParticleScale + (float)Game1.random.NextDouble() * (ped.maxParticleScale - ped.minParticleScale);
                particle.alpha = ped.minAlpha + (float)Game1.random.NextDouble() * (ped.maxAlpha - ped.minAlpha);
                particle.rotationRate = ped.minRotationRate + (float)Game1.random.NextDouble() * (ped.maxRotationRate - ped.minRotationRate);
                particle.option = Game1.random.Next(ped.spriteSheet.Height / ped.particleHeight);
                if (ped.fieldOuterRadius <= 0)
                {
                    double x;
                    double y;
                    if (screen)
                    {
                        if (ped.fieldOffsetX < 0)
                            ped.fieldOffsetX = Game1.viewport.Width / 2;
                        if (ped.fieldOffsetY < 0)
                            ped.fieldOffsetY = Game1.viewport.Height / 2;
                        if (ped.fieldOuterWidth < 0)
                            ped.fieldOuterWidth = Game1.viewport.Width;
                        if (ped.fieldOuterHeight < 0)
                            ped.fieldOuterHeight = Game1.viewport.Height;
                    }
                    else
                    {
                        if (ped.fieldOffsetX < 0)
                            ped.fieldOffsetX = Game1.currentLocation.map.DisplayWidth / 2;
                        if (ped.fieldOffsetY < 0)
                            ped.fieldOffsetY = Game1.currentLocation.map.DisplayHeight / 2;
                        if (ped.fieldOuterWidth < 0)
                            ped.fieldOuterWidth = Game1.currentLocation.map.DisplayWidth;
                        if (ped.fieldOuterHeight < 0)
                            ped.fieldOuterHeight = Game1.currentLocation.map.DisplayHeight;
                    }
                    if (ped.fieldInnerHeight > 0)
                    {
                        var innerTop = (ped.fieldOuterHeight - ped.fieldInnerHeight) / 2;
                        var innerBottom = ped.fieldOuterHeight - innerTop;
                        var innerLeft = (ped.fieldOuterWidth - ped.fieldInnerWidth) / 2;
                        var innerRight = ped.fieldOuterWidth - innerLeft;
                        var pixel = (int)((ped.fieldOuterWidth * innerTop * 2 + ped.fieldInnerHeight * innerLeft * 2) * Game1.random.NextDouble());
                        if (pixel >= ped.fieldOuterWidth * innerTop + ped.fieldInnerHeight * innerLeft * 2) // bottom
                        {
                            pixel = pixel - ped.fieldOuterWidth * innerTop - ped.fieldInnerHeight * innerLeft * 2;
                            x = pixel % ped.fieldOuterWidth;
                            y = innerBottom + pixel / ped.fieldOuterWidth;
                        }
                        else if (pixel >= ped.fieldOuterWidth * innerTop + ped.fieldInnerHeight * innerLeft) // right
                        {
                            pixel = pixel - ped.fieldOuterWidth * innerTop - ped.fieldInnerHeight * innerLeft;
                            x = innerRight + pixel % innerLeft;
                            y = innerTop + pixel / innerLeft;
                        }
                        else if (pixel >= ped.fieldOuterWidth * innerTop) // left
                        {
                            pixel = pixel - ped.fieldOuterWidth * innerTop;
                            x = pixel % innerLeft;
                            y = innerTop + pixel / innerLeft;
                        }
                        else // top
                        {
                            x = pixel % ped.fieldOuterWidth;
                            y = pixel / ped.fieldOuterWidth;
                        }
                    }
                    else
                    {
                        x = ped.fieldOuterWidth * Game1.random.NextDouble();
                        y = ped.fieldOuterHeight * Game1.random.NextDouble();
                    }
                    particle.position = center - new Vector2(ped.fieldOuterWidth, ped.fieldOuterHeight) / 2 + new Vector2((float)x, (float)y);
                }
                else
                {
                    particle.position = center + GetCirclePos(ped);
                }
                particleList.Add(particle);
            }
            int frames = ped.spriteSheet.Width / ped.particleWidth;
            foreach(var particle in particleList)
            {
                float depthOffset = ped.belowOffset >= 0 ? (ped.aboveOffset >= 0 ? (Game1.random.NextDouble() < 0.5 ? ped.aboveOffset : ped.belowOffset) : ped.belowOffset) : ped.aboveOffset;
                int frame = (int)Math.Round(particle.age * ped.frameSpeed) % frames;
                b.Draw(ped.spriteSheet, new Rectangle(Utility.Vector2ToPoint(screen ? particle.position : Game1.GlobalToLocal(particle.position)), new Point((int)(ped.particleWidth * particle.scale), (int)(ped.particleHeight * particle.scale))), new Rectangle(frame * ped.particleWidth, particle.option * ped.particleHeight, ped.particleWidth, ped.particleHeight), Color.White * particle.alpha, particle.rotation, new Vector2(ped.particleWidth / 2, ped.particleHeight / 2), SpriteEffects.None, drawDepth + depthOffset);
            }
        }

        private static Vector2 GetCirclePos(ParticleEffectData ped)
        {
            var angle = (float)Game1.random.NextDouble() * 2 * Math.PI;
            var distance = (float)Math.Sqrt(ped.fieldInnerRadius / ped.fieldOuterRadius + (float)Game1.random.NextDouble() * (1 - ped.fieldInnerRadius / ped.fieldOuterRadius)) * ped.fieldOuterRadius;
            return new Vector2(distance * (float)Math.Cos(angle), distance * (float)Math.Sin(angle));
        }

        private static bool IsOutOfBounds(ParticleData particle, ParticleEffectData ped, Vector2 center)
        {
            if (!ped.restrictOuter && !ped.restrictInner)
                return false;
            if(ped.fieldOuterRadius > 0)
            {
                return (ped.restrictOuter && Vector2.Distance(center, particle.position) > ped.fieldOuterRadius) || (ped.restrictInner && Vector2.Distance(center, particle.position) <= ped.fieldInnerRadius);
            }
            else
            {
                return (ped.restrictOuter && Math.Abs(particle.position.X - center.X) > ped.fieldOuterWidth / 2 || Math.Abs(particle.position.Y - center.Y) > ped.fieldOuterHeight / 2) || (ped.restrictInner && Math.Abs(particle.position.X - center.X) <= ped.fieldInnerWidth / 2 && Math.Abs(particle.position.Y - center.Y) <= ped.fieldInnerHeight / 2);
            }
        }

        private void LoadEffects()
        {
            effectDict = Game1.content.Load<Dictionary<string, ParticleEffectData>>(dictPath);
            foreach (var key in effectDict.Keys)
            {
                effectDict[key].key = key;
                effectDict[key].spriteSheet = Game1.content.Load<Texture2D>(effectDict[key].spriteSheetPath);
            }
        }


        public static ParticleEffectData CloneParticleEffect(string key, string type, string name, int x, int y, ParticleEffectData template)
        {
            return new ParticleEffectData()
            {
                key = key,
                type = type,
                name = name,
                movementType = template.movementType,
                movementSpeed = template.movementSpeed,
                frameSpeed = template.frameSpeed,
                acceleration = template.acceleration,
                restrictOuter = template.restrictOuter,
                restrictInner = template.restrictInner,
                minRotationRate = template.minRotationRate,
                maxRotationRate = template.maxRotationRate,
                particleWidth = template.particleWidth,
                particleHeight = template.particleHeight,
                fieldInnerWidth = template.fieldInnerWidth,
                fieldInnerHeight = template.fieldInnerHeight,
                fieldOuterWidth = template.fieldOuterWidth,
                fieldOuterHeight = template.fieldOuterHeight,
                minParticleScale = template.minParticleScale,
                maxParticleScale = template.maxParticleScale,
                maxParticles = template.maxParticles,
                minLifespan = template.minLifespan,
                maxLifespan = template.maxLifespan,
                spriteSheetPath = template.spriteSheetPath,
                spriteSheet = template.spriteSheet,
                fieldOffsetX = x,
                fieldOffsetY = y
            };
        }
    }
}