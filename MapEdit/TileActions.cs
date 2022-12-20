using Microsoft.Xna.Framework;
using StardewValley;
using xTile.Layers;
using xTile.Tiles;

namespace MapEdit
{
    public class TileActions
    {
        public static void RevertCurrentTile()
        {
            ModEntry.pastedTileLoc.Value = new Vector2(-1, -1);
            MapActions.SaveMapTile(Game1.player.currentLocation.mapPath.Value.Replace("Maps\\", ""), Game1.currentCursorTile, null);
            MapActions.UpdateCurrentMap(true);
        }

        public static void CopyCurrentTile()
        {
            if (!Utility.isOnScreen(Game1.currentCursorTile * Game1.tileSize, 0))
                return;
            ModEntry.currentLayer.Value = 0;
            ModEntry.currentTileDict.Value.Clear();
            ModEntry.copiedTileLoc.Value = Game1.currentCursorTile;
            ModEntry.pastedTileLoc.Value = Game1.currentCursorTile;
            foreach (Layer layer in Game1.player.currentLocation.map.Layers)
            {
                if (layer.Id == "Paths")
                    continue;
                try
                {
                    Tile tile = layer.Tiles[(int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y];
                    ModEntry.currentTileDict.Value.Add(layer.Id, tile.Clone(layer));
                    ModEntry.SMonitor.Log($"Copied layer {layer.Id} tile index {tile.TileIndex}");
                }
                catch { }
            }
            Game1.playSound(ModEntry.Config.CopySound);
            ModEntry.SMonitor.Log($"Copied tile at {Game1.currentCursorTile}");
        }

        public static void PasteCurrentTile()
        {
            if (!Utility.isOnScreen(Game1.currentCursorTile * Game1.tileSize, Game1.tileSize))
                return;

            string mapName = Game1.player.currentLocation.mapPath.Value.Replace("Maps\\", "");

            MapActions.SaveMapTile(mapName, Game1.currentCursorTile, new TileLayers(ModEntry.currentTileDict.Value));
            ModEntry.cleanMaps.Remove(mapName);
            MapActions.UpdateCurrentMap(false);
            ModEntry.pastedTileLoc.Value = Game1.currentCursorTile;
            Game1.playSound(ModEntry.Config.PasteSound);
            ModEntry.SMonitor.Log($"Pasted tile to {Game1.currentCursorTile}");
        }
    }
}
