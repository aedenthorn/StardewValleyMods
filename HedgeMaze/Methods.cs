using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace HedgeMaze
{
    public partial class ModEntry
    {
        public static int mapSize = 57;
        public static Point[] neighbours = new Point[]
        {
            new Point(0, -2),
            new Point(2, 0),
            new Point(0, 2),
            new Point(-2, 0)
        };

        private void ModifyMap(IAssetData obj)
        {
            var map = obj.AsMap().Data;
            var sheet = new TileSheet("Fest", map, "Maps/Festivals", new Size(512 / 16, 512 / 16), new Size(16, 16));
            map.AddTileSheet(sheet);
            var tiles = MakeMapArray();
            var back = map.GetLayer("Back");
            var buildings = map.GetLayer("Buildings");
            var front = map.GetLayer("Front");
            foreach(var s in map.TileSheets)
            {
                Monitor.Log(s.Id);
            }
            TileSheet mainSheet = map.GetTileSheet("untitled tile sheet");
            for(int y = 0; y < mapSize; y++)
            {
                for (int x = 0; x < mapSize; x++)
                {
                    var tx = x + 1;
                    var ty = y + 33;
                    if (!tiles[x, y])
                    {
                        bool left = x > 0 && !tiles[x - 1, y];
                        bool right = x < mapSize - 2 && !tiles[x + 1, y];
                        bool up = y > 0 && !tiles[x, y - 1];
                        bool down = y < mapSize - 2 && !tiles[x, y + 1];
                        if (left)
                        {
                            if (right)
                            {
                                buildings.Tiles[tx, ty] = new StaticTile(buildings, sheet, BlendMode.Alpha, 660);
                                if (up)
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 563);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 626);
                                    }
                                }
                                else
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 563);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 628);
                                    }
                                }
                            }
                            else
                            {
                                buildings.Tiles[tx, ty] = new StaticTile(buildings, sheet, BlendMode.Alpha, 661);
                                if (up)
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 565);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 629);
                                    }
                                }
                                else
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 565);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 629);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (right)
                            {
                                buildings.Tiles[tx, ty] = new StaticTile(buildings, sheet, BlendMode.Alpha, 659);
                                if (up)
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 563);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 627);
                                    }
                                }
                                else
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 563);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 627);
                                    }
                                }
                            }
                            else
                            {
                                buildings.Tiles[tx, ty] = new StaticTile(buildings, sheet, BlendMode.Alpha, 661);
                                if (up)
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 597);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 629);
                                    }
                                }
                                else
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 565);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, sheet, BlendMode.Alpha, 665);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var which = Game1.random.NextDouble();
                        if(which < 0.1f)
                        {
                            back.Tiles[tx, ty] = new StaticTile(back, mainSheet, BlendMode.Alpha, 305);

                        }
                        else if(which < 0.25f)
                        {
                            back.Tiles[tx, ty] = new StaticTile(back, mainSheet, BlendMode.Alpha, 300);
                        }
                        else
                        {
                            back.Tiles[tx, ty] = new StaticTile(back, mainSheet, BlendMode.Alpha, 351);
                        }
                    }
                }
            }
        }


        public static bool[,] MakeMapArray()
        {
            bool[,] map = new bool[mapSize, mapSize];

            int start = Game1.random.Next(mapSize / 2) * 2 + 1; // starting x
            map[start, 0] = true; // knock down entrance
            List<Point> checkedTiles = new();
            List<Point> checkingTiles = new();
            CheckTile(ref map, checkedTiles, checkingTiles, new Point(start, 1)); // start at tile below entrance
            if (Config.Debug)
            {
                List<string> output = new List<string>();
                for (int y = 0; y < mapSize; y++)
                {
                    string line = "";
                    for (int x = 0; x < mapSize; x++)
                    {
                        line += map[x, y] ? " " : "#";
                    }
                    output.Add(line);
                }
                File.WriteAllLines(Path.Combine(SHelper.DirectoryPath, "map.txt"), output.ToArray());
            }
            return map;
        }

        private static void CheckTile(ref bool[,] map, List<Point> checkedTiles, List<Point> checkingTiles, Point tile)
        {
            map[tile.X, tile.Y] = true;
            var list = neighbours.ToList();
            ShuffleList(list);
            for(var i = 0; i < 4; i++)
            {
                Point n = tile + list[i];
                if (IsInMap(n) && !checkedTiles.Contains(n) && !checkingTiles.Contains(n))
                {
                    var x = tile.X + list[i].X / 2;
                    var y = tile.Y + list[i].Y / 2;

                    map[x, y] = true; // knock down wall;

                    checkingTiles.Add(tile);
                    CheckTile(ref map, checkedTiles, checkingTiles, n);
                    return;
                }
            }
            checkedTiles.Add(tile);
            if(checkingTiles.Any())
            {
                tile = checkingTiles[checkingTiles.Count - 1];
                checkingTiles.RemoveAt(checkingTiles.Count - 1);
                CheckTile(ref map, checkedTiles, checkingTiles, tile);
            }
        }

        private static bool IsInMap(Point n)
        {
            return n.X > 0 && n.X < 56 && n.Y > 0 && n.Y < 56;
        }

        public static void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Game1.random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}