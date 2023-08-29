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
                    if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(GameLocation), nameof(GameLocation.isOutdoors)))
                    {
                        SMonitor.Log($"adding method to prevent killing");
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(CheckKill))));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_S, 5));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}