using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace HarvestSeeds
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
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Regrowable Seeds?",
                getValue: () => Config.RegrowableSeeds,
                setValue: value => Config.RegrowableSeeds = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Seed % Chance",
                getValue: () => Config.SeedChance,
                setValue: value => Config.SeedChance = value,
                min: 0,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Seeds",
                getValue: () => Config.MinSeeds,
                setValue: value => Config.MinSeeds = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Seeds",
                getValue: () => Config.MaxSeeds,
                setValue: value => Config.MaxSeeds = value
            );
        }
    }
}