using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace GiftRejection
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static IList<Item> playerItems;
        public static IList<Item> otherItems;

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
                name: () => "Reject Disliked",
                getValue: () => Config.RejectDisliked,
                setValue: value => Config.RejectDisliked = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Reject Hated",
                getValue: () => Config.RejectHated,
                setValue: value => Config.RejectHated = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Disliked Throw Distance",
                getValue: () => Config.DislikedThrowDistance+"",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float vf)){ Config.DislikedThrowDistance = vf; }  }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Hated Throw Distance",
                getValue: () => Config.HatedThrowDistance+"",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float vf)){ Config.HatedThrowDistance = vf; }  }
            );
        }
    }
}