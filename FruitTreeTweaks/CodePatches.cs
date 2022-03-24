
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
using Object = StardewValley.Object;

namespace FruitTreeTweaks
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(FruitTree), new Type[] { typeof(int), typeof(int) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class FruitTree__Patch1
        {
            public static void Postfix(FruitTree __instance)
            {
                if (!Config.EnableMod)
                    return;
                __instance.daysUntilMature.Value = Config.DaysUntilMature;
                SMonitor.Log($"New fruit tree: set days until mature to {Config.DaysUntilMature}");
            }
        }
        [HarmonyPatch(typeof(FruitTree), new Type[] { typeof(int) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class FruitTree__Patch2
        {
            public static void Postfix(FruitTree __instance)
            {
                if (!Config.EnableMod)
                    return;
                __instance.daysUntilMature.Value = Config.DaysUntilMature;
                SMonitor.Log($"New fruit tree: set days until mature to {Config.DaysUntilMature}");
            }
        }

        [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.IsInSeasonHere))]
        public class FruitTree_IsInSeasonHere_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                if (!Config.EnableMod || !Config.FruitAllSeasons)
                    return true;
                __result = !Game1.IsWinter;
                return false;
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

                    if (Config.ObjectsBlock && environment.isTileOccupied(v, "", true))
                    {
                        Object o = environment.getObjectAtTile((int)v.X, (int)v.Y);
                        if (o == null || !Utility.IsNormalObjectAtParentSheetIndex(o, 590))
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.draw), new Type[] { typeof(SpriteBatch), typeof(Vector2) })]
        public class FruitTree_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FruitTree.draw");
                var codes = new List<CodeInstruction>(instructions);
                bool found1 = false;
                int which = 0;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!found1 && i < codes.Count - 2 && codes[i].opcode == OpCodes.Ldc_I4_1 && codes[i + 1].opcode == OpCodes.Ldc_R4 && (float)codes[i + 1].operand == 1E-07f)
                    {
                        SMonitor.Log("shifting bottom of tree draw layer offset");
                        codes[i + 1].opcode = OpCodes.Ldarg_0;
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetTreeBottomOffset))));
                        found1 = true;
                    }
                    if (i > 0 && i < codes.Count - 15 && codes[i].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Game1), nameof(Game1.objectSpriteSheet)) && codes[i + 15].opcode == OpCodes.Call && (MethodInfo)codes[i + 15].operand == AccessTools.PropertyGetter(typeof(Color), nameof(Color.White)))
                    {
                        SMonitor.Log("modifying fruit color");
                        codes[i + 15].opcode = OpCodes.Call;
                        codes[i + 15].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetFruitColor));
                        codes.Insert(i + 15, new CodeInstruction(OpCodes.Ldc_I4, which++));
                        codes.Insert(i + 15, new CodeInstruction(OpCodes.Ldarg_0));
                    }
                    if (found1 && which >= 2)
                        break;
                }

                return codes.AsEnumerable();
            }
            public static void Postfix(FruitTree __instance, SpriteBatch spriteBatch, Vector2 tileLocation)
            {
                if (!Config.EnableMod || __instance.fruitsOnTree.Value <= 3 || __instance.growthStage.Value < 4)
                    return;
                for (int i = 3; i < __instance.fruitsOnTree.Value; i++)
                {
                    Vector2 offset = GetFruitOffset(__instance, i);
                    Color color = fruitColors[Game1.currentLocation][tileLocation][i];

                    spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, tileLocation * 64 - new Vector2(16, 80) * 4 + offset), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (__instance.struckByLightningCountdown.Value > 0) ? 382 : __instance.indexOfFruit.Value, 16, 16)), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)__instance.getBoundingBox(tileLocation).Bottom / 10000f + 0.002f - tileLocation.X / 1000000f + i / 100000f);
                }
            }
        }

        [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.shake))]
        public class FruitTree_shake_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FruitTree.shake");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 4 && codes[i].opcode == OpCodes.Ldloca_S && codes[i + 1].opcode == OpCodes.Ldc_R4 && codes[i + 2].opcode == OpCodes.Ldc_R4 && (float)codes[i + 1].operand == 0 && (float)codes[i + 2].operand == 0 && codes[i + 3].opcode == OpCodes.Call && (ConstructorInfo)codes[i + 3].operand == AccessTools.Constructor(typeof(Vector2), new Type[] { typeof(float), typeof(float) }) && codes[i + 4].opcode == OpCodes.Ldloc_3)
                    {
                        SMonitor.Log("replacing default fruit offset with method");
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Stloc_S, 4));
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetFruitOffsetForShake))));
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Ldloc_3));
                        codes.Insert(i + 4, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public class Object_placementAction_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FruitTree.shake");
                var codes = new List<CodeInstruction>(instructions);
                bool found1 = false;
                bool found2 = false;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!found1 && i > 0 && i < codes.Count - 6 && codes[i + 1].opcode == OpCodes.Ldstr && (string)codes[i + 1].operand == "Strings\\StringsFromCSFiles:Object.cs.13060")
                    {
                        SMonitor.Log("adding extra check for tree blocking placement");
                        codes.Insert(i, codes[i - 1]);
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.TreesBlock))));
                        found1 = true;
                    }
                    if (!found2 && i > 0 && i < codes.Count - 6 && codes[i + 1].opcode == OpCodes.Ldstr && (string)codes[i + 1].operand == "Strings\\StringsFromCSFiles:Object.cs.13060_Fruit")
                    {
                        SMonitor.Log("adding extra check for fruit tree blocking placement");
                        codes.Insert(i, codes[i - 1]);
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.FruitTreesBlock))));
                        found2 = true;
                    }
                    if (found1 && found2)
                        break;
                }

                return codes.AsEnumerable();
            }
        }


        [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.dayUpdate))]
        public class FruitTree_dayUpdate_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FruitTree.dayUpdate");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 6 && codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(FruitTree), nameof(FruitTree.fruitsOnTree)) && codes[i + 1].opcode == OpCodes.Ldc_I4_3 && codes[i + 2].opcode == OpCodes.Ldarg_0 && codes[i + 5].opcode == OpCodes.Ldc_I4_1 && codes[i + 6].opcode == OpCodes.Add)
                    {
                        SMonitor.Log("replacing max fruits and fruit per day with methods");
                        codes[i + 1].opcode = OpCodes.Call;
                        codes[i + 1].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMaxFruit));
                        codes[i + 5].opcode = OpCodes.Call;
                        codes[i + 5].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetFruitPerDay));
                    }
                    if (i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(FruitTree), nameof(FruitTree.daysUntilMature)) && codes[i + 3].opcode == OpCodes.Bgt_S)
                    {
                        SMonitor.Log("replacing daysUntilMature value with method");
                        codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.ChangeDaysToMatureCheck))));
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}