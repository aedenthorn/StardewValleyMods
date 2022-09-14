using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SlimeHutchWaterSpots
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(SlimeHutch), nameof(SlimeHutch.UpdateWhenCurrentLocation))]
        public class SlimeHutch_UpdateWhenCurrentLocation_Patch
        {
            public static bool Prefix(SlimeHutch __instance, GameTime time)
            {
                var ptr = typeof(GameLocation).GetMethod("UpdateWhenCurrentLocation", BindingFlags.Public | BindingFlags.Instance).MethodHandle.GetFunctionPointer();
                var baseMethod = (Func<GameTime, GameLocation>)Activator.CreateInstance(typeof(Func<GameTime, GameLocation>), __instance, ptr);
                baseMethod(time);
                var size = __instance.Map.GetLayer("Buildings").LayerSize;
                for (int y = 0; y < size.Height; y++)
                {
                    for (int x = 0; x < size.Width; x++)
                    {
                        Point p = new Point(x, y);
                        int idx = y * size.Width + x;
                        int tile = __instance.getTileIndexAt(p, "Buildings");
                        if (tile == 2134 && __instance.waterSpots[idx])
                        {
                            __instance.setMapTileIndex(x, y, 2135, "Buildings", 0);
                        }
                        else if (tile == 2135 && !__instance.waterSpots[idx])
                        {
                            __instance.setMapTileIndex(x, y, 2134, "Buildings", 0);
                        }
                    }
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(SlimeHutch), nameof(SlimeHutch.TransferDataFromSavedLocation))]
        public class SlimeHutch_TransferDataFromSavedLocation_Patch
        {
            public static void Prefix(SlimeHutch __instance)
            {
                SMonitor.Log("building waterspots array");
                __instance.waterSpots.Clear();
                var length = __instance.Map.GetLayer("Buildings").LayerSize.Area;
                for (int i = 0; i < length; i++)
                    __instance.waterSpots.Add(false);
            }
        }

        [HarmonyPatch(typeof(SlimeHutch), nameof(SlimeHutch.performToolAction))]
        public class SlimeHutch_performToolAction_Patch
        {
            public static bool Prefix(SlimeHutch __instance, Tool t, int tileX, int tileY)
            {
                if (t is WateringCan)
                {
                    __instance.waterSpots[tileY * __instance.Map.GetLayer("Buildings").LayerWidth + tileX] = true;
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate))]
        public class SlimeHutch_DayUpdate_Patch
        {
            public static void Prefix(SlimeHutch __instance)
            {
                if (__instance.waterSpots.Count > 4)
                    return;
                SMonitor.Log("building waterspots array");
                __instance.waterSpots.Clear();
                var length = __instance.Map.GetLayer("Buildings").LayerSize.Area;
                for (int i = 0; i < length; i++)
                    __instance.waterSpots.Add(false);
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling SlimeHutch.DayUpdate");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldloc_S && codes[i+1].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+1].operand == AccessTools.Field(typeof(Vector2), nameof(Vector2.X)) && codes[i+2].opcode == OpCodes.Ldc_R4 && (float)codes[i+2].operand == 16)
                    {
                        SMonitor.Log("Replacing watering logic");
                        codes.RemoveRange(i + 1, 20);
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.WaterSpot))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        private static void WaterSpot(SlimeHutch hutch, Vector2 tile)
        {
            int idx = hutch.getTileIndexAt(Utility.Vector2ToPoint(tile), "Buildings");
            if (idx == 2135 || idx == 2134)
                hutch.waterSpots[hutch.Map.GetLayer("Buildings").LayerWidth * (int)tile.Y + (int)tile.X] = true;
        }
   }
}