using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using xTile.Dimensions;
using xTile.Tiles;

namespace LikeADuckToWater
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        
        public static string swamTodayKey = "aedenthorn.LikeADuckToWater/swamToday";
        public static string checkedIndexKey = "aedenthorn.LikeADuckToWater/swamToday";

        private static List<Vector2> pickedTiles = new();

        private static List<int> waterBuildingTiles = new()
        {
            209,
            628,
            629,
            734,
            759,
            1293,
            1318
        };
        
        private static Dictionary<Vector2, List<HopInfo>> hopTileDict;

        public static Dictionary<FarmAnimal, Stack<HopInfo>> ducksToCheck = new Dictionary<FarmAnimal, Stack<HopInfo>>();

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
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !ducksToCheck.Any())
            {
                pickedTiles.Clear();
                return;
            }
            if (FarmAnimal.NumPathfindingThisTick >= FarmAnimal.MaxPathfindingPerTick || Game1.random.NextDouble() > Config.ChancePerTick)
                return;
            foreach(var key in ducksToCheck.Keys.ToArray())
            {
                if (key.modData.ContainsKey(swamTodayKey) || ducksToCheck[key].Count == 0)
                {
                    ducksToCheck.Remove(key);
                    continue;
                }
                if(CheckDuck(key, ducksToCheck[key].Pop()))
                {
                    ducksToCheck.Remove(key);
                }
                break;
            }
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            RebuildHopSpots(Game1.getFarm());
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                name: () => "Swim After Auto Pet",
                getValue: () => Config.SwimAfterAutoPet,
                setValue: value => Config.SwimAfterAutoPet = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Max Distance",
                tooltip: () => "Max distance in tiles to allow adding to move queue",
                getValue: () => "" + Config.MaxDistance,
                setValue: delegate (string value) { try { Config.MaxDistance = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Chance / Tick",
                tooltip: () => "Chance for a queued duck to begin moving each game tick",
                getValue: () => "" + Config.ChancePerTick,
                setValue: delegate (string value) { try { Config.ChancePerTick = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Friendship Gain",
                getValue: () => Config.FriendshipGain,
                setValue: value => Config.FriendshipGain = value
            );
        }

    }
}