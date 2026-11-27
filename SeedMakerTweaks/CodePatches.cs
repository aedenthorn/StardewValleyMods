using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace SeedMakerTweaks
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        public static IEnumerable<CodeInstruction> Object_performObjectDropInAction_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Object_performObjectDropInAction");

            var codes = new List<CodeInstruction>(instructions);
            bool startLooking = false;
            bool gotSeedAmount = false;
            bool gotAncientSeedChance = false;
            bool gotMixedSeedChance = false;
            bool gotMixedSeedAmount = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (startLooking)
                {
                    if (codes[i].opcode == OpCodes.Ret && codes[i - 1].opcode == OpCodes.Ldc_I4_1)
                        break;
                    if (gotSeedAmount)
                    {
                        if (gotAncientSeedChance)
                        {
                            if (gotMixedSeedChance)
                            {
                                if (gotMixedSeedAmount)
                                {
                                    break;
                                }
                                else if (codes[i].opcode == OpCodes.Ldc_I4_5 && codes[i - 1].opcode == OpCodes.Ldc_I4_1)
                                {
                                    gotMixedSeedAmount = true;
                                    SMonitor.Log($"got mixed seed amount!");
                                    codes[i - 1] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMinMixedSeeds)));
                                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMaxMixedSeeds)));
                                }
                            }
                            else if (codes[i].opcode == OpCodes.Ldc_R8 && (double)codes[i].operand == 0.02)
                            {
                                gotMixedSeedChance = true;
                                SMonitor.Log($"got mixed seed chance!");
                                codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMixedSeedChance)));
                            }

                        }
                        else if (codes[i].opcode == OpCodes.Ldc_R8 && (double)codes[i].operand == 0.005)
                        {
                            gotAncientSeedChance = true;
                            SMonitor.Log($"got ancient seed chance!");
                            codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetAncientSeedChance)));
                        }

                    }
                    else if (codes[i].opcode == OpCodes.Ldc_I4_4 && codes[i - 1].opcode == OpCodes.Ldc_I4_1)
                    {
                        gotSeedAmount = true;
                        SMonitor.Log($"got seed amount!");
                        codes[i - 1] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMinSeeds)));
                        codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMaxSeeds)));
                    }
                }
                else if (codes[i].opcode == OpCodes.Ldstr && (codes[i].operand as string) == "Seed Maker")
                {
                    SMonitor.Log($"got Seed Maker string!");
                    startLooking = true;
                }
            }

            return codes.AsEnumerable();
        }
        public static IEnumerable<CodeInstruction> SeedMakerMachine_SetInput_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"patching SeedMakerMachine_SetInput");

            var codes = new List<CodeInstruction>(instructions);
            bool gotSeedAmount = false;
            bool gotAncientSeedChance = false;
            bool gotMixedSeedChance = false;
            bool gotMixedSeedAmount = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ret && codes[i - 1].opcode == OpCodes.Ldc_I4_1)
                    break;
                if (gotSeedAmount)
                {
                    if (gotAncientSeedChance)
                    {
                        if (gotMixedSeedChance)
                        {
                            if (gotMixedSeedAmount)
                            {
                                break;
                            }
                            else if (codes[i].opcode == OpCodes.Ldc_I4_5 && codes[i - 1].opcode == OpCodes.Ldc_I4_1)
                            {
                                gotMixedSeedAmount = true;
                                SMonitor.Log($"got mixed seed amount!");
                                codes[i - 1] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMinMixedSeeds)));
                                codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMaxMixedSeeds)));
                            }
                        }
                        else if (codes[i].opcode == OpCodes.Ldc_R8 && (double)codes[i].operand == 0.02)
                        {
                            gotMixedSeedChance = true;
                            SMonitor.Log($"got mixed seed chance!");
                            codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMixedSeedChance)));
                        }

                    }
                    else if (codes[i].opcode == OpCodes.Ldc_R8 && (double)codes[i].operand == 0.005)
                    {
                        gotAncientSeedChance = true;
                        SMonitor.Log($"got ancient seed chance!");
                        codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetAncientSeedChance)));
                    }

                }
                else if (codes[i].opcode == OpCodes.Ldc_I4_4 && codes[i - 1].opcode == OpCodes.Ldc_I4_1)
                {
                    gotSeedAmount = true;
                    SMonitor.Log($"got seed amount!");
                    codes[i - 1] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMinSeeds)));
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMaxSeeds)));
                }
            }
            return codes.AsEnumerable();
        }
    }
}