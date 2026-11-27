using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace Survivors
{
    public partial class ModEntry
    {

        private static bool IsChunkInMap(int cx, int cy)
        {
            return (cx >= 0 && cy >= 0 && cx < Config.OpenWorldSize / openWorldChunkSize && cy < Config.OpenWorldSize / openWorldChunkSize);
        }
        private static bool IsChunkInMap(Point cp)
        {
            return (cp.X >= 0 && cp.Y >= 0 && cp.X < Config.OpenWorldSize / openWorldChunkSize && cp.Y < Config.OpenWorldSize / openWorldChunkSize);
        }
        private static bool IsVectorInMap(Vector2 v)
        {
            return (v.X >= 0 && v.Y >= 0 && v.X < Config.OpenWorldSize && v.Y < Config.OpenWorldSize);
        }
        private static int ChunkDisplayOffset(int value)
        {
            return (Game1.viewport.Y % (openWorldChunkSize * 64) < value / (openWorldChunkSize * 64) ? openWorldChunkSize * 64 : 0);
        }
        private static float ChunkDisplayOffset(float value)
        {
            return (Game1.viewport.Y % (openWorldChunkSize * 64) < (int)value / (openWorldChunkSize * 64) ? openWorldChunkSize * 64 : 0);
        }
        private static float ChunkDisplayOffsetTile(float value)
        {
            return (Game1.viewport.Y % (openWorldChunkSize * 64) < (int)value / openWorldChunkSize ? openWorldChunkSize : 0);
        }

        public static bool CodesCompare(List<CodeInstruction> codes, int i, OpCode[] opCodes)
        {
            for (int j = 0; j < opCodes.Length; j++)
            {
                if (codes.Count <= i + j)
                    return false;
                if (codes[i + j].opcode != opCodes[j])
                    return false;
            }
            return true;
        }
        private static bool ConflictsRect(Point cp, Rectangle r)
        {
            foreach(var p in GetSurroundingPointArrayAbs(cp, true))
            {
                if (ConflictsRect(p, r, landmarkRects))
                    return true;
                if (ConflictsRect(p, r, lakeRects))
                    return true;
                if (ConflictsRect(p, r, outcropRects))
                    return true;
            }
            return false;
        }
        public static void AddRectToList(Rectangle rect, Dictionary<Point, HashSet<Rectangle>> dict)
        {
            for (int x = 0; x < rect.Width + openWorldChunkSize; x += openWorldChunkSize)
            {
                for (int y = 0; y < rect.Height + openWorldChunkSize; y += openWorldChunkSize)
                {
                    var point = new Point(rect.Left + x, rect.Top + y);
                    Point cp = GetTileChunk(point);
                    if (!dict.TryGetValue(cp, out var list))
                    {
                        list = new HashSet<Rectangle>();
                        dict[cp] = list;
                    }
                    list.Add(rect);
                }
            }
        }

        public static Point GetTileChunk(Point point)
        {
            return new Point(point.X / openWorldChunkSize, point.Y / openWorldChunkSize);
        }
        private static bool ConflictsRect(Point cp, Rectangle ar, Dictionary<Point, HashSet<Rectangle>> dict)
        {
            if (dict.TryGetValue(cp, out var rhs))
            {
                foreach (var rect in rhs)
                {
                    if (rect.Intersects(ar))
                        return true;
                }
            }
            return false;
        }
        public static bool IsOpenTile(Vector2 av)
        {
            if (!IsVectorInMap(av))
                return false;
            var cp = new Point((int)av.X / openWorldChunkSize, (int)av.Y / openWorldChunkSize);
            if (!cachedChunks.TryGetValue(cp, out var chunk))
            {
                chunk = CacheChunk(cp, false);
            }

            if (chunk.tiles["Buildings"][(int)av.X % openWorldChunkSize, (int)av.Y % openWorldChunkSize] != null)
                return false;

            var back = chunk.tiles["Back"][(int)av.X % openWorldChunkSize, (int)av.Y % openWorldChunkSize];
            if (back != null && back.Properties.ContainsKey("Water"))
                return false;
            var pos = av * 64.5f;
            foreach(var ltf in openWorldLocation.largeTerrainFeatures)
            {
                if (ltf.getBoundingBox().Contains(pos.ToPoint()))
                {
                    return false;
                }
            }
            foreach(var rc in openWorldLocation.resourceClumps)
            {
                if (rc.getBoundingBox().Contains(pos.ToPoint()))
                {
                    return false;
                }
            }
            return !openWorldLocation.terrainFeatures.ContainsKey(av) && !openWorldLocation.Objects.ContainsKey(av) && !openWorldLocation.overlayObjects.ContainsKey(av);
        }

        public static Color MakeTint(double fraction)
        {
            return tintColors[(int)Math.Floor(fraction * tintColors.Length)];
        }

        public static Vector2 ToGlobalTile(Point p, Vector2 tile)
        {
            return tile + new Vector2(p.X * openWorldChunkSize, p.Y * openWorldChunkSize);
        }
        public static Point ToGlobalTile(Point p, Point tile)
        {
            return tile + new Point(p.X * openWorldChunkSize, p.Y * openWorldChunkSize);
        }

        private static Rectangle ToGlobalRect(Point cp, Rectangle r)
        {
            return new Rectangle(ToGlobalTile(cp, r.Location), r.Size);
        }
        private static Point GetPlayerChunk(Farmer f)
        {
            return new Point(f.TilePoint.X / openWorldChunkSize, f.TilePoint.Y / openWorldChunkSize);
        }
        public static Tile GetTile(Layer layer, int x, int y)
        {
            return null;
        }

        public static void SetTile(Layer layer, int x, int y, Tile value)
        {
        }
        public static Point[] GetSurroundingPointArrayAbs(Point center, bool include)
        {
            if (include)
            {
                return new Point[]
                {
                    center,
                    new Point(-1, 0) + center,
                    new Point(1, 0) + center,
                    new Point(0, 1) + center,
                    new Point(0, -1) + center,
                    new Point(-1, -1) + center,
                    new Point(1, -1) + center,
                    new Point(1, 1) + center,
                    new Point(-1, 1) + center
                };
            }
            return new Point[]
            {
                new Point(-1, 0) + center,
                new Point(1, 0) + center,
                new Point(0, 1) + center,
                new Point(0, -1) + center,
                new Point(-1, -1) + center,
                new Point(1, -1) + center,
                new Point(1, 1) + center,
                new Point(-1, 1) + center
            };
        }
        public static Point[] GetSurroundingPointArrayRel(bool include)
        {
            if (include)
            {
                return new Point[]
                {
                    new Point(0, 0),
                    new Point(-1, 0),
                    new Point(1, 0),
                    new Point(0, 1),
                    new Point(0, -1),
                    new Point(-1, -1),
                    new Point(1, -1),
                    new Point(1, 1),
                    new Point(-1, 1)
                };
            }
            return new Point[]
            {
                new Point(-1, 0),
                new Point(1, 0),
                new Point(0, 1),
                new Point(0, -1),
                new Point(-1, -1),
                new Point(1, -1),
                new Point(1, 1),
                new Point(-1, 1)
            };
        }
        public static Point GetAbsolutePosition(Point cp, int x, int y)
        {
            return new Point(cp.X * openWorldChunkSize + x, cp.Y * openWorldChunkSize + y);
        }
        public static Point GetLocalTile(Point cp, int x, int y, out Point ocp)
        {
            var point = new Point(x - cp.X * openWorldChunkSize,  y - cp.Y * openWorldChunkSize);
            var offset = GetPointOffset(point);
            ocp = cp + offset;
            point -= new Point(offset.X * openWorldChunkSize, offset.Y * openWorldChunkSize);
            return point;
        }
        public static Point GetLocalTile(Point p)
        {
            return new Point(p.X % openWorldChunkSize,  p.Y % openWorldChunkSize);
        }
        public static Point GetLocalTile(int x, int y)
        {
            return new Point(x % openWorldChunkSize,  y % openWorldChunkSize);
        }
        private static Point GetGlobalTile(Point cp, int rx, int ry)
        {
            return new Point(rx + openWorldChunkSize * cp.X, ry + openWorldChunkSize * cp.Y);
        }
        private static Point GetGlobalTile(Point cp, Point p)
        {
            return new Point(p.X + openWorldChunkSize * cp.X, p.Y + openWorldChunkSize * cp.Y);
        }
        private static Point GetPointOffset(Point p)
        {

            Point offset = new();
            if (p.X < 0)
            {
                offset += new Point(-1, 0);
            }
            else if (p.X >= openWorldChunkSize)
            {
                offset += new Point(1, 0);
            }
            if (p.Y < 0)
            {
                offset += new Point(0, -1);
            }
            else if (p.Y >= openWorldChunkSize)
            {
                offset += new Point(0, 1);
            }
            return offset;
        }
        private static bool IsInsideRect(Point cp, Vector2 av, Dictionary<Point, HashSet<Rectangle>> dict)
        {
            foreach (var p in GetSurroundingPointArrayAbs(cp, true))
            {
                if (dict.TryGetValue(p, out var rhs))
                {
                    foreach (var rect in rhs)
                    {
                        if (rect.Contains(av.ToPoint()))
                            return true;
                    }
                }
            }
            return false;
        }
        private static List<Point> GetBlob(Point c, Random r, int maxTiles)
        {

            int maxSize = (int)Math.Round(Math.Sqrt(maxTiles));
            int maxVariation = (int)Math.Round(maxSize / 2f);
            int height = maxSize - r.Next(maxVariation);
            int width = maxSize - r.Next(maxVariation);

            List<Point> tiles = MakeBlob(c, r, height, width, maxSize);
            return tiles;

        }

        private static List<Point> MakeBlob(Point c, Random r, int height, int width, int maxSize)
        {
            List<Point> tiles = new List<Point>();

            int leftTop = (maxSize - height) / 2;
            int rightTop = leftTop;
            int leftBot = maxSize - leftTop;
            int rightBot = leftBot;
            for (int x = width / 2; x >= 0; x--)
            {

                double chance = (width / 2 - x) / ((width + 1) / 2.0);
                while (leftTop < height / 2)
                {
                    if (r.NextDouble() < chance)
                    {
                        leftTop++;
                    }
                    else
                    {
                        break;
                    }
                }
                while (leftBot > height / 2)
                {
                    if (r.NextDouble() < chance)
                    {
                        leftBot--;
                    }
                    else
                    {
                        break;
                    }
                }
                for (int y = leftTop; y < leftBot; y++)
                {
                    tiles.Add(c + new Point(x - width / 2, y - height / 2));
                }
            }
            for (int x = width / 2 + 1; x < width; x++)
            {
                double chance = (x - width / 2) / ((width + 1) / 2.0);
                while (rightTop < height / 2)
                {
                    if (r.NextDouble() < chance)
                    {
                        rightTop++;
                    }
                    else
                    {
                        break;
                    }
                }
                while (rightBot > height / 2)
                {
                    if (r.NextDouble() < chance)
                    {
                        rightBot--;
                    }
                    else
                    {
                        break;
                    }
                }
                if (rightBot <= rightTop)
                {
                    break;
                }
                for (int y = rightTop; y < rightBot; y++)
                {
                    tiles.Add(c + new Point(x - width / 2, y - height / 2));
                }

            }
            return tiles;
        }

        private static List<Point> MakeBlobFromTiles(Random rand, List<Point> tiles)
        {
            int l = int.MaxValue;
            int t = int.MaxValue;
            int r = -1;
            int b = -1;
            foreach(var tile in tiles)
            {
                if(tile.X < l)
                {
                    l = tile.X;
                }
                if(tile.X > r)
                {
                    r = tile.X;
                }
                if (tile.Y < t)
                {
                    t = tile.Y;
                }
                if (tile.Y > b)
                {
                    b = tile.Y;
                }
            }
            var c = new Point(l + (r - l) / 2, t + (b - t) / 2);
            return MakeBlob(c, rand, b - t + 2, r - l + 2, r - l + 2);
        }
        private static List<Point> GetBlobPadding(List<Point> tiles, int amount, bool include)
        {
            List<Point> temp = new();
            temp.AddRange(tiles);
            List<Point> newTiles = new();
            for(int i = 0; i < amount; i++)
            {
                foreach (var t in temp)
                {
                    for(int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            var np = t + new Point(x, y);
                            if (!temp.Contains(np) && !newTiles.Contains(np))
                            {
                                newTiles.Add(np);
                            }
                        }
                    }
                    temp = new();
                    temp.AddRange(tiles);
                    temp.AddRange(newTiles);
                }
            }
            if (include)
                newTiles.AddRange(tiles);
            return newTiles;
        }
        private static void ChangeMapProperties(string propertyName, string toAdd)
        {
            try
            {
                PropertyValue oldValue = null;
                if (!openWorldLocation.map.Properties.TryGetValue(propertyName, out oldValue))
                {
                    openWorldLocation.map.Properties[propertyName] = new PropertyValue(toAdd);
                }
                else if (!((string)oldValue).Contains(toAdd))
                {
                    string newValue = oldValue + " " + toAdd;
                    openWorldLocation.map.Properties[propertyName] = new PropertyValue(newValue);
                }
            }
            catch
            {
            }
        }
    }
}