using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DialogueTrees
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static ModConfig Config;
        private static Random myRand;
        private static Dictionary<string, DialogueTree> loadedDialogues;
        private static Dictionary<string, string> loadedQuestionStrings;
        private static Dictionary<string, string> loadedResponseStrings;
        private static Dictionary<string, string> loadedTopicNames;
        private static DialogueTreeResponse responseData;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.Enabled)
                return;

            myRand = new Random();

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed; ;


        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {

            // check for followup dialogue

            if (Game1.activeClickableMenu == null && responseData != null)
            {
                DialogueTree p = responseData.nextTopic;

                Monitor.Log($"Asking about followup topic {loadedTopicNames[p.topicID]}");

                Dictionary<string, string> npcDic = Helper.Content.Load<Dictionary<string, string>>($"Characters/Dialogue/{responseData.npc.name}", ContentSource.GameContent);
                List<string> npcKeys = npcDic.Keys.ToList();

                List<Response> responses = new List<Response>();
                foreach (var r in p.responseIDs)
                {
                    Monitor.Log(r);
                    responses.Add(new Response($"DialogueTrees_Response#{responseData.npc.Name}#{p.topicID}#{r}", string.Format(loadedResponseStrings[r], loadedTopicNames[p.topicID])));
                }
                string qid = p.questionIDs[myRand.Next(p.questionIDs.Count)];
                List<string> possibleQuestionStrings = npcDic.Keys.ToList().FindAll(k => k.StartsWith("DialogueTrees_question_" + qid + "_"));
                string questionString = possibleQuestionStrings.Any() ? npcDic[possibleQuestionStrings[myRand.Next(possibleQuestionStrings.Count)]] : loadedQuestionStrings[qid];

                Game1.player.currentLocation.createQuestionDialogue(string.Format(questionString, loadedTopicNames[p.topicID]), responses.ToArray(), "DialogueTrees_NPC_Question");
                Game1.objectDialoguePortraitPerson = responseData.npc;
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.Enabled || !Context.IsWorldReady)
                return;

            Dictionary<string, string> npcDic;
            List<string> npcKeys;

            // check for click on dialogue

            if (Game1.activeClickableMenu != null && Game1.player?.currentLocation?.lastQuestionKey?.StartsWith("DialogueTrees_") == true)
            {
                IClickableMenu menu = Game1.activeClickableMenu;
                if (menu == null || menu.GetType() != typeof(DialogueBox))
                    return;

                DialogueBox db = menu as DialogueBox;
                int resp = db.selectedResponse;
                List<Response> resps = db.responses;

                if (resp < 0 || resps == null || resp >= resps.Count || resps[resp] == null)
                    return;

                string[] parts = resps[resp].responseKey.Split('#');
                string npcName = parts[1];
                string topicID = parts[2];
                string responseID = parts[3];
                npcDic = Helper.Content.Load<Dictionary<string, string>>($"Characters/Dialogue/{npcName}", ContentSource.GameContent);
                npcKeys = npcDic.Keys.ToList();

                if (Game1.player.currentLocation.lastQuestionKey == "DialogueTrees_Player_Question")
                {
                    Monitor.Log($"asking {npcName} about {loadedTopicNames[topicID]}");
                    var possibleResponses = npcKeys.FindAll(k => k.StartsWith("DialogueTrees_response_" + topicID + "_"));
                    NPC n = Game1.getCharacterFromName(npcName);
                    Game1.drawDialogue(n, npcDic[possibleResponses[myRand.Next(possibleResponses.Count)]]);

                    string nextTopicID = GetNextTopicID(topicID, "any");

                    if (nextTopicID != null && loadedDialogues.ContainsKey(nextTopicID) && npcKeys.Exists(k => k.StartsWith("DialogueTrees_response_" + nextTopicID + "_")))
                    {
                        if (responseData != null)
                        {
                            responseData.lastTopic = loadedDialogues[topicID];
                            responseData.nextTopic = loadedDialogues[nextTopicID];
                            responseData.npc = n;
                            responseData.topicResponses[topicID] = responseID;
                        }
                        else
                            responseData = new DialogueTreeResponse(loadedDialogues[topicID], loadedDialogues[nextTopicID], n, responseID);
                    }
                }
                else if(Game1.player.currentLocation.lastQuestionKey == "DialogueTrees_NPC_Question")
                {
                    Monitor.Log($"{npcName} is asking player about {loadedTopicNames[topicID]}, player response: {loadedResponseStrings[responseID]}");

                    var possibleReactions = npcKeys.FindAll(k => k.StartsWith("DialogueTrees_reaction_" + topicID + "_" + responseID + "_"));

                    if (!possibleReactions.Any())
                    {
                        Monitor.Log($"{npcName} has no reaction to {loadedTopicNames[topicID]} response {loadedResponseStrings[responseID]}! Check the [DT] content pack.", LogLevel.Warn);
                    }
                    else
                    {
                        NPC n = Game1.getCharacterFromName(npcName);
                        Game1.drawDialogue(n, npcDic[possibleReactions[myRand.Next(possibleReactions.Count)]]);
                        if (npcDic.ContainsKey("DialogueTrees_friendship_" + topicID + "_" + responseID) && int.TryParse(npcDic["DialogueTrees_friendship_" + topicID + "_" + responseID], out int amount))
                        {
                            Monitor.Log($"changing friendship with {n.Name} by {amount}");
                            Game1.player.changeFriendship(amount, n);
                        }

                        string nextTopicID = GetNextTopicID(topicID, responseID);

                        if (nextTopicID != null && loadedDialogues.ContainsKey(nextTopicID))
                        {
                            Monitor.Log($"Preparing followup dialogue {nextTopicID}");
                            if (responseData != null)
                            {
                                Monitor.Log($"Adding to existing responseData");

                                responseData.lastTopic = loadedDialogues[topicID];
                                responseData.nextTopic = loadedDialogues[nextTopicID];
                                responseData.npc = n;
                                responseData.topicResponses[topicID] = responseID;
                            }
                            else
                                responseData = new DialogueTreeResponse(loadedDialogues[topicID], loadedDialogues[nextTopicID], n, responseID);
                        }
                        else
                        {
                            if(responseData != null)
                                Monitor.Log("No next topic, erasing response data");
                            responseData = null;
                        }
                    }
                }
                
                Game1.player.currentLocation.lastQuestionKey = "";
                return;
            }

            // check for click on NPC

            if (Game1.activeClickableMenu != null || !Context.CanPlayerMove || (Config.ModButton != SButton.None && !Helper.Input.IsDown(Config.ModButton)) || (e.Button != Config.AskButton && e.Button != Config.AnswerButton))
                return;

            Monitor.Log($"Pressed modkey + {e.Button}");

            Rectangle tileRect = new Rectangle((int)Game1.currentCursorTile.X * 64, (int)Game1.currentCursorTile.Y * 64, 64, 64);

            NPC npc = null;

            foreach (NPC i in Game1.currentLocation.characters)
            {
                if (i != null && !i.IsMonster && (!Game1.player.isRidingHorse() || !(i is Horse)) && i.GetBoundingBox().Intersects(tileRect) && !i.IsInvisible && !i.checkAction(Game1.player, Game1.currentLocation))
                {
                    npc = i;
                    break;
                }
            }

            if (npc == null)
                return;
            try
            {
                npcDic = Helper.Content.Load<Dictionary<string, string>>($"Characters/Dialogue/{npc.Name}", ContentSource.GameContent);

            }
            catch
            {
                Monitor.Log($"No dialogue file for {npc.Name}", LogLevel.Warn);

                return;
            }
            npcKeys = npcDic.Keys.ToList();

            if (e.Button == Config.AskButton)
            {

                Monitor.Log($"Asking question of {npc.Name}");

                var shuffled = loadedDialogues.Values.ToList().FindAll(d => d.isStarter && d.playerCanAsk && npcKeys.Exists(k => k.StartsWith("DialogueTrees_response_" + d.topicID+"_")));

                if(!shuffled.Any())
                {
                    Monitor.Log($"No questions that {npc.Name} has a response to! Check the [DT] content pack.", LogLevel.Warn);
                    return;
                }
                Monitor.Log($"{shuffled.Count} questions that {npc.Name} has a response to.");

                ShuffleList(shuffled);
                var questions = shuffled.Take(Config.MaxPlayerQuestions);
                List<Response> responses = new List<Response>();
                foreach(var q in questions)
                {
                    Monitor.Log(q.topicID);
                    responses.Add(new Response($"DialogueTrees_Response#{npc.Name}#{q.topicID}", string.Format(loadedQuestionStrings[q.questionIDs[myRand.Next(q.questionIDs.Count)]], loadedTopicNames[q.topicID])));
                }

                Game1.player.currentLocation.createQuestionDialogue(string.Format(Helper.Translation.Get("ask-npc"), npc.Name), responses.ToArray(), "DialogueTrees_Player_Question");
            }
            else if (e.Button == Config.AnswerButton)
            {
                Monitor.Log($"Answering {npc.Name}'s question");

                var possibleQuestions = loadedDialogues.Values.ToList().FindAll(d => d.isStarter && npcKeys.Exists(k => k.StartsWith("DialogueTrees_reaction_" + d.topicID+"_")));

                if (!possibleQuestions.Any())
                {
                    Monitor.Log($"No questions that {npc.Name} can ask (no reactions)! Check the [DT] content pack.", LogLevel.Warn);
                    return;
                }

                DialogueTree p = possibleQuestions[myRand.Next(possibleQuestions.Count)];

                Monitor.Log($"Asking about {loadedTopicNames[p.topicID]}");

                List<Response> responses = new List<Response>();
                foreach(var r in p.responseIDs)
                {
                    Monitor.Log(r);

                    responses.Add(new Response($"DialogueTrees_Response#{npc.Name}#{p.topicID}#{r}", string.Format(loadedResponseStrings[r], loadedTopicNames[p.topicID])));
                }
                string qid = p.questionIDs[myRand.Next(p.questionIDs.Count)];

                List<string> possibleQuestionStrings = npcDic.Keys.ToList().FindAll(k => k.StartsWith("DialogueTrees_question_" + qid + "_"));
                string questionString = possibleQuestionStrings.Any() ? npcDic[possibleQuestionStrings[myRand.Next(possibleQuestionStrings.Count)]] : loadedQuestionStrings[qid];

                Game1.player.currentLocation.createQuestionDialogue(string.Format(questionString, loadedTopicNames[p.topicID]), responses.ToArray(), "DialogueTrees_NPC_Question");
                Game1.objectDialoguePortraitPerson = npc;
            }
        }

        private string GetNextTopicID(string topicID, string responseID)
        {
            Monitor.Log($"Looking for next topic after {topicID} for response {responseID}");

            DialogueTree ldt = loadedDialogues[topicID];
            List<string> nextTopicIDs;
            if (ldt.nextTopics.ContainsKey("any"))
            {
                nextTopicIDs = ldt.nextTopics["any"];
            }
            else if (ldt.nextTopics.ContainsKey(responseID))
            {
                nextTopicIDs = ldt.nextTopics[responseID];
            }
            else
                return null;

            Monitor.Log($"Got {nextTopicIDs.Count} topics for next topic");

            List<DialogueTree> possibleTopics = new List<DialogueTree>();
            float totalChance = 0;
            foreach (string id in nextTopicIDs)
            {
                if (!loadedDialogues.ContainsKey(id))
                {
                    Monitor.Log($"Invalid topic {id}");
                    continue;
                }

                bool possible = true;
                var ndt = loadedDialogues[id];
                foreach(var kvp in ndt.requiredResponses)
                {
                    if (!kvp.Value.Any())
                        continue;
                    if (kvp.Value[0] == "")
                    {
                        if (responseData != null && responseData.topicResponses.ContainsKey(kvp.Key))
                        {
                            Monitor.Log($"Disallowed topic {id}");
                            possible = false;
                        }
                        break;
                    }
                    if (responseData == null || !responseData.topicResponses.ContainsKey(kvp.Key) || !kvp.Value.Contains(responseData.topicResponses[kvp.Key]))
                    {
                        Monitor.Log($"missing required topic response to {id}. responseData: {responseData != null}, has response to {kvp.Key}: {responseData?.topicResponses.ContainsKey(kvp.Key)}");
                        possible = false;
                        break;
                    }
                }
                if (possible)
                {
                    Monitor.Log($"Adding {ndt.topicID} as possible next topic, chance {ndt.followupChance}");
                    possibleTopics.Add(ndt);
                    totalChance += ndt.followupChance;
                }
            }
            if (!possibleTopics.Any())
            {
                Monitor.Log($"No possible topics");
                return null;
            }
            float addChance = 0;
            double rand = Game1.random.NextDouble();
            foreach(DialogueTree dt in possibleTopics)
            {
                addChance += dt.followupChance;
                if (rand < addChance / totalChance)
                {
                    Monitor.Log($"Choosing {dt.topicID} as next topic");
                    return dt.topicID;
                }
            }
            return null;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            loadedDialogues = new Dictionary<string, DialogueTree>();
            loadedQuestionStrings = new Dictionary<string, string>();
            loadedResponseStrings = new Dictionary<string, string>();
            loadedTopicNames = new Dictionary<string, string>();
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                try
                {
                    int add = 0;
                    Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                    DialogueTreeData json = contentPack.ReadJsonFile<DialogueTreeData>("content.json");
                    foreach (DialogueTree dialogue in json.dialogues)
                    {
                        loadedDialogues.Add(dialogue.topicID, dialogue);
                        add++;
                    }
                    foreach (var kvp in json.standardQuestions)
                    {
                        loadedQuestionStrings[kvp.Key] = kvp.Value;
                    }
                    foreach (var kvp in json.playerResponses)
                    {
                        loadedResponseStrings[kvp.Key] = kvp.Value;
                    }
                    foreach (var kvp in json.topicNames)
                    {
                        loadedTopicNames[kvp.Key] = kvp.Value;
                    }
                    Monitor.Log($"Got {add} dialogues from content pack {contentPack.Manifest.Name}", LogLevel.Debug);
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Error processing content.json in content pack {contentPack.Manifest.Name} {ex}", LogLevel.Error);
                }
            }
            foreach(string t in loadedDialogues.Keys)
            {
                if (!loadedTopicNames.ContainsKey(t))
                    loadedTopicNames[t] = t;
            }
            Monitor.Log($"Got {loadedDialogues.Count} dialogues total", LogLevel.Debug);

            if(Directory.Exists(Path.Combine(Helper.DirectoryPath, "work")))
            {
                foreach(string file in Directory.GetFiles(Path.Combine(Helper.DirectoryPath, "work"), "*.json"))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    if (File.ReadAllText(file).Length > 0)
                        continue;
                    if(name == "content")
                    {
                        Monitor.Log($"Building content.json for [DT] pack");
                        DialogueTreeData dtd = new DialogueTreeData();
                        foreach(DialogueTree dt in loadedDialogues.Values)
                        {
                            foreach(string id in dt.questionIDs)
                            {
                                if(!loadedQuestionStrings.ContainsKey(id))
                                    dtd.standardQuestions[id] = "";
                            }
                            foreach(string id in dt.responseIDs)
                            {
                                dtd.playerResponses[id] = "";
                            }
                        }
                        Helper.Data.WriteJsonFile(Path.Combine("work",Path.GetFileName(file)), dtd);
                    }
                    else
                    {
                        Monitor.Log($"Building {name}.json for [CP] pack");
                        CharacterDataModel cdm = new CharacterDataModel(name);
                        foreach (DialogueTree dt in loadedDialogues.Values)
                        {
                            foreach (string id in dt.questionIDs)
                            {
                                cdm.Changes[0].Entries[$"DialogueTrees_question_{id}"] = "";
                            }
                            cdm.Changes[0].Entries[$"DialogueTrees_response_{dt.topicID}_1"] = "";
                            foreach (string id in dt.responseIDs)
                            {
                                cdm.Changes[0].Entries[$"DialogueTrees_reaction_{dt.topicID}_{id}_1"] = "";
                                cdm.Changes[0].Entries[$"DialogueTrees_friendship_{dt.topicID}_{id}_1"] = "0";
                            }
                        }
                        Helper.Data.WriteJsonFile(Path.Combine("work", Path.GetFileName(file)), cdm);
                    }
                }
            }
        }

        public static void ShuffleList<T>(List<T> list)
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
    }

}