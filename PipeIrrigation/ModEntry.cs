using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using Object = StardewValley.Object;

namespace PipeIrrigation
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static Texture2D dropTexture;
        public static Dictionary<string, List<Vector2>> wateringTileDict = new Dictionary<string, List<Vector2>>();
        public static Dictionary<string, List<Vector2>> wateringPipeDict = new Dictionary<string, List<Vector2>>();

        public static Harmony harmony;
        public static IUtilityGridApi utilityGridAPI;

        public static Vector2[] tileOffsets = new Vector2[]
        {
            new Vector2(-1, -1),
            new Vector2(-1, 0),
            new Vector2(-1, 1),
            new Vector2(0, -1),
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, -1),
            new Vector2(1, 0),
            new Vector2(1, 1)
        };

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

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;

            harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
               original: AccessTools.Method(typeof(Farm), nameof(Farm.DayUpdate)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farm_DayUpdate_Postfix))
            );
            dropTexture = Helper.Content.Load<Texture2D>("assets/drop.png");
        }

        private void GameLoop_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (!Config.EnableMod || !(Game1.player.currentLocation is Farm))
                return;
            RefreshWateringTiles(Game1.player.currentLocation, false);
        }

        private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod ||!Config.ShowWateredTilesLabelOnGrid || !utilityGridAPI.ShowingWaterGrid() || !wateringTileDict.ContainsKey(Game1.currentLocation.Name))
                return;
            foreach(var tile in wateringTileDict[Game1.currentLocation.Name])
            {
                e.SpriteBatch.Draw(dropTexture, Game1.GlobalToLocal(Game1.viewport, tile * 64), Color.White);
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            utilityGridAPI = Helper.ModRegistry.GetApi<IUtilityGridApi>("aedenthorn.UtilityGrid");
            if (utilityGridAPI == null)
            {
                Monitor.Log("Utility Grid API not loaded", LogLevel.Error);
            }
            utilityGridAPI.AddRefreshAction(GridActionHandler);
            utilityGridAPI.AddShowGridAction(GridActionHandler);
            utilityGridAPI.AddHideGridAction(GridActionHandler);
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null)
            {
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
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Water Surrounding Tiles?",
                    getValue: () => Config.WaterSurroundingTiles,
                    setValue: value => Config.WaterSurroundingTiles = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Show Grid Label?",
                    tooltip: () => "Show a water drop on each watered tile",
                    getValue: () => Config.ShowWateredTilesLabelOnGrid,
                    setValue: value => Config.ShowWateredTilesLabelOnGrid = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Show Sprinkler Animations?",
                    getValue: () => Config.ShowSprinklerAnimations,
                    setValue: value => Config.ShowSprinklerAnimations = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "% Water Per Tile",
                    tooltip: () => "100 means each tile uses 1 water",
                    getValue: () => Config.PercentWaterPerTile,
                    setValue: value => Config.PercentWaterPerTile = value
                );
            }
        }

        private void GridActionHandler(object sender, KeyValuePair<GameLocation, int> e)
        {
            if (!Config.EnableMod || !(e.Key is Farm) || e.Value != 0)
                return;
            RefreshWateringTiles(e.Key, false);
        }

        private static void RefreshWateringTiles(GameLocation location, bool use)
        {
            wateringPipeDict[location.Name] = new List<Vector2>();
            wateringTileDict[location.Name] = new List<Vector2>();
            foreach (List<Vector2> group in utilityGridAPI.LocationWaterPipes(location))
            {
                if (group.Count == 0)
                    continue;
                Vector2 power = utilityGridAPI.TileGroupWaterVector(location, (int)group[0].X, (int)group[0].Y);
                float netExcess = power.X + power.Y;

                List<Vector2> pipeList = new List<Vector2>();
                List<Vector2> hoeDirtList = new List<Vector2>();
                foreach (Vector2 tile in group)
                {
                    if (Config.WaterSurroundingTiles)
                    {
                        List<Vector2> tileHoeDirtList = new List<Vector2>();
                        foreach (Vector2 offset in tileOffsets)
                        {
                            if (location.terrainFeatures.ContainsKey(tile + offset) && location.terrainFeatures[tile + offset] is HoeDirt)
                                tileHoeDirtList.Add(tile + offset);
                        }
                        if (tileHoeDirtList.Count > 0)
                        {
                            pipeList.Add(tile);
                            foreach(var v in tileHoeDirtList)
                            {
                                if(!hoeDirtList.Contains(v))
                                    hoeDirtList.Add(v);
                            }
                        }
                    }
                    else if (location.terrainFeatures.ContainsKey(tile) && location.terrainFeatures[tile] is HoeDirt)
                    {
                        pipeList.Add(tile);
                        if (!hoeDirtList.Contains(tile))
                            hoeDirtList.Add(tile);
                    }
                }
                bool enough = hoeDirtList.Count * Config.PercentWaterPerTile / 100f <= netExcess;
                if (use)
                    SMonitor.Log($"tiles {hoeDirtList.Count}, percent {Config.PercentWaterPerTile / 100f}, vector {power}, excess {netExcess}");
                if (!enough)
                {
                    float lacking = hoeDirtList.Count * Config.PercentWaterPerTile / 100f - netExcess;

                    foreach (var obj in utilityGridAPI.TileGroupWaterObjects(location, (int)group[0].X, (int)group[0].Y))
                    {
                        if(location.objects.ContainsKey(obj) && location.objects[obj].modData.ContainsKey("aedenthorn.UtilityGrid/waterCharge"))
                        {
                            float charge = float.Parse(location.objects[obj].modData["aedenthorn.UtilityGrid/waterCharge"], CultureInfo.InvariantCulture);
                            float required = Math.Min(charge, lacking);
                            lacking -= required;
                            if(use)
                                location.objects[obj].modData["aedenthorn.UtilityGrid/waterCharge"] = (charge - required).ToString();
                            if (lacking <= 0)
                            {
                                enough = true;
                                break;
                            }
                        }
                    }
                }
                if (enough)
                {
                    wateringPipeDict[location.Name].AddRange(pipeList);
                    wateringTileDict[location.Name].AddRange(hoeDirtList);
                }
            }
        }

        public static void Farm_DayUpdate_Postfix(Farm __instance)
        {
            if (!Config.EnableMod || !wateringPipeDict.ContainsKey(__instance.Name))
                return;
            RefreshWateringTiles(__instance, true);
            SMonitor.Log($"{wateringPipeDict[__instance.Name].Count} pipes watering {wateringTileDict[__instance.Name].Count} tiles in {__instance.Name}");
            foreach(Vector2 pipe in wateringPipeDict[__instance.Name])
            {
                Object sprinkler = new Object(pipe, 621);
                __instance.postFarmEventOvernightActions.Add(delegate
                {
                    if (Game1.player.team.SpecialOrderRuleActive("NO_SPRINKLER", null))
                    {
                        return;
                    }
                    if (Config.WaterSurroundingTiles)
                    {
                        foreach (Vector2 tile in tileOffsets)
                        {
                            sprinkler.ApplySprinkler(__instance, pipe + tile);
                        }
                        if (Config.ShowSprinklerAnimations)
                        {
                            __instance.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 1984, 192, 192), 60f, 3, 100, pipe * 64f + new Vector2(-64f, -64f), false, false)
                            {
                                color = Color.White * 0.4f,
                                delayBeforeAnimationStart = Game1.random.Next(1000),
                                id = pipe.X * 4000f + pipe.Y
                            });
                        }
                    }
                    else
                    {
                        sprinkler.ApplySprinkler(__instance, pipe);
                        if (Config.ShowSprinklerAnimations)
                        {
                            int delay = Game1.random.Next(1000);
                            __instance.temporarySprites.Add(new TemporaryAnimatedSprite(29, pipe * 64f + new Vector2(0f, -48f), Color.White * 0.5f, 4, false, 60f, 100, -1, -1f, -1, 0)
                            {
                                delayBeforeAnimationStart = delay,
                                id = pipe.X * 4000f + pipe.Y
                            });
                        }
                    }
                });
            }
        }
    }
}