using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewValley.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace AreaOfEffect
{
    public partial class ModEntry
    {
        public static void DoCastSpell(GameLocation l, Farmer who, Vector2 center, SpellCastData spell)
        {
            if (!Context.IsWorldReady || l is null || spell is null)
                return;
            if (!string.IsNullOrEmpty(spell.TriggerAction))
            {
                TriggerActionManager.TryRunAction(spell.TriggerAction, out _, out _);
            }
            foreach (var str in spell.Buffs)
            {
                who.applyBuff(str);
            }

            foreach (var e in spell.Effects)
            {
                if (e.PerTile)
                {
                    if (spell.AreaType == AOEType.Line)
                        continue;
                    List<object> applied = new();
                    var tiles = GetTiles(who.Tile, center, spell);
                    if(e.Delay > 0)
                    {
                        DelayTileList.Add(new(l, who, tiles, e, e.Delay));
                    }
                    else
                    {
                        foreach (var tile in tiles)
                        {
                            ApplyEffectToTile(l, who, tile, e, applied);
                        }
                        if (e.Seconds > 0 && e.ReapplyToTile)
                        {
                            EOTTileList.Add(new(l, who, tiles, e, e.Seconds * 1000 - 1));
                        }
                    }
                }
                else
                {
                    if (e.Delay > 0)
                    {
                        DelayTileList.Add(new(l, who, new Vector2[] { center }, e, e.Delay));
                    }
                    else
                    {
                        ApplyCenteredEffect(l, who, center, e, spell.Radius);
                        if (e.Seconds > 0 && e.ReapplyToTile)
                        {
                            EOTTileList.Add(new(l, who, new Vector2[] { center }, e, e.Seconds * 1000 - 1));
                        }
                    }
                }
            }
            foreach (var s in spell.Sprites)
            {
                if (s.PerTile)
                {
                    if (spell.AreaType == AOEType.Line)
                        continue;
                    foreach (var tile in GetTiles(who.Tile, center, spell))
                    {
                        ApplySpriteToTile(l, tile, s, Vector2.Distance(center, tile));
                    }
                }
                else
                {
                    switch (s.Type)
                    {
                        case SpriteType.Fountain:
                            CreateFountain(l, center, who, spell, s);
                            break;
                        case SpriteType.Lightning:
                            Utility.drawLightningBolt(center * 64, l);
                            break;
                        default:
                            ApplySpriteToTile(l, center, s, 0);
                            break;
                    }
                }
            }
        }
        public static void ApplySpriteToTile(GameLocation l, Vector2 tile, SpriteData data, float distance)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            switch (data.Type)
            {
                case SpriteType.Animation:
                    CreateAnimation(l, tile * 64, data); 
                    break;
                case SpriteType.Balloon:
                    CreateBalloon(l, tile * 64, data); 
                    break;
                case SpriteType.Butterfly:
                    CreateButterfly(l, tile * 64, data); 
                    break;
                case SpriteType.Explosion:
                    CreateExplosion(l, tile * 64, distance, data); 
                    break;
                case SpriteType.EvilRabbit:
                    CreateEvilRabbit(l, tile * 64, data); 
                    break;
                case SpriteType.Fire:
                    CreateFire(l, tile * 64, data); 
                    break;
                case SpriteType.Heart:
                    CreateHeart(l, tile * 64, data);
                    break;
                case SpriteType.Ice:
                    CreateIce(l, tile * 64, data); 
                    break;
                case SpriteType.Poof:
                    CreatePoof(l, tile * 64, data); 
                    break;
                case SpriteType.Smoking:
                    CreateSmoking(l, tile * 64, data); 
                    break;
                case SpriteType.Sparkle:
                    Rectangle r = new((tile * 64).ToPoint(), new Point(64, 64));
                    CreateSparkle(l, r, data); 
                    break;
                case SpriteType.Texture:
                    CreateTexture(l, tile * 64, data);
                    break;
            }
        }
        public static void ApplySpriteToCharacter(GameLocation l, Character c, SpriteData data)
        {
            if (!Context.IsWorldReady || l is null)
                return;
            TemporaryAnimatedSprite t = null;
            var pos = c.Position;
            switch (data.Type)
            {
                case SpriteType.Animation:
                    t = CreateAnimation(l, pos, data); 
                    break;
                case SpriteType.Balloon:
                    t = CreateBalloon(l, pos, data); 
                    break;
                case SpriteType.Butterfly:
                    t = CreateButterfly(l, pos, data); 
                    break;
                case SpriteType.Explosion:
                    t = CreateExplosion(l, pos, 0, data); 
                    break;
                case SpriteType.EvilRabbit:
                    t = CreateEvilRabbit(l, pos, data); 
                    break;
                case SpriteType.Fire:
                    t = CreateFire(l, pos, data); 
                    break;
                case SpriteType.Heart:
                    t = CreateHeart(l, pos, data);
                    break;
                case SpriteType.Ice:
                    t = CreateIce(l, pos, data); 
                    break;
                case SpriteType.Poof:
                    t = CreatePoof(l, pos, data); 
                    break;
                case SpriteType.Smoking:
                    t = CreateSmoking(l, pos, data); 
                    break;
                case SpriteType.Sparkle:
                    Rectangle r = new((pos).ToPoint(), new Point(64, 64));
                    var ts = CreateSparkle(l, r, data);
                    foreach(var a in ts)
                    {
                        if(a is not null)
                            MovingSpriteDict[a] = new()
                            {
                                LastPos = pos,
                                Location = l,
                                Parent = c
                            };
                    }
                    return;
                case SpriteType.Texture:
                    t = CreateTexture(l, pos, data);
                    break;
            }
            if(t is not null)
            {
                MovingSpriteDict[t] = new()
                {
                    LastPos = pos,
                    Location = l,
                    Parent = c
                };
            }
        }
        public static void ApplyCenteredEffect(GameLocation l, Farmer who, Vector2 center, SpellEffect effect, int radius)
        {
            if (!string.IsNullOrEmpty(effect.TriggerAction))
            {
                TriggerActionManager.TryRunAction(TriggerActionManager.ParseAction(effect.TriggerAction), new("Manual", new object[] { center.X, center.Y }, null), out _, out _);
            }
            switch (effect.EffectType)
            {
                case SpellEffectType.Explode:
                    l.explode(center, effect.Radius > 0 ? (int)effect.Radius : radius, who, effect.Affected.Contains(SpellAffectedType.Farmer), (int)effect.Value, effect.Affected.Contains(SpellAffectedType.Object));
                    break;
            }
        }
        public static bool ApplyEffectToTile(GameLocation l, Farmer who, Vector2 tile, SpellEffect effect, List<object> applied)
        {
            if (!Context.IsWorldReady || l is null)
                return false;
            bool result = false;
            if (!string.IsNullOrEmpty(effect.TriggerAction))
            {
                TriggerActionManager.TryRunAction(TriggerActionManager.ParseAction(effect.TriggerAction), new("Manual", new object[] { tile.X, tile.Y }, null), out _, out _);
            }
            foreach (var t in effect.Affected)
            {
                result = ApplyEffect(l, who, tile, t, effect, applied);
                if (effect.First && result)
                    return result;
            }
            return result;
        }

        public static bool ApplyEffect(GameLocation l, Farmer who, Vector2 tile, SpellAffectedType t, SpellEffect effect, List<object> applied)
        {
            List<object> newApplied = new();
            Object o;
            TerrainFeature tf;
            bool result = false;
            switch (t)
            {
                case SpellAffectedType.Animal:
                    foreach(var a in l.animals.Values)
                    {
                        if (applied.Contains(a))
                            continue;
                        var bb = a.GetBoundingBox();
                        var rect = new Rectangle((tile * 64).ToPoint(), new(64, 64));
                        if (bb.Intersects(rect))
                        {
                            ApplyAnimalEffect(l, who, a, effect);
                            applied.Add(a);
                            newApplied.Add(a);
                            result = true;
                        }
                    }
                    break;
                case SpellAffectedType.Building:
                    foreach(var a in l.buildings)
                    {
                        if (applied.Contains(a))
                            continue;
                        var bb = a.GetBoundingBox();
                        var rect = new Rectangle((tile * 64).ToPoint(), new(64, 64));
                        if (bb.Intersects(rect))
                        {
                            ApplyBuildingEffect(l, who, a, effect);
                            applied.Add(a);
                            newApplied.Add(a);
                            result = true;
                        }
                    }
                    break;
                case SpellAffectedType.Pet:
                    foreach(var a in l.characters.Where(c => c is Pet))
                    {
                        if (applied.Contains(a))
                            continue;
                        var bb = a.GetBoundingBox();
                        var rect = new Rectangle((tile * 64).ToPoint(), new(64, 64));
                        if (bb.Intersects(rect))
                        {
                            ApplyPetEffect(l, who, a as Pet, effect);
                            applied.Add(a);
                            newApplied.Add(a);
                            result = true;
                        }
                    }
                    break;
                case SpellAffectedType.Monster:
                    foreach(var a in l.characters.Where(c => c is Monster))
                    {
                        if (applied.Contains(a))
                            continue;
                        var bb = a.GetBoundingBox();
                        var rect = new Rectangle((tile * 64).ToPoint(), new(64, 64));
                        if (bb.Intersects(rect))
                        {
                            ApplyMonsterEffect(l, who, a as Monster, effect);
                            applied.Add(a);
                            newApplied.Add(a);
                            result = true;
                        }
                    }
                    break;
                case SpellAffectedType.NPC:
                    foreach (var a in l.characters.Where(c => c.IsVillager))
                    {
                        if (applied.Contains(a))
                            continue;
                        var bb = a.GetBoundingBox();
                        var rect = new Rectangle((tile * 64).ToPoint(), new(64, 64));
                        if (bb.Intersects(rect))
                        {
                            ApplyNPCEffect(l, who, a, effect);
                            applied.Add(a);
                            newApplied.Add(a);
                            result = true;
                        }
                    }
                    break;
                case SpellAffectedType.Farmer:
                    foreach (var a in l.farmers)
                    {
                        if (applied.Contains(a))
                            continue;
                        var bb = a.GetBoundingBox();
                        var rect = new Rectangle((tile * 64).ToPoint(), new(64, 64));
                        if (bb.Intersects(rect))
                        {
                            ApplyFarmerEffect(l, a, effect);
                            applied.Add(a);
                            newApplied.Add(a);
                            result = true;
                        }
                    }
                    break;
                case SpellAffectedType.Tile:
                    ApplyTileEffect(l, who, tile, effect);
                    break;
                case SpellAffectedType.Object:
                    if(l.Objects.TryGetValue(tile, out o) && !applied.Contains(o))
                    {
                        ApplyObjectEffect(l, who, tile, o, effect);
                        applied.Add(o);
                        newApplied.Add(o);
                        result = true;
                    }
                    break;
                case SpellAffectedType.ResourceClump:
                    var rc = l.resourceClumps.FirstOrDefault(c => c.occupiesTile((int)tile.X, (int)tile.Y));
                    if (rc is not null)
                    {
                        ApplyResourceClumpEffect(l, who, tile, rc, effect);
                        applied.Add(rc);
                        newApplied.Add(rc);
                        result = true;
                    }
                    break;
                case SpellAffectedType.HoeDirt:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is HoeDirt && !applied.Contains(tf))
                    {
                        ApplyTerrainFeatureEffect(l, who, tile, tf, effect);
                        applied.Add(tf);
                        newApplied.Add(tf);
                        result = true;
                    }
                    break;
                case SpellAffectedType.Crop:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is HoeDirt dirt && dirt.crop is not null && !applied.Contains(tf))
                    {
                        ApplyCropEffect(l, who, tile, dirt, effect);
                        applied.Add(tf);
                        newApplied.Add(tf);
                        result = true;
                    }
                    break;
                case SpellAffectedType.Grass:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is Grass && !applied.Contains(tf))
                    {
                        ApplyTerrainFeatureEffect(l, who, tile, tf, effect);
                        applied.Add(tf);
                        newApplied.Add(tf);
                        result = true;
                    }
                    break;
                case SpellAffectedType.Horse:
                    foreach (var a in l.characters.Where(c => c is Horse))
                    {
                        if (applied.Contains(a))
                            continue;
                        var bb = a.GetBoundingBox();
                        var rect = new Rectangle((tile * 64).ToPoint(), new(64, 64));
                        if (bb.Intersects(rect))
                        {
                            ApplyHorseEffect(l, who, a as Horse, effect);
                            applied.Add(a);
                            newApplied.Add(a);
                            result = true;
                        }
                    }
                    break;
                case SpellAffectedType.Stone:
                    if (l.Objects.TryGetValue(tile, out o) && o.IsBreakableStone() && !applied.Contains(o))
                    {
                        ApplyObjectEffect(l, who, tile, o, effect);
                        applied.Add(o);
                        newApplied.Add(o);
                        result = true;
                    }
                    break;
                case SpellAffectedType.Twig:
                    if (l.Objects.TryGetValue(tile, out o) && o.IsTwig() && !applied.Contains(o))
                    {
                        ApplyObjectEffect(l, who, tile, o, effect);
                        applied.Add(o);
                        newApplied.Add(o);
                        result = true;
                    }
                    break;
                case SpellAffectedType.Weed:
                    if (l.Objects.TryGetValue(tile, out o) && o.IsWeeds() && !applied.Contains(o))
                    {
                        ApplyObjectEffect(l, who, tile, o, effect);
                        applied.Add(o);
                        newApplied.Add(o);
                        result = true;
                    }
                    break;
                case SpellAffectedType.Tree:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is Tree && !applied.Contains(tf))
                    {
                        ApplyTerrainFeatureEffect(l, who, tile, tf, effect);
                        applied.Add(tf);
                        newApplied.Add(tf);
                        result = true;
                    }
                    break;
                case SpellAffectedType.FruitTree:
                    if (l.terrainFeatures.TryGetValue(tile, out tf) && tf is FruitTree && !applied.Contains(tf))
                    {
                        ApplyTerrainFeatureEffect(l, who, tile, tf, effect);
                        applied.Add(tf);
                        newApplied.Add(tf);
                        result = true;
                    }
                    break;
            }
            if (effect.Seconds > 0 && !effect.ReapplyToTile && newApplied.Any())
            {
                for(int i = 0; i < newApplied.Count; i++)
                {
                    EOTObjDict[newApplied[i]] = new(l, who, new Vector2[] { tile }, effect, effect.Seconds * 1000 - 1);
                }
            }
            return result;
        }

        private static void ApplyAnimalEffect(GameLocation l, Farmer who, FarmAnimal a, SpellEffect effect)
        {
            if (effect.Sprites.Any())
            {
                foreach (var sprite in effect.Sprites)
                {
                    ApplySpriteToCharacter(l, a, sprite);
                }
            }
            switch (effect.EffectType)
            {
                case SpellEffectType.Pet:
                    a.pet(effect.AsFarmer ? who : new Farmer(), true);
                    break;
                case SpellEffectType.Harvest:
                    if(a.currentProduce.Value != null && a.isAdult())
                    {
                        Object produce = ItemRegistry.Create<Object>("(O)" + a.currentProduce.Value, 1, 0, false);
                        produce.CanBeSetDown = false;
                        produce.Quality = a.produceQuality.Value;
                        if (a.hasEatenAnimalCracker.Value)
                        {
                            produce.Stack = 2;
                        }
                        TryReturnObject(produce, who);
                        a.HandleStatsOnProduceCollected(produce, (uint)produce.Stack);
                        a.currentProduce.Value = null;
                        if(effect.AsFarmer)
                            a.friendshipTowardFarmer.Value = Math.Min(1000, a.friendshipTowardFarmer.Value + 5);
                        a.ReloadTextureIfNeeded(false);
                        who.gainExperience(0, 5);
                    }
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;

            }
        }

        private static void ApplyBuildingEffect(GameLocation l, Farmer who, Building a, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Water:
                    if (a is PetBowl pb)
                        pb.watered.Value = true;
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
            }
        }

        private static void ApplyPetEffect(GameLocation l, Farmer who, Pet a, SpellEffect effect)
        {
            if (effect.Sprites.Any())
            {
                foreach (var sprite in effect.Sprites)
                {
                    ApplySpriteToCharacter(l, a, sprite);
                }
            }
            switch (effect.EffectType)
            {
                case SpellEffectType.Pet:
                    a.checkAction(effect.AsFarmer ? who : new Farmer(), l);
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
            }
        }

        private static void ApplyTileEffect(GameLocation l, Farmer who, Vector2 tile, SpellEffect effect)
        {
            switch (effect.EffectType)
            {
                case SpellEffectType.Light:
                    var id = Guid.NewGuid().ToString();
                    l.sharedLights.AddLight(new(id, 1, tile * 64 + new Vector2(32, 32), effect.Radius, effect.Color));
                    LightDict.Add(id, new()
                    {
                        id = id,
                        location = l,
                        timeLeft = Convert.ToInt32(effect.Value),
                        totalTime = Convert.ToInt32(effect.Value),
                        radius = effect.Radius
                    });
                    break;
                case SpellEffectType.Tool:
                    PerformTool(l, tile, who, (string)effect.Value, effect.AsFarmer);
                    break;
                case SpellEffectType.Explode:
                    l.explode(tile, 0, who, effect.Affected.Contains(SpellAffectedType.Farmer), Convert.ToInt32(effect.Value), effect.Affected.Contains(SpellAffectedType.Object));
                    break;
            }
        }

        public static void ApplyMonsterEffect(GameLocation l, Farmer who, Monster a, SpellEffect effect)
        {
            if (effect.Sprites.Any())
            {
                foreach (var sprite in effect.Sprites)
                {
                    ApplySpriteToCharacter(l, a, sprite);
                }
            }
            switch (effect.EffectType)
            {
                case SpellEffectType.Damage:
                    a.takeDamage(Convert.ToInt32(effect.Value), 0, 0, false, 0, who);
                    break;
                case SpellEffectType.Buff:
                    BuffDict.Add(a, new MonsterBuffManager(a));
                    BuffDict[a].AddBuff((string)effect.Value);
                    break;
                case SpellEffectType.Freeze:
                    a.stunTime.Value = Convert.ToInt32(effect.Value);
                    break;
                case SpellEffectType.Light:
                    var id = Guid.NewGuid().ToString();
                    a.currentLocation.sharedLights.AddLight(new(id, 1, a.GetBoundingBox().Center.ToVector2(), effect.Radius, effect.Color));
                    LightDict.Add(id, new()
                    {
                        id = id,
                        location = a.currentLocation,
                        target = a,
                        timeLeft = Convert.ToInt32(effect.Value),
                        totalTime = Convert.ToInt32(effect.Value),
                        radius = effect.Radius
                    });
                        break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
            }
        }

        public static void ApplyNPCEffect(GameLocation l, Farmer who, NPC a, SpellEffect effect)
        {
            if (effect.Sprites.Any())
            {
                foreach (var sprite in effect.Sprites)
                {
                    ApplySpriteToCharacter(l, a, sprite);
                }
            }
            switch (effect.EffectType)
            {
                case SpellEffectType.Light:
                    var id = Guid.NewGuid().ToString();
                    a.currentLocation.sharedLights.AddLight(new(id, 1, a.GetBoundingBox().Center.ToVector2(), effect.Radius, effect.Color));
                    LightDict.Add(id, new()
                    {
                        id = id,
                        location = a.currentLocation,
                        target = a,
                        timeLeft = Convert.ToInt32(effect.Value),
                        totalTime = Convert.ToInt32(effect.Value),
                        radius = effect.Radius
                    });
                        break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
            }
        }

        public static void ApplyHorseEffect(GameLocation l, Farmer who, Horse a, SpellEffect effect)
        {
            if (effect.Sprites.Any())
            {
                foreach (var sprite in effect.Sprites)
                {
                    ApplySpriteToCharacter(l, a, sprite);
                }
            }
            switch (effect.EffectType)
            {
                case SpellEffectType.Light:
                    var id = Guid.NewGuid().ToString();
                    a.currentLocation.sharedLights.AddLight(new(id, 1, a.GetBoundingBox().Center.ToVector2(), effect.Radius, effect.Color));
                    LightDict.Add(id, new()
                    {
                        id = id,
                        location = a.currentLocation,
                        target = a,
                        timeLeft = Convert.ToInt32(effect.Value),
                        totalTime = Convert.ToInt32(effect.Value),
                        radius = effect.Radius
                    });
                        break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
            }
        }

        public static void ApplyFarmerEffect(GameLocation l, Farmer a, SpellEffect effect)
        {
            if (effect.Sprites?.Any() == true)
            {
                foreach (var sprite in effect.Sprites)
                {
                    ApplySpriteToCharacter(l, a, sprite);
                }
            }
            switch (effect.EffectType)
            {
                case SpellEffectType.Buff:
                    a.applyBuff((string)effect.Value);
                    break;
                case SpellEffectType.Damage:
                    a.takeDamage(Convert.ToInt32(effect.Value), true, null);
                    break;
                case SpellEffectType.Heal:
                    a.health = Math.Clamp(a.health + Convert.ToInt32(effect.Value), 0, a.maxHealth);
                    break;
                case SpellEffectType.Invincible:
                    a.temporarilyInvincible = true;
                    a.flashDuringThisTemporaryInvincibility = true;
                    a.temporaryInvincibilityTimer = 0;
                    a.currentTemporaryInvincibilityDuration = Convert.ToInt32(effect.Value);
                    break;
                case SpellEffectType.Light:
                    var id = Guid.NewGuid().ToString();
                    a.currentLocation.sharedLights.AddLight(new(id, 1, new Vector2(a.Position.X + 32f, a.Position.Y + 64f), effect.Radius, effect.Color, LightSource.LightContext.None, a.UniqueMultiplayerID));
                    LightDict.Add(id, new()
                    {
                        id = id,
                        location = a.currentLocation,
                        target = a,
                        timeLeft = Convert.ToInt32(effect.Value),
                        totalTime = Convert.ToInt32(effect.Value),
                        radius = effect.Radius
                    });
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;

                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
            }
        }

        public static void ApplyObjectEffect(GameLocation l, Farmer who, Vector2 tile, Object a, SpellEffect effect)
        {
            if (effect.Unaffected?.Count > 0)
            {
                if ((a.IsTwig() && effect.Unaffected.Contains(SpellAffectedType.Twig)) || (a.IsBreakableStone() && effect.Unaffected.Contains(SpellAffectedType.Stone)) || (a.IsWeeds() && effect.Unaffected.Contains(SpellAffectedType.Weed)))
                    return;
            }
            switch (effect.EffectType)
            {
                case SpellEffectType.Burn:
                    CreateBurn(l, tile, a, null);
                    break;
                case SpellEffectType.Tool:
                    PerformTool(l, tile, who, (string)effect.Value, effect.AsFarmer);
                    break;
                case SpellEffectType.Destroy:
                    l.Objects.Remove(tile);
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
            }
        }

        public static void ApplyResourceClumpEffect(GameLocation l, Farmer who, Vector2 tile, ResourceClump a, SpellEffect effect)
        {

            switch (effect.EffectType)
            {
                case SpellEffectType.Burn:
                    CreateBurn(l, tile, a, null);
                    break;
                case SpellEffectType.Tool:
                    PerformTool(l, tile, who, (string)effect.Value, effect.AsFarmer);
                    break;
                case SpellEffectType.Destroy:
                    l.Objects.Remove(tile);
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
            }
        }

        public static void ApplyTerrainFeatureEffect(GameLocation l, Farmer who, Vector2 tile, TerrainFeature a, SpellEffect effect)
        {

            if (effect.Unaffected?.Count > 0)
            {
                if ((a is HoeDirt dirt && (effect.Unaffected.Contains(SpellAffectedType.HoeDirt)|| (dirt.crop is not null && effect.Unaffected.Contains(SpellAffectedType.Crop)))) || (a is Grass && effect.Unaffected.Contains(SpellAffectedType.Grass)) || (a is Tree && effect.Unaffected.Contains(SpellAffectedType.Tree)) || (a is FruitTree && effect.Unaffected.Contains(SpellAffectedType.FruitTree)))
                    return;
            }
            switch (effect.EffectType)
            {
                case SpellEffectType.Burn:
                    CreateBurn(l, tile, a, null);
                    break;
                case SpellEffectType.Destroy:
                    l.terrainFeatures.Remove(tile);
                    break;
                case SpellEffectType.Fertilize:
                    if(a is HoeDirt)
                    {
                        (a as HoeDirt).plant((string)effect.Value, who, true);
                    }
                    break;
                case SpellEffectType.Grow:
                    if(a is Grass)
                    {
                        (a as Grass).numberOfWeeds.Value = effect.Value is null ? 4 : Convert.ToInt32(effect.Value);
                    }
                    else if (a is FruitTree)
                    {
                        (a as FruitTree).growthStage.Value = effect.Value is null ? 5 : Convert.ToInt32(effect.Value);
                    }
                    else if(a is Tree)
                    {
                        (a as Tree).growthStage.Value = effect.Value is null ? 4 : Convert.ToInt32(effect.Value);
                    }
                    break;
                case SpellEffectType.Harvest:
                    if(a is Grass)
                    {
                        (a as Grass).TryDropItemsOnCut((Tool)ItemRegistry.Create(effect.Value as string ?? "(W)47"), false);
                    }
                    else if (a is FruitTree && (a as FruitTree).struckByLightningCountdown.Value <= 0)
                    {
                        foreach (var f in (a as FruitTree).fruit)
                        {
                            TryReturnObject(f, who);
                        }
                        (a as FruitTree).fruit.Clear();
                    }
                    break;
                case SpellEffectType.Plant:
                    if(a is HoeDirt)
                    {
                        (a as HoeDirt).plant((string)effect.Value, who, false);
                    }
                    break;
                case SpellEffectType.Tool:
                    PerformTool(l, tile, who, (string)effect.Value, effect.AsFarmer);
                    break;
                case SpellEffectType.Water:
                    if (a is HoeDirt)
                    {
                        (a as HoeDirt).state.Value = 1;
                    }
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
            }
        }

        public static void ApplyCropEffect(GameLocation l, Farmer who, Vector2 tile, HoeDirt a, SpellEffect effect)
        {

            switch (effect.EffectType)
            {
                case SpellEffectType.Burn:
                    CreateBurn(l, tile, a, null);
                    break;
                case SpellEffectType.Grow:
                    if(effect.Value is null)
                    {
                        a.crop?.growCompletely();
                    }
                    else
                    {
                        a.crop.currentPhase.Value = Math.Min(a.crop.phaseDays.Count - 1, Convert.ToInt32(effect.Value));
                        a.crop.dayOfCurrentPhase.Value = 0;
                    }
                    break;
                case SpellEffectType.Harvest:
                    a.crop?.harvest((int)tile.X,(int)tile.Y, a, null, true);
                    break;
                case SpellEffectType.Tool:
                    PerformTool(l, tile, who, (string)effect.Value, effect.AsFarmer);
                    break;
                case SpellEffectType.Water:
                    a.state.Value = 1;
                    break;
                case SpellEffectType.Custom:
                    TrySetCustomVariable(a.crop, effect);
                    break;
                case SpellEffectType.ModData:
                    SetModData(a.modData, effect);
                    break;
            }
        }
    }
}