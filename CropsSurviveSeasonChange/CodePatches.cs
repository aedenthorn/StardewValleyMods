using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CropsSurviveSeasonChange
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Crop), nameof(Crop.newDay))]
        public class Billboard_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Crop.newDay");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Crop), nameof(Crop.Kill)))
                    {
                        SMonitor.Log($"adding method to prevent killing");
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(CheckKill));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_S, 5));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}