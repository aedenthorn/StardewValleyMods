using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace DynamicMapTiles
{
    public partial class ModEntry
    {
        private static Farmer explodingFarmer;
        
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.explode))]
        public class GameLocation_explode_Patch
        {

            public static void Prefix(Farmer who)
            {
                if (!Config.ModEnabled)
                    return;
                explodingFarmer = who;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.explosionAt))]
        public class GameLocation_explosionAt_Patch
        {
            public static void Postfix(GameLocation __instance, float x, float y)
            {
                if (!Config.ModEnabled || !__instance.isTileOnMap(new Vector2(x, y)) || explodingFarmer is null)
                    return;
                foreach(var layer in __instance.map.Layers)
                {
                    var tile = layer.Tiles[(int)x, (int)y];
                    if (tile is null)
                        continue;
                    if (tile.Properties.TryGetValue(explodeKey, out PropertyValue mail))
                    {
                        if (!string.IsNullOrEmpty(mail) && !explodingFarmer.mailReceived.Contains(mail))
                        {
                            explodingFarmer.mailReceived.Add(mail);
                        }
                        TriggerActions(new List<Layer>() { tile.Layer }, explodingFarmer, new Point((int)x, (int)y), new List<string>() { "Explode" });
                        layer.Tiles[(int)x, (int)y] = null;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character) })]
        public class GameLocation_isCollidingPosition_Patch
        {
            public static bool Prefix(GameLocation __instance, Rectangle position, ref bool __result)
            {
                if (!Config.ModEnabled || !pushingDict.TryGetValue(__instance.Name, out List<PushedTile> tiles))
                    return true;
                foreach (var tile in tiles)
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
                    Game1.mapDisplayDevice.DrawTile(tile.tile, new Location(tile.position.X - Game1.viewport.X, tile.position.Y - Game1.viewport.Y), (float)(tile.position.Y + 64 + (tile.tile.Layer.Id.Contains("Front") ? 16 : 0)) / 10000f);
                }
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performToolAction))]
        public class GameLocation_performToolAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Tool t, int tileX, int tileY, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.isTileOnMap(new Vector2(tileX, tileY)))
                    return true;

                if(TriggerActions(__instance.Map.Layers.ToList(), t.getLastFarmerToUse(), new Point(tileX, tileY), new List<string>() { t.GetType().Name, t.Name }))
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.isTileOnMap(new Vector2(tileLocation.X, tileLocation.Y)))
                    return true;
                if ((who.ActiveObject is not null && TriggerActions(__instance.Map.Layers.ToList(), who, new Point(tileLocation.X, tileLocation.Y), new List<string>() { "Object" + who.ActiveObject.Name, "Object" + who.ActiveObject.ParentSheetIndex })) || TriggerActions(__instance.Map.Layers.ToList(), who, new Point(tileLocation.X, tileLocation.Y), new List<string>() { "Action" }))
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.getMovementSpeed))]
        public class Farmer_getMovementSpeed_Patch
        {
            public static void Postfix(Farmer __instance, ref float __result)
            {
                if (!Config.ModEnabled)
                    return;
                var tileLoc = __instance.getTileLocation();
                if (__instance.currentLocation.isTileOnMap(tileLoc))
                {
                    var tile = __instance.currentLocation.Map.GetLayer("Back").Tiles[(int)tileLoc.X, (int)tileLoc.Y];
                    if (tile is not null && tile.Properties.TryGetValue(speedKey, out PropertyValue value) && float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float mult))
                    {
                        __result *= mult;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.MovePosition))]
        public class Farmer_MovePosition_Patch
        {
            public static void Prefix(Farmer __instance, ref Vector2[] __state)
            {
                if (!Config.ModEnabled)
                    return;
                var tileLoc = __instance.getTileLocation();
                if (__instance.currentLocation.isTileOnMap(tileLoc))
                {
                    var tile = __instance.currentLocation.Map.GetLayer("Back").Tiles[(int)tileLoc.X, (int)tileLoc.Y];
                    if(tile is not null && tile.Properties.TryGetValue(moveKey, out PropertyValue value))
                    {
                        var split = value.ToString().Split(' ');
                        __instance.xVelocity = float.Parse(split[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                        __instance.yVelocity = float.Parse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    }
                }

                __state = new Vector2[] { __instance.Position, tileLoc };
            }
            public static void Postfix(Farmer __instance, Vector2[] __state)
            {
                if (!Config.ModEnabled || __state is null)
                    return;
                var tilePos = __instance.getTileLocationPoint();
                var oldTile = Utility.Vector2ToPoint(__state[1]);
                if(oldTile != tilePos)
                {
                    DoStepOffActions(__instance, oldTile);
                    DoStepOnActions(__instance, tilePos);
                }

                if (__instance.currentLocation.isTileOnMap(tilePos.ToVector2()) && __instance.currentLocation.isTileOnMap(tilePos.ToVector2()))
                {
                    var backTile = __instance.currentLocation.Map.GetLayer("Back").Tiles[tilePos.X, tilePos.Y];
                    var backOldTile = __instance.currentLocation.Map.GetLayer("Back").Tiles[oldTile.X, oldTile.Y];
                    if (backTile != null && backTile.Properties.TryGetValue(slipperyKey, out PropertyValue value) && float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float amount))
                    {
                        if (__instance.movementDirections.Contains(0))
                            __instance.yVelocity += amount;
                        if (__instance.movementDirections.Contains(1))
                            __instance.xVelocity += amount;
                        if (__instance.movementDirections.Contains(2))
                            __instance.yVelocity -= amount;
                        if (__instance.movementDirections.Contains(3))
                            __instance.xVelocity -= amount;
                    }
                    else if (backOldTile != null && backOldTile.Properties.TryGetValue(slipperyKey, out value) && float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
                    {
                        __instance.xVelocity = 0;
                        __instance.yVelocity = 0;
                    }
                }

                if (__instance.movementDirections.Any() && __state[0] == __instance.Position)
                {
                    var startTile = new Point(__instance.GetBoundingBox().Center.X / 64, __instance.GetBoundingBox().Center.Y / 64);
                    startTile += GetNextTile(__instance.FacingDirection);
                    Point start = new Point(startTile.X * 64, startTile.Y * 64);
                    var startLoc = new Location(start.X, start.Y);

                    var build = __instance.currentLocation.Map.GetLayer("Buildings");
                    var tile = build.PickTile(startLoc, Game1.viewport.Size);

                    if (tile is not null && tile.Properties.TryGetValue(pushKey, out PropertyValue tiles))
                    {
                        var destTile = startTile + GetNextTile(__instance.FacingDirection);
                        foreach (var item in tiles.ToString().Split(','))
                        {
                            var split = item.Split(' ');
                            if (split.Length == 2 && int.TryParse(split[0], out int x) && int.TryParse(split[1], out int y) && destTile.X == x && destTile.Y == y)
                            {
                                PushTileWithOthers(__instance, tile, startTile);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}