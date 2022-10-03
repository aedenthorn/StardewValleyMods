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
        public static FieldInfo sourceRect = AccessTools.Field(typeof(Crop), "sourceRect");
        public static bool skipping = false;

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public static class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
            {
                if (!Config.ModEnabled || !Context.IsWorldReady)
                    return true;
                var mousePos = Game1.getMousePosition();
                var box = new Rectangle(Utility.Vector2ToPoint(Game1.GlobalToLocal(new Vector2(tileLocation.X, tileLocation.Y) * 64)), new Point(64, 64));
                if (!box.Contains(mousePos))
                    return true;
                var offset = mousePos - box.Location;
                if (offset.X < 32 && offset.Y < 32)
                    return true;
                int idx = 1;
                if (offset.Y > 32)
                {
                    if (offset.X > 32)
                    {
                        idx = 3;
                    }
                    else
                    {
                        idx = 2;
                    }
                }
                Location tile = tileLocation + new Location(tileOffset * idx, tileOffset * idx);
                return true;
            }
        }

        
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
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isTileOnMap), new Type[] { typeof(Vector2) })]
        public static class GameLocation_isTileOnMap_Patch
        {
            public static void Postfix(GameLocation __instance, Vector2 position, ref bool __result)
            {
                if (!Config.ModEnabled || __result || position.X < tileOffset)
                    return;
                __result = __instance.terrainFeatures.ContainsKey(position);
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
                        if (location.terrainFeatures.TryGetValue(tileLocation, out TerrainFeature f) && f is HoeDirt && (f as HoeDirt).state != 2)
                        {
                            (location.terrainFeatures[tileLocation] as HoeDirt).state.Value = 1;
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.makeHoeDirt))]
        public static class GameLocation_makeHoeDirt_Patch
        {
            public static void Prefix(GameLocation __instance, Vector2 tileLocation, ref bool __state)
            {
                if (!Config.ModEnabled)
                    return;
                __state = __instance.terrainFeatures.ContainsKey(tileLocation);
            }
            public static void Postfix(GameLocation __instance, Vector2 tileLocation, bool __state)
            {
                if (!Config.ModEnabled || !__instance.terrainFeatures.ContainsKey(tileLocation) || __state)
                    return;
                for(int i = 1; i < 4; i++)
                {
                    var tile = tileLocation + new Vector2(tileOffset * i, tileOffset * i);
                    if (!__instance.terrainFeatures.ContainsKey(tile))
                    {
                        __instance.terrainFeatures.Add(tile, new HoeDirt((Game1.IsRainingHere(__instance) && __instance.IsOutdoors && !__instance.Name.Equals("Desert")) ? 1 : 0, __instance));
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
                AccessTools.StaticFieldRefAccess<Crop, Vector2>("origin") = new Vector2(14, 34);
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

                    }
                }

                return codes.AsEnumerable();
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