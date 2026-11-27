using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Tools;
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

        //[HarmonyPatch(typeof(Game1), nameof(Game1.loadForNewGame))]
        public static class Game1_loadForNewGame_Patch
        {
            public static void Postfix()
            {
                if (!Config.ModEnabled)
                    return;

                openWorldLocation = new GameLocation(mapPath, locName) { IsOutdoors = true, IsFarm = true, IsGreenhouse = false };
                SMonitor.Log("Created new game location");

                Game1.locations.Add(openWorldLocation);
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.loadMap))]
        public static class GameLocation_loadMap_Patch
        {
            public static void Postfix(GameLocation __instance)
            {
                if (!Config.ModEnabled || !__instance.Name.Contains(locName))
                    return;
                AccessTools.FieldRefAccess<Map, Size>(__instance.Map, "m_displaySize") = new Size(Config.OpenWorldSize, Config.OpenWorldSize);
                AccessTools.FieldRefAccess<Map, string>(__instance.Map, "m_id") = locName;
            }
        }
        [HarmonyPatch(typeof(Map), "UpdateDisplaySize")]
        public static class Map_UpdateDisplaySize_Patch
        {
            public static bool Prefix(string ___m_id, ref Size ___m_displaySize)
            {
                if (!Config.ModEnabled || !___m_id.Contains(locName))
                    return true;
                ___m_displaySize = new Size(100000, 100000);
                return false;
            }
        }
        [HarmonyPatch(typeof(Map), nameof(Map.DisplayWidth))]
        [HarmonyPatch(MethodType.Getter)]
        public static class Map_DisplayWidth_Patch
        {
            public static bool Prefix(Map __instance, ref int __result)
            {
                if (!Config.ModEnabled || !__instance.Id.Contains(locName))
                    return true;
                __result = Config.OpenWorldSize * 64;
                return false;
            }
        }
        [HarmonyPatch(typeof(Map), nameof(Map.DisplayHeight))]
        [HarmonyPatch(MethodType.Getter)]
        public static class Map_DisplayHeight_Patch
        {
            public static bool Prefix(Map __instance, ref int __result)
            {
                if (!Config.ModEnabled || !__instance.Id.Contains(locName))
                    return true;
                __result = Config.OpenWorldSize * 64;
                return false;
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.CanTakeScreenshots))]
        public static class Game1_CanTakeScreenshots_Patch
        {
            public static void Postfix(ref bool __result)
            {
                if(Config.ModEnabled && __result && Game1.currentLocation == openWorldLocation)
                    __result = true;
            }
        }
        [HarmonyPatch(typeof(Game1), "GetScreenshotRegion")]
        public static class Game1_GetScreenshotRegion_Patch
        {
            public static void Postfix(GameLocation screenshotLocation, ref int startX, ref int startY, ref int width, ref int height)
            {
                if (!Config.ModEnabled || Game1.currentLocation != openWorldLocation)
                    return;
                startX = (int)Game1.player.Position.X - openWorldChunkSize * 64;
                startY = (int)Game1.player.Position.Y - openWorldChunkSize * 64;
                width = openWorldChunkSize * 128;
                height = openWorldChunkSize * 128;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public static class Object_placementAction_Patch
        {
            public static void Postfix(Object __instance, GameLocation location)
            {
                if(!Config.ModEnabled || location != openWorldLocation) 
                    return;
                __instance.modData[modPlacedKey] = "True";
            }
        }
        [HarmonyPatch(typeof(Serpent), nameof(Serpent.behaviorAtGameTick))]
        public static class Serpent_behaviorAtGameTick_Patch
        {
            public static bool Prefix(Serpent __instance, GameTime time)
            {
                if (!Config.ModEnabled || __instance.currentLocation != openWorldLocation)
                    return true;
                var method = typeof(Monster).GetMethod(nameof(Monster.behaviorAtGameTick));
                var ftn = method.MethodHandle.GetFunctionPointer();
                var func = (Action<GameTime>)Activator.CreateInstance(typeof(Action<GameTime>), __instance, ftn);
                func(time);

                if (double.IsNaN((double)__instance.xVelocity) || double.IsNaN((double)__instance.yVelocity))
                {
                    __instance.Health = -500;
                }
                if (__instance.Position.X <= -640f || __instance.Position.Y <= -640f || __instance.Position.X >= Config.OpenWorldSize * 64 + 640 || __instance.Position.Y >= Config.OpenWorldSize * 64 + 640)
                {
                    __instance.Health = -500;
                }
                if (__instance.withinPlayerThreshold() && __instance.invincibleCountdown <= 0)
                {
                    __instance.faceDirection(2);
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(Monster), nameof(Monster.updateMovement))]
        public static class Monster_updateMovement_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Monster.updateMovement");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Stloc_0)
                    {
                        SMonitor.Log("Overriding player tile check");

                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMonsterPlayerTile))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_1));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }
                return codes.AsEnumerable();
            }
        }


        [HarmonyPatch(typeof(Monster), nameof(Monster.update))]
        public static class Monster_update_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Monster.update");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(NetCollection<NPC>), "Remove", new Type[] { typeof(NPC) }))
                    {
                        SMonitor.Log("Preventing monster removal for out of bounds");

                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DontRemoveMonster));
                        codes.RemoveAt(i + 1);
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_2));
                        break;
                    }
                }
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.hasTileAt), new Type[] { typeof(int),typeof(int),typeof(string),typeof(string) })]
        public static class GameLocation_hasTileAt_Patch1
        {
            public static bool Prefix(GameLocation __instance, int x, int y, string layer, string tilesheetId, ref bool __result)
            {
                if (!Config.ModEnabled || layer != "Back" || !__instance.Name.Contains(locName) || x < 0 || y < 0 || x >= Config.OpenWorldSize || y >= Config.OpenWorldSize)
                    return true;
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.hasTileAt), new Type[] { typeof(Location),typeof(string),typeof(string) })]
        public static class GameLocation_hasTileAt_Patch2
        {
            public static bool Prefix(GameLocation __instance, Location tile, string layer, string tilesheetId, ref bool __result)
            {
                if (!Config.ModEnabled || layer != "Back" || !__instance.Name.Contains(locName) || tile.X < 0 || tile.Y < 0 || tile.X >= Config.OpenWorldSize || tile.Y >= Config.OpenWorldSize)
                    return true;
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.isOutdoorMapSmallerThanViewport))]
        public static class Game1_isOutdoorMapSmallerThanViewport_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                return !Config.ModEnabled || Game1.currentLocation is null || !Game1.currentLocation.Name.Contains(locName);
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.UpdateViewPort))]
        public static class Game1_UpdateViewPort_Patch
        {
            public static void Prefix(ref bool overrideFreeze)
            {
                if (!Config.ModEnabled || !Game1.currentLocation.Name.Contains(locName))
                    return;
                overrideFreeze = true;
                Game1.forceSnapOnNextViewportUpdate = true;
                Game1.currentLocation.forceViewportPlayerFollow = true;
            }
        }
        [HarmonyPatch(typeof(Layer), nameof(Layer.Tiles))]
        [HarmonyPatch(MethodType.Getter)]
        public static class Layer_Tiles_Getter_Patch
        {            
            public static void Postfix(Layer __instance, TileArray __result)
            {
                if (!Config.ModEnabled || !__instance.Map.Id.Contains(locName))
                    return;
                __result = new MyTileArray(__instance, __result);
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.IsOutOfBounds))]
        public static class GameLocation_IsOutOfBounds_Patch
        {            
            public static bool Prefix(GameLocation __instance, Rectangle pixelPosition, ref bool __result)
            {
                if (!Config.ModEnabled || __instance != openWorldLocation)
                    return true;
                if (pixelPosition.Right < 0 || pixelPosition.Bottom < 0)
                {
                    __result = true;
                }
                __result = pixelPosition.X > Config.OpenWorldSize * 64 || pixelPosition.Top > Config.OpenWorldSize * 64;
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        public static class GameLocation_isCollidingPosition_Patch
        {
            public static bool Prefix(GameLocation __instance, Rectangle position, Rectangle viewport, bool isFarmer, bool projectile, Character character, ref bool __result)
            {
                if (!Config.ModEnabled || __instance != openWorldLocation)
                    return true;
                BoundingBoxGroup passableTiles = null;
                Rectangle? currentBounds;
                if (character is Farmer farmer)
                {
                    isFarmer = true;
                    var bb = farmer.GetBoundingBox();
                    currentBounds = new Rectangle?(bb);
                    
                    passableTiles = farmer.TemporaryPassableTiles;
                }
                else
                {
                    farmer = null;
                    isFarmer = false;
                    currentBounds = null;
                }
                Vector2? currentTopRight = null;
                Vector2? currentTopLeft = null;
                Vector2? currentBottomRight = null;
                Vector2? currentBottomLeft = null;
                Vector2? currentBottomMid = null;
                Vector2? currentTopMid = null;
                if (currentBounds != null)
                {
                    currentTopRight = new Vector2?(new Vector2((float)((currentBounds.Value.Right - 1) / 64), (float)(currentBounds.Value.Top / 64)));
                    currentTopLeft = new Vector2?(new Vector2((float)(currentBounds.Value.Left / 64), (float)(currentBounds.Value.Top / 64)));
                    currentBottomRight = new Vector2?(new Vector2((float)((currentBounds.Value.Right - 1) / 64), (float)((currentBounds.Value.Bottom - 1) / 64)));
                    currentBottomLeft = new Vector2?(new Vector2((float)(currentBounds.Value.Left / 64), (float)((currentBounds.Value.Bottom - 1) / 64)));
                    currentBottomMid = new Vector2?(new Vector2((float)(currentBounds.Value.Center.X / 64), (float)((currentBounds.Value.Bottom - 1) / 64)));
                    currentTopMid = new Vector2?(new Vector2((float)(currentBounds.Value.Center.X / 64), (float)(currentBounds.Value.Top / 64)));
                }

                Vector2 nextTopRight = new Vector2((float)(position.Right / 64), (float)(position.Top / 64));
                Vector2 nextTopLeft = new Vector2((float)(position.Left / 64), (float)(position.Top / 64));
                Vector2 nextBottomRight = new Vector2((float)(position.Right / 64), (float)(position.Bottom / 64));
                Vector2 nextBottomLeft = new Vector2((float)(position.Left / 64), (float)(position.Bottom / 64));
                bool nextLargerThanTile = position.Width > 64;
                Vector2 nextBottomMid = new Vector2((float)(position.Center.X / 64), (float)(position.Bottom / 64));
                Vector2 nextTopMid = new Vector2((float)(position.Center.X / 64), (float)(position.Top / 64));

                Tile tmp;

                if ((bool)AccessTools.Method(typeof(GameLocation), "_TestCornersTiles").Invoke(__instance, new object[]{nextTopRight, nextTopLeft, nextBottomRight, nextBottomLeft, nextTopMid, nextBottomMid, currentTopRight, currentTopLeft, currentBottomRight, currentBottomLeft, currentTopMid, currentBottomMid, nextLargerThanTile, delegate (Vector2 tile)
                {
                    //if (isFarmer && __instance.terrainFeatures.TryGetValue(tile, out var tf))
                    //{
                    //    var asdf = tf.getBoundingBox();
                    //    var xxx= asdf.Intersects(position);
                    //    var yyy = tf.isPassable(character);
                    //    if(xxx && !yyy)
                    //    {
                    //        if(tf is Bush)
                    //        {
                    //            var asdfjkl = tf.getBoundingBox();
                    //            var asdfjklsdaf = Game1.player.GetBoundingBox();
                    //            var wqeopiuqwer = true;
                    //        }
                    //        return true;
                    //    }
                    //}
                    //if (__instance.Objects.TryGetValue(tile, out var obj))
                    //{
                    //    var asdf = obj.GetBoundingBox();
                    //    var xxx= asdf.Intersects(position);
                    //    var yyy = obj.isPassable();
                    //    if(xxx && !yyy)
                    //        return true;
                    //}
                    int cx = (int)tile.X / openWorldChunkSize;
                    int cy = (int)tile.Y / openWorldChunkSize;
                    int tx = (int)tile.X % openWorldChunkSize;
                    int ty = (int)tile.Y % openWorldChunkSize;
                    var backTiles = GetChunkTiles("Back", cx, cy);
                    var buildingTiles = GetChunkTiles("Buildings", cx, cy);
                    if(buildingTiles is null)
                        return false;
                    tmp = buildingTiles[tx,ty];
                    if (tmp != null)
                    {
                        bool flag3;
                        if (!tmp.TileIndexProperties.ContainsKey("Shadow") && !tmp.TileIndexProperties.ContainsKey("Passable") && !tmp.Properties.ContainsKey("Passable") && (!projectile || (!tmp.TileIndexProperties.ContainsKey("ProjectilePassable") && !tmp.Properties.ContainsKey("ProjectilePassable"))))
                        {
                            if (!isFarmer)
                            {
                                if (!tmp.TileIndexProperties.ContainsKey("NPCPassable") && !tmp.Properties.ContainsKey("NPCPassable"))
                                {
                                    Character character3 = character;
                                    bool? flag2 = ((character3 != null) ? new bool?(character3.canPassThroughActionTiles()) : null);
                                    flag3 = flag2 != null && flag2.GetValueOrDefault() && tmp.Properties.ContainsKey("Action");
                                }
                                else
                                {
                                    flag3 = true;
                                }
                            }
                            else
                            {
                                flag3 = false;
                            }
                        }
                        else
                        {
                            flag3 = true;
                        }
                        if (!flag3)
                        {
                            return passableTiles == null || !passableTiles.Contains((int)tile.X, (int)tile.Y);
                        }
                    }
                    return false;
                } }))
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isTileOnMap), new Type[] { typeof(Vector2) })]
        public static class GameLocation_isTileOnMap_Patch1
        {
            public static bool Prefix(GameLocation __instance, Vector2 position, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.Name.Contains(locName))
                    return true;
                __result = position.X >= 0f && position.X < Config.OpenWorldSize && position.Y >= 0f && position.Y < Config.OpenWorldSize;
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isTileOnMap), new Type[] { typeof(int), typeof(int) })]
        public static class GameLocation_isTileOnMap_Patch2
        {
            public static bool Prefix(GameLocation __instance, int x, int y, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.Name.Contains(locName))
                    return true;
                __result = x >= 0f && x < Config.OpenWorldSize && y >= 0f && y < Config.OpenWorldSize;
                return false;
            }
        }

        [HarmonyPatch(typeof(FarmerRenderer), nameof(FarmerRenderer.drawHairAndAccesories))]
        public static class FarmerRenderer_drawHairAndAccesories_Patch2
        {
            public static void Prefix(FarmerRenderer __instance, SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, ref float layerDepth)
            {
                if (!Config.ModEnabled)
                    return;
            }
            public static void Postfix(FarmerRenderer __instance, SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth)
            {
                if (!Config.ModEnabled)
                    return;
            }
        }

    }
}