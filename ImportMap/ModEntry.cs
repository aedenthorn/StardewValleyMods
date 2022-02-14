using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using Object = StardewValley.Object;

namespace ImportMap
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        private static IAdvancedFluteBlocksApi fluteBlockApi;
        private static ITrainTracksApi trainTrackApi;

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

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            Helper.ConsoleCommands.Add("importmap", "Import map data from import.tmx", new Action<string, string[]>(ImportMap));
            Helper.ConsoleCommands.Add("nukemap", "Nuke the map.", new Action<string, string[]>(NukeMap));
            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(ChatBox), "runCommand"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.ChatBox_runCommand_Prefix))
            );
        }

        private void ImportMap(string arg1, string[] arg2)
        {
            DoImport();
        }

        public static bool ChatBox_runCommand_Prefix(string command)
        {
            if (!Config.EnableMod)
                return true;

            if (command.Equals("nukemap"))
            {
                NukeMap(null, null);
                return false;
            }
            if (command.Equals("importmap"))
            {
                DoImport();
                return false;
            }
            return true;
        }
        private static void NukeMap(string arg1, string[] arg2)
        {
            Game1.currentLocation.objects.Clear();
            Game1.currentLocation.terrainFeatures.Clear();
            Game1.currentLocation.overlayObjects.Clear();
            Game1.currentLocation.resourceClumps.Clear();
            Game1.currentLocation.largeTerrainFeatures.Clear();
            Game1.currentLocation.furniture.Clear();
        }

        private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
        {
            if (Config.EnableMod && Config.ImportKey.JustPressed())
            {
                Monitor.Log("importing map");
                DoImport();
            }
        }

        private static void DoImport()
        {
            if (!File.Exists(Path.Combine(SHelper.DirectoryPath, "assets", "import.tmx")))
            {
                SMonitor.Log("import file not found", LogLevel.Error);
                return;
            }
            Map map = SHelper.Content.Load<Map>("assets/import.tmx");
            if (map == null)
            {
                SMonitor.Log("map is null", LogLevel.Error);
                return;
            }
            Dictionary<string, Layer> layersById = AccessTools.FieldRefAccess<Map, Dictionary<string, Layer>>(map, "m_layersById");
            if (layersById.TryGetValue("Flooring", out Layer flooringLayer))
            {
                for (int y = 0; y < flooringLayer.LayerHeight; y++)
                {
                    for (int x = 0; x < flooringLayer.LayerWidth; x++)
                    {
                        if(flooringLayer.Tiles[x, y] != null && flooringLayer.Tiles[x, y].TileIndex >= 0)
                        {
                            Game1.player.currentLocation.terrainFeatures[new Vector2(x, y)] = new Flooring(flooringLayer.Tiles[x, y].TileIndex);
                        }
                    }
                }
            }
            if (trainTrackApi != null && layersById.TryGetValue("TrainTracks", out Layer trackLayer))
            {
                foreach (var v in Game1.player.currentLocation.terrainFeatures.Keys)
                {
                    trainTrackApi.RemoveTrack(Game1.player.currentLocation, v);
                }

                for (int y = 0; y < trackLayer.LayerHeight; y++)
                {
                    for (int x = 0; x < trackLayer.LayerWidth; x++)
                    {
                        if(trackLayer.Tiles[x, y] != null && trackLayer.Tiles[x, y].TileIndex >= 0)
                        {
                            PropertyValue switchData;
                            PropertyValue speedData;
                            trackLayer.Tiles[x, y].Properties.TryGetValue("Switches", out switchData);
                            if(switchData != null)
                            {
                               SMonitor.Log($"Got switch data for tile {x},{y}: {switchData}");
                            }
                            trackLayer.Tiles[x, y].Properties.TryGetValue("Speed", out speedData);
                            int speed = -1;
                            if(speedData != null && int.TryParse(speedData, out speed))
                            {
                                SMonitor.Log($"Got speed for tile {x},{y}: {speed}");
                            }
                            trainTrackApi.TryPlaceTrack(Game1.currentLocation, new Vector2(x, y), trackLayer.Tiles[x, y].TileIndex, switchData == null ? null : switchData.ToString(), speed, true);
                        }
                    }
                }
            }
            if (layersById.TryGetValue("FluteBlocks", out Layer fluteLayer))
            {
                foreach (var v in Game1.player.currentLocation.objects.Keys)
                {
                    if (Game1.player.currentLocation.objects[v] is not null && Game1.player.currentLocation.objects[v].Name == "Flute Block")
                        Game1.player.currentLocation.objects.Remove(v);
                }
                for (int y = 0; y < fluteLayer.LayerHeight; y++)
                {
                    for (int x = 0; x < fluteLayer.LayerWidth; x++)
                    {
                        if(fluteLayer.Tiles[x, y] != null && fluteLayer.Tiles[x, y].TileIndex >= 0 && !Game1.player.currentLocation.objects.ContainsKey(new Vector2(x, y)))
                        {
                            var block = new Object(new Vector2(x, y), 464, 1);
                            block.preservedParentSheetIndex.Value = fluteLayer.Tiles[x, y].TileIndex % 24 * 100;
                            if(fluteBlockApi != null)
                            {
                                var tone = fluteBlockApi.GetFluteBlockToneFromIndex(fluteLayer.Tiles[x, y].TileIndex / 24);
                                if(tone != null)
                                {
                                    block.modData["aedenthorn.AdvancedFluteBlocks/tone"] = tone;
                                }
                            }
                            Game1.player.currentLocation.objects[new Vector2(x, y)] = block;
                        }
                    }
                }
            }
            if (layersById.TryGetValue("DrumBlocks", out Layer drumLayer))
            {
                foreach (var v in Game1.player.currentLocation.objects.Keys)
                {
                    if (Game1.player.currentLocation.objects[v] is not null && Game1.player.currentLocation.objects[v].Name == "Drum Block")
                        Game1.player.currentLocation.objects.Remove(v);
                }
                for (int y = 0; y < drumLayer.LayerHeight; y++)
                {
                    for (int x = 0; x < drumLayer.LayerWidth; x++)
                    {
                        if(drumLayer.Tiles[x, y] != null && drumLayer.Tiles[x, y].TileIndex >= 0 && !Game1.player.currentLocation.objects.ContainsKey(new Vector2(x, y)))
                        {
                            var block = new Object(new Vector2(x, y), 463, 1);
                            block.preservedParentSheetIndex.Value = drumLayer.Tiles[x, y].TileIndex;
                            Game1.player.currentLocation.objects[new Vector2(x, y)] = block;
                        }
                    }
                }
            }
            if (layersById.TryGetValue("Objects", out Layer objLayer))
            {
                var dict = SHelper.Content.Load<Dictionary<int, string>>("Data/Crops", ContentSource.GameContent);

                for (int y = 0; y < objLayer.LayerHeight; y++)
                {
                    for (int x = 0; x < objLayer.LayerWidth; x++)
                    {
                        if(objLayer.Tiles[x, y] != null && objLayer.Tiles[x, y].TileIndex >= 0 && !Game1.player.currentLocation.terrainFeatures.ContainsKey(new Vector2(x, y)) && !Game1.player.currentLocation.objects.ContainsKey(new Vector2(x, y)) && !Game1.player.currentLocation.objects.ContainsKey(new Vector2(x, y)))
                        {
                            if (dict.TryGetValue(objLayer.Tiles[x, y].TileIndex, out string cropData))
                            {
                                Crop crop = new Crop(objLayer.Tiles[x, y].TileIndex, x, y);
                                HoeDirt dirt = new HoeDirt(1, crop);
                                Game1.player.currentLocation.terrainFeatures.Add(new Vector2(x, y), dirt);
                                continue;
                            }
                            var cropkvp = dict.FirstOrDefault(kvp => kvp.Value.Split('/')[3] == objLayer.Tiles[x, y].TileIndex + "");
                            if (cropkvp.Value != null)
                            {
                                Crop crop = new Crop(cropkvp.Key, x, y);
                                crop.growCompletely();
                                HoeDirt dirt = new HoeDirt(1, crop);
                                Game1.player.currentLocation.terrainFeatures.Add(new Vector2(x, y), dirt);
                            }
                            else
                            {
                                var obj = new Object(new Vector2(x, y), objLayer.Tiles[x, y].TileIndex, 1);
                                Game1.player.currentLocation.objects[new Vector2(x, y)] = obj;
                            }
                        }
                    }
                }
            }
            if (layersById.TryGetValue("Trees", out Layer treeLayer))
            {
                for (int y = 0; y < treeLayer.LayerHeight; y++)
                {
                    for (int x = 0; x < treeLayer.LayerWidth; x++)
                    {
                        Tree tree;
                        if (treeLayer.Tiles[x, y] != null && treeLayer.Tiles[x, y].TileIndex >= 9 && !Game1.player.currentLocation.terrainFeatures.ContainsKey(new Vector2(x, y)))
                        {
                            switch(treeLayer.Tiles[x, y].TileIndex)
                            {
                                case 9:
                                    tree = new Tree(1, 5);
                                    break;
                                case 10:
                                    tree = new Tree(2, 5);
                                    break;
                                case 11:
                                    tree = new Tree(3, 5);
                                    break;
                                case 12:
                                    tree = new Tree(6, 5);
                                    break;
                                case 31:
                                    tree = new Tree(7, 5);
                                    break;
                                case 32:
                                    tree = new Tree(9, 5);
                                    break;
                                default:
                                    continue;
                            }
                            Game1.player.currentLocation.terrainFeatures[new Vector2(x, y)] = tree;
                        }
                    }
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            fluteBlockApi = Helper.ModRegistry.GetApi<IAdvancedFluteBlocksApi>("aedenthorn.AdvancedFluteBlocks");
            trainTrackApi = Helper.ModRegistry.GetApi<ITrainTracksApi>("aedenthorn.TrainTracks");

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
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Import Key",
                getValue: () => Config.ImportKey.ToString(),
                setValue: delegate (string value) { try { Config.ImportKey = KeybindList.Parse(value); } catch { } }
            );
        }
    }
}
