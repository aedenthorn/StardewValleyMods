using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace MinuteDisplay
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.draw), new Type[] { typeof(SpriteBatch) })]
        public class DayTimeMoneyBox_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling DayTimeMoneyBox.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].operand is sbyte && (sbyte)codes[i].operand == 100 && codes[i + 1].opcode == OpCodes.Rem && codes[i + 2].opcode == OpCodes.Call && codes[i + 2].operand.Equals(AccessTools.Method(typeof(StringBuilderFormatEx), nameof(StringBuilderFormatEx.AppendEx), new Type[] { typeof(StringBuilder), typeof(int) })) && codes[i + 7].opcode == OpCodes.Ldfld && (FieldInfo)codes[i + 7].operand == AccessTools.Field(typeof(DayTimeMoneyBox), "_padZeros"))
                    {
                        SMonitor.Log("Overriding minute display");
                        codes[i + 9].opcode = OpCodes.Nop;
                        codes[i + 8].opcode = OpCodes.Nop;
                        codes[i + 7].opcode = OpCodes.Nop;
                        codes[i + 6].opcode = OpCodes.Nop;
                        codes[i + 5].opcode = OpCodes.Nop;
                        codes[i + 4].opcode = OpCodes.Nop;
                        codes[i + 2].opcode = OpCodes.Callvirt;
                        codes[i + 2].operand = AccessTools.Method(typeof(StringBuilder), nameof(StringBuilder.Append), new Type[] { typeof(string) });
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetActualMinutes))));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        private static string GetActualMinutes(int tens)
        {
            var ones = (int)Math.Floor(Game1.gameTimeInterval / 7000f * 10);
            return tens < 10 ? "0" + ones : "" + (tens + ones);
        }
        private static void DoNothing(StringBuilder s, StringBuilder i)
        {
        }
    }
}