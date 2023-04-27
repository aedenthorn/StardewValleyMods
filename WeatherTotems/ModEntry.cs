using HarmonyLib;
using StardewModdingAPI;
using System;
using System.Globalization;
using System.Linq;

namespace WeatherTotems
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.ModEnabled)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled || !e.NameWithoutLocale.IsEquivalentTo("Data/ObjectInformation"))
                return;
            e.Edit(delegate (IAssetData data)
            {
                data.AsDictionary<int, string>().Data[681] = SHelper.Translation.Get("object-info");
            });
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

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Invoke Sound",
                getValue: () => Config.InvokeSound,
                setValue: value => Config.InvokeSound = value
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Cloudy Sound",
                getValue: () => Config.CloudySound,
                setValue: value => Config.CloudySound = value
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Rain Sound",
                getValue: () => Config.RainSound,
                setValue: value => Config.RainSound = value
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Thunder Sound",
                getValue: () => Config.ThunderSound,
                setValue: value => Config.ThunderSound = value
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Sunny Sound",
                getValue: () => Config.SunnySound,
                setValue: value => Config.SunnySound = value
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Snowy Sound",
                getValue: () => Config.SnowSound,
                setValue: value => Config.SnowSound = value
            );
        }

    }
}