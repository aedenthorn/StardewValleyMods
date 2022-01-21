using HarmonyLib;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CustomTreeDrops
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        public static IEnumerable<CodeInstruction> Tree_performTreeFall_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Tree_performTreeFall");

            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>();
            for (int i = 0; i < codes.Count; i++)
            {
                if (i < codes.Count - 12 && codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 12].opcode == OpCodes.Call && (MethodInfo)codes[i + 12].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), new System.Type[] { typeof(int),typeof(int),typeof(int),typeof(int),typeof(GameLocation)}))
                {
                    SMonitor.Log($"Switching drop method");
                    newCodes.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newCodes.Add(codes[i]);
                    codes[i+12].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DropItems));
                }
                else if (i > 6 && codes[i - 7].opcode == OpCodes.Ldc_I4_S && (int)codes[i - 7].operand == 92 && codes[i].opcode == OpCodes.Ldc_I4_2)
                {
                    SMonitor.Log($"Converting sap to drop x2");
                    newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetSapToDropTwice))));
                }
                else if (i > 0 && codes[i - 1].opcode == OpCodes.Ldc_I4_S && (int)codes[i - 1].operand == 92 && codes[i].opcode == OpCodes.Ldc_I4_1)
                {
                    SMonitor.Log($"Converting sap to drop");
                    newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetSapToDrop))));
                }
                else
                    newCodes.Add(codes[i]);
            }

            return newCodes.AsEnumerable();
        }
    }
}