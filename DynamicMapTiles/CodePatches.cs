using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using xTile.ObjectModel;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace DynamicMapTiles
{
    public partial class ModEntry
    {
       [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.explosionAt))]
        public class GameLocation_explosionAt_Patch
        {
            public static void Postfix(GameLocation __instance, float x, float y)
            {
                if (!Config.ModEnabled)
                    return;
                try
                {
                    var tile = __instance.Map.GetLayer("Buildings").Tiles[(int)x, (int)y];
                    if (tile is not null && tile.Properties.ContainsKey(explodableKey))
                    {
                        __instance.Map.GetLayer("Buildings").Tiles[(int)x, (int)y] = null;
                    }
                }
                catch { }
                
            }
        }
       [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character) })]
        public class GameLocation_isCollidingPosition_Patch
        {
            public static bool Prefix(GameLocation __instance, Rectangle position, ref bool __result)
            {
                if (!Config.ModEnabled || !pushingDict.TryGetValue(__instance.Name, out List<PushedTile> tiles))
                    return true;
                foreach(var tile in tiles)
                {
                    Rectangle tileRect = new Rectangle(tile.position, new Point(64, 64));
                    if (position.Intersects(tileRect))
                    {
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }
       [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.draw))]
        public class GameLocation_draw_Patch
        {
            public static void Postfix(GameLocation __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !pushingDict.TryGetValue(__instance.Name, out List<PushedTile> tiles))
                    return;
                foreach(var tile in tiles)
                {
                    Game1.mapDisplayDevice.DrawTile(tile.tile, new Location(tile.position.X - Game1.viewport.X, tile.position.Y - Game1.viewport.Y), (float)(tile.position.Y + 64) / 10000f);
                }
            }
        }
       [HarmonyPatch(typeof(Farmer), nameof(Farmer.MovePosition))]
        public class Farmer_MovePosition_Patch
        {
            public static void Prefix(Farmer __instance, ref Vector2[] __state)
            {
                if (!Config.ModEnabled || double.IsNaN((double)__instance.xVelocity) || double.IsNaN((double)__instance.yVelocity))
                    return;
                __state = new Vector2[] { __instance.Position, new Vector2(__instance.xVelocity, __instance.yVelocity), __instance.getTileLocation() };
            }
            public static void Postfix(Farmer __instance, Vector2[] __state)
            {
                if (!Config.ModEnabled || __state is null)
                    return;
                var tilePos = __instance.getTileLocationPoint();
                var oldTile = Utility.Vector2ToPoint(__state[2]);
                if(oldTile != tilePos)
                {
                    var back = __instance.currentLocation.Map.GetLayer("Back");
                    if (__instance.currentLocation.isTileOnMap(tilePos.ToVector2()))
                    {
                        var tile = back.Tiles[tilePos.X, tilePos.Y];
                        if (tile is not null && tile.Properties.TryGetValue(stepKey, out PropertyValue value) && int.TryParse(value, out int index))
                        {
                            tile.Properties[stepKey] = tile.TileIndex.ToString();
                            back.Tiles[tilePos.X, tilePos.Y].TileIndex = index;
                        }
                    }
                    if (__instance.currentLocation.isTileOnMap(oldTile.ToVector2()))
                    {
                        var tile = back.Tiles[oldTile.X, oldTile.Y];
                        if (tile is not null && tile.Properties.TryGetValue(stepKey, out PropertyValue value) && int.TryParse(value, out int index))
                        {
                            tile.Properties[stepKey] = tile.TileIndex.ToString();
                            back.Tiles[oldTile.X, oldTile.Y].TileIndex = index;
                        }
                    }
                }
                if (__state[0] == __instance.Position && __instance.movementDirections.Any())
                {
                    var startTile = new Point(__instance.GetBoundingBox().Center.X / 64, __instance.GetBoundingBox().Center.Y / 64);
                    Point destTile = startTile;
                    switch (__instance.FacingDirection)
                    {
                        case 0:
                            startTile += new Point(0, -1);
                            destTile += new Point(0, -2);
                            break;
                        case 1:
                            startTile += new Point(1, 0);
                            destTile += new Point(2, 0);
                            break;
                        case 2:
                            startTile += new Point(0, 1);
                            destTile += new Point(0, 2);
                            break;
                        case 3:
                            startTile += new Point(-1, 0);
                            destTile += new Point(-2, 0);
                            break;
                    }
                    Point start = new Point(startTile.X * 64, startTile.Y * 64);
                    Point dest = new Point(destTile.X * 64, destTile.Y * 64);
                    var startLoc = new Location(start.X, start.Y);
                    

                    var build = __instance.currentLocation.Map.GetLayer("Buildings");
                    var tile = build.PickTile(startLoc, Game1.viewport.Size);

                    if (tile is not null && tile.Properties.TryGetValue(pushKey, out PropertyValue dirs) && dirs.ToString().Split(',').ToList().Contains(__instance.FacingDirection+""))
                    {
                        if(!pushingDict.TryGetValue(__instance.currentLocation.Name, out List<PushedTile> tiles))
                        {
                            pushingDict.Add(__instance.currentLocation.Name, tiles = new List<PushedTile>());
                        }


                        tiles.Add(new PushedTile() { tile = tile, destination = dest, position = start, dir = __instance.FacingDirection });
                        __instance.currentLocation.Map.GetLayer("Buildings").Tiles[new Location(startTile.X, startTile.Y)] = null;
                    }
                }
            }
        }
    }
}