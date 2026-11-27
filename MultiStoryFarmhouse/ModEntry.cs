using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Tiles;

namespace MultiStoryFarmhouse
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetEditor, IAssetLoader
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public static ModEntry context;
        public static ModConfig config;
        public static Dictionary<string, Floor> floorsList = new Dictionary<string, Floor>();
        public static Dictionary<int, Map> floorMaps = new Dictionary<int, Map>();

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
            
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            Helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getWalls)),
               prefix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.getWalls_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(DecoratableLocation), nameof(DecoratableLocation.getFloors)),
               prefix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.getFloors_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(SaveGame), nameof(SaveGame.loadDataToLocations)),
               prefix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.loadDataToLocations_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), "resetLocalState"),
               prefix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.resetLocalState_Prefix)),
               postfix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.resetLocalState_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CanPlaceThisFurnitureHere)),
               prefix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.CanPlaceThisFurnitureHere_Prefix))
            );
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
        }

        public static Floor GetFloor(string name)
        {
            int floorNo = int.Parse(name[name.Length - 1].ToString());
            return floorsList[GetPossibleFloors()[floorNo]];
        }

        public static List<string> GetPossibleFloors()
        {
            return config.FloorNames.Where(s => floorsList.ContainsKey(s)).ToList();
        }

        public void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }
        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!floorsList.Any() || Game1.player == null || Utility.getHomeOfFarmer(Game1.player) == null)
                return;

            var warps = Utility.getHomeOfFarmer(Game1.player).warps;
            if (warps.Where(w => w.TargetName == "MultipleFloors0").Any())
            {
                return;
            }
            Monitor.Log("Doesn't have warp");

            Vector2 stairs = floorsList[GetPossibleFloors()[0]].stairsStart;
            int x = (int)stairs.X;
            int y = (int)stairs.Y;

            Warp warp = new Warp(config.MainFloorStairsX + 1, config.MainFloorStairsY + 3, "MultipleFloors0", x + 1, y + 2, true, false);
            Utility.getHomeOfFarmer(Game1.player).warps.Add(warp);

            Warp warp2 = new Warp(config.MainFloorStairsX + 2, config.MainFloorStairsY + 3, "MultipleFloors0", x + 2, y + 2, true, false);
            Utility.getHomeOfFarmer(Game1.player).warps.Add(warp2);
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked -= GameLoop_OneSecondUpdateTicked;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {

                FloorsData floorsData = contentPack.ReadJsonFile<FloorsData>("content.json") ?? new FloorsData();
                foreach (Floor floor in floorsData.floors)
                {
                    try
                    {
                        for(int i = 0; i < config.FloorNames.Count; i++)
                        {
                            if (floor.name == config.FloorNames[i])
                            {
                                Monitor.Log($"Setting floor {i} map to {floor.name}.");
                                floorMaps[i] = contentPack.LoadAsset<Map>(floor.mapPath);
                            }
                        }
                        floorsList.Add(floor.name, floor);
                    }
                    catch(Exception ex)
                    {
                        Monitor.Log($"Exception getting map at {floor.mapPath} for {floor.name} in content pack {contentPack.Manifest.Name}:\n{ex}", LogLevel.Error);
                    }
                }
            }
            Monitor.Log($"Loaded {floorsList.Count} floors.");
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {

        }




        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!config.EnableMod)
                return false;

            if (asset.AssetName.Contains("MultipleFloorsMap"))
            {
                Monitor.Log($"can load floor {asset.AssetName[asset.AssetName.Length - 1]} map");
                return true;
            }

            return false;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetName.Contains("MultipleFloorsMap"))
            {
                int floorNo = int.Parse(asset.AssetName[asset.AssetName.Length - 1].ToString());
                Monitor.Log($"Loading floor {asset.AssetName[asset.AssetName.Length - 1]} map");
                Map map = floorMaps[floorNo];

                AddStairs(ref map, floorNo);
                return (T)(object) map;
            }

            throw new InvalidOperationException($"Unexpected asset '{asset.AssetName}'.");
        }


        private void AddStairs(ref Map map, int floorNo)
        {
            try
            {
                TileSheet indoor = map.TileSheets.First(s => s.Id == "indoor");
                TileSheet untitled = map.TileSheets.First(s => s.Id == "untitled tile sheet");
                Vector2 stairs = floorsList[GetPossibleFloors()[floorNo]].stairsStart;
                int x = (int)stairs.X;
                int y = (int)stairs.Y;
                // left 
                map.GetLayer("Buildings").Tiles[x + 1, y + 1] = null;
                map.GetLayer("Buildings").Tiles[x + 2, y + 1] = null;

                map.GetLayer("Front").Tiles[x + 1, y] = null;
                map.GetLayer("Front").Tiles[x + 2, y] = null;
                map.GetLayer("Front").Tiles[x + 1, y + 1] = null;
                map.GetLayer("Front").Tiles[x + 2, y + 1] = null;
                map.GetLayer("Front").Tiles[x, y] = new StaticTile(map.GetLayer("Buildings"), indoor, BlendMode.Alpha, 162);
                map.GetLayer("Front").Tiles[x + 3, y] = new StaticTile(map.GetLayer("Buildings"), indoor, BlendMode.Alpha, 163);

                map.GetLayer("Buildings").Tiles[x, y + 1] = new StaticTile(map.GetLayer("Buildings"), indoor, BlendMode.Alpha, 64);
                map.GetLayer("Front").Tiles[x, y + 1] = new StaticTile(map.GetLayer("Front"), indoor, BlendMode.Alpha, 64);

                map.GetLayer("Buildings").Tiles[x, y + 2] = new StaticTile(map.GetLayer("Buildings"), indoor, BlendMode.Alpha, 96);

                map.GetLayer("Front").Tiles[x + 1, y + 2] = new StaticTile(map.GetLayer("Front"), indoor, BlendMode.Alpha, 165);
                map.GetLayer("Front").Tiles[x + 2, y + 2] = new StaticTile(map.GetLayer("Front"), indoor, BlendMode.Alpha, 165);

                map.GetLayer("Back").Tiles[x + 1, y + 1] = new StaticTile(map.GetLayer("Back"), indoor, BlendMode.Alpha, 1043);
                map.GetLayer("Back").Tiles[x + 2, y + 1] = new StaticTile(map.GetLayer("Back"), indoor, BlendMode.Alpha, 1043);
                map.GetLayer("Back").Tiles[x + 1, y + 2] = new StaticTile(map.GetLayer("Back"), indoor, BlendMode.Alpha, 1075);
                map.GetLayer("Back").Tiles[x + 2, y + 2] = new StaticTile(map.GetLayer("Back"), indoor, BlendMode.Alpha, 1075);

                map.GetLayer("Back").Tiles[x + 1, y + 1].Properties["NoFurniture"] = "t";
                map.GetLayer("Back").Tiles[x + 2, y + 1].Properties["NoFurniture"] = "t";
                map.GetLayer("Back").Tiles[x + 1, y + 1].Properties["NPCBarrier"] = "t";
                map.GetLayer("Back").Tiles[x + 2, y + 1].Properties["NPCBarrier"] = "t";

                map.GetLayer("Back").Tiles[x + 1, y + 2].Properties["NoFurniture"] = "t";
                map.GetLayer("Back").Tiles[x + 2, y + 2].Properties["NoFurniture"] = "t";

                map.GetLayer("Buildings").Tiles[x + 3, y + 1] = new StaticTile(map.GetLayer("Buildings"), indoor, BlendMode.Alpha, 68);
                map.GetLayer("Front").Tiles[x + 3, y + 1] = new StaticTile(map.GetLayer("Front"), indoor, BlendMode.Alpha, 68);
                map.GetLayer("Buildings").Tiles[x + 3, y + 2] = new StaticTile(map.GetLayer("Buildings"), indoor, BlendMode.Alpha, 130);

                map.GetLayer("Front").Tiles[x + 1, y + 3] = new StaticTile(map.GetLayer("Front"), indoor, BlendMode.Alpha, 0);
                map.GetLayer("Front").Tiles[x + 2, y + 3] = new StaticTile(map.GetLayer("Front"), indoor, BlendMode.Alpha, 0);


                if (floorNo < GetPossibleFloors().Count - 1)
                {
                    Monitor.Log($"adding upstairs for floor {floorNo} / {GetPossibleFloors().Count}");

                    // right 

                    map.GetLayer("Buildings").Tiles[x + 4, y + 1] = null;
                    map.GetLayer("Buildings").Tiles[x + 5, y + 1] = null;

                    map.GetLayer("Front").Tiles[x + 4, y] = null;
                    map.GetLayer("Front").Tiles[x + 5, y] = null;
                    map.GetLayer("Front").Tiles[x + 4, y + 1] = null;
                    map.GetLayer("Front").Tiles[x + 5, y + 1] = null;

                    map.GetLayer("Front").Tiles[x + 3, y] = new StaticTile(map.GetLayer("Buildings"), indoor, BlendMode.Alpha, 100);
                    map.GetLayer("Front").Tiles[x + 6, y] = new StaticTile(map.GetLayer("Buildings"), indoor, BlendMode.Alpha, 163);


                    map.GetLayer("Buildings").Tiles[x + 3, y + 1] = new StaticTile(map.GetLayer("Buildings"), indoor, BlendMode.Alpha, 132);
                    map.GetLayer("Front").Tiles[x + 3, y + 1] = new StaticTile(map.GetLayer("Front"), indoor, BlendMode.Alpha, 132);

                    map.GetLayer("Buildings").Tiles[x + 3, y + 2] = new StaticTile(map.GetLayer("Buildings"), indoor, BlendMode.Alpha, 164);

                    map.GetLayer("Front").Tiles[x + 4, y + 2] = new StaticTile(map.GetLayer("Front"), indoor, BlendMode.Alpha, 165);
                    map.GetLayer("Front").Tiles[x + 5, y + 2] = new StaticTile(map.GetLayer("Front"), indoor, BlendMode.Alpha, 165);


                    map.GetLayer("Back").Tiles[x + 4, y + 1] = new StaticTile(map.GetLayer("Back"), untitled, BlendMode.Alpha, 181);
                    map.GetLayer("Back").Tiles[x + 5, y + 1] = new StaticTile(map.GetLayer("Back"), untitled, BlendMode.Alpha, 181);
                    map.GetLayer("Back").Tiles[x + 4, y + 2] = new StaticTile(map.GetLayer("Back"), untitled, BlendMode.Alpha, 181);
                    map.GetLayer("Back").Tiles[x + 5, y + 2] = new StaticTile(map.GetLayer("Back"), untitled, BlendMode.Alpha, 181);

                    map.GetLayer("Back").Tiles[x + 4, y + 1].Properties["@Flip"] = 2;
                    map.GetLayer("Back").Tiles[x + 5, y + 1].Properties["@Flip"] = 2;
                    map.GetLayer("Back").Tiles[x + 4, y + 2].Properties["@Flip"] = 2;
                    map.GetLayer("Back").Tiles[x + 5, y + 2].Properties["@Flip"] = 2;

                    map.GetLayer("Back").Tiles[x + 4, y + 1].Properties["NoFurniture"] = "t";
                    map.GetLayer("Back").Tiles[x + 5, y + 1].Properties["NoFurniture"] = "t";
                    map.GetLayer("Back").Tiles[x + 4, y + 1].Properties["NPCBarrier"] = "t";
                    map.GetLayer("Back").Tiles[x + 5, y + 1].Properties["NPCBarrier"] = "t";
                    map.GetLayer("Back").Tiles[x + 4, y + 2].Properties["NoFurniture"] = "t";
                    map.GetLayer("Back").Tiles[x + 5, y + 2].Properties["NoFurniture"] = "t";

                    map.GetLayer("Buildings").Tiles[x + 6, y + 1] = new StaticTile(map.GetLayer("Buildings"), indoor, BlendMode.Alpha, 68);
                    map.GetLayer("Front").Tiles[x + 6, y + 1] = new StaticTile(map.GetLayer("Front"), indoor, BlendMode.Alpha, 68);
                    map.GetLayer("Buildings").Tiles[x + 6, y + 2] = new StaticTile(map.GetLayer("Buildings"), indoor, BlendMode.Alpha, 130);

                    map.GetLayer("Front").Tiles[x + 4, y + 3] = new StaticTile(map.GetLayer("Front"), indoor, BlendMode.Alpha, 0);
                    map.GetLayer("Front").Tiles[x + 5, y + 3] = new StaticTile(map.GetLayer("Front"), indoor, BlendMode.Alpha, 0);

                    map.GetLayer("Buildings").Tiles[x + 4, y + 1] = null;
                    map.GetLayer("Buildings").Tiles[x + 5, y + 1] = null;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Exception adding stair tiles.\n{ex}", LogLevel.Error);
            }
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!config.EnableMod)
                return false;

            if (asset.AssetNameEquals("Maps/FarmHouse2") || asset.AssetNameEquals("Maps/FarmHouse2_marriage"))
            {
                return true;
            }

            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            Monitor.Log("Editing asset" + asset.AssetName);

            if (asset.AssetNameEquals("Maps/FarmHouse2") || asset.AssetNameEquals("Maps/FarmHouse2_marriage"))
            {
                try
                {
                    var mapData = asset.AsMap();

                    TileSheet indoor = mapData.Data.TileSheets.First(s => s.Id == "indoor");
                    TileSheet untitled = mapData.Data.TileSheets.First(s => s.Id == "untitled tile sheet");

                    int x = config.MainFloorStairsX;
                    int y = config.MainFloorStairsY;

                    mapData.Data.GetLayer("Front").Tiles[x,y].TileIndex = 162;
                    mapData.Data.GetLayer("Front").Tiles[x + 1,y] = null;
                    mapData.Data.GetLayer("Front").Tiles[x + 2,y] = null;
                    mapData.Data.GetLayer("Front").Tiles[x + 1,y + 1] = null;
                    mapData.Data.GetLayer("Front").Tiles[x + 2,y + 1] = null;
                    mapData.Data.GetLayer("Front").Tiles[x + 1,y + 2] = null;
                    mapData.Data.GetLayer("Front").Tiles[x + 2,y + 2] = null;
                    mapData.Data.GetLayer("Buildings").Tiles[x + 1,y + 1] = null;
                    mapData.Data.GetLayer("Buildings").Tiles[x + 2,y + 1] = null;
                    mapData.Data.GetLayer("Buildings").Tiles[x + 1,y + 2] = null;
                    mapData.Data.GetLayer("Buildings").Tiles[x + 2,y + 2] = null;
                    mapData.Data.GetLayer("Back").Tiles[x + 1,y + 1] = null;
                    mapData.Data.GetLayer("Back").Tiles[x + 2,y + 1] = null;
                    mapData.Data.GetLayer("Back").Tiles[x + 1,y + 2] = null;
                    mapData.Data.GetLayer("Back").Tiles[x + 2,y + 2] = null;

                    mapData.Data.GetLayer("Buildings").Tiles[x + 3,y].TileIndex = 68;
                    mapData.Data.GetLayer("Front").Tiles[x + 3,y].TileIndex = 68;
                    
                    mapData.Data.GetLayer("Buildings").Tiles[x,y + 1] = new StaticTile(mapData.Data.GetLayer("Buildings"), indoor, BlendMode.Alpha, 64);
                    mapData.Data.GetLayer("Front").Tiles[x, y + 1] = new StaticTile(mapData.Data.GetLayer("Front"), indoor, BlendMode.Alpha, 64);

                    mapData.Data.GetLayer("Buildings").Tiles[x, y + 2] = new StaticTile(mapData.Data.GetLayer("Buildings"), indoor, BlendMode.Alpha, 96);

                    mapData.Data.GetLayer("Front").Tiles[x + 1, y + 2] = new StaticTile(mapData.Data.GetLayer("Front"), indoor, BlendMode.Alpha, 165);
                    mapData.Data.GetLayer("Front").Tiles[x + 2, y + 2] = new StaticTile(mapData.Data.GetLayer("Front"), indoor, BlendMode.Alpha, 165);


                    mapData.Data.GetLayer("Back").Tiles[x + 1, y + 1] = new StaticTile(mapData.Data.GetLayer("Back"), untitled, BlendMode.Alpha, 181);
                    mapData.Data.GetLayer("Back").Tiles[x + 2, y + 1] = new StaticTile(mapData.Data.GetLayer("Back"), untitled, BlendMode.Alpha, 181);
                    mapData.Data.GetLayer("Back").Tiles[x + 1,y + 1].Properties["NoFurniture"] = "t";
                    mapData.Data.GetLayer("Back").Tiles[x + 2,y + 1].Properties["NoFurniture"] = "t";
                    mapData.Data.GetLayer("Back").Tiles[x + 1,y + 1].Properties["NPCBarrier"] = "t";
                    mapData.Data.GetLayer("Back").Tiles[x + 2,y + 1].Properties["NPCBarrier"] = "t";

                    mapData.Data.GetLayer("Back").Tiles[x + 1, y + 2] = new StaticTile(mapData.Data.GetLayer("Back"), untitled, BlendMode.Alpha, 181);
                    mapData.Data.GetLayer("Back").Tiles[x + 2, y + 2] = new StaticTile(mapData.Data.GetLayer("Back"), untitled, BlendMode.Alpha, 181);
                    mapData.Data.GetLayer("Back").Tiles[x + 1,y + 2].Properties["NoFurniture"] = "t";
                    mapData.Data.GetLayer("Back").Tiles[x + 2,y + 2].Properties["NoFurniture"] = "t";

                    mapData.Data.GetLayer("Buildings").Tiles[x + 3, y + 1] = new StaticTile(mapData.Data.GetLayer("Buildings"), indoor, BlendMode.Alpha, 68);
                    mapData.Data.GetLayer("Front").Tiles[x + 3, y + 1] = new StaticTile(mapData.Data.GetLayer("Front"), indoor, BlendMode.Alpha, 68);
                    mapData.Data.GetLayer("Buildings").Tiles[x + 3, y + 2] = new StaticTile(mapData.Data.GetLayer("Buildings"), indoor, BlendMode.Alpha, 130);

                    mapData.Data.GetLayer("Front").Tiles[x + 1, y + 3] = new StaticTile(mapData.Data.GetLayer("Front"), indoor, BlendMode.Alpha, 0);
                    mapData.Data.GetLayer("Front").Tiles[x + 2, y + 3] = new StaticTile(mapData.Data.GetLayer("Front"), indoor, BlendMode.Alpha, 0);

                    mapData.Data.GetLayer("Buildings").Tiles[x + 1,y + 1] = null;
                    mapData.Data.GetLayer("Buildings").Tiles[x + 2,y + 1] = null;

                }
                catch (Exception ex)
                {
                    Monitor.Log($"Exception adding stair tiles.\n{ex}", LogLevel.Error);
                }
                return;
            }
        }
    }
}