using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace UtilityGrid
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static Texture2D pipeTexture;

        public static readonly string dictPath = "utility_grid_object_dictionary";
        public enum GridType {
            water,
            electric
        }  
        public static readonly string saveKey = "utility-grid-data";

        public static bool ShowingGrid { get; set; } = false;
        public static GridType CurrentGrid { get; set; } = GridType.water;
        public static int CurrentTile { get; set; } = 0;
        public static int CurrentRotation { get; set; } = 0;

        public static int[][] intakeArray = { 
            new int[] { 1, 0, 0, 0 }, 
            new int[] { 0, 1, 0, 1 },
            new int[] { 1, 0, 0, 1 },
            new int[] { 0, 1, 1, 1 },
            new int[] { 1, 1, 1, 1 }
        };

        public static Dictionary<Vector2, GridPipe> waterPipes = new Dictionary<Vector2, GridPipe>();
        public static Dictionary<Vector2, GridPipe> electricPipes = new Dictionary<Vector2, GridPipe>();
        public static Dictionary<string, UtilityObject> objectDict = new Dictionary<string, UtilityObject>();
        
        public static List<PipeGroup> waterGroups = new List<PipeGroup>();
        public static List<PipeGroup> electricGroups = new List<PipeGroup>();

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

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.Saving += GameLoop_Saving;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.playerCanPlaceItemHere)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Utility_playerCanPlaceItemHere_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.IsSprinkler)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_IsSprinkler_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.updateWhenCurrentLocation)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_updateWhenCurrentLocation_Prefix))
            );

            pipeTexture = Helper.Content.Load<Texture2D>("assets/pipes.png");

        }


        public override object GetApi()
        {
            return new UtilityGridApi();
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            return asset.AssetNameEquals(dictPath);
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Monitor.Log("Loading dictionary");

            return (T)(object)new Dictionary<string, UtilityObject>();
        }
    }
}