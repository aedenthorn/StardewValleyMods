using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using xTile.Layers;
using xTile.Tiles;

namespace RandomNPC
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetEditor, IAssetLoader
    {
        internal static ModConfig Config { get; private set; }
        internal DialogueData RNPCdialogueData { get; private set; }
        internal ModData RNPCengagementDialogueStrings { get; private set; }
        internal ModData RNPCgiftDialogueStrings { get; private set; }
        internal ModData RNPCscheduleStrings { get; private set; }
        internal ModData RNPCfemaleNameStrings { get; private set; }
        internal ModData RNPCmaleNameStrings { get; private set; }
        internal ModData RNPCbodyTypes { get; private set; }
        internal ModData RNPCdarkSkinColours { get; private set; }
        internal ModData RNPClightSkinColours { get; private set; }
        internal ModData RNPChairStyles { get; private set; }
        internal ModData RNPCnaturalHairColours { get; private set; }
        internal ModData RNPCdarkHairColours { get; private set; }
        internal ModData RNPCexoticHairColours { get; private set; }
        internal ModData RNPCclothes { get; private set; }
        internal ModData RNPCskinColours { get; private set; }
        internal ModData RNPCsavedNPCs { get; private set; }

        private List<RNPCSchedule> RNPCSchedules = new List<RNPCSchedule>(); 
        public int RNPCMaxVisitors { get; private set; }

        private bool droveOff = false;
        public List<RNPC> RNPCs = new List<RNPC>();
        private bool drivingOff;
        private string[] SDVNPCS = new string[] { "Alex", "Elliott", "Harvey", "Sam", "Sebastian", "Shane", "Abigail", "Emily", "Haley", "Leah", "Maru", "Penny", "Caroline", "Clint", "Demetrius", "Evelyn", "George", "Gus", "Jas", "Jodi", "Krobus", "Lewis", "Linus", "Marnie", "Pam", "Pierre", "Robin", "Sandy", "Vincent", "Willy", "Gunther", "Marlon" };



        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = this.Helper.ReadConfig<ModConfig>();

            this.RNPCdialogueData = this.Helper.Data.ReadJsonFile<DialogueData>("assets/dialogue.json") ?? new DialogueData();
            this.RNPCengagementDialogueStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/engagement_dialogues.json") ?? new ModData();
            this.RNPCgiftDialogueStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/gift_dialogues.json") ?? new ModData();

            this.RNPCscheduleStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/schedules.json") ?? new ModData();

            this.RNPCfemaleNameStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/female_names.json") ?? new ModData();
            this.RNPCmaleNameStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/male_names.json") ?? new ModData();

            this.RNPCbodyTypes = this.Helper.Data.ReadJsonFile<ModData>("assets/body_types.json") ?? new ModData();
            this.RNPCdarkSkinColours = this.Helper.Data.ReadJsonFile<ModData>("assets/dark_skin_colours.json") ?? new ModData();
            this.RNPClightSkinColours = this.Helper.Data.ReadJsonFile<ModData>("assets/light_skin_colours.json") ?? new ModData();

            this.RNPChairStyles = this.Helper.Data.ReadJsonFile<ModData>("assets/hair_styles.json") ?? new ModData();
            this.RNPCnaturalHairColours = this.Helper.Data.ReadJsonFile<ModData>("assets/natural_hair_sets.json") ?? new ModData();
            this.RNPCdarkHairColours = this.Helper.Data.ReadJsonFile<ModData>("assets/dark_hair_sets.json") ?? new ModData();
            this.RNPCexoticHairColours = this.Helper.Data.ReadJsonFile<ModData>("assets/exotic_hair_sets.json") ?? new ModData();

            this.RNPCclothes = this.Helper.Data.ReadJsonFile<ModData>("assets/clothes.json") ?? new ModData();

            this.RNPCMaxVisitors = Math.Min(24, Math.Min(Config.RNPCTotal, Config.RNPCMaxVisitors));

            this.RNPCsavedNPCs = this.Helper.Data.ReadJsonFile<ModData>("saved_npcs.json") ?? new ModData();
            while (RNPCsavedNPCs.data.Count < Config.RNPCTotal)
            {
                RNPCsavedNPCs.data.Add(GenerateNPCString());
            }
            RNPCsavedNPCs.data = RNPCsavedNPCs.data.Take(Config.RNPCTotal).ToList();

            this.Helper.Data.WriteJsonFile<ModData>("saved_npcs.json", RNPCsavedNPCs);

            for (int i = 0; i < RNPCsavedNPCs.data.Count; i++)
            {
                string npc = RNPCsavedNPCs.data[i];
                this.RNPCs.Add(new RNPC(npc, i));
            }

            // shuffle for visitors

            RNPCs = RNPCs.OrderBy(n => Guid.NewGuid()).ToList();

            for (int i = 0; i < RNPCs.Count; i++)
            {
                //RNPCs[i].startLoc = "BusStop " + (13 + (i % 6)) + " " + (11 + i / 6);
                if (i < RNPCMaxVisitors)
                {
                    RNPCs[i].visiting = true;
                }
                else
                {
                    RNPCs[i].visiting = false;
                }
            }

            helper.Events.GameLoop.ReturnedToTitle += this.ReturnedToTitle;
            helper.Events.GameLoop.DayEnding += this.DayEnding;
            helper.Events.GameLoop.DayStarted += this.DayStarted;
            helper.Events.GameLoop.SaveLoaded += this.SaveLoaded;
            //helper.Events.GameLoop.TimeChanged += this.TimeChanged;
            helper.Events.GameLoop.OneSecondUpdateTicked += this.OneSecondUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += this.UpdateTicked;
            //helper.Events.Display.MenuChanged += this.MenuChanged;
            helper.Events.Input.ButtonPressed += this.ButtonPressed;
            if (!Config.DestroyObjectsUnderfoot)
            {
                helper.Events.GameLoop.UpdateTicking += this.UpdateTicking;
            }

        }

        private void SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            foreach(RNPC rnpc in RNPCs)
            {
                rnpc.questItem = GetRandomQuestItem(rnpc);
            }
        }

        private int GetRandomQuestItem(RNPC rnpc)
        {
            //Alert(Game1.currentSeason);
            int item;
            if (!Game1.currentSeason.Equals("winter") && Game1.random.NextDouble() < 0.5)
            {
                List<int> crops = Utility.possibleCropsAtThisTime(Game1.currentSeason, Game1.dayOfMonth <= 7);
                item = crops.ElementAt(Game1.random.Next(crops.Count));
            }
            else
            {
                item = Utility.getRandomItemFromSeason(Game1.currentSeason, 1000, true, true);
                if (item == -5)
                {
                    item = 176;
                }
                if (item == -6)
                {
                    item = 184;
                }
            }
            return item;
        }

        private void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if(e.Button == SButton.MouseLeft || e.Button == SButton.MouseRight)
            {
                IClickableMenu menu = Game1.activeClickableMenu;
                if (menu == null || menu.GetType() != typeof(DialogueBox))
                    return;
                DialogueBox db = menu as DialogueBox;
                int resp = db.selectedResponse;
                List<Response> resps = db.responses;

                if (resp < 0 || resp >= resps.Count || resps[resp] == null)
                    return;
                string key = resps[resp].responseKey;
                if (key.StartsWith("accept_npc_quest"))
                {
                    StartQuest(key);
                }
            }
        }

        private void StartQuest(string key)
        {
            int id = int.Parse(key.Split('_')[3]); // accept_npc_quest_ID
            Game1.player.addQuest(id);
        }

        private void MenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.OldMenu is DialogueBox)
            {
                DialogueBox db2 = (DialogueBox)e.OldMenu;
                FieldInfo fi = typeof(DialogueBox).GetField("selectedResponse", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo fi2 = typeof(DialogueBox).GetField("dialogues", BindingFlags.NonPublic | BindingFlags.Instance);
                List<string> dialogues = (List<string>)fi.GetValue(db2);

            }
        }

        private void UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            // go over obstacles

            foreach (RNPC rnpc in RNPCs)
            {
                NPC npc = Game1.getCharacterFromName(rnpc.nameID);
                if (npc == null)
                    continue;
                GameLocation currentLocation = npc.currentLocation;
                int dir = npc.getDirection();
                if (currentLocation.isCollidingPosition(npc.nextPosition(dir), Game1.viewport, false, 0, false, npc))
                {
                    npc.isCharging = true;
                }
                else
                    npc.isCharging = false;
            }
        }

        private void UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            if (drivingOff && !droveOff)
            {
                FieldInfo pos = typeof(BusStop).GetField("busPosition", BindingFlags.NonPublic | BindingFlags.Instance);
                BusStop bs = (BusStop)Game1.getLocationFromName("BusStop");
                if (((Vector2)pos.GetValue(bs)).X + 512f >= 0f)
                {
                    FieldInfo mot = typeof(BusStop).GetField("busMotion", BindingFlags.NonPublic | BindingFlags.Instance);
                    mot.SetValue(bs, new Vector2(((Vector2)mot.GetValue(bs)).X - 0.075f, ((Vector2)mot.GetValue(bs)).Y));
                }
                else
                {
                    droveOff = true;
                    drivingOff = false;
                }
            }
        }

        private void TimeChanged(object sender, TimeChangedEventArgs e)
        {
        }

        private void OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (Game1.timeOfDay >= Config.LeaveTime && !droveOff && !drivingOff)
            {
                int gone = 0;
                foreach (NPC npc in Game1.getLocationFromName("BusStop").characters)
                {
                    if (npc.getTileLocation().X > 1000 && npc.getTileLocation().Y > 1000)
                    {
                        gone++;
                    }
                    if (npc.getTileLocation().X == 12 && npc.getTileLocation().Y == 9)
                    {
                        foreach (RNPC rnpc in RNPCs)
                        {
                            if (npc.name.Equals(rnpc.nameID))
                            {
                                Game1.warpCharacter(npc, "BusStop", new Vector2(10000, 10000));
                            }
                        }
                    }
                }

                if (!drivingOff && gone == RNPCs.Count)
                {
                    //Alert("Driving off");
                    drivingOff = true;
                    FieldInfo door = typeof(BusStop).GetField("busDoor", BindingFlags.NonPublic | BindingFlags.Instance);
                    FieldInfo pos = typeof(BusStop).GetField("busPosition", BindingFlags.NonPublic | BindingFlags.Instance);
                    FieldInfo mot = typeof(BusStop).GetField("busMotion", BindingFlags.NonPublic | BindingFlags.Instance);
                    BusStop bs = (BusStop)Game1.getLocationFromName("BusStop");
                    door.SetValue(bs, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1311, 16, 38), (Vector2)pos.GetValue(bs) + new Vector2(16f, 26f) * 4f, false, 0f, Color.White)
                    {
                        interval = 999999f,
                        animationLength = 6,
                        holdLastFrame = true,
                        layerDepth = (((Vector2)pos.GetValue(bs)).Y + 192f) / 10000f + 1E-05f,
                        scale = 4f
                    });
                    ((TemporaryAnimatedSprite)door.GetValue(bs)).timer = 0f;
                    ((TemporaryAnimatedSprite)door.GetValue(bs)).interval = 70f;
                    ((TemporaryAnimatedSprite)door.GetValue(bs)).endFunction = new TemporaryAnimatedSprite.endBehavior(delegate
                    {
                        bs.localSound("batFlap");
                        bs.localSound("busDriveOff");
                    });
                    bs.localSound("trashcanlid");
                    ((TemporaryAnimatedSprite)door.GetValue(bs)).paused = false;

                    for (int i = 11; i < 19; i++)
                    {
                        for (int j = 7; j < 10; j++)
                        {
                            if (i == 12 && j == 9)
                                continue;
                            bs.removeTile(i, j, "Buildings");
                            //bs.setTileProperty(i, j, "Buildings", "Passable", "T");
                        }
                    }

                }
            }
        }

        private void ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            // shuffle for visitors

            RNPCs = RNPCs.OrderBy(n => Guid.NewGuid()).ToList();

            for (int i = 0; i < RNPCs.Count; i++)
            {
                //RNPCs[i].startLoc = "BusStop " + (13 + (i % 6)) + " " + (11 + i / 6);
                if (i < RNPCMaxVisitors)
                {
                    RNPCs[i].visiting = true;
                }
                else
                {
                    RNPCs[i].visiting = false;
                }
                this.Helper.Content.InvalidateCache("Characters/schedules/" + RNPCs[i].nameID);
            }
            this.Helper.Content.InvalidateCache("Data/NPCDispositions");
            this.Helper.Content.InvalidateCache("Data/Quests");
        }

        private void DayEnding(object sender, DayEndingEventArgs e)
        {

            // reset schedules

            RNPCSchedules = new List<RNPCSchedule>();

            // reset quests and quest items

            this.Helper.Content.InvalidateCache("Data/Quests");
            foreach (RNPC rnpc in RNPCs)
            {
                foreach(Quest quest in Game1.player.questLog)
                {
                    if(quest.id == rnpc.npcID + 4200)
                    {
                        Game1.player.removeQuest(quest.id);
                    }
                }
                rnpc.questItem = GetRandomQuestItem(rnpc);
            }

            //reset bus

            drivingOff = false;
            droveOff = false;
            BusStop bs = (BusStop)Game1.getLocationFromName("BusStop");
            Layer layer = bs.map.GetLayer("Buildings");
            for (int i = 11; i < 19; i++)
            {
                for (int j = 8; j < 10; j++)
                {
                    if (i == 12 && j == 9)
                        continue;

                    TileSheet tilesheet = bs.map.GetTileSheet("outdoors");
                    int index = j == 8 ? 1056 : 1054;
                    layer.Tiles[i, j] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: index);
                }
            }

            // shuffle for visitors
            RNPCs = RNPCs.OrderBy(n => Guid.NewGuid()).ToList();

            for (int i = 0; i < RNPCs.Count; i++)
            {
                //RNPCs[i].startLoc = "BusStop " + (13 + (i % 6)) + " " + (11 + i / 6);
                if (i < RNPCMaxVisitors)
                {
                    RNPCs[i].visiting = true;
                }
                else
                {
                    RNPCs[i].visiting = false;
                }
                this.Helper.Content.InvalidateCache("Characters/schedules/" + RNPCs[i].nameID);
                this.Helper.Content.InvalidateCache("Characters/" + RNPCs[i].nameID);
                this.Helper.Content.InvalidateCache("Portraits/" + RNPCs[i].nameID);
            }
            this.Helper.Content.InvalidateCache("Data/NPCDispositions");
        }

        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            foreach (GameLocation l in Game1.locations)
            {
                if (l.GetType() == typeof(BusStop))
                {
                    foreach (NPC npc in l.getCharacters())
                    {
                        for (int i = 0; i < RNPCs.Count; i++)
                        {
                            RNPC rnpc = RNPCs[i];
                            if (rnpc.nameID == npc.name)
                            {
                                npc.willDestroyObjectsUnderfoot = Config.DestroyObjectsUnderfoot;
                                if (rnpc.visiting)
                                {
                                    string[] startLoc = rnpc.startLoc.Split(' ');
                                    Game1.warpCharacter(npc, "BusStop", new Vector2(int.Parse(startLoc[1]), int.Parse(startLoc[2])));
                                    l.getCharacterFromName(npc.name).faceDirection(2);
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Data/NPCDispositions") || asset.AssetNameEquals("Data/NPCGiftTastes") || asset.AssetNameEquals("Characters/EngagementDialogue") || asset.AssetNameEquals("Data/Quests"))
            {
                //base.Monitor.Log("Can load: " + asset.AssetName, LogLevel.Alert);
                return true;
            }

            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/NPCDispositions"))
            {

                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                int idx = 0;
                for (int i = 0; i < RNPCs.Count; i++)
                {
                    data[RNPCs[i].nameID] = RNPCs[i].MakeDisposition(idx++);
                }
            }
            else if (asset.AssetNameEquals("Data/NPCGiftTastes"))
            {

                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                foreach (RNPC npc in RNPCs)
                {
                    data[npc.nameID] = MakeGiftDialogue(npc);
                }
            }
            else if (asset.AssetNameEquals("Characters/EngagementDialogue"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                foreach (RNPC npc in RNPCs)
                {
                    string[] str = MakeEngagementDialogue(npc);
                    data[npc.nameID + "0"] = str[0];
                    data[npc.nameID + "1"] = str[1];
                }
            }
            else if (asset.AssetNameEquals("Data/Quests"))
            {
                IDictionary<int, string> data = asset.AsDictionary<int, string>().Data;
                
                foreach (RNPC npc in RNPCs)
                {
                    string str = MakeQuest(npc);
                    data[npc.npcID+4200] = str;
                }
            }
        }

        private string MakeQuest(RNPC npc)
        {
            string questInfo = GetRandomDialogue(npc, RNPCdialogueData.quiz_quest);

            int item = npc.questItem;

            string itemName = Game1.objectInformation[item].Split('/')[0];

            questInfo = questInfo.Replace("$name",npc.name).Replace("$what",itemName);
            string[] qia = questInfo.Split('|');
            string output = "ItemDelivery/"+qia[0]+"/" + qia[0]+ ": " + Game1.objectInformation[item].Split('/')[5] + "/"+qia[1]+"/" + npc.nameID + " " + item + "/-1/"+Config.QuestReward+"/-1/true/"+ qia[2];
            //Alert("Creating quest "+(npc.npcID+4200)+": "+output); 
            return output;
        }

        private string[] MakeEngagementDialogue(RNPC npc)
        {
            List<string[]> potentialDialogues = new List<string[]>();
            List<string> potentialDialogue = new List<string>();
            potentialDialogue = GetHighestRankedStrings(npc.npcString, RNPCengagementDialogueStrings.data, 7);
            for (int j = 0; j < potentialDialogue.Count; j++)
            {
                potentialDialogues.Add(potentialDialogue[j].Split('/'));
            }

            string[] output = potentialDialogues[Game1.random.Next(0, potentialDialogues.Count)];
            return output;
        }

        private string MakeGiftDialogue(RNPC npc)
        {
            List<string> potentialDialogue = GetHighestRankedStrings(npc.npcString, RNPCgiftDialogueStrings.data, 7);
            for (int j = 0; j < potentialDialogue.Count; j++)
            {
                string str = potentialDialogue[j];
                string[] tastes = str.Split('^');
                for (int i = 0; i < tastes.Length; i++)
                {
                    tastes[i] += "/" + npc.giftTaste[i];
                }
                potentialDialogue[j] = String.Join("/", tastes) + "/";
            }

            string d = potentialDialogue[Game1.random.Next(0, potentialDialogue.Count)];
            Monitor.Log($"{npc.nameID} gift taste: {d}");
            return d;
        }


        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            foreach (RNPC npc in RNPCs)
            {
                if (asset.AssetNameEquals("Portraits/" + npc.nameID) || asset.AssetNameEquals("Characters/" + npc.nameID) || asset.AssetNameEquals("Characters/Dialogue/" + npc.nameID) || asset.AssetNameEquals("Characters/schedules/" + npc.nameID))
                {
                    return true;
                }

            }

            return false;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Texture2D transparentP = this.Helper.Content.Load<Texture2D>("assets/transparent_portrait.png");
            Texture2D transparentC = this.Helper.Content.Load<Texture2D>("assets/transparent_character.png");
            foreach (RNPC npc in RNPCs)
            {
                if (asset.AssetNameEquals("Portraits/" + npc.nameID))
                {
                    if (!npc.visiting)
                    {
                        return (T)(object)transparentP;
                    }
                    Texture2D texture = CreateCustomCharacter(npc, "portrait");

                    return (T)(object)texture;
                }
                else if (asset.AssetNameEquals("Characters/" + npc.nameID))
                {
                    if (!npc.visiting)
                    {
                        return (T)(object)transparentC;
                    }
                    Texture2D texture = CreateCustomCharacter(npc, "character");

                    return (T)(object)texture;
                }
                else if (asset.AssetNameEquals("Characters/schedules/" + npc.nameID))
                {
                    return (T)(object)MakeSchedule(npc);
                }
                else if (asset.AssetNameEquals("Characters/Dialogue/" + npc.nameID))
                {
                    return (T)(object)MakeDialogue(npc);
                }

            }

            throw new InvalidOperationException($"Unexpected asset '{asset.AssetName}'.");
        }

        private Dictionary<string, string> MakeDialogue(RNPC rnpc)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            List<string> intros = GetHighestRankedStrings(rnpc.npcString, RNPCdialogueData.introductions, 7);
            data.Add("Introduction", intros[Game1.random.Next(0, intros.Count)]);

            // make dialogue

            string[] dow = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

            NPC npc = Game1.getCharacterFromName(rnpc.nameID);
            int hearts = Game1.player.getFriendshipHeartLevelForNPC(rnpc.nameID);

            if (!Config.RequireHeartsForDialogue)
            {
                hearts = 10;
            }

            string question = GetRandomDialogue(rnpc, RNPCdialogueData.questions);
            List<string> farmerQuestions = RNPCdialogueData.farmer_questions;
            List<string> rejections = GetHighestRankedStrings(rnpc.npcString, RNPCdialogueData.rejections, 7);


            string questionString = "$q 4242 question_asked#" + question;

            int fqi = 0;

            questionString += "#$r 4343 0 fquest_" + (fqi) + "#" + farmerQuestions[fqi];
            if (hearts >= 2) // allow asking about personality
            {
                string manner = GetRandomDialogue(rnpc, RNPCdialogueData.manner);
                string anxiety = GetRandomDialogue(rnpc, RNPCdialogueData.anxiety);
                string optimism = GetRandomDialogue(rnpc, RNPCdialogueData.optimism);

                string infoResponse = GetRandomDialogue(rnpc, RNPCdialogueData.info_responses);

                data.Add("fquest_" + (fqi), infoResponse.Replace("$name", rnpc.name).Replace("$manner", manner).Replace("$anxiety", anxiety).Replace("$optimism", optimism));
            }
            else
            {
                data.Add("fquest_" + (fqi), rejections[Game1.random.Next(0, rejections.Count)]);
            }
            fqi++;

            questionString += "#$r 4343 0 fquest_" + (fqi) + "#" + farmerQuestions[fqi];
            if (hearts >= 4) // allow asking about plans
            {
                string morning = "THIS IS AN ERROR";
                string afternoon = "THIS IS AN ERROR";
                foreach (RNPCSchedule schedule in RNPCSchedules)
                {
                    if (schedule.npc.nameID == rnpc.nameID)
                    {
                        morning = GetRandomDialogue(rnpc, RNPCdialogueData.places[schedule.morningLoc.Split(' ')[0]]);
                        afternoon = GetRandomDialogue(rnpc, RNPCdialogueData.places[schedule.afternoonLoc.Split(' ')[0]]);
                        break;
                    }
                }
                string scheduleDialogue = GetRandomDialogue(rnpc, RNPCdialogueData.schedules);
                data.Add("fquest_" + (fqi), scheduleDialogue.Replace("@", morning).Replace("#", afternoon));
            }
            else
            {
                data.Add("fquest_" + (fqi), rejections[Game1.random.Next(0, rejections.Count)]);
            }
            fqi++;

            questionString += "#$r 4343 0 fquest_" + (fqi) + "#" + farmerQuestions[fqi];
            if (hearts >= 6) // allow asking about advice
            {
                string advice = GetRandomDialogue(rnpc, RNPCdialogueData.advice);
                data.Add("fquest_" + (fqi), advice);
            }
            else
            {
                data.Add("fquest_" + (fqi), rejections[Game1.random.Next(0, rejections.Count)]);
            }
            fqi++;

            questionString += "#$r 4343 0 fquest_" + (fqi) + "#" + farmerQuestions[fqi];
            if (hearts >= 6) // allow asking about help
            {
                int quizType = Game1.random.Next(0, 5);
                string quiz;
                int friendship = 10;
                List<string> quiza = new List<string>();
                List<string> rl = new List<string>();
                string[] ra;
                string[] quizA;
                switch (quizType)
                {
                    case 0:
                        quiz = GetRandomDialogue(rnpc, RNPCdialogueData.quiz_types["where"]);
                        rl = RNPCdialogueData.quiz_where_answers.OrderBy(n => Guid.NewGuid()).ToList();
                        for (int i = 0; i < 4; i++)
                        {
                            ra = rl[i].Split('|');
                            string ra1 = ra[1];
                            foreach (string str in SDVNPCS)
                            {
                                string str1 = str;
                                if (Game1.getCharacterFromName(str) != null)
                                {
                                    str1 = Game1.getCharacterFromName(str).getName();
                                    if (!str.Equals(str1))
                                    {
                                        ra1 = ra1.Replace(str, str1); // replace with display name
                                    }
                                }
                            }
                            if (Game1.getCharacterFromName(ra[0]) != null)
                            {
                                ra[0] = Game1.getCharacterFromName(ra[0]).getName();
                            }
                            if (i == 0) // right answer
                            {
                                quiza.Add(friendship + " quiz_right#" + ra1);
                                quiz = quiz.Replace("$who", ra[0]); // replace name in question
                            }
                            else
                            {
                                if (ra[1] == rl[0].Split('|')[1]) // same answer
                                    continue;
                                quiza.Add("-" + friendship + " quiz_wrong#" + ra[1]);
                            }
                        }
                        break;
                    case 1:
                        quizA = GetRandomDialogue(rnpc, RNPCdialogueData.quiz_types["when"]).Split('|');
                        quiz = quizA[0];
                        rl = RNPCdialogueData.quiz_when_answers.OrderBy(n => Guid.NewGuid()).ToList();

                        bool open = Game1.random.Next(0, 1) == 0;
                        ra = rl[0].Split('|');
                        quiz = quiz.Replace("$where", ra[0]).Replace("$openclose", open ? quizA[1] : quizA[2]); // replace values in question
                        int h = int.Parse(open ? ra[1] : ra[2]);
                        quiza.Add(friendship + " quiz_right#" + quizA[3].Replace("$time", h > 12 ? (h - 12) + quizA[5] : h + quizA[4]));

                        List<int> hours = new List<int>
                        {
                            h
                        };
                        while (quiza.Count < 4)
                        {
                            int rhour;
                            if (open)
                                rhour = Game1.random.Next(6, 12);
                            else
                                rhour = Game1.random.Next(15, 23);

                            if (hours.Contains(rhour)) // don't duplicate
                                continue;

                            hours.Add(rhour);

                            quiza.Add("-" + friendship + " quiz_wrong#" + quizA[3].Replace("$time", rhour > 12 ? (rhour - 12) + quizA[5] : rhour + quizA[4]));
                        }

                        break;
                    case 2:
                        quizA = GetRandomDialogue(rnpc, RNPCdialogueData.quiz_types["howmuch"]).Split('|');
                        quiz = quizA[0];

                        string item = RNPCdialogueData.quiz_howmuch_items[Game1.random.Next(0, RNPCdialogueData.quiz_howmuch_items.Count)];
                        int where = Game1.random.Next(0, 1);
                        ra = item.Split('|');
                        quiza.Add(friendship + " quiz_right#" + quizA[1].Replace("$gold", ra[1 + where]));
                        quiz = quiz.Replace("$what", ra[0]); // replace name in question
                        quiz = quiz.Replace("$where", RNPCdialogueData.quiz_howmuch_places[where]); // replace name in question

                        int price = int.Parse(ra[1 + where]);
                        List<int> prices = new List<int>
                        {
                            price
                        };
                        int price1 = int.Parse(ra[where == 1 ? 1 : 2]);
                        prices.Add(price1);

                        quiza.Add("-" + friendship + " quiz_wrong#" + quizA[1].Replace("$gold", price1.ToString()));

                        while (quiza.Count < 4)
                        {
                            int rprice = int.Parse(RNPCdialogueData.quiz_howmuch_items[Game1.random.Next(0, RNPCdialogueData.quiz_howmuch_items.Count)].Split('|')[1]);
                            if (prices.Contains(rprice))
                                continue;
                            prices.Add(rprice);

                            quiza.Add("-" + friendship + " quiz_wrong#" + quizA[1].Replace("$gold", rprice.ToString()));
                        }

                        break;
                    case 3:
                        quizA = GetRandomDialogue(rnpc, RNPCdialogueData.quiz_types["who"]).Split('|');
                        quiz = quizA[0];

                        string who = RNPCdialogueData.quiz_who_answers[Game1.random.Next(0, RNPCdialogueData.quiz_who_answers.Count)];
                        ra = who.Split('|');

                        quiza.Add(friendship + " quiz_right#" + quizA[1].Replace("$what", ra[0]).Replace("$who", Game1.getCharacterFromName(ra[1]).getName()));
                        quiz = quiz.Replace("$what", ra[0]); // replace name in question

                        List<string> people = new List<string>
                        {
                            ra[1]
                        };

                        while (quiza.Count < 4)
                        {
                            string rname = SDVNPCS[Game1.random.Next(0, SDVNPCS.Length)];
                            if (people.Contains(rname))
                                continue;
                            people.Add(rname);
                            if (Game1.getCharacterFromName(rname) != null)
                            {
                                rname = Game1.getCharacterFromName(rname).getName();
                            }
                            quiza.Add("-" + friendship + " quiz_wrong#" + quizA[1].Replace("$what", ra[0]).Replace("$who", rname));
                        }

                        break;
                    case 4:
                    default:
                        string tmp = GetRandomDialogue(rnpc, RNPCdialogueData.quiz_types["quest"]);
                        string[] quiz4 = tmp.Replace("$what", Game1.objectInformation[rnpc.questItem].Split('/')[0]).Split('|');
                        quiz = quiz4[0];

                        quiza.Add("2 accept_npc_quest_"+(rnpc.npcID+4200)+"#" + quiz4[1]);
                        quiza.Add("0 decline_npc_quest#" + quiz4[2]);
                        data.Add("accept_npc_quest_" + (rnpc.npcID + 4200), quiz4[3]);
                        data.Add("decline_npc_quest", quiz4[4]);
                        break;
                }


                string quizq = "$q 4242 questq_answered#" + quiz;
                string quizRight = GetRandomDialogue(rnpc, RNPCdialogueData.quizRight);
                string quizWrong = GetRandomDialogue(rnpc, RNPCdialogueData.quizWrong);
                string quizUnknown = GetRandomDialogue(rnpc, RNPCdialogueData.quizUnknown);

                if(quizType != 4)
                {
                    quiza = quiza.OrderBy(n => Guid.NewGuid()).ToList();
                    quiza.Add("0 quiz_unknown#" + RNPCdialogueData.quiz_dont_know);
                }

                for (int i = 0; i < quiza.Count; i++)
                {
                    quizq += "#$r 4343 " + quiza[i];
                }

                Alert("NPC quiz: fquest_"+quizq);

                data.Add("fquest_" + (fqi), quizq);

                data.Add("quiz_right", quizRight);
                data.Add("quiz_wrong", quizWrong);
                data.Add("quiz_unknown", quizUnknown);
            }
            else
            {
                data.Add("fquest_" + (fqi), rejections[Game1.random.Next(0, rejections.Count)]);
            }
            fqi++;

            if (!npc.datingFarmer)
            {
                questionString += "#$r 4343 0 fquest_" + (fqi) + "#" + farmerQuestions[fqi];
                if (hearts >= 8) // allow asking about datability
                {
                    string datable = GetRandomDialogue(rnpc, RNPCdialogueData.datable);

                    data.Add("fquest_" + (fqi), npc.datable ? datable.Split('^')[0] : datable.Split('^')[1]);
                }
                else
                {
                    data.Add("fquest_" + (fqi), rejections[Game1.random.Next(0, rejections.Count)]);
                }
            }
            /*
            fqi++;
            questionString += "#$r 4343 0 fquest_" + (fqi) + "#" + farmerQuestions[fqi];
            data.Add("fquest_" + (fqi), "...");
            */
            //base.Monitor.Log(questionString, LogLevel.Alert);

            foreach (string d in dow)
            {
                data.Add(d, questionString);
            }

            return data;
        }

        private void Alert(string alert)
        {
            Monitor.Log(alert);
        }

        private string GetRandomDialogue(RNPC rnpc, List<string> dialogues)
        {
            List<string> potStrings = GetHighestRankedStrings(rnpc.npcString, dialogues, 7);
            return potStrings[Game1.random.Next(0, potStrings.Count)];
        }

        private Dictionary<string, string> MakeSchedule(RNPC npc)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            RNPCSchedule schedule = new RNPCSchedule(npc);

            if (!npc.visiting)
            {
                //base.Monitor.Log(npc.nameID + " at "+ npc.startLoc+" is not visiting", LogLevel.Alert);

                data.Add("spring", "");
                return data;
            }

            string[] morning = MakeRandomAppointment(npc, "morning");
            string[] afternoon = MakeRandomAppointment(npc, schedule.morningLoc);
            if (morning.Length != 2 || afternoon.Length != 2)
            {
                data.Add("spring", "");
                return data;
            }
            schedule.morningEarliest = morning[0];
            schedule.morningLoc = morning[1];
            schedule.afternoonEarliest = afternoon[0];
            schedule.afternoonLoc = afternoon[1];

            RNPCSchedules.Add(schedule);

            string sstr = schedule.MakeString();

            //Alert(npc.nameID + " at " + npc.startLoc + " has schedule: " + sstr);

            data.Add("spring", sstr);
            return data;
        }

        private string[] MakeRandomAppointment(RNPC npc, string morning)
        {
            List<string[]> potentialApps = new List<string[]>();
            List<string> fitApps = GetHighestRankedStrings(npc.npcString, RNPCscheduleStrings.data, 7);

            foreach (string appset in fitApps)
            {
                string time = appset.Split('^')[0];
                string place = appset.Split('^')[1];
                string[] locs = appset.Split('^')[2].Split('#');
                if (time == "any" || morning != "morning" || int.Parse(time) < 1100)
                {
                    foreach (string loc in locs)
                    {
                        string thisApp = place + " " + loc;
                        if (thisApp != morning)
                        {
                            bool taken = false;
                            foreach (RNPCSchedule schedule in RNPCSchedules)
                            {
                                if ((morning == "morning" && schedule.morningLoc == thisApp) || (morning != "morning" && schedule.afternoonLoc == thisApp))
                                {
                                    taken = true;
                                    break;
                                }
                            }
                            if (!taken)
                            {
                                potentialApps.Add(new string[] { time, thisApp });
                            }
                        }
                    }
                }
            }
            if(potentialApps.Count == 0)
            {
                Monitor.Log("No available schedule for " + npc.nameID, LogLevel.Warn);
                return new string[0];
            }
            return potentialApps[Game1.random.Next(0, potentialApps.Count)];

        }

        private Texture2D CreateCustomCharacter(RNPC npc, string type)
        {
            Texture2D sprite = this.Helper.Content.Load<Texture2D>("assets/body/" + npc.bodyType + "_" + type + ".png", ContentSource.ModFolder);
            Texture2D hairT = this.Helper.Content.Load<Texture2D>("assets/hair/" + npc.hairStyle + "_" + type + ".png", ContentSource.ModFolder);
            Texture2D eyeT = this.Helper.Content.Load<Texture2D>("assets/body/" + npc.gender + "_eyes_" + type + ".png", ContentSource.ModFolder);

            Texture2D eyeBackT = null;
            Texture2D noseT = null;
            Texture2D mouthT = null;
            if (type == "portrait")
            {
                eyeBackT = this.Helper.Content.Load<Texture2D>("assets/body/" + npc.gender + "_eyes_back.png", ContentSource.ModFolder);
                noseT = this.Helper.Content.Load<Texture2D>("assets/body/" + npc.gender + "_nose.png", ContentSource.ModFolder);
                mouthT = this.Helper.Content.Load<Texture2D>("assets/body/" + npc.gender + "_mouth.png", ContentSource.ModFolder);
            }
            Texture2D topT = this.Helper.Content.Load<Texture2D>("assets/transparent_" + type + ".png", ContentSource.ModFolder);
            Texture2D bottomT = topT;
            Texture2D shoesT = topT;

            // clothes

            // try and share with other type (char/portrait)
            string[] clothes;
            if (npc.clothes != null)
            {
                clothes = npc.clothes;
            }
            else
            {
                string npcString = string.Join("/", npc.npcString.Split('/').Take(7)) + "/" + npc.bodyType;
                List<string> potentialClothes = GetHighestRankedStrings(npcString, RNPCclothes.data, 8);

                clothes = potentialClothes[Game1.random.Next(0, potentialClothes.Count)].Split('^');
                //base.Monitor.Log(string.Join(" | ", clothes), LogLevel.Debug);
                npc.clothes = clothes;
                npc.topRandomColour = new string[] { Game1.random.Next(0, 255).ToString(), Game1.random.Next(0, 255).ToString(), Game1.random.Next(0, 255).ToString() };
            }

            if (clothes[0] != "")
            {
                topT = this.Helper.Content.Load<Texture2D>("assets/clothes/" + clothes[0] + "_" + type + ".png", ContentSource.ModFolder);
            }
            if (clothes[1] != "" && type == "character")
            {
                bottomT = this.Helper.Content.Load<Texture2D>("assets/clothes/" + clothes[1] + ".png", ContentSource.ModFolder);
            }
            if (clothes[2] != "" && type == "character")
            {
                shoesT = this.Helper.Content.Load<Texture2D>("assets/clothes/" + clothes[2] + ".png", ContentSource.ModFolder);
            }

            Color[] data = new Color[sprite.Width * sprite.Height];
            Color[] dataH = new Color[hairT.Width * hairT.Height];
            Color[] dataE = new Color[eyeT.Width * eyeT.Height];
            Color[] dataEB = null;
            Color[] dataN = null;
            Color[] dataM = null;
            if (type == "portrait")
            {
                dataEB = new Color[eyeBackT.Width * eyeBackT.Height];
                dataN = new Color[noseT.Width * noseT.Height];
                dataM = new Color[mouthT.Width * mouthT.Height];
            }
            Color[] dataT = new Color[topT.Width * topT.Height];
            Color[] dataB = new Color[bottomT.Width * bottomT.Height];
            Color[] dataS = new Color[shoesT.Width * shoesT.Height];
            sprite.GetData(data);
            hairT.GetData(dataH);
            eyeT.GetData(dataE);
            if (type == "portrait")
            {
                eyeBackT.GetData(dataEB);
                noseT.GetData(dataN);
                mouthT.GetData(dataM);
            }
            topT.GetData(dataT);
            bottomT.GetData(dataB);
            shoesT.GetData(dataS);

            string[] skinRBG = npc.skinColour.Split(' ');
            string[] eyeRBG = npc.eyeColour.Split(' ');
            List<string> hairRBGs = npc.hairColour.Split('^').ToList();

            string[] baseColourT = clothes[3] == "any" ? npc.topRandomColour : null;

            string[] baseColourB;
            switch (clothes[4])
            {
                case "any":
                    baseColourB = new string[] { Game1.random.Next(0, 255).ToString(), Game1.random.Next(0, 255).ToString(), Game1.random.Next(0, 255).ToString() };
                    break;
                case "top":
                    baseColourB = baseColourT;
                    break;
                default:
                    baseColourB = null;
                    break;
            }
            string[] baseColourS;
            switch (clothes[5])
            {
                case "any":
                    baseColourS = new string[] { Game1.random.Next(0, 255).ToString(), Game1.random.Next(0, 255).ToString(), Game1.random.Next(0, 255).ToString() };
                    break;
                case "top":
                    baseColourS = baseColourT;
                    break;
                case "bottom":
                    baseColourS = baseColourB;
                    break;
                default:
                    baseColourS = null;
                    break;
            }

            // make hair gradient

            List<string> hairGreyStrings = new List<string>();
            for (int i = 0; i < dataH.Length; i++)
            {
                if (dataH[i].R == dataH[i].G && dataH[i].R == dataH[i].B && dataH[i].G == dataH[i].B) // greyscale
                {
                    if (!hairGreyStrings.Contains(dataH[i].R.ToString()))
                    { // only add one of each grey
                        hairGreyStrings.Add(dataH[i].R.ToString());
                    }
                }
            }

            // make same number of greys as colours in gradient

            if (hairRBGs.Count > hairGreyStrings.Count) // ex 9 and 6
            {
                hairGreyStrings = LengthenToMatch(hairGreyStrings, hairRBGs);
            }
            else if (hairRBGs.Count < hairGreyStrings.Count)
            {
                hairRBGs = LengthenToMatch(hairRBGs, hairGreyStrings);

            }
            List<byte> hairGreys = new List<byte>();
            foreach (string str in hairGreyStrings)
            {
                hairGreys.Add(byte.Parse(str));
            }
            hairGreys.Sort();
            hairGreys.Reverse(); // lightest to darkest
            //Alert(hairGreys.Count+ " " +hairRBGs.Count);
            
            // start putting it together

            for (int i = 0; i < data.Length; i++)
            {
                if (dataH.Length > i && dataH[i] != Color.Transparent)
                {
                    if (dataH[i].R == dataH[i].G && dataH[i].R == dataH[i].B && dataH[i].G == dataH[i].B) // greyscale
                    {
                        // hair gradient

                        // for cases where fewer greys than colours (multiple of same grey)
                        List<int> greyMatches = new List<int>();
                        for (int j = 0; j < hairGreys.Count; j++)
                        {
                            if (hairGreys[j] == dataH[i].R)
                            {
                                greyMatches.Add(j);
                            }
                        }

                        string[] hairRBG;
                        int rnd = Game1.random.Next(0, greyMatches.Count);
                        int match = greyMatches[rnd];
                        hairRBG = hairRBGs[match].Split(' '); // turns single grey into set of colours

                        data[i] = new Color(byte.Parse(hairRBG[0]), byte.Parse(hairRBG[1]), byte.Parse(hairRBG[2]), dataH[i].A);
                    }
                    else // ignore already coloured parts
                    {
                        data[i] = dataH[i];
                    }
                }
                else if (dataT.Length > i && dataT[i] != Color.Transparent)
                {
                    data[i] = baseColourT != null ? ColorizeGrey(baseColourT, dataT[i]) : dataT[i];
                }
                else if (dataB.Length > i && dataB[i] != Color.Transparent)
                {
                    data[i] = baseColourB != null ? ColorizeGrey(baseColourB, dataB[i]) : dataB[i];
                }
                else if (dataS.Length > i && dataS[i] != Color.Transparent)
                {
                    data[i] = baseColourS != null ? ColorizeGrey(baseColourS, dataS[i]) : dataS[i];
                }
                else if (dataE.Length > i && dataE[i] != Color.Transparent)
                {
                    if (dataE[i] != Color.White)
                    {
                        data[i] = ColorizeGrey(eyeRBG, dataE[i]);
                    }
                    else
                    {
                        data[i] = Color.White;
                    }
                }
                else if (type == "portrait" && dataEB.Length > i && dataEB[i] != Color.Transparent)
                {
                    data[i] = ColorizeGrey(skinRBG, dataEB[i]);
                }
                else if (type == "portrait" && dataN.Length > i && dataN[i] != Color.Transparent)
                {
                    data[i] = ColorizeGrey(skinRBG, dataN[i]);
                }
                else if (type == "portrait" && dataM.Length > i && dataM[i] != Color.Transparent)
                {
                    if (dataM[i] != Color.White)
                    {
                        data[i] = ColorizeGrey(skinRBG, dataM[i]);
                    }
                    else
                    {
                        data[i] = Color.White;
                    }
                }
                else if (data[i] != Color.Transparent)
                {
                    data[i] = ColorizeGrey(skinRBG, data[i]);
                }
            }
            sprite.SetData<Color>(data);
            return sprite;
        }

        private Color ColorizeGrey(string[] baseColour, Color greyMap)
        {
            if (greyMap.R != greyMap.G || greyMap.R != greyMap.B || greyMap.G != greyMap.B) // not greyscale
            {
                return greyMap;
            }
            //base.Monitor.Log(string.Join("", baseColour), LogLevel.Alert);
            Color outColour = new Color
            {
                R = (byte)(greyMap.R - Math.Round((255 - double.Parse(baseColour[0])) * greyMap.R / 255)),
                G = (byte)(greyMap.G - Math.Round((255 - double.Parse(baseColour[1])) * greyMap.G / 255)),
                B = (byte)(greyMap.B - Math.Round((255 - double.Parse(baseColour[2])) * greyMap.B / 255)),
                A = greyMap.A
            };
            return outColour;
        }

        private string GenerateNPCString()
        {
            string npcstring = "";

            // age
            //string[] ages = { "child", "teen", "adult" };
            string[] ages = { "teen", "adult" };
            string age = GetRandomFromDist(ages, Config.AgeDist);

            // manners
            string[] manners = { "polite", "rude", "neutral" };
            string manner = manners[Game1.random.Next(0, manners.Length)];

            // social anxiety
            string[] anxieties = { "outgoing", "shy", "neutral" };
            string anxiety = anxieties[Game1.random.Next(0, anxieties.Length)];

            // optimism
            string[] optimisms = { "positive", "negative", "neutral" };
            string optimism = optimisms[Game1.random.Next(0, optimisms.Length)];

            // gender
            double female = Config.FemaleChance;

            string gender = Game1.random.NextDouble() < female ? "female" : "male";

            // datable
            double datableChance = Config.DatableChance;
            string datable = Game1.random.NextDouble() < datableChance ? "datable" : "non-datable";

            // traits
            string traits = "none"; // not used yet

            // birthday
            string[] seasons = { "spring", "summer", "fall", "winter" };
            string season = seasons[Game1.random.Next(0, seasons.Length)];
            int day = Game1.random.Next(1, 29);
            string birthday = season + " " + day;

            //name

            string name = "";
            bool freename = false;
            while (freename == false)
            {
                string firstName = (gender == "female" ? RNPCfemaleNameStrings.data[Game1.random.Next(0, RNPCfemaleNameStrings.data.Count)] : RNPCmaleNameStrings.data[Game1.random.Next(0, RNPCmaleNameStrings.data.Count)]);

                name = firstName;

                if (RNPCsavedNPCs.data.Count == 0)
                    freename = true;
                bool thisfreename = true;
                for (int i = 0; i < RNPCsavedNPCs.data.Count; i++)
                {
                    if (RNPCsavedNPCs.data[i].Split('/')[8] == name)
                    {
                        thisfreename = false;
                        break;
                    }
                }
                foreach (string str in SDVNPCS)
                {
                    if (Game1.getCharacterFromName(str) != null)
                    {
                        if (Game1.getCharacterFromName(str).getName() == name)
                        {
                            thisfreename = false;
                            break;
                        }
                    }
                }
                if (thisfreename)
                    freename = true;
            }
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            name = textInfo.ToTitleCase(name.ToLower());

            // gift taste

            string giftTaste = "^^^^";

            // body type

            List<string> potentialBodyTypes = new List<string>();
            foreach (string body in RNPCbodyTypes.data)
            {
                string[] ba = body.Split('/');
                if ((ba[0] == "any" || ba[0] == age || ba[0].Split('|').Contains(age)) && (ba[1] == "any" || ba[1] == gender || ba[1].Split('|').Contains(gender)))
                {
                    potentialBodyTypes.Add(ba[2]);
                }
            }
            string bodyType = potentialBodyTypes[Game1.random.Next(0, potentialBodyTypes.Count)];

            // skin colour

            string skinColour = (Game1.random.NextDouble() < Config.LightSkinChance ? RNPClightSkinColours.data[Game1.random.Next(0, RNPClightSkinColours.data.Count)] : RNPCdarkSkinColours.data[Game1.random.Next(0, RNPCdarkSkinColours.data.Count)]);

            // hair style

            List<string> potentialHairStyles = new List<string>();
            foreach (string style in RNPChairStyles.data)
            {
                string[] ba = style.Split('/');
                if ((ba[0] == "any" || ba[0] == age || ba[0].Split('|').Contains(age)) && (ba[1] == "any" || ba[1] == gender) && (ba[2] == "any" || ba[2] == manner || ba[2].Split('|').Contains(manner)) && (ba[3] == "any" || SharesItems(traits.Split('|'), ba[3].Split('|'))))
                {
                    potentialHairStyles.Add(ba[4]);
                }
            }
            string hairStyle = potentialHairStyles[Game1.random.Next(0, potentialHairStyles.Count)];

            // hair colour

            string hairColour;
            if (int.Parse(skinColour.Split(' ')[0]) < 150 && Config.DarkSkinDarkHair)
            {
                hairColour = RNPCdarkHairColours.data[Game1.random.Next(0, RNPCdarkHairColours.data.Count)];
            }
            else if (Game1.random.NextDouble() < Config.NaturalHairChance)
            {
                hairColour = RNPCnaturalHairColours.data[Game1.random.Next(0, RNPCnaturalHairColours.data.Count)];
            }
            else
            {
                hairColour = RNPCexoticHairColours.data[Game1.random.Next(0, RNPCexoticHairColours.data.Count)];
            }

            string eyeColour;
            string[] eyeColours = { "green", "blue", "brown" };
            string eyeColourRange = eyeColours[Game1.random.Next(0, eyeColours.Length)];
            int r = 0;
            int g = 255;
            int b = 0;
            switch (eyeColourRange)
            {
                case "green":
                    g = 255;
                    b = Game1.random.Next(0, 200);
                    r = Game1.random.Next(0, b);
                    break;
                case "blue":
                    b = 255;
                    g = Game1.random.Next(0, 200);
                    r = Game1.random.Next(0, g);
                    break;
                case "brown":
                    r = 255;
                    g = Game1.random.Next(100, 150);
                    b = g - 100;
                    break;
                default:
                    break;

            };
            eyeColour = r + " " + g + " " + b;

            npcstring = age + "/" + manner + "/" + anxiety + "/" + optimism + "/" + gender + "/" + datable + "/" + traits + "/" + birthday + "/" + name + "/" + giftTaste + "/" + bodyType + "/" + skinColour + "/" + hairStyle + "/" + hairColour + "/" + eyeColour;

            return npcstring;
        }

        private bool SharesItems(string[] sharing, string[] shares)
        {
            foreach (string s in shares)
            {
                if (!sharing.Contains(s))
                {
                    return false;
                }
            }
            return true;
        }

        private List<string> LengthenToMatch(List<string> smallerL, List<string> largerL)
        {
            // for 10 and 6
            int multMax = (int)Math.Ceiling((double)(largerL.Count - 1) / (double)(smallerL.Count - 1)); // 9/5 = 2, leave last
            int multMin = (int)Math.Floor((double)(largerL.Count - 1) / (double)(smallerL.Count - 1));  // 9/5 = 2
            int multDiff = (largerL.Count - 1) - (smallerL.Count - 1) * multMin;  // 9 - 5*1 = 4 remainder, number of entries that get extra

            // total of 4*2 and 1*1 = 9, extra one will be add as last

            List<string> outList = new List<string>();

            for (int i = 0; i < smallerL.Count - 1; i++)
            {
                int no;
                if (i >= multDiff) // those starting at multDiff(4) get repeated fewer (1), means (0-3)*2=8entries and (4)*1=1entries
                {
                    no = multMin; // 4 gets 1
                }
                else
                {
                    no = multMax; // 0-3 get 2
                }
                outList.Add(smallerL[i]); // always add original before adding modified in between
                for (int j = 1; j < no; j++)
                {
                    if (smallerL[i].Contains(" "))
                    { // is rgb
                        string[] si = smallerL[i].Split(' ');
                        string[] sii = smallerL[i + 1].Split(' ');

                        int[] r = int.Parse(si[0]) > int.Parse(sii[0]) ? new int[] { int.Parse(si[0]), int.Parse(sii[0]), 1 } : new int[] { int.Parse(sii[0]), int.Parse(si[0]), -1 };
                        int[] g = int.Parse(si[1]) > int.Parse(sii[1]) ? new int[] { int.Parse(si[1]), int.Parse(sii[1]), 1 } : new int[] { int.Parse(sii[1]), int.Parse(si[1]), -1 };
                        int[] b = int.Parse(si[2]) > int.Parse(sii[2]) ? new int[] { int.Parse(si[2]), int.Parse(sii[2]), 1 } : new int[] { int.Parse(sii[2]), int.Parse(si[2]), -1 };

                        int rn = int.Parse(si[0]) - r[2] * (r[0] - r[1]) * j / no; // 200 - (200-100)*(0 or 1)/2 = fraction of difference based on which no
                        int gn = int.Parse(si[1]) - g[2] * (g[0] - g[1]) * j / no;
                        int bn = int.Parse(si[2]) - b[2] * (b[0] - b[1]) * j / no;
                        //Alert(smallerL[i]+" | "+ rn + " " + gn + " " + bn +" | " + smallerL[i+1]);
                        outList.Add(rn + " " + gn + " " + bn);
                    }
                    else // is grey
                    {
                        string grey = (float)no / i < 0.5 ? smallerL[i] : smallerL[i + 1];
                        outList.Add(grey);
                    }
                }
            }
            outList.Add(smallerL[smallerL.Count - 1]);
            //Alert(smallerL.Count + " " + largerL.Count);
            //Alert(outList.Count + " " + largerL.Count);
            return outList;
        }


        private List<string> GetHighestRankedStrings(string npcString, List<string> data, int checks)
        {
            List<string> outStrings = new List<string>();
            int rank = 0;
            foreach (string str in data)
            {
                int aRank = RankStringForNPC(npcString, str, checks);
                if (aRank > rank)
                {
                    outStrings = new List<string>(); // reset on higher rank
                    rank = aRank;
                }
                if (aRank == rank)
                {
                    outStrings.Add(string.Join("/", str.Split('/').Skip(checks)));
                }

            }
            return outStrings;
        }


        private int RankStringForNPC(string npcString, string str, int checks)
        {
            int rank = 0;

            IEnumerable<string> stra = str.Split('/').Take(checks);
            IEnumerable<string> npca = npcString.Split('/').Take(checks);
            for (int i = 0; i < checks; i++)
            {
                if (stra.Count() == i)
                {
                    break;
                }
                string strai = stra.ElementAt(i);
                string npcai = npca.ElementAt(i);
                if (strai != "any")
                {
                    List<string> straia = strai.Split('|').ToList();
                    if (strai != "" && strai != npcai && !straia.Contains(npcai))
                    {
                        return -1;
                    }
                    rank++;
                }
            }
            return rank;
        }
        private string GetRandomFromDist(string[] strings, double[] dists)
        {
            double rnd = Game1.random.NextDouble();
            double x = 0;
            for (int i = 0; i < strings.Length; i++)
            {
                if (rnd < x + dists[i])
                {
                    return strings[i];
                }
                else
                {
                    x += dists[i];
                }
            }
            return "";
        }

        private void LogColors()
        {
            for (int j = 1; j < 13; j++)
            {
                Texture2D sprite = this.Helper.Content.Load<Texture2D>("assets/work/hairs/" + j + ".png");
                Color[] data = new Color[sprite.Width * sprite.Height];
                sprite.GetData(data);
                Dictionary<int,string> tempList = new Dictionary<int,string>();
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] != Color.Transparent)
                    {
                        int total = (int)data[i].R + (int)data[i].G + (int)data[i].B;
                        string c = string.Join(" ", new string[] { data[i].R.ToString(), data[i].G.ToString(), data[i].B.ToString() });
                        if (!tempList.ContainsKey(total))
                        {
                            tempList.Add(total,c);
                        }
                        else if(tempList[total] != c)
                        {
                            while (tempList.ContainsKey(total))
                            {
                                total++;
                                if (!tempList.ContainsKey(total))
                                {
                                    tempList.Add(total, c);
                                }
                                else if (tempList[total] == c)
                                    break;
                            }
                        }
                    }

                }
                var keys = tempList.Keys.ToList();
                keys.Sort();
                keys.Reverse();

                var outList = new List<string>();
                foreach(var key in keys)
                {
                    outList.Add(tempList[key]);
                }
                Alert(string.Join("^", outList));
            }
        }
    }
}