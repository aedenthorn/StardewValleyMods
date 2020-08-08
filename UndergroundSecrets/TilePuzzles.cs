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
    internal class TilePuzzles
    {
        private static IModHelper helper;
        private static IMonitor monitor;
        private static ModConfig config;
        private static int cornerY = 12;

        public static void Initialize(IModHelper _helper, IMonitor _monitor, ModConfig _config)
        {
            helper = _helper;
            monitor = _monitor;
            config = _config;
        }

        internal static void Start(MineShaft shaft, ref List<Vector2> superClearCenters, ref List<Vector2> clearCenters, ref List<Vector2> clearSpots)
        {
            if (Game1.random.NextDouble() >= config.TilePuzzleChance || superClearCenters.Count == 0)
                return;

            monitor.Log($"adding a tile puzzle");

            Vector2 spot = superClearCenters[Game1.random.Next(0, superClearCenters.Count)];

            Vector2[] spots = Utils.getCenteredSpots(spot, true);
            
            spots = Utils.ShuffleList(spots.ToList()).ToArray();

            int idx = Game1.random.Next(0, 4);

            Layer layer = shaft.map.GetLayer("Back");
            TileSheet tilesheet = shaft.map.GetTileSheet(ModEntry.tileSheetId);

            monitor.Log($"building: {shaft.map.GetLayer("Buildings").LayerSize.Width},{shaft.map.GetLayer("Buildings").LayerSize.Height} front: {shaft.map.GetLayer("Front").LayerSize.Width},{shaft.map.GetLayer("Front").LayerSize.Height} spot {spot}");

            if(spot.X > 1)
            {
                if(spot.Y > 3)
                    shaft.map.GetLayer("Front").Tiles[(int)spot.X - 2, (int)spot.Y - 4] = new StaticTile(shaft.map.GetLayer("Front"), tilesheet, BlendMode.Alpha, tileIndex: 16 * cornerY + idx);
                if (spot.Y > 2)
                    shaft.map.GetLayer("Front").Tiles[(int)spot.X - 2, (int)spot.Y - 3] = new StaticTile(shaft.map.GetLayer("Front"), tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 1) + idx);
                if (spot.Y > 1)
                    shaft.map.GetLayer("Buildings").Tiles[(int)spot.X - 2, (int)spot.Y - 2] = new StaticTile(shaft.map.GetLayer("Buildings"), tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 2) + idx);

                if (spot.Y < shaft.map.GetLayer("Buildings").LayerSize.Height - 3)
                    shaft.map.GetLayer("Buildings").Tiles[(int)spot.X - 2, (int)spot.Y + 2] = new StaticTile(shaft.map.GetLayer("Buildings"), tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 2) + idx);

                shaft.map.GetLayer("Front").Tiles[(int)spot.X - 2, (int)spot.Y + 1] = new StaticTile(shaft.map.GetLayer("Front"), tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 1) + idx);
                shaft.map.GetLayer("Front").Tiles[(int)spot.X - 2, (int)spot.Y] = new StaticTile(shaft.map.GetLayer("Front"), tilesheet, BlendMode.Alpha, tileIndex: 16 * cornerY + idx);
            }
            if(spot.X < shaft.map.GetLayer("Buildings").LayerSize.Width - 3)
            {

                if (spot.Y > 3)
                    shaft.map.GetLayer("Front").Tiles[(int)spot.X + 2, (int)spot.Y - 4] = new StaticTile(shaft.map.GetLayer("Front"), tilesheet, BlendMode.Alpha, tileIndex: 16 * cornerY + idx);
                if (spot.Y > 2)
                    shaft.map.GetLayer("Front").Tiles[(int)spot.X + 2, (int)spot.Y - 3] = new StaticTile(shaft.map.GetLayer("Front"), tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 1) + idx);
                if (spot.Y > 1)
                    shaft.map.GetLayer("Buildings").Tiles[(int)spot.X + 2, (int)spot.Y - 2] = new StaticTile(shaft.map.GetLayer("Buildings"), tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 2) + idx);

                if (spot.Y < shaft.map.GetLayer("Buildings").LayerSize.Height - 3)
                    shaft.map.GetLayer("Buildings").Tiles[(int)spot.X + 2, (int)spot.Y + 2] = new StaticTile(shaft.map.GetLayer("Buildings"), tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 2) + idx);

                shaft.map.GetLayer("Front").Tiles[(int)spot.X + 2, (int)spot.Y + 1] = new StaticTile(shaft.map.GetLayer("Front"), tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 1) + idx);
                shaft.map.GetLayer("Front").Tiles[(int)spot.X + 2, (int)spot.Y] = new StaticTile(shaft.map.GetLayer("Front"), tilesheet, BlendMode.Alpha, tileIndex: 16 * cornerY + idx);
            }


            for (int i = 0; i < spots.Length; i++)
            {
                string action = $"tilePuzzle_{i}_{spot.X}_{spot.Y}";
                layer.Tiles[(int)spots[i].X, (int)spots[i].Y] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: i + 16 * idx);
                shaft.setTileProperty((int)spots[i].X, (int)spots[i].Y, "Back", "TouchAction", action);
            }
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

        internal static void pressTile(MineShaft shaft, Vector2 playerStandingPosition, string action)
        {
            string[] parts = action.Split('_').Skip(1).ToArray();
            monitor.Log($"Pressed floor number {parts[0]} (center: {parts[1]},{parts[2]}) at {playerStandingPosition} {shaft.Name}");

            int idx = int.Parse(parts[0]);
            int cx = int.Parse(parts[1]);
            int cy = int.Parse(parts[2]);

            Vector2[] spots = Utils.getCenteredSpots(new Vector2(cx, cy), true);

            bool correct = CheckTileOrder(shaft, spots, idx,cx,cy);

            Layer layer = shaft.map.GetLayer("Back");
            TileSheet tilesheet = shaft.map.GetTileSheet(ModEntry.tileSheetId);
            if (correct)
            {
                monitor.Log($"correct order, deactivating tile {idx}");
                shaft.playSound("Ship", SoundContext.Default);
                layer.Tiles[(int)playerStandingPosition.X, (int)playerStandingPosition.Y] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: idx + 8);
                shaft.removeTileProperty(cx, cy, "Back", "TouchAction");
            }
            else
            {
                shaft.playSound("Duggy", SoundContext.Default);
                foreach (Vector2 spot in spots)
                {
                    monitor.Log($"wrong order, deactivating puzzle");
                    layer.Tiles[(int)spot.X, (int)spot.Y] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: layer.Tiles[(int)spot.X, (int)spot.Y].TileIndex + 8);
                    shaft.removeTileProperty((int)spot.X, (int)spot.Y, "Back", "TouchAction");
                    Traps.TriggerRandomTrap(shaft, playerStandingPosition);
                }
            }
        }

        private static bool CheckTileOrder(MineShaft shaft, Vector2[] spots, int idx, int cx, int cy)
        {
            bool remain = false;

            foreach (Vector2 spot in spots)
            {
                string val = shaft.doesTileHaveProperty((int)spot.X, (int)spot.Y, "TouchAction", "Back");
                if (val == null)
                    continue;
                int i = int.Parse(val.Split('_')[1]);
                if (val.StartsWith("tilePuzzle_") && i != idx)
                {
                    monitor.Log($"remaining tile {i}");

                    remain = true;
                    if(i < idx)
                    {
                        return false;
                    }
                }
            }
            if (!remain)
                DropChest(shaft, new Vector2(cx, cy));
            return true;
        }

        private static void DropChest(MineShaft shaft, Vector2 spot)
        {
            monitor.Log($"solved, dropping chest!");
            shaft.playSound("yoba", SoundContext.Default);
            shaft.overlayObjects[spot] = new Chest(0, new List<Item>() { MineShaft.getTreasureRoomItem() }, spot, false, 0);
        }
    }
}