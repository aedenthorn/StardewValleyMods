using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FarmerCommissions
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static string dictPath = "aedenthorn.HelpWanted/dictionary";
        public static ModEntry context;
        public static IHelpWantedAPI helpWantedAPI;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.Edit(delegate(IAssetData data)
                {

                });
            }
        }


        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            helpWantedAPI = Helper.ModRegistry.GetApi<IHelpWantedAPI>("aedenthorn.HelpWanted");

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
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Must Like Item",
                getValue: () => Config.MustLikeItem,
                setValue: value => Config.MustLikeItem = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Must Love Item",
                getValue: () => Config.MustLoveItem,
                setValue: value => Config.MustLoveItem = value
            );
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Allow Artisan Goods",
                getValue: () => Config.AllowArtisanGoods,
                setValue: value => Config.AllowArtisanGoods = value
            );
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Ignore Game's Item Choice",
                getValue: () => Config.IgnoreVanillaItemSelection,
                setValue: value => Config.IgnoreVanillaItemSelection = value
            );
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "One Quest / Villager",
                getValue: () => Config.OneQuestPerVillager,
                setValue: value => Config.OneQuestPerVillager = value
            );
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Avoid Max Heart Villagers",
                getValue: () => Config.AvoidMaxHearts,
                setValue: value => Config.AvoidMaxHearts = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Item Price",
                getValue: () => Config.MaxPrice,
                setValue: value => Config.MaxPrice = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Days To Complete",
                getValue: () => Config.QuestDays,
                setValue: value => Config.QuestDays = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Quests",
                getValue: () => Config.MaxQuests,
                setValue: value => Config.MaxQuests = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Note Scale",
                getValue: () => Config.NoteScale + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.NoteScale = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Portrait Scale",
                getValue: () => Config.PortraitScale + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.PortraitScale = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "X Overlap Boundary",
                getValue: () => Config.XOverlapBoundary + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.XOverlapBoundary = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Y Overlap Boundary",
                getValue: () => Config.YOverlapBoundary+ "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.YOverlapBoundary = f; } }
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Portrait Offset X",
                getValue: () => Config.PortraitOffsetX,
                setValue: value => Config.PortraitOffsetX = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Portrait Offset Y",
                getValue: () => Config.PortraitOffsetY,
                setValue: value => Config.PortraitOffsetY = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Random Color Min",
                getValue: () => Config.RandomColorMin,
                setValue: value => Config.RandomColorMin = value,
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Random Color Max",
                getValue: () => Config.RandomColorMax,
                setValue: value => Config.RandomColorMax = value,
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Portrait Tint R",
                getValue: () => Config.PortraitTintR,
                setValue: value => Config.PortraitTintR = value,
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Portrait Tint G",
                getValue: () => Config.PortraitTintG,
                setValue: value => Config.PortraitTintG = value,
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Portrait Tint B",
                getValue: () => Config.PortraitTintB,
                setValue: value => Config.PortraitTintB = value,
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Portrait Tint A",
                getValue: () => Config.PortraitTintA,
                setValue: value => Config.PortraitTintA = value,
                min: 0,
                max: 255
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Resource Collect",
                getValue: () => Config.ResourceCollectionWeight+"",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.ResourceCollectionWeight = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Fishing",
                getValue: () => Config.FishingWeight+"",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.FishingWeight = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Slay Monster",
                getValue: () => Config.SlayMonstersWeight+"",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.SlayMonstersWeight = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Item Delivery",
                getValue: () => Config.ItemDeliveryWeight+"",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.ItemDeliveryWeight = f; } }
            );
        }
    }
}