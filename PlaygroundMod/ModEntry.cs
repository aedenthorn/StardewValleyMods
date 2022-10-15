using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlaygroundMod
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static string swingKey = "aedenthorn.PlaygroundMod/swing";
        public static string springKey = "aedenthorn.PlaygroundMod/spring";
        public static string slideKey = "aedenthorn.PlaygroundMod/slide";
        public static string climbKey = "aedenthorn.PlaygroundMod/climb";
        private static Texture2D slideTexture;
        private static Texture2D swingTexture;
        private static Texture2D springTexture;
        private static Dictionary<long, int> swingTicks = new();
        private static Dictionary<long, int> springTicks = new();
        private static Dictionary<long, int> climbTicks = new();
        private static Dictionary<long, int> slideTicks = new();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;
            context = this;


            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(swingKey))
            {
                e.LoadFromModFile<Texture2D>(Path.Combine("assets", "swing.png"), AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(slideKey))
            {
                e.LoadFromModFile<Texture2D>(Path.Combine("assets", "slide.png"), AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(springKey))
            {
                e.LoadFromModFile<Texture2D>(Path.Combine("assets", "spring.png"), AssetLoadPriority.Low);
            }
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            Helper.GameContent.InvalidateCache(swingKey);
            swingTexture = Helper.GameContent.Load<Texture2D>(swingKey);
            Helper.GameContent.InvalidateCache(springKey);
            springTexture = Helper.GameContent.Load<Texture2D>(springKey);
            Helper.GameContent.InvalidateCache(slideKey);
            slideTexture = Helper.GameContent.Load<Texture2D>(slideKey);
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
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
        }
    }
}