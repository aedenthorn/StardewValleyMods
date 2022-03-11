using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.IO;

namespace Screenshot
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

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(Config.EnableMod && e.Button == Config.ScreenshotKey)
            {
                Monitor.Log("Saving screenshot");

                var screenshot_name = string.Concat(new string[]
                {
                    SaveGame.FilterFileName(Game1.player.Name),
                    "_",
                    DateTime.UtcNow.Month.ToString(),
                    "-",
                    DateTime.UtcNow.Day.ToString(),
                    "-",
                    DateTime.UtcNow.Year.ToString(),
                    "_",
                    ((int)DateTime.UtcNow.TimeOfDay.TotalMilliseconds).ToString(),
                    ".png"
                });

                if (!Directory.Exists(Config.ScreenshotFolder))
                    Directory.CreateDirectory(Config.ScreenshotFolder);

                var width = Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth;
                var height = Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight;

                Color[] data = new Color[width * height];
                Game1.graphics.GraphicsDevice.GetBackBufferData(data);
                for(int i = 0; i < data.Length; i++)
                {
                    data[i].A = 255;
                }
                Texture2D tex = new Texture2D(Game1.graphics.GraphicsDevice, width, height);
                tex.SetData(data);
                Stream stream = File.Create(Path.Combine(Config.ScreenshotFolder, screenshot_name));
                tex.SaveAsPng(stream, width, height);
                stream.Close();

                if(Config.Message.Length > 0)
                {
                    Game1.addHUDMessage(new HUDMessage(string.Format(Config.Message, Path.Combine(Environment.CurrentDirectory, Config.ScreenshotFolder, screenshot_name))));
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
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Screenshot Key",
                getValue: () => Config.ScreenshotKey,
                setValue: value => Config.ScreenshotKey = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Screenshot Folder",
                getValue: () => Config.ScreenshotFolder,
                setValue: value => Config.ScreenshotFolder = value
            );

        }

    }

}