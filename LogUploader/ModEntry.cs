using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.IO;

namespace LogUploader
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        
        public static bool sending;

        public static bool sentOnError;
        private Texture2D buttonTexture;

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
            helper.Events.Display.Rendered += Display_Rendered;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method("StardewModdingAPI.Framework.SCore:OnGameUpdating"),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.OnGameUpdating_Postfix))
            );
            
            harmony.Patch(
                original: AccessTools.Method("StardewModdingAPI.Framework.Logging.LogManager:LogFatalLaunchError"),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.LogFatalLaunchError_Postfix))
            );
            
            AppDomain currentDomain = default(AppDomain);
            currentDomain = AppDomain.CurrentDomain;
            // Handler for unhandled exceptions.
            currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;

            buttonTexture = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "button.png"));
        }

        private void Display_Rendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            if(Config.ModEnabled && Config.ShowButton && SHelper.Input.IsDown(Config.ShowButtonButton))
            {
                string log = Helper.Translation.Get("log");
                e.SpriteBatch.Draw(buttonTexture, Vector2.Zero, Color.White);
                var m = new ClickableMenu();
                m.drawMouse(e.SpriteBatch);
            }
        }

        private void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if(Config.ModEnabled && Config.SendOnFatalError && !sentOnError)
            {
                sentOnError = true;
                sending = true;
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == Config.SendButton)
            {
                sending = true;
            }
            else if (e.Button == SButton.MouseLeft && Config.ShowButton && Helper.Input.IsDown(Config.ShowButtonButton))
            {
                string log = Helper.Translation.Get("log");
                var size = Game1.dialogueFont.MeasureString(log);
                Rectangle r = new Rectangle(0, 0, buttonTexture.Width, buttonTexture.Height);
                if (r.Contains(Game1.getMousePosition()))
                {
                    sending = true;
                    Helper.Input.Suppress(e.Button);
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
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Send On Fatal Error",
                tooltip: () => "Try to send a log if the game crashes",
                getValue: () => Config.SendOnFatalError,
                setValue: value => Config.SendOnFatalError = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Open Log Behind",
                getValue: () => Config.OpenBehind,
                setValue: value => Config.OpenBehind = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Enable Button",
                tooltip: () => "Enable a hidden onscreen button you can show by holding down a key and click on to send a log",
                getValue: () => Config.ShowButton,
                setValue: value => Config.ShowButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Send Key",
                tooltip: () => "Press this key to send a log",
                getValue: () => Config.SendButton,
                setValue: value => Config.SendButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Show Button Key",
                tooltip: () => "hold this key to show the button",
                getValue: () => Config.ShowButtonButton,
                setValue: value => Config.ShowButtonButton = value
            );
        }

    }
}