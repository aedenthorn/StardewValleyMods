using HarmonyLib;
using StardewModdingAPI;

namespace LadderSpawnOnEntry
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

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(e.Button == Config.ToggleKey)
            {
                Config.EnableMod = !Config.EnableMod;
                Monitor.Log($"Mod enabled: {Config.EnableMod}");
                Helper.WriteConfig(Config);
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
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
                    name: () => SHelper.Translation.Get("Config.EnableMod"),
                    getValue: () => Config.EnableMod,
                    setValue: value => Config.EnableMod = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("Config.EnableForDangerous"),
                    getValue: () => Config.EnableForDangerous,
                    setValue: value => Config.EnableForDangerous = value
                );

                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("Config.ToggleKey"),
                    getValue: () => Config.ToggleKey,
                    setValue: value => Config.ToggleKey = value
                );
            }
        }
    }
}
