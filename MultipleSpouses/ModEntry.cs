using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using xTile;

namespace MultipleSpouses
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetLoader, IAssetEditor
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public static ModConfig config;

        public static Dictionary<string, NPC> spouses { get; private set; } = new Dictionary<string, NPC>();
        public static string outdoorSpouse = null;
        public static string kitchenSpouse = null;
        public static string bedSpouse = null;
        public static string spouseToDivorce = null;
        public static int spouseRolesDate = -1;
        public static Multiplayer mp;
        public static Random myRand;
        public static List<string> allRandomSpouses;
        public static int bedSleepOffset = 48;
        public static List<string> allBedmates;
        public static bool bedMadeToday = false;
        public static bool kidsRoomExpandedToday = false;
        public static string officialSpouse = null;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<ModConfig>();

            if (!config.EnableMod)
                return;

            PMonitor = Monitor;
            PHelper = helper;

            mp = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            myRand = new Random();

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded; 
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;

            NPCPatches.Initialize(Monitor);
            LocationPatches.Initialize(Monitor);
            FarmerPatches.Initialize(Monitor);
            Maps.Initialize(Monitor);
            Kissing.Initialize(Monitor);
            UIPatches.Initialize(Monitor);

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);


            // npc patches

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.setUpForOutdoorPatioActivity)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_setUpForOutdoorPatioActivity_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.getSpouse)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_getSpouse_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.isRoommate)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isRoommate_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.isMarried)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isMarried_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.isMarriedOrEngaged)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isMarriedOrEngaged_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToReceiveActiveObject)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Prefix)),
               transpiler: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Transpiler)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.checkAction)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_checkAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.marriageDuties)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_marriageDuties_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.spouseObstacleCheck)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_spouseObstacleCheck_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), "engagementResponse"),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_engagementResponse_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Child), nameof(Child.reloadSprite)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.Child_reloadSprite_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Child), nameof(Child.resetForPlayerEntry)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.Child_resetForPlayerEntry_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Child), nameof(Child.dayUpdate)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.Child_dayUpdate_Prefix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(Child), nameof(Child.tenMinuteUpdate)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.Child_tenMinuteUpdate_Postfix))
            );


            // location patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Farm), "addSpouseOutdoorArea"),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.Farm_addSpouseOutdoorArea_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Beach), nameof(Beach.checkAction)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.Beach_checkAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(ManorHouse), nameof(ManorHouse.performAction)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.ManorHouse_performAction_Prefix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.getWalls)),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_getWalls_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.getFloors)),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_getFloors_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_performAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Beach), "resetLocalState"),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.Beach_resetLocalState_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), "resetLocalState"),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_resetLocalState_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), "checkEventPrecondition"),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_checkEventPrecondition_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.performTenMinuteUpdate)),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_performTenMinuteUpdate_Postfix))
            );
            
            

            // pregnancy patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.pickPersonalFarmEvent)),
               prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.Utility_pickPersonalFarmEvent_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(QuestionEvent), nameof(QuestionEvent.setUp)),
               prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.QuestionEvent_setUp_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(BirthingEvent), nameof(BirthingEvent.setUp)),
               prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.BirthingEvent_setUp_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(BirthingEvent), nameof(BirthingEvent.tickUpdate)),
               prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.BirthingEvent_tickUpdate_Prefix))
            );


            // Farmer patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doDivorce)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_doDivorce_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.isMarried)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_isMarried_Prefix))
            );

            // UI patches

            harmony.Patch(
               original: AccessTools.Method(typeof(SocialPage), "drawNPCSlot"),
               prefix: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.SocialPage_drawNPCSlot))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Event), nameof(Event.answerDialogueQuestion)),
               prefix: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.Event_answerDialogueQuestion_Prefix))
            );

            harmony.Patch(
               original: typeof(DialogueBox).GetConstructor(new Type[] { typeof(List<string>) }),
               prefix: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.DialogueBox_Prefix))
            );

        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            outdoorSpouse = null;
            kitchenSpouse = null;
            bedSpouse = null;
            spouseToDivorce = null;
            spouseRolesDate = -1;
            allRandomSpouses = null;
            bedSleepOffset = 48;
            allBedmates = null;
            bedMadeToday = false;
            kidsRoomExpandedToday = false;
            officialSpouse = null;
            SetAllNPCsDatable();
            LoadTMXSpouseRooms();
            ResetSpouses(Game1.player);
        }


        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            LoadKissAudio();
        }

        private void LoadKissAudio()
        {
            // kiss audio

            string filePath = $"{PHelper.DirectoryPath}/assets/kiss.wav";
            PMonitor.Log("Kissing audio path: " + filePath);
            if (File.Exists(filePath))
            {
                Kissing.kissEffect = SoundEffect.FromStream(new FileStream(filePath, FileMode.Open));
            }
            else
            {
                PMonitor.Log("Kissing audio not found at path: " + filePath);
            }
        }

        public static void LoadTMXSpouseRooms()
        {
            try
            {
                Maps.tmxSpouseRooms.Clear();
                var tmxlAPI = PHelper.ModRegistry.GetApi("Platonymous.TMXLoader");
                var tmxlAssembly = tmxlAPI?.GetType()?.Assembly;
                var tmxlModType = tmxlAssembly?.GetType("TMXLoader.TMXLoaderMod");
                var tmxlEditorType = tmxlAssembly?.GetType("TMXLoader.TMXAssetEditor");

                if (tmxlModType == null)
                    return;

                var tmxlHelper = PHelper.Reflection.GetField<IModHelper>(tmxlModType, "helper").GetValue();
                foreach (var editor in tmxlHelper.Content.AssetEditors)
                {
                    try
                    {
                        if (editor == null)
                            continue;
                        if (editor.GetType() != tmxlEditorType) continue;

                        if (editor.GetType().GetField("type", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(editor).ToString() != "SpouseRoom") continue;

                        string name = (string)tmxlEditorType.GetField("assetName").GetValue(editor);
                        if (name != "FarmHouse1_marriage") continue;

                        object edit = tmxlEditorType.GetField("edit", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(editor);
                        string info = (string)edit.GetType().GetProperty("info").GetValue(edit);

                        Map map = PHelper.Reflection.GetField<Map>(editor, "newMap").GetValue();
                        if (map != null && !Maps.tmxSpouseRooms.ContainsKey(info))
                        {
                            PMonitor.Log("Adding TMX spouse room for " + info, LogLevel.Debug);
                            Maps.tmxSpouseRooms.Add(info, map);
                        }
                    }
                    catch (Exception ex)
                    {
                        PMonitor.Log($"Failed getting TMX spouse room data. Exception: {ex}", LogLevel.Debug);
                    }
                }

            }
            catch (Exception ex)
            {
                PMonitor.Log($"Failed getting TMX spouse room data. Exception: {ex}", LogLevel.Debug);
            }
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            Helper.Content.InvalidateCache("Maps/FarmHouse1_marriage");
            Helper.Content.InvalidateCache("Maps/FarmHouse2");
            Helper.Content.InvalidateCache("Maps/FarmHouse2_marriage");
            Helper.Events.GameLoop.OneSecondUpdateTicked -= GameLoop_OneSecondUpdateTicked;
            allRandomSpouses = null;
            kidsRoomExpandedToday = false;
            bedMadeToday = false;
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            ResetSpouses(Game1.player);
            allRandomSpouses = GetRandomSpouses(true).Keys.ToList();

            Utility.getHomeOfFarmer(Game1.player).showSpouseRoom();
            Maps.BuildSpouseRooms(Utility.getHomeOfFarmer(Game1.player));
            PlaceSpousesInFarmhouse();
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked -= GameLoop_OneSecondUpdateTicked;
            spouses.Clear();
            outdoorSpouse = null;
            kitchenSpouse = null;
            bedSpouse = null;
            officialSpouse = null;
            spouseToDivorce = null;
            spouseRolesDate = -1;
            allRandomSpouses = null;
            allBedmates = null;
            bedMadeToday = false;
            kidsRoomExpandedToday = false;
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.MouseLeft || e.Button == SButton.MouseRight)
            {
                if (Game1.currentLocation == null || Game1.currentLocation.lastQuestionKey != "divorce")
                    return;
                
                IClickableMenu menu = Game1.activeClickableMenu;
                if (menu == null || menu.GetType() != typeof(DialogueBox))
                    return;
                int resp = (int)typeof(DialogueBox).GetField("selectedResponse", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menu);
                List<Response> resps = (List<Response>)typeof(DialogueBox).GetField("responses", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menu);

                if (resp < 0 || resps == null || resp >= resps.Count || resps[resp] == null)
                    return;

                string key = resps[resp].responseKey; 

                foreach (NPC spouse in spouses.Values)
                {
                    if (key == spouse.Name || key == "Krobus" || key == Game1.player.spouse)
                    {
                        if (Game1.player.Money >= 50000 || key == "Krobus")
                        {
                            if (!Game1.player.isRoommate(key))
                            {
                                Game1.player.Money -= 50000;
                                ModEntry.spouseToDivorce = key;
                            }
                            Game1.player.divorceTonight.Value = true;
                            string s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed_" + key);
                            if (s == null)
                            {
                                s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed");
                            }
                            Game1.drawObjectDialogue(s);
                            if (!Game1.player.isRoommate(Game1.player.spouse))
                            {
                                mp.globalChatInfoMessage("Divorce", new string[]
                                {
                                    Game1.player.Name
                                });
                            }
                        }
                        else
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
                        }
                        break;
                    }
                }
            }
        }
        private void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (Game1.player == null)
                return;
            FarmHouse fh = Utility.getHomeOfFarmer(Game1.player);
            if (fh == null)
                return;

            int bedWidth = GetBedWidth(fh);
            Point bedStart = GetBedStart(fh);
            foreach (NPC character in fh.characters)
            {
                if (allRandomSpouses.Contains(character.Name))
                {
                    if (IsInBed(character.GetBoundingBox()))
                    {
                        character.farmerPassesThrough = true;
                        if (Game1.timeOfDay >= 2000 && !character.isMoving())
                        {
                            Vector2 bedPos = GetSpouseBedPosition(fh, character.name);
                            character.position.Value = bedPos;
                        }
                    }
                    else
                    {
                        character.farmerPassesThrough = false;
                    }
                }
            }
            if (config.AllowSpousesToKiss)
            {
                Kissing.TrySpousesKiss();
            }
        }

        public static List<string> GetAllSpouseNamesOfficialFirst(Farmer farmer)
        {
            List<string> mySpouses = spouses.Keys.ToList();
            if (farmer.spouse != null)
            {
                mySpouses.Insert(0, farmer.spouse);
            }
            return mySpouses;
        }
        
        public static void GetSpouseRoomPosition(FarmHouse farmHouse, string spouse)
        {
        }

        public static void PlaceSpousesInFarmhouse()
        {
            Farmer farmer = Game1.player;
            FarmHouse farmHouse = Utility.getHomeOfFarmer(farmer);

            List<NPC> mySpouses = spouses.Values.ToList();
            if (farmer.spouse != null)
            {
                mySpouses.Insert(0, farmer.getSpouse());
            }

            foreach (NPC j in mySpouses) { 
                ModEntry.PMonitor.Log("placing " + j.Name);

                if (ModEntry.outdoorSpouse == j.Name && !Game1.isRaining && !Game1.IsWinter && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && !j.Name.Equals("Krobus"))
                {
                    ModEntry.PMonitor.Log("going to outdoor patio");
                    j.setUpForOutdoorPatioActivity();
                    continue;
                }

                if (j.currentLocation != farmHouse)
                {
                    continue;
                }


                ModEntry.PMonitor.Log("in farm house");
                j.shouldPlaySpousePatioAnimation.Value = false;

                Vector2 spot = (farmHouse.upgradeLevel == 1) ? new Vector2(32f, 5f) : new Vector2(38f, 14f);

                if (ModEntry.bedSpouse != null)
                {
                    foreach (NPC character in farmHouse.characters)
                    {
                        if (character.isVillager() && ModEntry.GetAllSpouses().ContainsKey(character.Name) && ModEntry.IsInBed(character.GetBoundingBox()))
                        {
                            ModEntry.PMonitor.Log($"{character.Name} is already in bed");
                            ModEntry.bedSpouse = character.Name;
                            character.position.Value = ModEntry.GetSpouseBedPosition(farmHouse, character.name);
                            break;
                        }
                    }
                }

                if (ModEntry.kitchenSpouse == j.Name)
                {
                    ModEntry.PMonitor.Log($"{j.Name} is in kitchen");
                    j.setTilePosition(farmHouse.getKitchenStandingSpot());
                    ModEntry.kitchenSpouse = null;
                }
                else if (ModEntry.bedSpouse == j.Name)
                {
                    ModEntry.PMonitor.Log($"{j.Name} is in bed");
                    j.position.Value = ModEntry.GetSpouseBedPosition(farmHouse, j.name);
                    j.faceDirection(ModEntry.myRand.NextDouble() > 0.5 ? 1 : 3);
                    ModEntry.bedSpouse = null;
                }
                else if (!ModEntry.config.BuildAllSpousesRooms && farmer.spouse != j.Name)
                {
                    j.setTilePosition(farmHouse.getRandomOpenPointInHouse(ModEntry.myRand));
                }
                else
                {
                    ModEntry.ResetSpouses(farmer);

                    List<NPC> roomSpouses = mySpouses.FindAll((s) => Maps.roomIndexes.ContainsKey(s.Name) || Maps.tmxSpouseRooms.ContainsKey(s.Name));


                    if (!roomSpouses.Contains(j))
                    {
                        j.setTilePosition(farmHouse.getRandomOpenPointInHouse(ModEntry.myRand));
                        j.faceDirection(ModEntry.myRand.Next(0, 4));
                        ModEntry.PMonitor.Log($"{j.Name} spouse random loc");
                        continue;
                    }
                    else
                    {
                        int offset = roomSpouses.IndexOf(j) * 7;
                        j.setTilePosition((int)spot.X + offset, (int)spot.Y);
                        j.faceDirection(ModEntry.myRand.Next(0, 4));
                        ModEntry.PMonitor.Log($"{j.Name} loc: {(spot.X + offset)},{spot.Y}");
                    }
                }
            }

        }

        public static bool IsInBed(Rectangle box)
        {
            FarmHouse fh = Utility.getHomeOfFarmer(Game1.player);
            int bedWidth = GetBedWidth(fh);
            Point bedStart = GetBedStart(fh);
            Rectangle bed = new Rectangle(bedStart.X * 64, bedStart.Y * 64 + 64, bedWidth * 64, 3 * 64);
            return box.Intersects(bed);
        }
        public static void SetBedmates()
        {
            if (allRandomSpouses == null)
            {
                allRandomSpouses = GetRandomSpouses(true).Keys.ToList();
            }

            List<string> bedmates = new List<string>();
            bedmates.Add("Game1.player");
            for (int i = 0; i < allRandomSpouses.Count; i++)
            {
                bedmates.Add(allRandomSpouses[i]);
            }
            allBedmates = new List<string>(bedmates);
        }
        public static Vector2 GetSpouseBedPosition(FarmHouse fh, string name)
        {
            SetBedmates();
            int bedWidth = GetBedWidth(fh);
            Point bedStart = GetBedStart(fh);
            int x = (int)(allBedmates.IndexOf(name) / (float)allBedmates.Count * (bedWidth - 1) * 64);
            return new Vector2(bedStart.X * 64 + x, bedStart.Y * 64 + 64 + bedSleepOffset + (name == "Game1.player"?32:0));
        }

        public static Point GetBedStart(FarmHouse fh)
        {
            bool up = fh.upgradeLevel > 1;
            return new Point(21 - (up ? (GetBedWidth(fh) / 2) - 1: 0) + (up ? 6 : 0), 2 + (up?9:0));
        }

        public static int GetBedWidth(FarmHouse fh)
        {
            if (config.CustomBed)
            {
                bool up = fh.upgradeLevel > 1;
                return Math.Min(up ? 9 : 6, Math.Max(config.BedWidth, 3));
            }
            else
            {
                return 3;
            }
        }

        public static void ResetSpouseRoles()
        {
            spouseRolesDate = new WorldDate().TotalDays;
            outdoorSpouse = null;
            kitchenSpouse = null;
            bedSpouse = null;
            ResetSpouses(Game1.player);
            List<NPC> allSpouses = GetAllSpouses().Values.ToList();
            PMonitor.Log("num spouses: " + allSpouses.Count);

            int n = allSpouses.Count;
            while (n > 1)
            {
                n--;
                int k = myRand.Next(n + 1);
                NPC value = allSpouses[k];
                allSpouses[k] = allSpouses[n];
                allSpouses[n] = value;
            }

            Game1.getFarm().addSpouseOutdoorArea("");

            foreach (NPC spouse in allSpouses)
            {
                int maxType = 4;


                int type = myRand.Next(0, maxType);

                PMonitor.Log("spouse type: " + type);
                switch (type)
                {
                    case 1:
                        if (bedSpouse == null)
                        {
                            PMonitor.Log("made bed spouse: " + spouse.Name);
                            bedSpouse = spouse.Name;
                        }
                        break;
                    case 2:
                        if (kitchenSpouse == null)
                        {
                            PMonitor.Log("made kitchen spouse: " + spouse.Name);
                            kitchenSpouse = spouse.Name;
                        }
                        break;
                    case 3:
                        if (outdoorSpouse == null)
                        {
                            PMonitor.Log("made outdoor spouse: " + spouse.Name);
                            outdoorSpouse = spouse.Name;
                            Game1.getFarm().addSpouseOutdoorArea(outdoorSpouse);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        internal static bool SpotHasSpouse(Vector2 position, GameLocation location)
        {
            foreach(NPC spouse in spouses.Values)
            {
                if (spouse.currentLocation == location)
                {
                    Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle((int)position.X + 1, (int)position.Y + 1, 62, 62);
                    if(spouse.GetBoundingBox().Intersects(rect))
                        return true;
                }
            }
            return false;
        }

        public static void ResetSpouses(Farmer f)
        {
            if (f.spouse == null)
            {
                if(officialSpouse != null && f.friendshipData[officialSpouse] != null && (f.friendshipData[officialSpouse].IsMarried() || f.friendshipData[officialSpouse].IsEngaged()))
                {
                    f.spouse = officialSpouse;
                }
            }
            officialSpouse = f.spouse;

            spouses.Clear();
            foreach (string name in f.friendshipData.Keys)
            {
                if (f.friendshipData[name].IsEngaged())
                {
                    if(f.friendshipData[name].WeddingDate.TotalDays < new WorldDate(Game1.Date).TotalDays)
                    {
                        PMonitor.Log("invalid engagement: " + name);
                        f.friendshipData[name].WeddingDate.TotalDays = new WorldDate(Game1.Date).TotalDays + 1;
                    }
                    if(f.spouse != name)
                    {
                        PMonitor.Log("setting spouse to engagee: " + name);
                        f.spouse = name;
                        officialSpouse = name;
                    }
                    continue;
                }
                if (f.friendshipData[name].IsMarried() && f.spouse != name)
                {
                    if (f.friendshipData[name].WeddingDate != null)
                    {
                        //PMonitor.Log($"wedding date {f.friendshipData[name].WeddingDate.TotalDays} " + name);
                    }
                    if (f.spouse != null && f.friendshipData[f.spouse] != null && !f.friendshipData[f.spouse].IsMarried() && !f.friendshipData[f.spouse].IsEngaged())
                    {
                        PMonitor.Log("invalid ospouse, setting: " + name);
                        f.spouse = name;
                        officialSpouse = name;
                        continue;
                    }
                    if (f.spouse == null)
                    {
                        PMonitor.Log("null ospouse, setting: " + name);
                        f.spouse = name;
                        officialSpouse = name;
                        continue;
                    }

                    NPC npc = Game1.getCharacterFromName(name);
                    if(npc == null)
                    {
                        foreach(GameLocation l in Game1.locations)
                        {
                            foreach(NPC c in l.characters)
                            {
                                if(c.Name == name)
                                {
                                    npc = c;
                                    goto next;
                                }
                            }
                        }
                    }
                    if(npc == null)
                    {
                        continue;
                    }
                    next:
                    PMonitor.Log("adding spouse: " + name);
                    spouses.Add(name,npc);
                }
            }
            PMonitor.Log("official spouse: " + officialSpouse);
        }
        private void SetAllNPCsDatable()
        {
            //if (!config.RomanceAllVillagers)
                return;
            Farmer f = Game1.player;
            if (f == null)
            {
                return;
            }
            foreach (string friend in f.friendshipData.Keys)
            {
                NPC npc = Game1.getCharacterFromName(friend);
                if (npc != null)
                {
                    npc.datable.Value = true;
                }
            }
        }

        public static Dictionary<string,NPC> GetAllSpouses()
        {
            Dictionary<string, NPC> npcs = new Dictionary<string, NPC>(spouses);
            NPC ospouse = Game1.player.getSpouse();
            if (ospouse != null)
            {
                npcs.Add(ospouse.Name, ospouse);
            }
            return npcs;
        }
        public static Dictionary<string,NPC> GetRandomSpouses(bool all = false)
        {
            Dictionary<string, NPC> npcs = new Dictionary<string, NPC>(spouses);
            if (all)
            {
                NPC ospouse = Game1.player.getSpouse();
                if (ospouse != null)
                {
                    npcs.Add(ospouse.Name, ospouse);
                }
            }

            ShuffleDic(ref npcs);

            return npcs;
        }

        public static void ShuffleList<T>(ref List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = myRand.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static void ShuffleDic<T1,T2>(ref Dictionary<T1,T2> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = myRand.Next(n + 1);
                var value = list[list.Keys.ToArray()[k]];
                list[list.Keys.ToArray()[k]] = list[list.Keys.ToArray()[n]];
                list[list.Keys.ToArray()[n]] = value;
            }
        }



        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!config.EnableMod)
                return false;

            string[] names = asset.AssetName.Split('_');
            if (config.ChildrenHaveHairOfSpouse && (names[0].Equals("Characters\\Baby") || names[0].Equals("Characters\\Toddler") || names[0].Equals("Characters/Baby") || names[0].Equals("Characters/Toddler")))
            {
                Monitor.Log($"can load child asset for {asset.AssetName}");
                return true;
            }

            return false;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Monitor.Log($"loading asset for {asset.AssetName}");
            if (asset.AssetName.StartsWith("Characters\\Baby") || asset.AssetName.StartsWith("Characters\\Toddler") || asset.AssetName.StartsWith("Characters/Baby") || asset.AssetName.StartsWith("Characters/Toddler"))
            {
                if(asset.AssetNameEquals("Characters\\Baby") || asset.AssetNameEquals("Characters\\Baby_dark") || asset.AssetNameEquals("Characters\\Toddler") || asset.AssetNameEquals("Characters\\Toddler_dark") || asset.AssetNameEquals("Characters\\Toddler_girl") || asset.AssetNameEquals("Characters\\Toddler_girl_dark"))
                {
                    Monitor.Log($"loading default child asset for {asset.AssetName}");
                    return (T)(object)Helper.Content.Load<Texture2D>($"assets/{asset.AssetName.Replace("Characters\\", "").Replace("Characters/", "")}.png", ContentSource.ModFolder);
                }
                if(asset.AssetNameEquals("Characters/Baby") || asset.AssetNameEquals("Characters/Baby_dark") || asset.AssetNameEquals("Characters/Toddler") || asset.AssetNameEquals("Characters/Toddler_dark") || asset.AssetNameEquals("Characters/Toddler_girl") || asset.AssetNameEquals("Characters/Toddler_girl_dark"))
                {
                    Monitor.Log($"loading default child asset for {asset.AssetName}");
                    return (T)(object)Helper.Content.Load<Texture2D>($"assets/{asset.AssetName.Replace("Characters/", "")}.png", ContentSource.ModFolder);
                }

                Monitor.Log($"loading child asset for {asset.AssetName}");

                string[] names = asset.AssetName.Split('_');
                string parent = names[names.Length - 1];
                Texture2D parentTexSheet = Helper.Content.Load<Texture2D>($"Characters/{parent}", ContentSource.GameContent);
                Texture2D babySheet = Helper.Content.Load<Texture2D>(string.Join("_", names.Take(names.Length - 1)), ContentSource.GameContent);
                if (parentTexSheet == null)
                {
                    Monitor.Log($"couldn't find parent sheet for {asset.AssetName}");
                    return (T)(object)babySheet;
                }
                Rectangle newBounds = parentTexSheet.Bounds;
                newBounds.X = 0;
                newBounds.Y = 64;
                newBounds.Width = 16;
                newBounds.Height = 32;
                Texture2D parentTex = new Texture2D(Game1.graphics.GraphicsDevice, 16, 32);
                Color[] data = new Color[parentTex.Width * parentTex.Height];
                parentTexSheet.GetData(0, newBounds, data, 0, newBounds.Width * newBounds.Height);
                
                int start = -1;
                Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();
                for (int i = 0; i < data.Length; i++)
                {
                    if(data[i] != Color.Transparent)
                    {
                        if(start == -1)
                        {
                            start = i / 16;
                        }
                        else
                        {
                            if (i / 16 - start > 8)
                                break;
                        }
                        if (colorCounts.ContainsKey(data[i]))
                        {
                            colorCounts[data[i]]++;
                        }
                        else
                        {
                            colorCounts.Add(data[i], 1);
                            Monitor.Log($"got hair color: {data[i]}");
                        }
                    }
                }

                if(colorCounts.Count == 0)
                {
                    Monitor.Log($"parent sheet empty for {asset.AssetName}");
                    return (T)(object)babySheet;
                }

                var countsList = colorCounts.ToList();

                countsList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

                List<Color> hairColors = new List<Color>();
                for (int k = 0; k < Math.Min(countsList.Count, 4); k++)
                {
                    Monitor.Log($"using hair color: {countsList[k].Key} {countsList[k].Value}");
                    hairColors.Add(countsList[k].Key);
                }
                hairColors.Sort((color1, color2) => (color1.R + color1.G + color1.B).CompareTo(color2.R + color2.G + color2.B));

                Texture2D hairSheet = Helper.Content.Load<Texture2D>($"assets/hair/{string.Join("_", names.Take(names.Length - 1)).Replace("Characters\\","").Replace("Characters/","").Replace("_dark","")}.png", ContentSource.ModFolder);
                Color[] babyData = new Color[babySheet.Width * babySheet.Height];
                Color[] hairData = new Color[babySheet.Width * babySheet.Height];
                babySheet.GetData(babyData);
                hairSheet.GetData(hairData);

                for(int i = 0; i < babyData.Length; i++)
                {
                    if(hairData[i] != Color.Transparent)
                    {
                        if(hairColors.Count == 1)
                        {
                            hairColors.Add(hairColors[0]);
                            hairColors.Add(hairColors[0]);
                            hairColors.Add(hairColors[0]);
                        }
                        else if(hairColors.Count == 2)
                        {
                            hairColors.Add(hairColors[1]);
                            hairColors.Add(hairColors[1]);
                            hairColors[1] = new Color((hairColors[0].R + hairColors[0].R + hairColors[1].R) / 3, (hairColors[0].G + hairColors[0].G + hairColors[1].G) / 3, (hairColors[0].B + hairColors[0].B + hairColors[1].B) / 3);
                            hairColors[2] = new Color((hairColors[0].R + hairColors[2].R + hairColors[2].R) / 3, (hairColors[0].G + hairColors[2].G + hairColors[2].G) / 3, (hairColors[0].B + hairColors[2].B + hairColors[2].B) / 3);
                        }
                        else if(hairColors.Count == 3)
                        {
                            hairColors.Add(hairColors[2]);
                            hairColors[2] = new Color((hairColors[1].R + hairColors[2].R + hairColors[2].R) / 3, (hairColors[1].G + hairColors[2].G + hairColors[2].G) / 3, (hairColors[1].B + hairColors[2].B + hairColors[2].B) / 3);
                            hairColors[1] = new Color((hairColors[0].R + hairColors[0].R + hairColors[1].R) / 3, (hairColors[0].G + hairColors[0].G + hairColors[1].G) / 3, (hairColors[0].B + hairColors[0].B + hairColors[1].B) / 3);
                        }
                        //Monitor.Log($"Hair grey: {hairData[i].R}");
                        switch (hairData[i].R)
                        {
                            case 42:
                                babyData[i] = hairColors[0];
                                break;
                            case 60:
                                babyData[i] = hairColors[1];
                                break;
                            case 66:
                                babyData[i] = hairColors[1];
                                break;
                            case 82:
                                babyData[i] = hairColors[2];
                                break;
                            case 93:
                                babyData[i] = hairColors[2];
                                break;
                            case 114:
                                babyData[i] = hairColors[3];
                                break;
                        }
                            //Monitor.Log($"Hair color: {babyData[i]}");
                    }
                }
                babySheet.SetData(babyData);
                return (T)(object)babySheet;
            }
            throw new InvalidOperationException($"Unexpected asset '{asset.AssetName}'.");
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!config.EnableMod)
                return false;

            if (asset.AssetNameEquals("Data/Events/HaleyHouse") || asset.AssetNameEquals("Data/Events/Saloon") || asset.AssetNameEquals("Data/EngagementDialogue") || asset.AssetNameEquals("Strings/StringsFromCSFiles"))
            {
                return true;
            }

            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/Events/HaleyHouse"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                data["195012/f Haley 2500/f Emily 2500/f Penny 2500/f Abigail 2500/f Leah 2500/f Maru 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 38/e 2123343/e 10/e 901756/e 54/e 15/k 195019"] = Regex.Replace(data["195012/f Haley 2500/f Emily 2500/f Penny 2500/f Abigail 2500/f Leah 2500/f Maru 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 38/e 2123343/e 10/e 901756/e 54/e 15/k 195019"], "(pause 1000/speak Maru \\\")[^$]+.a\\\"",$"$1{PHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21/emote Emily 21/emote Penny 21/emote Maru 21/emote Leah 21/emote Abigail 21").Replace("/dump girls 3", "");
                data["choseToExplain"] = Regex.Replace(data["choseToExplain"], "(pause 1000/speak Maru \\\")[^$]+.a\\\"",$"$1{PHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21/emote Emily 21/emote Penny 21/emote Maru 21/emote Leah 21/emote Abigail 21").Replace("/dump girls 4", "");
                data["lifestyleChoice"] = Regex.Replace(data["lifestyleChoice"], "(pause 1000/speak Maru \\\")[^$]+.a\\\"",$"$1{PHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21/emote Emily 21/emote Penny 21/emote Maru 21/emote Leah 21/emote Abigail 21").Replace("/dump girls 4", "");
            }
            else if (asset.AssetNameEquals("Data/Events/Saloon"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                data["195013/f Shane 2500/f Sebastian 2500/f Sam 2500/f Harvey 2500/f Alex 2500/f Elliott 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 911526/e 528052/e 9581348/e 43/e 384882/e 233104/k 195099"] = Regex.Replace(data["195013/f Shane 2500/f Sebastian 2500/f Sam 2500/f Harvey 2500/f Alex 2500/f Elliott 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 911526/e 528052/e 9581348/e 43/e 384882/e 233104/k 195099"], "(pause 1000/speak Sam \\\")[^$]+.a\\\"",$"$1{PHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21/emote Sebastian 21/emote Sam 21/emote Harvey 21/emote Alex 21/emote Elliott 21").Replace("/dump guys 3", "");
                data["choseToExplain"] = Regex.Replace(data["choseToExplain"], "(pause 1000/speak Sam \\\")[^$]+.a\\\"", $"$1{PHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21/emote Sebastian 21/emote Sam 21/emote Harvey 21/emote Alex 21/emote Elliott 21").Replace("/dump guys 4", "");
                data["crying"] = Regex.Replace(data["crying"], "(pause 1000/speak Sam \\\")[^$]+.a\\\"",$"$1{PHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21/emote Sebastian 21/emote Sam 21/emote Harvey 21/emote Alex 21/emote Elliott 21").Replace("/dump guys 4", "");
            }
            else if (asset.AssetNameEquals("Strings/StringsFromCSFiles"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                data["NPC.cs.3985"] = Regex.Replace(data["NPC.cs.3985"],  @"\$s.+", $"$n#$e#$c 0.5#{data["ResourceCollectionQuest.cs.13681"]}#{data["ResourceCollectionQuest.cs.13683"]}");
            }
            else if (asset.AssetNameEquals("Data/EngagementDialogue"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                if (!config.RomanceAllVillagers)
                    return;
                Farmer f = Game1.player;
                if (f == null)
                {
                    return;
                }
                foreach (string friend in f.friendshipData.Keys)
                {
                    if (!data.ContainsKey(friend+"0"))
                    {
                        data[friend + "0"] = "";
                    }
                    if (!data.ContainsKey(friend+"1"))
                    {
                        data[friend + "1"] = "";
                    }
                }
            }
        }

        public static Point getChildBed(FarmHouse farmhouse, string name)
        {
            List<NPC> children = farmhouse.characters.ToList().FindAll((n) => n is Child && n.Age == 3);
            int index = children.FindIndex((n) => n.Name == name);
            int offset = (index * 4) + (config.ExtraCribs * 3);
            if (index > config.ExtraKidsBeds + 1)
            {
                offset = (index % (config.ExtraKidsBeds + 1) * 4) + 1;
            }
            return new Point(22 + offset, 5);
        }
        public static bool ChangingHouse()
        {
            return config.BuildAllSpousesRooms || config.CustomBed || config.ExtraCribs != 0 || config.ExtraKidsBeds != 0 || config.ExtraKidsRoomWidth != 0;
        }
    }
}