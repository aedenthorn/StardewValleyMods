using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace FertilizerFramework
{
	public partial class ModEntry
    {
        public static IEnumerable<CodeInstruction> ChangeMoveSoundTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr && new string[] { "croak", "fishSlap", "batFlap", "slimeHit", "squid_move", "Duggy", "dustMeep", "waterSlosh", "skeletonStep", "parry" }.Contains((string)codes[i].operand))
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeMoveSound))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                }
                else if (codes[i].opcode == OpCodes.Ldstr && new string[] { "squid_bubble", "hammer" }.Contains((string)codes[i].operand))
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeMoveSound2))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                }
                else if (codes[i].opcode == OpCodes.Ldstr && new string[] { "furnace", "fireball", "flameSpellHit", "skeletonHit" }.Contains((string)codes[i].operand))
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeProjectileSound))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                }
                else if (codes[i].opcode == OpCodes.Ldstr && new string[] { "flameSpell", "skeletonStep" }.Contains((string)codes[i].operand))
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeProjectileSound2))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                }
                else if (codes[i].opcode == OpCodes.Ldstr && new string[] { "rockGolemSpawn" }.Contains((string)codes[i].operand))
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeSpawnSound))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                }
            }

            return codes.AsEnumerable();
        }
        public static IEnumerable<CodeInstruction> ChangeDamageSoundTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "crafting")
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeArmorSound))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));

                }
                else if (codes[i].opcode == OpCodes.Ldstr && new string[] { "clank", "magma_sprite_hit", "hitEnemy", "rockGolemHit", "slimeHit", "shadowHit", "serpentHit", "skeletonHit" }.Contains((string)codes[i].operand))
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeDamageSound))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                }
                else if (codes[i].opcode == OpCodes.Ldstr && new string[] { "squid_hit", "skeletonStep" }.Contains((string)codes[i].operand))
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeDamageSound2))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                }
                else if (codes[i].opcode == OpCodes.Ldstr && new string[] { "magma_sprite_die", "batScreech", "monsterdead", "slimedead", "shadowDie", "ghost" }.Contains((string)codes[i].operand))
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeDeathSound))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                }
            }

            return codes.AsEnumerable();
        }


        public static IEnumerable<CodeInstruction> ChangeDeathSoundTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr && new string[] { "ghost", "fireball", "slimedead", "monsterdead", "dustMeep", "rockGolemDie", "serpentDie", "shadowDie", "skeletonDie" }.Contains((string)codes[i].operand))
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeDeathSound))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                }
                else if (codes[i].opcode == OpCodes.Ldstr && new string[] { "grunt" }.Contains((string)codes[i].operand))
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeDeathSound2))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                }
            }

            return codes.AsEnumerable();
        }

        public static IEnumerable<CodeInstruction> ChangeContactSoundTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr && new string[] { "slime" }.Contains((string)codes[i].operand))
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeContactSound))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    break;
                }
            }

            return codes.AsEnumerable();
        }

        public static bool ReloadSpritePrefix(Monster __instance)
        {
            if (!__instance.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data))
                return true;
            if (__instance is Bat b)
            {
                if (b.Sprite == null)
                {
                    b.Sprite = new AnimatedSprite(data.Sprite ?? "Characters\\Monsters\\" + __instance.Name);
                }
                else
                {
                    b.Sprite.textureName.Value = data.Sprite ?? "Characters\\Monsters\\" + __instance.Name;
                }
                if (b.hauntedSkull.Value)
                {
                    b.Sprite.SpriteHeight = 16;
                }
                b.HideShadow = true;
                return false;
            }
            if (__instance is BlueSquid)
            {
                __instance.Sprite = new AnimatedSprite(data.Sprite ?? "Characters\\Monsters\\Blue Squid", 0, 24, 24);
                return false;
            }
            if(data.Sprite != null)
            {
                __instance.Sprite = new AnimatedSprite(data.Sprite);
            }
            if (__instance is AngryRoger)
            {
                __instance.Sprite.SpriteWidth = 32;
                __instance.Sprite.SpriteHeight = 32;
                __instance.HideShadow = true;
            }
            else if (__instance is BigSlime)
            {
                __instance.Sprite.SpriteWidth = 32;
                __instance.Sprite.SpriteHeight = 32;
                __instance.Sprite.interval = 300f;
                __instance.Sprite.ignoreStopAnimation = true;
                AccessTools.FieldRefAccess<Character, bool>(__instance, "ignoreMovementAnimations") = true;
                __instance.HideShadow = true;
                __instance.Sprite.framesPerAnimation = 8;
            }
            else if (__instance is Bug)
            {
                __instance.Sprite.SpriteHeight = 16;
            }
            else if (__instance is DinoMonster)
            {
                __instance.Sprite.SpriteWidth = 32;
                __instance.Sprite.SpriteHeight = 32;
            }
            else if (__instance is GreenSlime)
            {
                __instance.Sprite.SpriteHeight = 24;
                __instance.HideShadow = true;
            }
            else if (__instance is LavaLurk)
            {
                __instance.Sprite.SpriteWidth = 16;
                __instance.Sprite.SpriteHeight = 16;
            }
            else if (__instance is Leaper)
            {
                __instance.Sprite.SpriteWidth = 32;
                __instance.Sprite.SpriteHeight = 32;
            }
            else if (__instance is MetalHead mh)
            {
                __instance.Sprite.SpriteHeight = 16;
            }
            else if (__instance is Mummy)
            {
                __instance.Sprite.SpriteHeight = 32;
                __instance.Sprite.ignoreStopAnimation = true;
            }
            else if (__instance is Serpent s)
            {
                if (data.Scale > -1) 
                {
                    __instance.Scale =  data.Scale;
                }
                else 
                { 
                    __instance.Scale = s.IsRoyalSerpent() ? 1f : 0.75f; 
                }
                __instance.Sprite.SpriteWidth = 32;
                __instance.Sprite.SpriteHeight = 32;
                __instance.HideShadow = true;
            }
            else if (__instance is ShadowBrute)
            {
                __instance.Sprite.SpriteHeight = 32;
            }
            else if (__instance is Shooter sh)
            {
                sh.Sprite.SpriteHeight = 32;
                sh.Sprite.SpriteWidth = 32;
                sh.forceOneTileWide.Value = true;
                sh.InitializeVariant();
            }
            else if (__instance is Skeleton)
            {
                __instance.Sprite.SpriteHeight = 32;
            }
            else if (__instance is Spiker)
            {
                __instance.Sprite.SpriteWidth = 16;
                __instance.Sprite.SpriteHeight = 16;
                __instance.HideShadow = true;
            }
            if (data.HideShadow != null)
            {
                __instance.HideShadow = data.HideShadow.Value;
            }
            __instance.Sprite.UpdateSourceRect();
            return false;
        }
        private static void GetExtraDropItemsPostfix(Monster __instance, ref List<Item> __result)
        {
            if (!__instance.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data))
                return;
            if (data.ClearDrops)
            {
                __instance.objectsToDrop.Clear();
                __result?.Clear();
            }
            if (data.Drops == null)
                return;
            __result ??= new List<Item>();
            foreach (var item in data.Drops)
            {
                if (Game1.random.NextDouble() < item.Chance / 100.0)
                {
                    __result.Add(ItemRegistry.Create(item.ItemId, Game1.random.Next(item.MinQuantity, item.MaxQuantity + 1), item.Quality));
                }
            }
            if(__instance is BigSlime bs && bs.heldItem.Value != null)
            {
                __result.Add(bs.heldItem.Value);
            }
        }
        [HarmonyPatch(typeof(DwarvishSentry), new Type[] { typeof(Vector2) })]
        [HarmonyPatch(MethodType.Constructor)]
        public static class DwarvishSentry_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling DwarvishSentry.ctor");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "DwarvishSentry")
                    {
                        SMonitor.Log($"adding method for spawn sound");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeSpawnSound))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(GreenSlime), "doJump")]
        public static class GreenSlime_doJump_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling GreenSlime.doJump");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "slime")
                    {
                        SMonitor.Log($"adding method for move sound");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeMoveSound))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(GreenSlime), nameof(GreenSlime.onDealContactDamage))]
        public static class GreenSlime_onDealContactDamage_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling GreenSlime.onDealContactDamage");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "13")
                    {
                        SMonitor.Log($"adding method for contact debuff");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeContactDebuff))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(GreenSlime), "draw", new Type[] { typeof(SpriteBatch) })]
        public static class GreenSlime_draw_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling GreenSlime.draw");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 120000)
                    {
                        SMonitor.Log($"adding method for childhood length");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeChildhoodLength))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.withinPlayerThreshold), new Type[] { typeof(int) })]
        public static class NPC_withinPlayerThreshold_Patch
        {
            public static void Prefix(NPC __instance, ref int threshold)
            {
                if (__instance is not Monster m || !TryGetData(m, out var data) || data.PlayerThreshold < 0)
                    return;
                threshold = data.PlayerThreshold;
            }
        }
        [HarmonyPatch(typeof(GreenSlime), nameof(GreenSlime.mateWith))]
        public static class GreenSlime_mateWith_Patch
        {
            public static bool Prefix(GreenSlime __instance, GreenSlime mateToPursue, GameLocation location)
            {
                if (!TryGetData(__instance, out var data) || !TryGetData(mateToPursue, out var data2))
                    return true;
                if(data == null || data2 == null || data.MonsterId != data2.MonsterId)
                {
                    return false;
                }
                if (location.canSlimeMateHere())
                {
                    GreenSlime baby = (GreenSlime)CreateMonster(data.MonsterId, Vector2.Zero);
                    Utility.recursiveFindPositionForCharacter(baby, location, __instance.Tile, 30);
                    Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame / 10.0, (double)__instance.Scale * 100.0, (double)mateToPursue.Scale * 100.0, 0.0);
                    
                    baby.Health = r.Choose(__instance.Health, mateToPursue.Health);
                    baby.Health = Math.Max(1, __instance.Health + r.Next(-4, 5));
                    baby.DamageToFarmer = r.Choose(__instance.DamageToFarmer, mateToPursue.DamageToFarmer);
                    baby.DamageToFarmer = Math.Max(0, __instance.DamageToFarmer + r.Next(-1, 2));
                    baby.resilience.Value = r.Choose(__instance.resilience.Value, mateToPursue.resilience.Value);
                    baby.resilience.Value = Math.Max(0, __instance.resilience.Value + r.Next(-1, 2));
                    baby.missChance.Value = r.Choose(__instance.missChance.Value, mateToPursue.missChance.Value);
                    baby.missChance.Value = Math.Max(0.0, __instance.missChance.Value + (double)((float)r.Next(-1, 2) / 100f));
                    baby.Scale = r.Choose(__instance.Scale, mateToPursue.Scale);
                    baby.Scale = Math.Max(0.6f, Math.Min(1.5f, __instance.Scale + (float)r.Next(-2, 3) / 100f));
                    baby.Slipperiness = 8;
                    __instance.speed = r.Choose(__instance.speed, mateToPursue.speed);
                    if (r.NextDouble() < 0.015)
                    {
                        __instance.speed = Math.Max(1, Math.Min(6, __instance.speed + r.Next(-1, 2)));
                    }
                    baby.setTrajectory(Utility.getAwayFromPositionTrajectory(baby.GetBoundingBox(), __instance.getStandingPosition()) / 2f);
                    baby.ageUntilFullGrown.Value = data.ChildhoodLength ?? 120000;
                    baby.Halt();
                    baby.firstGeneration.Value = false;
                    if (Utility.isOnScreen(__instance.Position, 128))
                    {
                        __instance.currentLocation.playSound(data.SpawnSound ?? "slime", null, null, SoundContext.Default);
                    }
                }
                mateToPursue.doneMating();
                __instance.doneMating();
                return false;
            }

        }
        [HarmonyPatch(typeof(Mummy), "performCrumble")]
        public static class Mummy_performCrumble_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Mummy.performCrumble");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "monsterdead")
                    {
                        SMonitor.Log($"adding method for crumble sound");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeCrumbleSound))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                    else if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "skeletonDie")
                    {
                        SMonitor.Log($"adding method for uncrumble sound");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeUncrumbleSound))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                    else if (codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == 10000)
                    {
                        SMonitor.Log($"adding method for revive timer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeReviveTimer))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(RockCrab), "hitWithTool")]
        public static class RockCrab_hitWithTool_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling RockCrab_hitWithTool");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "hammer")
                    {
                        SMonitor.Log($"adding method for hit sound");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeHitSound))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                    else if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "stoneCrack")
                    {
                        SMonitor.Log($"adding method for break sound");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeBreakSound))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(HotHead), "DropBomb")]
        public static class HotHead_DropBomb_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling HotHead_DropBomb");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "Characters\\Monsters\\Hot Head")
                    {
                        SMonitor.Log($"adding method for sprite path");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeSpritePath))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        

        [HarmonyPatch(typeof(LavaLurk), "behaviorAtGameTick")]
        public static class LavaLurk_behaviorAtGameTick_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling LavaLurk.behaviorAtGameTick");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 4 && codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(GameLocation), nameof(GameLocation.playSound)) && codes[i + 1].opcode == OpCodes.Ldc_I4_S && codes[i + 1].opcode == OpCodes.Ldc_I4_S)
                    {
                        SMonitor.Log($"adding methods for projectile");
                        codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeProjectileIndex))));
                        codes.Insert(i + 3, new CodeInstruction(OpCodes.Ldarg_0));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeProjectileDamage))));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(SquidKid), "behaviorAtGameTick")]
        public static class SquidKid_behaviorAtGameTick_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling SquidKid.behaviorAtGameTick");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i].operand == 15 && codes[i + 1].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i + 1].operand == 10)
                    {
                        SMonitor.Log($"adding methods for projectile");
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeProjectileIndex))));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeProjectileDamage))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 4;
                    }
                    else if (codes[i].opcode == OpCodes.Stfld && codes[i].operand is FieldInfo fi && fi == AccessTools.Field(typeof(SquidKid), "numFireballsLeft") && codes[i - 1].opcode == OpCodes.Ldc_I4_4)
                    {
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeProjectileCount))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                    else if (codes[i].opcode == OpCodes.Stfld && codes[i].operand is FieldInfo fi2 && fi2 == AccessTools.Field(typeof(SquidKid), "firingTimer") && codes[i - 1].opcode == OpCodes.Ldc_R4 && (float)codes[i - 1].operand == 400)
                    {
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeProjectileTimer))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Skeleton), "behaviorAtGameTick")]
        public static class Skeleton_behaviorAtGameTick_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Skeleton.behaviorAtGameTick");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo fi && fi == AccessTools.Field(typeof(GameLocation), nameof(GameLocation.projectiles)))
                    {
                        if (codes[i + 1].opcode == OpCodes.Ldstr)
                        {
                            SMonitor.Log($"adding method for debuff projectile");
                            codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeProjectileIndex))));
                            codes.Insert(i + 3, new CodeInstruction(OpCodes.Ldarg_0));
                            codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeProjectileDebuff))));
                            codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                        }
                        else if (codes[i + 1].opcode == OpCodes.Ldarg_0)
                        {
                            if(codes[i + 3].opcode == OpCodes.Ldc_I4_4)
                            {
                                SMonitor.Log($"adding method for basic projectile");
                                codes.Insert(i + 4, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeProjectileIndex))));
                                codes.Insert(i + 4, new CodeInstruction(OpCodes.Ldarg_0));
                            }
                            else if(i < codes.Count - 6 && codes[i + 5].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i+5].operand == 9)
                            {
                                SMonitor.Log($"adding method for basic projectile");
                                codes.Insert(i + 6, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeProjectileIndex))));
                                codes.Insert(i + 6, new CodeInstruction(OpCodes.Ldarg_0));
                            }
                        }
                    }
                }

                return codes.AsEnumerable();
            }
        }



        [HarmonyPatch(typeof(DinoMonster.BreathProjectile), "Draw")]
        public static class DinoMonster_BreathProjectile_Draw_Patch
        {
            public static bool Prefix(DinoMonster.BreathProjectile __instance, SpriteBatch b)
            {
                if (__instance is not CustomBreathProjectile bp)
                    return true;
                if (!bp.active.Value)
                {
                    return false;
                }
                float currentScale = bp.data.ProjectileScale ?? 4f;
                Texture2D texture = bp.data.ProjectileSprite == null ? Projectile.projectileSheet : SHelper.GameContent.Load<Texture2D>(bp.data.ProjectileSprite);
                Rectangle sourceRect = bp.data.ProjectileSource == null ? Game1.getSourceRectForStandardTileSheet(texture, bp.data.ProjectileIndex ?? 0, 16, 16) : bp.data.ProjectileSource.Value;

                Vector2 pixelPosition = bp.position.Value;
                b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, pixelPosition + new Vector2(32f, 32f)), new Rectangle?(sourceRect), Color.White * bp.alpha, bp.rotation, new Vector2(8f, 8f), currentScale, SpriteEffects.None, (pixelPosition.Y + 96f) / 10000f); return false;
            }
        }

        [HarmonyPatch(typeof(ShadowShaman), "draw")]
        public static class ShadowShaman_draw_Patch
        {
            public static bool Prefix(ShadowShaman __instance, SpriteBatch b)
            {
                if(!Config.ModEnabled || !__instance.casting.Value || !__instance.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || (data.ProjectileSprite == null && data.ProjectileIndex == null))
                    return true;
                var method = typeof(Monster).GetMethod("draw", new Type[] { typeof(SpriteBatch) });
                var ftn = method.MethodHandle.GetFunctionPointer();
                var func = (Action<SpriteBatch>)Activator.CreateInstance(typeof(Action<SpriteBatch>), __instance, ftn);
                func(b);
                var texture = data.ProjectileSprite == null ? Projectile.projectileSheet : SHelper.GameContent.Load<Texture2D>(data.ProjectileSprite);
                var src = data.ProjectileSource == null ? (data.ProjectileIndex == null ? new Rectangle?(new Rectangle(119, 6, 3, 3)) : Game1.getSourceRectForStandardTileSheet(texture, data.ProjectileIndex.Value, 16, 16)) : data.ProjectileSource;
                for (int i = 0; i < 8; i++)
                {
                    b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, __instance.getStandingPosition()), src, Color.White * 0.7f, __instance.rotationTimer + (float)i * 3.1415927f / 4f, new Vector2(8f, 48f), 6f, SpriteEffects.None, 0.95f);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Spiker), "takeDamage")]
        public static class Spiker_takeDamage_Patch
        {
            public static bool Prefix(Spiker __instance, int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
            {
                if(!Config.ModEnabled || !__instance.modData.TryGetValue(monsterKey, out var id) || !Monsters.TryGetValue(id, out var data) || data.Vulnerable != true)
                    return true;
                var method = typeof(Monster).GetMethod("takeDamage", new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) });
                var ftn = method.MethodHandle.GetFunctionPointer();
                var func = (Action<int, int, int, bool, double, Farmer>)Activator.CreateInstance(typeof(Action<int, int, int, bool, double, Farmer>), __instance, ftn);
                func(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
                return false;
            }
        }

        [HarmonyPatch(typeof(DinoMonster.BreathProjectile), "Update")]
        public static class DinoMonster_BreathProjectile_Update_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling BreathProjectile.Update");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(Farmer), nameof(Farmer.takeDamage)))
                    {
                        SMonitor.Log($"adding method for sprinkle color");
                        codes.Insert(i - 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeProjectileDamage2))));
                        codes.Insert(i - 2, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }


        [HarmonyPatch(typeof(AngryRoger), nameof(AngryRoger.takeDamage))]
        public static class AngryRoger_takeDamage_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling AngryRoger.takeDamage");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi && mi == AccessTools.PropertyGetter(typeof(Color), nameof(Color.LightBlue)))
                    {
                        SMonitor.Log($"adding method for sprinkle color");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeSprinkleColor))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(AngryRoger), "updateAnimation")]
        public static class AngryRoger_updateAnimation_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling AngryRoger.updateAnimation");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo fi && fi == AccessTools.Field(typeof(AngryRoger), nameof(AngryRoger.lightSourceId)) && codes[i + 1].opcode == OpCodes.Ldc_I4_5)
                    {
                        SMonitor.Log($"adding method for light type");
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeLightType))));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }

        }

        [HarmonyPatch(typeof(Ghost), "updateAnimation")]
        public static class Ghost_updateAnimation_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Ghost.updateAnimation");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo fi && fi == AccessTools.Field(typeof(Ghost), nameof(Ghost.lightSourceId)) && (codes[i + 1].opcode == OpCodes.Ldc_I4_5 || codes[i + 1].opcode == OpCodes.Ldc_I4_4))
                    {
                        SMonitor.Log($"adding method for light type");
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeLightType))));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }

        }

        [HarmonyPatch(typeof(MineShaft), "getMonsterForThisLevel")]
        public static class MineShaft_getMonsterForThisLevel_Patch
        {
            public static void Postfix(MineShaft __instance, int level, int xTile, int yTile, ref Monster __result)
            {
                if (!Config.ModEnabled)
                    return;
                foreach (var kvp in Monsters.Where(kvp => kvp.Value.MineSpawns != null))
                {
                    var monster = GetSpawnMonster(__result, kvp.Key, kvp.Value.MineSpawns, level, __instance.GetAdditionalDifficulty(), new Vector2(xTile, yTile) * 64);
                    if (monster != null)
                    {
                        __result = monster;
                        SMonitor.Log($"Spawning monster {kvp.Key} at mine level {level}");
                        return;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(VolcanoDungeon), "GenerateEntities")]
        public static class VolcanoDungeon_GenerateEntities_Patch
        {
            public static void Postfix(VolcanoDungeon __instance)
            {
                if (!Config.ModEnabled)
                    return;
                for(int i = 0; i< __instance.characters.Count; i++)
                {
                    if( __instance.characters[i] is Monster old) 
                    {
                        foreach (var kvp in Monsters.Where(m => m.Value.VolcanoSpawns != null))
                        {
                            var m = GetSpawnMonster(old, kvp.Key, kvp.Value.VolcanoSpawns, __instance.level.Value, 0, old.Position);
                            if (m != null)
                            {
                                SMonitor.Log($"Spawning monster {kvp.Key} at volcano level {__instance.level.Value}");
                                __instance.characters[i] = m;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
