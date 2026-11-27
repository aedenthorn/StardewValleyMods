using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace CropVariation
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        private static string sizeVarKey = "aedenthorn.CropVariation/sizeVar";
        private static string redVarKey = "aedenthorn.CropVariation/redVar";
        private static string greenVarKey = "aedenthorn.CropVariation/greenVar";
        private static string blueVarKey = "aedenthorn.CropVariation/blueVar";

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
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
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EnableTrellisResize_Name"),
                getValue: () => Config.EnableTrellisResize,
                setValue: value => Config.EnableTrellisResize = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_SizeVariationPercent_Name"),
                getValue: () => Config.SizeVariationPercent,
                setValue: value => Config.SizeVariationPercent = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ColorVariation_Name"),
                getValue: () => Config.ColorVariation,
                setValue: value => Config.ColorVariation = value,
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_SizeVariationQualityFactor_Name"),
                getValue: () => Config.SizeVariationQualityFactor,
                setValue: value => Config.SizeVariationQualityFactor = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ColorVariationQualityFactor_Name"),
                getValue: () => Config.ColorVariationQualityFactor,
                setValue: value => Config.ColorVariationQualityFactor = value,
                min: 0,
                max: 100
            );
        }
    }
}