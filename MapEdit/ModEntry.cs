using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using xTile.Layers;
using xTile.Tiles;

namespace MapEdit
{
    public class ModEntry : Mod, IAssetEditor
    {
        public static ModEntry context;

        internal static ModConfig Config;
        private static Texture2D existsTexture;
        private static Texture2D activeTexture;
        private static Texture2D copiedTexture;
        private static bool modActive = false;
        private static int modNumber = 189017541;
        private static MapCollectionData mapCollectionData = new MapCollectionData();

        private static Vector2 copiedTileLoc = new Vector2(-1, -1);
        private static Vector2 pastedTileLoc = new Vector2(-1, -1);
        private static Dictionary<string, Tile> currentTileDict = new Dictionary<string, Tile>();
        private static int currentLayer = 0;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();


            mapCollectionData = Helper.Data.ReadJsonFile<MapCollectionData>("map_data.json") ?? new MapCollectionData();

            CreateTextures();

            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            Helper.Events.Player.Warped += Player_Warped;

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.pressSwitchToolButton)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.pressSwitchToolButton_Prefix))
            );
        }

        private static bool pressSwitchToolButton_Prefix()
        {
            if (Context.IsPlayerFree && Game1.input.GetMouseState().ScrollWheelValue != Game1.oldMouseState.ScrollWheelValue && modActive && copiedTileLoc.X > -1)
            {
                if (!Config.EnableMod)
                    return true;

                int delta = Game1.input.GetMouseState().ScrollWheelValue - Game1.oldMouseState.ScrollWheelValue;

                var layers = currentTileDict.Keys.ToArray();
                if (context.Helper.Input.IsDown(Config.LayerModButton))
                {
                    if (currentTileDict.Count < 2)
                        return false;
                    if (delta > 0)
                    {
                        if (currentLayer >= layers.Length - 1)
                            currentLayer = 0;
                        else
                            currentLayer++;
                    }
                    else
                    {
                        if (currentLayer < 1)
                            currentLayer = layers.Length - 1;
                        else
                            currentLayer--;
                    }
                    string text = string.Format(context.Helper.Translation.Get("current-layer"), layers[currentLayer]);
                    context.Monitor.Log(text);
                    context.ShowMessage(text);
                }
                else if (context.Helper.Input.IsDown(Config.SheetModButton))
                {
                    List<TileSheet> sheets = new List<TileSheet>();
                    foreach (TileSheet sheet in Game1.player.currentLocation.map.TileSheets)
                    {
                        if (sheet.Id != "Paths")
                            sheets.Add(sheet);
                    }
                    if (sheets.Count < 2)
                        return false;
                    Tile tile = currentTileDict[layers[currentLayer]];
                    if (tile == null)
                    {
                        Layer layer = Game1.player.currentLocation.map.GetLayer(layers[currentLayer]);
                        tile = new StaticTile(layer, sheets[0], BlendMode.Alpha, -1);
                    }
                    int tileSheetIndex = sheets.IndexOf(tile.TileSheet);
                    if (delta > 0)
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
                    context.Helper.Reflection.GetField<TileSheet>(tile, "m_tileSheet").SetValue(tileSheet);
                    tile.TileIndex = 0;
                    string text = string.Format(context.Helper.Translation.Get("current-tilesheet"), tileSheet.Id);
                    context.Monitor.Log(text);
                    context.ShowMessage(text);
                }
                else
                {
                    Tile tile = currentTileDict[layers[currentLayer]];
                    if (tile == null)
                    {
                        context.Monitor.Log($"Layer {layers[currentLayer]} copied tile is null, creating empty tile");
                        Layer layer = Game1.player.currentLocation.map.GetLayer(layers[currentLayer]);
                        currentTileDict[layers[currentLayer]] = new StaticTile(layer, Game1.player.currentLocation.map.TileSheets[0], BlendMode.Alpha, -1);
                    }
                    if (delta > 0)
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
                    //context.Monitor.Log($"layer {layers[currentLayer]} new tile index {tile.TileIndex}");
                }
                Game1.playSound(Config.ScrollSound);
                return false;
            }
            return true;
        }

        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {

        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if(modActive && (Helper.Input.IsDown(Config.PasteButton) || Helper.Input.IsSuppressed(Config.PasteButton)) && pastedTileLoc.X > -1 && pastedTileLoc != Game1.currentCursorTile)
            {
                PasteCurrentTile();
            }
        }


        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            DeactivateMod();
            UpdateCurrentMap(false);
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree)
            {
                DeactivateMod();
                return;
            }

            if (e.Button == Config.ToggleButton)
            {
                Helper.Input.Suppress(e.Button);
                modActive = !modActive;
                copiedTileLoc = new Vector2(-1, -1);
                currentTileDict.Clear();
                Monitor.Log($"Toggled mod: {modActive}");
                if (modActive)
                    ShowMessage(string.Format(Helper.Translation.Get("mod-active"), Config.ToggleButton));
                else
                    ShowMessage(string.Format(Helper.Translation.Get("mod-inactive"), Config.ToggleButton));
            }
            else if (modActive && e.Button == Config.CopyButton)
            {
                Helper.Input.Suppress(e.Button);

                CopyCurrentTile();

            }
            else if (modActive && copiedTileLoc.X > -1 && e.Button == Config.PasteButton && pastedTileLoc != Game1.currentCursorTile)
            {
                Helper.Input.Suppress(e.Button);
                PasteCurrentTile();
                
            }
            else if (modActive && e.Button == SButton.Escape)
            {
                Helper.Input.Suppress(e.Button);
                modActive = !modActive;
                copiedTileLoc = new Vector2(-1, -1);
                currentTileDict.Clear();
            }
            else if (modActive && e.Button == Config.RefreshButton)
            {
                Helper.Input.Suppress(e.Button);
                mapCollectionData = Helper.Data.ReadJsonFile<MapCollectionData>("map_data.json") ?? new MapCollectionData();
                SaveMapData();
                Monitor.Log($"Refreshed map edits, {mapCollectionData.mapDataDict.Count} maps edited");
                UpdateCurrentMap(true);
            }
        }

        private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod || !modActive)
                return;

            if (Game1.activeClickableMenu != null)
            {
                modActive = false;
                return;
            }

            Vector2 mouseTile = Game1.currentCursorTile;
            Vector2 mouseTilePos = mouseTile * Game1.tileSize - new Vector2(Game1.viewport.X, Game1.viewport.Y);
            if (copiedTileLoc.X > -1)
            { 
                foreach (var kvp in currentTileDict)
                {
                    int offset = kvp.Key.Equals("Front") ? (16 * Game1.pixelZoom) : 0;
                    float layerDepth = (copiedTileLoc.Y * (16 * Game1.pixelZoom) + 16 * Game1.pixelZoom + offset) / 10000f;
                    Tile tile = kvp.Value;
                    if (tile == null)
                        continue;

                    var xRect = tile.TileSheet.GetTileImageBounds(tile.TileIndex);
                    Rectangle sourceRectangle = new Rectangle(xRect.X, xRect.Y, xRect.Width, xRect.Height);

                    //Monitor.Log($"{layer.Id} {tile.TileSheet.Id} {tile.TileIndex} {xRect}");

                    Texture2D texture2D;
                    try
                    {
                        texture2D = Helper.Reflection.GetField<Dictionary<TileSheet, Texture2D>>(Game1.mapDisplayDevice, "m_tileSheetTextures", true)?.GetValue()?[tile.TileSheet];
                    }
                    catch
                    {
                        texture2D = Helper.Reflection.GetField<Dictionary<TileSheet, Texture2D>>(Game1.mapDisplayDevice, "m_tileSheetTextures2", true)?.GetValue()?[tile.TileSheet];
                    }
                    if (texture2D != null)
                        e.SpriteBatch.Draw(texture2D, mouseTilePos, sourceRectangle, Color.White, 0f, Vector2.Zero, Layer.zoom, SpriteEffects.None, layerDepth);
                }
                e.SpriteBatch.Draw(copiedTexture, mouseTilePos, Color.White);
                
            }
            else if (MapHasTile(mouseTile))
                e.SpriteBatch.Draw(existsTexture, mouseTilePos, Color.White);
            else
                e.SpriteBatch.Draw(activeTexture, mouseTilePos, Color.White);

        }
        private void DeactivateMod()
        {
            modActive = false;
            copiedTileLoc = new Vector2(-1, -1);
            pastedTileLoc = new Vector2(-1, -1);
            currentTileDict.Clear();
            currentLayer = 0;
        }

        private void CreateTextures()
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

        private void CopyCurrentTile()
        {
            if (!Utility.isOnScreen(Game1.currentCursorTile * Game1.tileSize, 0))
                return;

            currentTileDict.Clear();
            foreach (Layer layer in Game1.player.currentLocation.map.Layers)
            {
                if (layer.Id == "Paths")
                    continue;
                try
                {
                    Tile tile = layer.Tiles[(int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y];
                    copiedTileLoc = Game1.currentCursorTile;
                    pastedTileLoc = Game1.currentCursorTile;
                    currentTileDict.Add(layer.Id, tile.Clone(layer));
                }
                catch { }
            }
            Game1.playSound(Config.CopySound);
            Monitor.Log($"Copied tile at {Game1.currentCursorTile}");
        }

        private void PasteCurrentTile()
        {
            if (!Utility.isOnScreen(Game1.currentCursorTile * Game1.tileSize, 0))
                return;

            if (!mapCollectionData.mapDataDict.ContainsKey(Game1.player.currentLocation.Name))
                mapCollectionData.mapDataDict[Game1.player.currentLocation.Name] = new MapData();

            mapCollectionData.mapDataDict[Game1.player.currentLocation.Name].tileDataDict[Game1.currentCursorTile] = new TileData(currentTileDict);
            UpdateCurrentMap(false);
            SaveMapData();
            pastedTileLoc = Game1.currentCursorTile;
            Game1.playSound(Config.PasteSound);
            Monitor.Log($"Pasted tile to {Game1.currentCursorTile}");
        }

        private void SaveMapData()
        {
            Helper.Data.WriteJsonFile("map_data.json", mapCollectionData);
        }

        private void UpdateCurrentMap(bool force)
        {
            if (!Config.EnableMod || (!force && !mapCollectionData.mapDataDict.ContainsKey(Game1.player.currentLocation.Name)))
                return;

            Helper.Content.InvalidateCache("Maps/" + Game1.player.currentLocation.Name);
            Game1.player.currentLocation.reloadMap();
        }

        private void ShowMessage(string message)
        {
            Game1.hudMessages.RemoveAll((HUDMessage p) => p.number == modNumber);
            Game1.addHUDMessage(new HUDMessage(message, 3)
            {
                noIcon = true,
                number = modNumber
            });
        }

        private bool MapHasTile(Vector2 tileLoc)
        {
            return mapCollectionData.mapDataDict.ContainsKey(Game1.player.currentLocation.Name) && mapCollectionData.mapDataDict[Game1.player.currentLocation.Name].tileDataDict.ContainsKey(tileLoc);
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            if (asset.AssetName.StartsWith("Maps"))
            {
                foreach (string name in mapCollectionData.mapDataDict.Keys)
                {
                    if (asset.AssetNameEquals("Maps/"+name))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            foreach (string name in mapCollectionData.mapDataDict.Keys)
            {
                if (asset.AssetNameEquals("Maps/" + name))
                {
                    Monitor.Log("Editing map " + asset.AssetName);
                    var mapData = asset.AsMap();
                    MapData data = mapCollectionData.mapDataDict[name];
                    int count = 0;
                    foreach (var kvp in data.tileDataDict)
                    {
                        foreach (Layer layer in mapData.Data.Layers)
                        {
                            if (layer.Id == "Paths")
                                continue;
                            try
                            {
                                layer.Tiles[(int)kvp.Key.X, (int)kvp.Key.Y] = null;
                            }
                            catch
                            {

                            }
                        }
                        foreach (var kvp2 in kvp.Value.tileDict)
                        {
                            try
                            {
                                mapData.Data.GetLayer(kvp2.Key).Tiles[(int)kvp.Key.X, (int)kvp.Key.Y] = new StaticTile(mapData.Data.GetLayer(kvp2.Key), mapData.Data.GetTileSheet(kvp2.Value.tileSheet), kvp2.Value.blendMode, kvp2.Value.index);
                                foreach (var prop in kvp2.Value.properties)
                                {
                                    mapData.Data.GetLayer(kvp2.Key).Tiles[(int)kvp.Key.X, (int)kvp.Key.Y].Properties[prop.Key] = prop.Value;
                                }
                                //Monitor.Log($"new tile {kvp.Key}, layer {kvp2.Key}, index {mapData.Data.GetLayer(kvp2.Key).Tiles[(int)kvp.Key.X, (int)kvp.Key.Y]?.TileIndex}", LogLevel.Info);
                                count++;
                            }
                            catch
                            {

                            }
                        }
                    }
                    Monitor.Log($"Added {count} custom tiles to map {name}");
                }
            }
        }
    }
}
