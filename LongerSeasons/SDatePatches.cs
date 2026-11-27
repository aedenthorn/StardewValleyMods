using HarmonyLib;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LongerSeasons
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {


        public static IEnumerable<CodeInstruction> SDate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling SDate()");

            var codes = new List<CodeInstruction>(instructions);
            var outCodes = new List<CodeInstruction>();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldarg_1 && codes[i+1].opcode == OpCodes.Ldarg_0 && codes[i+2].opcode == OpCodes.Ldfld && ((FieldInfo)codes[i + 2].operand).Name == "DaysInSeason")
                {
                    SMonitor.Log($"Avoiding SMAPI {((FieldInfo)codes[i + 2].operand).Name} {Config.DaysPerMonth}");
                    codes[i + 1] = new CodeInstruction(OpCodes.Ldc_I4, Config.DaysPerMonth);
                    outCodes.Add(codes[i++]);
                    outCodes.Add(codes[i++]);
                    i++;
                }
                outCodes.Add(codes[i]);
            }

            return outCodes.AsEnumerable();
        }
        private static void SDate_Postfix(SDate __instance)
        {
            typeof(SDate).GetField("DaysInSeason", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, Config.DaysPerMonth);
        }

    }
}