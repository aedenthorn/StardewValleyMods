using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using xTile.Layers;
using xTile.Tiles;

namespace MapEdit
{
    public class HelperEvents
    {
        public static ModConfig Config;
        public static IModHelper Helper;
        public static IMonitor Monitor;

        public static void Initialize(ModConfig config, IMonitor monitor, IModHelper helper)
        {
            Config = config;
            Helper = helper;
            Monitor = monitor;
        }
        public static void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            ModActions.DeactivateMod();
            ModEntry.cleanMaps.Clear();
        }

        public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            MapActions.GetMapCollectionData();
        }


        public static void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (ModEntry.modActive.Value && (Helper.Input.IsDown(Config.PasteButton) || Helper.Input.IsSuppressed(Config.PasteButton)) && ModEntry.pastedTileLoc.Value.X > -1 && ModEntry.pastedTileLoc.Value != Game1.currentCursorTile)
            {
                TileActions.PasteCurrentTile();
            }
            else if (ModEntry.modActive.Value && (Helper.Input.IsDown(Config.RevertButton) || Helper.Input.IsSuppressed(Config.RevertButton)) && MapActions.MapHasTile(Game1.currentCursorTile))
            {
                TileActions.RevertCurrentTile();
            }
        }


        public static void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            ModActions.DeactivateMod();
            MapActions.UpdateCurrentMap(false);
        }

        public static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree)
            {
                ModActions.DeactivateMod();
                return;
            }

            if (e.Button == Config.ToggleButton)
            {
                Helper.Input.Suppress(e.Button);
                ModEntry.modActive.Value = !ModEntry.modActive.Value;
                ModEntry.copiedTileLoc.Value = new Vector2(-1, -1);
                ModEntry.currentTileDict.Value.Clear();
                Monitor.Log($"Toggled mod: {ModEntry.modActive}");
                if (ModEntry.modActive.Value)
                    ModActions.ShowMessage(string.Format(Helper.Translation.Get("mod-active"), Config.ToggleButton));
                else
                    ModActions.ShowMessage(string.Format(Helper.Translation.Get("mod-inactive"), Config.ToggleButton));
            }
            else if (ModEntry.modActive.Value) 
            {
                if (e.Button == Config.CopyButton)
                {
                    Helper.Input.Suppress(e.Button);

                    TileActions.CopyCurrentTile();

                }
                else if (ModEntry.copiedTileLoc.Value.X > -1 && e.Button == Config.PasteButton && ModEntry.pastedTileLoc.Value != Game1.currentCursorTile)
                {
                    Helper.Input.Suppress(e.Button);
                    TileActions.PasteCurrentTile();

                }
                else if (e.Button == Config.RevertButton && MapActions.MapHasTile(Game1.currentCursorTile))
                {
                    Helper.Input.Suppress(e.Button);
                    TileActions.RevertCurrentTile();
                }
                else if (e.Button == SButton.Escape)
                {
                    Helper.Input.Suppress(e.Button);
                    if (ModEntry.copiedTileLoc.Value.X > -1)
                    {
                        ModEntry.copiedTileLoc.Value = new Vector2(-1, -1);
                        ModEntry.pastedTileLoc.Value = new Vector2(-1, -1);
                        ModEntry.currentLayer.Value = 0;
                        ModEntry.currentTileDict.Value.Clear();
                    }
                    else
                        ModActions.DeactivateMod();
                }
                else if (e.Button == Config.RefreshButton)
                {
                    Helper.Input.Suppress(e.Button);
                    ModEntry.cleanMaps.Clear();
                    MapActions.GetMapCollectionData();
                    MapActions.UpdateCurrentMap(true);
                }
                else if (e.Button == Config.ScrollUpButton)
                {
                    Helper.Input.Suppress(e.Button);
                    ModActions.SwitchTile(true);
                }
                else if (e.Button == Config.ScrollDownButton)
                {
                    Helper.Input.Suppress(e.Button);
                    ModActions.SwitchTile(false);
                }
            } 

        }

        public static void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod || !ModEntry.modActive.Value)
                return;

            if (Game1.activeClickableMenu != null)
            {
                ModEntry.modActive.Value = false;
                return;
            }

            Vector2 mouseTile = Game1.currentCursorTile;
            Vector2 mouseTilePos = mouseTile * Game1.tileSize - new Vector2(Game1.viewport.X, Game1.viewport.Y);
            var ts1 = (Dictionary<TileSheet, Texture2D>)AccessTools.Field(Game1.mapDisplayDevice.GetType(), "m_tileSheetTextures")?.GetValue(Game1.mapDisplayDevice);
            var ts2 = (Dictionary<TileSheet, Texture2D>)AccessTools.Field(Game1.mapDisplayDevice.GetType(), "m_tileSheetTextures2")?.GetValue(Game1.mapDisplayDevice);
            if (ModEntry.copiedTileLoc.Value.X > -1)
            {
                foreach (var kvp in ModEntry.currentTileDict.Value)
                {
                    int offset = kvp.Key.Equals("Front") ? (16 * Game1.pixelZoom) : 0;
                    float layerDepth = (ModEntry.copiedTileLoc.Value.Y * (16 * Game1.pixelZoom) + 16 * Game1.pixelZoom + offset) / 10000f;
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
                e.SpriteBatch.Draw(ModEntry.copiedTexture, mouseTilePos, Color.White);

            }
            else if (MapActions.MapHasTile(mouseTile))
                e.SpriteBatch.Draw(ModEntry.existsTexture, mouseTilePos, Color.White);
            else
                e.SpriteBatch.Draw(ModEntry.activeTexture, mouseTilePos, Color.White);

        }
    }
}