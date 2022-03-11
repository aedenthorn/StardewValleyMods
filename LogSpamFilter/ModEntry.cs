using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Object = StardewValley.Object;

namespace LogSpamFilter
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        private Harmony harmony;
        public static int throttled = 0;

        private static Dictionary<string, ModMessageData> messageData = new Dictionary<string, ModMessageData>();
        private static List<string> allowList = new List<string>();
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;


            harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method( typeof(StardewModdingAPI.Mod).Assembly.GetType("StardewModdingAPI.Framework.Monitor"), "LogImpl"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.LogImpl_Prefix))
            );

            allowList = Config.AllowList.Split(',').ToList();

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

            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => $"Total Messages Throttled: {throttled}"
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Debug Messages?",
                getValue: () => Config.IsDebug,
                setValue: value => Config.IsDebug = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Interval",
                tooltip: () => "In milliseconds",
                getValue: () => Config.MSBetweenMessages,
                setValue: value => Config.MSBetweenMessages = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Identical Interval",
                tooltip: () => "In milliseconds",
                getValue: () => Config.MSBetweenIdenticalMessages,
                setValue: value => Config.MSBetweenIdenticalMessages = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Similar Interval",
                tooltip: () => "In milliseconds",
                getValue: () => Config.MSBetweenSimilarMessages,
                setValue: value => Config.MSBetweenSimilarMessages = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Similarity",
                tooltip: () => "In percent",
                getValue: () => Config.PercentSimilarity,
                setValue: value => Config.PercentSimilarity = value,
                min: 0,
                max: 100
            );
        }

    }
}