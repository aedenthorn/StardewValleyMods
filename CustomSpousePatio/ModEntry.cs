using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile.Dimensions;
using xTile.Tiles;

namespace CustomSpousePatio
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        public static ModConfig Config;
        public static Dictionary<string, object> outdoorAreas = new Dictionary<string, object>();
        public static IMonitor SMonitor;
        public static IModHelper SHelper;

        public static Dictionary<string, Point> spousePatioOffsets = new Dictionary<string, Point>()
        {
            {"Sam", new Point(2,2)},
            {"Penny", new Point(2,2)},
            {"Sebastian", new Point(2,3)},
            {"Shane", new Point(0,3)},
            {"Alex", new Point(2,2)},
            {"Maru", new Point(1,2)},
            {"Emily", new Point(1,3)},
            {"Haley", new Point(1,2)},
            {"Harvey", new Point(2,2)},
            {"Elliott", new Point(2,2)},
            {"Leah", new Point(2,2)},
            {"Abigail", new Point(2,2)},

        };
        private static Dictionary<string, TileSheetInfo> tileSheetsToAdd = new Dictionary<string, TileSheetInfo>();

        public static Point DefaultSpouseAreaLocation { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;
            SMonitor = Monitor;
            SHelper = Helper;

            //Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;

            var harmony = new  Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.setUpForOutdoorPatioActivity)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_setUpForOutdoorPatioActivity_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), "doPlaySpousePatioAnimation"),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_doPlaySpousePatioAnimation_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farm), "addSpouseOutdoorArea"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farm_addSpouseOutdoorArea_Prefix))
            );

        }

        public override object GetApi()
        {
            return new CustomSpousePatioApi();
        }
        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            RemoveAllSpouseAreas();
            SMonitor.Log($"Clearing spouse areas on return to title.");
            outdoorAreas.Clear();
        }
        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            LoadSpouseAreaData();
            if (!Context.IsMainPlayer)
            {
                SMonitor.Log($"Not the host player, this copy of the mod will not do anything.", LogLevel.Warn);
                return;
            }
            DefaultSpouseAreaLocation = Utility.Vector2ToPoint(Game1.getFarm().GetSpouseOutdoorAreaCorner());
        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;
            if (!Game1.isRaining && !Game1.IsWinter && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat"))
            {
                Farmer farmer = Game1.player;
                //Game1.getFarm().addSpouseOutdoorArea(Game1.player.spouse == null ? "" : Game1.player.spouse);
                var spouses = farmer.friendshipData.Pairs.Where(f => f.Value.IsMarried()).Select(f => f.Key).ToList();
                NPC ospouse = farmer.getSpouse();
                if (ospouse != null)
                {
                    spouses.Add(ospouse.Name);
                }
                foreach (string name in spouses)
                {
                    NPC npc = Game1.getCharacterFromName(name);

                    if (outdoorAreas.ContainsKey(name) || (farmer.spouse.Equals(npc.Name) && name != "Krobus"))
                    {
                        SMonitor.Log($"placing {name} outdoors");
                        npc.setUpForOutdoorPatioActivity();
                    }
                }
            }
            SetupSpouseAreas();
        }

        public static void LoadSpouseAreaData()
        {
            SMonitor.Log($"loading outdoor patios, clearing outdoorAreas {outdoorAreas.Count}");
            outdoorAreas.Clear();

            string path = Path.Combine("assets", "outdoor-areas.json");

            if(!File.Exists(Path.Combine(SHelper.DirectoryPath, path)))
                path = "outdoor-areas.json";

            try
            {
                OutdoorAreaData json = SHelper.Data.ReadJsonFile<OutdoorAreaData>(path) ?? null;

                if (json != null)
                {
                    if (json.areas != null && json.areas.Count > 0)
                    {
                        foreach (KeyValuePair<string, OutdoorArea> area in json.areas)
                        {
                            if (area.Key == "default")
                            {
                                if (Game1.MasterPlayer.spouse != null)
                                {
                                    outdoorAreas[Game1.MasterPlayer.spouse] = area.Value;
                                    SMonitor.Log($"Added outdoor area at {area.Value.location} or {area.Value.GetLocation()} for {area.Key} ({Game1.MasterPlayer.spouse})", LogLevel.Debug);

                                }
                            }
                            else
                            {
                                outdoorAreas[area.Key] = area.Value;
                                SMonitor.Log($"Added outdoor area at {area.Value.location} or {area.Value.GetLocation()} for {area.Key}", LogLevel.Debug);
                            }
                        }
                    }
                    if (json.tileSheetsToAdd != null)
                    {
                        foreach (KeyValuePair<string, TileSheetInfo> kvp in json.tileSheetsToAdd)
                        {
                            string name = "zzz_custom_spouse_patio_" + kvp.Key;
                            if (tileSheetsToAdd.ContainsKey(name))
                            {
                                SMonitor.Log($"Duplicate tilesheet {name} in list of tilesheets to add", LogLevel.Warn);
                                continue;
                            }
                            tileSheetsToAdd.Add(name, kvp.Value);
                            tileSheetsToAdd[name].realPath = SHelper.Content.GetActualAssetKey(FixTileSheetPath(kvp.Value.path));
                            SMonitor.Log($"Added tilesheet {name} to list of tilesheets to add", LogLevel.Debug);
                        }
                    }
                }
                else
                {
                    SMonitor.Log($"Couldn't get spouse areas from {path}", LogLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Error reading {path}:\r\n {ex}", LogLevel.Error);
            }

            foreach (IContentPack contentPack in SHelper.ContentPacks.GetOwned())
            {
                SMonitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}", LogLevel.Debug);
                try
                {
                    OutdoorAreaData json = contentPack.ReadJsonFile<OutdoorAreaData>("content.json") ?? null;

                    if (json != null)
                    {
                        if (json.areas != null && json.areas.Count > 0)
                        {
                            foreach (KeyValuePair<string,OutdoorArea> area in json.areas)
                            {
                                if (area.Key == "default")
                                {
                                    if (Game1.MasterPlayer.spouse != null)
                                    {
                                        outdoorAreas[Game1.MasterPlayer.spouse] = area.Value;
                                        SMonitor.Log($"Added outdoor area at {area.Value.location} or {area.Value.GetLocation()} for {area.Key} ({Game1.MasterPlayer.spouse})", LogLevel.Debug);

                                    }
                                }
                                else if(!outdoorAreas.ContainsKey(area.Key))
                                {
                                    outdoorAreas[area.Key] = area.Value;
                                    SMonitor.Log($"Added outdoor area at {area.Value.location} or {area.Value.GetLocation()} for {area.Key}", LogLevel.Debug);
                                }
                                else
                                {
                                    SMonitor.Log($"Outdoor area already exists for {area.Key}, not adding duplicate from content pack", LogLevel.Debug);
                                }
                            }
                        }
                        if(json.tileSheetsToAdd != null)
                        {
                            foreach(KeyValuePair<string,TileSheetInfo> kvp in json.tileSheetsToAdd)
                            {
                                string name = "zzz_custom_spouse_patio_" + kvp.Key;
                                if (tileSheetsToAdd.ContainsKey(name))
                                {
                                    SMonitor.Log($"Duplicate tilesheet {name} in list of tilesheets to add", LogLevel.Warn);
                                    continue;
                                }
                                tileSheetsToAdd.Add(name, kvp.Value);
                                tileSheetsToAdd[name].realPath = contentPack.GetActualAssetKey(FixTileSheetPath(kvp.Value.path));
                                SMonitor.Log($"Added tilesheet {name} to list of tilesheets to add", LogLevel.Debug);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    SMonitor.Log($"error reading content.json file in content pack {contentPack.Manifest.Name}.\r\n{ex}", LogLevel.Error);
                }
            }
            SMonitor.Log($"Total outdoor spouse areas: {outdoorAreas.Count}", LogLevel.Debug);
        }

        private static string FixTileSheetPath(string path)
        {
            return path.Replace("{season}", Game1.Date.Season.ToLower());
        }

        public static void SetupSpouseAreas()
        {
            SMonitor.Log($"SetupSpouseAreas Total outdoor spouse areas: {outdoorAreas.Count}");

            AddTileSheets();
            RemoveAllSpouseAreas();
            ShowSpouseAreas();
        }

        public static void AddTileSheets()
        {
            SMonitor.Log($"AddTileSheets Total outdoor spouse areas: {outdoorAreas.Count}");

            Farm farm = Game1.getFarm();
            if (farm.map.TileSheets.FirstOrDefault(s => s.Id == "zzz_custom_spouse_default_patio") == null)
            {
                string season = Game1.Date.Season.ToLower();
                Texture2D tex = SHelper.Content.Load<Texture2D>($"Maps/{season}_outdoorsTileSheet", ContentSource.GameContent);
                farm.map.AddTileSheet(new TileSheet("zzz_custom_spouse_default_patio", farm.map, SHelper.Content.GetActualAssetKey($"Maps/{season}_outdoorsTileSheet", ContentSource.GameContent), new Size(tex.Width / 16, tex.Height / 16), new Size(16, 16)));
                farm.map.LoadTileSheets(Game1.mapDisplayDevice);
                SMonitor.Log($"Added default tilesheet zzz_custom_spouse_default_patio to farm map", LogLevel.Debug);
            }

            foreach (KeyValuePair<string, TileSheetInfo> kvp in tileSheetsToAdd)
            {
                if (farm.map.TileSheets.FirstOrDefault(s => s.Id == kvp.Key) == null)
                {
                    farm.map.AddTileSheet(new TileSheet(kvp.Key, farm.map, kvp.Value.realPath, new Size(kvp.Value.width, kvp.Value.height), new Size(kvp.Value.tileWidth, kvp.Value.tileHeight)));
                    SMonitor.Log($"Added tilesheet {kvp.Key} to farm map", LogLevel.Debug);
                }
            }
        }

        public static void RemoveAllSpouseAreas()
        {
            SMonitor.Log($"Removing all spouse areas {outdoorAreas.Count}");

            Farm farm = Game1.getFarm();

            Point patio_corner = Utility.Vector2ToPoint(farm.GetSpouseOutdoorAreaCorner());
            string above_always_layer = "AlwaysFront";
            farm.removeTile(patio_corner.X + 1, patio_corner.Y + 3, "Buildings");
            farm.removeTile(patio_corner.X + 2, patio_corner.Y + 3, "Buildings");
            farm.removeTile(patio_corner.X + 3, patio_corner.Y + 3, "Buildings");
            farm.removeTile(patio_corner.X, patio_corner.Y + 3, "Buildings");
            farm.removeTile(patio_corner.X + 1, patio_corner.Y + 2, "Buildings");
            farm.removeTile(patio_corner.X + 2, patio_corner.Y + 2, "Buildings");
            farm.removeTile(patio_corner.X + 3, patio_corner.Y + 2, "Buildings");
            farm.removeTile(patio_corner.X, patio_corner.Y + 2, "Buildings");
            farm.removeTile(patio_corner.X + 1, patio_corner.Y + 1, "Front");
            farm.removeTile(patio_corner.X + 2, patio_corner.Y + 1, "Front");
            farm.removeTile(patio_corner.X + 3, patio_corner.Y + 1, "Front");
            farm.removeTile(patio_corner.X, patio_corner.Y + 1, "Front");
            farm.removeTile(patio_corner.X + 1, patio_corner.Y, above_always_layer);
            farm.removeTile(patio_corner.X + 2, patio_corner.Y, above_always_layer);
            farm.removeTile(patio_corner.X + 3, patio_corner.Y, above_always_layer);
            farm.removeTile(patio_corner.X, patio_corner.Y, above_always_layer);


            foreach(KeyValuePair<string, object> kvp in outdoorAreas)
            {
                OutdoorArea area = kvp.Value as OutdoorArea;
                int x = area.GetLocation().X;
                int y = area.GetLocation().Y;

                if (farm.map.Layers[0].LayerWidth <= x + 3 || farm.map.Layers[0].LayerHeight <= y + 3)
                {
                    SMonitor.Log($"Invalid spouse area coordinates {x},{y} for {kvp.Key}", LogLevel.Error);
                    continue;
                }

                farm.removeTile(x + 1, y + 3, "Buildings");
                farm.removeTile(x + 2, y + 3, "Buildings");
                farm.removeTile(x + 3, y + 3, "Buildings");
                farm.removeTile(x, y + 3, "Buildings");
                farm.removeTile(x + 1, y + 2, "Buildings");
                farm.removeTile(x + 2, y + 2, "Buildings");
                farm.removeTile(x + 3, y + 2, "Buildings");
                farm.removeTile(x, y + 2, "Buildings");
                farm.removeTile(x + 1, y + 1, "Front");
                farm.removeTile(x + 2, y + 1, "Front");
                farm.removeTile(x + 3, y + 1, "Front");
                farm.removeTile(x, y + 1, "Front");
                farm.removeTile(x + 1, y, "AlwaysFront");
                farm.removeTile(x + 2, y, "AlwaysFront");
                farm.removeTile(x + 3, y, "AlwaysFront");
                farm.removeTile(x, y, "AlwaysFront");
            }

        }
        public static void ShowSpouseAreas() {
            SMonitor.Log($"ShowSpouseAreas Total outdoor spouse areas: {outdoorAreas.Count}");

            Farm farm = Game1.getFarm();

            Farmer f = Game1.MasterPlayer;


            List<string> customAreaSpouses = new List<string>();
            string aSpouse = null;
            foreach (KeyValuePair<string, Friendship> spouse in f.friendshipData.Pairs.Where(s => s.Value.IsMarried()))
            {
                if (outdoorAreas.ContainsKey(spouse.Key))
                    customAreaSpouses.Add(spouse.Key);
                aSpouse = spouse.Key;
            }

            if(customAreaSpouses.Count == 0)
            {
                SMonitor.Log($"no custom spouse areas", LogLevel.Debug);
                if (f.spouse != null)
                    aSpouse = f.spouse;
                if(aSpouse != null)
                {
                    customAreaSpouses.Add(aSpouse);
                    SMonitor.Log($"Adding spouse {aSpouse} for default area", LogLevel.Debug);
                }
            }

            foreach (string spouse in customAreaSpouses)
            {
                bool useDefaultTiles = true;
                OutdoorArea area = new OutdoorArea();
                List<SpecialTile> specialTiles = new List<SpecialTile>();
                int x = DefaultSpouseAreaLocation.X;
                int y = DefaultSpouseAreaLocation.Y;
                if (outdoorAreas.ContainsKey(spouse))
                {
                    area = outdoorAreas[spouse] as OutdoorArea;
                    x = area.GetLocation().X;
                    y = area.GetLocation().Y;
                    useDefaultTiles = area.useDefaultTiles;
                    specialTiles = new List<SpecialTile>(area.specialTiles);
                }

                SMonitor.Log($"Adding spouse area for {spouse}: use default tiles: {useDefaultTiles}, special tiles: {specialTiles.Count}");

                if (farm.map.Layers[0].LayerWidth <= x + 3 || farm.map.Layers[0].LayerHeight <= y + 3)
                {
                    SMonitor.Log($"Invalid spouse area coordinates {x},{y} for {spouse}", LogLevel.Error); 
                    continue;
                }

                /*
                farm.removeTile(x + 1, y + 3, "Buildings");
                farm.removeTile(x + 2, y + 3, "Buildings");
                farm.removeTile(x + 3, y + 3, "Buildings");
                farm.removeTile(x, y + 3, "Buildings");
                farm.removeTile(x + 1, y + 2, "Buildings");
                farm.removeTile(x + 2, y + 2, "Buildings");
                farm.removeTile(x + 3, y + 2, "Buildings");
                farm.removeTile(x, y + 2, "Buildings");
                farm.removeTile(x + 1, y + 1, "Front");
                farm.removeTile(x + 2, y + 1, "Front");
                farm.removeTile(x + 3, y + 1, "Front");
                farm.removeTile(x, y + 1, "Front");
                farm.removeTile(x + 1, y, "AlwaysFront");
                farm.removeTile(x + 2, y, "AlwaysFront");
                farm.removeTile(x + 3, y, "AlwaysFront");
                farm.removeTile(x, y, "AlwaysFront");
                */
                if (useDefaultTiles)
                {
                    TileSheet defaultTilesheet = farm.Map.GetTileSheet("zzz_custom_spouse_default_patio");
                    int sheetIdx = farm.Map.TileSheets.IndexOf(defaultTilesheet);
                    SMonitor.Log($"Default custom tilesheet index: {sheetIdx}");
                    if(sheetIdx == -1)
                    {
                        AddTileSheets();
                        sheetIdx = farm.Map.TileSheets.IndexOf(defaultTilesheet);
                    }

                    string which = area.useTilesOf != null ? area.useTilesOf : spouse;

                    switch (which)
                    {
                        case "Sam":
                            farm.setMapTileIndex(x, y + 2, 1173, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 3, y + 2, 1174, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 1, y + 2, 1198, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 2, y + 2, 1199, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x, y + 1, 1148, "Front", sheetIdx);
                            farm.setMapTileIndex(x + 3, y + 1, 1149, "Front", sheetIdx);
                            break;
                        case "Penny":
                            farm.setMapTileIndex(x, y + 2, 1098, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 1, y + 2, 1123, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 3, y + 2, 1098, "Buildings", sheetIdx);
                            break;
                        case "Sebastian":
                            farm.setMapTileIndex(x + 1, y + 2, 1927, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 2, y + 2, 1928, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 3, y + 2, 1929, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 1, y + 1, 1902, "Front", sheetIdx);
                            farm.setMapTileIndex(x + 2, y + 1, 1903, "Front", sheetIdx);
                            break;
                        case "Shane":
                            farm.setMapTileIndex(x + 1, y + 3, 1940, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 2, y + 3, 1941, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 3, y + 3, 1942, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 1, y + 2, 1915, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 2, y + 2, 1916, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 3, y + 2, 1917, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 1, y + 1, 1772, "Front", sheetIdx);
                            farm.setMapTileIndex(x + 2, y + 1, 1773, "Front", sheetIdx);
                            farm.setMapTileIndex(x + 3, y + 1, 1774, "Front", sheetIdx);
                            farm.setMapTileIndex(x + 1, y, 1747, "AlwaysFront", sheetIdx);
                            farm.setMapTileIndex(x + 2, y, 1748, "AlwaysFront", sheetIdx);
                            farm.setMapTileIndex(x + 3, y, 1749, "AlwaysFront", sheetIdx);
                            break;
                        case "Alex":
                            farm.setMapTileIndex(x, y + 2, 1099, "Buildings", sheetIdx);
                            break;
                        case "Maru":
                            farm.setMapTileIndex(x + 2, y + 2, 1124, "Buildings", sheetIdx);
                            break;
                        case "Emily":
                            farm.setMapTileIndex(x, y + 2, 1867, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 3, y + 2, 1867, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x, y + 1, 1842, "Front", sheetIdx);
                            farm.setMapTileIndex(x + 3, y + 1, 1842, "Front", sheetIdx);
                            farm.setMapTileIndex(x, y + 3, 1866, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 2, y + 2, 1866, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 3, y + 3, 1967, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 1, y + 2, 1967, "Buildings", sheetIdx);
                            break;
                        case "Haley":
                            farm.setMapTileIndex(x, y + 2, 1074, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x, y + 1, 1049, "Front", sheetIdx);
                            farm.setMapTileIndex(x, y, 1024, "AlwaysFront", sheetIdx);
                            farm.setMapTileIndex(x + 3, y + 2, 1074, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 3, y + 1, 1049, "Front", sheetIdx);
                            farm.setMapTileIndex(x + 3, y, 1024, "AlwaysFront", sheetIdx);
                            break;
                        case "Harvey":
                            farm.setMapTileIndex(x, y + 2, 1098, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 1, y + 2, 1123, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 3, y + 2, 1098, "Buildings", sheetIdx);
                            break;
                        case "Elliott":
                            farm.setMapTileIndex(x, y + 2, 1098, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 1, y + 2, 1123, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 3, y + 2, 1098, "Buildings", sheetIdx);
                            break;
                        case "Leah":
                            farm.setMapTileIndex(x + 1, y + 2, 1122, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 1, y + 1, 1097, "Front", sheetIdx);
                            break;
                        case "Abigail":
                            farm.setMapTileIndex(x, y + 2, 1098, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 1, y + 2, 1123, "Buildings", sheetIdx);
                            farm.setMapTileIndex(x + 3, y + 2, 1098, "Buildings", sheetIdx);
                            break;
                        default:
                            SMonitor.Log($"No default tiles for {spouse}", LogLevel.Warn);
                            break;

                    }
                }

                foreach (SpecialTile tile in area.specialTiles)
                {
                    TileSheet tilesheet = farm.Map.GetTileSheet("zzz_custom_spouse_patio_"+tile.tilesheet);
                    int idx = farm.Map.TileSheets.IndexOf(tilesheet);
                    SMonitor.Log($"Adding specialTile at {tile.location}, {tile.tilesheet}, tilesheet idx {idx}");
                    farm.setMapTileIndex(tile.location.X, tile.location.Y, tile.tileIndex, tile.layer, idx);
                }
            }
        }


        public static bool NPC_setUpForOutdoorPatioActivity_Prefix(NPC __instance) 
        {
            try
            {
                SMonitor.Log($"Checking {__instance.Name} for spouse patio. {outdoorAreas.Count} total areas");
                Point point;
                if (outdoorAreas.ContainsKey(__instance.Name))
                {
                    point = (outdoorAreas[__instance.Name] as OutdoorArea).NpcPos(__instance.Name);
                    SMonitor.Log($"{__instance.Name} location: {(outdoorAreas[__instance.Name] as OutdoorArea).location} offset: {(outdoorAreas[__instance.Name] as OutdoorArea).npcOffset}");
                }
                else if (Game1.player.spouse == __instance.Name && outdoorAreas.Count == 0)
                {
                    SMonitor.Log($"Placing main spouse {__instance.Name} outdoors");
                    point = DefaultSpouseAreaLocation;
                    if (spousePatioOffsets.ContainsKey(__instance.Name))
                    {
                        point = new Point(DefaultSpouseAreaLocation.X + spousePatioOffsets[__instance.Name].X, DefaultSpouseAreaLocation.Y + spousePatioOffsets[__instance.Name].Y);
                    }
                }
                else
                {
                    SMonitor.Log($"NPC not in outdoorAreas and not main spouse");
                    __instance.shouldPlaySpousePatioAnimation.Value = false;
                    return false;
                }
                Game1.warpCharacter(__instance, "Farm", point);
                __instance.popOffAnyNonEssentialItems();
                __instance.currentMarriageDialogue.Clear(); 
                __instance.addMarriageDialogue("MarriageDialogue", "patio_" + __instance.Name, false, new string[0]);
                __instance.shouldPlaySpousePatioAnimation.Value = true;
                SMonitor.Log($"placed spouse {__instance.Name} at {point}");
                return false;
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Failed in {nameof(NPC_setUpForOutdoorPatioActivity_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static void NPC_doPlaySpousePatioAnimation_Postfix(NPC __instance)
        {
            try
            {
                if (outdoorAreas.ContainsKey(__instance.Name) && (outdoorAreas[__instance.Name] as OutdoorArea).npcAnimation != null)
                {
                    SMonitor.Log($"got animation for {__instance.Name}");
                    NPCDoAnimation(__instance, (outdoorAreas[__instance.Name] as OutdoorArea).npcAnimation);
                }
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Failed in {nameof(NPC_doPlaySpousePatioAnimation_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static bool Farm_addSpouseOutdoorArea_Prefix(ref string spouseName)
        {
            try
            {
                if (!outdoorAreas.Any())
                    return true;
                SetupSpouseAreas();
                return false;
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Failed in {nameof(Farm_addSpouseOutdoorArea_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }


        private static void NPCDoAnimation(NPC npc, string npcAnimation)
        {
            Dictionary<string, string> animationDescriptions = SHelper.Content.Load<Dictionary<string, string>>("Data\\animationDescriptions", ContentSource.GameContent);
            if (!animationDescriptions.ContainsKey(npcAnimation))
                return;

            string[] rawData = animationDescriptions[npcAnimation].Split('/');
            var animFrames = Utility.parseStringToIntArray(rawData[1], ' ');

            List<FarmerSprite.AnimationFrame> anim = new List<FarmerSprite.AnimationFrame>();
            for (int i = 0; i < animFrames.Length; i++)
            {
                anim.Add(new FarmerSprite.AnimationFrame(animFrames[i], 100, 0, false, false, null, false, 0));
            }
            SMonitor.Log($"playing animation {npcAnimation} for {npc.Name}");
            npc.Sprite.setCurrentAnimation(anim);
        }
        public static bool IsSpousePatioDay(NPC npc)
        {
            return !Game1.isRaining && !Game1.IsWinter && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && npc.getSpouse() == Game1.MasterPlayer && !npc.Name.Equals("Krobus");
        }
        public static void PlaceSpouses()
        {
            foreach(KeyValuePair<string, Friendship> kvp in Game1.MasterPlayer.friendshipData.Pairs.Where(n => n.Value.IsMarried() && !n.Value.IsEngaged()))
            {
                NPC npc = Game1.getCharacterFromName(kvp.Key);
                if (IsSpousePatioDay(npc))
                    npc.setUpForOutdoorPatioActivity();
            }
        }
    }

}