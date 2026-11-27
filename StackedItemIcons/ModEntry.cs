using HarmonyLib;
using StardewModdingAPI;
using System.Collections.Generic;
using System.IO;

namespace StackedItemIcons
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static string allowPath = "allow_list.json";

        public static ModEntry context;
        public static bool reloadAllowed;
        public static bool writeAllowed;
        public static Dictionary<string, bool> allowList;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked; 
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            if(!File.Exists(Path.Combine(SHelper.DirectoryPath, allowPath)))
            {
                allowList = new();
                SHelper.Data.WriteJsonFile(allowPath, allowList);
            }
            else
            {
                allowList = SHelper.ModContent.Load<Dictionary<string, bool>>(allowPath);
            }
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if (writeAllowed)
            {
                SHelper.Data.WriteJsonFile(allowPath, allowList);
                writeAllowed = false;
            }
            if (reloadAllowed)
            {
                if (!File.Exists(Path.Combine(SHelper.DirectoryPath, allowPath)))
                {
                    allowList = new();
                    SHelper.Data.WriteJsonFile(allowPath, allowList);
                }
                else
                {
                    allowList = SHelper.ModContent.Load<Dictionary<string, bool>>(allowPath) ?? new();
                }
                reloadAllowed = false;
            }
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
                name: delegate() { reloadAllowed = true; return "Mod Enabled"; },
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min For Double",
                getValue: () => Config.MinForDoubleStack,
                setValue: value => Config.MinForDoubleStack = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min For Triple",
                getValue: () => Config.MinForTripleStack,
                setValue: value => Config.MinForTripleStack = value
            );
            
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Spacing",
                getValue: () => Config.Spacing,
                setValue: value => Config.Spacing = value
            );


        }
    }
}
