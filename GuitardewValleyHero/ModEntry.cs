using Force.DeepCloner;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace GuitardewValleyHero
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        private static SongData currentData;
        public static string currentSong;
        public static bool isShredding;
        public static bool isIntro;
        public static int introLength;
        public static float timeShredded;
        public static int shredScore;
        private static int targetStart;

        private static Texture2D barTexture;

        private static Dictionary<string, SongData> songDataDict = new Dictionary<string, SongData>();
        private static Dictionary<string, int> highScores = new Dictionary<string, int>();
        public static readonly string dictPath = "aedenthorn.GuitardewValleyHero/dictionary";
        private static List<string> loadedContentPacks = new List<string>();
        private static bool[] keysPressed = new bool[4];
        private static float[] timePressed = new float[4];

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
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }
        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (Config.ResetKeys.JustPressed())
            {
                Monitor.Log($"Reloading {loadedContentPacks.Count} content pack(s)");
                foreach (var pack in loadedContentPacks)
                {
                    Helper.ConsoleCommands.Trigger("patch", new string[] { "reload", pack });
                }
                ReloadSongData();
            }
        }

        private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod || !isShredding)
                return;

            if(isIntro && introLength < timeShredded)
            {
                isIntro = false;
                Game1.currentSong = Game1.soundBank.GetCue(currentSong);
            }

            keysPressed = new bool[] { Config.FretKey1.JustPressed(), Config.FretKey2.JustPressed(), Config.FretKey3.JustPressed(), Config.FretKey4.JustPressed() };
            for (int j = 0; j < 4; j++)
            {
                if (keysPressed[j])
                    timePressed[j] = timeShredded;
            }

            int width = (int)(currentData.noteScale * 16);

            float[] buttonSwells = new float[] { 1, 1, 1, 1 };
            for (int j = 0; j < 4; j++)
            {
                if (timePressed[j] == 0)
                    continue;
                if (timeShredded - timePressed[j] < Config.ButtonSwell / 2)
                    buttonSwells[j] = 1 + (timeShredded - timePressed[j]) / width;
                else if (timeShredded - timePressed[j] < Config.ButtonSwell)
                    buttonSwells[j] = 1 + (Config.ButtonSwell - (timeShredded - timePressed[j])) / width;
            }


            int barX = Game1.viewport.Width / 2 - barTexture.Width / 2;
            e.SpriteBatch.Draw(barTexture, new Vector2(barX, targetStart), null, Color.White);
            e.SpriteBatch.Draw(barTexture, new Vector2(barX, targetStart + width), null, Color.White);

            KeybindList[] keys = new KeybindList[] { Config.FretKey1, Config.FretKey2, Config.FretKey3, Config.FretKey4 };
            for (int j = 0; j < 4; j++)
            {
                Game1.drawDialogueBox(barX + width * j + width / 2 - 128, targetStart + width + width / 2 - 128, 256, 256, false, true, null, false, true, -1, -1, -1);
                e.SpriteBatch.DrawString(Game1.dialogueFont, keys[j].ToString(), new Vector2(barX + width * j + width / 2 - 16 * buttonSwells[j], targetStart + width + width / 2 - 16 * buttonSwells[j]), buttonSwells[j] > 1 ? Color.Green : Color.White, 0, Vector2.Zero, 2 * buttonSwells[j], SpriteEffects.None, 1);
            }

            bool stillShredding = false;
            bool nextNote = false;
            for(int i = 0; i < currentData.beatDataList.Count; i++)
            {
                float notePos = timeShredded - currentData.noteScale * 16 * (i + 1);

                foreach (var note in currentData.beatDataList[i])
                {
                    var height = currentData.noteScale * 16 * note.length;
                    var yPos = timeShredded - currentData.noteScale * 16 * (i + note.length);
                    if (yPos > Game1.viewport.Height)
                        continue;
                    stillShredding = true;
                    if (yPos <= -height || note.fret < 0)
                        continue;
                    var xPos = Game1.viewport.Width / 2 - currentData.noteScale * 16 * 4 / 2 + currentData.noteScale * 16 * note.fret;
                    Rectangle? sourceRectangle = new Rectangle?(GameLocation.getSourceRectForObject(note.index >= 0 ? note.index : currentData.defaultIconIndexes[note.fret]));
                    if (note.length > 1)
                        e.SpriteBatch.Draw(Game1.objectSpriteSheet, new Rectangle((int)xPos, (int)yPos, (int)width, (int)height), sourceRectangle, Color.White * 0.5f);
                    e.SpriteBatch.Draw(Game1.objectSpriteSheet, new Rectangle((int)xPos, (int)(yPos + height - width), (int)width, (int)width), sourceRectangle, Color.White);
                }
                if (!nextNote && notePos < targetStart + width / 2)
                {
                    nextNote = true;
                    for (int j = 0; j < keysPressed.Length; j++)
                    {
                        if (keysPressed[j])
                        {
                            var noteScore = 0;
                            bool found = false;
                            foreach (var note in currentData.beatDataList[i])
                            {
                                if(note.fret == j)
                                {
                                    noteScore += GetNoteScore(notePos);
                                    found = true;
                                }
                            }
                            if (!found)
                                noteScore = currentData.missScore;
                            shredScore += noteScore;
                            Game1.player.currentLocation.debris.Add(new Debris(Math.Abs(noteScore), new Vector2((Game1.player.getStandingX() + Game1.random.Next(-100, 100)), Game1.player.getStandingY() + Game1.random.Next(100)), noteScore == currentData.perfectScore ? Color.Green : (noteScore > 0 ? Color.Yellow : Color.Red), 1f, Game1.player));
                        }
                    }
                }

            }
            e.SpriteBatch.DrawString(Game1.dialogueFont, $"Score: {shredScore}", new Vector2(Game1.viewport.Width / 2 + width * 2 + 64, targetStart + width / 2), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 1f);


            timeShredded += currentData.speed;
            if (!stillShredding)
            {
                if(!highScores.TryGetValue(currentSong, out int highScore) || shredScore * 100 > highScore)
                {
                    highScores[currentSong] = shredScore;
                    Game1.addHUDMessage(new HUDMessage($"You got a new high score of {shredScore} for {Utility.getSongTitleFromCueName(currentSong)}!"));
                }
                Game1.player.canMove = true;
                isShredding = false;
                currentData = null;
                currentSong = null;
                timeShredded = 0;
            }
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            ReloadSongData();
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

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            return asset.AssetNameEquals(dictPath);
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Monitor.Log("Loading dictionary");

            return (T)(object)new Dictionary<string, SongData>();
        }
    }
}