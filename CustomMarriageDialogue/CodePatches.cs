using HarmonyLib;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CustomMarriageDialogue
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(NPC), nameof(NPC.setRandomAfternoonMarriageDialogue))]
        public class NPC_setRandomAfternoonMarriageDialogue_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                PMonitor.Log($"Transpiling NPC.setRandomAfternoonMarriageDialogue");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(int), nameof(int.ToString), new Type[] { }))
                    {
                        PMonitor.Log($"Inserting method to get random dialogue");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetRandomDialogueKey))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    }
                }

                return codes.AsEnumerable();
            }
        }
        
        [HarmonyPatch(typeof(NPC), nameof(NPC.marriageDuties))]
        public class NPC_marriageDuties_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                PMonitor.Log($"Transpiling NPC.marriageDuties");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(int), nameof(int.ToString), new Type[] { }))
                    {
                        PMonitor.Log($"Inserting method to get random dialogue");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetRandomDialogueKey))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    }
                }

                return codes.AsEnumerable();
            }
        }

        public static string GetRandomDialogueKey(string oldString, NPC npc)
        {
            if (!Config.ModEnabled)
                return oldString;
            var parts = oldString.Split('_');
            if (!int.TryParse(parts[parts.Length - 1], out int oldRandom))
                return oldString;
            parts[parts.Length - 1] = "";
            string stem = string.Join("_", parts);
            IEnumerable<string> dict1 = Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\MarriageDialogue").Keys.ToList().Where(s => s.StartsWith(stem));
            IEnumerable<string> dict2 = null;
            try
            {
                dict2 = Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\MarriageDialogue" + npc.Name)?.Keys.ToList().Where(s => s.StartsWith(stem));
            }
            catch { }

            int total = Math.Max(dict1.Count(), (dict2 is null ? 0 : dict2.Count()));
            int newRandom = Game1.random.Next(total);
            string newString = stem + newRandom;
            PMonitor.Log($"Got {total} marriage strings starting with {stem}, chose {newRandom}");
            if (!dict1.Contains(newString) && (dict2 is null || !dict2.Contains(newString)))
                return oldString;
            return newString;
        }
    }
}