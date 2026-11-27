using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace SmallerCrops
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Object), nameof(Object.canBePlacedHere))]
        public static class Object_canBePlacedHere_Patch
        {
            public static void Postfix(Object __instance, GameLocation l, ref Vector2 tile, ref bool __result)
            {
                if (!Config.ModEnabled || __result || tile.X >= tileOffset || (__instance.Category != -74 && __instance.Category != -19) || l.getObjectAtTile((int)tile.X, (int)tile.Y) is IndoorPot)
                    return;
                int idx = GetMouseIndex((int)tile.X, (int)tile.Y);
                if (idx <= 0)
                    return;
                tile = new Vector2(tile.X + tileOffset * idx, tile.Y + tileOffset * idx);
                if(l.terrainFeatures.TryGetValue(tile, out TerrainFeature f) && f is HoeDirt && (f as HoeDirt).canPlantThisSeedHere(__instance.ParentSheetIndex, (int)tile.X, (int)tile.Y))
                {
                    __result = true;
                }
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.drawPlacementBounds))]
        public static class Object_drawPlacementBounds_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Object.drawPlacementBounds");
                bool found1 = false;
                bool found2 = false;
                bool found3 = false;
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!found1 && i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 4f && codes[i + 1].opcode == OpCodes.Ldc_I4_0)
                    {
                        SMonitor.Log("Replacing scale with method");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetPlacementScale));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        i++;
                        found1 = true;
                    }
                    else if (i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldsflda && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Game1), nameof(Game1.viewport)) && codes[i + 1].opcode == OpCodes.Call)
                    {
                        if(!found2 && (MethodInfo)codes[i + 1].operand == AccessTools.PropertyGetter(typeof(xTile.Dimensions.Rectangle), nameof(xTile.Dimensions.Rectangle.X)))
                        {
                            SMonitor.Log("Replacing x position with method");
                            codes[i].opcode = OpCodes.Ldarg_0;
                            codes[i].operand = null;
                            codes[i + 1].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetPlacementX));
                            i += 2;
                            found2 = true;
                        }
                        else if(!found3 && (MethodInfo)codes[i + 1].operand == AccessTools.PropertyGetter(typeof(xTile.Dimensions.Rectangle), nameof(xTile.Dimensions.Rectangle.Y)))
                        {
                            SMonitor.Log("Replacing y position with method");
                            codes[i].opcode = OpCodes.Ldarg_0;
                            codes[i].operand = null;
                            codes[i + 1].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetPlacementY));
                            i += 2;
                            found3 = true;
                        }
                    }
                    if (found1 && found2 && found3)
                        break;
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.ApplySprinkler))]
        public static class Object_ApplySprinkler_Patch
        {
            public static void Postfix(GameLocation location, Vector2 tile)
            {
                if (!Config.ModEnabled)
                    return;
                for(int i = 1; i < 4; i++)
                {
                    tile += new Vector2(tileOffset, tileOffset);
                    if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature f) && f is HoeDirt && (f as HoeDirt).state.Value != 2)
                    {
                        (location.terrainFeatures[tile] as HoeDirt).state.Value = 1;
                    }
                }
            }
        }

    }
}