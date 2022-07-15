using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Globalization;
using System.IO;

namespace PrismaticFire
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static string modKey = "aedenthorn.PrismaticFire";
        public static Texture2D cursors;

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
            cursors = new Texture2D(Game1.graphics.GraphicsDevice, Game1.mouseCursors.Width, Game1.mouseCursors.Height);
            Color[] data = new Color[cursors.Width * cursors.Height];
            Game1.mouseCursors.GetData(data);
            for(int i = 0; i < data.Length; i++)
            {
                //SMonitor.Log($"{data[i]}");
                byte c = Convert.ToByte((data[i].R + data[i].G + data[i].B) / 3f);
                data[i] = new Color(c, c, c, data[i].A);
            }
            cursors.SetData(data);
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
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Prismatic Speed",
                getValue: () => Config.PrismaticSpeed.ToString(),
                setValue: delegate(string value) { try { Config.PrismaticSpeed = float.Parse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture); } catch { } } 
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Drop In Sound",
                getValue: () => Config.TriggerSound,
                setValue: value => Config.TriggerSound = value
            );
        }
    }
}