using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LoadingScreens
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        
        public static string dictPath = "aedenthorn.LoadingScreens/dictionary";
        
        public static List<KeyValuePair<string, LoadingScreen>> screenDict = new();
        public static LoadingScreen currentLoadingScreen;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            Helper.Events.Display.Rendered += Display_Rendered;

            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if(e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, LoadingScreen>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            screenDict = Helper.GameContent.Load<Dictionary<string, LoadingScreen>>(dictPath).ToList();
            foreach(var f in Directory.GetFiles(Path.Combine(Helper.DirectoryPath, "Screens"), "*.png", SearchOption.AllDirectories))
            {
                screenDict.Add(new(Path.GetFileNameWithoutExtension(f), new LoadingScreen() { texture = Texture2D.FromFile(Game1.graphics.GraphicsDevice, f) }));
            }
            foreach(var f in Directory.GetFiles(Path.Combine(Helper.DirectoryPath, "Screens"), "*.jpg", SearchOption.AllDirectories))
            {
                screenDict.Add(new(Path.GetFileNameWithoutExtension(f), new LoadingScreen() { texture = Texture2D.FromFile(Game1.graphics.GraphicsDevice, f) }));
            }
        }

        private void Display_Rendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (Game1.gameMode != 0 && Game1.fadeToBlackAlpha > 0 && (Config.ShowOnWarp || !Context.IsWorldReady))
            {
                DrawLoadingScreen(e.SpriteBatch);
            }
            else
            {
                currentLoadingScreen = null;
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
