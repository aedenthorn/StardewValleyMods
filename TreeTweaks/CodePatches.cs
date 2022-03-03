using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LogSpamFilter
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {

        public static IEnumerable<CodeInstruction> Tree_draw_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Tree_draw");

            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>();
            int count = 0;
            for (int i = 0; i < codes.Count; i++)
            {
                if (i > 0 && codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.Draw), new System.Type[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) }))
                {
                    SMonitor.Log($"Switching draw {count}");
                    var ci = new CodeInstruction(OpCodes.Ldc_I4, count++);
                    ci.MoveLabelsFrom(codes[i]);
                    newCodes.Add(ci);
                    newCodes.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newCodes.Add(new CodeInstruction(OpCodes.Ldarg_2));
                    newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawTree))));
                }
                else
                    newCodes.Add(codes[i]);
            }

            return newCodes.AsEnumerable();
        }
        public static void Tree_getBoundingBox_Postfix(Tree __instance, ref Rectangle __result)
        {
            if (!Config.EnableMod || __instance.growthStage.Value <= 5)
                return;
            float increase = (__instance.growthStage.Value - 5) * Config.SizeIncreasePerDay / 100f;
            __result = new Rectangle((int)(__result.X - (__result.Width * increase)), (int)(__result.Y - (__result.Height * increase)), (int)(__result.Width * (1 + increase)), (int)(__result.Height * (1 + increase)));
        }
        public static void Tree_performTreeFall_Prefix(Tree __instance)
        {
            if (!Config.EnableMod)
                return;
            dropDict = SHelper.Content.Load<Dictionary<string, DropData>>(dictPath, StardewModdingAPI.ContentSource.GameContent);
        }
        public static IEnumerable<CodeInstruction> Tree_performTreeFall_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Tree_performTreeFall");

            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>();
            for (int i = 0; i < codes.Count; i++)
            {
                if (i < codes.Count - 23 && codes[i].opcode == OpCodes.Ldarg_S && codes[i + 12].opcode == OpCodes.Ldc_I4_0 && codes[i + 23].opcode == OpCodes.Call && (MethodInfo)codes[i + 23].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createRadialDebris), new System.Type[] { typeof(GameLocation), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(int), typeof(bool), typeof(int) }))
                {
                    SMonitor.Log($"Switching stump drop wood fake");
                    var ci = new CodeInstruction(OpCodes.Ldarg_0);
                    ci.MoveLabelsFrom(codes[i]);
                    newCodes.Add(ci);
                    newCodes.Add(codes[i]);
                    codes[i + 23].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.StumpDropWoodFake));

                }
                else if (i < codes.Count - 9 && codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].operand is sbyte && (sbyte)codes[i].operand == 92 && codes[i + 9].opcode == OpCodes.Call && (MethodInfo)codes[i + 9].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), new System.Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(GameLocation) }))
                {
                    SMonitor.Log($"Switching extra drop 2 method non-farmer");
                    var ci = new CodeInstruction(OpCodes.Ldarg_0);
                    ci.MoveLabelsFrom(codes[i]);
                    newCodes.Add(ci);
                    newCodes.Add(codes[i]);
                    codes[i + 9].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.StumpDrop2ExtraNF));
                }
                else if (false && i < codes.Count - 13 && codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].operand is sbyte && (sbyte)codes[i].operand == 92 && codes[i + 12].opcode == OpCodes.Call && (MethodInfo)codes[i + 12].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createItemDebris), new System.Type[] { typeof(Item), typeof(Vector2), typeof(int), typeof(GameLocation), typeof(int) }))
                {
                    if (codes[i - 1].opcode != OpCodes.Pop)
                    {
                        SMonitor.Log($"Switching extra drop 1 method non-farmer 1");
                        var ci = new CodeInstruction(OpCodes.Ldarg_0);
                        ci.MoveLabelsFrom(codes[i]);
                        newCodes.Add(ci);
                        newCodes.Add(codes[i]);
                        codes[i + 12].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.StumpDropExtraNF1));
                    }
                    else
                    {
                        SMonitor.Log($"Switching extra drop 1 method non-farmer 2");
                        var ci = new CodeInstruction(OpCodes.Ldarg_0);
                        newCodes.Add(ci);
                        newCodes.Add(codes[i]);
                        codes[i + 12].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.StumpDropExtraNF2));
                    }
                }
                else if (i < codes.Count - 12 && codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 12].opcode == OpCodes.Call && (MethodInfo)codes[i + 12].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), new System.Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(long), typeof(GameLocation) }))
                {
                    SMonitor.Log($"Switching extra drop method mp");
                    var ci = new CodeInstruction(OpCodes.Ldarg_0);
                    ci.MoveLabelsFrom(codes[i]);
                    newCodes.Add(ci);
                    newCodes.Add(codes[i]);
                    codes[i + 12].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.StumpDropExtraMP));
                }
                else if (i < codes.Count - 26 && codes[i].opcode == OpCodes.Ldarg_S && codes[i + 22].opcode == OpCodes.Ldc_I4_1 && codes[i + 26].opcode == OpCodes.Call && (MethodInfo)codes[i + 26].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createRadialDebris), new System.Type[] { typeof(GameLocation), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(int), typeof(bool), typeof(int) }))
                {
                    SMonitor.Log($"Switching stump drop wood mp");
                    var ci = new CodeInstruction(OpCodes.Ldarg_0);
                    ci.MoveLabelsFrom(codes[i]);
                    newCodes.Add(ci);
                    newCodes.Add(codes[i]);
                    codes[i + 26].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.StumpDropWood));

                }
                else if (i < codes.Count - 31 && codes[i].opcode == OpCodes.Ldarg_S && codes[i + 27].opcode == OpCodes.Ldc_I4_1 && codes[i + 31].opcode == OpCodes.Call && (MethodInfo)codes[i + 31].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createRadialDebris), new System.Type[] { typeof(GameLocation), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(int), typeof(bool), typeof(int) }))
                {
                    SMonitor.Log($"Switching stump drop wood sp");
                    var ci = new CodeInstruction(OpCodes.Ldarg_0);
                    ci.MoveLabelsFrom(codes[i]);
                    newCodes.Add(ci);
                    newCodes.Add(codes[i]);
                    codes[i + 31].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.StumpDropWood));

                }
                else if (i < codes.Count - 9 && codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 9].opcode == OpCodes.Call && (MethodInfo)codes[i + 9].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), new System.Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(GameLocation) }))
                {
                    SMonitor.Log($"Switching extra drop method sp");
                    var ci = new CodeInstruction(OpCodes.Ldarg_0);
                    ci.MoveLabelsFrom(codes[i]);
                    newCodes.Add(ci);
                    newCodes.Add(codes[i]);
                    codes[i + 9].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.StumpDropExtra));
                }
                else
                    newCodes.Add(codes[i]);
            }

            return newCodes.AsEnumerable();
        }
        public static IEnumerable<CodeInstruction> Tree_tickUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Tree_tickUpdate");

            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>();
            for (int i = 0; i < codes.Count; i++)
            {
                if (i < codes.Count - 39 && codes[i].opcode == OpCodes.Ldarg_3 && codes[i + 39].opcode == OpCodes.Call && (MethodInfo)codes[i + 39].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createRadialDebris), new System.Type[] { typeof(GameLocation), typeof(int),typeof(int),typeof(int),typeof(int), typeof(bool), typeof(int), typeof(bool), typeof(int)}))
                {
                    var ci = new CodeInstruction(OpCodes.Ldarg_0);
                    ci.MoveLabelsFrom(codes[i]);
                    newCodes.Add(ci);
                    newCodes.Add(codes[i]);
                    if(codes[i + 35].opcode == OpCodes.Ldc_I4_1)
                    {
                        SMonitor.Log($"Switching drop wood");
                        codes[i + 39].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DropWood));
                    }
                    else
                    {
                        SMonitor.Log($"Switching drop wood fake");
                        codes[i + 39].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DropWoodFake));
                    }
                }
                else if (i < codes.Count - 20 && codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].operand is sbyte && (sbyte)codes[i].operand == 92 && codes[i + 20].opcode == OpCodes.Call && (MethodInfo)codes[i + 20].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), new System.Type[] { typeof(int), typeof(int),typeof(int),typeof(int),typeof(long),typeof(GameLocation)}))
                {
                    SMonitor.Log($"Switching mp drop sap method");
                    var ci = new CodeInstruction(OpCodes.Ldarg_0);
                    ci.MoveLabelsFrom(codes[i]);
                    newCodes.Add(ci);
                    newCodes.Add(codes[i]);
                    codes[i+20].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DropSapMP));
                }
                else if (i < codes.Count - 20 && codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == 709 && codes[i + 20].opcode == OpCodes.Call && (MethodInfo)codes[i + 20].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), new System.Type[] { typeof(int), typeof(int),typeof(int),typeof(int),typeof(long),typeof(GameLocation)}))
                {
                    SMonitor.Log($"Switching mp drop hardwood method");
                    if (newCodes[newCodes.Count - 1].opcode == OpCodes.Ble_S)
                        newCodes[newCodes.Count - 1].opcode = OpCodes.Blt_S;
                    var ci = new CodeInstruction(OpCodes.Ldarg_0);
                    ci.MoveLabelsFrom(codes[i]);
                    newCodes.Add(ci);
                    newCodes.Add(codes[i]);
                    codes[i + 20].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DropHardwoodMP));
                }
                else if (i < codes.Count - 27 && codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == 308 && codes[i + 27].opcode == OpCodes.Call && (MethodInfo)codes[i + 27].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), new System.Type[] { typeof(int), typeof(int),typeof(int),typeof(int),typeof(long),typeof(GameLocation)}))
                {
                    SMonitor.Log($"Switching mp drop seed method 1");
                    var ci = new CodeInstruction(OpCodes.Ldarg_0);
                    ci.MoveLabelsFrom(codes[i]);
                    newCodes.Add(ci);
                    newCodes.Add(codes[i]);
                    codes[i+27].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DropSeedMP1));
                }
                else if (i < codes.Count - 23 && codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == 292 && codes[i + 23].opcode == OpCodes.Call && (MethodInfo)codes[i + 23].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), new System.Type[] { typeof(int), typeof(int),typeof(int),typeof(int),typeof(long),typeof(GameLocation)}))
                {
                    SMonitor.Log($"Switching mp drop seed method 2");
                    var ci = new CodeInstruction(OpCodes.Ldarg_0);
                    ci.MoveLabelsFrom(codes[i]);
                    newCodes.Add(ci);
                    newCodes.Add(codes[i]);
                    codes[i+ 23].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DropSeedMP2));
                }
                else if (i < codes.Count - 17 && codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].operand is sbyte && (sbyte)codes[i].operand == 92 && codes[i + 17].opcode == OpCodes.Call && (MethodInfo)codes[i + 17].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), new System.Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(GameLocation) }))
                {
                    SMonitor.Log($"Switching sp drop sap method");
                    var ci = new CodeInstruction(OpCodes.Ldarg_0);
                    ci.MoveLabelsFrom(codes[i]);
                    newCodes.Add(ci);
                    newCodes.Add(codes[i]);
                    codes[i + 17].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DropSap));
                }
                else if (i < codes.Count - 24 && codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == 308 && codes[i + 24].opcode == OpCodes.Call && (MethodInfo)codes[i + 24].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), new System.Type[] { typeof(int), typeof(int),typeof(int),typeof(int),typeof(GameLocation)}))
                {
                    SMonitor.Log($"Switching sp drop seed method 1");
                    var ci = new CodeInstruction(OpCodes.Ldarg_0);
                    ci.MoveLabelsFrom(codes[i]);
                    newCodes.Add(ci);
                    newCodes.Add(codes[i]);
                    codes[i+24].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DropSeed1));
                }
                else if (i < codes.Count - 20 && codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == 292 && codes[i + 20].opcode == OpCodes.Call && (MethodInfo)codes[i + 20].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), new System.Type[] { typeof(int), typeof(int),typeof(int),typeof(int),typeof(GameLocation)}))
                {
                    SMonitor.Log($"Switching sp drop seed method 2");
                    var ci = new CodeInstruction(OpCodes.Ldarg_0);
                    ci.MoveLabelsFrom(codes[i]);
                    newCodes.Add(ci);
                    newCodes.Add(codes[i]);
                    codes[i+ 20].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DropSeed2));
                }
                else if (i < codes.Count - 17 && codes[i].opcode == OpCodes.Ldc_I4 && codes[i + 17].opcode == OpCodes.Call && (MethodInfo)codes[i + 17].operand == AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), new System.Type[] { typeof(int), typeof(int),typeof(int),typeof(int),typeof(GameLocation)}))
                {
                    switch ((int)codes[i].operand)
                    {
                        case 709:
                            SMonitor.Log($"Switching sp drop hardwood method");
                            if (newCodes[newCodes.Count - 1].opcode == OpCodes.Ble_S)
                                newCodes[newCodes.Count - 1].opcode = OpCodes.Blt_S;
                            var ci = new CodeInstruction(OpCodes.Ldarg_0);
                            ci.MoveLabelsFrom(codes[i]);
                            newCodes.Add(ci);
                            newCodes.Add(codes[i]);
                            codes[i + 17].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DropHardwood));
                            break;
                        case 420:
                            SMonitor.Log($"Switching mushroom drop wood method");
                            var ci2 = new CodeInstruction(OpCodes.Ldarg_0);
                            ci2.MoveLabelsFrom(codes[i]);
                            newCodes.Add(ci2);
                            newCodes.Add(codes[i]);
                            codes[i + 17].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DropMushroomWood));
                            break;
                    }
                }
                else
                    newCodes.Add(codes[i]);
            }

            return newCodes.AsEnumerable();
        }
    }
}