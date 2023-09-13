using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.IO;

namespace MapTeleport
{
    public partial class ModEntry : Mod
    {
        public static ModEntry context;
        public static ModConfig Config;
        public static IModHelper SHelper;
        public static IMonitor SMonitor;
        private static bool isSVE;
        private Harmony harmony;

        public static string dictPath = "aedenthorn.MapTeleport/coordinates";

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.ModEnabled)
                return;

            context = this;
            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            isSVE = Helper.ModRegistry.IsLoaded("FlashShifter.SVECode");

            harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }


        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                if (File.Exists(Path.Combine(SHelper.DirectoryPath, "coordinates.json")))
                {
                    e.LoadFromModFile<CoordinatesList>("coordinates.json", AssetLoadPriority.Exclusive);
                }
                else
                {
                    CoordinatesList coordinatesList = new CoordinatesList();
                    if (isSVE)
                    {
                        coordinatesList = Helper.Data.ReadJsonFile<CoordinatesList>("assets/sve_coordinates.json");
                    }
                    else
                    {
                        coordinatesList = Helper.Data.ReadJsonFile<CoordinatesList>("assets/coordinates.json");
                    }
                    e.LoadFrom(() => coordinatesList, AssetLoadPriority.Exclusive);
                }
            }
        }


        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {

                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );

            }

        }
    }
}
