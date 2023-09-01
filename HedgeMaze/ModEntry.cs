using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
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
        private static Color[] tintColors = new Color[]
        {
            Color.DarkGray,
            Color.Brown,
            Color.Silver,
            Color.Gold,
            Color.Purple,
        };

        public static string dictPath = "aedenthorn.HedgeMaze/dictionary";
        public static Dictionary<string, List<MazeInstance>> mazeLocationDict = new();
        public static Dictionary<string, MazeData> mazeDataDict = new();

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
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            //var data = new MazeData();
            //File.WriteAllText(Path.Combine(SHelper.DirectoryPath, "test.json"), JsonConvert.SerializeObject(data));
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled || !Game1.IsMasterGame)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, MazeData>(), AssetLoadPriority.Exclusive);
                return;
            }
            if (e.DataType == typeof(Map))
            {
                foreach(var kvp in mazeLocationDict)
                {
                    foreach(var inst in kvp.Value)
                    {
                        if (e.NameWithoutLocale.IsEquivalentTo("Maps/Farm"))
                        {
                            var x = inst.mapPath;
                        }
                        if (e.NameWithoutLocale.IsEquivalentTo(inst.mapPath))
                        {
                            e.Edit(delegate (IAssetData data)
                            {
                                SMonitor.Log($"adding maze to map {e.NameWithoutLocale}");
                                ModifyMap(data.AsMap().Data, inst);
                            });
                        }
                    }
                }
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if(Config.ModEnabled && Context.IsWorldReady && Config.Debug && Game1.IsMasterGame && e.Button == SButton.F5)
            {
                PopulateMazes();
            }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Config.ModEnabled || !Game1.IsMasterGame)
                return;
            SHelper.GameContent.InvalidateCache(dictPath);
            SHelper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked_AfterDayStarted;
        }

        private void GameLoop_UpdateTicked_AfterDayStarted(object sender, UpdateTickedEventArgs e)
        {
            ReloadMazes();
            SHelper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked_AfterDayStarted;
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            DepopulateMaps();
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            Helper.GameContent.InvalidateCache("Maps/Woods");
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            advancedLootFrameworkApi = context.Helper.ModRegistry.GetApi<IAdvancedLootFrameworkApi>("aedenthorn.AdvancedLootFramework");

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