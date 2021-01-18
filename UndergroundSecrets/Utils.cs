using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using xTile.Dimensions;
using xTile.Tiles;
using Object = StardewValley.Object;

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
            if (config.OverrideTreasureRooms && (helper.Reflection.GetField<NetBool>(shaft, "netIsTreasureRoom").GetValue().Value || (shaft.mineLevel < 121 && shaft.mineLevel % 20 == 0) || shaft.mineLevel == 10 || shaft.mineLevel == 50 || shaft.mineLevel == 70 || shaft.mineLevel == 90 || shaft.mineLevel == 110))
            {
                monitor.Log($"is treasure room");

                return;
            }

            List <Vector2> clearSpots = new List<Vector2>();
            List<Vector2> clearCenters = new List<Vector2>();
            List<Vector2> superClearCenters = new List<Vector2>();

            Vector2 tileBeneathLadder = helper.Reflection.GetField<NetVector2>(shaft, "netTileBeneathLadder").GetValue();

            monitor.Log($"tileBeneathLadder: {tileBeneathLadder}");

            for (int x = 0; x < shaft.map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < shaft.map.Layers[0].LayerHeight; y++)
                {
                    Tile build = shaft.map.GetLayer("Buildings").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);

                    if(build != null)
                    {
                        if(build.TileIndex == 115)
                        {
                            tileBeneathLadder = new Vector2(x, y + 1);
                            monitor.Log($"made tileBeneathLadder: {tileBeneathLadder}");
                        }
                        continue;
                    }

                    if (x == tileBeneathLadder.X && y == tileBeneathLadder.Y)
                        continue;
                    Tile tile = shaft.map.GetLayer("Back").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                    if (tile != null && (tile.TileIndex / 16 > 7 || tile.TileIndex % 16 < 9) && shaft.map.GetLayer("Front").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size) == null)
                    {
                        clearSpots.Add(new Vector2(x, y));
                    }
                }
            }
            monitor.Log($"clearSpots contains tileBeneathLadder: {clearSpots.Contains(tileBeneathLadder)}");


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
            TileSheet mine = shaft.map.TileSheets[0];
            helper.Reflection.GetField<Size>(mine, "m_sheetSize").SetValue(new Size(16, 18));
            shaft.map.AddTileSheet(new TileSheet(ModEntry.tileSheetId, shaft.map, ModEntry.tileSheetPath, new Size(16,18), new Size(16,16)));
            shaft.map.LoadTileSheets(Game1.mapDisplayDevice);
            TilePuzzles.Start(shaft, ref superClearCenters, ref clearCenters, ref clearSpots);
            LightPuzzles.Start(shaft, ref superClearCenters, ref clearCenters, ref clearSpots);
            OfferingPuzzles.Start(shaft, ref superClearCenters, ref clearCenters, ref clearSpots);
            Altars.Start(shaft, ref superClearCenters, ref clearCenters, ref clearSpots);
            RiddlePuzzles.Start(shaft, ref superClearCenters, ref clearCenters, ref clearSpots);
            Traps.Start(shaft, ref superClearCenters, ref clearCenters, ref clearSpots);
            if(shaft.mineLevel > 120)
                CollapsingFloors.Start(shaft, ref superClearCenters, ref clearCenters, ref clearSpots);
            MushroomTrees.Start(shaft, ref superClearCenters, ref clearCenters, ref clearSpots);
        }
        public static void DropChest(MineShaft shaft, Vector2 spot)
        {
            monitor.Log($"solved, dropping chest!");
            shaft.playSound("yoba");

            if (config.OverrideTreasureRooms && ((shaft.mineLevel < 121 && shaft.mineLevel % 20 == 0) || shaft.mineLevel == 10 || shaft.mineLevel == 50 || shaft.mineLevel == 70 || shaft.mineLevel == 90 || shaft.mineLevel == 110))
            {
                addLevelChests(shaft);
                return;
            }
            if (ModEntry.treasureChestsExpandedApi == null)
            {
                shaft.overlayObjects[spot] = new Chest(0, new List<Item>() { MineShaft.getTreasureRoomItem() }, spot, false, 0);
            }
            else
            {
                monitor.Log($"dropping expanded chest!");

                shaft.overlayObjects[spot] = ModEntry.treasureChestsExpandedApi.MakeChest(shaft.mineLevel, spot);
            }
        }

        private static void addLevelChests(MineShaft shaft)
        {

            List<Item> chestItem = new List<Item>();
            Vector2 chestSpot = new Vector2(9f, 9f);
            Color tint = Color.White;
            if (shaft.mineLevel % 20 == 0 && shaft.mineLevel % 40 != 0)
                chestSpot.Y += 4f;
            int mineLevel = shaft.mineLevel;
            if (mineLevel == 10)
                chestItem.Add(new Boots(506));
            else if (mineLevel == 20)
                chestItem.Add(new MeleeWeapon(11));
            else if (mineLevel == 40)
            {
                Game1.player.completeQuest(17);
                chestItem.Add(new Slingshot());
            }
            else if (mineLevel == 50)
                chestItem.Add(new Boots(509));
            else if (mineLevel == 60)
                chestItem.Add(new MeleeWeapon(21));
            else if (mineLevel == 70)
                chestItem.Add(new Slingshot(33));
            else if (mineLevel == 80)
                chestItem.Add(new Boots(512));
            else if (mineLevel == 90)
                chestItem.Add(new MeleeWeapon(8));
            else if (mineLevel == 100)
                chestItem.Add(new Object(434, 1, false, -1, 0));
            else if (mineLevel == 110)
                chestItem.Add(new Boots(514));
            else if (mineLevel == 120)
            {
                Game1.player.completeQuest(18);
                Game1.getSteamAchievement("Achievement_TheBottom");
                if (!Game1.player.hasSkullKey)
                {
                    chestItem.Add(new SpecialItem(4, ""));
                }
                tint = Color.Pink;
            }
            else if (helper.Reflection.GetField<NetBool>(shaft, "netIsTreasureRoom").GetValue().Value)
            {
                chestItem.Add(MineShaft.getTreasureRoomItem());
            }
            if (chestItem.Count > 0 && !Game1.player.chestConsumedMineLevels.ContainsKey(shaft.mineLevel))
            {
                shaft.overlayObjects[chestSpot] = new Chest(0, chestItem, chestSpot, false, 0)
                {
                    Tint = tint
                };
            }
        }

        internal static float Clamp(float min, float max, float val)
        {
            return Math.Max(min, Math.Min(max, val));
        }

        internal static List<Vector2> GetSurroundingTiles(Vector2 spot, int radius, bool skipCenter = false)
        {
            List<Vector2> spots = new List<Vector2>();
            int diam = radius * 2 + 1;
            for (int x = 0; x < diam; x++)
            {
                for (int y = 0; y < diam; y++)
                {
                    if (!skipCenter || x != radius || y != radius)
                        spots.Add(new Vector2((int)spot.X - radius + x, (int)spot.Y - radius + y));
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
        internal static int GetMainSheetIndex(MineShaft shaft)
        {
            for(int i = 0; i < shaft.map.TileSheets.Count; i++)
            {
                monitor.Log($"{shaft.Name} tilesheet {i}: {shaft.map.TileSheets[i].Id}");
                if (shaft.map.TileSheets[i].Id.StartsWith("mine"))
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

        public static List<T> ShuffleList<T>(List<T> _list)
        {
            List<T> list = new List<T>(_list);
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