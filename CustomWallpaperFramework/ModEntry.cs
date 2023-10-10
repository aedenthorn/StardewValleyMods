using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Locations;
using System;
using System.Collections.Generic;

namespace CustomWallpaperFramework
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static readonly string dictPath = "aedenthorn.CustomWallpaperFramework/dictionary";
        public static Dictionary<string, WallpaperData> wallpaperDataDict = new Dictionary<string, WallpaperData>();
        public static Dictionary<DecoratableLocation, Dictionary<string, WallPaperTileData>> locationDataDict = new Dictionary<DecoratableLocation, Dictionary<string, WallPaperTileData>>();

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
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, WallpaperData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            wallpaperDataDict = SHelper.Content.Load<Dictionary<string, WallpaperData>>(dictPath, ContentSource.GameContent) ?? new Dictionary<string, WallpaperData>();
            foreach(var data in wallpaperDataDict.Values)
            {
                data.texture = Game1.content.Load<Texture2D>(data.texturePath);
            }
            Monitor.Log($"Loaded {wallpaperDataDict.Count} wallpapers");
            foreach (var loc in Game1.locations)
            {
                if (loc is DecoratableLocation)
                {
                    (loc as DecoratableLocation).setWallpapers();
                }
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
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
        }
    }
}