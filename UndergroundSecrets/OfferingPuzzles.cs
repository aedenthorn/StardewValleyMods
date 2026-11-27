using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using static StardewValley.Network.NetAudio;

namespace UndergroundSecrets
{
    internal class OfferingPuzzles
    {
        private static IModHelper helper;
        private static IMonitor monitor;
        private static ModConfig config;
        private static int cornerY = 12;
        public static int offerIdx = 276;
        private static int[] ores = new int[] { 378, 380, 384, 386 };

        public static void Initialize(IModHelper _helper, IMonitor _monitor, ModConfig _config)
        {
            helper = _helper;
            monitor = _monitor;
            config = _config;
        }

        public static void Start(MineShaft shaft, ref List<Vector2> superClearCenters, ref List<Vector2> clearCenters, ref List<Vector2> clearSpots)
        {
            if (Game1.random.NextDouble() >= config.OfferingPuzzleBaseChance * Math.Pow(shaft.mineLevel, config.PuzzleChanceIncreaseRate) || clearCenters.Count == 0)
                return;


            Vector2 spot = clearCenters[Game1.random.Next(0,clearCenters.Count)];
            if (spot.Y < 3)
                return;

            CreatePuzzle(spot, shaft);


            foreach (Vector2 v in Utils.GetSurroundingTiles(spot, 4))
            {
                superClearCenters.Remove(v);
                if (Math.Abs(v.X - spot.X) < 3 && Math.Abs(v.Y - spot.Y) < 3)
                {
                    clearCenters.Remove(v);
                    if (Math.Abs(v.X - spot.X) < 2 && Math.Abs(v.Y - spot.Y) < 2)
                        clearSpots.Remove(v);
                }
            }
        }

        public static void CreatePuzzle(Vector2 spot, MineShaft shaft)
        {

            monitor.Log($"adding an offering puzzle");

            int idx = 0;
            if (shaft.mineLevel > 40 && shaft.mineLevel < 80)
            {
                idx = Game1.random.Next(0, 2);
            }
            else if (shaft.mineLevel > 80 && shaft.mineLevel < 120)
            {
                idx = Game1.random.Next(0, 3);
            }
            else if (shaft.mineLevel > 120)
            {
                idx = Game1.random.Next(0, 4);
            }

            Layer buildings = shaft.map.GetLayer("Buildings");
            Layer front = shaft.map.GetLayer("Front");
            if(shaft.map.TileSheets.FirstOrDefault(s => s.Id == ModEntry.tileSheetId) == null)
                shaft.map.AddTileSheet(new TileSheet(ModEntry.tileSheetId, shaft.map, ModEntry.tileSheetPath, new Size(16, 18), new Size(16, 16)));
            
            TileSheet tilesheet = shaft.map.GetTileSheet(ModEntry.tileSheetId);


            bool which = Game1.random.NextDouble() < 0.5f;
            int full = which ? 1 : -1;


            // full

            front.Tiles[(int)spot.X + full, (int)spot.Y - 1] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 245);
            front.Tiles[(int)spot.X + full, (int)spot.Y] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 260);
            buildings.Tiles[(int)spot.X + full, (int)spot.Y + 1] = new StaticTile(buildings, tilesheet, BlendMode.Alpha, tileIndex: offerIdx + 1 + idx);

            // empty

            front.Tiles[(int)spot.X - full, (int)spot.Y - 1] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 244);
            front.Tiles[(int)spot.X - full, (int)spot.Y] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 260);
            buildings.Tiles[(int)spot.X - full, (int)spot.Y + 1] = new StaticTile(buildings, tilesheet, BlendMode.Alpha, tileIndex: offerIdx);

            shaft.setTileProperty((int)spot.X - full, (int)spot.Y + 1, "Buildings", "Action", $"offerPuzzle_{idx}_{spot.X}_{spot.Y}");
            shaft.setTileProperty((int)spot.X + full, (int)spot.Y + 1, "Buildings", "Action", $"offerPuzzleSteal_{spot.X}_{spot.Y}");
        }

        internal static void StealAttempt(MineShaft shaft, string action, Location tileLocation, Farmer who)
        {
            monitor.Log($"Attempting to steal from altar");

            string[] parts = action.Split('_').Skip(1).ToArray();

            Vector2 spot = new Vector2(int.Parse(parts[0]), int.Parse(parts[1]));
            shaft.setMapTileIndex((int)spot.X - 1, (int)spot.Y + 1, OfferingPuzzles.offerIdx, "Buildings");
            shaft.setMapTileIndex((int)spot.X + 1, (int)spot.Y + 1, OfferingPuzzles.offerIdx, "Buildings");
            shaft.setMapTileIndex((int)spot.X - 1, (int)spot.Y - 1, 244, "Front");
            shaft.setMapTileIndex((int)spot.X + 1, (int)spot.Y - 1, 244, "Front");
            shaft.removeTileProperty((int)spot.X - 1, (int)spot.Y + 1, "Buildings", "Action");
            shaft.removeTileProperty((int)spot.X + 1, (int)spot.Y + 1, "Buildings", "Action");
            Traps.TriggerRandomTrap(shaft, new Vector2(who.getTileLocation().X, who.getTileLocation().Y), false);
        }

        internal static void OfferObject(MineShaft shaft, string action, Location tileLocation, Farmer who)
        {
            monitor.Log($"Attempting to offer to altar");

            string[] parts = action.Split('_').Skip(1).ToArray();
            Vector2 spot = new Vector2(int.Parse(parts[1]), int.Parse(parts[2]));
            if (ores[int.Parse(parts[0])] == who.ActiveObject.ParentSheetIndex)
            {
                monitor.Log($"Made acceptable offering to altar");
                who.reduceActiveItemByOne();
                shaft.setMapTileIndex(tileLocation.X, tileLocation.Y, OfferingPuzzles.offerIdx + 1 + int.Parse(parts[0]), "Buildings");
                shaft.setMapTileIndex(tileLocation.X, tileLocation.Y - 2, 245, "Front");
                shaft.setTileProperty((int)spot.X - 1, (int)spot.Y + 1, "Buildings", "Action", $"offerPuzzleSteal_{parts[1]}_{parts[2]}");
                shaft.setTileProperty((int)spot.X + 1, (int)spot.Y + 1, "Buildings", "Action", $"offerPuzzleSteal_{parts[1]}_{parts[2]}");
                Utils.DropChest(shaft, spot);
            }
            else
            {
                monitor.Log($"Made unacceptable offering to altar");
                who.reduceActiveItemByOne();
                shaft.setMapTileIndex((int)spot.X - 1, (int)spot.Y + 1, OfferingPuzzles.offerIdx, "Buildings");
                shaft.setMapTileIndex((int)spot.X + 1, (int)spot.Y + 1, OfferingPuzzles.offerIdx, "Buildings");
                shaft.removeTileProperty((int)spot.X - 1, (int)spot.Y + 1, "Buildings", "Action");
                shaft.removeTileProperty((int)spot.X + 1, (int)spot.Y + 1, "Buildings", "Action");
                Traps.TriggerRandomTrap(shaft, new Vector2(who.getTileLocation().X, who.getTileLocation().Y), false);
            }
        }
    }
}