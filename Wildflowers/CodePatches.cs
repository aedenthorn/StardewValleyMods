using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace Wildflowers
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.DayUpdate))]
        public class GameLocation_DayUpdate_Patch
        {
            public static void Postfix(GameLocation __instance)
            {
                if (!Config.ModEnabled)
                    return;
                cropDict[__instance.Name] = new Dictionary<Vector2, Crop>();
                foreach (var key in __instance.terrainFeatures.Pairs.Where(p => p.Value is Grass).Select(p => p.Key))
                {
                    Crop crop;
                    if (!__instance.terrainFeatures[key].modData.TryGetValue(wildKey, out string data))
                    {
                        var chance = Game1.random.NextDouble();
                        
                        if (chance <= Config.wildflowerGrowChance)
                        {
                            var flowers = Game1.objectInformation.Where(p => p.Value.Contains("/Basic -80/") && !Config.DisallowNames.Contains(p.Value.Split('/')[0])).Select(p => p.Key).ToArray();
                            int idx = GetRandomFlowerSeed(flowers);
                            if (idx < 0)
                            {
                                SMonitor.Log($"no flowers for this season");
                                return;
                            }
                            crop = new Crop(idx, (int)key.X, (int)key.Y);
                            crop.growCompletely();
                            SMonitor.Log($"Added new wild flower {crop.indexOfHarvest} to {__instance.Name} at {key}");
                        }
                        else
                            continue;
                    }
                    else
                    {
                        var cropData = JsonConvert.DeserializeObject<CropData>(data);
                        crop = cropData.ToCrop();
                        if(IsCropDataInvalid(crop, cropData))
                        {
                            SMonitor.Log($"Invalid wild flower data {data}, removing");
                            cropDict[__instance.Name].Remove(key);
                            __instance.terrainFeatures[key].modData.Remove(wildKey);
                            continue;
                        }
                    }
                    crop.newDay(1, 0, (int)key.X, (int)key.Y, __instance);
                    cropDict[__instance.Name][key] = crop;
                }
            }
        }
        [HarmonyPatch(typeof(Grass), nameof(Grass.dayUpdate))]
        public class Grass_dayUpdate_Patch
        {
            public static void Postfix(Grass __instance, GameLocation environment, Vector2 tileLocation)
            {
                if (!Config.ModEnabled || Game1.dayOfMonth != 1 ||  !__instance.modData.TryGetValue(wildKey, out string data))
                    return;
                var c = JsonConvert.DeserializeObject<CropData>(data);
                if(c is not null && environment.IsOutdoors && !environment.SeedsIgnoreSeasonsHere() && !c.seasonsToGrowIn.Contains(environment.GetSeasonForLocation()))
                {
                    __instance.modData.Remove(wildKey);
                }
            }
        }
        [HarmonyPatch(typeof(Grass), nameof(Grass.draw))]
        public class Grass_draw_Patch
        {
            public static void Postfix(Grass __instance, SpriteBatch spriteBatch, Vector2 tileLocation)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(wildKey, out string data))
                    return;
                if (!cropDict.TryGetValue(__instance.currentLocation.Name, out Dictionary<Vector2, Crop> locDict) || !locDict.TryGetValue(tileLocation, out Crop crop))
                {
                    if (locDict is null)
                    {
                        cropDict[__instance.currentLocation.Name] = new Dictionary<Vector2, Crop>();
                    }
                    var cropData = JsonConvert.DeserializeObject<CropData>(data);
                    crop = cropData.ToCrop();
                    if (IsCropDataInvalid(crop, cropData))
                    {
                        SMonitor.Log($"Invalid wild flower data {data}, removing");
                        cropDict[__instance.currentLocation.Name].Remove(tileLocation);
                        __instance.modData.Remove(wildKey);
                        return;
                    }
                    cropDict[__instance.currentLocation.Name][tileLocation] = crop;
                }
                //locDict.Remove(tileLocation);
                //__instance.modData.Remove(wildKey);
                crop.draw(spriteBatch, tileLocation, Color.White, 0);
            }
        }
        [HarmonyPatch(typeof(Utility), nameof(Utility.findCloseFlower), new Type[] { typeof(GameLocation), typeof(Vector2), typeof(int), typeof(Func<Crop, bool>)})]
        public class Utility_findCloseFlower_Patch
        {
            public static void Postfix(GameLocation location, Vector2 startTileLocation, int range, Func<Crop, bool> additional_check, ref Crop __result)
            {
                if (!Config.ModEnabled || !Config.WildFlowersMakeFlowerHoney || !cropDict.TryGetValue(location.Name, out Dictionary<Vector2, Crop> locDict))
                    return;
                Vector2 tilePos = __result is null ? Vector2.Zero : AccessTools.FieldRefAccess<Crop, Vector2>(__result, "tilePosition");
                float closestDistance = Vector2.Distance(startTileLocation, tilePos);
                foreach (var v in locDict.Keys.ToArray())
                {
                    if (!location.terrainFeatures.TryGetValue(v, out var tf) || tf is not Grass || !tf.modData.ContainsKey(wildKey))
                    {
                        locDict.Remove(v);
                        continue;
                    }
                    if (Config.FixFlowerFind)
                    {
                        var distance = Vector2.Distance(startTileLocation, v);
                        if (distance <= range && distance < closestDistance)
                        {
                            closestDistance = distance;
                            __result = locDict[v];
                            AccessTools.FieldRefAccess<Crop, Vector2>(__result, "tilePosition") = v;
                        }
                    }
                    else
                    {
                        if (__result is null)
                        {
                            if (range < 0 || Math.Abs(v.X - startTileLocation.X) + Math.Abs(v.Y - startTileLocation.Y) <= range)
                            {
                                tilePos = v;
                                __result = locDict[v];
                                AccessTools.FieldRefAccess<Crop, Vector2>(__result, "tilePosition") = v;
                            }
                        }
                        else if (Vector2.Distance(startTileLocation, v) < Vector2.Distance(tilePos, startTileLocation))
                        {
                            __result = locDict[v];
                            tilePos = v;
                            AccessTools.FieldRefAccess<Crop, Vector2>(__result, "tilePosition") = v;
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Utility), nameof(Utility.canGrabSomethingFromHere))]
        public class Utility_canGrabSomethingFromHere_Patch
        {
            public static void Postfix(int x, int y, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || __result || Game1.currentLocation is null || !cropDict.TryGetValue(Game1.currentLocation.Name, out Dictionary<Vector2, Crop> locDict) || !locDict.TryGetValue(new Vector2(x / 64, y / 64), out Crop crop))
                    return;
                var t = new Vector2(x / 64, y / 64);
                if (!Game1.currentLocation.terrainFeatures.TryGetValue(t, out TerrainFeature f) || f is not Grass)
                {
                    SMonitor.Log($"Grass removed at {t}");
                    locDict.Remove(t);
                    return;
                }
                __result = crop != null && (!crop.fullyGrown.Value || crop.dayOfCurrentPhase.Value <= 0) && crop.currentPhase.Value >= crop.phaseDays.Count - 1 && !crop.dead.Value && (!crop.forageCrop.Value || crop.whichForageCrop.Value != 2);
                if (__result)
                {
                    Game1.mouseCursor = 6;
                    if (!Utility.withinRadiusOfPlayer(x, y, 1, who))
                    {
                        Game1.mouseCursorTransparency = 0.5f;
                        __result = false;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(TerrainFeature), nameof(TerrainFeature.performUseAction))]
        public class TerrainFeature_performUseAction_Patch
        {
            public static void Postfix(TerrainFeature __instance, Vector2 tileLocation, GameLocation location, ref bool __result)
            {
                if (!Config.ModEnabled || __result || __instance is not Grass || !__instance.modData.ContainsKey(wildKey) || location is null || !cropDict.TryGetValue(location.Name, out Dictionary<Vector2, Crop> locDict) || !locDict.TryGetValue(tileLocation, out Crop crop))
                    return;

                __result = crop.harvest((int)tileLocation.X, (int)tileLocation.Y, new HoeDirt(1, crop));
                if (__result)
                {
                    locDict.Remove(tileLocation);
                    __instance.modData.Remove(wildKey);
                    SMonitor.Log($"harvested wild flower in {location.Name} at {tileLocation}");
                }
            }
        }
        [HarmonyPatch(typeof(Grass), nameof(Grass.performToolAction))]
        public class Grass_performToolAction_Patch
        {
            public static void Postfix(Grass __instance, Vector2 tileLocation, GameLocation location, bool __result)
            {
                if (!Config.ModEnabled || !__result || !__instance.modData.ContainsKey(wildKey) || location is null || !cropDict.TryGetValue(location.Name, out Dictionary<Vector2, Crop> locDict) || !locDict.TryGetValue(tileLocation, out Crop crop))
                    return;
                SMonitor.Log($"Grass removed at {tileLocation}");
                if (Config.WeaponsHarvestFlowers)
                {
                    crop.whichForageCrop.Value = -424242;
                    crop.harvest((int)tileLocation.X, (int)tileLocation.Y, new HoeDirt(1, crop));
                    locDict.Remove(tileLocation);
                    __instance.modData.Remove(wildKey);
                    SMonitor.Log($"harvested wild flower in {location.Name} at {tileLocation}");
                }
            }
        }
        [HarmonyPatch(typeof(Crop), nameof(Crop.harvest))]
        public class Crop_harvest_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Crop.harvest");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 6 && codes[i].opcode == OpCodes.Ldc_I4_0 && codes[i + 5].opcode == OpCodes.Callvirt && codes[i + 5].operand is MethodInfo && (MethodInfo)codes[i + 5].operand == AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)))
                    {
                        SMonitor.Log($"adding method to switch exp type");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(SwitchExpType))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    }
                }

                return codes.AsEnumerable();
            }
        }

    }
}