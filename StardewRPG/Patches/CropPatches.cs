using HarmonyLib;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace StardewRPG
{
    public partial class ModEntry
    {
        public static IEnumerable<CodeInstruction> Crop_harvest_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Crop.harvest");

            var codes = new List<CodeInstruction>(instructions);
            for(int i = 0; i < codes.Count; i++)
            {
                if (i < codes.Count - 6 && codes[i].opcode == OpCodes.Stloc_S && codes[i + 1].opcode == OpCodes.Ldc_R8 && (double)codes[i + 1].operand == 0.75 && codes[i + 2].opcode == OpCodes.Ldloc_S && codes[i + 6].opcode == OpCodes.Stloc_S)
                {
                    SMonitor.Log("Overriding crop quality");
                    codes.Insert(i + 6, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetChanceForSilverCrop))));
                    codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetChanceForGoldCrop))));
                    break;
                }
            }
            return codes.AsEnumerable();
        }

        private static double GetChanceForGoldCrop(double chance)
        {
            return chance + GetStatMod(GetStatValue(Game1.player, "int", Config.BaseStatValue)) * Config.IntCropQualityBonus;
        }

        private static double GetChanceForSilverCrop(double chance)
        {
            return chance + GetStatMod(GetStatValue(Game1.player, "int", Config.BaseStatValue)) * Config.IntCropQualityBonus;
        }
    }
}