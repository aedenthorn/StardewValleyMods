using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile.Dimensions;
using xTile.Tiles;
using Object = StardewValley.Object;

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
            var festSheet = new TileSheet("Fest", map, "Maps/Festivals", new Size(512 / 16, 512 / 16), new Size(16, 16));
            map.AddTileSheet(festSheet);
            var tiles = MakeMapArray();
            var back = map.GetLayer("Back");
            var buildings = map.GetLayer("Buildings");
            var front = map.GetLayer("Front");
            var alwaysFront = map.GetLayer("AlwaysFront");
            TileSheet mainSheet = map.GetTileSheet("untitled tile sheet");


            openTiles = new();
            endTiles = new();
            vertTiles = new();
            fairyTiles = new();

            var torch = new AnimatedTile(alwaysFront, new StaticTile[] { 
                new StaticTile(alwaysFront, festSheet, BlendMode.Alpha, 599),
                new StaticTile(alwaysFront, festSheet, BlendMode.Alpha, 600),
                new StaticTile(alwaysFront, festSheet, BlendMode.Alpha, 601)
            }, 100);
            alwaysFront.Tiles[26, 32] = torch;
            alwaysFront.Tiles[28, 32] = torch;

            for (int y = 0; y < mapSize; y++)
            {
                for (int x = 0; x < mapSize; x++)
                {
                    var tx = x + 1;
                    var ty = y + 34;
                    if (Config.HideMaze && alwaysFront.Tiles[tx, ty] is not AnimatedTile)
                    {
                        alwaysFront.Tiles[tx, ty] = new StaticTile(alwaysFront, mainSheet, BlendMode.Alpha, 946);
                    }

                    bool left = x > 0 && !tiles[x - 1, y];
                    bool right = x < mapSize - 2 && !tiles[x + 1, y];
                    bool up = y > 0 && !tiles[x, y - 1];
                    bool down = y < mapSize - 2 && !tiles[x, y + 1];
                    if (!tiles[x, y])
                    {
                        if (left)
                        {
                            if (right)
                            {
                                buildings.Tiles[tx, ty] = new StaticTile(buildings, festSheet, BlendMode.Alpha, 660);
                                if (up)
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 563);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 626);
                                    }
                                }
                                else
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 563);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 628);
                                    }
                                }
                            }
                            else
                            {
                                buildings.Tiles[tx, ty] = new StaticTile(buildings, festSheet, BlendMode.Alpha, 661);
                                if (up)
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 565);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 629);
                                    }
                                }
                                else
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 565);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 629);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (right)
                            {
                                buildings.Tiles[tx, ty] = new StaticTile(buildings, festSheet, BlendMode.Alpha, 659);
                                if (up)
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 563);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 627);
                                    }
                                }
                                else
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 563);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 627);
                                    }
                                }
                            }
                            else
                            {
                                buildings.Tiles[tx, ty] = new StaticTile(buildings, festSheet, BlendMode.Alpha, 661);
                                if (up)
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 597);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 629);
                                    }
                                }
                                else
                                {
                                    if (down)
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 565);
                                    }
                                    else
                                    {
                                        front.Tiles[tx, ty - 1] = new StaticTile(front, festSheet, BlendMode.Alpha, 665);
                                    }
                                }
                            }
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
                        openTiles.Add(new Point(tx, ty));
                        if (!down && !up)
                        {
                            vertTiles.Add(new Point(tx, ty));
                        }
                        else if (
                            (!down && up && left && right)
                            || (down && !up && left && right)
                            || (down && up && !left && right)
                            || (down && up && left && !right)
                            )
                        {
                            endTiles.Add(new Point(tx, ty));
                        }
                    }
                }
            }
        }
        public static void PopulateMap()
        {

            var woods = Game1.getLocationFromName("Woods");
            if (woods is null)
                return;
            int slimes = Game1.random.Next(Config.SlimeMin, Config.SlimeMax + 1);
            int bats = Game1.random.Next(Config.BatMin, Config.BatMax + 1);
            int serpents = Game1.random.Next(Config.SerpentMin, Config.SerpentMax + 1);
            int brutes = Game1.random.Next(Config.ShadowBruteMin, Config.ShadowBruteMax + 1);
            int shamans = Game1.random.Next(Config.ShadowShamanMin, Config.ShadowShamanMax + 1);
            int squids = Game1.random.Next(Config.SquidMin, Config.SquidMax + 1);
            int skeletons = Game1.random.Next(Config.SkeletonMin, Config.SkeletonMax + 1);
            int dusts = Game1.random.Next(Config.DustSpriteMin, Config.DustSpriteMax + 1);
            int fairies = Game1.random.Next(Config.FairiesMin, Config.FairiesMax + 1);
            int treasures = Game1.random.Next(Config.TreasureMin, Config.TreasureMax + 1);
            int forages = Game1.random.Next(Config.ForageMin, Config.ForageMax + 1);

            Dictionary<string, string> locationData = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
            if (locationData.TryGetValue(woods.Name, out string rawData))
            {
                var data = rawData.Split('/', StringSplitOptions.None)[Utility.getSeasonNumber(woods.GetSeasonForLocation())];
                if (!data.Equals("-1"))
                {
                    string[] split = data.Split(' ');
                    for (int i = 0; i < forages; i++)
                    {
                        if (!vertTiles.Any())
                            break;
                        int idx = Game1.random.Next(vertTiles.Count);
                        Vector2 v = vertTiles[idx].ToVector2();
                        vertTiles.RemoveAt(idx);
                        woods.objects.TryGetValue(v, out Object o);
                        int whichObject = Game1.random.Next(split.Length / 2) * 2;
                        woods.dropObject(new Object(v, int.Parse(split[whichObject]), null, false, true, false, true), new Vector2((float)(v.X * 64), (float)(v.Y * 64)), Game1.viewport, true, null);
                        if (Config.Debug)
                        {
                            SMonitor.Log($"Spawning forage at {v}");
                        }

                    }

                }
            }

            if(treasuresList is not null)
            {
                for (int i = 0; i < treasures; i++)
                {
                    if (!endTiles.Any())
                        break;
                    int idx = Game1.random.Next(endTiles.Count);
                    Vector2 v = endTiles[idx].ToVector2();
                    endTiles.RemoveAt(idx);
                    double fraction = Math.Pow(Game1.random.NextDouble(), 1 / Config.RarityChance);
                    int level = (int)Math.Ceiling(fraction * Config.Mult);
                    //Monitor.Log($"Adding expanded chest of value {level} to {l.name}");
                    Chest chest = advancedLootFrameworkApi.MakeChest(treasuresList, Config.ItemListChances, Config.MaxItems, Config.MinItemValue, Config.MaxItemValue, level, Config.IncreaseRate, Config.ItemsBaseMaxValue, Config.CoinBaseMin, Config.CoinBaseMax, v);
                    chest.playerChoiceColor.Value = MakeTint(fraction);
                    woods.overlayObjects[v] = chest;
                    if (Config.Debug)
                    {
                        SMonitor.Log($"Spawning chest at {v}");
                    }
                }
            }

            for (int i = 0; i < fairies; i++)
            {
                if (!endTiles.Any())
                    break;
                int idx = Game1.random.Next(endTiles.Count);
                Vector2 v = endTiles[idx].ToVector2() - new Vector2(0, 1);
                endTiles.RemoveAt(idx);
                fairyTiles.Add(v);
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning fairy at {v}");
                }
            }
            if (endTiles.Any())
            {
                int idx = Game1.random.Next(endTiles.Count);
                Vector2 v = endTiles[idx].ToVector2();
                endTiles.RemoveAt(idx);
                woods.addCharacter(new NPC(new AnimatedSprite("Characters\\Dwarf", 0, 16, 24), v * 64, "Woods", 2, "Dwarf", false, null, Game1.content.Load<Texture2D>("Portraits\\Dwarf"))
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
                if (!openTiles.Any())
                    break;
                int idx = Game1.random.Next(openTiles.Count);
                Vector2 v = openTiles[idx].ToVector2() * 64;
                openTiles.RemoveAt(idx);
                woods.characters.Add(new GreenSlime(v, Game1.random.Next(Config.MineLevelMin, Config.MineLevelMax)));
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning slime at {v}");
                }
            }
            for (int i = 0; i < bats; i++)
            {
                if (!openTiles.Any())
                    break;
                int idx = Game1.random.Next(openTiles.Count);
                Vector2 v = openTiles[idx].ToVector2() * 64;
                openTiles.RemoveAt(idx);
                woods.characters.Add(new Bat(v, Game1.random.Next(Config.MineLevelMin, Config.MineLevelMax)));
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning bat at {v}");
                }
            }
            for (int i = 0; i < serpents; i++)
            {
                if (!openTiles.Any())
                    break;
                int idx = Game1.random.Next(openTiles.Count);
                Vector2 v = openTiles[idx].ToVector2() * 64;
                openTiles.RemoveAt(idx);
                woods.characters.Add(new Serpent(v));
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning serpent at {v}");
                }
            }
            for (int i = 0; i < brutes; i++)
            {
                if (!openTiles.Any())
                    break;
                int idx = Game1.random.Next(openTiles.Count);
                Vector2 v = openTiles[idx].ToVector2() * 64;
                openTiles.RemoveAt(idx);
                woods.characters.Add(new ShadowBrute(v));
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning brute at {v}");
                }
            }
            for (int i = 0; i < shamans; i++)
            {
                if (!openTiles.Any())
                    break;
                int idx = Game1.random.Next(openTiles.Count);
                Vector2 v = openTiles[idx].ToVector2() * 64;
                openTiles.RemoveAt(idx);
                woods.characters.Add(new ShadowShaman(v));
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning shaman at {v}");
                }

            }
            for (int i = 0; i < squids; i++)
            {
                if (!openTiles.Any())
                    break;
                int idx = Game1.random.Next(openTiles.Count);
                Vector2 v = openTiles[idx].ToVector2() * 64;
                openTiles.RemoveAt(idx);
                woods.characters.Add(new SquidKid(v));
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning squid at {v}");
                }
            }
            for (int i = 0; i < skeletons; i++)
            {
                if (!openTiles.Any())
                    break;
                int idx = Game1.random.Next(openTiles.Count);
                Vector2 v = openTiles[idx].ToVector2() * 64;
                openTiles.RemoveAt(idx);
                woods.characters.Add(new Skeleton(v));
                if (Config.Debug)
                {
                    SMonitor.Log($"Spawning skeleton at {v}");
                }

            }
            for (int i = 0; i < dusts; i++)
            {
                if (!openTiles.Any())
                    break;
                int idx = Game1.random.Next(openTiles.Count);
                Vector2 v = openTiles[idx].ToVector2() * 64;
                openTiles.RemoveAt(idx);
                woods.characters.Add(new DustSpirit(v));

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

        public static bool[,] MakeMapArray()
        {
            bool[,] map = new bool[mapSize, mapSize];

            int start = 26; // Game1.random.Next(mapSize / 2) * 2 + 1; // starting x
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

        private static bool IsTileInMaze(Vector2 n)
        {
            return n.X > 1 && n.X < 57 && n.Y > 34 && n.Y < 92;
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