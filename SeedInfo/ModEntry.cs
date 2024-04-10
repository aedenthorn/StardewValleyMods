﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SeedInfo
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static int[] qualities = new int[] { 0, 1, 2, 4 };

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

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                name: () => "Days Per Month",
                getValue: () => Config.DaysPerMonth,
                setValue: value => Config.DaysPerMonth = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Price Color R",
                getValue: () => Config.PriceColor.R,
                setValue: value => Config.PriceColor = new Color(value, Config.PriceColor.G, Config.PriceColor.B, Config.PriceColor.A),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Price Color G",
                getValue: () => Config.PriceColor.G,
                setValue: value => Config.PriceColor = new Color(Config.PriceColor.R, value, Config.PriceColor.B, Config.PriceColor.A),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Price Color B",
                getValue: () => Config.PriceColor.B,
                setValue: value => Config.PriceColor = new Color(Config.PriceColor.R, Config.PriceColor.G, value, Config.PriceColor.A),
                min: 0,
                max: 255
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Display Mead",
                tooltip: () => "Whether to display mead as the keg option for flowers, which have no keg option otherwise.",
                getValue: () => Config.DisplayMead,
                setValue: value => Config.DisplayMead = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Display Crop",
                tooltip: () => "Whether to display the base crop value in the shop menu.",
                getValue: () => Config.DisplayCrop,
                setValue: value => Config.DisplayCrop = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Display Pickle",
                tooltip: () => "Whether to display the preserve jar output in the shop menu.",
                getValue: () => Config.DisplayPickle,
                setValue: value => Config.DisplayPickle = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Display Keg",
                tooltip: () => "Whether to display the keg output in the shop menu.",
                getValue: () => Config.DisplayKeg,
                setValue: value => Config.DisplayKeg = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Display Dehydrate",
                tooltip: () => "Whether to display the dehydrator output in the shop menu.",
                getValue: () => Config.DisplayDehydrator,
                setValue: value => Config.DisplayDehydrator = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Divide Dehydrate Value",
                tooltip: () => "Whether to divide the dehydrator value by five to reflect the value per item.",
                getValue: () => Config.DivideDehydrate,
                setValue: value => Config.DivideDehydrate = value
            );
        }

    }
}