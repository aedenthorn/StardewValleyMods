using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomMonsters
{
    public partial class ModEntry : Mod
    {

        public static Monster CreateMonster(string id, Vector2 position)
        {
            if (!Monsters.TryGetValue(id, out var data))
            {
                return null;
            }

            SMonitor.Log($"Creating monster '{id}' of type '{data.Type}' at position {position}.");

            var type = typeof(Monster).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Monster))).FirstOrDefault(t => t.Name == data.Type);
            if (type == null)
            {
                SMonitor.Log($"Invalid monster type {data.Type}", LogLevel.Warn);
                return null;
            }
            List<object> parameters = new List<object>();
            foreach (var param in data.Parameters.Split(','))
            {
                switch (param.Trim().ToLower())
                {
                    case "position":
                        parameters.Add(position);
                        break;
                    case "level":
                        parameters.Add(data.Level);
                        break;
                    case "facing":
                        parameters.Add(data.Facing);
                        break;
                    case "name":
                        parameters.Add(data.Name);
                        break;
                    case "color":
                        parameters.Add(MakeColor(data));
                        break;
                    case "switch":
                        parameters.Add(data.Switch);
                        break;
                    default:
                        SMonitor.Log($"Unknown parameter '{param}' for monster '{id}'.", LogLevel.Warn);
                        break;
                }
            }
            Monster monster = null;
            try
            {
                monster = (Monster)Activator.CreateInstance(type, parameters.ToArray());
            }
            catch
            {
                if (parameters.Count == 1 && parameters[0] is Vector2)
                {
                    monster = (Monster)Activator.CreateInstance(type, new object[] { });
                }
                else
                {
                    SMonitor.Log($"Failed to create monster of type {data.Type} with parameters {string.Join(",", parameters)}", LogLevel.Warn);
                    return null;
                }
            }
            if (!parameters.Any())
            {
                monster.Position = position;
            }
            monster.modData[monsterKey] = id;
            if (!string.IsNullOrEmpty(data.Despawn))
            {
                if (!despawnDict.ContainsKey(data.Despawn))
                {
                    despawnDict[data.Despawn] = new List<Monster>();
                }
                despawnDict[data.Despawn].Add(monster);
            }
            if (monster is RockCrab crab)
            {
                if (data.StickBug)
                {
                    crab.makeStickBug();
                }
                if (data.WaiterChance != null)
                {
                    crab.waiter = Game1.random.NextDouble() < data.WaiterChance.Value / 100.0;
                }
            }
            if (data.Scale > -1)
            {
                monster.Scale = data.Scale;
            }
            if (data.MissChance > -1)
            {
                monster.missChance.Value = data.MissChance / 100.0;
            }
            if (data.Slipperiness > -1)
            {
                monster.Slipperiness = data.Slipperiness;
            }
            if (data.Jitteriness > -1)
            {
                monster.jitteriness.Value = data.Jitteriness;
            }
            if (data.Speed > -1)
            {
                monster.speed = data.Speed;
            }
            if (data.Damage > -1)
            {
                monster.DamageToFarmer = data.Damage;
            }
            if (data.Health > -1)
            {
                monster.Health = data.Health;
                monster.MaxHealth = data.Health;
            }
            if (data.Experience > -1)
            {
                monster.ExperienceGained = data.Experience;
            }
            if (data.Resilience > -1)
            {
                monster.resilience.Value = data.Resilience;
            }
            monster.reloadSprite();
            if (monster is not DwarvishSentry && monster is not RockGolem && data.SpawnSound != null)
            {
                DelayedAction.playSoundAfterDelay(data.SpawnSound, 500, null, null, -1, false);
            }
            if (data.HardMode)
            {
                monster.isHardModeMonster.Value = true;
            }
            if (monster is AngryRoger)
            {
                monster.HideShadow = true;
            }
            else if (monster is Bat b)
            {
                b.canLunge.Value = data.CanLunge;
                b.hauntedSkull.Value = data.HauntedSkull;
                b.cursedDoll.Value = data.CursedDoll;
                if (data.SpeedMin > -1 && data.SpeedMax > -1)
                {
                    AccessTools.FieldRefAccess<Bat, float>(b, "maxSpeed") = Game1.random.Next(data.SpeedMin, data.SpeedMax + 1);
                }
                if (data.ExtraVelocity != null)
                {
                    AccessTools.FieldRefAccess<Bat, float>(b, "extraVelocity") = data.ExtraVelocity.Value;
                }
                if (data.HideShadow != null)
                {
                    b.HideShadow = data.HideShadow.Value;
                }
                b.reloadSprite();
                return b;
            }
            else if (monster is BigSlime bs)
            {
                if (data.HeldItem != null)
                {
                    int totalWeight = data.HeldItem.Sum(i => i.Chance);
                    int roll = Game1.random.Next(0, totalWeight);
                    int cumulativeWeight = 0;
                    foreach (var item in data.HeldItem)
                    {
                        cumulativeWeight += item.Chance;
                        if (roll < cumulativeWeight)
                        {
                            bs.heldItem.Value = item.ItemId != null ? ItemRegistry.Create(item.ItemId, Game1.random.Next(item.MinQuantity, item.MaxQuantity + 1), item.Quality) : null;
                            break;
                        }
                    }
                }
                if (data.Colors != null || data.Color != null)
                {
                    bs.c.Value = MakeColor(data);
                }
                return bs;
            }
            else if (monster is Bug bug)
            {
                if (data.Armored != null)
                {
                    bug.isArmoredBug.Value = data.Armored.Value;
                    return bug;
                }
            }
            else if (monster is DinoMonster dino)
            {
                if (data.ProjectileIndex != null || data.ProjectileSprite != null || data.ProjectileSource != null)
                {
                    for (int i = 0; i < dino.projectiles.Length; i++)
                    {
                        dino.projectiles[i] = new CustomBreathProjectile(data);
                    }
                }
                return dino;
            }
            else if (monster is Fly fly)
            {
                fly.HideShadow = data.HideShadow ?? true;
                return fly;
            }
            else if (monster is GreenSlime gs)
            {
                if (data.Colors != null || data.Color != null)
                {
                    gs.color.Value = MakeColor(data);
                    if(gs.color.Value.R == 6 && gs.color.Value.G == 6 && gs.color.Value.B == 6)
                    {
                        gs.prismatic.Value = true;
                    }

                }
                if (data.Stacks != null)
                {
                    int totalWeight = data.Stacks.Sum(s => s.Chance);
                    int roll = Game1.random.Next(0, totalWeight);
                    int cumulativeWeight = 0;
                    foreach (var stack in data.Stacks)
                    {
                        cumulativeWeight += stack.Chance;
                        if (cumulativeWeight < totalWeight)
                        {
                            gs.stackedSlimes.Value = Game1.random.Next(stack.MinStacks, stack.MaxStacks + 1);
                            break;
                        }
                    }
                }
                if (data.Prismatic != null)
                {
                    gs.prismatic.Value = data.Prismatic.Value;
                }
                if (data.HideShadow != null)
                {
                    gs.HideShadow = data.HideShadow.Value;
                }
                return gs;
            }
            else if (monster is MetalHead mh)
            {
                if (data.Colors != null || data.Color != null)
                {
                    mh.c.Value = MakeColor(data);
                }
                return mh;
            }
            else if (monster is Serpent s)
            {
                if (data.MinSegment != null && data.MaxSegment != null)
                {
                    s.segmentCount.Value = Game1.random.Next(data.MinSegment.Value, data.MaxSegment.Value + 1);
                    s.reloadSprite();
                }
                return s;
            }
            else if (monster is Shooter sh)
            {
                if (data.ProjectileIndex != null)
                {
                    sh.firedProjectile = data.ProjectileIndex.Value;
                }
                if (data.ProjectileDebuff != null)
                {
                    sh.projectileDebuff = data.ProjectileDebuff;
                }
                if (data.AimTime != null)
                {
                    sh.aimTime = data.AimTime.Value;
                }
                if (data.AimEndTime != null)
                {
                    sh.aimEndTime = data.AimEndTime.Value;
                }
                if (data.BurstTime != null)
                {
                    sh.burstTime = data.BurstTime.Value;
                }
                if (data.ShotsPerFire != null)
                {
                    sh.numberOfShotsPerFire = data.ShotsPerFire.Value;
                }
                if (data.ProjectileRange != null)
                {
                    sh.projectileRange = data.ProjectileRange.Value;
                }
                if (data.DesiredDistance != null)
                {
                    sh.desiredDistance = data.DesiredDistance.Value;
                }
                if (data.FireRange != null)
                {
                    sh.fireRange = data.FireRange.Value;
                }
                if (data.ProjectileSound != null)
                {
                    sh.fireSound = data.ProjectileSound;
                }
                if (data.DamageSound != null)
                {
                    sh.damageSound = data.DamageSound;
                }
                return sh;
            }
            return monster;
        }
    }
}
