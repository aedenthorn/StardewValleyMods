using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace OpenFolder
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        private Harmony harmony;
        public static ModConfig Config;

        public static ModEntry context;
        public static SpriteFont dialogueFont;
        public static SpriteFont smallFont;
        public static SpriteFont tinyFont;
        //private Dictionary<string, Mapping> OpenFolderMap;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;
            /*
            if (Config.AddGMCMModsFolderButton)
            {
                var type = configMenu.GetType();
                var t = type.Assembly.GetType("GenericModConfigMenu.Framework.SpecificModConfigMenu");
                harmony.Patch(
                    original: AccessTools.Method(t, "AddDefaultLabels"),
                    postfix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(GMCM_AddDefaultLabels_Postfix)))
                );
                harmony.Patch(
                    original: AccessTools.Method(t, "draw"),
                    prefix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(GMCM_draw_Prefix)))
                );
            }
            */
            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("ModEnabled_GMCM"),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("AddGameFolderButton_GMCM"),
                getValue: () => Config.AddGameFolderButton,
                setValue: value => Config.AddGameFolderButton = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("AddModsFolderButton_GMCM"),
                getValue: () => Config.AddModsFolderButton,
                setValue: value => Config.AddModsFolderButton = value
            );
            /*
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "AddGMCMModsFolderButton_GMCM",
                getValue: () => Config.AddGMCMModsFolderButton,
                setValue: value => Config.AddGMCMModsFolderButton = value
            );
            */
        }

    }
}