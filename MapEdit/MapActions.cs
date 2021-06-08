using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.IO;

namespace MapEdit
{
    public class MapActions
    {
        public static void GetMapCollectionData()
        {

            ModEntry.mapCollectionData = new MapCollectionData();
            ModEntry.jsonFileName = ModEntry.Config.GlobalChanges ? "map_data.json" : Path.Combine("data", Constants.SaveFolderName + "_map_data.json");

            string modPath = ModEntry.SHelper.DirectoryPath;

            if (!ModEntry.Config.GlobalChanges && !Directory.Exists(Path.Combine(modPath, "data")))
                Directory.CreateDirectory(Path.Combine(modPath, "data"));

            try // convert legacy
            {
                MapCollectionDataOld oldData = ModEntry.SHelper.Data.ReadJsonFile<MapCollectionDataOld>(ModEntry.jsonFileName);
                foreach (var kvp in oldData.mapDataDict)
                {
                    ModEntry.mapCollectionData.mapDataDict.Add(kvp.Key, new MapData());
                    foreach (var kvp2 in kvp.Value.tileDataDict)
                    {
                        ModEntry.mapCollectionData.mapDataDict[kvp.Key].tileDataDict.Add(kvp2.Key, new TileData());
                        foreach (var kvp3 in kvp2.Value.tileDict)
                        {
                            ModEntry.mapCollectionData.mapDataDict[kvp.Key].tileDataDict[kvp2.Key].tileDict.Add(kvp3.Key, new TileLayerData());
                            ModEntry.mapCollectionData.mapDataDict[kvp.Key].tileDataDict[kvp2.Key].tileDict[kvp3.Key].tiles.Add(new TileInfo() { properties = kvp3.Value.properties, blendMode = kvp3.Value.blendMode, tileIndex = kvp3.Value.index, tileSheet = kvp3.Value.tileSheet });
                        }
                    }
                }
                ModEntry.SMonitor.Log($"Converted legacy data from file {ModEntry.jsonFileName}");
                SaveMapData();
            }
            catch
            {
                ModEntry.SMonitor.Log($"Loading map data from file {ModEntry.jsonFileName}");
                ModEntry.mapCollectionData = ModEntry.SHelper.Data.ReadJsonFile<MapCollectionData>(ModEntry.jsonFileName) ?? new MapCollectionData();
            }
            ModEntry.SMonitor.Log($"Loaded map data for {ModEntry.mapCollectionData.mapDataDict.Count} maps");

        }
        public static void SaveMapData()
        {
            ModEntry.SHelper.Data.WriteJsonFile(ModEntry.jsonFileName, ModEntry.mapCollectionData);
        }

        public static void UpdateCurrentMap(bool force)
        {
            if (!ModEntry.Config.EnableMod || (!force && (!ModEntry.mapCollectionData.mapDataDict.ContainsKey(Game1.player.currentLocation.Name) || ModEntry.cleanMaps.Contains(Game1.player.currentLocation.Name))))
                return;

            ModEntry.SHelper.Content.InvalidateCache("Maps/" + Game1.player.currentLocation.Name);
            Game1.player.currentLocation.reloadMap();
        }
        public static bool MapHasTile(Vector2 tileLoc)
        {
            return ModEntry.mapCollectionData.mapDataDict.ContainsKey(Game1.player.currentLocation.Name) && ModEntry.mapCollectionData.mapDataDict[Game1.player.currentLocation.Name].tileDataDict.ContainsKey(tileLoc);
        }
    }
}