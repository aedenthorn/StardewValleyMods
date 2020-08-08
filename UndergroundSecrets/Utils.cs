using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using xTile.Dimensions;
using xTile.Tiles;

namespace UndergroundSecrets
{
    public class Utils
    {
        private static IModHelper helper;
        private static IMonitor monitor;
        private static ModConfig config;


        public static void Initialize(IModHelper _helper, IMonitor _monitor, ModConfig _config)
        {
            helper = _helper;
            monitor = _monitor;
            config = _config;
        }
        public static void AddSecrets(MineShaft shaft)
        {
            List<Vector2> clearSpots = new List<Vector2>();
            List<Vector2> clearCenters = new List<Vector2>();
            List<Vector2> superClearCenters = new List<Vector2>();

            Vector2 tileBeneathLadder = helper.Reflection.GetField<NetVector2>(shaft, "netTileBeneathLadder").GetValue();

            for (int x = 0; x < shaft.map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < shaft.map.Layers[0].LayerHeight; y++)
                {
                    if (x == tileBeneathLadder.X && y == tileBeneathLadder.Y)
                        continue;
                    Tile tile = shaft.map.GetLayer("Back").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                    if (tile != null && (tile.TileIndex / 16 > 7 || tile.TileIndex % 16 < 9) && shaft.map.GetLayer("Buildings").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size) == null && shaft.map.GetLayer("Front").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size) == null)
                    {
                        clearSpots.Add(new Vector2(x, y));
                    }
                }
            }


            foreach (Vector2 spot in clearSpots)
            {
                int clear = GetTileClearance(shaft, spot, clearSpots);
                if(clear > 0)
                {
                    clearCenters.Add(spot);
                }
                if(clear > 1)
                {
                    superClearCenters.Add(spot);
                }
            }
            monitor.Log($"got {clearSpots.Count} clear spots in {shaft.Name}");
            monitor.Log($"got {clearCenters.Count} clear centers in {shaft.Name}");
            monitor.Log($"got {superClearCenters.Count} super clear centers in {shaft.Name}");

            monitor.Log($"adding underground_secrets tilesheet");
            shaft.map.AddTileSheet(new TileSheet(ModEntry.tileSheetId, shaft.map, ModEntry.tileSheetPath, new Size(16,18), new Size(16,16)));
            shaft.map.LoadTileSheets(Game1.mapDisplayDevice);
            TilePuzzles.Start(shaft, ref superClearCenters, ref clearCenters, ref clearSpots);
            OfferingPuzzles.Start(shaft, ref superClearCenters, ref clearCenters, ref clearSpots);
            Traps.Start(shaft, ref superClearCenters, ref clearCenters, ref clearSpots);
            CollapsedFloors.Start(shaft, ref superClearCenters, ref clearCenters, ref clearSpots);
            MushroomTrees.Start(shaft, ref superClearCenters, ref clearCenters, ref clearSpots);
        }
        public static void DropChest(MineShaft shaft, Vector2 spot)
        {
            monitor.Log($"solved, dropping chest!");
            shaft.playSound("yoba");
            shaft.overlayObjects[spot] = new Chest(0, new List<Item>() { MineShaft.getTreasureRoomItem() }, spot, false, 0);
        }
        internal static float Clamp(float min, float max, float val)
        {
            return Math.Max(min, Math.Min(max, val));
        }

        internal static List<Vector2> GetSurroundingTiles(Vector2 spot, int v, bool skipCenter = false)
        {
            List<Vector2> spots = new List<Vector2>();
            int diam = v * 2 + 1;
            for (int x = 0; x < diam; x++)
            {
                for (int y = 0; y < diam; y++)
                {
                    if (!skipCenter || x != v || y != v)
                        spots.Add(new Vector2((int)spot.X - v + x, (int)spot.Y - v + y));
                }
            }
            return spots;
        }

        private static int GetTileClearance(GameLocation l, Vector2 spot, List<Vector2> clearSpots)
        {
            bool superClear = true;
            for(int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    if (!clearSpots.Contains(new Vector2((int)spot.X - 2 + x, (int)spot.Y - 2 + y)))
                    {
                        superClear = false;
                        if (x < 4 && y < 4 && x > 0 && y > 0)
                        {
                            return 0;
                        }
                    }
                }
            }
            return superClear ? 2 : 1;
        }

        internal static int GetSheetIndex(MineShaft shaft)
        {
            for(int i = 0; i < shaft.map.TileSheets.Count; i++)
            {
                monitor.Log($"{shaft.Name} tilesheet {i}: {shaft.map.TileSheets[i].Id}");
                if (shaft.map.TileSheets[i].Id == "z_underground_secrets")
                    return i;
            }
            return -1;
        }

        internal static Vector2[] getCenteredSpots(Vector2 spot, bool skipCenter = false)
        {
            if (skipCenter)
            {
                return new Vector2[]
                {
                    new Vector2(spot.X - 1, spot.Y - 1),
                    new Vector2(spot.X, spot.Y - 1),
                    new Vector2(spot.X + 1, spot.Y - 1),
                    new Vector2(spot.X - 1, spot.Y),
                    new Vector2(spot.X + 1, spot.Y),
                    new Vector2(spot.X - 1, spot.Y + 1),
                    new Vector2(spot.X, spot.Y + 1),
                    new Vector2(spot.X + 1, spot.Y + 1)
                };
            }
            return new Vector2[]
            {
                new Vector2(spot.X - 1, spot.Y - 1),
                new Vector2(spot.X, spot.Y - 1),
                new Vector2(spot.X + 1, spot.Y - 1),
                new Vector2(spot.X - 1, spot.Y),
                new Vector2(spot.X, spot.Y),
                new Vector2(spot.X + 1, spot.Y),
                new Vector2(spot.X - 1, spot.Y + 1),
                new Vector2(spot.X, spot.Y + 1),
                new Vector2(spot.X + 1, spot.Y + 1)
            };
        }

        public static List<T> ShuffleList<T>(List<T> list)
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
            return list;
        }
    }
}