using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System.Globalization;
using System.Reflection;

namespace ToolSmartSwitch
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

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Input_ButtonPressed(object? sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if(Context.CanPlayerMove && Config.ToggleButton != SButton.None && e.Button == Config.ToggleButton)
            {
                Config.SwitchEnabled = !Config.SwitchEnabled;
                Helper.WriteConfig(Config);
                Game1.addHUDMessage(new HUDMessage(SHelper.Translation.Get("enabled-" + Config.SwitchEnabled), 1));
                SMonitor.Log("Mod enabled: " + Config.SwitchEnabled);
            }
        }

        public override object GetApi()
        {
            return new ToolSmartSwitchAPI();
        }


        private void GameLoop_GameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
            PropertyInfo[] props = [.. typeof(ModConfig).GetProperties()];

            foreach (PropertyInfo p in props)
            {
                if (p.Name == nameof(Config.ModEnabled) || p.Name == nameof(Config.Debug))
                    continue;
                if (p.PropertyType == typeof(bool))
                {
                    configMenu.AddBoolOption(
                        mod: ModManifest,
                        name: () => SHelper.Translation.Get(p.Name),
                        getValue: () => (bool)p.GetValue(Config),
                        setValue: value => p.SetValue(Config, value)
                    );
                }
                else if (p.PropertyType == typeof(int))
                {
                    configMenu.AddNumberOption(
                        mod: ModManifest,
                        name: () => SHelper.Translation.Get(p.Name),
                        getValue: () => (int)p.GetValue(Config),
                        setValue: value => p.SetValue(Config, value)
                    );
                }
                else if (p.PropertyType == typeof(float))
                {
                    configMenu.AddTextOption(
                        mod: ModManifest,
                        name: () => SHelper.Translation.Get(p.Name),
                        getValue: () => p.GetValue(Config).ToString(),
                        setValue: value => { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)) { p.SetValue(Config, f); } }
                    );
                }
                else if (p.PropertyType == typeof(double))
                {
                    configMenu.AddTextOption(
                        mod: ModManifest,
                        name: () => SHelper.Translation.Get(p.Name),
                        getValue: () => p.GetValue(Config).ToString(),
                        setValue: value => { if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) { p.SetValue(Config, d); } }
                    );
                }
                else if (p.PropertyType == typeof(string))
                {
                    configMenu.AddTextOption(
                        mod: ModManifest,
                        name: () => SHelper.Translation.Get(p.Name),
                        getValue: () => (string)p.GetValue(Config),
                        setValue: value => p.SetValue(Config, value)
                    );
                }
                else if (p.PropertyType == typeof(SButton))
                {
                    configMenu.AddKeybind(
                        mod: ModManifest,
                        name: () => SHelper.Translation.Get(p.Name),
                        getValue: () => (SButton)p.GetValue(Config),
                        setValue: value => p.SetValue(Config, value)
                    );
                }
            }
        }

    }
}
