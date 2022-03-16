using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace SelfServe
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        private Harmony harmony;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_performAction_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(IslandSouth), nameof(IslandSouth.checkAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.IslandSouth_checkAction_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.openShopMenu)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_openShopMenu_Prefix))
            );
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
                name: () => "Enable Mod",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Animal Shop",
                getValue: () => Config.AnimalShop,
                setValue: value => Config.AnimalShop = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Carpenter",
                getValue: () => Config.CarpenterShop,
                setValue: value => Config.CarpenterShop = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Fish Shop",
                getValue: () => Config.FishShop,
                setValue: value => Config.FishShop = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Hospital Shop",
                getValue: () => Config.HospitalShop,
                setValue: value => Config.HospitalShop = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Ice Cream Stand",
                getValue: () => Config.IceCreamShop,
                setValue: value => Config.IceCreamShop = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Sandy Shop",
                getValue: () => Config.SandyShop,
                setValue: value => Config.SandyShop = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Saloon Shop",
                getValue: () => Config.SaloonShop,
                setValue: value => Config.SaloonShop = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Seed Shop",
                getValue: () => Config.SeedShop,
                setValue: value => Config.SeedShop = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Smith Shop",
                getValue: () => Config.SmithShop,
                setValue: value => Config.SmithShop = value
            );
        }
    }
}