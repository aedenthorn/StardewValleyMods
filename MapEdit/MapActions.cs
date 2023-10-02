using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile;
using xTile.Tiles;

namespace MapEdit
{
    public partial class ModEntry
    {
        public static void GetMapCollectionData()
        {
            string modPath = SHelper.DirectoryPath;
            mapCollectionData = new MapCollectionData();

            var mapEdits = new List<MapCollectionData>();

            if (Config.IncludeGlobalEdits && File.Exists(Path.Combine(modPath, "map_data.json")))
            {
                mapEdits.Add(GetMapEdits("map_data.json"));
            }

            if (Directory.Exists(Path.Combine(SHelper.DirectoryPath, "custom")))
            {
                foreach (string file in Directory.GetFiles(Path.Combine(SHelper.DirectoryPath, "custom"), "*.json"))
                {
                    mapEdits.Add(GetMapEdits(Path.Combine("custom", Path.GetFileName(file))));
                }
            }

            if (Config.UseSaveSpecificEdits)
            {
                if (!Directory.Exists(Path.Combine(modPath, "data")))
                    Directory.CreateDirectory(Path.Combine(modPath, "data"));
                mapEdits.Add(GetMapEdits(Path.Combine("data", Constants.SaveFolderName + "_map_data.json")));
            }

            foreach(MapCollectionData data in mapEdits)
            {
                foreach(var map in data.mapDataDict)
                {
                    if (!mapCollectionData.mapDataDict.ContainsKey(map.Key))
                    {
                        mapCollectionData.mapDataDict[map.Key] = map.Value;
                    }
                    else
                    {
                        foreach(var tile in map.Value.tileDataDict)
                        {
                            mapCollectionData.mapDataDict[map.Key].tileDataDict[tile.Key] = tile.Value;
                        }
                    }
                }
            }

            SMonitor.Log($"Loaded map data for {mapCollectionData.mapDataDict.Count} maps");
        }

        private static MapCollectionData GetMapEdits(string path)
        {
            SMonitor.Log($"Loading map data from file {path}");
            MapCollectionData collection = SHelper.Data.ReadJsonFile<MapCollectionData>(path) ?? new MapCollectionData();
            return collection;
        }

        public static void SaveMapData(string path, MapCollectionData collection)
        {
            SHelper.Data.WriteJsonFile(path, collection);
            SMonitor.Log($"Saved edits to file {path}");
        }

        public static void RevertCurrentMap()
        {
            var mapName = Game1.player.currentLocation.mapPath.Value.Replace("Maps\\", "");
            pastedTileLoc.Value = new Vector2(-1, -1);
            mapCollectionData.mapDataDict.Remove(mapName);

            string modPath = SHelper.DirectoryPath;
            if (Config.IncludeGlobalEdits)
            {
                var mapData = GetMapEdits("map_data.json");
                if (mapData.mapDataDict.ContainsKey(mapName)) // only edit if custom file contains this
                {
                    mapData.mapDataDict.Remove(mapName);
                    SaveMapData(Path.Combine("map_data.json"), mapData);
                }
            }

            if (Directory.Exists(Path.Combine(modPath, "custom")))
            {
                foreach (string file in Directory.GetFiles(Path.Combine(modPath, "custom"), "*.json"))
                {
                    string relPath = Path.Combine("custom", Path.GetFileName(file));
                    var mapData = GetMapEdits(relPath);

                    if (mapData.mapDataDict.ContainsKey(mapName)) // only edit if custom file contains this
                    {
                        mapData.mapDataDict.Remove(mapName);
                        SaveMapData(relPath, mapData);
                    }
                }
            }

            if (Config.UseSaveSpecificEdits)
            {
                string relPath = Path.Combine("data", Constants.SaveFolderName + "_map_data.json");
                var mapData = new MapCollectionData();

                if (!Directory.Exists(Path.Combine(modPath, "data")))
                    Directory.CreateDirectory(Path.Combine(modPath, "data"));
                else
                    mapData = GetMapEdits(relPath);
                if (mapData.mapDataDict.ContainsKey(mapName)) // only edit if custom file contains this
                {
                    mapData.mapDataDict.Remove(mapName);
                    SaveMapData(relPath, mapData);
                }
            }
            cleanMaps.Remove(mapName); 
            UpdateCurrentMap(true);
        }

