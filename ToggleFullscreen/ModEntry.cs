using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace ToggleFullScreen
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
            Helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;

        }

        private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
        {
            if (Config.EnableMod && Config.ToggleButtons.JustPressed())
            {
                if (!Game1.options.isCurrentlyWindowed())
                {
                    Game1.options.setWindowedOption("Windowed");
                    if (Config.LastWindowedWidth > 0)
                    {
                        Game1.graphics.PreferredBackBufferWidth = Config.LastWindowedWidth;
                        Game1.graphics.PreferredBackBufferHeight = Config.LastWindowedHeight;
                        Game1.graphics.ApplyChanges();
                    }
                }
                else
                {
                    Config.LastWindowedWidth = Program.gamePtr.Window.ClientBounds.Width;
                    Config.LastWindowedHeight = Program.gamePtr.Window.ClientBounds.Height;
                    Game1.options.setWindowedOption("Windowed Borderless");
                }
                foreach(var k in Config.ToggleButtons.Keybinds)
                {
                    foreach(var b in k.Buttons)
                    {
                        Helper.Input.Suppress(b);
                    }
                }
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
                name: () => "Mod Enabled",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => "TOggle Keys",
                getValue: () => Config.ToggleButtons,
                setValue: value => Config.ToggleButtons = value
            );
        }
    }
}
