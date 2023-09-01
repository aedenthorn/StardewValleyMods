using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace HedgeMaze
{
    public partial class ModEntry
    {
        //public static int mapSize = 57;
        public static Point[] neighbours = new Point[]
        {
            new Point(0, -2),
            new Point(2, 0),
            new Point(0, 2),
            new Point(-2, 0)
        };

        private void ReloadMazes()
        {

            mazeDataDict = SHelper.GameContent.Load<Dictionary<string, MazeData>>(dictPath);
            mazeLocationDict.Clear();
            SMonitor.Log($"Got {mazeDataDict.Count} mazes");
            List<string> maps = new();
            foreach (var kvp in mazeDataDict.ToArray())
            {
                var gameLocation = kvp.Value.gameLocation is null ? kvp.Key : kvp.Value.gameLocation;
                var gl = Game1.getLocationFromName(gameLocation);
                if (gl is null)
                    continue;
                if (!mazeLocationDict.TryGetValue(gameLocation, out var list)) 
                {
                    list = new();
                    mazeLocationDict[gameLocation] = list;
                }
                var inst = new MazeInstance()
                {
                    id = kvp.Key
                };
                
                if (kvp.Value.mapSize.X % 2 == 0)
                {
                    kvp.Value.mapSize.X--;
                }
                if (kvp.Value.mapSize.Y % 2 == 0)
                {
                    kvp.Value.mapSize.Y--;
                }
                mazeDataDict[kvp.Key] = kvp.Value;

                inst.tiles = MakeMapArray(kvp.Value, inst);
                inst.mapPath = gl.mapPath.Value;
                maps.Add(inst.mapPath);
                list.Add(inst);
            }
            foreach(var map in maps)
            {
                SHelper.GameContent.InvalidateCache(map);
            }
            PopulateMazes();
        }
        private static void ModifyMap(Map map, MazeInstance inst)
        {
            MazeData data = mazeDataDict[inst.id];
            ExtendMap(map, data.corner.X + data.mapSize.X, data.corner.Y + data.mapSize.Y);
            var festSheet = new TileSheet("Custom_HedgeMaze_Fest", map, "Maps/Festivals", new Size(512 / 16, 512 / 16), new Size(16, 16));
            var mainSheet = new TileSheet("Custom_HedgeMaze_Main", map, "Maps/spring_outdoorsTileSheet", new Size(400 / 16, 1264 / 16), new Size(16, 16));
            map.AddTileSheet(festSheet);
            map.AddTileSheet(mainSheet);
            
            var back = map.GetLayer("Back");
            var buildings = map.GetLayer("Buildings");
            var front = map.GetLayer("Front");
            var alwaysFront = map.GetLayer("AlwaysFront");
            if(alwaysFront is null)
            {
                alwaysFront = new Layer("AlwaysFront", map, back.LayerSize, back.TileSize);
                map.AddLayer(alwaysFront);
            }

            inst.openTiles = new();
            inst.endTiles = new();
            inst.vertTiles = new();
            inst.fairyTiles = new();
            if (data.AddTorches)
            {
                var torch = new AnimatedTile(alwaysFront, new StaticTile[] {
                    new StaticTile(alwaysFront, festSheet, BlendMode.Alpha, 599),
                    new StaticTile(alwaysFront, festSheet, BlendMode.Alpha, 600),
                    new StaticTile(alwaysFront, festSheet, BlendMode.Alpha, 601)
                }, 100);
                switch (data.entranceSide)
                {
                    case EntranceSide.Left:
                        alwaysFront.Tiles[data.corner.X, data.corner.Y + data.entranceOffset - 2] = torch;
                        alwaysFront.Tiles[data.corner.X, data.corner.Y + data.entranceOffset] = torch;
                        break;
                    case EntranceSide.Bottom:
                        alwaysFront.Tiles[data.corner.X + data.entranceOffset - 1, data.corner.Y + data.mapSize.Y - 2] = torch;
                        alwaysFront.Tiles[data.corner.X + data.entranceOffset + 1, data.corner.Y + data.mapSize.Y - 2] = torch;
                        break;
                    case EntranceSide.Right:
                        alwaysFront.Tiles[data.corner.X + data.mapSize.X - 1, data.corner.Y + data.entranceOffset - 2] = torch;
                        alwaysFront.Tiles[data.corner.X + data.mapSize.X - 1, data.corner.Y + data.entranceOffset] = torch;
                        break;
                    default:
                        alwaysFront.Tiles[data.corner.X + data.entranceOffset - 1, data.corner.Y - 1] = torch;
                        alwaysFront.Tiles[data.corner.X + data.entranceOffset + 1, data.corner.Y - 1] = torch;
                        break;
                }
            }


            for (int y = 0; y < data.mapSize.Y; y++)
            {
                for (int x = 0; x < data.mapSize.X; x++)
                {
                    var tx = x + data.corner.X;
                    var ty = y + data.corner.Y;
                    if (data.HideMaze && (data.HideBorders || (tx != data.corner.X && tx < data.corner.X + data.mapSize.X - 1 && ty < data.corner.Y + data.mapSize.Y - 2)))
                    {
                        front.Tiles[tx, ty] = new StaticTile(front, mainSheet, BlendMode.Alpha, 946);
                        try
                        {
                            if (data.HideBorders && y == 0 && front.Tiles[tx, ty - 1] is not AnimatedTile)
                            {
                                front.Tiles[tx, ty - 1] = new StaticTile(front, mainSheet, BlendMode.Alpha, 946);
                            }
                        }
                        catch { }
                    }

                    bool left = x > 0 && !inst.tiles[x - 1, y];
                    bool right = x < data.mapSize.X - 2 && !inst.tiles[x + 1, y];
                    bool up = y > 0 && !inst.tiles[x, y - 1];
                    bool down = y < data.mapSize.Y - 2 && !inst.tiles[x, y + 1];
                    if (!inst.tiles[x, y])
                    {
                        int[] wallTiles = GetWallTiles(left, right, up, down);
                        if (wallTiles[0] > -1)
                        {
                            buildings.Tiles[tx, ty] = new StaticTile(buildings, festSheet, BlendMode.Alpha, wallTiles[0]);
                        }
                        if (wallTiles[1] > -1 && front.Tiles[tx, ty - 1]?.TileIndex != 946)
                        {
                            front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, wallTiles[1]);
                        }
                    }
                    else
                    {
                        var which = Game1.random.NextDouble();
                        if (which < 0.025f)
                        {
                            back.Tiles[tx, ty] = new StaticTile(back, mainSheet, BlendMode.Alpha, 304);

                        }
                        else if (which < 0.05f)
                        {
                            back.Tiles[tx, ty] = new StaticTile(back, mainSheet, BlendMode.Alpha, 305);

                        }
                        else if (which < 0.15f)
                        {
                            back.Tiles[tx, ty] = new StaticTile(back, mainSheet, BlendMode.Alpha, 300);
                        }
                        else
                        {
                            back.Tiles[tx, ty] = new StaticTile(back, mainSheet, BlendMode.Alpha, 351);
                        }
                        inst.openTiles.Add(new Point(tx, ty));
                        if (!down && !up)
                        {
                            inst.vertTiles.Add(new Point(tx, ty));
                        }
                        else if (
                            (!down && up && left && right)
                            || (down && !up && left && right)
                            || (down && up && !left && right)
                            || (down && up && left && !right)
                            )
                        {
                            inst.endTiles.Add(new Point(tx, ty));
                        }
                    }
                }
            }
        }

        private static int[] GetWallTiles(bool left, bool right, bool up, bool down)
        {
            int[] wallTiles = new int[] { -1, -1 };

            if (left)
            {
                if (right)
                {
                    wallTiles[0]  = 660;
                    if (up)
                    {
                        if (down)
                        {
                            wallTiles[1] = 563;
                        }
                        else
                        {
                            wallTiles[1] = 626;
                        }
                    }
                    else
                    {
                        if (down)
                        {
                            wallTiles[1] = 563;
                        }
                        else
                        {
                            wallTiles[1] = 628;
                        }
                    }
                }
                else
                {
                    wallTiles[0]  = 661;
                    if (up)
                    {
                        if (down)
                        {
                            wallTiles[1] = 565;
                        }
                        else
                        {
                            wallTiles[1] = 629;
                        }
                    }
                    else
                    {
                        if (down)
                        {
                            wallTiles[1] = 565;
                        }
                        else
                        {
                            wallTiles[1] = 629;
                        }
                    }
                }
            }
            else
            {
                if (right)
                {
                    wallTiles[0]  = 659;
                    if (up)
                    {
                        if (down)
                        {
                            wallTiles[1] = 563;
                        }
                        else
                        {
                            wallTiles[1] = 627;
                        }
                    }
                    else
                    {
                        if (down)
                        {
                            wallTiles[1] = 563;
                        }
                        else
                        {
                            wallTiles[1] = 627;
                        }
                    }
                }
                else
                {
                    wallTiles[0]  = 661;
                    if (up)
                    {
                        if (down)
                        {
                            wallTiles[1] = 597;
                        }
                        else
                        {
                            wallTiles[1] = 629;
                        }
                    }
                    else
                    {
                        if (down)
                        {
                            wallTiles[1] = 565;
                        }
                        else
                        {
                            wallTiles[1] = 665;
                        }
                    }
                }
            }
            return wallTiles;
        }

        public static void PopulateMazes()
        {
            foreach (var kvp in mazeLocationDict)
            {
                foreach(var inst in kvp.Value)
                {
                    MazeData data = mazeDataDict[inst.id];
                    if (advancedLootFrameworkApi != null)
                    {
                        SMonitor.Log($"loaded AdvancedLootFramework API", LogLevel.Debug);
                        treasuresList = advancedLootFrameworkApi.LoadPossibleTreasures(data.ItemListChances.Where(p => p.Value > 0).ToDictionary(s => s.Key, s => s.Value).Keys.ToArray(), data.MinItemValue, data.MaxItemValue);
                        if (treasuresList != null)
                            SMonitor.Log($"Got {treasuresList.Count} possible treasures");
                    }
                    var gl = Game1.getLocationFromName(kvp.Key);
                    if (gl is null)
                        continue;
                    PopulateMaze(gl, data, inst);
                }
            }
        }
        public static void PopulateMaze(GameLocation gl, MazeData mazeData, MazeInstance inst)
        {
            for(int x = mazeData.corner.X; x < mazeData.corner.X + mazeData.mapSize.X; x++)
            {
                for (int y = mazeData.corner.Y; y < mazeData.corner.Y + mazeData.mapSize.Y; y++)
                {
                    var tile = new Vector2(x, y);
                    gl.terrainFeatures.Remove(tile);
                    gl.objects.Remove(tile);
                }
            }

            int slimes = Game1.random.Next(mazeData.SlimeMin, mazeData.SlimeMax + 1);
            int bats = Game1.random.Next(mazeData.BatMin, mazeData.BatMax + 1);
            int serpents = Game1.random.Next(mazeData.SerpentMin, mazeData.SerpentMax + 1);
            int brutes = Game1.random.Next(mazeData.ShadowBruteMin, mazeData.ShadowBruteMax + 1);
            int shamans = Game1.random.Next(mazeData.ShadowShamanMin, mazeData.ShadowShamanMax + 1);
            int squids = Game1.random.Next(mazeData.SquidMin, mazeData.SquidMax + 1);
            int skeletons = Game1.random.Next(mazeData.SkeletonMin, mazeData.SkeletonMax + 1);
            int dusts = Game1.random.Next(mazeData.DustSpriteMin, mazeData.DustSpriteMax + 1);
            int fairies = Game1.random.Next(mazeData.FairiesMin, mazeData.FairiesMax + 1);
            int treasures = Game1.random.Next(mazeData.TreasureMin, mazeData.TreasureMax + 1);
            int forages = Game1.random.Next(mazeData.ForageMin, mazeData.ForageMax + 1);

            Dictionary<string, string> locationData = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
            if (locationData.TryGetValue(gl.Name, out string rawData))
            {
                var data = rawData.Split('/', StringSplitOptions.None)[Utility.getSeasonNumber(gl.GetSeasonForLocation())];
                if (!data.Equals("-1"))
                {
                    string[] split = data.Split(' ');
                    for (int i = 0; i < forages; i++)
                    {
                        if (!inst.vertTiles.Any())
                            break;
                        int idx = Game1.random.Next(inst.vertTiles.Count);
                        Vector2 v = inst.vertTiles[idx].ToVector2();
                        inst.vertTiles.RemoveAt(idx);
                        gl.objects.TryGetValue(v, out Object o);
                        int whichObject = Game1.random.Next(split.Length / 2) * 2;
                        gl.dropObject(new Object(v, int.Parse(split[whichObject]), null, false, true, false, true), new Vector2((float)(v.X * 64), (float)(v.Y * 64)), Game1.viewport, true, null);
                        if (Config.Debug)
                        {
                            SMonitor.Log($"Spawning forage at {v}");
                        }

                    }

                }
            }
            if (treasuresList is not null)
            {
                for (int i = 0; i < treasures; i++)
                {
                    if (!inst.endTiles.Any())
                        break;
                    int idx = Game1.random.Next(inst.endTiles.Count);
                    Vector2 v = inst.endTiles[idx].ToVector2();
                    inst.endTiles.RemoveAt(idx);
                    double fraction = Math.Pow(Game1.random.NextDouble(), 1 / mazeData.RarityChance);
                    int level = (int)Math.Ceiling(fraction * mazeData.Mult);
                    //Monitor.Log($"Adding expanded chest of value {level} to {l.name}");
                    Chest chest = advancedLootFrameworkApi.MakeChest(treasuresList, mazeData.ItemListChances, mazeData.MaxItems, mazeData.MinItemValue, mazeData.MaxItemValue, level, mazeData.IncreaseRate, mazeData.ItemsBaseMaxValue, mazeData.CoinBaseMin, mazeData.CoinBaseMax, v);
                    chest.playerChoiceColor.Value = MakeTint(fraction);
                    chest.CanBeGrabbed = false;
                    gl.overlayObjects[v] = chest;
                    if (Config.Debug)
                    {
                        SMonitor.Log($"Spawning chest at {v}");
                    }
                }
            }

            for (int i = 0; i < fairies; i++)
            {
                if (!inst.endTiles.Any())
                    break;
                int idx = Game1.random.Next(inst.endTiles.Count);
                Vector2 v = inst.endTiles[idx].ToVector2() - new Vector2(0, 1);
                inst.endTiles.RemoveAt(idx);
                inst.fairyTiles.Add(v);
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning fairy at {v}");
                }
            }
            if (inst.endTiles.Any() && mazeData.AddDwarf)
            {
                int idx = Game1.random.Next(inst.endTiles.Count);
                Vector2 v = inst.endTiles[idx].ToVector2();
                inst.endTiles.RemoveAt(idx);
                gl.addCharacter(new NPC(new AnimatedSprite("Characters\\Dwarf", 0, 16, 24), v * 64, "Woods", 2, "Dwarf", false, null, Game1.content.Load<Texture2D>("Portraits\\Dwarf"))
                {
                    Breather = false
                });
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning dwarf at {v}");
                }
            }
            for (int i = 0; i < slimes; i++)
            {
                if (!inst.openTiles.Any())
                    break;
                int idx = Game1.random.Next(inst.openTiles.Count);
                Vector2 v = inst.openTiles[idx].ToVector2() * 64;
                inst.openTiles.RemoveAt(idx);
                gl.characters.Add(new GreenSlime(v, Game1.random.Next(mazeData.MineLevelMin, mazeData.MineLevelMax)));
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning slime at {v}");
                }
            }
            for (int i = 0; i < bats; i++)
            {
                if (!inst.openTiles.Any())
                    break;
                int idx = Game1.random.Next(inst.openTiles.Count);
                Vector2 v = inst.openTiles[idx].ToVector2() * 64;
                inst.openTiles.RemoveAt(idx);
                gl.characters.Add(new Bat(v, Game1.random.Next(mazeData.MineLevelMin, mazeData.MineLevelMax)));
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning bat at {v}");
                }
            }
            for (int i = 0; i < serpents; i++)
            {
                if (!inst.openTiles.Any())
                    break;
                int idx = Game1.random.Next(inst.openTiles.Count);
                Vector2 v = inst.openTiles[idx].ToVector2() * 64;
                inst.openTiles.RemoveAt(idx);
                gl.characters.Add(new Serpent(v));
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning serpent at {v}");
                }
            }
            for (int i = 0; i < brutes; i++)
            {
                if (!inst.openTiles.Any())
                    break;
                int idx = Game1.random.Next(inst.openTiles.Count);
                Vector2 v = inst.openTiles[idx].ToVector2() * 64;
                inst.openTiles.RemoveAt(idx);
                gl.characters.Add(new ShadowBrute(v));
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning brute at {v}");
                }
            }
            for (int i = 0; i < shamans; i++)
            {
                if (!inst.openTiles.Any())
                    break;
                int idx = Game1.random.Next(inst.openTiles.Count);
                Vector2 v = inst.openTiles[idx].ToVector2() * 64;
                inst.openTiles.RemoveAt(idx);
                gl.characters.Add(new ShadowShaman(v));
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning shaman at {v}");
                }

            }
            for (int i = 0; i < squids; i++)
            {
                if (!inst.openTiles.Any())
                    break;
                int idx = Game1.random.Next(inst.openTiles.Count);
                Vector2 v = inst.openTiles[idx].ToVector2() * 64;
                inst.openTiles.RemoveAt(idx);
                gl.characters.Add(new SquidKid(v));
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning squid at {v}");
                }
            }
            for (int i = 0; i < skeletons; i++)
            {
                if (!inst.openTiles.Any())
                    break;
                int idx = Game1.random.Next(inst.openTiles.Count);
                Vector2 v = inst.openTiles[idx].ToVector2() * 64;
                inst.openTiles.RemoveAt(idx);
                gl.characters.Add(new Skeleton(v));
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning skeleton at {v}");
                }

            }
            for (int i = 0; i < dusts; i++)
            {
                if (!inst.openTiles.Any())
                    break;
                int idx = Game1.random.Next(inst.openTiles.Count);
                Vector2 v = inst.openTiles[idx].ToVector2() * 64;
                inst.openTiles.RemoveAt(idx);
                gl.characters.Add(new DustSpirit(v));

                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning dust sprite at {v}");
                }
            }
        }
        private static Color MakeTint(double fraction)
        {
            Color color = tintColors[(int)Math.Floor(fraction * tintColors.Length)];
            return color;
        }

        public static bool[,] MakeMapArray(MazeData data, MazeInstance inst)
        {
            var mapSize = data.mapSize;
            bool[,] map = new bool[mapSize.X, mapSize.Y];
            Point start = new Point(1,1);
            List<Point> checkedTiles = new();
            List<Point> checkingTiles = new();
            CheckTile(ref map, checkedTiles, checkingTiles, start, mapSize); // start at tile below entrance
            if (data.entranceOffset > -1)
            {
                switch (data.entranceSide)
                {
                    case EntranceSide.Left:
                        map[0, data.entranceOffset] = true;
                        map[0, data.entranceOffset + 1] = true;
                        break;
                    case EntranceSide.Bottom:
                        map[data.entranceOffset, data.mapSize.Y - 1] = true;
                        map[data.entranceOffset, data.mapSize.Y - 2] = true;
                        break;
                    case EntranceSide.Right:
                        map[data.mapSize.X - 1, data.entranceOffset] = true;
                        map[data.mapSize.X - 2, data.entranceOffset] = true;
                        break;
                    default:
                        map[data.entranceOffset, 0] = true;
                        map[data.entranceOffset, 1] = true;
                        break;
                }
            }
            else
            {
                foreach (var i in data.topEntranceOffsets)
                {
                    map[i, 0] = true; // knock down entrance
                    map[i, 1] = true; // knock down tile inside
                }
                foreach (var i in data.rightEntranceOffsets)
                {
                    map[data.mapSize.X - 1, i] = true; // knock down entrance
                    map[data.mapSize.X - 2, i] = true; // knock down tile inside
                }
                foreach (var i in data.leftEntranceOffsets)
                {
                    map[0, i] = true; // knock down entrance
                    map[1, i] = true; // knock down tile inside
                }
                foreach (var i in data.bottomEntranceOffsets)
                {
                    map[i, data.mapSize.Y - 1] = true; // knock down entrance
                    map[i, data.mapSize.Y - 2] = true; // knock down tile inside
                }
            }
            if (Config.Debug)
            {
                List<string> output = new List<string>();
                for (int y = 0; y < mapSize.Y; y++)
                {
                    string line = "";
                    for (int x = 0; x < mapSize.X; x++)
                    {
                        line += map[x, y] ? " " : "#";
                    }
                    output.Add(line);
                }
                File.WriteAllLines(Path.Combine(SHelper.DirectoryPath, "map.txt"), output.ToArray());
            }
            return map;
        }

        private static void CheckTile(ref bool[,] map, List<Point> checkedTiles, List<Point> checkingTiles, Point tile, Point mapSize)
        {
            map[tile.X, tile.Y] = true;
            var list = neighbours.ToList();
            ShuffleList(list);
            for(var i = 0; i < 4; i++)
            {
                Point n = tile + list[i];
                if (IsInMap(n, mapSize) && !checkedTiles.Contains(n) && !checkingTiles.Contains(n))
                {
                    var x = tile.X + list[i].X / 2;
                    var y = tile.Y + list[i].Y / 2;

                    map[x, y] = true; // knock down wall;

                    checkingTiles.Add(tile);
                    CheckTile(ref map, checkedTiles, checkingTiles, n, mapSize);
                    return;
                }
            }
            checkedTiles.Add(tile);
            if(checkingTiles.Any())
            {
                tile = checkingTiles[checkingTiles.Count - 1];
                checkingTiles.RemoveAt(checkingTiles.Count - 1);
                CheckTile(ref map, checkedTiles, checkingTiles, tile, mapSize);
            }
        }

        private static bool IsTileOnMaze(Point n, Point mapSize)
        {
            return n.X >= 0 && n.X < mapSize.X && n.Y >= 0 && n.Y < mapSize.Y;
        }
        private static bool IsInMap(Point n, Point mapSize)
        {
            return n.X > 0 && n.X < mapSize.X - 1 && n.Y > 0 && n.Y < mapSize.Y - 1;
        }

        private static bool IsTileInMaze(Point n, Point mapSize, Point corner)
        {
            return n.X > corner.X && n.X < corner.X + mapSize.X && n.Y > corner.Y && n.Y < corner.Y + mapSize.Y;
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

        private void DepopulateMaps()
        {
            foreach (var kvp in mazeLocationDict)
            {
                var gl = Game1.getLocationFromName(kvp.Key);
                if (gl is null)
                {
                    continue;
                }
                Helper.GameContent.InvalidateCache(gl.mapPath.Value);
                foreach (var inst in kvp.Value)
                {
                    MazeData data = mazeDataDict[inst.id];
                    for (int i = gl.characters.Count - 1; i >= 0; i--)
                    {
                        if (IsTileInMaze(gl.characters[i].getTileLocationPoint(), data.mapSize, data.corner) && (gl.characters[i] is Monster || gl.characters[i].Name.Equals("Dwarf")))
                        {
                            gl.characters.RemoveAt(i);
                        }
                    }
                    foreach (var kvp2 in gl.objects.Pairs.ToArray())
                    {
                        if (IsTileInMaze(Utility.Vector2ToPoint(kvp2.Key), data.mapSize, data.corner))
                            gl.objects.Remove(kvp2.Key);
                    }
                }
            }
            mazeLocationDict.Clear();
        }
        private static void ExtendMap(Map map, int x, int y)
        {
            SMonitor.Log($"Extending map to {x}x{y}");
            List<Layer> layers = AccessTools.Field(typeof(Map), "m_layers").GetValue(map) as List<Layer>;
            for (int i = 0; i < layers.Count; i++)
            {
                Tile[,] tiles = AccessTools.Field(typeof(Layer), "m_tiles").GetValue(layers[i]) as Tile[,];
                Size size = (Size)AccessTools.Field(typeof(Layer), "m_layerSize").GetValue(layers[i]);
                if (size.Width >= x && size.Height >= y)
                    continue;
                if (size.Width >= x)
                {
                    x = size.Width;
                }
                if (size.Height >= y)
                {
                    y = size.Height;
                }
                size = new Size(x, y);
                AccessTools.Field(typeof(Layer), "m_layerSize").SetValue(layers[i], size);
                AccessTools.Field(typeof(Map), "m_layers").SetValue(map, layers);

                Tile[,] newTiles = new Tile[x, y];

                for (int k = 0; k < tiles.GetLength(0); k++)
                {
                    for (int l = 0; l < tiles.GetLength(1); l++)
                    {
                        newTiles[k, l] = tiles[k, l];
                    }
                }
                AccessTools.Field(typeof(Layer), "m_tiles").SetValue(layers[i], newTiles);
                AccessTools.Field(typeof(Layer), "m_tileArray").SetValue(layers[i], new TileArray(layers[i], newTiles));

            }
            AccessTools.Field(typeof(Map), "m_layers").SetValue(map, layers);
        }

    }
}