        public static void SaveMapTile(string mapName, Vector2 tileLoc, TileLayers tile)
        {
            if (tile == null)
            {
                mapCollectionData.mapDataDict[mapName].tileDataDict.Remove(tileLoc);
                if (mapCollectionData.mapDataDict[mapName].tileDataDict.Count == 0)
                    mapCollectionData.mapDataDict.Remove(mapName);
            }
            else
            {
                if (!mapCollectionData.mapDataDict.ContainsKey(mapName))
                    mapCollectionData.mapDataDict[mapName] = new MapData();

                mapCollectionData.mapDataDict[mapName].tileDataDict[Game1.currentCursorTile] = new TileLayers(currentTileDict.Value);
            }

            string modPath = SHelper.DirectoryPath;
            if (Config.IncludeGlobalEdits)
            {
                var mapData = GetMapEdits("map_data.json");
                if (tile == null)
                {
                    if (mapData.mapDataDict.ContainsKey(mapName) && mapData.mapDataDict[mapName].tileDataDict.ContainsKey(tileLoc))
                    {
                        mapData.mapDataDict[mapName].tileDataDict.Remove(tileLoc);
                        if (mapData.mapDataDict[mapName].tileDataDict.Count == 0)
                            mapData.mapDataDict.Remove(mapName);
                        SaveMapData(Path.Combine("map_data.json"), mapData);
                    }
                }
                else if (!Config.UseSaveSpecificEdits || (mapData.mapDataDict.ContainsKey(mapName) && mapData.mapDataDict[mapName].tileDataDict.ContainsKey(tileLoc))) // save new to global if not using save specific
                {
                    if (!mapData.mapDataDict.ContainsKey(mapName))
                        mapData.mapDataDict[mapName] = new MapData();

                    mapData.mapDataDict[mapName].tileDataDict[tileLoc] = tile;
                    SaveMapData(Path.Combine("map_data.json"), mapData);
                }
            }

            if (Directory.Exists(Path.Combine(modPath, "custom")))
            {
                foreach (string file in Directory.GetFiles(Path.Combine(modPath, "custom"), "*.json"))
                {
                    string relPath = Path.Combine("custom", Path.GetFileName(file));
                    var mapData = GetMapEdits(relPath);

                    if (mapData.mapDataDict.ContainsKey(mapName) && mapData.mapDataDict[mapName].tileDataDict.ContainsKey(tileLoc)) // only edit if custom file contains this
                    {
                        if (tile == null)
                        {
                            mapData.mapDataDict[mapName].tileDataDict.Remove(tileLoc);
                            if (mapData.mapDataDict[mapName].tileDataDict.Count == 0)
                                mapData.mapDataDict.Remove(mapName);
                        }
                        else
                        {
                            mapData.mapDataDict[mapName].tileDataDict[tileLoc] = tile;
                        }
                        SaveMapData(relPath, mapData);
                    }
                }
            }

            if (Config.UseSaveSpecificEdits)
            {
                string relPath = Path.Combine("data", Constants.SaveFolderName + "_map_data.json");
                var mapData = new MapCollectionData();

                if (!Directory.Exists(Path.Combine(modPath, "data")))
                    Directory.CreateDirectory(Path.Combine(modPath, "data"));
                else
                    mapData = GetMapEdits(relPath);

                if (tile == null)
                {
                    if (mapData.mapDataDict.ContainsKey(mapName) && mapData.mapDataDict[mapName].tileDataDict.ContainsKey(tileLoc))
                    {
                        mapData.mapDataDict[mapName].tileDataDict.Remove(tileLoc);
                        if (mapData.mapDataDict[mapName].tileDataDict.Count == 0)
                            mapData.mapDataDict.Remove(mapName);
                    }
                }
                else // always save new to save specific
                {
                    if (!mapData.mapDataDict.ContainsKey(mapName))
                        mapData.mapDataDict[mapName] = new MapData();
                    mapData.mapDataDict[mapName].tileDataDict[tileLoc] = tile;
                }
                SaveMapData(relPath, mapData);
            }
            cleanMaps.Remove(mapName);
        }
        public static void AddTilesheet(TileSheet tileSheet, string mapName)
        {
            string modPath = SHelper.DirectoryPath;
            string dataPath = "map_data.json";
            if (Config.UseSaveSpecificEdits)
            {
                dataPath = Path.Combine("data", Constants.SaveFolderName + "_map_data.json");

                if (!Directory.Exists(Path.Combine(modPath, "data")))
                    Directory.CreateDirectory(Path.Combine(modPath, "data"));
            }
            var mapsData = GetMapEdits(dataPath);
            if (!mapsData.mapDataDict.ContainsKey(mapName))
            {
                mapsData.mapDataDict[mapName] = new MapData();
            }
            if (mapsData.mapDataDict[mapName].customSheets is null)
            {
                mapsData.mapDataDict[mapName].customSheets = new();
            }
            if (mapsData.mapDataDict[mapName].customSheets.Values.FirstOrDefault(v => v.path == tileSheet.ImageSource) == null)
            {
                mapsData.mapDataDict[mapName].customSheets[tileSheet.Id] = new()
                {
                    path = tileSheet.ImageSource,
                    width = tileSheet.SheetWidth, 
                    height = tileSheet.SheetHeight
                };
            }
            mapCollectionData = mapsData;

            SaveMapData(dataPath, mapsData);
            cleanMaps.Remove(mapName);
            UpdateCurrentMap(false);
            Game1.playSound(Config.PasteSound);
            SMonitor.Log($"Added tilesheet {tileSheet.Id} to {mapName}");
        }
        
