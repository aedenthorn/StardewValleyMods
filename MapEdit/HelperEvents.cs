using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace MapEdit
{
    public partial class ModEntry
    {
        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod)
                return;

            foreach (string name in mapCollectionData.mapDataDict.Keys)
            {
                if (e.DataType == typeof(Map) && (e.NameWithoutLocale.IsEquivalentTo("Maps/" + name) || e.NameWithoutLocale.IsEquivalentTo(name)))
                {
                    e.Edit(delegate (IAssetData idata)
                    {
                        SMonitor.Log("Editing map " + e.Name);
                        var mapData = idata.AsMap();
                        MapData data = mapCollectionData.mapDataDict[name];
                        foreach(var kvp in data.customSheets)
                        {
                            if (mapData.Data.TileSheets.FirstOrDefault(s => s.ImageSource == kvp.Value.path) != null)
                                continue;
                            string name = kvp.Key;
                            int which = 0;
                            while(mapData.Data.Layers.FirstOrDefault(l => l.Id == name) != null)
                            {
                                which++;
                            }
                            if(which > 0)
                            {
                                name += "_" + which;
                            }
                            mapData.Data.AddTileSheet(new TileSheet(name, mapData.Data, kvp.Value.path, new xTile.Dimensions.Size(kvp.Value.width, kvp.Value.height), new xTile.Dimensions.Size(16, 16)));
                        }
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
                                    List<StaticTile> tiles = new List<StaticTile>();
                                    for (int i = 0; i < kvp2.Value.tiles.Count; i++)
                                    {
                                        TileInfo tile = kvp2.Value.tiles[i];
                                        tiles.Add(new StaticTile(mapData.Data.GetLayer(kvp2.Key), mapData.Data.GetTileSheet(tile.tileSheet), tile.blendMode, tile.tileIndex));
                                        foreach (var prop in kvp2.Value.tiles[i].properties)
                                        {
                                            tiles[i].Properties[prop.Key] = prop.Value;
                                        }
                                    }

                                    if (kvp2.Value.tiles.Count == 1)
                                    {
                                        mapData.Data.GetLayer(kvp2.Key).Tiles[(int)kvp.Key.X, (int)kvp.Key.Y] = tiles[0];
                                    }
                                    else
                                    {
                                        mapData.Data.GetLayer(kvp2.Key).Tiles[(int)kvp.Key.X, (int)kvp.Key.Y] = new AnimatedTile(mapData.Data.GetLayer(kvp2.Key), tiles.ToArray(), kvp2.Value.frameInterval);
                                    }
                                    count++;
                                }
                                catch
                                {

                                }
                            }
                        }
                        SMonitor.Log($"Added {count} custom tiles to map {name}");
                    }, StardewModdingAPI.Events.AssetEditPriority.Late);
                    cleanMaps.Add(name);
                }
            }
        }
        public static void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            DeactivateMod();
            cleanMaps.Clear();
        }

        public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            GetMapCollectionData();
        }


        public static void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (MouseInMenu())
            {
                pastedTileLoc.Value = -Vector2.One;
                return;
            }
            if (modActive.Value && (SHelper.Input.IsDown(Config.PasteButton) || SHelper.Input.IsSuppressed(Config.PasteButton)) && pastedTileLoc.Value.X > -1 && pastedTileLoc.Value != Game1.currentCursorTile)
            {
                PasteCurrentTile();
            }
            else if (modActive.Value && (SHelper.Input.IsDown(Config.RevertButton) || SHelper.Input.IsSuppressed(Config.RevertButton)) && MapHasTile(Game1.currentCursorTile))
            {
                RevertCurrentTile();
            }
        }


        public static void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            DeactivateMod();
            UpdateCurrentMap(false);
        }
        private void Input_MouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if(MouseInMenu())
            {
                tileMenu.Value.receiveScrollWheelAction(e.Delta > 0 ? 1 : -1);
            }
        }
        public static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree)
            {
                DeactivateMod();
                return;
            }

            if (e.Button == Config.ToggleButton)
            {
                SHelper.Input.Suppress(e.Button);
                modActive.Value = !modActive.Value;
                copiedTileLoc.Value = new Vector2(-1, -1);
                currentTileDict.Value.Clear();
                SMonitor.Log($"Toggled mod: {modActive}");
                tileMenu.Value = null;
                if (modActive.Value)
                    ShowMessage(string.Format(SHelper.Translation.Get("mod-active"), Config.ToggleButton));
                else
                    ShowMessage(string.Format(SHelper.Translation.Get("mod-inactive"), Config.ToggleButton));
            }
            else if (modActive.Value)
            {
                if (MouseInMenu())
                {
                    if(e.Button == SButton.MouseLeft)
                    {
                        tileMenu.Value.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY());
                    }
                    else if(e.Button == SButton.MouseRight)
                    {
                        tileMenu.Value.receiveRightClick(Game1.getMouseX(), Game1.getMouseY());
                    }
                    else
                    {
                        tileMenu.Value.receiveKeyPress((Keys)e.Button);
                    }
                    SHelper.Input.Suppress(e.Button);
                    return;
                }
                if (e.Button == Config.CopyButton)
                {
                    SHelper.Input.Suppress(e.Button);

                    CopyCurrentTile();

                }
                else if (currentTileDict.Value.Count > 0 && e.Button == Config.PasteButton && pastedTileLoc.Value != Game1.currentCursorTile)
                {
                    SHelper.Input.Suppress(e.Button);
                    PasteCurrentTile();

                }
                else if (e.Button == Config.RevertButton)
                {
                    if (SHelper.Input.IsDown(Config.RevertModButton))
                    {
                        RevertCurrentMap();
                    }
                    else if(MapHasTile(Game1.currentCursorTile))
                    {
                        RevertCurrentTile();
                    }
                    SHelper.Input.Suppress(e.Button);
                }
                else if (e.Button == SButton.Escape)
                {
                    SHelper.Input.Suppress(e.Button);
                    if (copiedTileLoc.Value.X > -1)
                    {
                        copiedTileLoc.Value = new Vector2(-1, -1);
                        pastedTileLoc.Value = new Vector2(-1, -1);
                        currentLayer.Value = null;
                        currentTileDict.Value.Clear();
                    }
                    else
                        DeactivateMod();
                }
                else if (e.Button == Config.RefreshButton)
                {
                    SHelper.Input.Suppress(e.Button);
                    cleanMaps.Clear();
                    GetMapCollectionData();
                    UpdateCurrentMap(true);
                }
                else if (e.Button == Config.ScrollUpButton)
                {
                    SHelper.Input.Suppress(e.Button);
                    SwitchTile(true);
                }
                else if (e.Button == Config.ScrollDownButton)
                {
                    SHelper.Input.Suppress(e.Button);
                    SwitchTile(false);
                }
            } 

        }

        public static void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod || !modActive.Value)
                return;

            if (Game1.activeClickableMenu != null)
            {
                modActive.Value = false;
                return;
            }
            if (tileMenu.Value is null)
            {
                tileMenu.Value = new();
            }
            TileSelectMenu.button.draw(e.SpriteBatch);

            if (MouseInMenu())
            {
                if (!tileMenu.Value.showing)
                {
                    tileMenu.Value.drawMouse(e.SpriteBatch, true);
                }
                return;
            }

            Vector2 mouseTile = Game1.currentCursorTile;
            Vector2 mouseTilePos = mouseTile * Game1.tileSize - new Vector2(Game1.viewport.X, Game1.viewport.Y);
            var ts1 = (Dictionary<TileSheet, Texture2D>)AccessTools.Field(Game1.mapDisplayDevice.GetType(), "m_tileSheetTextures")?.GetValue(Game1.mapDisplayDevice);
            var ts2 = (Dictionary<TileSheet, Texture2D>)AccessTools.Field(Game1.mapDisplayDevice.GetType(), "m_tileSheetTextures2")?.GetValue(Game1.mapDisplayDevice);
            if (currentTileDict.Value.Count > 0)
            {
                foreach (var kvp in currentTileDict.Value)
                {
                    int offset = kvp.Key.Equals("Front") ? (16 * Game1.pixelZoom) : 0;
                    float layerDepth = (copiedTileLoc.Value.Y * (16 * Game1.pixelZoom) + 16 * Game1.pixelZoom + offset) / 10000f;
                    Tile tile = kvp.Value;
                    if (tile == null)
                        continue;

                    var xRect = tile.TileSheet.GetTileImageBounds(tile.TileIndex);
                    Rectangle sourceRectangle = new Rectangle(xRect.X, xRect.Y, xRect.Width, xRect.Height);

                    Texture2D texture2D = null;
                    try
                    {
                        ts1?.TryGetValue(tile.TileSheet, out texture2D);
                    }
                    catch
                    {
                       ts2?.TryGetValue(tile.TileSheet, out texture2D);
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
        private void Display_RenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (!Config.EnableMod || !modActive.Value)
                return;
            if (tileMenu.Value is null)
            {
                tileMenu.Value = new TileSelectMenu();
            }
            tileMenu.Value.draw(e.SpriteBatch);
        }
    }
}