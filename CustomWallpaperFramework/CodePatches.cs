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
            public static void Postfix(DecoratableLocation __instance, string wallpaper_id)
            {
                if (__instance.appliedWallpaper.ContainsKey(wallpaper_id))
                {
                    SMonitor.Log($"pattern id: {__instance.appliedWallpaper[wallpaper_id]}");
                }
                if (!Config.EnableMod || !__instance.appliedWallpaper.ContainsKey(wallpaper_id) || !__instance.wallpaperTiles.ContainsKey(wallpaper_id))
                    return;

                if (locationDataDict.TryGetValue(__instance, out Dictionary<string, WallPaperTileData> tileDataDict))
                {
                    locationDataDict[__instance].Remove(wallpaper_id);
                }

                if(!wallpaperDataDict.TryGetValue(__instance.appliedWallpaper[wallpaper_id].Split(':')[0], out WallpaperData data) || data.isFloor)
                    return;

                string wallpaperKey = __instance.appliedWallpaper[wallpaper_id].Split(':')[0];

                SMonitor.Log($"Updatating custom-sized wallpaper {wallpaperKey} in area {wallpaper_id}");

                if (!locationDataDict.ContainsKey(__instance))
                    locationDataDict.Add(__instance, new Dictionary<string, WallPaperTileData>());
                if (!locationDataDict[__instance].ContainsKey(wallpaper_id))
                {
                    locationDataDict[__instance].Add(wallpaper_id, new WallPaperTileData()
                    {
                        id = data.id,
                    });
                }

                foreach (Vector3 vector in __instance.wallpaperTiles[wallpaper_id])
                {
                    if(locationDataDict[__instance][wallpaper_id].startTile.X < 0)
                    {
                        Vector2 startTile = new Vector2(vector.X, vector.Y);
                        while (__instance.wallpaperTiles[wallpaper_id].Exists(v => v.X == startTile.X - 1))
                        {
                            startTile = new Vector2(startTile.X - 1, startTile.Y);
                        }
                        while (__instance.wallpaperTiles[wallpaper_id].Exists(v => v.Y == startTile.Y - 1))
                        {
                            startTile = new Vector2(startTile.X, startTile.Y - 1);
                        }
                        locationDataDict[__instance][wallpaper_id].startTile = startTile;
                    }
                    if (vector.Z == 2)
                    {
                        if ((bool)AccessTools.Method(typeof(DecoratableLocation), "IsFloorableOrWallpaperableTile").Invoke(__instance, new object[] { (int)vector.X, (int)vector.Y, "Buildings" }))
                        {
                            locationDataDict[__instance][wallpaper_id].buildingTiles.Add(new Vector2(vector.X, vector.Y));
                            continue;
                        }
                    }
                    if((bool)AccessTools.Method(typeof(DecoratableLocation), "IsFloorableOrWallpaperableTile").Invoke(__instance, new object[] { (int)vector.X, (int)vector.Y, "Back" }))
                        locationDataDict[__instance][wallpaper_id].backTiles.Add(new Vector2(vector.X, vector.Y));
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
                    var data = wallpaperDataDict[kvp.Value.id];
                    Texture2D tex = data.texture;
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