using HarmonyLib;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using System;
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
        private Harmony harmony;


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

            harmony = new Harmony(ModManifest.UniqueID);
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
            var bcapi = Helper.ModRegistry.GetApi("leclair.bettercrafting");
            if (bcapi is not null)
            {
                var type = bcapi.GetType().Assembly.GetType("Leclair.Stardew.Common.InventoryHelper");
                if (type is not null)
                {
                    try
                    {
                        foreach(var m in type.GetMethods())
                        {
                            if(m.Name == "CountItem" && m.GetParameters().Length > 1 && m.GetParameters()[1].ParameterType == typeof(Farmer))
                            {
                                harmony.Patch(
                                    original: m,
                                    postfix: new HarmonyMethod(typeof(ModEntry), nameof(Leclair_Stardew_Common_InventoryHelper_CountItem_Postfix))
                                );
                            }
                            else if (m.Name == "ConsumeItem")
                            {
                                harmony.Patch(
                                    original: m,
                                    prefix: new HarmonyMethod(typeof(ModEntry), nameof(Leclair_Stardew_Common_InventoryHelper_ConsumeItem_Prefix))
                                );
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Monitor.Log($"Error: {ex}", LogLevel.Error);
                    }
                }
            }
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

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ShowMessage_Name"),
                    getValue: () => Config.ShowMessage,
                    setValue: value => Config.ShowMessage = value
                );
                
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ResourcesKey_Name"),
                    getValue: () => Config.ResourcesKey,
                    setValue: value => Config.ResourcesKey = value
                );
                
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ModKey1_Name"),
                    getValue: () => Config.ModKey1,
                    setValue: value => Config.ModKey1 = value
                );
                
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ModKey1Amount_Name"),
                    getValue: () => Config.ModKey1Amount,
                    setValue: value => Config.ModKey1Amount = value
                );
                
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ModKey2_Name"),
                    getValue: () => Config.ModKey2,
                    setValue: value => Config.ModKey2 = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ModKey2Amount_Name"),
                    getValue: () => Config.ModKey2Amount,
                    setValue: value => Config.ModKey2Amount = value
                );


                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ModKey3_Name"),
                    getValue: () => Config.ModKey3,
                    setValue: value => Config.ModKey3 = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ModKey3Amount_Name"),
                    getValue: () => Config.ModKey3Amount,
                    setValue: value => Config.ModKey3Amount = value
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