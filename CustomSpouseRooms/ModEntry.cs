using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using xTile;

namespace CustomSpouseRooms
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public static ModConfig config;

        public static Dictionary<string, int> roomIndexes = new Dictionary<string, int>{
            { "Abigail", 0 },
            { "Penny", 1 },
            { "Leah", 2 },
            { "Haley", 3 },
            { "Maru", 4 },
            { "Sebastian", 5 },
            { "Alex", 6 },
            { "Harvey", 7 },
            { "Elliott", 8 },
            { "Sam", 9 },
            { "Shane", 10 },
            { "Emily", 11 },
            { "Krobus", 12 },
        };

        public static Dictionary<string, SpouseRoomData> customRoomData = new Dictionary<string, SpouseRoomData>();
        public static Dictionary<string, SpouseRoomData> currentRoomData = new Dictionary<string, SpouseRoomData>();
        public static Dictionary<string, SpouseRoomData> currentIslandRoomData = new Dictionary<string, SpouseRoomData>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<ModConfig>();

            if (!config.EnableMod)
                return;

            PMonitor = Monitor;
            PHelper = helper;

            LocationPatches.Initialize(Monitor, Helper, config);
            NetWorldStatePatches.Initialize(Monitor, Helper, config);
            Misc.Initialize(Monitor, Helper, config);

            var harmony = new Harmony(ModManifest.UniqueID);

            // Location patches

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), "resetLocalState"),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_resetLocalState_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.checkAction)),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_checkAction_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.loadSpouseRoom)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_loadSpouseRoom_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.updateFarmLayout)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_updateFarmLayout_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(DecoratableLocation), nameof(DecoratableLocation.MakeMapModifications)),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.DecoratableLocation_MakeMapModifications_Postfix))
            );
            /*
            harmony.Patch(
               original: AccessTools.Method(typeof(DecoratableLocation), "IsFloorableOrWallpaperableTile"),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.DecoratableLocation_IsFloorableOrWallpaperableTile_Prefix))
            );
            */

            // NetWorldState patch 

            harmony.Patch(
               original: AccessTools.Method(typeof(NetWorldState), nameof(NetWorldState.hasWorldStateID)),
               prefix: new HarmonyMethod(typeof(NetWorldStatePatches), nameof(NetWorldStatePatches.hasWorldStateID_Prefix))
            );

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                SpouseRoomDataObject obj = contentPack.ReadJsonFile<SpouseRoomDataObject>("content.json");
                foreach (var srd in obj.data)
                {
                    customRoomData.Add(srd.name, srd);
                    Monitor.Log($"Added {srd.name} room data, template {srd.templateName} start pos {srd.startPos}");
                }

                Monitor.Log($"Added {obj.data.Count} room datas from {contentPack.Manifest.Name}");
            }
        }

        public override object GetApi()
        {
            return new CustomSpouseRoomsAPI();
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!config.EnableMod)
                return false;

            if (asset.AssetName.Contains("custom_spouse_room_"))
            {
                Monitor.Log($"can load map {asset.AssetName}");
                return true;
            }

            return false;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            return Helper.Content.Load<T>(asset.AssetName + ".tmx");
        }
    }
}