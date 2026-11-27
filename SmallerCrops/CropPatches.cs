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
        [HarmonyPatch(typeof(Crop), nameof(Crop.newDay))]
        public static class Crop_newDay_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Crop.newDay");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == AccessTools.Method("StardewValley.OneTimeRandom:GetDouble"))
                    {
                        SMonitor.Log("Preventing vanilla giant crop");
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetGiantCropDouble));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
            public static void Postfix(Crop __instance, int state, int fertilizer, int xTile, int yTile, GameLocation environment)
            {
                if (!Config.ModEnabled || __instance.dead.Value)
                    return;
                __instance.growCompletely();
                if (state == 1 || __instance.indexOfHarvest.Value == 771)
                {
                    if (environment is Farm && __instance.currentPhase.Value == __instance.phaseDays.Count - 1 && (__instance.indexOfHarvest.Value == 276 || __instance.indexOfHarvest.Value == 190 || __instance.indexOfHarvest.Value == 254) && (double)AccessTools.Method("StardewValley.OneTimeRandom:GetDouble").Invoke(null, new object[] { Game1.uniqueIDForThisGame, (ulong)Game1.stats.DaysPlayed, (ulong)((long)xTile), (ulong)((long)yTile) }) < Config.GiantCropChance)
                    {
                        Vector2[] tiles = new Vector2[]
                        {
                            new Vector2(xTile, yTile),
                            new Vector2(xTile + tileOffset, yTile + tileOffset),
                            new Vector2(xTile + tileOffset * 2, yTile + tileOffset * 2),
                            new Vector2(xTile + tileOffset * 3, yTile + tileOffset * 3)
                        };
                        for (int i = 0; i < tiles.Length; i++)
                        {
                            if (!environment.terrainFeatures.TryGetValue(tiles[i], out TerrainFeature f) || f is not HoeDirt || (f as HoeDirt).crop == null || (f as HoeDirt).crop.indexOfHarvest.Value != __instance.indexOfHarvest.Value)
                            {
                                return;
                            }
                        }
                        for (int i = 0; i < tiles.Length; i++)
                        {
                            (environment.terrainFeatures[tiles[i]] as HoeDirt).crop = null;
                        }
                        (environment as Farm).resourceClumps.Add(new GiantCrop(__instance.indexOfHarvest.Value, new Vector2(xTile, yTile)));
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Crop), nameof(Crop.draw))]
        public static class Crop_draw_Patch
        {
            public static void Prefix(Vector2 tileLocation, ref Vector2 ___drawPosition, ref Vector2 __state)
            {
                if (!Config.ModEnabled)
                    return;
                AccessTools.StaticFieldRefAccess<Crop, Vector2>("origin") = new Vector2(15, 32);
                if (___drawPosition.X / 64 >= tileOffset)
                {
                    int idx = (int)___drawPosition.X / 64 / tileOffset;
                    var newPos = new Vector2(___drawPosition.X % tileOffset + (idx != 2 ? 32 : 0), ___drawPosition.Y % tileOffset + (idx != 1 ? 32 : 0));
                    __state = ___drawPosition;
                    ___drawPosition = newPos;
                }
            }
            public static void Postfix(Vector2 tileLocation, ref Vector2 ___drawPosition, Vector2 __state)
            {
                if (!Config.ModEnabled)
                    return;
                if (__state.X >= tileOffset)
                    ___drawPosition = __state;
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Crop.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 4f && codes[i + 1].opcode == OpCodes.Ldloc_2)
                    {
                        SMonitor.Log("Replacing scale with method");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetCropScale));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(GiantCrop), nameof(GiantCrop.draw))]
        public static class GiantCrop_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling GiantCrop.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 4f && codes[i + 1].opcode == OpCodes.Ldc_I4_0)
                    {
                        SMonitor.Log("Replacing scale with method");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetGiantCropScale));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}