using Force.DeepCloner;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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
        public static float timeShredded;

        private static Dictionary<string, SongData> songDataDict = new Dictionary<string, SongData>();
        public static readonly string dictPath = "aedenthorn.GuitardewValleyHero/dictionary";
        private static List<string> loadedContentPacks = new List<string>();

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
            bool stillShredding = false;
            int beatIndex = 0;
            foreach(var beat in currentData.beatDataList)
            {
                var width = currentData.noteScale * 16;
                var height = currentData.noteScale * 16 * beat.length;
                var yPos = timeShredded - currentData.noteScale * 16 * (beatIndex + beat.length);
                beatIndex += beat.length;
                if (yPos > Game1.viewport.Height)
                    continue;
                stillShredding = true;
                if (yPos <= -height || beat.fret < 0)
                    continue;
                var xPos = Game1.viewport.Width / 2 - currentData.noteScale * 16 * 4 / 2 + currentData.noteScale * 16 * beat.fret;
                Rectangle? sourceRectangle = new Rectangle?(GameLocation.getSourceRectForObject(beat.index >= 0 ? beat.index : currentData.defaultIconIndexes[beat.fret]));
                if(beat.length > 1)
                    e.SpriteBatch.Draw(Game1.objectSpriteSheet, new Rectangle((int)xPos, (int)yPos, (int)width, (int)height), sourceRectangle, Color.White * 0.5f);
                e.SpriteBatch.Draw(Game1.objectSpriteSheet, new Rectangle((int)xPos, (int)(yPos + height - width), (int)width, (int)width), sourceRectangle, Color.White);
            }
            timeShredded += currentData.speed;
            if (!stillShredding)
            {
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