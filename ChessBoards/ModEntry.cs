using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace ChessBoards
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static Object heldPiece;

        private static Texture2D piecesSheet;

        private static int parentIndex = 0;
        private static string pieceKey = "aedenthorn.ChessBoards/piece";
        private static string movedKey = "aedenthorn.ChessBoards/moved";
        private static string squareKey = "aedenthorn.ChessBoards/square";
        private static string flippedKey = "aedenthorn.ChessBoards/flipped";
        private static string lastKey = "aedenthorn.ChessBoards/lastPiece";
        private static string pawnKey = "aedenthorn.ChessBoards/pawn";
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
            if (!Config.EnableMod)
                return;
            if (heldPiece != null)
            {
                Vector2 scaleFactor = heldPiece.getScale();
                Vector2 position = Game1.getMousePosition().ToVector2() - new Vector2(32, 92);
                Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((heldPiece.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((heldPiece.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                float draw_layer = 1;
                e.SpriteBatch.Draw(piecesSheet, destination, GetSourceRectForPiece(heldPiece.modData[pieceKey]), Color.White * Config.HeldPieceOpacity, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree)
                return;

            if(GetChessBoardsBoardTileAt(Game1.lastCursorTile, out Point tile))
            {
                var cornerTile = new Vector2(Game1.lastCursorTile.X - tile.X + 1, Game1.lastCursorTile.Y + tile.Y - 1);
                if (!Game1.currentLocation.terrainFeatures.TryGetValue(cornerTile, out var t) || t is not Flooring)
                    return;
                Vector2 cursorTile = Game1.currentLocation.terrainFeatures[cornerTile].modData.ContainsKey(flippedKey) ? GetFlippedTile(cornerTile, tile) : Game1.lastCursorTile;
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
                            obj.modData[squareKey] = $"{x + 1},{y + 1}";
                            Game1.currentLocation.objects.Add(thisTile, obj);
                        }
                    }
                    PlaySound(Config.SetupSound);
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
                    PlaySound(Config.ClearSound);
                }
                else if (e.Button == Config.FlipKey)
                {
                    if (Game1.currentLocation.terrainFeatures[cornerTile].modData.ContainsKey(flippedKey))
                    {
                        Game1.currentLocation.terrainFeatures[cornerTile].modData.Remove(flippedKey);
                        Monitor.Log("Board unflipped");
                        PlaySound(Config.UnflipSound);
                    }
                    else
                    {
                        Game1.currentLocation.terrainFeatures[cornerTile].modData[flippedKey] = "true";
                        Monitor.Log("Board flipped");
                        PlaySound(Config.FlipSound);
                    }
                }
                else if (e.Button == Config.ModeKey)
                {
                    Config.FreeMode = !Config.FreeMode;
                    Game1.addHUDMessage(new HUDMessage(SHelper.Translation.Get("free-" + Config.FreeMode), 1));
                    PlaySound(Config.FreeMode ? Config.FlipSound : Config.UnflipSound);
                }
                else if (Config.FreeMode && e.Button == Config.RemoveKey)
                {
                    Game1.currentLocation.objects.Remove(cursorTile);
                    PlaySound(Config.CancelSound);
                }
                else if (Config.FreeMode && e.Button == Config.ChangeKey)
                {
                    if (Game1.currentLocation.objects.TryGetValue(cursorTile, out var obj) && obj.modData.TryGetValue(pieceKey, out string piece))
                    {
                        if (Helper.Input.IsDown(Config.ChangeModKey))
                        {
                            Game1.currentLocation.objects[cursorTile].modData[pieceKey] = (piece[0] == 'b' ? "w" : "b") + piece[1].ToString();
                        }
                        else
                        {
                            string which = pieceIndexes[(pieceIndexes.IndexOf(piece[1]) + 1) % pieceIndexes.Length].ToString();
                            Game1.currentLocation.objects[cursorTile].modData[pieceKey] = piece[0].ToString() + which;
                        }
                    }
                    else
                    {
                        var newObj = new Object(cursorTile, parentIndex);
                        newObj.modData[pieceKey] = "wp";
                        newObj.modData[squareKey] = $"{cursorTile.X - cornerTile.X + 1},{cornerTile.Y - cursorTile.Y + 1}";
                        Game1.currentLocation.objects.Add(cursorTile, newObj);
                    }
                    PlaySound(Config.PlaceSound);
                }
                else if (e.Button == SButton.MouseLeft)
                {
                    var lastMovedPiece = GetLastMovedPiece(cornerTile);
                    if (heldPiece is not null && cursorTile == heldPiece.TileLocation)
                    {
                        heldPiece = null;
                    }
                    else if (Game1.currentLocation.objects.TryGetValue(cursorTile, out var obj) && obj.modData.TryGetValue(pieceKey, out string piece))
                    {
                        if (heldPiece == null)
                        {
                            if ((lastMovedPiece == null && piece[0] == 'b') || (lastMovedPiece?.modData[pieceKey][0] == piece[0]))
                            {
                                Game1.addHUDMessage(new HUDMessage(SHelper.Translation.Get("turn"), 1));
                                PlaySound(Config.CancelSound);
                            }
                            else
                            {
                                heldPiece = obj;
                                PlaySound(Config.PickupSound);
                            }
                        }
                        else if (!IsValidMove(heldPiece, piece, lastMovedPiece, cornerTile, heldPiece.TileLocation, cursorTile, out bool enPassant, out bool castle, out bool pawnAdvance))
                        {
                            PlaySound(Config.CancelSound);
                            heldPiece = null;
                        }
                        else
                        {
                            MovePiece(cornerTile, cursorTile, enPassant, castle, pawnAdvance);
                        }
                    }
                    else if (heldPiece != null)
                    {
                        if (!IsValidMove(heldPiece, null, lastMovedPiece, cornerTile, heldPiece.TileLocation, cursorTile, out bool enPassant, out bool castle, out bool pawnAdvance))
                        {
                            PlaySound(Config.CancelSound);
                            heldPiece = null;
                        }
                        else
                        {
                            MovePiece(cornerTile, cursorTile, enPassant, castle, pawnAdvance);
                        }
                    }
                }
                else
                    return;
                Helper.Input.Suppress(e.Button);
            }
            else if (e.Button == SButton.MouseLeft && heldPiece != null)
            {
                PlaySound(Config.CancelSound);
                if (Config.FreeMode)
                    Game1.currentLocation.objects.Remove(heldPiece.TileLocation);
                heldPiece = null;
                Helper.Input.Suppress(e.Button);
            }
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                piecesSheet = Game1.content.Load<Texture2D>("aedenthorn.ChessBoards/pieces");
                Monitor.Log("Loaded custom pieces sheet");
            }
            catch
            {
                piecesSheet = Helper.ModContent.Load<Texture2D>("assets/pieces.png");
                Monitor.Log("Loaded default pieces sheet");
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
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => ModEntry.SHelper.Translation.Get("GMCM_Title_ChessPieces_Text")
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_SetupKey_Name"),
                tooltip: () => ModEntry.SHelper.Translation.Get("GMCM_Option_SetupKey_Tooltip"),
                getValue: () => Config.SetupKey,
                setValue: value => Config.SetupKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_FlipKey_Name"),
                tooltip: () => ModEntry.SHelper.Translation.Get("GMCM_Option_FlipKey_Tooltip"),
                getValue: () => Config.FlipKey,
                setValue: value => Config.FlipKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ClearKey_Name"),
                tooltip: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ClearKey_Tooltip"),
                getValue: () => Config.ClearKey,
                setValue: value => Config.ClearKey = value
            );

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => ModEntry.SHelper.Translation.Get("GMCM_Title_FreeMode_Text")
            );
            configMenu.AddParagraph(
                mod: ModManifest,
                text: () => ModEntry.SHelper.Translation.Get("GMCM_Paragraph_FreeMode_Text")
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModeKey_Name"),
                tooltip: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModeKey_Tooltip"),
                getValue: () => Config.ModeKey,
                setValue: value => Config.ModeKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ChangeKey_Name"),
                tooltip: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ChangeKey_Tooltip"),
                getValue: () => Config.ChangeKey,
                setValue: value => Config.ChangeKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ChangeModKey_Name"),
                tooltip: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ChangeModKey_Tooltip"),
                getValue: () => Config.ChangeModKey,
                setValue: value => Config.ChangeModKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_RemoveKey_Name"),
                tooltip: () => ModEntry.SHelper.Translation.Get("GMCM_Option_RemoveKey_Tooltip"),
                getValue: () => Config.RemoveKey,
                setValue: value => Config.RemoveKey = value
            );

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => ModEntry.SHelper.Translation.Get("GMCM_Title_SoundEffects_Text")
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_SetupSound_Name"),
                getValue: () => Config.SetupSound,
                setValue: value => Config.SetupSound = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_FlipSound_Name"),
                getValue: () => Config.FlipSound,
                setValue: value => Config.FlipSound = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ClearSound_Name"),
                getValue: () => Config.ClearSound,
                setValue: value => Config.ClearSound = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_PickupSound_Name"),
                getValue: () => Config.PickupSound,
                setValue: value => Config.PickupSound = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_PlaceSound_Name"),
                getValue: () => Config.PlaceSound,
                setValue: value => Config.PlaceSound = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_CancelSound_Name"),
                getValue: () => Config.CancelSound,
                setValue: value => Config.CancelSound = value
            );
        }
    }
}
