using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace BuildingShift
{
    public partial class ModEntry
    {
        
        [HarmonyPatch(typeof(Building), nameof(Building.isActionableTile))]
        public class Building_isActionableTile_Patch
        {
            public static void Prefix(Building __instance, ref int xTile, ref int yTile)
            {
                if (!Config.EnableMod || !TryGetShift(__instance, out var amount))
                {
                    return;
                }
                xTile = (Game1.viewport.X + Game1.getOldMouseX() - (int)amount.X * 4) / 64;
                yTile = (Game1.viewport.Y + Game1.getOldMouseY() - (int)amount.Y * 4) / 64;
            }
        }
        
        [HarmonyPatch(typeof(Building), nameof(Building.GetBoundingBox))]
        public class Building_GetBoundingBox_Patch
        {
            public static void Postfix(Building __instance, ref Rectangle __result)
            {
                if (!Config.EnableMod || !TryGetShift(__instance, out var amount))
                {
                    return;
                }
                __result.Offset((int)amount.X * 4, (int)amount.Y * 4);


            }
        }

        public static IEnumerable<CodeInstruction> Draw_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Game1), nameof(Game1.GlobalToLocal), new System.Type[] { typeof(xTile.Dimensions.Rectangle), typeof(Vector2) }))
                {
                    SMonitor.Log("Adding method to check for building shift");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.ShiftVector))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    i += 2;
                }
                else if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Game1), nameof(Game1.GlobalToLocal), new System.Type[] { typeof(Vector2) }))
                {
                    SMonitor.Log("Adding method to check for building shift");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.ShiftVector))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    i += 2;
                }
            }

            return codes.AsEnumerable();
        }

    }
}