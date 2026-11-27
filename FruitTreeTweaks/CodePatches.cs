
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using LogLevel = StardewModdingAPI.LogLevel;
using Object = StardewValley.Object;

namespace FruitTreeTweaks
{
    public partial class ModEntry
    {
        #region Constructors & Variables
        [HarmonyPatch(typeof(FruitTree), new Type[] { typeof(string), typeof(int) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class FruitTree__Patch1
        {
            public static void Postfix(FruitTree __instance)
            {
                if (!Config.EnableMod && __instance.daysUntilMature.Value != Config.DaysUntilMature)
                    return;
                __instance.daysUntilMature.Value = Math.Min(Config.DaysUntilMature, __instance.daysUntilMature.Value);
            }
        }
        [HarmonyPatch(typeof(FruitTree), new Type[] { typeof(string), typeof(int) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class FruitTree__Patch2
        {
            public static void Postfix(FruitTree __instance)
            {
                if (!Config.EnableMod && __instance.daysUntilMature.Value != Config.DaysUntilMature)
                    return;
                __instance.daysUntilMature.Value = Math.Min(Config.DaysUntilMature, __instance.daysUntilMature.Value);
            }
        }
        [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.dayUpdate))]
        public class FruitTree_dayUpdate_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FruitTree.dayUpdate", LogLevel.Debug);
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(FruitTree), nameof(FruitTree.daysUntilMature)) && codes[i + 3].opcode == OpCodes.Bgt_S)
                    {
                        SMonitor.Log("replacing daysUntilMature value with method", LogLevel.Debug);
                        codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.ChangeDaysToMatureCheck))));
                    }
                }

                return codes.AsEnumerable();
            }

            public static bool Prefix()
            {
                fruitToday = GetFruitPerDay();
                attempts = 0;
                return true;
            }
        }
        #endregion

        #region Method Patches
        [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.IsInSeasonHere))]
        public class FruitTree_IsInSeasonHere_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                __result = (Config.EnableMod && Config.FruitAllSeasons && (!Game1.IsWinter || Config.FruitInWinter));
                return !__result;
            }
        }
        [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.IsGrowthBlocked))]
        public class FruitTree_IsGrowthBlocked_Patch
        {
            public static bool Prefix(FruitTree __instance, Vector2 tileLocation, GameLocation environment, ref bool __result)
            {
                if (!Config.EnableMod)
                    return true;
                foreach (Vector2 v in Utility.getSurroundingTileLocationsArray(tileLocation))
                {
                    if (Config.CropsBlock && environment.terrainFeatures.TryGetValue(v, out TerrainFeature feature) && feature is HoeDirt && (feature as HoeDirt).crop != null)
                    {
                        __result = true;
                        return false;
                    }

                    if (Config.ObjectsBlock && environment.IsTileOccupiedBy(v, CollisionMask.All, CollisionMask.None))
                    {
                        Object o = environment.getObjectAtTile((int)v.X, (int)v.Y);
                        if (o == null || !Utility.IsNormalObjectAtParentSheetIndex(o, "590"))
                        {
                            __result = true;
                            return false;
                        }
                    }
                    if (Config.TreesBlock && environment.terrainFeatures.TryGetValue(v, out TerrainFeature feature2) && feature2 is Tree)
                    {
                        __result = true;
                        return false;
                    }
                }
                __result = false;
                return false;
            }
        }
        [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.draw), new Type[] { typeof(SpriteBatch) })]
        public class FruitTree_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                if (!Config.EnableMod) return instructions;
                SMonitor.Log($"Transpiling FruitTree.draw", LogLevel.Debug);
                var codes = new List<CodeInstruction>(instructions);
                bool found1 = false;
                int which = 0;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!found1 && i < codes.Count - 2 && codes[i].opcode == OpCodes.Ldc_I4_1 && codes[i + 1].opcode == OpCodes.Ldc_R4 && (float)codes[i + 1].operand == 1E-07f)
                    {
                        SMonitor.Log("shifting bottom of tree draw layer offset", LogLevel.Debug);
                        codes[i + 1].opcode = OpCodes.Ldarg_0;
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetTreeBottomOffset))));
                        found1 = true;
                    }
                    else if (i < codes.Count && i > 2 && codes[i - 2].opcode == OpCodes.Ldloc_S && codes[i + 1].opcode == OpCodes.Ldc_R4 && (float)codes[i + 1].operand == 0.0f && codes[i + 3].opcode == OpCodes.Ldc_R4 && (float)codes[i + 3].operand == 4.0f && codes[i + 4].opcode == OpCodes.Ldc_I4_0)
                    {
                        SMonitor.Log("modifying fruit color", LogLevel.Debug);
                        codes.RemoveAt(i);
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetFruitColor))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldloc_S, 8));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0, null));
                        which++;
                    }
                    else if (i < codes.Count && i > 8 && codes[i - 8].opcode == OpCodes.Ldloc_S && codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 4.0f && codes[i + 1].opcode == OpCodes.Ldc_I4_0 && codes[i + 2].opcode == OpCodes.Ldloca_S)
                    {
                        SMonitor.Log("modifying fruit scale", LogLevel.Debug);
                        codes.RemoveAt(i);
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetFruitScale))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldloc_S, 8));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0, null));
                        which++;
                    }
                    if (found1 && which >= 2) // the which check is a little redundant since the condition checks ldc.i4.0 which only applies to the first 2 draws anyway, but ill allow it
                        break;
                }

                return codes.AsEnumerable();
            }
            public static void Postfix(FruitTree __instance, SpriteBatch spriteBatch)
            {
                if (__instance is not FruitTree) return;
                if (!Config.EnableMod || __instance.fruit.Count <= 3 || __instance.growthStage.Value < 4)
                    return;
                if (!fruitData.TryGetValue(Game1.currentLocation, out var dict) || !dict.TryGetValue(__instance.Tile, out var data))
                    ReloadFruit(__instance.Location, __instance.Tile, __instance.fruit.Count);
                dict.TryGetValue(__instance.Tile, out data);
                for (int i = 3; i < __instance.fruit.Count; i++)
                {
                    Vector2 offset = GetFruitOffset(__instance, i);
                    Vector2 tileLocation = __instance.Tile;
                    Color color = data.colors[i];

                    Texture2D texture = GetTexture(__instance, out var sourceRect);
                    spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f - new Vector2(16, 80) * 4 + offset), sourceRect, color, 0f, Vector2.Zero, GetFruitScale(__instance, i), SpriteEffects.None, (float)__instance.getBoundingBox().Bottom / 10000f + 0.002f - tileLocation.X / 1000000f + i / 100000f);

                }
            }
        }
        [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.shake))]
        public class FruitTree_shake_Patch
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FruitTree.shake", LogLevel.Debug);
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 4 && codes[i].opcode == OpCodes.Ldloca_S && codes[i + 1].opcode == OpCodes.Ldc_R4 && codes[i + 2].opcode == OpCodes.Ldc_R4 && (float)codes[i + 1].operand == 0 && (float)codes[i + 2].operand == 0 && codes[i + 3].opcode == OpCodes.Call && (ConstructorInfo)codes[i + 3].operand == AccessTools.Constructor(typeof(Vector2), new Type[] { typeof(float), typeof(float) }) && codes[i + 4].opcode == OpCodes.Ldloc_S && codes[i + 4].operand == codes[1 + 4].operand)
                    {
                        SMonitor.Log("replacing default fruit offset with method", LogLevel.Debug);
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Stloc_S, 4));
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetFruitOffsetForShake))));
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Ldloc_S));
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 8;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.GetQuality))]
        public class FruitTree_GetQuality_Patch
        {
            public static bool Prefix(FruitTree __instance, ref int __result)
            {
                // __instance.stump.Value was used to remove quality from matured sapling, however I just found out this is not a bug but a new 1.6 feature.
                if (!Config.EnableMod) return true;

                int days = __instance.daysUntilMature.Value;
                if (__instance.struckByLightningCountdown.Value > 0 || days >= 0)
                {
                    __result = 0;
                    return false;
                }
                if (days > -Config.DaysUntilIridiumFruit) // 0 = base, 1 = silver, 2 = gold, 3 = shadow realm, 4 = iridium
                {
                    if (days > -Config.DaysUntilGoldFruit)
                    {
                        if (days > -Config.DaysUntilSilverFruit)
                        {
                            __result = 0;
                        }
                        else
                        {
                            __result = 1;
                        }
                    }
                    else
                    {
                        __result = 2;
                    }
                }
                else
                {
                    __result = 4;
                }

                return false;
            }
        }
        [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.TryAddFruit))]
        public class FruitTree_TryAddFruit_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                if (!Config.EnableMod) return instructions;

                SMonitor.Log("Transpiling FruitTree.TryAddFruit", LogLevel.Debug);

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldc_I4_3)
                    {
                        SMonitor.Log("replacing max fruit per tree with method", LogLevel.Debug);
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMaxFruit))));
                        codes.RemoveAt(i + 1);
                    }
                }

                return codes.AsEnumerable();
            }

            public static void Postfix(FruitTree __instance, ref bool __result) // 1 by default because base function already added one, or tried
            {
                attempts++;
                if (!Config.EnableMod || !__result || fruitToday == 1) return;

                if (!__instance.stump.Value && __instance.growthStage.Value >= 4 && __instance.IsInSeasonHere() && __instance.fruit.Count < GetMaxFruit() && attempts + 1 <= fruitToday)
                {
                    __instance.TryAddFruit();
                }
                return;
            }
        }
        #endregion

        #region Placement Logic
        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location, int x, int y, ref bool __result, Farmer who = null)
            {
                if (location is not Farm && !Config.PlantAnywhere) return true;

                if (!__instance.TypeDefinitionId.Equals("(O)")) return true;

                Object obj = __instance as Object ?? null;
                if (Config.EnableMod && obj.IsFruitTreeSapling())
                {
                    Vector2 placementTile = new Vector2(x / 64, y / 64);
                    string deniedMessage = string.Empty;
                    if (CanItemBePlacedHere(location, placementTile, out deniedMessage))
                    {
                        location.playSound("dirtyHit");
                        DelayedAction.playSoundAfterDelay("coin", 100);
                        FruitTree fruitTree = new FruitTree(obj.ItemId)
                        {
                            GreenHouseTileTree = (location.IsGreenhouse)
                        };
                        location.terrainFeatures.Remove(placementTile);
                        location.terrainFeatures.Add(placementTile, fruitTree);
                        __result = true;
                        return false;
                    }
                    else
                    {
                        SMonitor.Log($"{deniedMessage}", LogLevel.Warn);
                    }
                }
                SMonitor.LogOnce($"placementAction for {obj?.DisplayName} passed to vanilla method.", LogLevel.Debug);
                return true;
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.canBePlacedHere))]
        public class Object_canBePlacedHere_Patch
        {

            public static bool Prefix(GameLocation l, Vector2 tile, ref bool __result)
            {
                //CollisionMask mask = CollisionMask.All;
                Farmer who = Game1.player;
                Object tree = who?.ActiveObject ?? null;

                if (tree is null || !Config.EnableMod || (l is not Farm && !l.IsGreenhouse && !Config.PlantAnywhere)) return true;

                if (tree.IsFruitTreeSapling())
                {
                    if (!CanItemBePlacedHere(l, tile, out var deniedMessage))
                    {
                        SMonitor.LogOnce($"canBePlacedHere.CanItemBePlacedHere denied: {deniedMessage}", LogLevel.Debug);
                        return true;
                    }

                    __result = true;
                    return false;
                }
                SMonitor.LogOnce($"canBePlacedHere handling for {tree?.DisplayName} passed to original method.", LogLevel.Debug);
                return true;
            }

            public static bool Prefix(GameLocation l, Vector2 tile, bool showError, ref bool __result)
            {
                //CollisionMask mask = CollisionMask.All;
                Farmer who = Game1.player;
                Object tree = who?.ActiveObject ?? null;

                if (tree is null || !Config.EnableMod || (l is not Farm && !l.IsGreenhouse && !Config.PlantAnywhere)) return true;

                if (tree.IsFruitTreeSapling())
                {
                    if (!CanItemBePlacedHere(l, tile, out var deniedMessage))
                    {
                        SMonitor.LogOnce($"canBePlacedHere.CanItemBePlacedHere denied: {deniedMessage}", LogLevel.Debug);
                        return true;
                    }

                    __result = true;
                    return false;
                }
                SMonitor.LogOnce($"canBePlacedHere handling for {tree?.DisplayName} passed to original method.", LogLevel.Debug);
                return true;
            }

            public static bool Prefix(GameLocation l, Vector2 tile, CollisionMask collisionMask, ref bool __result)
            {
                Farmer who = Game1.player;
                Object tree = who?.ActiveObject ?? null;

                if (tree is null || !Config.EnableMod || (l is not Farm && !l.IsGreenhouse && !Config.PlantAnywhere)) return true;

                if (tree.IsFruitTreeSapling())
                {
                    if (!CanItemBePlacedHere(l, tile, out var deniedMessage))
                    {
                        SMonitor.LogOnce($"canBePlacedHere.CanItemBePlacedHere denied: {deniedMessage}", LogLevel.Debug);
                        return true;
                    }

                    __result = true;
                    return false;
                }
                SMonitor.LogOnce($"canBePlacedHere handling for {tree?.DisplayName} passed to original method.", LogLevel.Debug);
                return true;
            }

            public static bool Prefix(GameLocation l, Vector2 tile, CollisionMask collisionMask, bool showError, ref bool __result)
            {
                Farmer who = Game1.player;
                Object tree = who?.ActiveObject ?? null;

                if (tree is null || !Config.EnableMod || (l is not Farm && !l.IsGreenhouse && !Config.PlantAnywhere)) return true;

                if (tree.IsFruitTreeSapling())
                {
                    if (!CanItemBePlacedHere(l, tile, out var deniedMessage))
                    {
                        SMonitor.LogOnce($"canBePlacedHere.CanItemBePlacedHere denied: {deniedMessage}", LogLevel.Debug);
                        return true;
                    }

                    __result = true;
                    return false;
                }
                SMonitor.LogOnce($"canBePlacedHere handling for {tree?.DisplayName} passed to original method.", LogLevel.Debug);
                return true;
            }
        }
        #endregion
    }
}