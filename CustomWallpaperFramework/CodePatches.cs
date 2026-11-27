using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using xTile.Layers;
using Object = StardewValley.Object;

namespace CustomWallpaperFramework
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(DecoratableLocation), nameof(DecoratableLocation.UpdateWallpaper))]
        public class UpdateWallpaper_Patch
        {
            public static void Postfix(DecoratableLocation __instance, string wallpaperId)
            {
                if (!Config.EnableMod || !__instance.appliedWallpaper.ContainsKey(wallpaperId) || !__instance.wallpaperTiles.ContainsKey(wallpaperId))
                    return;

                if (locationDataDict.TryGetValue(__instance, out Dictionary<string, WallPaperTileData> tileDataDict))
                {
                    locationDataDict[__instance].Remove(wallpaperId);
                }

                if(!WallpaperDataDict.TryGetValue(__instance.appliedWallpaper[wallpaperId].Split(':')[0], out WallpaperData data) || data.isFloor)
                    return;

                string wallpaperKey = __instance.appliedWallpaper[wallpaperId].Split(':')[0];

                //SMonitor.Log($"Updatating custom-sized wallpaper {wallpaperKey} in area {wallpaper_id}");

                if (!locationDataDict.ContainsKey(__instance))
                    locationDataDict.Add(__instance, new Dictionary<string, WallPaperTileData>());
                if (!locationDataDict[__instance].ContainsKey(wallpaperId))
                {
                    locationDataDict[__instance].Add(wallpaperId, new WallPaperTileData()
                    {
                        id = data.id,
                    });
                }

                foreach (Vector3 vector in __instance.wallpaperTiles[wallpaperId])
                {
                    if (locationDataDict[__instance][wallpaperId].startTile.X < 0)
                    {
                        Vector2 startTile = new Vector2(vector.X, vector.Y);
                        while (__instance.wallpaperTiles[wallpaperId].Exists(v => v.X == startTile.X - 1))
                        {
                            startTile = new Vector2(startTile.X - 1, startTile.Y);
                        }
                        while (__instance.wallpaperTiles[wallpaperId].Exists(v => v.Y == startTile.Y - 1))
                        {
                            startTile = new Vector2(startTile.X, startTile.Y - 1);
                        }
                        locationDataDict[__instance][wallpaperId].startTile = startTile;
                    }
                    if (vector.Z == 2)
                    {
                        if ((bool)AccessTools.Method(typeof(DecoratableLocation), "IsFloorableOrWallpaperableTile", new Type[] { typeof(int), typeof(int), typeof(string) }).Invoke(__instance, new object[] { (int)vector.X, (int)vector.Y, "Buildings" }))
                        {
                            locationDataDict[__instance][wallpaperId].buildingTiles.Add(new Vector2(vector.X, vector.Y));
                            continue;
                        }
                    }
                    if ((bool)AccessTools.Method(typeof(DecoratableLocation), "IsFloorableOrWallpaperableTile", new Type[] { typeof(int), typeof(int), typeof(string) }).Invoke(__instance, new object[] { (int)vector.X, (int)vector.Y, "Back" }))
                        locationDataDict[__instance][wallpaperId].backTiles.Add(new Vector2(vector.X, vector.Y));
                }
            }

        }
        [HarmonyPatch(typeof(Layer), "DrawNormal")]
        public class GameLocation_draw_Patch
        {
            public static void Postfix(Layer __instance)
            {
                if (!Config.EnableMod || (__instance.Id != "Buildings" && __instance.Id != "Back") || !Game1.currentLocation.GetType().IsAssignableTo(typeof(DecoratableLocation)) || !locationDataDict.TryGetValue(Game1.currentLocation as DecoratableLocation, out Dictionary<string, WallPaperTileData> dict))
                    return;
                int pixelZoom = 1;
                foreach(var kvp in dict)
                {
                    var data = WallpaperDataDict[kvp.Value.id];
                    Texture2D tex = SHelper.GameContent.Load<Texture2D>(data.texturePath);

                    int tileSize = (int)Math.Floor(64 / data.scale);
                    var tiles = __instance.Id == "Buildings" ? kvp.Value.buildingTiles : kvp.Value.backTiles;
                    foreach (var v in tiles)
                    {
                        int x = ((int)v.X - (int)kvp.Value.startTile.X) % data.width;
                        int y = ((int)v.Y - (int)kvp.Value.startTile.Y) % data.height;
                        Game1.spriteBatch.Draw(tex, Game1.GlobalToLocal(v * 64), new Rectangle(new Point(x * tileSize, y * tileSize), new Point(tileSize, tileSize)), Color.White, 0, Vector2.Zero, data.scale, SpriteEffects.None, ((v.Y * 16 * pixelZoom) + 16 * pixelZoom) / 10000f);
                    }
                }
            }
        }
    }
}