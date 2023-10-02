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
        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.DataType == typeof(Map))
            {
                foreach (string name in mapCollectionData.mapDataDict.Keys)
                {
                    if (e.NameWithoutLocale.IsEquivalentTo("Maps/" + name) || e.NameWithoutLocale.IsEquivalentTo(name))
                    {
                        e.Edit(delegate (IAssetData idata)
                        {
                            SMonitor.Log("Editing map " + e.Name);
                            var mapData = idata.AsMap();
                            MapData data = mapCollectionData.mapDataDict[name];
                            mapData.ReplaceWith(EditMap(e.Name.Name, mapData.Data, data));

                        });
                        return;
                    }
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
            UpdateCurrentMap(true);
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
            if (!Config.ModEnabled)
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
        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree)
            {
                DeactivateMod();
                return;
            }
            if(Config.ToggleButton.JustPressed())
            {
                foreach(var kb in Config.ToggleButton.Keybinds)
                {
                    foreach(var b in kb.Buttons)
                    {
                        SHelper.Input.Suppress(b);
                    }
                }
                modActive.Value = !modActive.Value;
                copiedTileLoc.Value = new Vector2(-1, -1);
                currentTileDict.Value.Clear();
                SMonitor.Log($"Toggled mod: {modActive}");
                tileMenu.Value = null;
                if (!Config.ShowMenu)
                {
                    if (modActive.Value)
                        ShowMessage(string.Format(SHelper.Translation.Get("mod-active"), Config.ToggleButton));
                    else
                        ShowMessage(string.Format(SHelper.Translation.Get("mod-inactive"), Config.ToggleButton));
                }
            }

        }
        public static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree)
            {
                DeactivateMod();
                return;
            }

            if (modActive.Value)
            {
                if(e.Button == Config.ToggleMenuButton)
                {
                    SHelper.Input.Suppress(e.Button);

                    Config.ShowMenu = !Config.ShowMenu;
                    Game1.playSound(Config.ShowMenu ? "bigSelect" : "bigDeSelect");
                    SMonitor.Log($"Toggled menu: {Config.ShowMenu}");
                    SHelper.WriteConfig(Config);
                    return;
                }
                if (MouseInMenu())
                {
                    if(e.Button == SButton.MouseLeft)
                    {
                        tileMenu.Value.receiveLeftClick(Game1.getMouseX(true), Game1.getMouseY(true));
                    }
                    else if(e.Button == SButton.MouseRight)
                    {
                        tileMenu.Value.receiveRightClick(Game1.getMouseX(true), Game1.getMouseY(true));
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
            if (!Config.ModEnabled || !modActive.Value)
                return;

            if (Game1.activeClickableMenu != null)
            {
                modActive.Value = false;
                return;
            }

            if (MouseInMenu())
            {
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
            if (!Config.ModEnabled || !modActive.Value)
                return;
            if (tileMenu.Value is null)
            {
                tileMenu.Value = new TileSelectMenu();
            }

            tileMenu.Value.draw(e.SpriteBatch);
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_UseSaveSpecificEdits_Name"),
                    getValue: () => Config.UseSaveSpecificEdits,
                    setValue: value => Config.UseSaveSpecificEdits = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_IncludeGlobalEdits_Name"),
                    getValue: () => Config.IncludeGlobalEdits,
                    setValue: value => Config.IncludeGlobalEdits = value
                );



                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => SHelper.Translation.Get("GMCM_Option_Buttons_Text")
                );

                
                configMenu.AddKeybindList(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ToggleButton_Name"),
                    getValue: () => Config.ToggleButton,
                    setValue: value => Config.ToggleButton = value
                );
                
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ToggleMenuButton_Name"),
                    getValue: () => Config.ToggleMenuButton,
                    setValue: value => Config.ToggleMenuButton = value
                );
                
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_RefreshButton_Name"),
                    getValue: () => Config.RefreshButton,
                    setValue: value => Config.RefreshButton = value
                );
                
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_CopyButton_Name"),
                    getValue: () => Config.CopyButton,
                    setValue: value => Config.CopyButton = value
                );
                
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_PasteButton_Name"),
                    getValue: () => Config.PasteButton,
                    setValue: value => Config.PasteButton = value
                );
                
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_RevertButton_Name"),
                    getValue: () => Config.RevertButton,
                    setValue: value => Config.RevertButton = value
                );
                
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_RevertModButton_Name"),
                    getValue: () => Config.RevertModButton,
                    setValue: value => Config.RevertModButton = value
                );
                
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ScrollUpButton_Name"),
                    getValue: () => Config.ScrollUpButton,
                    setValue: value => Config.ScrollUpButton = value
                );
                
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ScrollDownButton_Name"),
                    getValue: () => Config.ScrollDownButton,
                    setValue: value => Config.ScrollDownButton = value
                );
                
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_LayerModButton_Name"),
                    getValue: () => Config.LayerModButton,
                    setValue: value => Config.LayerModButton = value
                );
                
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_SheetModButton_Name"),
                    getValue: () => Config.SheetModButton,
                    setValue: value => Config.SheetModButton = value
                );
                


                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => SHelper.Translation.Get("GMCM_Option_Sounds_Text")
                );

                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_CopySound_Name"),
                    getValue: () => Config.CopySound,
                    setValue: value => Config.CopySound = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_PasteSound_Name"),
                    getValue: () => Config.PasteSound,
                    setValue: value => Config.PasteSound = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ScrollSound_Name"),
                    getValue: () => Config.ScrollSound,
                    setValue: value => Config.ScrollSound = value
                );

                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => SHelper.Translation.Get("GMCM_Option_Misc_Text")
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_BorderThickness_Name"),
                    getValue: () => Config.BorderThickness,
                    setValue: value => Config.BorderThickness = value,
                    min: 1,
                    max: 16
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_DefaultAnimationInterval_Name"),
                    getValue: () => Config.DefaultAnimationInterval,
                    setValue: value => Config.DefaultAnimationInterval = value
                );
            }
        }
    }
}