using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;

namespace CustomDecorationAreas
{
    public class ModEntry : Mod
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public static ModEntry context;
        public static ModConfig config;
        public static Dictionary<string, FloorWallData> floorsWallsDataDict = new Dictionary<string, FloorWallData>();
        public static List<string> convertedLocations = new List<string>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            config = Helper.ReadConfig<ModConfig>();

            if (!config.EnableMod)
                return;

            PMonitor = Monitor;
            PHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getWalls)),
               postfix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.getWalls_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.getWalls)),
               postfix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.getWalls_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Shed), nameof(Shed.getWalls)),
               postfix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.getWalls_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.getFloors)),
               postfix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.getFloors_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Shed), nameof(Shed.getFloors)),
               postfix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.getFloors_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(DecoratableLocation), nameof(DecoratableLocation.getFloors)),
               postfix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.getFloors_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.loadForNewGame)),
               postfix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.loadForNewGame_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(DecoratableLocation), "doSetVisibleFloor"),
               prefix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.doSetVisibleFloor_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(DecoratableLocation), "doSetVisibleWallpaper"),
               prefix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.doSetVisibleWallpaper_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(DecoratableLocation), "IsFloorableOrWallpaperableTile"),
               prefix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.IsFloorableOrWallpaperableTile_Prefix))
            );
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {

                FloorWallDataDict floorWallDataDict = contentPack.ReadJsonFile<FloorWallDataDict>("content.json") ?? new FloorWallDataDict();
                foreach (FloorWallData data in floorWallDataDict.data)
                {
                    try
                    {
                        Monitor.Log($"Adding custom walls and floors for {data.name}");
                        floorsWallsDataDict.Add(data.name, data);
                    }
                    catch(Exception ex)
                    {
                        Monitor.Log($"Exception getting data for {data.name} in content pack {contentPack.Manifest.Name}:\n{ex}", LogLevel.Error);
                    }
                }
            }
            Monitor.Log($"Loaded floors and walls for {floorsWallsDataDict.Count} locations.");
        }
    }
}