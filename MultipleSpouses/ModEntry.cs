using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Events;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using xTile;

namespace MultipleSpouses
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
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
        public static bool bedMade = false;

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
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.checkAction)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_checkAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.marriageDuties)),
               //prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_marriageDuties_Prefix)),
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
               original: AccessTools.Method(typeof(GameLocation), "resetLocalState"),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_resetLocalState_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), "updateMap"),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_updateMap_Prefix))
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


            // misc patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doDivorce)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_doDivorce_Prefix))
            );

        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
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
            Maps.tmxSpouseRooms.Clear();
            // TMX spouse rooms
            try
            {
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
            Helper.Events.GameLoop.OneSecondUpdateTicked -= GameLoop_OneSecondUpdateTicked;
            allRandomSpouses = null;
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;

            allRandomSpouses = GetRandomSpouses(true).Keys.ToList();
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked -= GameLoop_OneSecondUpdateTicked;
            spouses.Clear();
            outdoorSpouse = null;
            kitchenSpouse = null;
            bedSpouse = null;
            spouseToDivorce = null;
            spouseRolesDate = -1;
            allRandomSpouses = null;
            bedMade = false;
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
                            Vector2 bedPos = GetSpouseBedLocation(character.name);
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
        public static Vector2 GetSpouseBedLocation(string name)
        {
            SetBedmates();
            FarmHouse fh = Utility.getHomeOfFarmer(Game1.player);
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
            List<NPC> allSpouses = spouses.Values.ToList();
            PMonitor.Log("num spouses: " + allSpouses.Count);
            if(Game1.player.getSpouse() != null)
            {
                PMonitor.Log("official spouse: " + Game1.player.getSpouse().Name);
                allSpouses.Add(Game1.player.getSpouse()); 
            }

            foreach (NPC npc in allSpouses)
            {
                Friendship friendship = npc.getSpouse().friendshipData[npc.Name];
                PMonitor.Log($"spouse: {npc.Name}{(friendship.DaysUntilBirthing >= 0 ? " gives birth in: " + friendship.DaysUntilBirthing : "")}");
            }



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
            PMonitor.Log("Resetting spouses");
            PMonitor.Log("official spouse: " + f.spouse);
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
                    }
                    continue;
                }
                if (f.friendshipData[name].IsMarried() && f.spouse != name)
                {
                    if (f.friendshipData[name].WeddingDate != null)
                    {
                        //PMonitor.Log($"wedding date {f.friendshipData[name].WeddingDate.TotalDays} " + name);
                    }
                    if (f.spouse != null && f.friendshipData[f.spouse] != null && !f.friendshipData[f.spouse].IsMarried() && !f.friendshipData[f.spouse].IsEngaged() && !f.friendshipData[f.spouse].IsRoommate())
                    {
                        PMonitor.Log("invalid ospouse, setting: " + name);
                        f.spouse = name;
                        continue;
                    }
                    if (f.spouse == null)
                    {
                        f.spouse = name;
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
                Utility.getHomeOfFarmer(f).showSpouseRoom();
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


    }
}