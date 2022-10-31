using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace SDIEmily
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string dictPath = "aedenthorn.StardewImpact/characters";
        public static string slotPrefix = "aedenthorn.StardewImpact/slot";
        public static string currentSlotKey = "aedenthorn.StardewImpact/currentSlot";
        
        
        public static string skillIconPath = "aedenthorn.SDIEmily/skillIcon";
        public static string burstIconPath = "aedenthorn.SDIEmily/burstIcon";
        

        public static Texture2D frameTexture;
        public static Texture2D backTexture;
        public static Texture2D skillIcon;
        public static Texture2D burstIcon;
        
        public static IStardewImpactApi sdiAPI;


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

            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(skillIconPath))
            {
                e.LoadFromModFile<Texture2D>("assets/skill.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(burstIconPath))
            {
                e.LoadFromModFile<Texture2D>("assets/burst.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (sdiAPI is not null)
            {
                sdiAPI.AddCharacter("Emily", Color.Green, 0, 20, 10, 80, 5, 10, null, null, skillIconPath, burstIconPath, new List<Action<string, Farmer>>() { SkillEvent }, new List<Action<string, Farmer>>() { BurstEvent });
            }
        }


        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            sdiAPI = Helper.ModRegistry.GetApi<IStardewImpactApi>("aedenthorn.StardewImpact");


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