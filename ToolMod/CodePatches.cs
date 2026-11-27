using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using Object = StardewValley.Object;
using Microsoft.Xna.Framework.Graphics;

namespace ToolMod
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Tool), "tilesAffected")]
        public class Tool_tilesAffected_Patch
        {
            public static void Postfix(Tool __instance, Vector2 tileLocation, int power, Farmer who, ref List<Vector2> __result)
            {
                if (!Config.EnableMod || ((__instance is not WateringCan || !Config.WateringCanPowerLevelRange.TryGetValue(power, out Point size)) && (__instance is not Hoe || !Config.HoePowerLevelRange.TryGetValue(power, out size))))
                    return;
                __result = GetTilesAffected(tileLocation, size, who.FacingDirection);
            }
        }
        public static IEnumerable<CodeInstruction> Game1_UpdateControlInput_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Game1_UpdateControlInput");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo &&  (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Tool), nameof(Tool.upgradeLevel)) && codes[i + 3].opcode == OpCodes.Add)
                {
                    SMonitor.Log("Replacing max power with method");
                    codes.Insert(i + 4, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetToolMaxPower))));
                    i += 4;
                }
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(Tree), nameof(Tree.performToolAction))]
        public class Tree_performToolAction_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Tree.performToolAction");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 5 && codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Tree), nameof(Tree.health)) && codes[i + 4].opcode == OpCodes.Sub)
                    {
                        SMonitor.Log("Replacing damage with method");
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetToolDamage))));
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Ldarg_1));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        
        [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.performToolAction))]
        public class FruitTree_performToolAction_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FruitTree.performToolAction");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 5 && codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(FruitTree), nameof(FruitTree.health)) && codes[i + 4].opcode == OpCodes.Sub)
                    {
                        SMonitor.Log("Replacing damage with method");
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetToolDamage))));
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Ldarg_1));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        
        [HarmonyPatch(typeof(ResourceClump), nameof(ResourceClump.performToolAction))]
        public class ResourceClump_performToolAction_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling ResourceClump.performToolAction");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 5 && codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(ResourceClump), nameof(ResourceClump.health)) && codes[i + 4].opcode == OpCodes.Sub)
                    {
                        SMonitor.Log("Replacing damage with method");
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetToolDamage))));
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Ldarg_1));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Pickaxe), nameof(Pickaxe.DoFunction))]
        public class Pickaxe_DoFunction_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Pickaxe.DoFunction");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 5 && codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Object), nameof(Object.minutesUntilReady)) && codes[i + 4].opcode == OpCodes.Sub)
                    {
                        SMonitor.Log("Replacing damage with method");
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetToolDamage))));
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

    }
}