        public static void RemoveTilesheet(string sheetName, string mapName)
        {
            mapCollectionData.mapDataDict[mapName].customSheets.Remove(sheetName);

            string modPath = SHelper.DirectoryPath;
            if (Config.IncludeGlobalEdits)
            {
                var mapData = GetMapEdits("map_data.json");

                if (mapData.mapDataDict.TryGetValue(mapName, out var data))
                {
                    data.customSheets.Remove(sheetName);
                    SaveMapData(Path.Combine("map_data.json"), mapData);
                }
            }

            if (Directory.Exists(Path.Combine(modPath, "custom")))
            {
                foreach (string file in Directory.GetFiles(Path.Combine(modPath, "custom"), "*.json"))
                {
                    string relPath = Path.Combine("custom", Path.GetFileName(file));
                    var mapData = GetMapEdits(relPath);

                    if (mapData.mapDataDict.TryGetValue(mapName, out var data))
                    {
                        data.customSheets.Remove(sheetName);
                        SaveMapData(relPath, mapData);
                    }
                }
            }

            if (Config.UseSaveSpecificEdits)
            {
                string relPath = Path.Combine("data", Constants.SaveFolderName + "_map_data.json");
                var mapData = new MapCollectionData();

                if (!Directory.Exists(Path.Combine(modPath, "data")))
                    Directory.CreateDirectory(Path.Combine(modPath, "data"));
                else
                    mapData = GetMapEdits(relPath);

                if (mapData.mapDataDict.TryGetValue(mapName, out var data))
                {
                    data.customSheets.Remove(sheetName);
                    SaveMapData(relPath, mapData);
                }
            }
            cleanMaps.Remove(mapName);
            UpdateCurrentMap(false);
            Game1.playSound(Config.PasteSound);
            SMonitor.Log($"removed tilesheet {sheetName} from {mapName}");
        }

        public static void UpdateCurrentMap(bool force)
        {
            if (!Config.ModEnabled || Game1.player?.currentLocation?.mapPath?.Value is null)
                return;
            string mapName = Game1.player.currentLocation.mapPath.Value.Replace("Maps\\", "");

            if (!mapCollectionData.mapDataDict.ContainsKey(mapName) || (!force && cleanMaps.Contains(mapName)))
                return;
            if(Game1.player.currentLocation is FarmHouse)
            {
                AccessTools.Field(typeof(FarmHouse), "displayingSpouseRoom").SetValue(Game1.player.currentLocation, false);
                AccessTools.Field(typeof(FarmHouse), "currentlyDisplayedUpgradeLevel").SetValue(Game1.player.currentLocation, 0);
            }
            Game1.player.currentLocation.loadMap(Game1.player.currentLocation.mapPath.Value, true);
            Game1.player.currentLocation.MakeMapModifications(true);
        }
        public static bool MapHasTile(Vector2 tileLoc)
        {
            string mapName = Game1.player.currentLocation.mapPath.Value.Replace("Maps\\", "");
            return mapCollectionData.mapDataDict.TryGetValue(mapName, out var data) && data.tileDataDict.ContainsKey(tileLoc);
        }
    }
}