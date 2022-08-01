using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Globalization;

namespace MusicalPaths
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string typeKey = "aedenthorn.MusicalPaths/type";
        public static string lastTimeKey = "aedenthorn.MusicalPaths/lastTime";
        public static string whichKey = "aedenthorn.MusicalPaths/pitch";

        public static bool checking = false;

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
                name: () => "Consume Block",
                getValue: () => Config.ConsumeBlock,
                setValue: value => Config.ConsumeBlock = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Mod Key",
                getValue: () => Config.ModKey,
                setValue: value => Config.ModKey = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Block Outline",
                getValue: () => Config.ShowBlockOutLine,
                setValue: value => Config.ShowBlockOutLine = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Block Outline Opacity",
                getValue: () => Config.BlockOutLineOpacity +"",
                setValue: delegate(string value) { try { Config.BlockOutLineOpacity = float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture); } catch { } } 
            );
        }

    }
}