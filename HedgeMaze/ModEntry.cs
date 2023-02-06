using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using xTile;

namespace HedgeMaze
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        private static IAdvancedLootFrameworkApi advancedLootFrameworkApi;
        private static List<object> treasuresList;
        private static List<Point> openTiles;
        private static List<Point> endTiles;
        private static List<Point> vertTiles;
        private static List<Vector2> fairyTiles = new();
        private static Color[] tintColors = new Color[]
        {
            Color.DarkGray,
            Color.Brown,
            Color.Silver,
            Color.Gold,
            Color.Purple,
        };

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;
            context = this;


            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if(Config.ModEnabled && Context.IsWorldReady && Config.Debug && e.Button == SButton.F5)
            {
                Helper.GameContent.InvalidateCache("Maps/Woods");
                Game1.getLocationFromName("Woods").reloadMap();
                PopulateMap();
            }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            PopulateMap();
        }
        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {

        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            if (!Config.ModEnabled)
                return; 
            Helper.GameContent.InvalidateCache("Maps/Woods");

            var woods = Game1.getLocationFromName("Woods");
            for (int i = woods.characters.Count - 1; i >= 0; i--)
            {
                if (woods.characters[i] is Monster || woods.characters[i].Name.Equals("Dwarf"))
                {
                    woods.characters.RemoveAt(i);
                }
            }
            woods.objects.Clear();
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            Helper.GameContent.InvalidateCache("Maps/Woods");
        }


        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/Woods"))
            {
                e.LoadFromModFile<Map>(Path.Combine("assets", "WoodsMaze.tmx"), AssetLoadPriority.Exclusive);
                e.Edit(ModifyMap, AssetEditPriority.Early);
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            advancedLootFrameworkApi = context.Helper.ModRegistry.GetApi<IAdvancedLootFrameworkApi>("aedenthorn.AdvancedLootFramework");
            if (advancedLootFrameworkApi != null)
            {
                Monitor.Log($"loaded AdvancedLootFramework API", LogLevel.Debug);
                treasuresList = advancedLootFrameworkApi.LoadPossibleTreasures(Config.ItemListChances.Where(p => p.Value > 0).ToDictionary(s => s.Key, s => s.Value).Keys.ToArray(), Config.MinItemValue, Config.MaxItemValue);
                Monitor.Log($"Got {treasuresList.Count} possible treasures");
            }

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