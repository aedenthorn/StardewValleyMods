using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace OutfitSets
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string keyPrefix = "aedenthorn.OutfitSets/";
        public static int IDOffset = -3939300;

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

            helper.ConsoleCommands.Add("outfit", "Manually set player outfit. Usage: outfit <number>", SetOutfit);


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void SetOutfit(string arg1, string[] arg2)
        {
            if(arg2.Length == 1 && int.TryParse(arg2[0], out int which))
            {
                SwitchSet(which);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {

            if (!Game1.player.modData.TryGetValue(keyPrefix + "currentSet", out string data) || !int.TryParse(data, out int oldSet))
            {
                Game1.player.modData[keyPrefix + "currentSet"] = "1";
                Monitor.Log("Setting initial set to 1 for player");
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
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Number of Sets",
                getValue: () => Config.Sets,
                setValue: value => Config.Sets = value,
                min: 1,
                max: 12
            );
        }

    }
}