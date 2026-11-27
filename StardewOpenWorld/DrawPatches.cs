using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Netcode;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewOpenWorld
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Character), nameof(Character.draw), new Type[] { typeof(SpriteBatch), typeof(float) })]
        public static class Character_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Character.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Point), nameof(Point.Y)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(IntToLocalY))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.draw), new Type[] { typeof(SpriteBatch), typeof(float) })]
        public static class NPC_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling NPC.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Point), nameof(Point.Y)) && codes[i - 1].opcode == OpCodes.Call && codes[i - 1].operand is MethodInfo && (MethodInfo)codes[i - 1].operand == AccessTools.PropertyGetter(typeof(Character), nameof(Character.StandingPixel)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(IntToLocalY))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.DrawBreathing), new Type[] { typeof(SpriteBatch), typeof(float) })]
        public static class NPC_DrawBreathing_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling NPC.DrawBreathing");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Point), nameof(Point.Y)) && codes[i - 1].opcode == OpCodes.Call && codes[i - 1].operand is MethodInfo && (MethodInfo)codes[i - 1].operand == AccessTools.PropertyGetter(typeof(Character), nameof(Character.StandingPixel)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(IntToLocalY))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Horse), nameof(Horse.draw), new Type[] { typeof(SpriteBatch)})]
        public static class Horse_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Horse.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Point), nameof(Point.Y)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(IntToLocalY))));
                        i++;
                    }
                    else if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Character), nameof(Character.position)) && codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 1].operand is MethodInfo && (MethodInfo)codes[i + 1].operand == AccessTools.PropertyGetter(typeof(NetPosition), nameof(NetPosition.Y)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(FloatToLocalY))));
                        i += 2;
                    }
                    else if (i > 0 && codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Vector2), nameof(Vector2.Y)) && codes[i - 1].opcode == OpCodes.Call && codes[i - 1].operand is MethodInfo && (MethodInfo)codes[i - 1].operand == AccessTools.PropertyGetter(typeof(Character), nameof(Character.Position)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(FloatToLocalY))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(GreenSlime), nameof(GreenSlime.draw), new Type[] { typeof(SpriteBatch) })]
        public static class GreenSlime_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling GreenSlime.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Point), nameof(Point.Y)) && codes[i - 1].opcode == OpCodes.Call && codes[i - 1].operand is MethodInfo && (MethodInfo)codes[i - 1].operand == AccessTools.PropertyGetter(typeof(Character), nameof(Character.StandingPixel)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.IntToLocalY))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(BigSlime), nameof(BigSlime.draw), new Type[] { typeof(SpriteBatch) })]
        public static class BigSlime_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling BigSlime.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Point), nameof(Point.Y)) && codes[i - 1].opcode == OpCodes.Call && codes[i - 1].operand is MethodInfo && (MethodInfo)codes[i - 1].operand == AccessTools.PropertyGetter(typeof(Character), nameof(Character.StandingPixel)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.IntToLocalY))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(DinoMonster), nameof(DinoMonster.draw), new Type[] { typeof(SpriteBatch) })]
        public static class DinoMonster_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling DinoMonster.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Point), nameof(Point.Y)) && codes[i - 1].opcode == OpCodes.Call && codes[i - 1].operand is MethodInfo && (MethodInfo)codes[i - 1].operand == AccessTools.PropertyGetter(typeof(Character), nameof(Character.StandingPixel)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.IntToLocalY))));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Grass), nameof(Grass.draw))]
        public static class Grass_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Grass.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Vector2), nameof(Vector2.X)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.FloatToLocalX))));
                        i++;
                    }
                    else if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Vector2), nameof(Vector2.Y)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.FloatToLocalY))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Tree), nameof(Tree.draw))]
        public static class Tree_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Tree.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.PropertyGetter(typeof(Rectangle), nameof(Rectangle.Bottom)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.IntToLocalY))));
                        i++;
                    }
                    else if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Vector2), nameof(Vector2.X)) && codes[i + 1].opcode == OpCodes.Ldc_R4 && codes[i + 2].opcode == OpCodes.Div)
                    {
                        SMonitor.Log("Adding method to adjust draw layer");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.FloatToLocalXTile))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Crop), nameof(Crop.updateDrawMath))]
        public static class Crop_updateDrawMath_Patch
        {
            public static void Postfix(Crop __instance, Vector2 tileLocation)
            {
                if (__instance.currentLocation != openWorldLocation || !Context.IsWorldReady)
                    return;
                var y = FloatToLocalYTile(tileLocation.Y);
                var x = FloatToLocalXTile(tileLocation.X);
                if (__instance.forageCrop.Value)
                {
                    __instance.layerDepth = (y * 64f + 32f + ((y * 11f + x * 7f) % 10f - 5f)) / 10000f;
                }
                else
                {
                    __instance.layerDepth = (y * 64f + 32f + ((!__instance.shouldDrawDarkWhenWatered() || __instance.currentPhase.Value >= __instance.phaseDays.Count - 1) ? 0f : ((y * 11f + x * 7f) % 10f - 5f))) / 10000f / ((__instance.currentPhase.Value == 0 && __instance.shouldDrawDarkWhenWatered()) ? 2f : 1f);
                    __instance.coloredLayerDepth = (y * 64f + 32f + ((y * 11f + x * 7f) % 10f - 5f)) / 10000f / (float)((__instance.currentPhase.Value == 0 && __instance.shouldDrawDarkWhenWatered()) ? 2 : 1);

                }

            }
        }
        
        [HarmonyPatch(typeof(ResourceClump), nameof(ResourceClump.draw), new Type[] { typeof(SpriteBatch) })]
        public static class ResourceClump_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling ResourceClump.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Vector2), nameof(Vector2.Y)) && codes[i + 8].opcode == OpCodes.Ldfld && (FieldInfo)codes[i + 8].operand == AccessTools.Field(typeof(Vector2), nameof(Vector2.X)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");

                        codes.Insert(i + 9, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.FloatToLocalXTile))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.FloatToLocalYTile))));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Bush), nameof(Bush.draw), new Type[] { typeof(SpriteBatch) })]
        public static class Bush_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Bush.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (CodesCompare(codes, i, new OpCode[] { OpCodes.Ldfld, OpCodes.Ldc_I4_S, OpCodes.Add, OpCodes.Conv_R4, OpCodes.Ldc_R4, OpCodes.Div, OpCodes.Ldloc_0, OpCodes.Ldfld, OpCodes.Ldc_R4, OpCodes.Div, OpCodes.Sub }))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");

                        codes.Insert(i + 11, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetBushDrawLayer))));
                        codes.Insert(i + 11, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Chest), nameof(Chest.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class Chest_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Chest.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (CodesCompare(codes, i, new OpCode[] { OpCodes.Ldc_R4, OpCodes.Ldloc_1, OpCodes.Ldc_R4, OpCodes.Add, OpCodes.Ldc_R4, OpCodes.Mul, OpCodes.Ldc_R4, OpCodes.Sub, OpCodes.Ldc_R4, OpCodes.Div, OpCodes.Call, OpCodes.Ldloc_0, OpCodes.Ldc_R4, OpCodes.Mul, OpCodes.Add, OpCodes.Stloc_2 }))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");

                        codes.Insert(i + 15, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetChestDrawLayer))));
                        codes.Insert(i + 15, new CodeInstruction(OpCodes.Ldarg_3));
                        codes.Insert(i + 15, new CodeInstruction(OpCodes.Ldarg_2));
                        codes.Insert(i + 15, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class Object_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Object.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (CodesCompare(codes, i, new OpCode[] { OpCodes.Ldc_R4, OpCodes.Ldarg_3, OpCodes.Ldc_I4_1, OpCodes.Add, OpCodes.Ldc_I4_S, OpCodes.Mul, OpCodes.Ldc_I4_S, OpCodes.Sub, OpCodes.Conv_R4, OpCodes.Ldc_R4, OpCodes.Div, OpCodes.Call, OpCodes.Ldarg_2, OpCodes.Conv_R4, OpCodes.Ldc_R4, OpCodes.Mul, OpCodes.Add, OpCodes.Stloc_S }))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");

                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetObjectDrawLayer))));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_3));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_2));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 21;
                    }
                    else if (CodesCompare(codes, i, new OpCode[] { OpCodes.Ldc_R4, OpCodes.Ldarg_3, OpCodes.Ldc_I4_1, OpCodes.Add, OpCodes.Ldc_I4_S, OpCodes.Mul, OpCodes.Ldc_I4_2, OpCodes.Add, OpCodes.Conv_R4, OpCodes.Ldc_R4, OpCodes.Div, OpCodes.Call, OpCodes.Ldarg_2, OpCodes.Conv_R4, OpCodes.Ldc_R4, OpCodes.Div, OpCodes.Add, OpCodes.Stloc_S }))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");

                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetObjectDrawLayer2))));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_3));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_2));
                        codes.Insert(i + 17, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 21;
                    }
                    else if (CodesCompare(codes, i, new OpCode[] { OpCodes.Ldarg_3, OpCodes.Ldc_I4_1, OpCodes.Add, OpCodes.Ldc_I4_S, OpCodes.Mul, OpCodes.Conv_R4, OpCodes.Ldc_R4, OpCodes.Div, OpCodes.Ldarg_0, OpCodes.Ldfld, OpCodes.Callvirt, OpCodes.Ldc_R4, OpCodes.Div, OpCodes.Add, OpCodes.Stloc_S }))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");

                        codes.Insert(i + 14, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetObjectDrawLayer3))));
                        codes.Insert(i + 14, new CodeInstruction(OpCodes.Ldarg_3));
                        codes.Insert(i + 14, new CodeInstruction(OpCodes.Ldarg_2));
                        codes.Insert(i + 14, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 15;
                    }
                    else if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Object), nameof(Object.GetBoundingBoxAt)))
                    {
                        SMonitor.Log("Adding method to adjust draw layer");

                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetObjectBoundingBox))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_3));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_2));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 4;
                    }
                }
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.getDrawLayer))]
        public static class Farmer_getDrawLayer_Patch
        {
            public static bool Prefix(Farmer __instance, ref float __result)
            {
                if (!Config.ModEnabled || !Context.IsWorldReady || __instance.currentLocation != openWorldLocation)
                    return true;
                int rsp = __instance.StandingPixel.Y % (openWorldChunkSize * 64) + (Game1.viewport.Y / (openWorldChunkSize * 64) < __instance.StandingPixel.Y / (openWorldChunkSize * 64) ? openWorldChunkSize * 64 : 0);
                if (__instance.onBridge.Value)
                {
                    __result = rsp / 10000f + __instance.drawLayerDisambiguator + 0.0256f;
                }
                else if (__instance.IsSitting() && __instance.mapChairSitPosition.Value.X != -1f && __instance.mapChairSitPosition.Value.Y != -1f)
                {
                    Vector2 sit_position = __instance.mapChairSitPosition.Value;
                    __result = ((sit_position.Y % (openWorldChunkSize * 128) + 1f) * 64f + 3141) / 10000f;
                }
                else
                    __result = rsp / 10000f + __instance.drawLayerDisambiguator;
                return false;
            }
        }



        [HarmonyPatch(typeof(Layer), nameof(Layer.Draw))]
        public static class Layer_Draw_Patch
        {
            public static bool Prefix(Layer __instance, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, int pixelZoom, float sort_offset)
            {
                if (!Config.ModEnabled || !Context.IsWorldReady || Game1.currentLocation != openWorldLocation)
                    return true;
                Layer.zoom = pixelZoom;
                int tileWidth = pixelZoom * 16;
                int tileHeight = pixelZoom * 16;

                Location tileInternalOffset = new Location(Wrap(mapViewport.X, tileWidth), Wrap(mapViewport.Y, tileHeight));
                int tileXMin = ((mapViewport.X >= 0) ? (mapViewport.X / tileWidth) : ((mapViewport.X - tileWidth + 1) / tileWidth));
                int tileYMin = ((mapViewport.Y >= 0) ? (mapViewport.Y / tileHeight) : ((mapViewport.Y - tileHeight + 1) / tileHeight));
                if (tileXMin < 0)
                {
                    displayOffset.X -= tileXMin * tileWidth;
                    tileXMin = 0;
                }
                if (tileYMin < 0)
                {
                    displayOffset.Y -= tileYMin * tileHeight;
                    tileYMin = 0;
                }
                int tileColumns = 1 + (mapViewport.Size.Width - 1) / tileWidth;
                int tileRows = 1 + (mapViewport.Size.Height - 1) / tileHeight;
                if (tileInternalOffset.X != 0)
                {
                    tileColumns++;
                }
                if (tileInternalOffset.Y != 0)
                {
                    tileRows++;
                }
                int tileXMax = Math.Min(tileXMin + tileColumns, Config.OpenWorldSize);
                int tileYMax = Math.Min(tileYMin + tileRows, Config.OpenWorldSize);

                Rectangle drawBounds = new(tileXMin, tileYMin, tileXMax - tileXMin, tileYMax - tileYMin);

                Location tileLocation = displayOffset - tileInternalOffset;

                Point loc = Game1.player.TilePoint;
                Tile[,] tiles = new Tile[openWorldChunkSize, openWorldChunkSize];
                Point playerChunk = new(loc.X / openWorldChunkSize, loc.Y / openWorldChunkSize);
                Rectangle playerBox = new(loc.X - openWorldChunkSize / 2, loc.Y - openWorldChunkSize / 2, openWorldChunkSize, openWorldChunkSize);
                Dictionary<Point, Tile[,]> surroundingChunkTiles = new();

                for (int y = -1; y < 2; y++)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        var cx = playerChunk.X + x;
                        var cy = playerChunk.Y + y;
                        if (!IsChunkInMap(cx, cy))
                            continue;
                        Rectangle chunkBounds = new(cx * openWorldChunkSize, cy * openWorldChunkSize, openWorldChunkSize, openWorldChunkSize);
                        if (!chunkBounds.Intersects(drawBounds))
                            continue;
                        Tile[,] chunkTiles = GetChunkTiles(__instance.Id, cx, cy);
                        surroundingChunkTiles.Add(new(cx, cy), chunkTiles);
                    }
                }
                for (int ay = drawBounds.Y; ay < drawBounds.Bottom; ay++)
                {
                    tileLocation.X = displayOffset.X - tileInternalOffset.X;
                    for (int ax = drawBounds.X; ax < drawBounds.Right; ax++)
                    {
                        foreach (var kvp in surroundingChunkTiles)
                        {
                            if (kvp.Value is null)
                                continue;
                            if (ax / openWorldChunkSize == kvp.Key.X && ay / openWorldChunkSize == kvp.Key.Y)
                            {
                                var ry = ay % openWorldChunkSize;
                                Tile tile = kvp.Value[ax % openWorldChunkSize, ry];
                                if (tile is not null)
                                {
                                    float drawn_sort = 0f;
                                    if (sort_offset >= 0f)
                                    {
                                        
                                        drawn_sort = (ay * (16 * pixelZoom) + 16 * pixelZoom + sort_offset + ChunkDisplayOffset(ay)) / 10000f;
                                        if(Game1.player.Tile.X == ax && Game1.player.Tile.Y == ay)
                                        {
                                            var asdf = drawn_sort;
                                        }
                                    }
                                    displayDevice.DrawTile(tile, tileLocation, drawn_sort);
                                }
                                break;
                            }
                        }
                        tileLocation.X += tileWidth;
                    }
                    tileLocation.Y += tileHeight;
                }
                return false;
            }

            private static int Wrap(int value, int span)
            {
                value %= span;
                if (value < 0)
                {
                    value += span;
                }
                return value;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.drawWater))]
        public static class GameLocation_drawWater_Patch
        {
            public static bool Prefix(GameLocation __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !Context.IsWorldReady || __instance != openWorldLocation)
                    return true;
                int sx = Game1.viewport.X / 64 - 1;
                int sy = Game1.viewport.Y / 64 - 1;
                int ex = sx + Game1.viewport.Width / 64 + 3;
                int ey = sy + Game1.viewport.Height / 64 + 3;
                for(int x = sx; x < ex; x++)
                {
                    for (int y = sy; y < ey; y++)
                    {
                        var cp = GetTileChunk(new Point(x, y));
                        if (cachedChunks.TryGetValue(cp, out var chunk) && chunk.tiles["Back"][x % openWorldChunkSize, y % openWorldChunkSize]?.Properties.ContainsKey("Water") == true)
                        {
                            drawWaterTile(b, x, y);
                        }
                    }
                }
                return false;
            }
        }


        public static void drawWaterTile(SpriteBatch b, int x, int y)
        {
            bool waterSouth = waterTiles.Contains(new Point(x, y + 1));
            bool topY = y == 0 || !waterTiles.Contains(new Point(x, y - 1));
            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - (int)((!topY) ? Game1.currentLocation.waterPosition : 0f)))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2064 + (((x + y) % 2 == 0) ? (Game1.currentLocation.waterTileFlip ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 0 : 128)) + (topY ? ((int)Game1.currentLocation.waterPosition) : 0), 64, 64 + (topY ? ((int)(-(int)Game1.currentLocation.waterPosition)) : 0))), Game1.currentLocation.waterColor.Value, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.56f);
            if (!waterSouth)
            {
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)((y + 1) * 64 - (int)Game1.currentLocation.waterPosition))), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2064 + (((x + (y + 1)) % 2 == 0) ? (Game1.currentLocation.waterTileFlip ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 0 : 128)), 64, 64 - (int)(64f - Game1.currentLocation.waterPosition) - 1)), Game1.currentLocation.waterColor.Value, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.56f);
            }
        }
    }
}