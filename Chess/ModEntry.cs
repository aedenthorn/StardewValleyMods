using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace Chess
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static Object heldPiece;

        public static bool flipped;

        private static Texture2D piecesSheet;

        private static int parentIndex = -424242;
        private static string pieceKey = "aedenthorn.Chess/piece";
        private static string movedKey = "aedenthorn.Chess/moved";
        private static string lastKey = "aedenthorn.Chess/lastPiece";
        private static string[][] startPieces = new string[][]
        {
            new string[]
            {
                "wr", "wn", "wb", "wq", "wk", "wb", "wn", "wr"
            },
            new string[]
            {
                "wp", "wp", "wp", "wp", "wp", "wp", "wp", "wp"
            },
            new string[0],
            new string[0],
            new string[0],
            new string[0],
            new string[]
            {
                "bp", "bp", "bp", "bp", "bp", "bp", "bp", "bp"
            },
            new string[]
            {
                "br", "bn", "bb", "bq", "bk", "bb", "bn", "br"
            }
        };

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if(heldPiece != null)
            {
                Vector2 scaleFactor = heldPiece.getScale();
                Vector2 position = Game1.getMousePosition().ToVector2() - new Vector2(32, 92);
                Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((heldPiece.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((heldPiece.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                float draw_layer = 1;
                e.SpriteBatch.Draw(piecesSheet, destination, new Rectangle(GetSourceRectForPiece(heldPiece.modData[pieceKey]), new Point(64, 128)), Color.White * 0.5f, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod)
                return;

            if(Context.IsPlayerFree && GetChessBoardTileAt(Game1.lastCursorTile, out Point tile))
            {
                var cornerTile = new Vector2(Game1.lastCursorTile.X - tile.X + 1, Game1.lastCursorTile.Y + tile.Y - 1);
                if (e.Button == Config.SetupKey)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        for (int y = 0; y < 8; y++)
                        {
                            var thisTile = new Vector2(cornerTile.X + x, cornerTile.Y - y);
                            Game1.currentLocation.objects.Remove(thisTile);
                            if (startPieces[y].Length == 0)
                                continue;
                            var obj = new Object(thisTile, parentIndex);
                            obj.modData[pieceKey] = startPieces[y][x];
                            Game1.currentLocation.objects.Add(thisTile, obj);
                        }
                    }
                    Game1.currentLocation.playSound("dwoop");
                    Helper.Input.Suppress(e.Button);
                    return;
                }
                else if (e.Button == Config.ClearKey)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        for (int y = 0; y < 8; y++)
                        {
                            var thisTile = new Vector2(cornerTile.X + x, cornerTile.Y - y);
                            Game1.currentLocation.objects.Remove(thisTile);
                        }
                    }
                    Game1.currentLocation.playSound("leafrustle");
                    Helper.Input.Suppress(e.Button);
                    return;
                }
                if (e.Button == SButton.MouseLeft)
                {
                    var lastMovedPiece = GetLastMovedPiece(cornerTile);
                    if (heldPiece is not null &&  Game1.lastCursorTile == heldPiece.TileLocation)
                    {
                        heldPiece = null;
                    }
                    else if (Game1.currentLocation.objects.TryGetValue(Game1.lastCursorTile, out var obj) && obj.modData.TryGetValue(pieceKey, out string piece))
                    {
                        if (heldPiece == null)
                        {
                            heldPiece = obj;
                            Game1.currentLocation.playSound("bigSelect");
                        }
                        else if (!IsValidMove(heldPiece, piece, lastMovedPiece, cornerTile, heldPiece.TileLocation, Game1.lastCursorTile, out bool enPassant, out bool castle))
                        {
                            Game1.currentLocation.playSound("leafrustle");
                            heldPiece = null;
                        }
                        else
                        {
                            MovePiece(cornerTile, enPassant, castle);
                            heldPiece = null;
                        }
                    }
                    else if (heldPiece != null)
                    {
                        if (!IsValidMove(heldPiece, null, lastMovedPiece, cornerTile, heldPiece.TileLocation, Game1.lastCursorTile, out bool enPassant, out bool castle))
                        {
                            Game1.currentLocation.playSound("leafrustle");
                        }
                        else
                        {
                            MovePiece(cornerTile, enPassant, castle);
                        }
                        heldPiece = null;
                    }
                    Helper.Input.Suppress(e.Button);
                    return;
                }
            }
            else if (e.Button == SButton.MouseLeft && heldPiece != null)
            {
                Game1.currentLocation.playSound("leafrustle");
                Game1.currentLocation.objects.Remove(heldPiece.TileLocation);
                heldPiece = null;
                Helper.Input.Suppress(e.Button);
            }
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                piecesSheet = Game1.content.Load<Texture2D>("aedenthorn.Chess/pieces");
            }
            catch
            {
                piecesSheet = Helper.Content.Load<Texture2D>("assets/pieces.png");
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
        }
    }
}