using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using xTile;
using xTile.Tiles;

namespace Survivors
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static string modKey = "aedenthorn.Survivors";
        private const string modCoinKey = "aedenthorn.Survivors/Coin";
        private const string modChunkKey = "aedenthorn.Survivors/Chunk";
        private const string modPlacedKey = "aedenthorn.Survivors/Placed";
        public static string codeBiomePath = "aedenthorn.Survivors/code_biomes";
        public static string landmarkDictPath = "aedenthorn.Survivors/landmarks";
        public static string monsterDictPath = "aedenthorn.Survivors/monsters";
        public static string seedKey = "aedenthorn_Survivors_seed";
        public static string mapPath = "aedenthorn_Survivors_Map";
        public static string locName = "StardewSurvivors";
        public static string tilePrefix = "SurvivorsTile";
        public static int openWorldChunkSize = 100;
        public static bool warping = false;

        public static Dictionary<long, Point> playerTilePoints = new();
        public static Dictionary<long, Point> playerChunks = new();

        public static Dictionary<string, Dictionary<int, AnimatedTile>> animatedTiles = new();
        
        public static Dictionary<Point, Dictionary<Vector2, string>> treeCenters = new();
        public static Dictionary<Point, HashSet<Vector2>> rockCenters = new();
        public static Dictionary<Point, HashSet<Vector2>> grassCenters = new();
        public static Dictionary<Point, List<Point>> lakeCenters = new();
        public static Dictionary<Point, Dictionary<Vector2, string>> monsterCenters = new();

        public static Dictionary<Point, HashSet<Rectangle>> landmarkRects = new();
        public static HashSet<Rectangle> loadedLandmarkRects = new();
        public static Dictionary<Point ,HashSet<Rectangle>> lakeRects = new();
        public static Dictionary<Point, HashSet<Rectangle>> outcropRects = new();

        public static GameLocation openWorldLocation;
        //private static Dictionary<string, Biome> biomes = new Dictionary<string, Biome>();
        public static Dictionary<string, Func<ulong, int, int, WorldChunk>> biomeCodeDict;

        public static Dictionary<Point, WorldChunk> cachedChunks = new();
        public static List<Point> loadedChunks = new();

        public static Dictionary<string, Landmark> landmarkDict = new();
        
        public static int RandomSeed = -1;
        public static List<int> grassTiles = new List<int>() { 300, 304, 305, 351, 150, 151, 152, 175 };

        private static IAdvancedLootFrameworkApi advancedLootFrameworkApi;
        private static IStardewGamesAPI gamesAPI;
        private static List<object> treasuresList = new();
        private static readonly Color[] tintColors = new Color[]
        {
            Color.DarkGray,
            Color.Brown,
            Color.Silver,
            Color.Gold,
            Color.Purple,
        };

        public enum BuildStage
        {
            Begin,
            Landmarks,
            Chests,
            Trees,
            Bushes,
            Artifacts,
            Chunks,
            Forage,
            Rocks,
            Grass,
            Monsters,
            Done
        }
        public enum LoadStage
        {
            TerrainFeatures,
            Objects,
            Monsters,
            Done
        }

        public static List<Point> chunksUnloading = new List<Point>();
        public static List<Point> chunksWaitingToCache = new List<Point>();
        public static List<Point> chunksCaching = new List<Point>();
        public static List<Point> chunksWaitingToBuild = new List<Point>();
        public static List<Point> chunksBuilding = new List<Point>();
        public static List<Point> chunksWaitingToLoad = new List<Point>();
        public static List<Point> chunksLoading = new List<Point>();
        public static BuildStage currentBuildStage;
        public static LoadStage currentLoadStage;

        private static ClickableTextureComponent upperRightCloseButton;
        private static ClickableTextureComponent northButton;
        private static ClickableTextureComponent westButton;
        private static ClickableTextureComponent eastButton;
        private static ClickableTextureComponent southButton;
        public static Texture2D renderTarget;
        public static bool playerTileChanged;

        private static bool showingMap;
        private static Rectangle mapRect;
        public static Point mapOffset;

        private static HashSet<Point> waterTiles = new();


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;

            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Display.RenderedActiveMenu += Display_RenderedActiveMenu;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.Display.WindowResized += Display_WindowResized;


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            openWorldLocation = null;
            renderTarget = null;

            cachedChunks.Clear();
            loadedChunks.Clear();

            chunksUnloading.Clear();
            chunksWaitingToCache.Clear();
            chunksCaching.Clear();
            chunksWaitingToBuild.Clear();
            chunksBuilding.Clear();
            chunksWaitingToLoad.Clear();
            chunksLoading.Clear();

            currentBuildStage = 0;
            currentLoadStage = 0;
            

            lakeRects.Clear();
            outcropRects.Clear();
            landmarkRects.Clear();
            loadedLandmarkRects.Clear();

            waterTiles.Clear();
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (Config.ModEnabled && Game1.IsMasterGame)
            {
                DoCachePoll();
            }
        }


        private void Display_WindowResized(object sender, WindowResizedEventArgs e)
        {
            renderTarget = null;
        }

        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            renderTarget = null;
            upperRightCloseButton = null;
            showingMap = true;
            if (!Config.ModEnabled || !Context.IsWorldReady || !Config.DrawMap || Game1.currentLocation != openWorldLocation || Game1.activeClickableMenu is not GameMenu || (Game1.activeClickableMenu as GameMenu).GetCurrentPage() is not MapPage)
                return;
            TakeMapScreenshot(openWorldLocation, Config.MapScale);

        }


        private void Display_RenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || !Config.DrawMap || Game1.currentLocation != openWorldLocation || Game1.activeClickableMenu is not GameMenu || (Game1.activeClickableMenu as GameMenu).GetCurrentPage() is not MapPage)
                return;
            DrawMap(e);
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;

            if (Config.Debug && e.Button == SButton.L)
            {
                ReloadOpenWorld(true);
                playerTilePoints.Clear();
                playerChunks.Clear();
            }
            if (Config.Debug && e.Button == SButton.N)
            {

                CreateAnimatedTiles();
            }
            if (Config.DrawMap && showingMap && e.Button == SButton.MouseLeft && renderTarget != null)
            {
                if (upperRightCloseButton.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                {
                    Game1.playSound("bigDeSelect");
                    showingMap = false;
                }
                //else if (westButton.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                //{
                //    //Game1.playSound("bigDeSelect");
                //    mapOffset += new Point(-1, 0);
                //    TakeMapScreenshot(openWorldLocation, 0.25f);
                //}
                //else if (eastButton.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                //{
                //    //Game1.playSound("bigDeSelect");
                //    mapOffset += new Point(1, 0);
                //    renderTarget = null;
                //}
                //else if (northButton.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                //{
                //    //Game1.playSound("bigDeSelect");
                //    mapOffset += new Point(0, -1);
                //    renderTarget = null;
                //}
                //else if (southButton.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                //{
                //    //Game1.playSound("bigDeSelect");
                //    mapOffset += new Point(0, 1);
                //    renderTarget = null;
                //}
                else if (Config.Debug && mapRect.Contains(Game1.getMousePosition()))
                {
                    float scale = mapRect.Width / (float)(Config.MapTilesDimension);
                    float x = Game1.getMousePosition().X - mapRect.X - mapRect.Width / 2f;
                    float y = Game1.getMousePosition().Y - mapRect.Y - mapRect.Height / 2f;
                    x /= scale;
                    y /= scale;
                    Game1.player.Position += new Vector2(x, y);
                    Game1.activeClickableMenu.exitThisMenu(true);
                }
                SHelper.Input.Suppress(e.Button);
            }
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            if(Config.ModEnabled && Game1.IsMasterGame)
                ReloadOpenWorld(Config.NewMapDaily);
        }

        public override object GetApi()
        {
            return new SurvivorsAPI();
        }
        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;

            openWorldLocation = Game1.RequireLocation(locName);

            CreateAnimatedTiles();

            treeCenters = new();
            rockCenters = new();
            grassCenters = new();
            monsterCenters = new();
            ReloadOpenWorld(true);

            //CacheAllChunks();
        }


        private void CacheAllChunks()
        {
            Stopwatch s = new();
            s.Start();
            int num = Config.OpenWorldSize / openWorldChunkSize;
            SMonitor.Log($"Caching {num * num} chunks");
            for (int x = 0; x < num; x++)
                for (int y = 0; y < num; y++)
                {
                    //if(y == 0)
                    //    SMonitor.Log($"Caching {(x * num)}/{(num*num)} chunks");
                    CacheChunk(new Point(x, y), false);
                }
            s.Stop();
            SMonitor.Log($"Cached {num * num} chunks in {s.Elapsed.TotalSeconds}s");
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(mapPath))
            {
                e.LoadFromModFile<Map>("assets/Survivors.tmx", AssetLoadPriority.Exclusive);

                e.Edit(asset =>
                {
                    Map map = asset.AsMap().Data;
                    map.Properties["Warp"] = $"{Config.OpenWorldSize / 2 + openWorldChunkSize / 2} {Config.OpenWorldSize - 1} Mine 68 7 {Config.OpenWorldSize / 2 + openWorldChunkSize / 2 + 1} {Config.OpenWorldSize - 1} Mine 69 7";
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(codeBiomePath))
            {
                e.LoadFrom(() => new Dictionary<string, Func<ulong, int, int, WorldChunk>>(), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Monsters/Shadow Guy"))
            {
                e.LoadFrom(() => SHelper.GameContent.Load<Texture2D>("Characters/Monsters/Shadow Brute"), AssetLoadPriority.High);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Monsters"))
            {
                e.Edit(asset =>
                {
                    var dict = asset.AsDictionary<string, string>().Data;
                    if(!dict.ContainsKey("Shadow Guy"))
                    {
                        dict["Shadow Guy"] = dict["Shadow Brute"].Replace("Brute", "Guy");
                    }
                    if(!dict.ContainsKey("Shadow Girl"))
                    {
                        dict["Shadow Girl"] = dict["Shadow Brute"].Replace("Brute", "Girl");
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(landmarkDictPath))
            {
                e.LoadFrom(() => new Dictionary<string, Landmark>(), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(monsterDictPath))
            {
                e.LoadFrom(() => GetMonsterDict(), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            {
                e.Edit(asset =>
                {
                    IDictionary<string, LocationData> data = asset.AsDictionary<string, LocationData>().Data;

                    data.Add(locName, new LocationData()
                    {
                        DisplayName = "Stardew Survivors World",
                        CreateOnLoad = new CreateLocationData()
                        {
                            MapPath = mapPath,
                            Type = "StardewValley.GameLocation",
                            AlwaysActive = false
                        },
                        CanPlantHere = true,
                        ExcludeFromNpcPathfinding = true,
                        ArtifactSpots = null,
                        FishAreas = new Dictionary<string, FishAreaData>(),
                        Fish = null,
                        Forage = null,
                        MinDailyWeeds = 0,
                        MaxDailyWeeds = 0,
                        FirstDayWeedMultiplier = 0,
                        MinDailyForageSpawn = 0,
                        MaxDailyForageSpawn = 0,
                        MaxSpawnedForageAtOnce = 0,
                        ChanceForClay = 0,
                        Music = null,
                        MusicDefault = !string.IsNullOrEmpty(Config.BackgroundMusic) ? Config.BackgroundMusic : "",
                        MusicContext = MusicContext.SubLocation,
                        MusicIgnoredInRain = false,
                        MusicIgnoredInSpring = false,
                        MusicIgnoredInSummer = false,
                        MusicIgnoredInFall = false,
                        MusicIgnoredInWinter = false,
                        MusicIgnoredInFallDebris = false,
                        MusicIsTownTheme = false
                    });
                });
            }
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady)
                return;
            if (Game1.IsMasterGame)
            {
                CheckForChunkLoading();
                CheckForChunkChange();
            }

            if (Config.Debug && !Game1.isWarping && Game1.player.currentLocation.Name.Equals("Backwoods"))
            {
                Game1.warpFarmer(locName, Config.OpenWorldSize / 2, Config.OpenWorldSize / 2, 0);
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {   
            // Get Advanced Loot Framework's API
            advancedLootFrameworkApi = Helper.ModRegistry.GetApi<IAdvancedLootFrameworkApi>("aedenthorn.AdvancedLootFramework");
            if (advancedLootFrameworkApi is not null)
            {
                Monitor.Log($"Loaded AdvancedLootFramework API");
                UpdateTreasuresList();
                Monitor.Log($"Got {treasuresList.Count} possible treasures");
            }
            
            // Get SDG's API
            gamesAPI = Helper.ModRegistry.GetApi<IStardewGamesAPI>("aedenthorn.StardewGames");
            if (advancedLootFrameworkApi is not null)
            {
                Monitor.Log($"Loaded StardewGames API");
                gamesAPI.AddGame(context.ModManifest.UniqueID, LoadGame, DrawMenuSlot);
            }

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
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
                var props = typeof(ModConfig).GetProperties().ToArray();
                Array.Sort(props, (PropertyInfo a, PropertyInfo b) =>
                {
                    return a.Name.CompareTo(b.Name);
                });
                foreach (var p in props)
                {
                    if (p.Name == nameof(Config.ModEnabled))
                        continue;
                    if(p.PropertyType == typeof(bool))
                    {
                        configMenu.AddBoolOption(
                            mod: ModManifest,
                            name: () => p.Name,
                            getValue: () => (bool)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if(p.PropertyType == typeof(int))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => p.Name,
                            getValue: () => (int)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if(p.PropertyType == typeof(float))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => p.Name,
                            getValue: () => p.GetValue(Config).ToString(),
                            setValue: value => { if (float.TryParse(value, out var f)){ p.SetValue(Config, f); } }
                        );
                    }
                    else if(p.PropertyType == typeof(double))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => p.Name,
                            getValue: () => p.GetValue(Config).ToString(),
                            setValue: value => { if (double.TryParse(value, out var f)){ p.SetValue(Config, f); } }
                        );
                    }
                    else if(p.PropertyType == typeof(string))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => p.Name,
                            getValue: () => (string)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                }
            }
        }
    }
}