using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace FieldHarvest
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

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;


            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.performUseAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.HoeDirt_performUseAction_Prefix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(Crop), nameof(Crop.harvest)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Crop_harvest_Prefix))
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
                name: () => "Mod Enabled",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Diagonals Joined",
                getValue: () => Config.AllowDiagonal,
                setValue: value => Config.AllowDiagonal = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Only Same Crop",
                getValue: () => Config.OnlySameSeed,
                setValue: value => Config.OnlySameSeed = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Auto Collect",
                getValue: () => Config.AutoCollect,
                setValue: value => Config.AutoCollect = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Modifier Key",
                getValue: () => Config.ModButton,
                setValue: value => Config.ModButton = value
            );
        }
    }

}