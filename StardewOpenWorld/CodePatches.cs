using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Diagnostics;
using System.Reflection;
using xTile;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;

namespace StardewOpenWorld
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Game1), nameof(Game1.loadForNewGame))]
        public static class Game1_loadForNewGame_Patch
        {
            public static void Postfix()
            {
                if (!Config.ModEnabled)
                    return;


                openWorldLocation = new GameLocation(mapPath, locName) { IsOutdoors = true, IsFarm = true, IsGreenhouse = false };
                SMonitor.Log("Created new game location");
                var back = openWorldLocation.Map.GetLayer("Back");
                var mainSheet = openWorldLocation.Map.GetTileSheet("outdoors");

                grassTiles = new Tile[openWorldTileSize, openWorldTileSize];
                SMonitor.Log($"creating grass tiles");
                for (int y = 0; y < openWorldTileSize; y++)
                {
                    for (int x = 0; x < openWorldTileSize; x++)
                    {
                        int idx = 351;
                        var which = Game1.random.NextDouble();
                        if (which < 0.025f)
                        {
                            idx = 304;
                        }
                        else if (which < 0.05f)
                        {
                            idx = 305;
                        }
                        else if (which < 0.15f)
                        {
                            idx = 300;
                        }
                        grassTiles[x, y] = new StaticTile(back, mainSheet, BlendMode.Alpha, idx);
                    }
                }

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
                AccessTools.FieldRefAccess<Map, Size>(__instance.Map, "m_displaySize") = new Size(100000, 100000);
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
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isTileOnMap), new Type[] { typeof(Vector2) })]
        public static class GameLocation_isTileOnMap_Patch1
        {
            public static bool Prefix(GameLocation __instance, Vector2 position, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.Name.Contains(locName))
                    return true;
                __result = position.X >= 0f && position.X < openWorldSize && position.Y >= 0f && position.Y < openWorldSize;
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
                __result = x >= 0f && x < openWorldSize && y >= 0f && y < openWorldSize;
                return false;
            }
        }
        [HarmonyPatch(typeof(Layer), nameof(Layer.Draw))]
        public static class Layer_Draw_Patch
        {
            public static bool Prefix(Layer __instance, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, int pixelZoom)
            {
                if (!Config.ModEnabled || !Game1.currentLocation.Name.Contains(locName))
                    return true;

                int tileWidth = pixelZoom * 16;
                int tileHeight = pixelZoom * 16;
                Location tileInternalOffset = new Location(Wrap(mapViewport.X, tileWidth), Wrap(mapViewport.Y, tileHeight));
                Location tileLocation = displayOffset - tileInternalOffset;
                int xMin = (tileInternalOffset.X <= 32 ? 1 : 0);
                int yMin = (tileInternalOffset.Y <= 32 ? 1 : 0);

                int xMax = mapViewport.Width / tileWidth + 2;
                int yMax = mapViewport.Height / tileHeight + 2;
                if (__instance.Id == "Back")
                {
                    Point loc = Game1.player.getTileLocationPoint();
                    if (playerTileLocation != loc)
                    {
                        Point delta = loc - playerTileLocation;
                        SetTiles(Game1.player.currentLocation, delta);
                        playerTileLocation = loc;
                    }
                    for (int y = yMin; y < yMax; y++)
                    {
                        tileLocation.X = displayOffset.X - tileInternalOffset.X;
                        for (int x = xMin; x < xMax; x++)
                        {

                            Tile tile = grassTiles[x, y];
                            displayDevice.DrawTile(tile, tileLocation, (float)(y * (16 * pixelZoom) + 16 * pixelZoom) / 10000f);
                            tileLocation.X += tileWidth;
                        }
                        tileLocation.Y += tileHeight;
                    }

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
    }
}