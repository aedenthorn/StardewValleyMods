using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System.IO;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace MapEdit
{
    public partial class ModEntry
    {
        public static void RevertCurrentTile()
        {
            pastedTileLoc.Value = new Vector2(-1, -1);
            SaveMapTile(Game1.player.currentLocation.mapPath.Value.Replace("Maps\\", ""), Game1.currentCursorTile, null);
            UpdateCurrentMap(true);
        }
        
        public static void CopyCurrentTile()
        {
            if (!Utility.isOnScreen(Game1.currentCursorTile * Game1.tileSize, 0))
                return;
            currentLayer.Value = null;
            currentTileDict.Value.Clear();
            copiedTileLoc.Value = Game1.currentCursorTile;
            pastedTileLoc.Value = Game1.currentCursorTile;
            foreach (Layer layer in Game1.player.currentLocation.map.Layers)
            {
                if (layer.Id == "Paths")
                    continue;
                try
                {
                    Tile tile = layer.Tiles[(int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y];
                    currentTileDict.Value.Add(layer.Id, tile.Clone(layer));
                    SMonitor.Log($"Copied layer {layer.Id} tile index {tile.TileIndex}");
                }
                catch { }
            }
            Game1.playSound(Config.CopySound);
            SMonitor.Log($"Copied tile at {Game1.currentCursorTile}");
        }

        public static void PasteCurrentTile()
        {
            if (!Utility.isOnScreen(Game1.currentCursorTile * Game1.tileSize, Game1.tileSize))
                return;

            string mapName = Game1.player.currentLocation.mapPath.Value.Replace("Maps\\", "");

            SaveMapTile(mapName, Game1.currentCursorTile, new TileLayers(currentTileDict.Value));
            cleanMaps.Remove(mapName);
            UpdateCurrentMap(false);
            pastedTileLoc.Value = Game1.currentCursorTile;
            Game1.playSound(Config.PasteSound);
            SMonitor.Log($"Pasted tile to {Game1.currentCursorTile}");
        }
    }
}
