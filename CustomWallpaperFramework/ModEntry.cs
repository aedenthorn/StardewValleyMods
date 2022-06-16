using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace CustomWallpaperFramework
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
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

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(e.Button == SButton.Y)
            {
                List<ModWallpaperOrFlooring> list = Game1.content.Load<List<ModWallpaperOrFlooring>>("Data\\AdditionalWallpaperFlooring");
                foreach(ModWallpaperOrFlooring wallpaper in list)
                {
                    Monitor.Log($"modded wallpaper {wallpaper.ID}");
                }
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

            return (T)(object)new Dictionary<string, WallpaperData>();
        }
    }
}