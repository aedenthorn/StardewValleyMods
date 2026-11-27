using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace FruitTreeTweaks
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        private static int attempts = 0;


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            I18n.Init(helper.Translation);

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

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
                save: () => onSave()
            );

            configMenu.AddSectionTitle( // Generic Options
                mod: ModManifest,
                text: () => I18n.SectionTitle_Generic()
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.EnableMod(),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => I18n.SectionTitle_Placement()
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.CropsBlock(),
                tooltip: () => I18n.CropsBlock_1(),
                getValue: () => Config.CropsBlock,
                setValue: value => Config.CropsBlock = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.TreesBlock(),
                tooltip: () => I18n.TreesBlock_1(),
                getValue: () => Config.TreesBlock,
                setValue: value => Config.TreesBlock = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.ObjectsBlock(),
                tooltip: () => I18n.ObjectsBlock_1(),
                getValue: () => Config.ObjectsBlock,
                setValue: value => Config.ObjectsBlock = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.PlantAnywhere(),
                tooltip: () => I18n.PlantAnywhere_1(),
                getValue: () => Config.PlantAnywhere,
                setValue: value => Config.PlantAnywhere = value
            );

            configMenu.AddSectionTitle( // Growth Options
                mod: ModManifest,
                text: () => I18n.SectionTitle_Growth()
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => I18n.DaysUntilMature(),
                getValue: () => Config.DaysUntilMature,
                setValue: value => Config.DaysUntilMature = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => I18n.MaxFruitTree(),
                getValue: () => Config.MaxFruitPerTree,
                setValue: value => Config.MaxFruitPerTree = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.FruitAllSeasons(),
                tooltip: () => I18n.FruitAllSeasons_1(),
                getValue: () => Config.FruitAllSeasons,
                setValue: value => Config.FruitAllSeasons = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.FruitInWinter(),
                tooltip: () => I18n.FruitInWinter_1(),
                getValue: () => Config.FruitInWinter,
                setValue: value => Config.FruitInWinter = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => I18n.MinFruitDay(),
                tooltip: () => I18n.MinFruitDay_1(),
                getValue: () => Config.MinFruitPerDay,
                setValue: value => Config.MinFruitPerDay = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => I18n.MaxFruitDay(),
                tooltip: () => I18n.MaxFruitDay_1(),
                getValue: () => Config.MaxFruitPerDay,
                setValue: value => Config.MaxFruitPerDay = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => I18n.DaysUntilSilver(),
                tooltip: () => I18n.DaysUntilTip(),
                getValue: () => Config.DaysUntilSilverFruit,
                setValue: value => Config.DaysUntilSilverFruit = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => I18n.DaysUntilGold(),
                tooltip: () => I18n.DaysUntilTip(),
                getValue: () => Config.DaysUntilGoldFruit,
                setValue: value => Config.DaysUntilGoldFruit = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => I18n.DaysUntilIridium(),
                tooltip: () => I18n.DaysUntilTip(),
                getValue: () => Config.DaysUntilIridiumFruit,
                setValue: value => Config.DaysUntilIridiumFruit = value
            );

            configMenu.SetTitleScreenOnlyForNextOptions( // Aesthetic Options
                mod: ModManifest,
                titleScreenOnly: true
                );

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => I18n.SectionTitle_Aesthetic(),
                tooltip: () => I18n.SectionTitle_Aesthetic_1()
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => I18n.ColorVar(),
                tooltip: () => I18n.ColorVar_1(),
                getValue: () => Config.ColorVariation,
                setValue: value => Config.ColorVariation = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => I18n.SizeVar(),
                tooltip: () => I18n.SizeVar_1(),
                getValue: () => Config.SizeVariation,
                setValue: value => Config.SizeVariation = value,
                min: 0,
                max: 99
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => I18n.BufferX(),
                tooltip: () => I18n.BufferX_1(),
                getValue: () => Config.FruitSpawnBufferX,
                setValue: value => Config.FruitSpawnBufferX = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => I18n.BufferY(),
                tooltip: () => I18n.BufferY_1(),
                getValue: () => Config.FruitSpawnBufferY,
                setValue: value => Config.FruitSpawnBufferY = value
            );
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            fruitToday = GetFruitPerDay(); // this breaks if it is anywhere else so dont move it
        }

        private void onSave()
        {
            Helper.WriteConfig(Config);
        }
    }
}
