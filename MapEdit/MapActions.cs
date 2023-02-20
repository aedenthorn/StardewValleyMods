using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;

namespace MapEdit
{
    public class MapActions
    {
        public static void GetMapCollectionData()
        {
            string modPath = ModEntry.SHelper.DirectoryPath;
            ModEntry.mapCollectionData = new MapCollectionData();

            var mapEdits = new List<MapCollectionData>();

            if (ModEntry.Config.IncludeGlobalEdits && File.Exists(Path.Combine(modPath, "map_data.json")))
            {
                mapEdits.Add(GetMapEdits("map_data.json"));
            }

            if (Directory.Exists(Path.Combine(ModEntry.SHelper.DirectoryPath, "custom")))
            {
                foreach (string file in Directory.GetFiles(Path.Combine(ModEntry.SHelper.DirectoryPath, "custom"), "*.json"))
                {
                    mapEdits.Add(GetMapEdits(Path.Combine("custom", Path.GetFileName(file))));
                }
            }

            if (ModEntry.Config.UseSaveSpecificEdits)
            {
                if (!Directory.Exists(Path.Combine(modPath, "data")))
                    Directory.CreateDirectory(Path.Combine(modPath, "data"));
                mapEdits.Add(GetMapEdits(Path.Combine("data", Constants.SaveFolderName + "_map_data.json")));
            }

            foreach(MapCollectionData data in mapEdits)
            {
                foreach(var map in data.mapDataDict)
                {
                    if (!ModEntry.mapCollectionData.mapDataDict.ContainsKey(map.Key))
                    {
                        ModEntry.mapCollectionData.mapDataDict[map.Key] = map.Value;
                    }
                    else
                    {
                        foreach(var tile in map.Value.tileDataDict)
                        {
                            ModEntry.mapCollectionData.mapDataDict[map.Key].tileDataDict[tile.Key] = tile.Value;
                        }
                    }
                }
            }

            ModEntry.SMonitor.Log($"Loaded map data for {ModEntry.mapCollectionData.mapDataDict.Count} maps");
        }

        private static MapCollectionData GetMapEdits(string path)
        {
            ModEntry.SMonitor.Log($"Loading map data from file {path}");
            MapCollectionData collection = ModEntry.SHelper.Data.ReadJsonFile<MapCollectionData>(path) ?? new MapCollectionData();
            return collection;
        }

        public static void SaveMapData(string path, MapCollectionData collection)
        {
            ModEntry.SHelper.Data.WriteJsonFile(path, collection);
            ModEntry.SMonitor.Log($"Saved edits to file {path}");
        }

        public static void SaveMapTile(string map, Vector2 tileLoc, TileLayers tile)
        {
            if (tile == null)
            {
                ModEntry.mapCollectionData.mapDataDict[map].tileDataDict.Remove(tileLoc);
                if (ModEntry.mapCollectionData.mapDataDict[map].tileDataDict.Count == 0)
                    ModEntry.mapCollectionData.mapDataDict.Remove(map);
            }
            else
            {
                if (!ModEntry.mapCollectionData.mapDataDict.ContainsKey(map))
                    ModEntry.mapCollectionData.mapDataDict[map] = new MapData();

                ModEntry.mapCollectionData.mapDataDict[map].tileDataDict[Game1.currentCursorTile] = new TileLayers(ModEntry.currentTileDict.Value);
            }

            string modPath = ModEntry.SHelper.DirectoryPath;
            if (ModEntry.Config.IncludeGlobalEdits)
            {
                var mapData = GetMapEdits("map_data.json");
                if (tile == null)
                {
                    if (mapData.mapDataDict.ContainsKey(map) && mapData.mapDataDict[map].tileDataDict.ContainsKey(tileLoc))
                    {
                        mapData.mapDataDict[map].tileDataDict.Remove(tileLoc);
                        if (mapData.mapDataDict[map].tileDataDict.Count == 0)
                            mapData.mapDataDict.Remove(map);
                        SaveMapData(Path.Combine("map_data.json"), mapData);
                    }
                }
                else if (!ModEntry.Config.UseSaveSpecificEdits || (mapData.mapDataDict.ContainsKey(map) && mapData.mapDataDict[map].tileDataDict.ContainsKey(tileLoc))) // save new to global if not using save specific
                {
                    if (!mapData.mapDataDict.ContainsKey(map))
                        mapData.mapDataDict[map] = new MapData();

                    mapData.mapDataDict[map].tileDataDict[tileLoc] = tile;
                    SaveMapData(Path.Combine("map_data.json"), mapData);
                }
            }

            if (Directory.Exists(Path.Combine(modPath, "custom")))
            {
                foreach (string file in Directory.GetFiles(Path.Combine(modPath, "custom"), "*.json"))
                {
                    string relPath = Path.Combine("custom", Path.GetFileName(file));
                    var mapData = GetMapEdits(relPath);

                    if (mapData.mapDataDict.ContainsKey(map) && mapData.mapDataDict[map].tileDataDict.ContainsKey(tileLoc)) // only edit if custom file contains this
                    {
                        if (tile == null)
                        {
                            mapData.mapDataDict[map].tileDataDict.Remove(tileLoc);
                            if (mapData.mapDataDict[map].tileDataDict.Count == 0)
                                mapData.mapDataDict.Remove(map);
                        }
                        else
                        {
                            mapData.mapDataDict[map].tileDataDict[tileLoc] = tile;
                        }
                        SaveMapData(relPath, mapData);
                    }
                }
            }

            if (ModEntry.Config.UseSaveSpecificEdits)
            {
                string relPath = Path.Combine("data", Constants.SaveFolderName + "_map_data.json");
                var mapData = new MapCollectionData();

                if (!Directory.Exists(Path.Combine(modPath, "data")))
                    Directory.CreateDirectory(Path.Combine(modPath, "data"));
                else
                    mapData = GetMapEdits(relPath);

                if (tile == null)
                {
                    if (mapData.mapDataDict.ContainsKey(map) && mapData.mapDataDict[map].tileDataDict.ContainsKey(tileLoc))
                    {
                        mapData.mapDataDict[map].tileDataDict.Remove(tileLoc);
                        if (mapData.mapDataDict[map].tileDataDict.Count == 0)
                            mapData.mapDataDict.Remove(map);
                    }
                }
                else // always save new to save specific
                {
                    if (!mapData.mapDataDict.ContainsKey(map))
                        mapData.mapDataDict[map] = new MapData();
                    mapData.mapDataDict[map].tileDataDict[tileLoc] = tile;
                }
                SaveMapData(relPath, mapData);
            }
            ModEntry.cleanMaps.Remove(map);
        }

        public static void UpdateCurrentMap(bool force)
        {
            if (!ModEntry.Config.EnableMod || Game1.player?.currentLocation?.mapPath?.Value is null)
                return;
            string mapName = Game1.player.currentLocation.mapPath.Value.Replace("Maps\\", "");

            if (!force && (!ModEntry.mapCollectionData.mapDataDict.ContainsKey(mapName) || ModEntry.cleanMaps.Contains(mapName)))
                return;

            ModEntry.SHelper.GameContent.InvalidateCache("Maps/" + mapName);
            Game1.player.currentLocation.reloadMap();
        }
        public static bool MapHasTile(Vector2 tileLoc)
        {
            string mapName = Game1.player.currentLocation.mapPath.Value.Replace("Maps\\", "");
            return ModEntry.mapCollectionData.mapDataDict.ContainsKey(mapName) && ModEntry.mapCollectionData.mapDataDict[mapName].tileDataDict.ContainsKey(tileLoc);
        }
    }
}