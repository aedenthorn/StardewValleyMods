using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;
using System.Collections.Immutable;
using xTile.Layers;
using xTile.Tiles;

namespace MapEdit
{
    public partial class ModEntry
    {
        public static void SwitchTile(bool increase)
        {
            List<string> layers = new List<string>();
            foreach (Layer layer in Game1.player.currentLocation.map.Layers)
            {
                if (currentTileDict.Value.ContainsKey(layer.Id))
                    layers.Add(layer.Id);
            }
            int layerIndex = layers.IndexOf(currentLayer.Value);
            if (layerIndex < 0)
            {
                layerIndex = 0;
                currentLayer.Value = layers[0];
                return;
            }
            if (SHelper.Input.IsDown(Config.LayerModButton))
            {
                if (layers.Count < 2)
                    return;
                if (increase)
                {
                    if (layerIndex >= layers.Count - 1)
                        currentLayer.Value = layers[0];
                    else
                        currentLayer.Value = layers[layerIndex + 1];
                }
                else
                {
                    if (layerIndex < 1)
                        currentLayer.Value = layers[layers.Count - 1];
                    else
                        currentLayer.Value = layers[layerIndex - 1];
                }

                string text = string.Format(SHelper.Translation.Get("current-layer"), currentLayer.Value);
                SMonitor.Log(text);
                ShowMessage(text);
            }
            else if (SHelper.Input.IsDown(Config.SheetModButton))
            {
                if (currentTileDict.Value[currentLayer.Value] is AnimatedTile)
                    return;

                List<TileSheet> sheets = new List<TileSheet>();
                foreach (TileSheet sheet in Game1.player.currentLocation.map.TileSheets)
                {
                    if (sheet.Id != "Paths")
                        sheets.Add(sheet);
                }
                if (sheets.Count < 2)
                    return;
                Tile tile = currentTileDict.Value[currentLayer.Value];
                if (tile == null)
                {
                    Layer layer = Game1.player.currentLocation.map.GetLayer(currentLayer.Value);
                    tile = new StaticTile(layer, sheets[0], BlendMode.Alpha, -1);
                }
                int tileSheetIndex = sheets.IndexOf(tile.TileSheet);
                if (increase)
                {
                    if (tileSheetIndex >= sheets.Count - 1)
                        tileSheetIndex = 0;
                    else
                        tileSheetIndex++;
                }
                else
                {
                    if (tileSheetIndex < 1)
                        tileSheetIndex = sheets.Count - 1;
                    else
                        tileSheetIndex--;
                }
                TileSheet tileSheet = sheets[tileSheetIndex];
                SHelper.Reflection.GetField<TileSheet>(tile, "m_tileSheet").SetValue(tileSheet);
                tile.TileIndex = 0;
                string text = string.Format(SHelper.Translation.Get("current-tilesheet"), tileSheet.Id);
                SMonitor.Log(text);
                ShowMessage(text);
            }
            else
            {
                if (!currentTileDict.Value.TryGetValue(currentLayer.Value, out var val) || val == null)
                {
                    currentTileDict.Value[currentLayer.Value] = new StaticTile(Game1.player.currentLocation.map.GetLayer(currentLayer.Value), Game1.player.currentLocation.map.TileSheets[0], BlendMode.Alpha, -1);
                }
                if (currentTileDict.Value[currentLayer.Value] is AnimatedTile)
                    return;

                Tile tile = currentTileDict.Value[currentLayer.Value];
                //context.Monitor.Log($"old index {tile.TileIndex}");
                if (tile == null)
                {
                    SMonitor.Log($"Layer {currentLayer.Value} copied tile is null, creating empty tile");
                    Layer layer = Game1.player.currentLocation.map.GetLayer(currentLayer.Value);
                    currentTileDict.Value[currentLayer.Value] = new StaticTile(layer, Game1.player.currentLocation.map.TileSheets[0], BlendMode.Alpha, -1);
                }
                if (increase)
                {
                    if (tile.TileIndex >= tile.TileSheet.TileCount - 1)
                        tile.TileIndex = -1;
                    else
                        tile.TileIndex++;
                }
                else
                {
                    if (tile.TileIndex < 0)
                        tile.TileIndex = tile.TileSheet.TileCount - 1;
                    else
                        tile.TileIndex--;
                }
                //context.Monitor.Log($"new index {tile.TileIndex}");
            }
            Game1.playSound(Config.ScrollSound);
        }


        public static void CreateTextures()
        {
            int thickness = Config.BorderThickness;

            existsTexture = new Texture2D(Game1.graphics.GraphicsDevice, Game1.tileSize, Game1.tileSize);
            Color[] data = new Color[Game1.tileSize * Game1.tileSize];
            for (int i = 0; i < data.Length; i++)
            {
                if (i < Game1.tileSize * thickness || i % Game1.tileSize < thickness || i % Game1.tileSize >= Game1.tileSize - thickness || i >= data.Length - Game1.tileSize * thickness)
                    data[i] = Config.ExistsColor;
                else
                    data[i] = Color.Transparent;
            }
            existsTexture.SetData(data);

            activeTexture = new Texture2D(Game1.graphics.GraphicsDevice, Game1.tileSize, Game1.tileSize);
            data = new Color[Game1.tileSize * Game1.tileSize];
            for (int i = 0; i < data.Length; i++)
            {
                if (i < Game1.tileSize * thickness || i % Game1.tileSize < thickness || i % Game1.tileSize >= Game1.tileSize - thickness || i >= data.Length - Game1.tileSize * thickness)
                    data[i] = Config.ActiveColor;
                else
                    data[i] = Color.Transparent;
            }
            activeTexture.SetData(data);

            copiedTexture = new Texture2D(Game1.graphics.GraphicsDevice, Game1.tileSize, Game1.tileSize);
            data = new Color[Game1.tileSize * Game1.tileSize];
            for (int i = 0; i < data.Length; i++)
            {
                if (i < Game1.tileSize * thickness || i % Game1.tileSize < thickness || i % Game1.tileSize >= Game1.tileSize - thickness || i >= data.Length - Game1.tileSize * thickness)
                    data[i] = Config.CopiedColor;
                else
                    data[i] = Color.Transparent;
            }
            copiedTexture.SetData(data);

        }

        public static void DeactivateMod()
        {
            modActive.Value = false;
            copiedTileLoc.Value = new Vector2(-1, -1);
            pastedTileLoc.Value = new Vector2(-1, -1);
            currentTileDict.Value = new Dictionary<string, Tile>();
            currentLayer.Value = null;
            tileMenu.Value = null;
            if(Game1.activeClickableMenu is TileSelectMenu)
            {
                Game1.activeClickableMenu = null;
            }
        }


        public static void ShowMessage(string message)
        {
            Game1.hudMessages.RemoveAll((HUDMessage p) => p.number == modNumber);
            Game1.addHUDMessage(new HUDMessage(message, 3)
            {
                noIcon = true,
                number = modNumber,
                timeLeft = 1000
            });
        }
    }
}