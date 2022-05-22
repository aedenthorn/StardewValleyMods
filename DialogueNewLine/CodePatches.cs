using HarmonyLib;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DialogueNewLine
{
    public partial class ModEntry
    {
        private static char specialCharacter = '¶';
        private static bool newLine;

        [HarmonyPatch(typeof(SpriteText), nameof(SpriteText.drawString))]
        public class SpriteText_drawString_Patch
        {
            public static void Prefix()
            {
                newLine = false;
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling SpriteText.drawString");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 1 && codes[i].opcode == OpCodes.Call && typeof(MethodInfo).IsAssignableFrom(codes[i].operand.GetType()) && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(SpriteText), nameof(SpriteText.positionOfNextSpace)))
                    {
                        SMonitor.Log("Adding check for special newline character");
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.positionOfNextSpace));
                    }
                }

                return codes.AsEnumerable();
            }
        }

        private static int positionOfNextSpace(string s, int i, int x, int space)
        {
            if (!Config.EnableMod)
                return SpriteText.positionOfNextSpace(s, i, x, space);
            if (newLine)
            {
                newLine = false;
                return int.MaxValue;
            }
            if(s[i] == specialCharacter)
                newLine = true;
            return SpriteText.positionOfNextSpace(s, i, x, space);
        }
    }
}