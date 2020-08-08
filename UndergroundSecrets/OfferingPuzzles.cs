using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static void Initialize(IModHelper _helper, IMonitor _monitor, ModConfig _config)
        {
            helper = _helper;
            monitor = _monitor;
            config = _config;
        }

        internal static void Start(MineShaft shaft, ref List<Vector2> superClearCenters, ref List<Vector2> clearCenters, ref List<Vector2> clearSpots)
        {
            if (Game1.random.NextDouble() >= config.OfferingPuzzleChance || clearCenters.Count == 0)
                return;


            Vector2 spot = clearCenters[Game1.random.Next(0,clearCenters.Count)];
            if (spot.Y < 3)
                return;

            monitor.Log($"adding an offering puzzle");

            int idx = 0;
            if(shaft.mineLevel > 40 && shaft.mineLevel < 80)
            {
                idx = Game1.random.Next(0, 2);
            }
            else if(shaft.mineLevel > 80 && shaft.mineLevel < 120)
            {
                idx = Game1.random.Next(0, 3);
            }
            else if(shaft.mineLevel > 120)
            {
                idx = Game1.random.Next(0, 4);
            }

            Layer buildings = shaft.map.GetLayer("Buildings");
            Layer front = shaft.map.GetLayer("Front");
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

            string action = $"offerPuzzle_{idx}_{spot.X}_{spot.Y}";
            shaft.setTileProperty((int)spot.X - full, (int)spot.Y + 1, "Buildings", "Action", action);

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

    }
}