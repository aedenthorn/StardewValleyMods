using StardewValley;
using xTile.Layers;
using xTile.Tiles;

namespace MapEdit
{
    public class TileActions
    {
        public static void RevertCurrentTile()
        {
            ModEntry.mapCollectionData.mapDataDict[Game1.player.currentLocation.Name].tileDataDict.Remove(Game1.currentCursorTile);
            if (ModEntry.mapCollectionData.mapDataDict[Game1.player.currentLocation.Name].tileDataDict.Count == 0)
                ModEntry.mapCollectionData.mapDataDict.Remove(Game1.player.currentLocation.Name);
            ModEntry.cleanMaps.Remove(Game1.player.currentLocation.Name);
            MapActions.SaveMapData();
            MapActions.UpdateCurrentMap(true);
        }

        public static void CopyCurrentTile()
        {
            if (!Utility.isOnScreen(Game1.currentCursorTile * Game1.tileSize, 0))
                return;
            ModEntry.currentLayer = 0;
            ModEntry.currentTileDict.Clear();
            foreach (Layer layer in Game1.player.currentLocation.map.Layers)
            {
                if (layer.Id == "Paths")
                    continue;
                try
                {
                    Tile tile = layer.Tiles[(int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y];
                    ModEntry.copiedTileLoc = Game1.currentCursorTile;
                    ModEntry.pastedTileLoc = Game1.currentCursorTile;
                    ModEntry.currentTileDict.Add(layer.Id, tile.Clone(layer));
                    //Monitor.Log($"Copied layer {layer.Id} tile index {tile.TileIndex}");
                }
                catch { }
            }
            Game1.playSound(ModEntry.Config.CopySound);
            ModEntry.SMonitor.Log($"Copied tile at {Game1.currentCursorTile}");
        }

        public static void PasteCurrentTile()
        {
            if (!Utility.isOnScreen(Game1.currentCursorTile * Game1.tileSize, 0))
                return;

            if (!ModEntry.mapCollectionData.mapDataDict.ContainsKey(Game1.player.currentLocation.Name))
                ModEntry.mapCollectionData.mapDataDict[Game1.player.currentLocation.Name] = new MapData();

            ModEntry.mapCollectionData.mapDataDict[Game1.player.currentLocation.Name].tileDataDict[Game1.currentCursorTile] = new TileData(ModEntry.currentTileDict);
            ModEntry.SMonitor.Log($"Pasted tile to {Game1.currentCursorTile}");
            ModEntry.cleanMaps.Remove(Game1.player.currentLocation.Name);
            MapActions.UpdateCurrentMap(false);
            MapActions.SaveMapData();
            ModEntry.pastedTileLoc = Game1.currentCursorTile;
            Game1.playSound(ModEntry.Config.PasteSound);
        }
    }
}
