using StardewValley;
using StardewValley.Tools;
using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace StardewRPG
{
    public partial class ModEntry
    {
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
                    codes[i+2].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMinForagingSpots));
                    codes[i+3].opcode = OpCodes.Call;
                    codes[i+3].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMinForagingSpots));
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
                    break;
                case 2: // club
                    damageMod = Config.StrClubDamageBonus * GetStatMod(str);
                    break;
                case 3: // sword
                    damageMod = Config.ConSwordDamageBonus * GetStatMod(con);
                    break;
            }
            minDamage = (int)Math.Round(minDamage * (1 + damageMod));
            maxDamage = (int)Math.Round(maxDamage * (1 + damageMod));
            critChance *= 1 + GetStatMod(dex) * Config.DexCritChanceBonus;
            critMultiplier *= 1 + GetStatMod(str) * Config.StrCritDamageBonus;
        }
    }
}