using HarmonyLib;
using StardewModdingAPI;

namespace CustomGiftLimits
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

            if (!Config.ModEnabled)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

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
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_OrdinaryGiftsPerDay_Name"),
                getValue: () => Config.OrdinaryGiftsPerDay,
                setValue: value => Config.OrdinaryGiftsPerDay = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_OrdinaryGiftsPerWeek_Name"),
                getValue: () => Config.OrdinaryGiftsPerWeek,
                setValue: value => Config.OrdinaryGiftsPerWeek = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_FriendGiftsPerDay_Name"),
                getValue: () => Config.FriendGiftsPerDay,
                setValue: value => Config.FriendGiftsPerDay = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_FriendGiftsPerWeek_Name"),
                getValue: () => Config.FriendGiftsPerWeek,
                setValue: value => Config.FriendGiftsPerWeek = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_DatingGiftsPerDay_Name"),
                getValue: () => Config.DatingGiftsPerDay,
                setValue: value => Config.DatingGiftsPerDay = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_DatingGiftsPerWeek_Name"),
                getValue: () => Config.DatingGiftsPerWeek,
                setValue: value => Config.DatingGiftsPerWeek = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_SpouseGiftsPerDay_Name"),
                getValue: () => Config.SpouseGiftsPerDay,
                setValue: value => Config.SpouseGiftsPerDay = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_SpouseGiftsPerWeek_Name"),
                getValue: () => Config.SpouseGiftsPerWeek,
                setValue: value => Config.SpouseGiftsPerWeek = value
            );
        }

    }
}