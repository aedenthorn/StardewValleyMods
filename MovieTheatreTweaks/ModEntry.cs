using HarmonyLib;
using StardewModdingAPI;

namespace MovieTheatreTweaks
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

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Remove Crane Man",
                getValue: () => Config.RemoveCraneMan,
                setValue: value => Config.RemoveCraneMan = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Open Time",
                getValue: () => Config.OpenTime,
                setValue: value => Config.OpenTime = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Close Time",
                getValue: () => Config.CloseTime,
                setValue: value => Config.CloseTime = value
            );
        }

    }
}