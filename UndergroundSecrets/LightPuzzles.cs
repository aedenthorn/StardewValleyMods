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
    internal class LightPuzzles
    {
        private static IModHelper helper;
        private static IMonitor monitor;
        private static ModConfig config;
        private static int cornerY = 15;

        public static void Initialize(IModHelper _helper, IMonitor _monitor, ModConfig _config)
        {
            helper = _helper;
            monitor = _monitor;
            config = _config;
        }

        internal static void Start(MineShaft shaft, ref List<Vector2> superClearCenters, ref List<Vector2> clearCenters, ref List<Vector2> clearSpots)
        {
            if (Game1.random.NextDouble() >= config.LightPuzzleBaseChance * Math.Pow(shaft.mineLevel, config.PuzzleChanceIncreaseRate) || superClearCenters.Count == 0)
                return;

            monitor.Log($"adding a light puzzle");

            Vector2 spot = superClearCenters[Game1.random.Next(0, superClearCenters.Count)];
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

            Layer back = shaft.map.GetLayer("Back");
            Layer front = shaft.map.GetLayer("Front");
            Layer buildings = shaft.map.GetLayer("Buildings");
            if (shaft.map.TileSheets.FirstOrDefault(s => s.Id == ModEntry.tileSheetId) == null)
                shaft.map.AddTileSheet(new TileSheet(ModEntry.tileSheetId, shaft.map, ModEntry.tileSheetPath, new Size(16, 18), new Size(16, 16)));
            TileSheet tilesheet = shaft.map.GetTileSheet(ModEntry.tileSheetId);

            int[] order = Utils.ShuffleList(new List<int>() { 0, 1, 2, 3 }).Take(3).ToArray();

            int tl = (order.Contains(0) && Game1.random.NextDouble() < 0.5 ? 1 : 0);
            int tr = (order.Contains(1) && Game1.random.NextDouble() < 0.5 ? 1 : 0);
            int bl = (order.Contains(2) && Game1.random.NextDouble() < 0.5 ? 1 : 0);
            int br = (order.Contains(3) && Game1.random.NextDouble() < 0.5 ? 1 : 0);

            if (spot.X > 1)
            {
                if (spot.Y > 3)
                    front.Tiles[(int)spot.X - 2, (int)spot.Y - 4] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 16 * cornerY + tl);
                if (spot.Y > 2)
                    front.Tiles[(int)spot.X - 2, (int)spot.Y - 3] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 1) + tl);
                if (spot.Y > 1)
                    buildings.Tiles[(int)spot.X - 2, (int)spot.Y - 2] = new StaticTile(buildings, tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 2) + tl);

                if (spot.Y < buildings.LayerSize.Height - 3)
                    buildings.Tiles[(int)spot.X - 2, (int)spot.Y + 2] = new StaticTile(buildings, tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 2) + bl);

                front.Tiles[(int)spot.X - 2, (int)spot.Y + 1] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 1) + bl);
                front.Tiles[(int)spot.X - 2, (int)spot.Y] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 16 * cornerY + bl);
            }
            if (spot.X < buildings.LayerSize.Width - 3)
            {

                if (spot.Y > 3)
                    front.Tiles[(int)spot.X + 2, (int)spot.Y - 4] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 16 * cornerY + tr);
                if (spot.Y > 2)
                    front.Tiles[(int)spot.X + 2, (int)spot.Y - 3] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 1) + tr);
                if (spot.Y > 1)
                    buildings.Tiles[(int)spot.X + 2, (int)spot.Y - 2] = new StaticTile(buildings, tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 2) + tr);

                if (spot.Y < buildings.LayerSize.Height - 3)
                    buildings.Tiles[(int)spot.X + 2, (int)spot.Y + 2] = new StaticTile(buildings, tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 2) + br);

                front.Tiles[(int)spot.X + 2, (int)spot.Y + 1] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 1) + br);
                front.Tiles[(int)spot.X + 2, (int)spot.Y] = new StaticTile(front, tilesheet, BlendMode.Alpha, tileIndex: 16 * cornerY + br);
            }


            back.Tiles[(int)spot.X - 1, (int)spot.Y - 1] = new StaticTile(back, tilesheet, BlendMode.Alpha, tileIndex: 176);
            back.Tiles[(int)spot.X - 1, (int)spot.Y + 1] = new StaticTile(back, tilesheet, BlendMode.Alpha, tileIndex: 176);
            back.Tiles[(int)spot.X + 1, (int)spot.Y - 1] = new StaticTile(back, tilesheet, BlendMode.Alpha, tileIndex: 176);
            back.Tiles[(int)spot.X + 1, (int)spot.Y + 1] = new StaticTile(back, tilesheet, BlendMode.Alpha, tileIndex: 176);

            int[][] solvable = {
                new int[]{ 1,3,2,4 },
                new int[]{ 3,2,4,1 },
                new int[]{ 2,4,1,3 },
                new int[]{ 4,1,3,2 },
                new int[]{ 2,3,1,4 },
                new int[]{ 3,1,4,2 },
                new int[]{ 1,4,2,3 },
                new int[]{ 4,2,3,1 }
            };

            int[] vals = solvable[Game1.random.Next(0,solvable.Length)];


            shaft.setTileProperty((int)spot.X - 1, (int)spot.Y - 1, "Back", "TouchAction", $"lightPuzzle_{vals[0]}_{spot.X}_{spot.Y}");
            shaft.setTileProperty((int)spot.X + 1, (int)spot.Y - 1, "Back", "TouchAction", $"lightPuzzle_{vals[1]}_{spot.X}_{spot.Y}");
            shaft.setTileProperty((int)spot.X + 1, (int)spot.Y + 1, "Back", "TouchAction", $"lightPuzzle_{vals[2]}_{spot.X}_{spot.Y}");
            shaft.setTileProperty((int)spot.X - 1, (int)spot.Y + 1, "Back", "TouchAction", $"lightPuzzle_{vals[3]}_{spot.X}_{spot.Y}");
        }

        internal static void pressTile(MineShaft shaft, Vector2 pos, string action)
        {
            string[] parts = action.Split('_').Skip(1).ToArray();
            monitor.Log($"Pressed floor number {parts[0]} (center: {parts[1]},{parts[2]}) at {pos} {shaft.Name}");
            shaft.playSound("Ship", SoundContext.Default);

            int idx = int.Parse(parts[0]);
            int cx = int.Parse(parts[1]);
            int cy = int.Parse(parts[2]);
            Vector2 spot = new Vector2(cx, cy);

            Vector2[] corners = new Vector2[]
            {
                new Vector2(-1,-1),
                new Vector2(1,-1),
                new Vector2(1,1),
                new Vector2(-1,1)
            };

            int corner = 0;

            if(cx < pos.X && cy > pos.Y) // tr
            {
                corner = 1;
            }
            else if(cx < pos.X && cy < pos.Y) // br
            {
                corner = 2;
            }
            else if (cx > pos.X && cy < pos.Y) // bl
            {
                corner = 3;
            }
            monitor.Log($"corner {corner}, idx {idx}, center {cx},{cy}, pos {pos}");

            TileSheet tilesheet = shaft.map.GetTileSheet(ModEntry.tileSheetId);
            int lit = 0;
            for (int i = 0; i < 4; i++)
            {
                bool isLit = shaft.map.GetLayer("Buildings").Tiles[(int)(spot.X + 2 * corners[corner].X), (int)(spot.Y + 2 * corners[corner].Y)].TileIndex == 16 * (cornerY + 2) + 1;
                int light = isLit ? 0 : 1;

                if(i < idx)
                {
                    shaft.map.GetLayer("Front").Tiles[(int)(spot.X + 2 * corners[corner].X), (int)(spot.Y + 2 * corners[corner].Y) - 2] = new StaticTile(shaft.map.GetLayer("Front"), tilesheet, BlendMode.Alpha, tileIndex: 16 * cornerY + light);
                    shaft.map.GetLayer("Front").Tiles[(int)(spot.X + 2 * corners[corner].X), (int)(spot.Y + 2 * corners[corner].Y - 1)] = new StaticTile(shaft.map.GetLayer("Front"), tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 1) + light);
                    shaft.map.GetLayer("Buildings").Tiles[(int)(spot.X + 2 * corners[corner].X), (int)(spot.Y + 2 * corners[corner].Y)] = new StaticTile(shaft.map.GetLayer("Buildings"), tilesheet, BlendMode.Alpha, tileIndex: 16 * (cornerY + 2) + light);
                    lit += light;
                    monitor.Log($"switched {i}, corner {corner}, idx {idx}, lit {lit}, isLit {isLit}, light {light}");
                }
                else
                {
                    lit += (isLit ? 1 : 0);
                    monitor.Log($"didn't switch {i}, corner {corner}, idx {idx}, lit {lit}, isLit {isLit}, light {light}");
                }
                corner++;
                corner %= 4;
            }
            if (lit == 4)
            {
                shaft.playSound("Duggy", SoundContext.Default);

                for (int i = 0; i < 4; i++)
                    shaft.removeTileProperty((int)(spot.X + corners[i].X), (int)(spot.Y + corners[i].Y), "Back", "TouchAction");
                Utils.DropChest(shaft, spot);
            }
        }

    }
}