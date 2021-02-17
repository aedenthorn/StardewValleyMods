using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MultiStoryFarmhouse
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetEditor, IAssetLoader
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        private static ModEntry context;
        public static ModConfig config;
        private static Dictionary<string, Floor> floorsList = new Dictionary<string, Floor>();
        private static Dictionary<int, Map> floorMaps = new Dictionary<int, Map>();

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

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(DecoratableLocation), nameof(DecoratableLocation.getWalls)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(getWalls_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(DecoratableLocation), nameof(DecoratableLocation.getFloors)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(getFloors_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(SaveGame), nameof(SaveGame.loadDataToLocations)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(loadDataToLocations_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), "resetLocalState"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(resetLocalState_Prefix)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(resetLocalState_Postfix))
            );
        }

        private static void resetLocalState_Prefix(ref Vector2 __state)
        {
            __state = new Vector2(-1,-1);
            if (Game1.isWarping && Game1.player.previousLocationName == "MultipleFloors0")
            {
                __state = new Vector2(Game1.xLocationAfterWarp, Game1.yLocationAfterWarp);
            }
        }
        private static void resetLocalState_Postfix(Vector2 __state)
        {
            if(__state.X >= 0)
            {
                Game1.player.Position = __state * 64f;
                Game1.xLocationAfterWarp = Game1.player.getTileX();
                Game1.yLocationAfterWarp = Game1.player.getTileY();
            }
        }

        private static void loadDataToLocations_Prefix(List<GameLocation> gamelocations)
        {
            context.Monitor.Log($"Checking save for multiple floors");

            for (int i = 0; i < config.FloorNames.Count; i++)
            {
                DecoratableLocation location = (DecoratableLocation)Game1.locations.FirstOrDefault(l => l.Name == $"MultipleFloors{i}");
                if (location == null)
                {
                    Vector2 stairs = floorsList[config.FloorNames[i]].stairsStart;
                    int x = (int)stairs.X;
                    int y = (int)stairs.Y;

                    context.Monitor.Log($"adding floor MultipleFloors{i}");
                    location = new DecoratableLocation($"Maps/MultipleFloorsMap{i}", $"MultipleFloors{i}");
                    Warp warp;
                    if (i < config.FloorNames.Count - 1)
                    {
                        Vector2 stairs1 = floorsList[config.FloorNames[i+1]].stairsStart;
                        int x1 = (int)stairs1.X;
                        int y1 = (int)stairs1.Y;
                        context.Monitor.Log($"adding upstairs warps");

                        warp = new Warp(x + 4, y + 3, $"MultipleFloors{i + 1}", x1 + 1, y1 + 2, true, false);
                        if (!location.warps.Contains(warp))
                            location.warps.Add(warp);
                        warp.TargetName = $"MultipleFloors{i + 1}";
                        warp = new Warp(x + 5, y + 3, $"MultipleFloors{i + 1}", x1 + 2, y1 + 2, true, false);
                        if (!location.warps.Contains(warp))
                            location.warps.Add(warp);
                        warp.TargetName = $"MultipleFloors{i + 1}";
                    }
                    if (i > 0)
                    {
                        Vector2 stairs0 = floorsList[config.FloorNames[i-1]].stairsStart;
                        int x0 = (int)stairs0.X;
                        int y0 = (int)stairs0.Y;
                        context.Monitor.Log($"adding downstairs warps");
                        warp = new Warp(x + 1, y + 3, $"MultipleFloors{i - 1}", x0 + 4, y0 + 2, true, false);
                        if (!location.warps.Contains(warp))
                            location.warps.Add(warp);
                        warp.TargetName = $"MultipleFloors0";
                        warp = new Warp(x + 2, y + 3, $"MultipleFloors{i - 1}", x0 + 5, y0 + 2, true, false);
                        if (!location.warps.Contains(warp))
                            location.warps.Add(warp);
                        warp.TargetName = $"MultipleFloors0";
                    }
                    else
                    {
                        context.Monitor.Log($"adding farmhouse warps");
                        warp = new Warp(x + 1, y + 3, "FarmHouse", 8, 24, true, false);
                        if (!location.warps.Contains(warp))
                            location.warps.Add(warp);
                        warp.TargetName = "FarmHouse";
                        warp = new Warp(x + 2, y + 3, "FarmHouse", 9, 24, true, false);
                        if (!location.warps.Contains(warp))
                            location.warps.Add(warp);
                        warp.TargetName = "FarmHouse";
                    }

                    Game1.locations.Add(location);

                }
                else
                    context.Monitor.Log($"Game already has floor MultipleFloors{i}");
            }

        }

        private static bool getFloors_Prefix(DecoratableLocation __instance, ref List<Rectangle> __result)
        {
            if (!__instance.Name.StartsWith("MultipleFloors"))
                return true;
            Floor floor = GetFloor(__instance.Name);
            __result = floor.floors;

            return false;
        }

        private static bool getWalls_Prefix(DecoratableLocation __instance, ref List<Rectangle> __result)
        {
            if (!__instance.Name.StartsWith("MultipleFloors"))
                return true;
            Floor floor = GetFloor(__instance.Name);
            __result = floor.walls;

            return false;
        }
        private static Floor GetFloor(string name)
        {
            int floorNo = int.Parse(name[name.Length - 1].ToString());
            return floorsList[config.FloorNames[floorNo]];
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if (!floorsList.Any())
                return;
            Vector2 stairs = floorsList[config.FloorNames[0]].stairsStart;
            int x = (int)stairs.X;
            int y = (int)stairs.Y;

            Warp warp = new Warp(config.MainFloorStairsX + 1, config.MainFloorStairsY + 3, "MultipleFloors0", x + 1, y + 2, true, false);
            if (!Utility.getHomeOfFarmer(Game1.player).warps.Contains(warp))
                Utility.getHomeOfFarmer(Game1.player).warps.Add(warp);
            warp.TargetName = "MultipleFloors0";
            warp = new Warp(config.MainFloorStairsX + 2, config.MainFloorStairsY + 3, "MultipleFloors0", x + 2, y + 2, true, false);
            if (!Utility.getHomeOfFarmer(Game1.player).warps.Contains(warp))
                Utility.getHomeOfFarmer(Game1.player).warps.Add(warp);
            warp.TargetName = "MultipleFloors0";
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
            TileSheet indoor = map.TileSheets.FirstOrDefault(s => s.Id == "indoor");
            TileSheet untitled = map.TileSheets.FirstOrDefault(s => s.Id == "untitled tile sheet");
            Vector2 stairs = floorsList[config.FloorNames[floorNo]].stairsStart;
            int x = (int)stairs.X;
            int y = (int)stairs.Y;
            try
            {
                // left 
                map.GetLayer("Buildings").Tiles[x + 1, y + 1] = null;
                map.GetLayer("Buildings").Tiles[x + 2, y + 1] = null;

                map.GetLayer("Front").Tiles[x + 1, y] = null;
                map.GetLayer("Front").Tiles[x + 2, y] = null;
                map.GetLayer("Front").Tiles[x + 1, y + 1] = null;
                map.GetLayer("Front").Tiles[x + 2, y + 1] = null;
                map.GetLayer("Front").Tiles[x, y] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 162);
                map.GetLayer("Front").Tiles[x + 3, y] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 163);

                map.GetLayer("Buildings").Tiles[x, y + 1] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 64);
                map.GetLayer("Front").Tiles[x, y + 1] = new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 64);

                map.GetLayer("Buildings").Tiles[x, y + 2] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 96);

                map.GetLayer("Front").Tiles[x + 1, y + 2] = new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 165);
                map.GetLayer("Front").Tiles[x + 2, y + 2] = new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 165);

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

                map.GetLayer("Buildings").Tiles[x + 3, y + 1] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 68);
                map.GetLayer("Front").Tiles[x + 3, y + 1] = new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 68);
                map.GetLayer("Buildings").Tiles[x + 3, y + 2] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 130);

                map.GetLayer("Front").Tiles[x + 1, y + 3] = new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 0);
                map.GetLayer("Front").Tiles[x + 2, y + 3] = new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 0);


                if (floorNo < config.FloorNames.Count - 1)
                {
                    Monitor.Log($"adding upstairs for floor {floorNo} / {config.FloorNames.Count}");

                    // right 

                    map.GetLayer("Buildings").Tiles[x + 4, y + 1] = null;
                    map.GetLayer("Buildings").Tiles[x + 5, y + 1] = null;

                    map.GetLayer("Front").Tiles[x + 4, y] = null;
                    map.GetLayer("Front").Tiles[x + 5, y] = null;
                    map.GetLayer("Front").Tiles[x + 4, y + 1] = null;
                    map.GetLayer("Front").Tiles[x + 5, y + 1] = null;

                    map.GetLayer("Front").Tiles[x + 3, y] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 100);
                    map.GetLayer("Front").Tiles[x + 6, y] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 163);


                    map.GetLayer("Buildings").Tiles[x + 3, y + 1] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 132);
                    map.GetLayer("Front").Tiles[x + 3, y + 1] = new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 132);

                    map.GetLayer("Buildings").Tiles[x + 3, y + 2] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 164);

                    map.GetLayer("Front").Tiles[x + 4, y + 2] = new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 165);
                    map.GetLayer("Front").Tiles[x + 5, y + 2] = new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 165);


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

                    map.GetLayer("Buildings").Tiles[x + 6, y + 1] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 68);
                    map.GetLayer("Front").Tiles[x + 6, y + 1] = new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 68);
                    map.GetLayer("Buildings").Tiles[x + 6, y + 2] = new StaticTile(map.GetLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 130);

                    map.GetLayer("Front").Tiles[x + 4, y + 3] = new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 0);
                    map.GetLayer("Front").Tiles[x + 5, y + 3] = new StaticTile(map.GetLayer("Front"), map.TileSheets[0], BlendMode.Alpha, 0);

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

                    int x = config.MainFloorStairsX;
                    int y = config.MainFloorStairsY;

                    mapData.Data.GetLayer("Front").Tiles[x,y].TileIndex = 162;
                    mapData.Data.GetLayer("Front").Tiles[x + 1,y] = null;
                    mapData.Data.GetLayer("Front").Tiles[x + 2,y] = null;
                    mapData.Data.GetLayer("Front").Tiles[x + 1,y + 1] = null;
                    mapData.Data.GetLayer("Front").Tiles[x + 2,y + 1] = null;

                    mapData.Data.GetLayer("Buildings").Tiles[x + 3,y].TileIndex = 68;
                    mapData.Data.GetLayer("Front").Tiles[x + 3,y].TileIndex = 68;
                    
                    mapData.Data.GetLayer("Buildings").Tiles[x,y + 1] = new StaticTile(mapData.Data.GetLayer("Buildings"), mapData.Data.TileSheets[0], BlendMode.Alpha, 64);
                    mapData.Data.GetLayer("Front").Tiles[x, y + 1] = new StaticTile(mapData.Data.GetLayer("Front"), mapData.Data.TileSheets[0], BlendMode.Alpha, 64);

                    mapData.Data.GetLayer("Buildings").Tiles[x, y + 2] = new StaticTile(mapData.Data.GetLayer("Buildings"), mapData.Data.TileSheets[0], BlendMode.Alpha, 96);

                    mapData.Data.GetLayer("Front").Tiles[x + 1, y + 2] = new StaticTile(mapData.Data.GetLayer("Front"), mapData.Data.TileSheets[0], BlendMode.Alpha, 165);
                    mapData.Data.GetLayer("Front").Tiles[x + 2, y + 2] = new StaticTile(mapData.Data.GetLayer("Front"), mapData.Data.TileSheets[0], BlendMode.Alpha, 165);


                    mapData.Data.GetLayer("Back").Tiles[x + 1, y + 1] = new StaticTile(mapData.Data.GetLayer("Back"), mapData.Data.TileSheets[2], BlendMode.Alpha, 181);
                    mapData.Data.GetLayer("Back").Tiles[x + 2, y + 1] = new StaticTile(mapData.Data.GetLayer("Back"), mapData.Data.TileSheets[2], BlendMode.Alpha, 181);
                    mapData.Data.GetLayer("Back").Tiles[x + 1,y + 1].Properties["NoFurniture"] = "t";
                    mapData.Data.GetLayer("Back").Tiles[x + 2,y + 1].Properties["NoFurniture"] = "t";
                    mapData.Data.GetLayer("Back").Tiles[x + 1,y + 1].Properties["NPCBarrier"] = "t";
                    mapData.Data.GetLayer("Back").Tiles[x + 2,y + 1].Properties["NPCBarrier"] = "t";

                    mapData.Data.GetLayer("Back").Tiles[x + 1, y + 2] = new StaticTile(mapData.Data.GetLayer("Back"), mapData.Data.TileSheets[2], BlendMode.Alpha, 181);
                    mapData.Data.GetLayer("Back").Tiles[x + 2, y + 2] = new StaticTile(mapData.Data.GetLayer("Back"), mapData.Data.TileSheets[2], BlendMode.Alpha, 181);
                    mapData.Data.GetLayer("Back").Tiles[x + 1,y + 2].Properties["NoFurniture"] = "t";
                    mapData.Data.GetLayer("Back").Tiles[x + 2,y + 2].Properties["NoFurniture"] = "t";

                    mapData.Data.GetLayer("Buildings").Tiles[x + 3, y + 1] = new StaticTile(mapData.Data.GetLayer("Buildings"), mapData.Data.TileSheets[0], BlendMode.Alpha, 68);
                    mapData.Data.GetLayer("Front").Tiles[x + 3, y + 1] = new StaticTile(mapData.Data.GetLayer("Front"), mapData.Data.TileSheets[0], BlendMode.Alpha, 68);
                    mapData.Data.GetLayer("Buildings").Tiles[x + 3, y + 2] = new StaticTile(mapData.Data.GetLayer("Buildings"), mapData.Data.TileSheets[0], BlendMode.Alpha, 130);

                    mapData.Data.GetLayer("Front").Tiles[x + 1, y + 3] = new StaticTile(mapData.Data.GetLayer("Front"), mapData.Data.TileSheets[0], BlendMode.Alpha, 0);
                    mapData.Data.GetLayer("Front").Tiles[x + 2, y + 3] = new StaticTile(mapData.Data.GetLayer("Front"), mapData.Data.TileSheets[0], BlendMode.Alpha, 0);

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