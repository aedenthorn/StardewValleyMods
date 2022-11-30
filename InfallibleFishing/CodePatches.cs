using HarmonyLib;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace InfallibleFishing
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(BobberBar), nameof(BobberBar.update))]
        public class BobberBar_update_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling BobberBar.update");

                var codes = new List<CodeInstruction>(instructions);
                for(int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 2 && codes[i].opcode == OpCodes.Ldfld  && codes[i + 1].opcode == OpCodes.Ldc_R4  && codes[i + 2].opcode == OpCodes.Bgt_Un_S && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(BobberBar), "distanceFromCatching") && (float)codes[i + 1].operand == 0)
                    {
                        SMonitor.Log($"Lowering min distance from catching to -1");
                        codes[i + 1].operand = -1f;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}