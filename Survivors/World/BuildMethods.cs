using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Sickhead.Engine.Util;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Locations;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using Bush = StardewValley.TerrainFeatures.Bush;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Survivors
{
    public partial class ModEntry
    {
        

        public static void CreateAnimatedTiles()
        {

            var amt = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, AnimatedTileData>>>(File.ReadAllText(Path.Combine(SHelper.DirectoryPath, "assets", "animated_tiles.json")));
            
            animatedTiles.Clear();
            foreach (var kvp in amt) 
            {
                if (!animatedTiles.TryGetValue(kvp.Key, out var dict))
                {
                    dict = new Dictionary<int, AnimatedTile>();
                    animatedTiles[kvp.Key] = dict;
                }
                foreach(var kvp2 in kvp.Value)
                {
                    if (!dict.ContainsKey(kvp2.Key))
                    {
                        List<StaticTile> tileFrames = new();
                        foreach(var td in kvp2.Value.tileFrames)
                        {
                            Layer l = openWorldLocation.Map.GetLayer(td.layer);
                            TileSheet ts = openWorldLocation.Map.GetTileSheet(td.ts);
                            StaticTile st = new StaticTile(l, ts, td.blend, td.tileIndex);
                            tileFrames.Add(st);
                        }
                        Layer layer = openWorldLocation.Map.GetLayer(kvp2.Value.layer);
                        AnimatedTile tile = new AnimatedTile(layer, tileFrames.ToArray(), kvp2.Value.frameInterval);
                        dict[kvp2.Key] = tile;
                    }
                }
            }
            amt = new Dictionary<string, Dictionary<int, AnimatedTileData>>();
            return;
            List<Layer> layers = new();
            layers.AddRange(Game1.getLocationFromName("Mountain").Map.Layers);
            layers.AddRange(Game1.getLocationFromName("Town").Map.Layers);
            layers.AddRange(Game1.getLocationFromName("Forest").Map.Layers);
            layers.AddRange(Game1.getLocationFromName("Beach").Map.Layers);
            foreach (var tlayer in layers)
            {
                var layer = openWorldLocation.Map.GetLayer(tlayer.Id);
                if (layer is null)
                    continue;
                for(int x = 0; x < tlayer.LayerWidth; x++)
                {
                    for (int y = 0; y < tlayer.LayerHeight; y++)
                    {
                        if (tlayer.Tiles[x, y] is AnimatedTile tile)
                        {
                            string sheetID = openWorldLocation.Map.TileSheets.FirstOrDefault(s => s.ImageSource == tile.TileSheet.ImageSource)?.Id;
                            if (sheetID == null)
                                continue;
                            if (!amt.ContainsKey(sheetID))
                            {
                                amt[sheetID] = new();
                            }
                            List<StaticTileInfo> tiles = new List<StaticTileInfo>();
                            foreach(var t in tile.TileFrames)
                            {
                                var ts = GetOpenWorldTileSheet(t.TileSheet);
                                tiles.Add(new StaticTileInfo(layer.Id, ts.Id, BlendMode.Alpha, t.TileIndex));
                            }
                            amt[sheetID][tile.TileIndex] = new AnimatedTileData(layer.Id, tiles.ToArray(), tile.FrameInterval);
                        }
                    }
                }
            }
            File.WriteAllText("test.json", JsonConvert.SerializeObject(amt, Formatting.Indented));
            
        }

        public static TileSheet GetOpenWorldTileSheet(TileSheet tileSheet)
        {
            var nts = openWorldLocation.Map.TileSheets.FirstOrDefault(s => s.ImageSource == tileSheet.ImageSource);
            if (nts == null)
            {
                nts = new TileSheet(tileSheet.Id, openWorldLocation.Map, tileSheet.ImageSource, tileSheet.SheetSize, new xTile.Dimensions.Size(64, 64));
                openWorldLocation.Map.AddTileSheet(nts);
            }
            return nts;
        }

        private void ReloadOpenWorld(bool force)
        {
            for (int i = loadedChunks.Count - 1; i >= 0; i--)
            {
                UnloadChunk(loadedChunks[i]);
            }
            loadedChunks.Clear();
            landmarkRects.Clear();
            lakeRects.Clear();
            outcropRects.Clear();
            landmarkDict = SHelper.GameContent.Load<Dictionary<string, Landmark>>(landmarkDictPath);
            foreach (var landmark in landmarkDict.Values)
            {
                var map = SHelper.GameContent.Load<Map>(landmark.MapPath);
                Rectangle mapBox = new(landmark.MapPosition - new Point(1, 1), new(map.Layers[0].LayerWidth + 1, map.Layers[0].LayerHeight + 1));
                AddRectToList(mapBox, landmarkRects);
            }

            if (force)
            {
                treeCenters = new();
                rockCenters = new();
                lakeCenters = new();
                monsterCenters = new();
                
                RandomSeed = Utility.CreateRandomSeed(Game1.uniqueIDForThisGame / 100UL, Config.NewMapDaily ? Game1.stats.DaysPlayed * 10U + 1U : 0.0);

                Random r = Utility.CreateRandom(RandomSeed, "centers".GetHashCode());
                for (int i = 0; i < r.Next(Config.OpenWorldSize / Config.TilesForestMax * Config.OpenWorldSize, Config.OpenWorldSize / Config.TilesForestMin * Config.OpenWorldSize + 1); i++)
                {
                    Point ap = new(
                            r.Next(10, Config.OpenWorldSize - 10),
                            r.Next(10, Config.OpenWorldSize - 10)
                        );
                    Point cp = new(ap.X / openWorldChunkSize, ap.Y / openWorldChunkSize);
                    Vector2 rp = new(ap.X % openWorldChunkSize, ap.Y % openWorldChunkSize);
                    if (!treeCenters.ContainsKey(cp))
                    {
                        treeCenters[cp] = new();
                    }
                    treeCenters[cp][rp] = GetRandomTree(ap.ToVector2(), r);
                }
                for (int i = 0; i < r.Next(Config.OpenWorldSize / Config.TilesLakeMax * Config.OpenWorldSize, Config.OpenWorldSize / Config.TilesLakeMin * Config.OpenWorldSize + 1); i++)
                {
                    Point ap = new(
                            r.Next(10, Config.OpenWorldSize - 10),
                            r.Next(10, Config.OpenWorldSize - 10)
                        );
                    Point cp = new(ap.X / openWorldChunkSize, ap.Y / openWorldChunkSize);
                    Point rp = new(ap.X % openWorldChunkSize, ap.Y % openWorldChunkSize);
                    if (!lakeCenters.ContainsKey(cp))
                    {
                        lakeCenters[cp] = new();
                    }
                    if (!lakeCenters[cp].Exists(c => Vector2.Distance(c.ToVector2(), rp.ToVector2()) < openWorldChunkSize))
                        lakeCenters[cp].Add(rp);
                }
                for (int i = 0; i < r.Next(Config.OpenWorldSize / Config.TilesOutcropMax * Config.OpenWorldSize, Config.OpenWorldSize / Config.TilesOutcropMin * Config.OpenWorldSize + 1); i++)
                {
                    Point ap = new(
                            r.Next(10, Config.OpenWorldSize - 10),
                            r.Next(10, Config.OpenWorldSize - 10)
                        );
                    Point cp = new(ap.X / openWorldChunkSize, ap.Y / openWorldChunkSize);
                    Vector2 rp = new(ap.X % openWorldChunkSize, ap.Y % openWorldChunkSize);
                    if (!rockCenters.ContainsKey(cp))
                    {
                        rockCenters[cp] = new();
                    }
                    rockCenters[cp].Add(rp);
                }
                for (int i = 0; i < r.Next(Config.OpenWorldSize / Config.TilesGrassMax * Config.OpenWorldSize, Config.OpenWorldSize / Config.TilesGrassMin * Config.OpenWorldSize + 1); i++)
                {
                    Point ap = new(
                            r.Next(10, Config.OpenWorldSize - 10),
                            r.Next(10, Config.OpenWorldSize - 10)
                        );
                    Point cp = new(ap.X / openWorldChunkSize, ap.Y / openWorldChunkSize);
                    Vector2 rp = new(ap.X % openWorldChunkSize, ap.Y % openWorldChunkSize);
                    if (!grassCenters.ContainsKey(cp))
                    {
                        grassCenters[cp] = new();
                    }
                    grassCenters[cp].Add(rp);
                }
                for (int i = 0; i < r.Next(Config.OpenWorldSize / Config.TilesMonsterMax * Config.OpenWorldSize, Config.OpenWorldSize / Config.TilesMonsterMin * Config.OpenWorldSize + 1); i++)
                {
                    Point ap = new(
                            r.Next(10, Config.OpenWorldSize - 10),
                            r.Next(10, Config.OpenWorldSize - 10)
                        );
                    Point cp = new(ap.X / openWorldChunkSize, ap.Y / openWorldChunkSize);
                    Vector2 rp = new(ap.X % openWorldChunkSize, ap.Y % openWorldChunkSize);
                    if (!monsterCenters.ContainsKey(cp))
                    {
                        monsterCenters[cp] = new();
                    }
                    var ms = GetRandomMonsterSpawn(ap.ToVector2(), r);
                    if(ms != null)
                    {
                        monsterCenters[cp][rp] = ms;
                    }
                }
                cachedChunks = new();
            }
        }


        public static void AddLandmarksToChunk(Point cp)
        {
            var chunkBox = new Rectangle(cp.X * openWorldChunkSize, cp.Y * openWorldChunkSize, openWorldChunkSize, openWorldChunkSize);
            foreach (var landmark in landmarkDict.Values)
            {
                var map = SHelper.GameContent.Load<Map>(landmark.MapPath);
                Rectangle mapBox = new(landmark.MapPosition, new(map.Layers[0].LayerWidth, map.Layers[0].LayerHeight));
                if (!chunkBox.Intersects(mapBox))
                    continue;
                foreach (var l in map.Layers)
                {
                    var nl = openWorldLocation.Map.GetLayer(l.Id);
                    if (nl == null)
                    {
                        nl = new Layer(l.Id, openWorldLocation.Map, new xTile.Dimensions.Size(openWorldChunkSize, openWorldChunkSize), l.TileSize);
                        openWorldLocation.Map.AddLayer(nl);
                        openWorldLocation.SortLayers();
                    }
                    if (!cachedChunks[cp].tiles.TryGetValue(l.Id, out var tiles))
                    {
                        tiles = new Tile[openWorldChunkSize, openWorldChunkSize];
                        cachedChunks[cp].tiles[l.Id] = tiles;
                    }
                    for (int x = 0; x < l.Tiles.Array.GetLength(0); x++)
                    {
                        for (int y = 0; y < l.Tiles.Array.GetLength(1); y++)
                        {
                            var rp = new Point(landmark.MapPosition.X + x, landmark.MapPosition.Y + y);
                            openWorldLocation.terrainFeatures.Remove(rp.ToVector2());
                            if (l.Tiles[x, y] != null && chunkBox.Contains(rp))
                            {
                                var rx = landmark.MapPosition.X + x - chunkBox.X;
                                var ry = landmark.MapPosition.Y + y - chunkBox.Y;
                                var ot = l.Tiles[x, y];
                                var ots = ot.TileSheet;
                                var nts = GetOpenWorldTileSheet(ots);
                                Tile tile;
                                if (ot is AnimatedTile)
                                {
                                    List<StaticTile> frames = new();
                                    foreach (var t in (ot as AnimatedTile).TileFrames)
                                    {
                                        var nts2 = openWorldLocation.Map.TileSheets.FirstOrDefault(s => s.ImageSource == t.TileSheet.ImageSource);
                                        if (nts2 == null)
                                        {
                                            nts2 = new TileSheet(ots.Id, openWorldLocation.Map, ots.ImageSource, ots.SheetSize, l.TileSize);
                                            openWorldLocation.Map.AddTileSheet(nts);
                                        }
                                        frames.Add(new StaticTile(nl, nts2, t.BlendMode, t.TileIndex));
                                    }
                                    tile = new AnimatedTile(nl, frames.ToArray(), (ot as AnimatedTile).FrameInterval);
                                }
                                else
                                {
                                    tile = new StaticTile(nl, nts, ot.BlendMode, ot.TileIndex);
                                }
                                foreach(var prop in ot.Properties)
                                {
                                    tile.Properties.Add(prop);
                                }
                                cachedChunks[cp].tiles[l.Id][rx, ry] = tile;
                            }
                        }
                    }
                    cachedChunks[cp].tiles[l.Id] = tiles;
                }
            }
        }


        private static void AddLakesToChunk(Point cp)
        {
            Random r = Utility.CreateRandom(RandomSeed, cp.X * cp.Y + cp.X, "lakes".GetHashCode());
            if (!lakeCenters.TryGetValue(cp, out var centers))
                return;
            var back = openWorldLocation.Map.GetLayer("Back");
            var mainSheet = openWorldLocation.Map.GetTileSheet("Landscape");
            foreach (var c in centers)
            {
                int maxTiles = r.Next(Config.MinLakeSize * 2, Config.MaxLakeSize * 2 + 1);
                List<Point> tiles = GetBlob(c, r, maxTiles);
                if(tiles != null)
                {
                    int x = int.MaxValue;
                    int y = int.MaxValue;
                    int w = 0;
                    int h = 0;
                    var padding = GetBlobPadding(tiles, 5, true);
                    foreach (var p in padding)
                    {
                        if (p.X < x) x = p.X;
                        if (p.X > w) w = p.X;
                        if (p.Y < y) y = p.Y;
                        if (p.Y > h) h = p.Y;
                    }
                    var begin = new Point(x - 5, y - 5);
                    var end = new Point(w + 6, h + 6);
                    var ar = new Rectangle(ToGlobalTile(cp, begin), end - begin);
                    if (ConflictsRect(cp, ar))
                        continue;
                    foreach (var p in padding)
                    {
                        AddBlobTileToChunk(BorderTiles.meadow, GetRandomMeadowTile(r, back, mainSheet), back, mainSheet, cp, p, padding, r);
                    }
                    padding = GetBlobPadding(tiles, 3, true);
                    foreach (var p in padding)
                    {
                        AddBlobTileToChunk(BorderTiles.dirt, GetRandomDirtTile(r, back, mainSheet), back, mainSheet, cp, p, padding, r);
                    }
                    foreach (var p in tiles)
                    {
                        AddWaterTileToChunk(cp, p, tiles, r);
                    }
                    AddRectToList(ar, lakeRects);
                    SMonitor.Log($"Added lake {ar}");
                }
            }
        }
        

        private static void AddWaterTileToChunk(Point chunkPoint, Point p, List<Point> tiles, Random r)
        {
            var back = openWorldLocation.Map.GetLayer("Back");
            var build = openWorldLocation.Map.GetLayer("Buildings");
            var mainSheet = openWorldLocation.Map.GetTileSheet("Landscape");
            var tile = GetRandomWaterTile(r, back, mainSheet);
            AddTileToChunk(chunkPoint, "Back", p.X, p.Y, tile, true);
            if(tile.TileIndex == 1299 && tiles.Contains(p + new Point(0, -1)))
            {
                AddTileToChunk(chunkPoint, "Buildings", p.X, p.Y - 1, animatedTiles[mainSheet.Id][r.Choose(1293, 1318)], false);
            }
            
            //return;
            if (!tiles.Contains(p + new Point(-1, 0)))
            {

                if (tiles.Contains(p + new Point(-1, 1)))
                {
                    if(!tiles.Contains(p + new Point(-2, 2)) && tiles.Contains(p + new Point(-2, 3)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1241), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y, animatedTiles[mainSheet.Id][283], true);
                        if (!tiles.Contains(p + new Point(0, -1)))
                        {
                            AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y - 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1268), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y - 1, animatedTiles[mainSheet.Id][r.Choose(233, 208)], false);
                            AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y - 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1243), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y - 2, animatedTiles[mainSheet.Id][183], false);
                        }

                    }
                    else
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1266), true);
                        if (!tiles.Contains(p + new Point(0, -1)))
                        {
                            AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y - 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1268), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y - 1, animatedTiles[mainSheet.Id][283], false);
                            AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y - 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1243), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y - 2, animatedTiles[mainSheet.Id][183], false);
                        }
                    }
                }
                else if (tiles.Contains(p + new Point(-1, 2)))
                {
                    AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y, animatedTiles[mainSheet.Id][283], true);
                    if (tiles.Contains(p + new Point(0, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1241), true);
                    }
                    else
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1266), true);
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1268), true);
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y - 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1243), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y - 1, animatedTiles[mainSheet.Id][183], false);
                    }

                }
                else
                {
                    if(tiles.Contains(p + new Point(-1, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, GetRandomWaterTile(r, back, mainSheet), true);
                        AddTileToChunk(chunkPoint, "Back", p.X - 2, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246) , true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X - 2, p.Y, animatedTiles[mainSheet.Id][258], false);
                    }
                    else if(tiles.Contains(p + new Point(-1, -2)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y, animatedTiles[mainSheet.Id][237], false);

                    }
                    else 
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y, animatedTiles[mainSheet.Id][r.Next() > 0.5 ? 208 : 233], false);

                        if (!tiles.Contains(p + new Point(0, -1)))
                        {
                            AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y - 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1268), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y - 1, animatedTiles[mainSheet.Id][r.Next() > 0.5 ? 208 : 233], false);
                            AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y - 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1243), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y - 2, animatedTiles[mainSheet.Id][183], false);
                        }
                    }
                    if (!tiles.Contains(p + new Point(0, 1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y + 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X - 1, p.Y + 1, animatedTiles[mainSheet.Id][258], false);
                    }
                }
            }
            if (!tiles.Contains(p + new Point(1, 0)))
            {
                if (tiles.Contains(p + new Point(1, 1)))
                {
                    AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1267), true);
                    if (!tiles.Contains(p + new Point(0, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y - 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1270), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y - 1, animatedTiles[mainSheet.Id][284], false);
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y - 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1245), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y - 2, animatedTiles[mainSheet.Id][185], false);
                    }
                }
                else if (tiles.Contains(p + new Point(1, 2)))
                {
                    AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y, animatedTiles[mainSheet.Id][284], false);
                    AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1242), true);
                    if (!tiles.Contains(p + new Point(0, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y - 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1270), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y - 1, animatedTiles[mainSheet.Id][r.Next() > 0.5 ? 210 : 235], false);
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y - 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1245), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y - 2, animatedTiles[mainSheet.Id][185], false);
                    }

                }
                else
                {
                    if (tiles.Contains(p + new Point(1, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Back", p.X + 2, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X + 2, p.Y, animatedTiles[mainSheet.Id][260], false);
                    }
                    else if (tiles.Contains(p + new Point(1, -2)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y, animatedTiles[mainSheet.Id][238], false);
                    }
                    else
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y, animatedTiles[mainSheet.Id][r.Next() > 0.5 ? 210 : 235], false);

                        if (!tiles.Contains(p + new Point(0, -1)))
                        {
                            AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y - 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1270), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y - 1, animatedTiles[mainSheet.Id][r.Next() > 0.5 ? 210 : 235], false);
                            AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y - 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1245), true);
                            AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y - 2, animatedTiles[mainSheet.Id][185], false);
                        }
                    }
                    if (!tiles.Contains(p + new Point(0, 1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y + 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X + 1, p.Y + 1, animatedTiles[mainSheet.Id][260], false);
                    }

                }
            }
            if (!tiles.Contains(p + new Point(0, -1)))
            {
                if (!tiles.Contains(p + new Point(1, -1)) && !tiles.Contains(p + new Point(-1, -1)))
                {

                    AddTileToChunk(chunkPoint, "Back", p.X, p.Y - 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1269), true);
                    AddTileToChunk(chunkPoint, "Back", p.X, p.Y - 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1244), true);
                    AddTileToChunk(chunkPoint, "Buildings", p.X, p.Y - 2, animatedTiles[mainSheet.Id][184], false);
                }
            }
            if (!tiles.Contains(p + new Point(0, 1)))
            {
                AddTileToChunk(chunkPoint, "Back", p.X, p.Y + 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                if (tiles.Contains(p + new Point(1, 1)))
                {
                    if(!tiles.Contains(p + new Point(1, 2)))
                    { 
                        AddTileToChunk(chunkPoint, "Back", p.X, p.Y + 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X, p.Y + 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 212), false);


                    }
                }
                else if(tiles.Contains(p + new Point(-1, 1)))
                {
                    if (!tiles.Contains(p + new Point(-1, 2)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X, p.Y + 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 1246), true);
                        AddTileToChunk(chunkPoint, "Buildings", p.X, p.Y + 2, new StaticTile(back, mainSheet, BlendMode.Alpha, 213), false);

                    }

                }
                else
                {
                    AddTileToChunk(chunkPoint, "Buildings", p.X, p.Y + 1, new StaticTile(back, mainSheet, BlendMode.Alpha, 259), false);
                }
            }
        }
        private static void AddBlobTileToChunk(BorderTiles border, Tile tile, Layer layer, TileSheet sheet, Point chunkPoint, Point p, List<Point> tiles, Random r)
        {
            AddTileToChunk(chunkPoint, "Back", p.X, p.Y, tile);
            //return;
            if (!tiles.Contains(p + new Point(-1, 0)))
            {

                if (tiles.Contains(p + new Point(-1, 1)))
                {
                    AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.TL));
                    if (!tiles.Contains(p + new Point(-2, 1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 2, p.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.TLC));
                    }
                }
                else if (tiles.Contains(p + new Point(-1, -1)))
                {
                    AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.BL));
                    if (!tiles.Contains(p + new Point(-2, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 2, p.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.BLC));
                    }
                }
                else
                {
                    AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.L));
                    if (!tiles.Contains(p + new Point(0, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y - 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.TLC));
                    }
                    if (!tiles.Contains(p + new Point(0, 1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y + 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.BLC));
                    }
                }
            }
            if (!tiles.Contains(p + new Point(1, 0)))
            {

                if (tiles.Contains(p + new Point(1, 1)))
                {
                    AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.TR));
                    if (!tiles.Contains(p + new Point(2, 1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 2, p.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.TRC));
                    }
                }
                else if (tiles.Contains(p + new Point(1, -1)))
                {
                    AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.BR));
                    if (!tiles.Contains(p + new Point(2, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 2, p.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.BRC));
                    }
                }
                else
                {
                    AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y, new StaticTile(layer, sheet, BlendMode.Alpha, border.R));
                    if (!tiles.Contains(p + new Point(0, -1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y - 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.TRC));
                    }
                    if (!tiles.Contains(p + new Point(0, 1)))
                    {
                        AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y + 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.BRC));
                    }
                }
            }
            if (!tiles.Contains(p + new Point(0, -1)))
            {
                if(!tiles.Contains(p + new Point(-1, -1)) && !tiles.Contains(p + new Point(1, -1)))
                {
                    AddTileToChunk(chunkPoint, "Back", p.X, p.Y - 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.T));
                }
                if (!tiles.Contains(p + new Point(-1, 0)))
                {
                    AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y - 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.TLC));
                }
                else if (!tiles.Contains(p + new Point(1, 0)))
                {
                    AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y - 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.TRC));
                }
            }
            if (!tiles.Contains(p + new Point(0, 1)))
            {
                if(!tiles.Contains(p + new Point(-1, 1)) && !tiles.Contains(p + new Point(1, 1)))
                {
                    AddTileToChunk(chunkPoint, "Back", p.X, p.Y + 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.B));
                }
                if (!tiles.Contains(p + new Point(-1, 0)))
                {
                    AddTileToChunk(chunkPoint, "Back", p.X - 1, p.Y + 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.BLC));
                }
                else if (!tiles.Contains(p + new Point(1, 0)))
                {
                    AddTileToChunk(chunkPoint, "Back", p.X + 1, p.Y + 1, new StaticTile(layer, sheet, BlendMode.Alpha, border.BRC));
                }
            }
        }

        private static Tile GetRandomMeadowTile(Random r, Layer back, TileSheet mainSheet)
        {
            var which = r.NextDouble();

            if (which < 0.01f)
            {
                return animatedTiles[mainSheet.Id][150];
            }
            else if (which < 0.02f)
            {
                return animatedTiles[mainSheet.Id][151];
            }
            else if (which < 0.03f)
            {
                return animatedTiles[mainSheet.Id][152];
            }
            else if (which < 0.04f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 400);
            }
            else if (which < 0.05f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 401);
            }
            else if (which < 0.05f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 254);
            }
            else if (which < 0.06f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 255);
            }
            else
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 175);
            }
        }
        private static Tile GetRandomDirtTile(Random r, Layer back, TileSheet mainSheet)
        {
            var which = r.NextDouble();
            if (which < 0.12f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, r.Choose(181, 488, 207, 614, 206, 560, 564, 513, 463, 610, 589, 227));
            }
            else
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, 227);
            }
        }
        private static Tile GetRandomWaterTile(Random r, Layer back, TileSheet mainSheet)
        {
            var which = r.NextDouble();

            if (which < 0.12f)
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, r.Choose(1247, 1248, 1249, 1272, 1273, 1274, 1297, 1298, 1299, 1322, 1323, 1324));
            }
            else
            {
                return new StaticTile(back, mainSheet, BlendMode.Alpha, r.Choose(1246, 1271));
            }
        }

        private static void AddTileToChunk(Point cp, string layer, int rx, int ry, Tile tile, bool water = false)
        {
            if (water)
            {
                tile.Properties["Water"] = "T";
                waterTiles.Add(GetGlobalTile(cp, rx, ry));
            }
            Point offset = new();
            if (rx < 0)
            {
                offset += new Point(-1, 0);
            }
            else if (rx >= openWorldChunkSize)
            {
                offset += new Point(1, 0);
            }
            if (ry < 0)
            {
                offset += new Point(0, -1);
            }
            else if (ry >= openWorldChunkSize)
            {
                offset += new Point(0, 1);
            }
            cp += offset;

            if (!IsChunkInMap(cp))
                return;
            Point point = new Point(rx - offset.X * openWorldChunkSize, ry - offset.Y * openWorldChunkSize);
            var chunk = CacheChunk(cp, false);
            if (!chunk.tiles.ContainsKey(layer))
            {
                chunk.tiles[layer] = new Tile[openWorldChunkSize, openWorldChunkSize];
            }
            chunk.tiles[layer][point.X, point.Y] = tile;
        }

        private static void AddGrassTilesToChunk(Point cp)
        {
            var layer = openWorldLocation.Map.GetLayer("Back");
            Random r = Utility.CreateRandom(RandomSeed, cp.X * cp.Y + cp.X, 42);
            var mainSheet = openWorldLocation.Map.GetTileSheet("Landscape");
            for (int y = 0; y < openWorldChunkSize; y++)
            {
                for (int x = 0; x < openWorldChunkSize; x++)
                {
                    if (cachedChunks[cp].tiles["Back"][x, y] != null)
                        continue;
                    int idx = 351;
                    var which = r.NextDouble();
                    if (which < 0.01f)
                    {
                        idx = 304;
                    }
                    else if (which < 0.02f)
                    {
                        idx = 305;
                    }
                    else if (which < 0.03f)
                    {
                        idx = 300;
                    }
                    AddTileToChunk(cp, "Back", x, y, new StaticTile(layer, mainSheet, BlendMode.Alpha, idx));
                    
                }
            }
        }

        private static void AddBorderToChunk(Point cp)
        {
            Random r = Utility.CreateRandom(RandomSeed, cp.X + cp.X * cp.Y, "border".GetHashCode());

            var grassTiles = new int[] { 150, 151, 152, 175, 175, 175, 175, 175, 175 };
            var rightTiles = new int[] { 316, 341, 366, 391, 416 };
            var leftTiles = new int[] { 319, 344, 369, 394, 419 };
            var buildLayer = openWorldLocation.Map.GetLayer("Buildings");
            var frontLayer = openWorldLocation.Map.GetLayer("Front");
            var ts = openWorldLocation.Map.GetTileSheet("Landscape");
            var chunk = cachedChunks[cp];
            if (cp.X == 0)
            {
                for (int y = 0; y < openWorldChunkSize; y++)
                {
                    chunk.tiles["Buildings"][0, y] = new StaticTile(buildLayer, ts, BlendMode.Alpha, leftTiles[r.Next(leftTiles.Length)]);
                }
            }
            else if (cp.X == Config.OpenWorldSize / openWorldChunkSize - 1)
            {
                for (int y = 0; y < openWorldChunkSize; y++)
                {
                    chunk.tiles["Buildings"][openWorldChunkSize - 1, y] = new StaticTile(buildLayer, ts, BlendMode.Alpha, rightTiles[r.Next(rightTiles.Length)]);
                }
            }
            if (cp.Y == 0)
            {
                for (int x = 0; x < openWorldChunkSize; x++)
                {
                    chunk.tiles["Buildings"][x, 0] = new StaticTile(buildLayer, ts, BlendMode.Alpha, 217);
                }
            }
            else if (cp.Y == Config.OpenWorldSize / openWorldChunkSize - 1)
            {

                for (int x = 0; x < openWorldChunkSize; x++)
                {
                    if (cp.X == Config.OpenWorldSize / openWorldChunkSize / 2)
                    {
                        var off = x - openWorldChunkSize / 2;
                        if (off == 0 || off == 1)
                        {
                            continue;
                        }
                        else if (off == -1)
                        {
                            chunk.tiles["Front"][x, openWorldChunkSize - 2] = new StaticTile(frontLayer, ts, BlendMode.Alpha, 438);
                            chunk.tiles["Buildings"][x, openWorldChunkSize - 1] = new StaticTile(buildLayer, ts, BlendMode.Alpha, 217);
                            chunk.tiles["Front"][x, openWorldChunkSize - 1] = new StaticTile(frontLayer, ts, BlendMode.Alpha, leftTiles[r.Next(leftTiles.Length)]);
                        }
                        else if (off == 2)
                        {
                            chunk.tiles["Front"][x, openWorldChunkSize - 2] = new StaticTile(frontLayer, ts, BlendMode.Alpha, 439);
                            chunk.tiles["Buildings"][x, openWorldChunkSize - 1] = new StaticTile(buildLayer, ts, BlendMode.Alpha, 217);
                            chunk.tiles["Front"][x, openWorldChunkSize - 1] = new StaticTile(frontLayer, ts, BlendMode.Alpha, rightTiles[r.Next(rightTiles.Length)]);
                        }
                        else
                        {
                            chunk.tiles["Front"][x, openWorldChunkSize - 2] = new StaticTile(buildLayer, ts, BlendMode.Alpha, r.Next(413, 415));
                            chunk.tiles["Buildings"][x, openWorldChunkSize - 1] = new StaticTile(buildLayer, ts, BlendMode.Alpha, 217);
                            chunk.tiles["Front"][x, openWorldChunkSize - 1] = new StaticTile(frontLayer, ts, BlendMode.Alpha, grassTiles[r.Next(grassTiles.Length)]);
                        }
                    }
                    else
                    {
                        chunk.tiles["Front"][x, openWorldChunkSize - 2] = new StaticTile(buildLayer, ts, BlendMode.Alpha, r.Next(413, 415));
                        chunk.tiles["Buildings"][x, openWorldChunkSize - 1] = new StaticTile(buildLayer, ts, BlendMode.Alpha, 217);
                        chunk.tiles["Front"][x, openWorldChunkSize - 1] = new StaticTile(frontLayer, ts, BlendMode.Alpha, grassTiles[r.Next(grassTiles.Length)]);
                    }
                }
            }
        }


        private static void AddRocksToChunk(Point cp)
        {

            Random r = Utility.CreateRandom(RandomSeed, cp.X * cp.Y + cp.X, "rocks".GetHashCode());
            if (!rockCenters.TryGetValue(cp, out var centers))
                return;

            var back = openWorldLocation.Map.GetLayer("Back");
            var mainSheet = openWorldLocation.Map.GetTileSheet("Landscape");

            MethodInfo litter = typeof(MineShaft).GetMethod("createLitterObject", BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var c in centers)
            {
                Point begin = new(0, 0);
                Point end = new(0, 0);
                var level = (int)((Config.OpenWorldSize - c.Y - cp.Y * openWorldChunkSize) / Config.OpenWorldSize * Config.MaxOutcropLevel);
                MineShaft shaft = new(level);
                double gemStoneChance = 0.0015 + 0.0015 * shaft.mineLevel / 10;
                int idx = 0;
                int rocks = r.Next(Config.MinRocksPerOutcrop, Config.MaxRocksPerOutcrop + 1);
                List<Point> tiles = new();
                Rectangle ar = new();
                while (idx < rocks)
                {
                    ar = new Rectangle(ToGlobalTile(cp, c.ToPoint() + begin - new Point(10, 10)), end - begin + new Point(11, 11));
                    if (ConflictsRect(cp, ar))
                    {
                        goto next;
                    }
                    if (end.X > openWorldChunkSize / 10)
                        break;
                    for (int x = begin.X; x <= end.X; x++)
                    {
                        for (int y = begin.Y; y <= end.Y; y++)
                        {
                            if (x != begin.X && x != end.X && y != begin.Y && y != end.Y)
                                continue;
                            Vector2 v = c + new Vector2(x, y);
                            if (r.NextDouble() < Math.Pow(Config.RockDensity, Vector2.Distance(v, c) / 3f))
                            {
                                var offset = GetPointOffset(v.ToPoint());
                                if (offset.X == 0 && offset.Y == 0)
                                {
                                    var av = v + cp.ToVector2() * openWorldChunkSize;

                                    if (!IsOpenTile(av))
                                        continue;
                                    if (!cachedChunks[cp].objects.ContainsKey(av))
                                    {
                                        cachedChunks[cp].objects[av] = (Object)litter.Invoke(shaft, new object[] { 0.001, 5E-05, gemStoneChance, av });
                                        tiles.Add(av.ToPoint() - new Point(cp.X * openWorldChunkSize, cp.Y * openWorldChunkSize));
                                    }
                                }
                                else
                                {
                                    Point ocp = cp + offset;
                                    if (IsChunkInMap(ocp))
                                    {
                                        var av = v + ocp.ToVector2() * openWorldChunkSize;
                                        CacheChunk(ocp, false);
                                        if (!IsOpenTile(av))
                                            continue;
                                        cachedChunks[ocp].objects[av] = (Object)litter.Invoke(shaft, new object[] { 0.001, 5E-05, gemStoneChance, av });
                                        tiles.Add(av.ToPoint() - new Point(ocp.X * openWorldChunkSize, ocp.Y * openWorldChunkSize));
                                    }
                                }
                                idx++;
                                if (idx >= rocks)
                                    goto cont;
                            }
                        }
                    }
                    begin -= new Point(1, 1);
                    end += new Point(1, 1);
                }
            cont:
                var newTiles = MakeBlobFromTiles(r, tiles);
                if (newTiles != null)
                {
                    var padding = GetBlobPadding(newTiles, 4, true);
                    foreach (var rp in padding)
                    {
                        AddBlobTileToChunk(BorderTiles.meadow, GetRandomMeadowTile(r, back, mainSheet), back, mainSheet, cp, rp, padding, r);
                    }
                    padding = GetBlobPadding(newTiles, 2, true);
                    foreach (var rp in padding)
                    {
                        AddBlobTileToChunk(BorderTiles.dirt, GetRandomDirtTile(r, back, mainSheet), back, mainSheet, cp, rp, padding, r);
                    }
                    AddRectToList(ar, outcropRects);
                }
            next:
                continue;
            }
        }


        private static void AddGrassToChunk(Point cp)
        {

            Random r = Utility.CreateRandom(RandomSeed, cp.X * cp.Y + cp.X, "grass".GetHashCode());
            if (!grassCenters.TryGetValue(cp, out var centers))
                return;
            foreach (var c in centers)
            {
                Point begin = new(0, 0);
                Point end = new(0, 0);
                int idx = 0;
                int grass = r.Next(Config.MinGrassPerField, Config.MaxGrassPerField + 1);
                while (idx < grass)
                {
                    if (ConflictsRect(cp, new Rectangle(ToGlobalTile(cp, c + begin.ToVector2()).ToPoint(), end - begin + new Point(1, 1))))
                    {
                        goto next;
                    }
                    for (int x = begin.X; x <= end.X; x++)
                    {
                        for (int y = begin.Y; y <= end.Y; y++)
                        {
                            if (x != begin.X && x != end.X && y != begin.Y && y != end.Y)
                                continue;
                            Vector2 v = c + new Vector2(x, y);
                            if (r.NextDouble() < Math.Pow(Config.GrassDensity, Vector2.Distance(v, c) / 3f))
                            {
                                Point offset = new();
                                if (v.X < 0)
                                {
                                    offset += new Point(-1, 0);
                                }
                                else if (v.X >= openWorldChunkSize)
                                {
                                    offset += new Point(1, 0);
                                }
                                if (v.Y < 0)
                                {
                                    offset += new Point(0, -1);
                                }
                                else if (v.Y >= openWorldChunkSize)
                                {
                                    offset += new Point(0, 1);
                                }
                                if (offset.X == 0 && offset.Y == 0)
                                {
                                    var av = v + cp.ToVector2() * openWorldChunkSize;

                                    if (!IsOpenTile(av))
                                        continue;
                                    if (!cachedChunks[cp].terrainFeatures.ContainsKey(av))
                                        cachedChunks[cp].terrainFeatures[av] = new Grass(1, 3);
                                }
                                else
                                {
                                    Point ocp = cp + offset;
                                    if (IsChunkInMap(ocp))
                                    {
                                        var av = v + ocp.ToVector2() * openWorldChunkSize;
                                        CacheChunk(ocp, false);
                                        if (!IsOpenTile(av))
                                            continue;
                                        cachedChunks[ocp].terrainFeatures[av] = new Grass(1, 3);
                                    }
                                }
                                idx++;
                                if (idx >= grass)
                                    goto next;
                            }
                        }
                    }
                    begin -= new Point(1, 1);
                    end += new Point(1, 1);
                    if (end.X > openWorldChunkSize / 10)
                        break;
                }
            next:
                continue;
            }
        }
        private static void AddPathsObjectsToChunk(Point cp)
        {
            if (!landmarkRects.TryGetValue(cp, out var rects))
                return;
            var chunkRect = new Rectangle(cp.X * openWorldChunkSize, cp.Y * openWorldChunkSize, openWorldChunkSize, openWorldChunkSize);
            foreach (var rect in rects)
            {
                if (chunkRect.Intersects(rect))
                {
                    var startx = Math.Max(0, rect.X - cp.X * openWorldChunkSize);
                    var starty = Math.Max(0, rect.Y - cp.Y * openWorldChunkSize);
                    var endx = Math.Min(openWorldChunkSize, rect.X - cp.X * openWorldChunkSize + rect.Width);
                    var endy = Math.Min(openWorldChunkSize, rect.Y - cp.Y * openWorldChunkSize + rect.Height);
                    for (int x = startx; x < endx; x++)
                    {
                        for (int y = starty; y < endy; y++)
                        {

                            Tile t = cachedChunks[cp].tiles["Paths"][x, y];
                            if (t != null)
                            {
                                Vector2 tile = ToGlobalTile(cp, new Vector2(x , y));
                                string treeId;
                                int? growthStageOnLoad;
                                int? growthStageOnRegrow;
                                bool isFruitTree;
                                if (openWorldLocation.TryGetTreeIdForTile(t, out treeId, out growthStageOnLoad, out growthStageOnRegrow, out isFruitTree))
                                {
                                    if (openWorldLocation.GetFurnitureAt(tile) == null && !openWorldLocation.terrainFeatures.ContainsKey(tile) && !openWorldLocation.objects.ContainsKey(tile))
                                    {
                                        if (isFruitTree)
                                        {
                                            cachedChunks[cp].terrainFeatures.Add(tile, new FruitTree(treeId, growthStageOnLoad.GetValueOrDefault(4)));
                                        }
                                        else
                                        {
                                            cachedChunks[cp].terrainFeatures.Add(tile, new Tree(treeId, growthStageOnLoad.GetValueOrDefault(5), false));
                                        }
                                    }
                                }
                                else
                                {
                                    switch (t.TileIndex)
                                    {
                                        case 13:
                                        case 14:
                                        case 15:
                                            if (!openWorldLocation.objects.ContainsKey(tile) && !Game1.IsWinter)
                                            {
                                                openWorldLocation.objects.Add(tile, ItemRegistry.Create<Object>(GameLocation.getWeedForSeason(Game1.random, openWorldLocation.GetSeason()), 1, 0, false));
                                            }
                                            break;
                                        case 16:
                                            if (!openWorldLocation.objects.ContainsKey(tile))
                                            {
                                                openWorldLocation.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450"), 1, 0, false));
                                            }
                                            break;
                                        case 17:
                                            if (!openWorldLocation.objects.ContainsKey(tile))
                                            {
                                                openWorldLocation.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450"), 1, 0, false));
                                            }
                                            break;
                                        case 18:
                                            if (!openWorldLocation.objects.ContainsKey(tile))
                                            {
                                                openWorldLocation.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)294", "(O)295"), 1, 0, false));
                                            }
                                            break;
                                        case 19:
                                            openWorldLocation.addResourceClumpAndRemoveUnderlyingTerrain(602, 2, 2, tile);
                                            break;
                                        case 20:
                                            openWorldLocation.addResourceClumpAndRemoveUnderlyingTerrain(672, 2, 2, tile);
                                            break;
                                        case 21:
                                            openWorldLocation.addResourceClumpAndRemoveUnderlyingTerrain(600, 2, 2, tile);
                                            break;
                                        case 22:
                                        case 36:
                                            if (!openWorldLocation.terrainFeatures.ContainsKey(tile))
                                            {
                                                Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);
                                                tileRect.Inflate(-1, -1);
                                                bool fail = false;
                                                using (List<ResourceClump>.Enumerator enumerator = openWorldLocation.resourceClumps.GetEnumerator())
                                                {
                                                    while (enumerator.MoveNext())
                                                    {
                                                        if (enumerator.Current.getBoundingBox().Intersects(tileRect))
                                                        {
                                                            fail = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                                if (!fail)
                                                {
                                                    openWorldLocation.terrainFeatures.Add(tile, new Grass((t.TileIndex == 36) ? 7 : 1, 3));
                                                }
                                            }
                                            break;
                                        case 23:
                                            if (!openWorldLocation.terrainFeatures.ContainsKey(tile))
                                            {
                                                openWorldLocation.terrainFeatures.Add(tile, new Tree(Game1.random.Next(1, 4).ToString(), Game1.random.Next(2, 4), false));
                                            }
                                            break;
                                        case 24:
                                            if (!openWorldLocation.terrainFeatures.ContainsKey(tile))
                                            {
                                                openWorldLocation.largeTerrainFeatures.Add(new Bush(tile, 2, openWorldLocation, -1));
                                            }
                                            break;
                                        case 25:
                                            if (!openWorldLocation.terrainFeatures.ContainsKey(tile))
                                            {
                                                openWorldLocation.largeTerrainFeatures.Add(new Bush(tile, 1, openWorldLocation, -1));
                                            }
                                            break;
                                        case 26:
                                            if (!openWorldLocation.terrainFeatures.ContainsKey(tile))
                                            {
                                                openWorldLocation.largeTerrainFeatures.Add(new Bush(tile, 0, openWorldLocation, -1));
                                            }
                                            break;
                                        case 27:
                                            ChangeMapProperties("BrookSounds", tile.X.ToString() + " " + tile.Y.ToString() + " 0");
                                            break;
                                        case 29:
                                        case 30:
                                            {
                                                string rawOrder;
                                                if (Game1.startingCabins > 0 && t.Properties.TryGetValue("Order", out rawOrder) && int.Parse(rawOrder) <= Game1.startingCabins && ((t.TileIndex == 29 && !Game1.cabinsSeparate) || (t.TileIndex == 30 && Game1.cabinsSeparate)))
                                                {
                                                    AccessTools.FieldRefAccess<GameLocation, List<Vector2>>(openWorldLocation, "_startingCabinLocations").Add(tile);
                                                }
                                                break;
                                            }
                                        case 33:
                                            if (!openWorldLocation.terrainFeatures.ContainsKey(tile))
                                            {
                                                openWorldLocation.largeTerrainFeatures.Add(new Bush(tile, 4, openWorldLocation, -1));
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                    }

                }
            }
        }
        public static void AddChestsToChunk(Point cp)
        {
            Stopwatch s = Stopwatch.StartNew();
            if (advancedLootFrameworkApi is null)
                return;
            Random r = Utility.CreateRandom(RandomSeed, cp.X + cp.X * cp.Y, "chests".GetHashCode());

            int count = (int)Math.Floor(openWorldChunkSize * openWorldChunkSize / (float)(Config.TilesChestMin + ((Config.TilesChestMax - Config.TilesChestMin) * r.NextDouble() * cp.Y * openWorldChunkSize / Config.OpenWorldSize ))); 
            int i = 0;
            int attempt = 0;
            while (i < count && attempt < count * 10)
            {
                Vector2 freeTile = new(r.Next(openWorldChunkSize), r.Next(openWorldChunkSize));
                var av = ToGlobalTile(cp, freeTile);
                if (IsInsideRect(cp, av, landmarkRects))
                {
                    goto next;
                }
                if (IsOpenTile(av))
                {
                    float distance = (Config.OpenWorldSize - av.Y) / Config.OpenWorldSize;
                    double fraction = Math.Min(0.99, Math.Max(0, distance + (r.NextDouble() - 0.5 - (1 - Config.ChestRarityBias))));
                    int level = (int)Math.Ceiling(fraction * Config.OpenWorldSize / openWorldChunkSize);
                    Chest chest = advancedLootFrameworkApi.MakeChest(treasuresList, Config.ItemListChances, Config.ChestMaxItems, Config.ChestMinItemValue, Config.ChestMaxItemValue, level, Config.ChestValueIncreaseRate, Config.ChestItemsBaseMaxValue, freeTile);
                    chest.CanBeGrabbed = false;
                    chest.playerChoiceColor.Value = MakeTint(fraction);
                    chest.modData.Add(modKey, "T");
                    chest.modData.Add(modCoinKey, advancedLootFrameworkApi.GetChestCoins(level, Config.ChestValueIncreaseRate, Config.CoinBaseMin, Config.CoinBaseMax).ToString());
                    cachedChunks[cp].overlayObjects[av] = chest;
                    i++;
                }
            next:
                attempt++;
            }
        }
        public static void AddBushesToChunk(Point cp)
        {
            Random r = Utility.CreateRandom(RandomSeed, cp.X + cp.X * cp.Y, "bush".GetHashCode());
            int count = (int)Math.Floor(openWorldChunkSize * openWorldChunkSize / (float)(Config.TilesBushMin + ((Config.TilesBushMax - Config.TilesBushMin) * r.NextDouble() ))); 
            int i = 0;
            int attempt = 0;
            while (i < count && attempt < count * 10)
            {
                Vector2 freeTile = new(r.Next(openWorldChunkSize), r.Next(openWorldChunkSize));
                var av = ToGlobalTile(cp, freeTile);
                if (IsInsideRect(cp, av, landmarkRects))
                {
                    goto next;
                }
                if (IsOpenTile(av))
                {
                    Bush bush = new Bush(av, r.Next(3), openWorldLocation);
                    bush.modData[modChunkKey] = $"{cp.X},{cp.Y}";
                    cachedChunks[cp].largeTerrainFeatures[av] = bush;
                    i++;
                }
            next:
                attempt++;
            }
        }
        public static void AddArtifactsToChunk(Point cp)
        {
            Random r = Utility.CreateRandom(RandomSeed, cp.X + cp.X * cp.Y, "artifact".GetHashCode());
            int count = (int)Math.Floor(openWorldChunkSize * openWorldChunkSize / (float)(Config.TilesArtifactMin + ((Config.TilesArtifactMax - Config.TilesBushMin) * r.NextDouble() * cp.Y * openWorldChunkSize / Config.OpenWorldSize ))); 
            int i = 0;
            int attempt = 0;
            while (i < count && attempt < count * 10)
            {
                Vector2 freeTile = new(r.Next(openWorldChunkSize), r.Next(openWorldChunkSize));
                var av = ToGlobalTile(cp, freeTile);
                if (IsInsideRect(cp, av, landmarkRects))
                {
                    goto next;
                }
                if (IsOpenTile(av))
                {
                    Object obj = ItemRegistry.Create<Object>("(O)590");

                    cachedChunks[cp].objects[av] = obj;
                    i++;
                }
            next:
                attempt++;
            }
        }
        public static void AddForageToChunk(Point cp)
        {
            Random r = Utility.CreateRandom(RandomSeed, cp.X + cp.X * cp.Y, "forage".GetHashCode());
            int count = (int)Math.Floor(openWorldChunkSize * openWorldChunkSize / (float)(Config.TilesForageMin + ((Config.TilesForageMax - Config.TilesForageMin) * r.NextDouble()))); 
            int i = 0;
            int attempt = 0;
            var forest = Game1.getLocationFromName("Forest");
            LocationData data = forest.GetData();
            List<SpawnForageData> possibleForage = new List<SpawnForageData>();
            foreach (SpawnForageData spawn in GameLocation.GetData("Default").Forage.Concat(data.Forage))
            {
                if (spawn.Condition == null || GameStateQuery.CheckConditions(spawn.Condition, forest, null, null, null, r, null))
                {
                    if (spawn.Season != null)
                    {
                        Season? season2 = spawn.Season;
                        Season season3 = forest.GetSeason();
                        if (!((season2.GetValueOrDefault() == season3) & (season2 != null)))
                        {
                            continue;
                        }
                    }
                    possibleForage.Add(spawn);
                }
            }
            if (!possibleForage.Any())
                return;
            ItemQueryContext itemQueryContext = new ItemQueryContext(forest, null, r, "location '" + forest.NameOrUniqueName + "' > forage");

            while (i < count && attempt < count * 10)
            {
                Vector2 freeTile = new(r.Next(openWorldChunkSize), r.Next(openWorldChunkSize));
                var av = ToGlobalTile(cp, freeTile);
                if(IsInsideRect(cp, av, landmarkRects))
                {
                    goto next;
                }
                if (IsOpenTile(av))
                {
                    SpawnForageData forage = r.ChooseFrom(possibleForage);
                    Item forageItem = ItemQueryResolver.TryResolveRandomItem(forage, itemQueryContext, false, null, null, null, delegate (string query, string error)
                    {
                    });
                    if (forageItem != null)
                    {
                        Object forageObj = forageItem as Object;
                        if (forageObj != null)
                        {
                            forageObj.IsSpawnedObject = true;
                            cachedChunks[cp].objects[av] = forageObj;
                            i++;
                        }
                    }
                }
            next:
                attempt++;
            }
        }


        public static void AddClumpsToChunk(Point cp)
        {
            Random r = Utility.CreateRandom(RandomSeed, cp.X + cp.X * cp.Y, "clump".GetHashCode());
            int count = (int)Math.Floor(openWorldChunkSize * openWorldChunkSize / (float)(Config.TilesClumpMin + ((Config.TilesClumpMax - Config.TilesClumpMin) * r.NextDouble() ))); 
            int i = 0;
            int attempt = 0;
            while (i < count && attempt < count * 10)
            {
                Vector2 freeTile = new(r.Next(openWorldChunkSize), r.Next(openWorldChunkSize));
                var av = ToGlobalTile(cp, freeTile);
                if (IsInsideRect(cp, av, landmarkRects))
                {
                    goto next;
                }
                if (IsOpenTile(av) && IsOpenTile(av + new Vector2(0, 1)) && IsOpenTile(av + new Vector2(1, 0)) && IsOpenTile(av + new Vector2(1, 1)))
                {
                    string sheet = null;
                    int which = 0;
                    int? health = null;
                    double roll = r.NextDouble() + (1 - av.Y / Config.OpenWorldSize) / 4;
                    if(roll < 0.6)
                    {
                        if(r.NextDouble() < 0.3)
                        {
                            which = r.Choose(44, 46);
                            sheet = "TileSheets\\Objects_2";
                            health = 4;
                        }
                        else
                            which = r.Choose(600, 602, 752, 754);
                    }
                    else if(roll < 0.95)
                    {
                        which = r.Choose(756, 758);
                    }
                    else
                    {
                        which = 622;
                    }
                    if(which != 0)
                    {

                        cachedChunks[cp].resourceClumps[av] = new ResourceClump(which, 2, 2, av, health, sheet);
                    }
                }
            next:
                attempt++;
            }
        }

        private static void AddTreesToChunk(Point cp)
        {

            Random r = Utility.CreateRandom(RandomSeed, cp.X * cp.Y + cp.X, "trees".GetHashCode());
            if (!treeCenters.TryGetValue(cp, out var centers))
                return;
            foreach (var c in centers)
            {
                Point begin = new(0, 0);
                Point end = new(0, 0);
                int idx = 0;
                int trees = r.Next(Config.MinTreesPerForest, Config.MaxTreesPerForest + 1);
                while (idx < trees)
                {
                    if (ConflictsRect(cp, new Rectangle(ToGlobalTile(cp, c.Key.ToPoint() + begin), end - begin + new Point(1, 1))))
                    {
                        goto next;
                    }
                    for (int x = begin.X; x <= end.X; x++)
                    {
                        for (int y = begin.Y; y <= end.Y; y++)
                        {
                            if (x != begin.X && x != end.X && y != begin.Y && y != end.Y)
                                continue;
                            Vector2 v = c.Key + new Vector2(x, y);
                            if (r.NextDouble() < Math.Pow(Config.TreeDensity, Vector2.Distance(v, c.Key) / 3f))
                            {
                                Point offset = new();
                                if (v.X < 0)
                                {
                                    offset += new Point(-1, 0);
                                }
                                else if (v.X >= openWorldChunkSize)
                                {
                                    offset += new Point(1, 0);
                                }
                                if (v.Y < 0)
                                {
                                    offset += new Point(0, -1);
                                }
                                else if (v.Y >= openWorldChunkSize)
                                {
                                    offset += new Point(0, 1);
                                }
                                if (offset.X == 0 && offset.Y == 0)
                                {
                                    var av = v + cp.ToVector2() * openWorldChunkSize;
                                    if (!IsOpenTile(av))
                                        continue;
                                    if (!cachedChunks[cp].terrainFeatures.ContainsKey(av))
                                        cachedChunks[cp].terrainFeatures[av] = new Tree(GetRandomTree(av, r, c.Value), r.NextDouble() < 0.2 ? 4 : 5);
                                }
                                else
                                {
                                    Point ocp = cp + offset;
                                    if (IsChunkInMap(ocp))
                                    {
                                        var av = v + ocp.ToVector2() * openWorldChunkSize;
                                        CacheChunk(ocp, false);
                                        if (!IsOpenTile(av))
                                            continue;
                                        cachedChunks[ocp].terrainFeatures[av] = new Tree(GetRandomTree(av, r, c.Value), r.NextDouble() < 0.2 ? 4 : 5);
                                    }
                                }
                                idx++;
                                if (idx >= trees)
                                    goto next;
                            }
                        }
                    }
                    begin -= new Point(1, 1);
                    end += new Point(1, 1);
                    if (end.X > openWorldChunkSize / 5)
                        break;
                }
            next:
                continue;
            }
        }

        private static string GetRandomTree(Vector2 p, Random r, string which = null)
        {
            float distance = (Config.OpenWorldSize - p.Y) / Config.OpenWorldSize;
            Dictionary<string, double> chances = new()
            {
                { "1", Math.Max(0, 1 - distance) },
                { "3", Math.Pow(distance, 2) },
                { "6", Math.Max(0, 1 - Math.Pow(distance, 2) * 2) },
                { "7", Math.Max(0, 1 - Math.Pow(distance, 2) * 2) / 2 },
                { "8", Math.Pow(distance, 2) / 2 },
                { "9", Math.Max(0, 1 - Math.Pow(distance, 2) * 2) }
            };
            double total = 0;
            foreach (var key in chances.Keys.ToArray())
            {
                if (which == key)
                    chances[key] *= 8;
                total += chances[key];
            }
            double roll = r.NextDouble();
            double count = 0;
            foreach (var kvp in chances)
            {
                count += kvp.Value / total;
                if (roll < count)
                {
                    return kvp.Key;
                }
            }
            return "1";
        }
        private Dictionary<string, MonsterSpawnInfo> GetMonsterDict()
        {
            if (File.Exists(Path.Combine(SHelper.DirectoryPath, "monsters.json")))
            {
                return JsonConvert.DeserializeObject<Dictionary<string, MonsterSpawnInfo>>(File.ReadAllText(Path.Combine(SHelper.DirectoryPath, "monsters.json")));
            }
            var dict = new Dictionary<string, MonsterSpawnInfo>();
            dict["Slimes"] = new MonsterSpawnInfo()
            {
                Chance = 1,
                Monsters = new List<MonsterInfo>()
                    {
                        new MonsterInfo()
                        {
                            Type = "GreenSlime",
                            Chance = 1,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 3,
                            Max = 10
                        },
                        new MonsterInfo()
                        {
                            Type = "BigSlime",
                            Chance = 0.5,
                            MinLevel = 1,
                            MaxLevel = 150,
                            Min = 1,
                            Max = 4
                        }
                    }
            };
            dict["Dinos"] = new MonsterSpawnInfo()
            {
                Chance = 0.5,
                Difficulty = 0.3f,
                Monsters = new List<MonsterInfo>()
                    {
                        new MonsterInfo()
                        {
                            Type = "Dino",
                            Chance = 1,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 3,
                            Max = 10
                        }
                    }
            };
            dict["Serpents"] = new MonsterSpawnInfo()
            {
                Chance = 0.3,
                Difficulty = 0.7f,
                Monsters = new List<MonsterInfo>()
                    {
                        new MonsterInfo()
                        {
                            Type = "Serpent",
                            Chance = 1,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 3,
                            Max = 10
                        }
                    }
            };
            dict["RockCrabs"] = new MonsterSpawnInfo()
            {
                Chance = 0.7,
                Difficulty = 0.1f,
                Monsters = new List<MonsterInfo>()
                    {
                        new MonsterInfo()
                        {
                            Type = "RockCrab",
                            Chance = 1,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 1,
                            Max = 1
                        }
                    }
            };
            dict["RockGolem"] = new MonsterSpawnInfo()
            {
                Chance = 0.7,
                Difficulty = 0.4f,
                Monsters = new List<MonsterInfo>()
                    {
                        new MonsterInfo()
                        {
                            Type = "RockCrab",
                            Chance = 1,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 1,
                            Max = 3
                        }
                    }
            };
            dict["ShadowTribe"] = new MonsterSpawnInfo()
            {
                Chance = 0.7,
                Difficulty = 0.6f,
                Monsters = new List<MonsterInfo>()
                    {
                        new MonsterInfo()
                        {
                            Type = "ShadowGirl",
                            Chance = 1,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 1,
                            Max = 3
                        },
                        new MonsterInfo()
                        {
                            Type = "ShadowGuy",
                            Chance = 1,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 1,
                            Max = 3
                        },
                        new MonsterInfo()
                        {
                            Type = "ShadowBrute",
                            Chance = 0.5,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 1,
                            Max = 2
                        },
                        new MonsterInfo()
                        {
                            Type = "ShadowShaman",
                            Chance = 0.5,
                            MinLevel = 1,
                            MaxLevel = 100,
                            Min = 1,
                            Max = 1
                        }
                    }
            };
            File.WriteAllText(Path.Combine(SHelper.DirectoryPath, "monsters.json"), JsonConvert.SerializeObject(dict, Formatting.Indented));
            return dict;
        }

        private static string GetRandomMonsterSpawn(Vector2 ap, Random r, string which = null)
        {
            var monsterDict = SHelper.GameContent.Load<Dictionary<string, MonsterSpawnInfo>>(monsterDictPath);

            Dictionary<string, double> chances = new();
            double total = 0;
            foreach (var m in monsterDict.Values)
            {
                var diffMod = m.Difficulty * (ap.Y / Config.OpenWorldSize);
                var chance = Math.Max(0, m.Chance - diffMod);
                total += chance;
            }
            double roll = r.NextDouble();
            double count = 0;
            foreach (var m in monsterDict)
            {
                var diffMod = m.Value.Difficulty * (ap.Y / Config.OpenWorldSize);
                var chance = Math.Max(0, m.Value.Chance - diffMod);
                count += chance / total;
                if (roll < count)
                {
                    return m.Key;
                }
            }
            return null;
        }
        private static void AddMonstersToChunk(Point cp)
        {
            var monsterDict = SHelper.GameContent.Load<Dictionary<string, MonsterSpawnInfo>>(monsterDictPath);

            Random r = Utility.CreateRandom(RandomSeed, cp.X * cp.Y + cp.X, "monsters".GetHashCode());

            if (!monsterCenters.TryGetValue(cp, out var mcs))
                return;
            foreach (var mc in mcs)
            {
                var mi = monsterDict[mc.Value];

                List<MonsterSpawn> monsters = new();
                foreach (var m in mi.Monsters)
                {
                    if (r.NextDouble() > m.Chance)
                        continue;
                    int amount = r.Next(m.Min, m.Max + 1);
                    if (amount == 0)
                        continue;
                    for (int i = 0; i < amount; i++)
                    {
                        monsters.Add(new() { Type = m.Type, Level = r.Next(m.MinLevel, m.MaxLevel + 1) });
                    }
                }
                if (!monsters.Any())
                    return;
                Point begin = new(0, 0);
                Point end = new(0, 0);
                int idx = 0;
                double maxDistance = openWorldChunkSize / 2;
                while (idx < monsters.Count)
                {
                    var ar = ToGlobalRect(cp, new Rectangle(begin, end - begin + new Point(1, 1)));
                    if (ConflictsRect(cp, ar))
                    {
                        goto next;
                    }
                    for (int x = begin.X; x <= end.X; x++)
                    {
                        for (int y = begin.Y; y <= end.Y; y++)
                        {
                            if (x != begin.X && x != end.X && y != begin.Y && y != end.Y)
                                continue;
                            Vector2 v = mc.Key + new Vector2(x, y);
                            if (r.NextDouble() < (Config.MonsterDensity / 10) /(Vector2.Distance(v, mc.Key) / maxDistance))
                            {
                                Point offset = new();
                                if (v.X < 0)
                                {
                                    offset += new Point(-1, 0);
                                }
                                else if (v.X >= openWorldChunkSize)
                                {
                                    offset += new Point(1, 0);
                                }
                                if (v.Y < 0)
                                {
                                    offset += new Point(0, -1);
                                }
                                else if (v.Y >= openWorldChunkSize)
                                {
                                    offset += new Point(0, 1);
                                }
                                if (offset.X == 0 && offset.Y == 0)
                                {
                                    var av = v + cp.ToVector2() * openWorldChunkSize;
                                    if (!IsOpenTile(av))
                                        continue;
                                    cachedChunks[cp].monsters[av] = monsters[idx];
                                }
                                else
                                {
                                    int num = Config.OpenWorldSize / openWorldChunkSize;
                                    Point ocp = cp + offset;
                                    if (ocp.X >= 0 && ocp.X < num && ocp.Y >= 0 && ocp.Y < num)
                                    {
                                        var av = v + ocp.ToVector2() * openWorldChunkSize;
                                        CacheChunk(ocp, false);
                                        if (!IsOpenTile(av))
                                            continue;
                                        cachedChunks[ocp].monsters[av] = monsters[idx];
                                    }
                                }
                                idx++;
                                if (idx >= monsters.Count)
                                    goto next;
                            }
                        }
                    }
                    begin -= new Point(1, 1);
                    end += new Point(1, 1);
                    if (end.X > openWorldChunkSize / 10)
                        break;
                }
            next:
                continue;
            }
        }
        private static void AddTerrainFeaturesFromChunk(Point pc)
        {
            var chunk = cachedChunks[pc];
            foreach (var t in chunk.terrainFeatures)
            {
                if (IsOpenTile(t.Key))
                {
                    openWorldLocation.terrainFeatures[t.Key] = t.Value;
                }
            }
            foreach (var t in chunk.largeTerrainFeatures)
            {
                if (IsOpenTile(t.Key))
                {
                    openWorldLocation.largeTerrainFeatures.Add(t.Value);
                }
            }
            foreach (var t in chunk.resourceClumps)
            {
                if (IsOpenTile(t.Key))
                {
                    openWorldLocation.resourceClumps.Add(t.Value);
                }
            }
        }
        private static void AddObjectsFromChunk(Point pc)
        {
            var chunk = cachedChunks[pc];
            foreach (var t in chunk.objects)
            {
                if (IsOpenTile(t.Key))
                {
                    openWorldLocation.objects[t.Key] = t.Value;
                }
            }
            foreach (var t in chunk.overlayObjects)
            {
                if (IsOpenTile(t.Key))
                {
                    openWorldLocation.overlayObjects[t.Key] = t.Value;
                }
            }
        }

        private static void AddMonstersFromChunk(Point pc)
        {
            var chunk = cachedChunks[pc];
            SMonitor.Log($"chunk {pc} has {chunk.monsters.Count} monsters");
            foreach (var t in chunk.monsters)
            {
                if (IsOpenTile(t.Key))
                {
                    Monster m = null;
                    switch (t.Value.Type)
                    {
                        case "GreenSlime":
                            m = new GreenSlime(t.Key * 64, t.Value.Level) { wildernessFarmMonster = true };
                            break;
                        case "BigSlime":
                            int mineArea = t.Value.Level;
                            if (mineArea < 121 && mineArea > 39)
                            {
                                mineArea = mineArea / 40 * 40;
                            }
                            else if (mineArea > 25)
                            {
                                mineArea = 40;
                            }
                            m = new BigSlime(t.Key * 64, mineArea) { wildernessFarmMonster = true };
                            break;
                        case "Dino":
                            m = new DinoMonster(t.Key * 64) { wildernessFarmMonster = true };
                            break;
                        case "BlueSquid":
                            m = new BlueSquid(t.Key * 64) { wildernessFarmMonster = true };
                            break;
                        case "RockCrab":
                            m = new RockCrab(t.Key * 64);
                            break;
                        case "RockGolem":
                            m = new RockGolem(t.Key * 64, t.Value.Level);
                            break;
                        case "Serpent":
                            m = new Serpent(t.Key * 64);
                            break;
                        case "ShadowBrute":
                            m = new ShadowBrute(t.Key * 64)
                            {
                                wildernessFarmMonster = true
                            };
                            break;
                        case "ShadowShaman":
                            m = new ShadowShaman(t.Key * 64)
                            {
                                wildernessFarmMonster = true
                            };
                            break;
                        case "ShadowGuy":
                            m = new ShadowBrute(t.Key * 64) { wildernessFarmMonster = true };
                            break;
                        case "ShadowGirl":
                            m = new ShadowGirl(t.Key * 64) { wildernessFarmMonster = true };
                            break;
                    }
                    if (m is not null)
                    {
                        m.modData[modKey] = $"{pc.X},{pc.Y}";
                        openWorldLocation.characters.Add(m);
                    }
                }
            }
        }

        public static void UpdateTreasuresList()
        {
            treasuresList = advancedLootFrameworkApi.LoadPossibleTreasures(Config.ItemListChances.Where(p => p.Value > 0).ToDictionary(s => s.Key, s => s.Value).Keys.ToArray(), Config.ChestMinItemValue, Config.ChestMaxItemValue);
        }
   }
}