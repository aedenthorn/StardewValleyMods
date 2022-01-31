using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Tiles;

namespace CustomSpousePatioRedux
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {
        public static ModConfig Config;
        public static OutdoorAreaData outdoorAreas = new OutdoorAreaData();
        public static IMonitor SMonitor;
        public static IModHelper SHelper;

        public static Dictionary<string, Dictionary<string, Dictionary<Point, Tile>>> baseSpouseAreaTiles = new Dictionary<string, Dictionary<string, Dictionary<Point, Tile>>>();
        public static Dictionary<string, Point> spousePositions = new Dictionary<string, Point>();
        private static List<string> noCustomAreaSpouses;
        public static Vector2 DefaultSpouseAreaLocation { get; set; }
        public static readonly string saveKey = "custom-spouse-patio-data";
        public static Point cursorLoc;

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

        }

        public static void LoadSpouseAreaData()
        {
            if (!Config.EnableMod)
                return;
            outdoorAreas = SHelper.Data.ReadSaveData<OutdoorAreaData>(saveKey) ?? new OutdoorAreaData();

            SMonitor.Log($"Total outdoor spouse areas: {outdoorAreas.areas.Count}", LogLevel.Debug);
        }
        private static Vector2 GetSpouseOutdoorAreaCorner(string spouseName)
        {
            if (!Config.EnableMod || outdoorAreas == null || outdoorAreas.areas.Count == 0)
                return Game1.getFarm().GetSpouseOutdoorAreaCorner();

            return outdoorAreas.areas.TryGetValue(spouseName, out Vector2 value) ? value : Game1.getFarm().GetSpouseOutdoorAreaCorner();
        }


        public void StartWizard()
        {
            cursorLoc = Utility.Vector2ToPoint(Game1.GetPlacementGrabTile());
            var pairs = Game1.player.friendshipData.Pairs.Where(s => s.Value.IsMarried());
            if (!pairs.Any())
            {
                Monitor.Log("You don't have any spouses.", LogLevel.Warn);
            }

            noCustomAreaSpouses = new List<string>();
            foreach (KeyValuePair<string, Friendship> spouse in pairs)
            {
                if(!outdoorAreas.areas.ContainsKey(spouse.Key))
                    noCustomAreaSpouses.Add(spouse.Key);
            }

            List<Response> responses = new List<Response>();
            if (noCustomAreaSpouses.Any())
                responses.Add(new Response("CSP_Wizard_Questions_AddPatio", string.Format(Helper.Translation.Get("new-patio"), cursorLoc.X, cursorLoc.Y)));
            if (outdoorAreas.areas.Any())
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
                            header = Helper.Translation.Get("new-patio-which");
                            foreach (string spouse in noCustomAreaSpouses)
                            {
                                responses.Add(new Response(spouse, spouse));
                            }
                            break;
                        case "CSP_Wizard_Questions_MovePatio":
                            header = Helper.Translation.Get("move-patio-which");
                            foreach (string spouse in outdoorAreas.areas.Keys)
                            {
                                responses.Add(new Response(spouse, spouse));
                            }
                            break;
                        case "CSP_Wizard_Questions_RemovePatio":
                            header = Helper.Translation.Get("remove-patio-which");
                            foreach (string spouse in outdoorAreas.areas.Keys)
                            {
                                responses.Add(new Response(spouse, spouse));
                            }
                            break;
                        case "CSP_Wizard_Questions_ListPatios":
                            Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("patios-exist-for"), string.Join(", ", outdoorAreas.areas.Keys)));
                            return;
                        case "CSP_Wizard_Questions_ReloadPatios":
                            outdoorAreas = Helper.Data.ReadSaveData<OutdoorAreaData>(saveKey);
                            Game1.getFarm().UpdatePatio();
                            Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("reloaded-patios"), outdoorAreas.areas.Count));
                            return;
                    }
                    break;
                case "CSP_Wizard_Questions_AddPatio":
                    outdoorAreas.areas.Add(whichAnswer, cursorLoc.ToVector2());
                    Game1.getFarm().UpdatePatio();
                    Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("created-patio"), cursorLoc.X, cursorLoc.Y));
                    return;
                case "CSP_Wizard_Questions_MovePatio":
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
                    if (outdoorAreas.areas.Remove(whichAnswer))
                        Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("removed-patio"), whichAnswer));
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
            Game1.getFarm().loadMap(Game1.getFarm().mapPath.Value, true);
            string spouse = spouse_dir.Split('_')[0];
            string dir = spouse_dir.Split('_')[1];
            bool success = false;
            Vector2 outdoorArea = outdoorAreas.areas[spouse];
            switch (dir)
            {
                case "cursorLoc":
                    outdoorArea = cursorLoc.ToVector2();
                    success = true;
                    break;
                case "up":
                    if (outdoorArea.Y <= 0)
                        break;
                    outdoorArea.Y--;
                    success = true;
                    break;
                case "down":
                    if (outdoorArea.Y >= Game1.getFarm().map.Layers[0].LayerHeight - 1)
                        break;
                    outdoorArea.Y++;
                    success = true;
                    break;
                case "left":
                    if (outdoorArea.X == 0)
                        break;
                    outdoorArea.X--;
                    success = true;
                    break;
                case "right":
                    if (outdoorArea.X >= Game1.getFarm().map.Layers[0].LayerWidth - 1)
                        break;
                    outdoorArea.X++;
                    success = true;
                    break;
            }
            if (success)
            {
                outdoorAreas.areas[spouse] = outdoorArea;
                SMonitor.Log($"Moved spouse patio for {spouse} to {outdoorArea}");
                Game1.getFarm().UpdatePatio();
            }
            return success;
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