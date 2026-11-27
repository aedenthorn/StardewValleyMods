using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace PrismaticSlimeJellyCandy
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(MineShaft), "populateLevel")]
        public static class MineShaft_populateLevel_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling MineShaft_populateLevel");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].operand is string str && str == "Wizard2" && codes[i + 1].operand is MethodInfo mi && mi == AccessTools.Method(typeof(FarmerTeam), nameof(FarmerTeam.SpecialOrderActive)))
                    {
                        SMonitor.Log($"adding override for special order active and prismatic chance");
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(OverrideSpecialOrderActive))));
                        codes.Insert(i - 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(MultiplyPrismaticChance))));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }

        }
    }
}