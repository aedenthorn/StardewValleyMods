using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace BirthdayFriendship
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Billboard), new Type[] { typeof(bool) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class Billboard_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Billboard.cctr");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(NPC), nameof(NPC.isVillager)))
                    {
                        SMonitor.Log($"adding method to check for hearts to show birthdays");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(CheckBirthday));
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}