using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Globalization;

namespace CraftFromContainers
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static List<NetObjectList<Item>> cachedContainers;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Display.MenuChanged += Display_MenuChanged;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            cachedContainers = null;
        }
        public override object GetApi()
        {
            return new CraftFromContainersAPI();
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
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ToggleButton_Name"),
                getValue: () => Config.ToggleButton,
                setValue: value => Config.ToggleButton = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () =>ModEntry.SHelper.Translation.Get("GMCM_Option_EnableForBuilding_Name"),
                getValue: () => Config.EnableForBuilding,
                setValue: value => Config.EnableForBuilding = value
            );
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EnableForCrafting_Name"),
                getValue: () => Config.EnableForCrafting,
                setValue: value => Config.EnableForCrafting = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EnableEverywhere_Name"),
                getValue: () => Config.EnableEverywhere,
                setValue: value => Config.EnableEverywhere = value
            );
        }

    }
}
