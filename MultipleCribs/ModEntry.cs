using HarmonyLib;
using StardewModdingAPI;

namespace MultipleCribs
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static IFreeLoveAPI freeLoveAPI;
        public static ModConfig Config;

        public static ModEntry context;

        public static string partnerName;

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
        public override object GetApi()
        {
            return new MultipleCribsAPI();
        }
        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            freeLoveAPI = SHelper.ModRegistry.GetApi<IFreeLoveAPI>("aedenthorn.FreeLove");

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
                name: () => SHelper.Translation.Get("Mod.Options.IsEnabled"),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Mod.Options.InBedOnly.Name"),
                tooltip: () => SHelper.Translation.Get("Mod.Options.InBedOnly.Description"),
                getValue: () => Config.InBed,
                setValue: value => Config.InBed = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("Mod.Options.MinHearts.Name"),
                getValue: () => Config.MinHearts,
                setValue: value => Config.MinHearts = value,
                min: 0,
                max: 14
            );
        }
    }
}