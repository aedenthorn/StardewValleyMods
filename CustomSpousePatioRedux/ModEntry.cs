using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Layers;
using xTile.Tiles;

namespace CustomSpousePatioRedux
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {
        public static ModConfig Config;
        public static OutdoorAreaData outdoorAreas;
        public static IMonitor SMonitor;
        public static IModHelper SHelper;

        public static Dictionary<string, Dictionary<string, Dictionary<Point, Tile>>> baseSpouseAreaTiles = new Dictionary<string, Dictionary<string, Dictionary<Point, Tile>>>();
        public static Dictionary<string, Point> spousePositions = new Dictionary<string, Point>();
        private static List<string> noCustomAreaSpouses;
        public static Vector2 DefaultSpouseAreaLocation { get; set; }
        public static readonly string saveKey = "custom-spouse-patio-data";
        public static Point cursorLoc;

        public static int currentPage;

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
            Helper.Events.GameLoop.Saving += GameLoop_Saving;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new  Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.GetSpousePatioPosition)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_GetSpousePatioPosition_Prefix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(Farm), nameof(Farm.addSpouseOutdoorArea)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farm_addSpouseOutdoorArea_Transpiler)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farm_addSpouseOutdoorArea_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Farm), nameof(Farm.CacheOffBasePatioArea)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farm_CacheOffBasePatioArea_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Farm), nameof(Farm.ReapplyBasePatioArea)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farm_ReapplyBasePatioArea_Prefix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.setUpForOutdoorPatioActivity)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_setUpForOutdoorPatioActivity_Prefix))
            );

        }

        public static void LoadSpouseAreaData()
        {
            if (!Config.EnableMod)
                return; 
            if (!Context.IsMainPlayer)
            {
                SMonitor.Log($"Not the host player, this copy of the mod will not do anything.", LogLevel.Warn);
                return;
            }
            outdoorAreas = SHelper.Data.ReadSaveData<OutdoorAreaData>(saveKey) ?? new OutdoorAreaData();
            if(outdoorAreas.areas != null)
            {
                foreach(var area in outdoorAreas.areas)
                {
                    outdoorAreas.dict.Add(area.Key, new OutdoorArea() { location = Game1.getFarm().Name, corner = area.Value});
                }
            }
            foreach (var area in outdoorAreas.dict)
            {
                CacheOffBasePatioArea(area.Key);
            }
            SMonitor.Log($"Total outdoor spouse areas: {outdoorAreas.dict.Count}", LogLevel.Debug);
        }
        private static Vector2 GetSpouseOutdoorAreaCorner(string spouseName)
        {
            if (!Config.EnableMod || outdoorAreas == null || outdoorAreas.dict.Count == 0)
                return Game1.getFarm().GetSpouseOutdoorAreaCorner();

            return outdoorAreas.dict.TryGetValue(spouseName, out OutdoorArea value) ? value.corner : Game1.getFarm().GetSpouseOutdoorAreaCorner();
        }
        
        private static void ApplyMapOverride(string spouseName, string map_name, string override_key_name, Rectangle? source_rect = null, Rectangle? destination_rect = null)
        {
            if (Config.EnableMod && outdoorAreas == null)
                return;
            GameLocation l = Game1.getLocationFromName(Config.EnableMod && outdoorAreas.dict.TryGetValue(spouseName, out OutdoorArea area) ? area.location : "Farm");
            if (l == null)
                l = Game1.getFarm();
            SMonitor.Log($"Applying patio map override for {spouseName} in {l.Name}");
            if (AccessTools.FieldRefAccess<GameLocation, HashSet<string>>(l, "_appliedMapOverrides").Contains("spouse_patio"))
            {
                AccessTools.FieldRefAccess<GameLocation, HashSet<string>>(l, "_appliedMapOverrides").Remove("spouse_patio");
            }
            if (AccessTools.FieldRefAccess<GameLocation, HashSet<string>>(l, "_appliedMapOverrides").Contains(spouseName+"_spouse_patio"))
            {
                AccessTools.FieldRefAccess<GameLocation, HashSet<string>>(l, "_appliedMapOverrides").Remove(spouseName + "_spouse_patio");
            }
            l.ApplyMapOverride(map_name, spouseName + "_spouse_patio", source_rect, destination_rect);
        }


        public void StartWizard()
        {
            currentPage = 0;
            if (outdoorAreas == null)
            {
                Monitor.Log("Outdoor ares is null.", LogLevel.Warn);
                return;
            }

            cursorLoc = Utility.Vector2ToPoint(Game1.GetPlacementGrabTile());
            var pairs = Game1.player.friendshipData.Pairs.Where(s => s.Value.IsMarried());
            if (!pairs.Any())
            {
                Monitor.Log("You don't have any spouses.", LogLevel.Warn);
                return;
            }

            noCustomAreaSpouses = new List<string>();
            foreach (KeyValuePair<string, Friendship> spouse in pairs)
            {
                if(!outdoorAreas.dict.ContainsKey(spouse.Key))
                    noCustomAreaSpouses.Add(spouse.Key);
            }

            List<Response> responses = new List<Response>();
            if (noCustomAreaSpouses.Any())
                responses.Add(new Response("CSP_Wizard_Questions_AddPatio", string.Format(Helper.Translation.Get("new-patio"), cursorLoc.X, cursorLoc.Y)));
            if (outdoorAreas.dict.Any())
            {
                responses.Add(new Response("CSP_Wizard_Questions_RemovePatio", Helper.Translation.Get("remove-patio")));
                responses.Add(new Response("CSP_Wizard_Questions_MovePatio", Helper.Translation.Get("move-patio")));
                responses.Add(new Response("CSP_Wizard_Questions_ListPatios", Helper.Translation.Get("list-patios")));
            }
            responses.Add(new Response("CSP_Wizard_Questions_ReloadPatios", Helper.Translation.Get("reload-patios")));
            responses.Add(new Response("cancel", Helper.Translation.Get("cancel")));
            Game1.player.currentLocation.createQuestionDialogue(Helper.Translation.Get("welcome"), responses.ToArray(), "CSP_Wizard_Questions");
        }


        public void CSPWizardDialogue(string whichQuestion, string whichAnswer)
        {
            Monitor.Log($"question: {whichQuestion}, answer: {whichAnswer}");
            if (whichAnswer == "cancel")
                return;

            List<Response> responses = new List<Response>();
            string header = "";
            string newQuestion = whichAnswer;
            switch (whichQuestion)
            {
                case "CSP_Wizard_Questions":
                    switch (whichAnswer)
                    {
                        case "CSP_Wizard_Questions_AddPatio":
                            if (cursorLoc.X > Game1.player.currentLocation.map.Layers[0].LayerWidth - 4 || cursorLoc.Y > Game1.player.currentLocation.map.Layers[0].LayerWidth - 4)
                            {
                                Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("cursor-out-of-bounds"), cursorLoc.X, cursorLoc.Y));
                                return;
                            }
                            header = Helper.Translation.Get("new-patio-which");
                            if (currentPage > 0)
                                responses.Add(new Response("last", "..."));
                            foreach (string spouse in noCustomAreaSpouses.Skip(currentPage * Config.MaxSpousesPerPage).Take(Config.MaxSpousesPerPage))
                            {
                                responses.Add(new Response(spouse, spouse));
                            }
                            if (noCustomAreaSpouses.Count > (currentPage + 1) * Config.MaxSpousesPerPage)
                                responses.Add(new Response("next", "..."));
                            break;
                        case "CSP_Wizard_Questions_MovePatio":
                            header = Helper.Translation.Get("move-patio-which");
                            if (currentPage > 0)
                                responses.Add(new Response("last", "..."));
                            foreach (string spouse in outdoorAreas.dict.Keys.Skip(currentPage * Config.MaxSpousesPerPage).Take(Config.MaxSpousesPerPage))
                            {
                                responses.Add(new Response(spouse, spouse));
                            }
                            if(outdoorAreas.dict.Keys.Count > (currentPage + 1) * Config.MaxSpousesPerPage)
                                responses.Add(new Response("next", "..."));
                            break;
                        case "CSP_Wizard_Questions_RemovePatio":
                            header = Helper.Translation.Get("remove-patio-which");
                            if (currentPage > 0)
                                responses.Add(new Response("last", "..."));
                            foreach (string spouse in outdoorAreas.dict.Keys.Skip(currentPage * Config.MaxSpousesPerPage).Take(Config.MaxSpousesPerPage))
                            {
                                responses.Add(new Response(spouse, spouse));
                            }
                            if (outdoorAreas.dict.Keys.Count > (currentPage + 1) * Config.MaxSpousesPerPage)
                                responses.Add(new Response("next", "..."));
                            break;
                        case "CSP_Wizard_Questions_ListPatios":
                            Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("patios-exist-for"), string.Join(", ", outdoorAreas.dict.Keys)));
                            return;
                        case "CSP_Wizard_Questions_ReloadPatios":
                            outdoorAreas = Helper.Data.ReadSaveData<OutdoorAreaData>(saveKey);
                            Game1.getFarm().UpdatePatio();
                            Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("reloaded-patios"), outdoorAreas.dict.Count));
                            return;
                    }
                    break;
                case "CSP_Wizard_Questions_AddPatio":
                    if(whichAnswer == "next")
                    {
                        currentPage++;
                        CSPWizardDialogue("CSP_Wizard_Questions", "CSP_Wizard_Questions_AddPatio");
                        return;
                    }
                    if(whichAnswer == "last")
                    {
                        currentPage--;
                        CSPWizardDialogue("CSP_Wizard_Questions", "CSP_Wizard_Questions_AddPatio");
                        return;
                    }
                    if(cursorLoc.X > Game1.player.currentLocation.map.Layers[0].LayerWidth - 4 || cursorLoc.Y > Game1.player.currentLocation.map.Layers[0].LayerWidth - 4)
                    {
                        Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("cursor-out-of-bounds"), cursorLoc.X, cursorLoc.Y));
                        return;
                    }
                    ReapplyBasePatioArea();
                    if (AccessTools.FieldRefAccess<GameLocation, HashSet<string>>(Game1.getFarm(), "_appliedMapOverrides").Contains("spouse_patio"))
                    {
                        AccessTools.FieldRefAccess<GameLocation, HashSet<string>>(Game1.getFarm(), "_appliedMapOverrides").Remove("spouse_patio");
                    }
                    if (AccessTools.FieldRefAccess<GameLocation, HashSet<string>>(Game1.getFarm(), "_appliedMapOverrides").Contains(whichAnswer + "_spouse_patio"))
                    {
                        AccessTools.FieldRefAccess<GameLocation, HashSet<string>>(Game1.getFarm(), "_appliedMapOverrides").Remove(whichAnswer + "_spouse_patio");
                    }
                    outdoorAreas.dict[whichAnswer] = new OutdoorArea() { location = Game1.player.currentLocation.Name, corner = cursorLoc.ToVector2() };
                    CacheOffBasePatioArea(whichAnswer);
                    Game1.getFarm().UpdatePatio();
                    if (Game1.getCharacterFromName(whichAnswer)?.shouldPlaySpousePatioAnimation.Value == true)
                    {
                        Game1.getCharacterFromName(whichAnswer).setUpForOutdoorPatioActivity();
                    }

                    Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("created-patio"), cursorLoc.X, cursorLoc.Y));
                    return;
                case "CSP_Wizard_Questions_MovePatio":
                    if (whichAnswer == "next")
                    {
                        currentPage++;
                        CSPWizardDialogue("CSP_Wizard_Questions", "CSP_Wizard_Questions_MovePatio");
                        return;
                    }
                    if (whichAnswer == "last")
                    {
                        currentPage--;
                        CSPWizardDialogue("CSP_Wizard_Questions", "CSP_Wizard_Questions_MovePatio");
                        return;
                    }
                    header = Helper.Translation.Get("move-patio-which-way");
                    newQuestion = "CSP_Wizard_Questions_MovePatio_2";
                    responses.Add(new Response($"{whichAnswer}_cursorLoc", string.Format(Helper.Translation.Get("cursor-location"), cursorLoc.X, cursorLoc.Y)));
                    responses.Add(new Response($"{whichAnswer}_up", Helper.Translation.Get("up")));
                    responses.Add(new Response($"{whichAnswer}_down", Helper.Translation.Get("down")));
                    responses.Add(new Response($"{whichAnswer}_left", Helper.Translation.Get("left")));
                    responses.Add(new Response($"{whichAnswer}_right", Helper.Translation.Get("right")));
                    break;
                case "CSP_Wizard_Questions_MovePatio_2":
                    if (MoveSpousePatio(whichAnswer, cursorLoc))
                        Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("moved-patio"), whichAnswer.Split('_')[0]));
                    else
                        Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("not-moved-patio"), whichAnswer.Split('_')[0]));
                    return;
                case "CSP_Wizard_Questions_RemovePatio":
                    if (whichAnswer == "next")
                    {
                        currentPage++;
                        CSPWizardDialogue("CSP_Wizard_Questions", "CSP_Wizard_Questions_RemovePatio");
                        return;
                    }
                    if (whichAnswer == "last")
                    {
                        currentPage--;
                        CSPWizardDialogue("CSP_Wizard_Questions", "CSP_Wizard_Questions_RemovePatio");
                        return;
                    }
                    if (outdoorAreas.dict.ContainsKey(whichAnswer))
                    {
                        ReapplyBasePatioArea(whichAnswer);
                        outdoorAreas.dict.Remove(whichAnswer);
                        baseSpouseAreaTiles.Remove(whichAnswer);
                        Game1.getFarm().UpdatePatio();
                        Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("removed-patio"), whichAnswer));
                    }
                    else
                        Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("not-removed-patio"), whichAnswer));
                    return;
                default:
                    return;
            }
            responses.Add(new Response("cancel", Helper.Translation.Get("cancel")));
            Game1.player.currentLocation.createQuestionDialogue($"{header}", responses.ToArray(), newQuestion);
        }

        public bool MoveSpousePatio(string spouse_dir, Point cursorLoc)
        {
            string spouse = spouse_dir.Split('_')[0];
            string dir = spouse_dir.Split('_')[1];
            Vector2 outdoorArea = outdoorAreas.dict[spouse].corner;
            string location = outdoorAreas.dict[spouse].location;
            switch (dir)
            {
                case "cursorLoc":
                    outdoorArea = cursorLoc.ToVector2();
                    location = Game1.player.currentLocation.Name;
                    break;
                case "up":
                    outdoorArea.Y--;
                    break;
                case "down":
                    outdoorArea.Y++;
                    break;
                case "left":
                    outdoorArea.X--;
                    break;
                case "right":
                    outdoorArea.X++;
                    break;
            }

            if (outdoorArea.X < 0 || outdoorArea.Y < 0 || outdoorArea.Y >= Game1.getFarm().map.Layers[0].LayerHeight - 4 || outdoorArea.X >= Game1.getFarm().map.Layers[0].LayerWidth - 4)
                return false;

            ReapplyBasePatioArea(spouse);
            outdoorAreas.dict[spouse].corner = outdoorArea;
            outdoorAreas.dict[spouse].location = location;
            SMonitor.Log($"Moved spouse patio for {spouse} to {outdoorArea}");
            CacheOffBasePatioArea(spouse);
            Game1.getFarm().UpdatePatio();
            if(Game1.getCharacterFromName(spouse)?.shouldPlaySpousePatioAnimation.Value == true)
            {
                Game1.getCharacterFromName(spouse).setUpForOutdoorPatioActivity();
            }
            return true;
        }

        private static void ReapplyBasePatioArea(string spouse = "default")
        {
            if (!baseSpouseAreaTiles.ContainsKey(spouse))
            {
                SMonitor.Log($"No cached tiles to reapply for {spouse}", LogLevel.Error);
                return;
            }
            GameLocation l = null;
            if (outdoorAreas != null && outdoorAreas.dict.TryGetValue(spouse, out OutdoorArea area))
            {
                l = Game1.getLocationFromName(area.location);
            }
            else
            {
                l = Game1.getFarm();
            }
            if (l == null)
                l = Game1.getFarm();

            SMonitor.Log($"Reapplying base patio area for {spouse} in {l.Name}");

            foreach (string layer in baseSpouseAreaTiles[spouse].Keys)
            {
                Layer map_layer = l.map.GetLayer(layer);
                foreach (Point location in baseSpouseAreaTiles[spouse][layer].Keys)
                {
                    Tile base_tile = baseSpouseAreaTiles[spouse][layer][location];
                    if (map_layer != null)
                    {
                        try
                        {
                            map_layer.Tiles[location.X, location.Y] = base_tile;
                        }
                        catch(Exception ex)
                        {
                            SMonitor.Log($"Error adding tile {spouse} in {l.Name}: {ex}");
                        }
                    }
                }
            }
        }

        private static void CacheOffBasePatioArea(string spouse)
        {
            if (!outdoorAreas.dict.TryGetValue(spouse, out OutdoorArea area))
                return;
            GameLocation l = Game1.getLocationFromName(area.location);
            if (l == null)
                l = Game1.getFarm();
            if (l == null)
                return;
            CacheOffBasePatioArea(spouse, l, area.corner);
        }
        private static void CacheOffBasePatioArea(string spouse, GameLocation l, Vector2 corner)
        {
            SMonitor.Log($"Caching base patio area for {spouse} in {l.Name} at {corner}");
            baseSpouseAreaTiles[spouse] = new Dictionary<string, Dictionary<Point, Tile>>();

            List<string> layers_to_cache = new List<string>();
            foreach (Layer layer in l.map.Layers)
            {
                layers_to_cache.Add(layer.Id);
            }
            foreach (string layer_name in layers_to_cache)
            {
                Layer original_layer = l.map.GetLayer(layer_name);
                Dictionary<Point, Tile> tiles = new Dictionary<Point, Tile>();
                baseSpouseAreaTiles[spouse][layer_name] = tiles;
                Vector2 spouse_area_corner = corner;
                for (int x = (int)spouse_area_corner.X; x < (int)spouse_area_corner.X + 4; x++)
                {
                    for (int y = (int)spouse_area_corner.Y; y < (int)spouse_area_corner.Y + 4; y++)
                    {
                        if (original_layer == null)
                        {
                            tiles[new Point(x, y)] = null;
                        }
                        else
                        {
                            tiles[new Point(x, y)] = original_layer.Tiles[x, y];
                        }
                    }
                }
            }
        }

        public static string GetSpousePatioName(string spouseName)
        {
            return spouseName + "_spouse_patio";
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