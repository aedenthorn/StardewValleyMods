using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace GardenPotTweaks
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static ICue buzz;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

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
                name: () => "Honey",
                getValue: () => Config.EnableHoney,
                setValue: value => Config.EnableHoney = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Fix Flower Find",
                getValue: () => Config.FixFlowerFind,
                setValue: value => Config.FixFlowerFind = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Sprinklering",
                getValue: () => Config.EnableSprinklering,
                setValue: value => Config.EnableSprinklering = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Ancient Seeds",
                getValue: () => Config.EnableAncientSeeds,
                setValue: value => Config.EnableAncientSeeds = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Crop Moving",
                getValue: () => Config.EnableMoving,
                setValue: value => Config.EnableMoving = value
            );
        }
    }
}