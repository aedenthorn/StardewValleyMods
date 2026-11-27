using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using System.Linq;
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
                    if(tile != null)
                    {
                        currentTileDict.Value.Add(layer.Id, tile.Clone(layer));
                        SMonitor.Log($"Copied layer {layer.Id} tile index {tile.TileIndex}");
                    }
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
            var tileDict = currentTileDict.Value;
            if (SHelper.Input.IsDown(Config.LayerModButton))
            {
                tileDict = new System.Collections.Generic.Dictionary<string, Tile>(currentTileDict.Value);
                
                foreach(var l in Game1.player.currentLocation.Map.Layers)
                {
                    if (!currentTileDict.Value.ContainsKey(l.Id))
                    {
                        var tile = l.PickTile(new xTile.Dimensions.Location((int)Game1.currentCursorTile.X * 64, (int)Game1.currentCursorTile.Y * 64), Game1.viewport.Size);
                        if(tile != null )
                        {
                            tileDict[l.Id] = tile;
                        }
                    }
                }
            }

            SaveMapTile(mapName, Game1.currentCursorTile, new TileLayers(tileDict));
            pastedTileLoc.Value = Game1.currentCursorTile;
            Game1.playSound(Config.PasteSound);
            SMonitor.Log($"Pasted tile to {Game1.currentCursorTile}");
        }

        public static void RotateOrFlipCurrentTile(bool cw)
        {
            string mapName = Game1.player.currentLocation.mapPath.Value.Replace("Maps\\", "");
            var tileDict = currentTileDict.Value;
            bool mod = SHelper.Input.IsDown(Config.LayerModButton);
            string key = mod ? "@Flip" : "@Rotation";
            foreach (var t in tileDict.Values)
            {
                int amount = 0;
                if (t.Properties.TryGetValue(key, out var value))
                {
                    int.TryParse(value, out amount);
                }
                if (cw)
                {
                    if (mod)
                    {
                        amount = (amount + 1) % 3;
                    }
                    else
                    {
                        amount = (amount / 90 * 90 + 90) % 360;
                    }
                }
                else
                {
                    if (mod)
                    {
                        amount--;
                        if (amount < 0)
                            amount = 2;
                    }
                    else
                    {
                        amount = (amount / 90 * 90 - 90);
                        if(amount < 0)
                        {
                            amount = 270;
                        }
                    }

                }
                t.Properties[key] = amount + "";
            }
            Game1.playSound(Config.ScrollSound);
        }
    }
}
