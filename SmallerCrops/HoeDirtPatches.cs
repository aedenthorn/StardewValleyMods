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
        [HarmonyPatch(typeof(HoeDirt), "gatherNeighbors")]
        public static class HoeDirt_gatherNeighbors_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling HoeDirt.gatherNeighbors");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(HoeDirt), "_offsets"))
                    {
                        SMonitor.Log("Replacing neighbour offsets with method");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetNeigbourOffsets));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_2));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_1));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.canPlantThisSeedHere))]
        public static class HoeDirt_canPlantThisSeedHere_Patch
        {
            public static bool Prefix(HoeDirt __instance, int objectIndex, ref int tileX, ref int tileY, ref bool __result)
            {
                if (!Config.ModEnabled || skipping || tileX >= tileOffset || __instance.currentLocation.getObjectAtTile((int)tileX, (int)tileY) is IndoorPot)
                    return true;
                int idx = GetMouseIndex(tileX, tileY);
                if (idx <= 0)
                    return true;
                tileX += tileOffset * idx;
                tileY += tileOffset * idx;
                if (__instance.currentLocation is not null && __instance.currentLocation.terrainFeatures.TryGetValue(new Vector2(tileX, tileY), out TerrainFeature dirt) && dirt is HoeDirt)
                {
                    skipping = true;
                    __result = (dirt as HoeDirt).canPlantThisSeedHere(objectIndex, tileX, tileY);
                    skipping = false;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.plant))]
        public static class HoeDirt_plant_Patch
        {
            public static bool Prefix(HoeDirt __instance, int index, ref int tileX, ref int tileY, Farmer who, bool isFertilizer, GameLocation location)
            {
                if (!Config.ModEnabled || !Context.IsWorldReady || skipping || location.getObjectAtTile((int)tileX, (int)tileY) is IndoorPot)
                    return true;
                int idx = GetMouseIndex(tileX, tileY);
                if (idx <= 0)
                    return true;
                tileX += tileOffset * idx;
                tileY += tileOffset * idx;
                if(__instance.currentLocation is not null && __instance.currentLocation.terrainFeatures.TryGetValue(new Vector2(tileX, tileY), out TerrainFeature dirt) && dirt is HoeDirt)
                {
                    skipping = true;
                    SMonitor.Log($"Planting at {tileX},{tileY}");
                    (dirt as HoeDirt).plant(index, tileX, tileY, who, isFertilizer, location);
                    skipping = false;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.performToolAction))]
        public static class HoeDirt_performToolAction_Patch
        {
            public static void Postfix(Tool t, int damage, Vector2 tileLocation, GameLocation location)
            {
                if (!Config.ModEnabled)
                    return;
                if (t is WateringCan)
                {
                    for (int i = 1; i < 4; i++)
                    {
                        tileLocation += new Vector2(tileOffset, tileOffset);
                        if (location.terrainFeatures.TryGetValue(tileLocation, out TerrainFeature f) && f is HoeDirt && (f as HoeDirt).state.Value != 2)
                        {
                            (location.terrainFeatures[tileLocation] as HoeDirt).state.Value = 1;
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.DrawOptimized))]
        public static class HoeDirt_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling HoeDirt.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 4f && codes[i + 1].opcode == OpCodes.Ldc_I4_0)
                    {
                        SMonitor.Log("Replacing scale with method");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetScale));
                    }
                }

                return codes.AsEnumerable();
            }
            public static bool skip;
            public static void Postfix(HoeDirt __instance, SpriteBatch dirt_batch, SpriteBatch fert_batch, SpriteBatch crop_batch, Vector2 tileLocation)
            {
                if (!Config.ModEnabled || skip)
                    return;
                for (int i = 1; i < 4; i++)
                {
                    if(Game1.currentLocation.terrainFeatures.TryGetValue(tileLocation + new Vector2(tileOffset * i, tileOffset * i), out TerrainFeature f) && f is HoeDirt)
                    {
                        skip = true;
                        var offset = new Vector2((i % 2) * 0.5f, i > 1 ? 0.5f : 0);
                        f.draw(dirt_batch, tileLocation + offset);
                        skip = false;
                    }
                }
            }
        }

    }
}