using HarmonyLib;
using StardewModdingAPI;

namespace LivestockChoices
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

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

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

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

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Blue Chicken Price",
                getValue: () => Config.BlueChickenPrice,
                setValue: value => Config.BlueChickenPrice = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Void Chicken Price",
                getValue: () => Config.VoidChickenPrice,
                setValue: value => Config.VoidChickenPrice = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Golden Chicken Price",
                getValue: () => Config.GoldenChickenPrice,
                setValue: value => Config.GoldenChickenPrice = value
            );
        }
    }
}