using HarmonyLib;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SittingAction
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Game1), nameof(Game1.pressActionButton))]
        public static class Game1_pressActionButton_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled)
                    return;
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.pressUseToolButton))]
        public static class Game1_pressUseToolButton_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled)
                    return;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.CanEmote))]
        public static class Farmer_CanEmote_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled)
                    return;
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) // reduce height for fewer weeds
            {
                SMonitor.Log($"Transpiling Farmer.CanEmote");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(Farmer), nameof(Farmer.IsSitting)))
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SwitchBool))));
                        i++;
                    }
                    else if (codes[i].operand is MethodInfo mi2 && mi2 == AccessTools.Method(typeof(Farmer), nameof(Farmer.isRidingHorse)))
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SwitchBool))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public static class GameLocation_checkAction_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled)
                    return;
            }
            public static void Postfix(Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || __result || !who.IsSitting())
                    return;
                who.StopSitting(true);
                __result = true;
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) // reduce height for fewer weeds
            {
                SMonitor.Log($"Transpiling GameLocation.checkAction");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(Farmer), nameof(Farmer.IsSitting)))
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SwitchBool))));
                        i++;
                    }
                    else if (codes[i].operand is MethodInfo mi2 && mi2 == AccessTools.Method(typeof(Farmer), nameof(Farmer.isRidingHorse)))
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SwitchBool))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}