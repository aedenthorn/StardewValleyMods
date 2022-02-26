using StardewValley;
using StardewValley.Tools;
using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace StardewRPG
{
    public partial class ModEntry
    {
        public static IEnumerable<CodeInstruction> GameLocation_performTenMinuteUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling GameLocation.performTenMinuteUpdate");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i > 9 && codes[i].opcode == OpCodes.Ldc_R8 && (double)codes[i].operand == 0.5 && codes[i - 9].opcode == OpCodes.Ldfld && (FieldInfo)codes[i - 9].operand == AccessTools.Field(typeof(GameLocation), nameof(GameLocation.fishSplashPoint)))
                {
                    SMonitor.Log("Overriding chance for fish splash spot");
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetFishSplashPointChance));
                    break;
                }
            }
            return codes.AsEnumerable();
        }

        private static double GetFishSplashPointChance()
        {
            return 0.5 + GetStatMod(GetStatValue(Game1.player, "int", Config.BaseStatValue)) * Config.IntFishSpotChanceBonus;
        }

        public static IEnumerable<CodeInstruction> GameLocation_performOrePanTenMinuteUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling GameLocation.performOrePanTenMinuteUpdate");

            var codes = new List<CodeInstruction>(instructions);
            if (codes[18].opcode == OpCodes.Ldc_R8 && (double)codes[18].operand == 0.5)
            {
                SMonitor.Log("Overriding pan spot chance");
                codes[18].opcode = OpCodes.Call;
                codes[18].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetPanSpotChance));
            }
            else
            {
                SMonitor.Log("Couldn't override pan spot ", StardewModdingAPI.LogLevel.Error);
            }
            return codes.AsEnumerable();
        }

        private static double GetPanSpotChance()
        {
            return 0.5 + GetStatMod(GetStatValue(Game1.player, "int", Config.BaseStatValue)) * Config.IntPanSpotChanceBonus;
        }

        public static IEnumerable<CodeInstruction> GameLocation_spawnObjects_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling GameLocation.spawnObjects");

            var codes = new List<CodeInstruction>(instructions);
            bool found1 = false;
            bool found2 = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (!found1 && i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldloc_S && codes[i + 1].opcode == OpCodes.Ldc_R8 && (double)codes[i + 1].operand == 0.75 && codes[i + 2].opcode == OpCodes.Mul && codes[i + 3].opcode == OpCodes.Stloc_S && codes[i].operand == codes[i+3].operand)
                {
                    SMonitor.Log("Overriding chance for new artifact attempt");
                    codes[i+1].opcode = OpCodes.Call;
                    codes[i+1].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetChanceForNewArtifactMult));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    found1 = true; 
                }
                else if (!found2 && i < codes.Count - 5 && codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 1].opcode == OpCodes.Ldc_I4_1 && codes[i + 2].opcode == OpCodes.Ldc_I4_5 && codes[i + 3].opcode == OpCodes.Ldc_I4_7 && codes[i + 4].opcode == OpCodes.Ldarg_0)
                {
                    SMonitor.Log("Overriding number of foraging spots");
                    codes[i+1].opcode = OpCodes.Call;
                    codes[i+1].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMinForagingSpots));
                    codes[i+2].opcode = OpCodes.Call;
                    codes[i+2].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMaxForagingSpots1));
                    codes[i+3].opcode = OpCodes.Call;
                    codes[i+3].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMaxForagingSpots2));
                    found2 = true;
                }
                if (found1 && found2)
                    break;
            }
            return codes.AsEnumerable();
        }

        private static int GetMinForagingSpots()
        {
            return 1 + GetStatMod(GetStatValue(Game1.player, "int", Config.BaseStatValue)) * Config.IntForagingSpotChanceBonus;
        }
        private static int GetMaxForagingSpots1()
        {
            return 5 + GetStatMod(GetStatValue(Game1.player, "int", Config.BaseStatValue)) * Config.IntForagingSpotChanceBonus;
        }
        private static int GetMaxForagingSpots2()
        {
            return 7 + GetStatMod(GetStatValue(Game1.player, "int", Config.BaseStatValue)) * Config.IntForagingSpotChanceBonus;
        }

        private static double GetChanceForNewArtifactMult(GameLocation location)
        {
            double chance = 0.75 + GetStatMod(GetStatValue(Game1.player, "int", Config.BaseStatValue)) * Config.IntArtifactSpotChanceBonus;
            return Math.Min(location.GetSeasonForLocation().Equals("winter") ? 0.85 : 0.95, chance);
        }

        private static void GameLocation_damageMonster_Prefix(ref int minDamage, ref int maxDamage, bool isBomb, ref int addedPrecision, ref float critChance, ref float critMultiplier, Farmer who)
        {
            if (!Config.EnableMod || who != Game1.player || isBomb)
                return;
            if (who.CurrentTool is not MeleeWeapon)
                return;

            float damageMod = 0;
            var str = GetStatValue(who, "str", Config.BaseStatValue);
            var con = GetStatValue(who, "con", Config.BaseStatValue);
            var dex = GetStatValue(who, "dex", Config.BaseStatValue);
            switch ((who.CurrentTool as MeleeWeapon).type.Value)
            {
                case 1: // dagger
                    damageMod = Config.DexDaggerDamageBonus * GetStatMod(dex);
                    SMonitor.Log($"modify dagger damage {damageMod}");
                    break;
                case 2: // club
                    damageMod = Config.StrClubDamageBonus * GetStatMod(str);
                    SMonitor.Log($"modify club damage {damageMod}");
                    break;
                case 3: // sword
                    damageMod = Config.ConSwordDamageBonus * GetStatMod(con);
                    SMonitor.Log($"modify sword damage {damageMod}");
                    break;
            }
            SMonitor.Log($"old min {minDamage}, max {maxDamage}, crit chance {critChance}, {critMultiplier}");
            minDamage = (int)Math.Round(minDamage * (1 + damageMod));
            maxDamage = (int)Math.Round(maxDamage * (1 + damageMod));
            critChance *= 1 + GetStatMod(dex) * Config.DexCritChanceBonus;
            critMultiplier *= 1 + GetStatMod(str) * Config.StrCritDamageBonus;
            SMonitor.Log($"new min {minDamage}, max {maxDamage}, crit chance {critChance}, {critMultiplier}");
        }

        private static void GameLocation_draw_Prefix(GameLocation __instance, SpriteBatch b)
        {
            if (!Config.EnableMod)
                return;
            int wis = GetStatValue(Game1.player, "wis", Config.BaseStatValue);
            if (wis == 10)
                return;
            float val = (wis - 10) * Config.WisSpotVisibility;
            double tick = Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1200;
            if (__instance.fishSplashAnimation != null)
            {
                if (val < 0)
                    __instance.fishSplashAnimation.color *= 1 + val;
                else if (tick < 300)
                {
                    __instance.fishSplashAnimation.position = new Vector2((float)Math.Ceiling(__instance.fishSplashAnimation.position.X / 64) * 64, (float)Math.Ceiling(__instance.fishSplashAnimation.position.Y / 64) * 64);
                    __instance.fishSplashAnimation.scale = 1;
                }
                else if (tick < 600)
                {
                    __instance.fishSplashAnimation.position = new Vector2((float)Math.Ceiling(__instance.fishSplashAnimation.position.X / 64) * 64 - val / 4 * 32, (float)Math.Ceiling(__instance.fishSplashAnimation.position.Y / 64) * 64 - val / 4 * 32);
                    __instance.fishSplashAnimation.scale = 1 + val / 4;
                }
                else if (tick < 900)
                {
                    __instance.fishSplashAnimation.position = new Vector2((float)Math.Ceiling(__instance.fishSplashAnimation.position.X / 64) * 64 - val / 2 * 32, (float)Math.Ceiling(__instance.fishSplashAnimation.position.Y / 64) * 64 - val / 2 * 32);
                    __instance.fishSplashAnimation.scale = 1 + val / 2;
                }
                else
                {
                    __instance.fishSplashAnimation.position = new Vector2((float)Math.Ceiling(__instance.fishSplashAnimation.position.X / 64) * 64 - val / 4 * 32, (float)Math.Ceiling(__instance.fishSplashAnimation.position.Y / 64) * 64 - val / 4 * 32);
                    __instance.fishSplashAnimation.scale = 1 + val / 4;
                }
            }
            if (__instance.orePanAnimation != null)
            {
                if (val < 0)
                    __instance.orePanAnimation.color *= 1 + val;
                else if (tick < 300)
                    __instance.orePanAnimation.scale = 1;
                else if (tick < 600)
                    __instance.orePanAnimation.scale = 1 + val / 2;
                else if (tick < 900)
                    __instance.orePanAnimation.scale = 1 + val;
                else
                    __instance.orePanAnimation.scale = 1 + val / 2;
            }
        }
    }
}