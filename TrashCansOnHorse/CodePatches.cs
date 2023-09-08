using HarmonyLib;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;

namespace TrashCansOnHorse
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Town), nameof(Town.checkAction))]
        public class Town_checkAction_Patch
        {
            public static void Prefix(Location tileLocation, Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return;
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Town.checkAction");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 2 && codes[i].opcode == OpCodes.Ldarg_3 && codes[i+1].opcode == OpCodes.Callvirt && codes[i+1].operand is MethodInfo && (MethodInfo)codes[i+1].operand == AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.mount)))
                    {
                        SMonitor.Log("adding method to ignore mount");
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(Farmer_mount_Override))));
                    }
                }

                return codes.AsEnumerable();
            }
        }

        private static bool Farmer_mount_Override(Horse horse)
        {
            if (!Config.ModEnabled)
                return horse != null;
            return false;
        }
    }
}