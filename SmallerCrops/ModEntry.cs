using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace SmallerCrops
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static int tileOffset = 100000;
        
        public static string keyPrefix = "aedenthorn.SmallerCrops/";
        public static Dictionary<string, Dictionary<Vector2, Crop[]>> drawCache = new Dictionary<string, Dictionary<Vector2, Crop[]>>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.ModEnabled)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            drawCache.Clear();
        }

        private void TerrainFeatures_OnValueRemoved(Vector2 key, StardewValley.TerrainFeatures.TerrainFeature value)
        {
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
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Ordinary / Day",
                getValue: () => Config.OrdinaryGiftsPerDay,
                setValue: value => Config.OrdinaryGiftsPerDay = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Ordinary / Week",
                getValue: () => Config.OrdinaryGiftsPerWeek,
                setValue: value => Config.OrdinaryGiftsPerWeek = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Friend / Day",
                getValue: () => Config.FriendGiftsPerDay,
                setValue: value => Config.FriendGiftsPerDay = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Friend / Week",
                getValue: () => Config.FriendGiftsPerWeek,
                setValue: value => Config.FriendGiftsPerWeek = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Dating / Day",
                getValue: () => Config.DatingGiftsPerDay,
                setValue: value => Config.DatingGiftsPerDay = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Dating / Week",
                getValue: () => Config.DatingGiftsPerWeek,
                setValue: value => Config.DatingGiftsPerWeek = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Spouse / Day",
                getValue: () => Config.SpouseGiftsPerDay,
                setValue: value => Config.SpouseGiftsPerDay = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Spouse / Week",
                getValue: () => Config.SpouseGiftsPerWeek,
                setValue: value => Config.SpouseGiftsPerWeek = value
            );
        }

    }
}