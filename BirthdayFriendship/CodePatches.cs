using HarmonyLib;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace BirthdayFriendship
{
    public partial class ModEntry
    {
        public class Billboard_GetBirthdays_Patch
        {
            public static void Postfix(Dictionary<int, List<NPC>> __result)
            {
                __result.Values.ToList().ForEach(npcList => npcList.RemoveAll(npc => !CheckBirthday(npc)));
            }
        }
        public class NPC_Birthday_Season_Patch
        {
            public static bool Prefix(NPC __instance, ref string __result)
            {
                if(Config.ModCheck && !CheckBirthday(__instance))
                {
                    __result = "foobar";
                    return false;
                }
                return true;
            }
        }
        public class ProfileMenu_draw_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                context.Monitor.Log("Transpiling NPC draw");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi && mi.Name == nameof(Utility.getSeasonNumber))
                    {
                        context.Monitor.Log("Adding method to nullify birthday season");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetSeasonNumber))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}