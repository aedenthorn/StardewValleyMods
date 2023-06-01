using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace AnimalDialogueFramework
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {
        
        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static Dictionary<Character, bool> genericPortraitList = new();
        public static Dictionary<Character, bool> genericDialogueList = new();

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
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            genericPortraitList.Clear();
            genericDialogueList.Clear();
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
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Child Enabled",
                getValue: () => Config.ChildEnabled,
                setValue: value => Config.ChildEnabled = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Pet Enabled",
                getValue: () => Config.PetEnabled,
                setValue: value => Config.PetEnabled = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Monster Enabled",
                getValue: () => Config.MonsterEnabled,
                setValue: value => Config.MonsterEnabled = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Junimo Enabled",
                getValue: () => Config.JunimoEnabled,
                setValue: value => Config.JunimoEnabled = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Horse Enabled",
                getValue: () => Config.HorseEnabled,
                setValue: value => Config.HorseEnabled = value
            );
        }
    }
}