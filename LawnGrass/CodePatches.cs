using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LawnGrass
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Grass), nameof(Grass.draw))]
        public static class Grass_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) // reduce height for fewer weeds
            {
                SMonitor.Log($"Transpiling Grass.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi && mi == AccessTools.PropertyGetter(typeof(Color), nameof(Color.White)))
                    {
                        SMonitor.Log("Intercepting source rect");
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetSourceRect))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                    else if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi2 && mi2 == AccessTools.Method(typeof(Game1), nameof(Game1.GlobalToLocal), new Type[] { typeof(xTile.Dimensions.Rectangle), typeof(Vector2) }))
                    {
                        SMonitor.Log("Intercepting position");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetPos))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.behaviors))]
        public static class FarmAnimal_behaviors_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) // for truffles
            {
                SMonitor.Log($"Transpiling FarmAnimal.behaviors");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>), "ContainsKey"))
                    {
                        SMonitor.Log("Intercepting check for terrain feature");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(CheckForGrass));
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Grass), new Type[] { typeof(int), typeof(int) })]
        [HarmonyPatch(MethodType.Constructor)]
        public static class Grass_Patch
        {
            public static void Prefix(int which, ref int numberOfWeeds) // if planting lawn, don't add weeds
            {
                if (Config.ModEnabled && which == 1 && SHelper.Input.IsDown(Config.ModKey) != Config.LawnByDefault)
                    numberOfWeeds = 0;
            }
            public static void Postfix(Grass __instance, int which) // set mod data
            {
                if (Config.ModEnabled && which == 1 && SHelper.Input.IsDown(Config.ModKey) != Config.LawnByDefault)
                {
                    __instance.modData[lawnKey] = "true";
                }
            }
        }
        [HarmonyPatch(typeof(Grass), nameof(Grass.performToolAction))] 
        public static class Grass_performToolAction_Patch
        {
            public static void Prefix(Grass __instance) // leads to createDestroySprites and TryDropItemsOnCut, so tell them last weed count
            {
                lastWeedCount.Value = __instance.numberOfWeeds.Value;
            }

            public static void Postfix(Grass __instance, Tool t, Vector2 tileLocation, ref bool __result) // set to 0, return starter
            {
                if (!Config.ModEnabled || __instance.grassType.Value != 1 || (!Config.ProtectNonLawn && !__instance.modData.ContainsKey(lawnKey)))
                    return;
                __instance.numberOfWeeds.Value = Math.Max(0, __instance.numberOfWeeds.Value);
                if (__result && t is not Pickaxe)
                {
                    __result = false;
                }
                else if(!__result && t is Pickaxe && Config.ReturnGrassStarter)
                {
                    if(IsLawn(__instance))
                        Game1.createItemDebris(ItemRegistry.Create("(O)297"), __instance.Tile * 64, 0, __instance.Location);
                    __result = true;
                }
            }
        }
        [HarmonyPatch(typeof(Grass), nameof(Grass.TryDropItemsOnCut))]
        public static class Grass_TryDropItemsOnCut_Patch
        {
            public static bool Prefix(Grass __instance)
            {
                if (!Config.ModEnabled || lastWeedCount.Value > 0) // don't drop items if it was already 0
                    return true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Grass), nameof(Grass.dayUpdate))]
        public static class Grass_dayUpdate_Patch
        {
            public static void Prefix(Grass __instance, ref int __state) // get weeds before update
            {
                if (!Config.ModEnabled || !IsLawn(__instance))
                    return;
                __state = __instance.numberOfWeeds.Value;
            }
            public static void Postfix(Grass __instance, int __state) // override growth for lawns
            {
                if (!Config.ModEnabled || !IsLawn(__instance))
                    return;
                if(__instance.numberOfWeeds.Value > __state)
                {
                    __instance.numberOfWeeds.Value = Math.Clamp(__state + (Game1.random.NextDouble() < Config.GrowChance ? Game1.random.Next(1, Math.Max(Config.MaxDailyGrowth, 1)) : 0), 0, 4);
                }
            }
        }
        [HarmonyPatch(typeof(Grass), nameof(Grass.reduceBy))] 
        public static class Grass_reduceBy_Patch
        {
            public static void Prefix(Grass __instance)
            {
                if (!Config.ModEnabled || !IsLawn(__instance)) // leads to createDestroySprites, so tell it last weed count
                    return;
                lastWeedCount.Value = __instance.numberOfWeeds.Value;
            }
            public static void Postfix(Grass __instance, ref bool __result) // prevent destroying grass
            {
                if (!Config.ModEnabled || !__result || !IsLawn(__instance))
                    return;
                __instance.numberOfWeeds.Value = Math.Max(__instance.numberOfWeeds.Value, 0);
                __result = false;
            }
        }
        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.eatGrass))]
        public static class FarmAnimal_eatGrass_Patch
        {
            public static bool Prefix(FarmAnimal __instance, GameLocation environment) // prevent eating if 0 weeds
            {
                if (!Config.ModEnabled)
                    return true;
                if(environment.terrainFeatures.TryGetValue(__instance.Tile, out var tf) && tf is Grass grass && grass.numberOfWeeds.Value <= 0)
                {
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Grass), "shake")]
        public static class Grass_shake_Patch  // reduce shake for fewer weeds
        {
            public static void Prefix(Grass __instance, ref float shake, ref float rate)
            {
                if (!Config.ModEnabled || __instance.numberOfWeeds.Value == 4)
                    return;
                shake *= (__instance.numberOfWeeds.Value / 4f);
            }
        }
        [HarmonyPatch(typeof(Grass), "createDestroySprites")]
        public static class Grass_createDestroySprites_Patch
        {
            public static bool Prefix() // follows from reduceBy and performToolAction which send last weed count - if 0, don't create sprites
            {
                if (!Config.ModEnabled || lastWeedCount.Value > 0)
                    return true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Grass), nameof(Grass.doCollisionAction))]
        public static class Grass_doCollisionAction_Patch
        {
            public static void Postfix(Grass __instance, Character who) // reduce speed buff
            {
                if (!Config.ModEnabled || __instance.numberOfWeeds.Value == 4 || who is not Farmer f)
                    return;
                f.temporarySpeedBuff *= Math.Max(0, __instance.numberOfWeeds.Value) / 4f;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.OnTerrainFeatureAdded))]
        public static class GameLocation_OnTerrainFeatureAdded_Patch
        {
            public static void Postfix(GameLocation __instance, TerrainFeature feature, Vector2 location) // trigger to create lawn, add mod data
            {
                if (!Config.ModEnabled || feature is not Grass grass || (!IsLawn(grass) && !Config.ProtectNonLawn))
                    return;
                OnAdded(grass, __instance, location);
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.OnTerrainFeatureRemoved))]
        public static class GameLocation_OnTerrainFeatureRemoved_Patch
        {
            public static void Postfix(GameLocation __instance, TerrainFeature feature) // trigger to change lawn for neighbors
            {
                if (!Config.ModEnabled || feature is not Grass grass || (!IsLawn(grass) && !Config.ProtectNonLawn))
                    return;
                OnRemoved(grass, __instance);
            }
        }
    }
}