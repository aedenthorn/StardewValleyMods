using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LongerSeasons
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {


        public static IEnumerable<CodeInstruction> Game1__newDayAfterFade_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Game1._newDayAfterFade");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i].operand == typeof(Game1).GetField(nameof(Game1.dayOfMonth), BindingFlags.Public | BindingFlags.Static) && codes[i + 1].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i + 1].operand == 29)
                {
                    SMonitor.Log($"Changing days per month to {Config.DaysPerMonth}");
                    codes[i + 1].operand = Config.DaysPerMonth + 1;
                    codes[i + 2].opcode = OpCodes.Blt_Un_S;
                    break;
                }
            }

            return codes.AsEnumerable();
        }

        private static void Game1__newDayAfterFade_Prefix()
        {
            SMonitor.Log($"dom {Game1.dayOfMonth}, year {Game1.year}, season {Game1.currentSeason}");
        }
        private static bool Game1_newSeason_Prefix()
        {
            SMonitor.Log($"{Environment.StackTrace} dom {Game1.dayOfMonth}, year {Game1.year}, season {Game1.currentSeason}");

            var model = SHelper.Data.ReadSaveData<SeasonMonth>(context.GetType().Namespace) ?? new SeasonMonth();
            if (model.month >= Config.MonthsPerSeason)
            {
                model.month = 1;
                SHelper.Data.WriteSaveData(context.GetType().Namespace, model);
                SMonitor.Log($"Allowing season change");
                return true;
            }
            SMonitor.Log($"Preventing season change");
            Game1.dayOfMonth = 1;
            if(Game1.currentSeason.ToLower() == "spring")
                Game1.year--;
            model.month++;
            SHelper.Data.WriteSaveData(context.GetType().Namespace, model);
            return false;
        }

    }
}