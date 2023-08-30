using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using xTile.Dimensions;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace HedgeMaze
{
    public partial class ModEntry
    {
        public static Point[] surrounding = new Point[]
        {
            new Point(0,0),
            new Point(0,1),
            new Point(1,0),
            new Point(1,1),
            new Point(0,-1),
            new Point(-1,0),
            new Point(-1,-1),
            new Point(1,-1),
            new Point(-1,1),
            new Point(-1,-2),
            new Point(0,-2),
            new Point(1,-2)
        };
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.Update))]
        public class Farmer_Update_Patch
        {
            public static void Postfix(Farmer __instance)
            {
                if (!Config.ModEnabled || !mazeLocationDict.TryGetValue(__instance.currentLocation.NameOrUniqueName, out var list))
                    return;

                Point tile = __instance.getTileLocationPoint();
                var front = __instance.currentLocation.Map.GetLayer("Front");
                foreach(var inst in list)
                {
                    MazeData data = mazeDataDict[inst.id]; 
                    if (!IsTileInMaze(tile, data.mapSize, data.corner) || front is null)
                        continue;
                    Point farmerMazeTile = tile - data.corner;
                    foreach (var t in surrounding)
                    {
                        var thisTile = farmerMazeTile + t;
                        if (!IsTileOnMaze(thisTile, data.mapSize))
                            continue;
                        if (t.Y == 1 && !inst.tiles[farmerMazeTile.X, farmerMazeTile.Y + 1])
                            continue;
                        if (front.Tiles.Array[(t.X + tile.X), (t.Y + tile.Y)] is StaticTile && front.Tiles.Array[(t.X + tile.X), (t.Y + tile.Y)].TileIndex == 946)
                        {
                            Point mazeTile = farmerMazeTile + t;
                            Point belowTile = mazeTile + new Point(0, 1);

                            Tile newTile = null;
                            if (belowTile.Y < inst.tiles.GetLength(1) && !inst.tiles[belowTile.X, belowTile.Y])
                            {
                                try
                                {
                                    bool left = belowTile.X > 0 && !inst.tiles[belowTile.X - 1, belowTile.Y];
                                    bool right = belowTile.X < data.mapSize.X - 2 && !inst.tiles[belowTile.X + 1, belowTile.Y];
                                    bool up = belowTile.Y > 0 && !inst.tiles[belowTile.X, belowTile.Y - 1];
                                    bool down = belowTile.Y < data.mapSize.Y - 2 && !inst.tiles[belowTile.X, belowTile.Y + 1];
                                    int tileIndex = GetWallTiles(left, right, up, down)[1];
                                    if (tileIndex > -1)
                                        newTile = new StaticTile(front, __instance.currentLocation.Map.GetTileSheet("Custom_HedgeMaze_Fest"), BlendMode.Alpha, tileIndex);
                                }
                                catch { }
                            }
                            __instance.currentLocation.Map.GetLayer("Front").Tiles.Array[(t.X + tile.X), (t.Y + tile.Y)] = newTile;
                        }
                    }
                }


            }
        }
        private static int fairyFrame;
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.draw), new Type[] { typeof(SpriteBatch) })]
        public class GameLocation_draw_Patch
        {
            public static void Postfix(GameLocation __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !mazeLocationDict.TryGetValue(__instance.NameOrUniqueName, out var list))
                    return;
                fairyFrame++;
                fairyFrame %= 32;
                foreach(var inst in list)
                {
                    var front = __instance.map.GetLayer("Front");
                    foreach (var f in inst.fairyTiles)
                    {
                        var tile = front.PickTile(new Location((int)f.X * 64, (int)f.Y * 64), Game1.viewport.Size);
                        if (tile?.TileIndex == 946)
                            continue;
                        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, f * 64), new Rectangle?(new Rectangle(16 + fairyFrame / 8 * 16, 592, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9999999f);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !mazeLocationDict.TryGetValue(__instance.NameOrUniqueName, out var list))
                    return true;
                foreach (var inst in list)
                {
                    var tv = new Vector2(tileLocation.X, tileLocation.Y);
                    foreach (var f in inst.fairyTiles)
                    {
                        if (f == tv)
                        {
                            __instance.localSound("yoba");
                            who.health = who.maxHealth;
                            who.stamina = who.MaxStamina;
                            inst.fairyTiles.Remove(f);
                            __result = true;
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.checkAction))]
        public class NPC_checkAction_Patch
        {
            public static bool Prefix(NPC __instance, Farmer who, GameLocation l, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.Name.Equals("Dwarf") || l.Name.Equals("Mine") || !who.canUnderstandDwarves)
                    return true;
                Game1.activeClickableMenu = new ShopMenu(Utility.getDwarfShopStock(), 0, "Dwarf", null, null, null);
                __result = true;
                return false;
            }
        }
    }
}