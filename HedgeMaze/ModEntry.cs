using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using xTile;

namespace HedgeMaze
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;
            context = this;


            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            Helper.GameContent.InvalidateCache("Maps/Woods");
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            Helper.GameContent.InvalidateCache("Maps/Woods");
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/Woods"))
            {
                e.LoadFromModFile<Map>(Path.Combine("assets", "WoodsMaze.tmx"), AssetLoadPriority.Exclusive);
                e.Edit(ModifyMap, AssetEditPriority.Early);
            }
        }
        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
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
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
        }
    }
}