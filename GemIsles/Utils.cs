using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace GemIsles
{
    internal class Utils
    {
        private static IModHelper Helper;
        private static IMonitor Monitor;
        private static ModConfig Config;
        public static int mapWidth = 128;
        public static int mapHeight = 72;
        internal static void Initialize(ModConfig config, IMonitor monitor, IModHelper helper)
        {
            Helper = helper;
            Monitor = monitor;
            Config = config;
        }
        internal static Map CreateIslesMap(GameLocation l)
        {
            Map map = Helper.Content.Load<Map>("assets/isles.tbin");
            Layer back = map.GetLayer("Back");
            TileSheet sheet = map.TileSheets[0];

            int isles = Game1.random.Next(1, Math.Max(1, Config.MaxIsles) + 1);
            Monitor.Log($"making {isles} isles");
            List<Point> points = new List<Point>();
            Rectangle bounds = new Rectangle(mapWidth / isles / 4, mapHeight / isles / 4, mapWidth - mapWidth / isles / 2, mapHeight - mapHeight / isles / 2);
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    Point p = new Point(x, y);
                    if (bounds.Contains(p))
                        points.Add(p);
                    back.Tiles[x, y] = new StaticTile(back, sheet, BlendMode.Alpha, GetRandomOceanTile());
                }
            }
            Monitor.Log($"got {points.Count} points");

            List<Rectangle> isleBoxes = new List<Rectangle>();
            for (int i = 0; i < isles; i++)
            {
                List<Point> freePoints = new List<Point>();
                int width = Game1.random.Next(mapWidth / isles / 2, mapWidth / isles);
                int height = Game1.random.Next(mapHeight / isles / 2, mapHeight / isles);

                Monitor.Log($"trying to make island of size {width}x{height}");
                Rectangle bounds2 = new Rectangle(width / 2, height / 2, mapWidth - width, mapHeight - height);
                for (int j = points.Count - 1; j >= 0; j--)
                {
                    if (bounds2.Contains(points[j]))
                    {
                        Rectangle testRect = new Rectangle(points[j].X - width /2, points[j].Y - height /2, width, height);
                        bool free = true;
                        foreach(Rectangle r in isleBoxes)
                        {
                            if (r.Intersects(testRect))
                            {
                                free = false;
                                break;
                            }
                        }
                        if (free)
                        {
                            freePoints.Add(points[j]);
                        }
                    }
                }
                if (!freePoints.Any())
                    continue;
                Point randPoint = freePoints[Game1.random.Next(freePoints.Count)];
                Rectangle isleBox = new Rectangle(randPoint.X - width / 2, randPoint.Y - height / 2, width, height);
                isleBoxes.Add(isleBox);
            }
            Monitor.Log($"got {isleBoxes.Count} island boxes");
            foreach(Rectangle isleBox in isleBoxes)
            {
                bool[] landTiles = new bool[isleBox.Width * isleBox.Height];
                for (int i = 0; i < isleBox.Width * isleBox.Height; i++)
                    landTiles[i] = true;

                for(int x = 0; x < isleBox.Width; x++)
                {
                    for (int y = 0; y < isleBox.Height; y++)
                    {
                        int idx = y * isleBox.Width + x;
                        float land = 1f;
                        if(x == 0 || x == isleBox.Width - 1 || y == 0 || y == isleBox.Height - 1)
                        {
                            landTiles[idx] = false;
                            continue;
                        }

                        float widthOffset = Math.Abs(isleBox.Width / 2f - x) / (isleBox.Width / 2f);
                        float heightOffset = Math.Abs(isleBox.Height / 2f - y) / (isleBox.Height / 2f);

                        land -= widthOffset + heightOffset;
                        landTiles[idx] = Game1.random.NextDouble() < land;
                    }
                }
                bool changed = true;
                while (changed)
                {
                    changed = false;
                    for (int x = 0; x < isleBox.Width; x++)
                    {
                        for (int y = 0; y < isleBox.Height; y++)
                        {
                            int idx = y * isleBox.Width + x;
                            float land = 1f;
                            if (x == 0 || x == isleBox.Width - 1 || y == 0 || y == isleBox.Height - 1)
                            {
                                landTiles[idx] = false;
                                continue;
                            }

                            float widthOffset = Math.Abs(isleBox.Width / 2f - x) / (isleBox.Width / 2f);
                            float heightOffset = Math.Abs(isleBox.Height / 2f - y) / (isleBox.Height / 2f);

                            land -= widthOffset + heightOffset;
                            landTiles[idx] = Game1.random.NextDouble() < land;
                        }
                    }
                }

                for (int x = 0; x < isleBox.Width; x++)
                {
                    for (int y = 0; y < isleBox.Height; y++)
                    {
                        int idx = y * isleBox.Width + x;

                        if (landTiles[idx])
                        {
                            back.Tiles[isleBox.X + x, isleBox.Y + y] = new StaticTile(back, sheet, BlendMode.Alpha, GetRandomLandTile());
                        }
                    }
                }
            }
            return map;
        }

        private static int GetRandomOceanTile()
        {
            double d = Game1.random.NextDouble();
            int[] tiles = new int[]
            {
                458,
                185,
                475,
                130
            };
            double chance = 0.02;
            for(int i = 0; i < tiles.Length; i++)
            {
                if (d < chance * (i + 1))
                    return tiles[i];
            }

            return 75;
        }
        private static int GetRandomLandTile()
        {
            double d = Game1.random.NextDouble();
            int[] tiles = new int[]
            {
                18,
                168,
                25,
                43
            };
            double chance = 0.02;
            for (int i = 0; i < tiles.Length; i++)
            {
                if (d < chance * (i + 1))
                    return tiles[i];
            }

            if (d < 0.02)
                return 18;
            if (d < 0.04)
                return 168;
            if (d < 0.06)
                return 25;
            if (d < 0.08)
                return 43;

            return 42;
        }
    }
}