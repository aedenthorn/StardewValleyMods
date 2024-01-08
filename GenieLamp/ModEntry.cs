using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace GenieLamp
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string modKey = "aedenthorn.GenieLamp";


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Game1.player.addItemToInventory(new Object("124", 1));
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
                
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_LampItem_Name"),
                    getValue: () => Config.LampItem,
                    setValue: value => Config.LampItem = value
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_WishesPerItem_Name"),
                    getValue: () => Config.WishesPerItem,
                    setValue: value => Config.WishesPerItem = value
                );

                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_MenuSound_Name"),
                    getValue: () => Config.MenuSound,
                    setValue: value => Config.MenuSound = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_WishSound_Name"),
                    getValue: () => Config.WishSound,
                    setValue: value => Config.WishSound = value
                );

            }
        }
    }
}