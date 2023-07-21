using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLightningRods
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
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;


            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.performLightningUpdate)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(Utility_performLightningUpdate_Transpiler))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Farm), "doLightningStrike"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farm_doLightningStrike_Prefix))
            );
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
                name: () => Helper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("GMCM_Option_UniqueCheck_Name"),
                tooltip: () => Helper.Translation.Get("GMCM_Option_UniqueCheck_Tooltip"),
                getValue: () => Config.UniqueCheck,
                setValue: value => Config.UniqueCheck = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("GMCM_Option_RodsToCheck_Name"),
                tooltip: () => Helper.Translation.Get("GMCM_Option_RodsToCheck_Tooltip"),
                getValue: () => Config.RodsToCheck,
                setValue: value => Config.RodsToCheck = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("GMCM_Option_LightningChance_Name"),
                getValue: () => (int)Config.LightningChance,
                setValue: value => Config.LightningChance = value,
                min: 0,
                max: 100
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_Astraphobia_Name"),
                tooltip: () => ModEntry.SHelper.Translation.Get("GMCM_Option_Astraphobia_Tooltip"),
                getValue: () => Config.Astraphobia,
                setValue: value => Config.Astraphobia = value
            );
        }
    }

}