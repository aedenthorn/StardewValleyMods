using HarmonyLib;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ImmersiveSprinklersAndScarecrows
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Farm), nameof(Farm.addCrows))]
        public class Farm_addCrows_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Farm.addCrows");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 4 && codes[i].opcode == OpCodes.Ldloc_S && codes[i + 1].opcode == OpCodes.Brtrue_S && codes[i + 4].opcode == OpCodes.Callvirt && codes[i + 4].operand is MethodInfo && (MethodInfo)codes[i + 4].operand == AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.destroyCrop)))
                    {
                        SMonitor.Log("Adding check for scarecrow at vector");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.IsScarecrowInRange))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldloc_S, 11));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}