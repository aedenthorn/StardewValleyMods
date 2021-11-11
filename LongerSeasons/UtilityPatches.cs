using HarmonyLib;
using StardewModdingAPI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace LongerSeasons
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {

        private static void Utility_getSeasonNameFromNumber_Postfix(ref string __result)
        {
            if(Config.MonthsPerSeason > 1 && Context.IsWorldReady)
            {
                __result += $" {(SHelper.Data.ReadSaveData<SeasonMonth>(context.GetType().Namespace) ?? new SeasonMonth()).month}";
            }
        }
        public static IEnumerable<CodeInstruction> Utility_getDateStringFor_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Utility.getDateStringFor");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i].operand == 28)
                {
                    SMonitor.Log($"Changing days per month to {Config.DaysPerMonth}");
                    codes[i] = new CodeInstruction(OpCodes.Ldc_I4, Config.DaysPerMonth);
                }
            }

            return codes.AsEnumerable();
        }
    }
}