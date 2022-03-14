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
        public static bool ShowingEdit { get; set; } = true;
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

        public static Dictionary<string, Dictionary<GridType, UtilitySystem>> utilitySystemDict = new Dictionary<string, Dictionary<GridType, UtilitySystem>>();
        public static Dictionary<string, UtilityObject> utilityObjectDict = new Dictionary<string, UtilityObject>();
        public static List<Func<string, int, List<Vector2>, Vector2>> powerFuctionList = new List<Func<string, int, List<Vector2>, Vector2>>();
        public static EventHandler<KeyValuePair<GameLocation, int>> refreshEventHandler;
        public static EventHandler<KeyValuePair<GameLocation, int>> showEventHandler;
        public static EventHandler<KeyValuePair<GameLocation, int>> hideEventHandler;
        
        public static Harmony harmony;

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
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;

            harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.playerCanPlaceItemHere)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Utility_playerCanPlaceItemHere_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.minutesElapsed)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_minutesElapsed_Prefix)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_Method_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.DayUpdate)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_DayUpdate_Prefix)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_Method_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.getScale)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_getScale_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_placementAction_Postfix))
            );
             harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.performRemoveAction)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_performRemoveAction_Postfix))
            );

            /*
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.numberOfObjectsWithName)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_numberOfObjectsWithName_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.dayUpdate)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.FarmAnimal_dayUpdate_Prefix)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.FarmAnimal_dayUpdate_Postfix))
            );
            */
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