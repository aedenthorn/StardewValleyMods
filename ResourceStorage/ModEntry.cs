using HarmonyLib;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;

namespace ResourceStorage
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static string dictKey = "aedenthorn.ResourceStorage/dictionary";
        public static Dictionary<long, Dictionary<string, long>> resourceDict = new();

        public static GameMenu gameMenu;
        public static ClickableTextureComponent resourceButton;


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.Saving += GameLoop_Saving;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }


        public void GameLoop_Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            foreach (var f in Game1.getAllFarmers())
            {
                if (resourceDict.TryGetValue(f.UniqueMultiplayerID, out var dict))
                {
                    f.modData[dictKey] = JsonConvert.SerializeObject(dict);
                }
            }
        }

        public void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            resourceDict.Clear();
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                    name: () => SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_AutoUse_Name"),
                    getValue: () => Config.AutoUse,
                    setValue: value => Config.AutoUse = value
                );
                
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ResourcesKey_Name"),
                    getValue: () => Config.ResourcesKey,
                    setValue: value => Config.ResourcesKey = value
                );
                
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ModKeyMax_Name"),
                    getValue: () => Config.ModKeyMax,
                    setValue: value => Config.ModKeyMax = value
                );
                
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_IconOffsetX_Name"),
                    getValue: () => Config.IconOffsetX,
                    setValue: value => Config.IconOffsetX = value
                );
                
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_IconOffsetY_Name"),
                    getValue: () => Config.IconOffsetY,
                    setValue: value => Config.IconOffsetY = value
                );

            }
        }
    }
}