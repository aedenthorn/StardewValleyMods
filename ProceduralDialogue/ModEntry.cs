using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProceduralDialogue
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static ModConfig Config;
        private static IMonitor PMonitor;
        private static Random myRand;
        private static Dictionary<string, ProceduralDialogue> loadedDialogues;
        private static Dictionary<string, string> loadedQuestionStrings;
        private static Dictionary<string, string> loadedResponseStrings;
        private static Dictionary<string, string> loadedTopicNames;
        private static Dictionary<string, string> loadedUIStrings;

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

            PMonitor = Monitor;

            myRand = new Random();

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed; ;

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.Enabled || !Context.IsWorldReady)
                return;

            Dictionary<string, string> npcDic;
            List<string> npcKeys;

            // check for click on dialogue

            if (Game1.activeClickableMenu != null && Game1.player?.currentLocation?.lastQuestionKey?.StartsWith("ProceduralDialogue_") == true)
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

                npcDic = Helper.Content.Load<Dictionary<string, string>>($"Characters/Dialogue/{parts[1]}", ContentSource.GameContent);
                npcKeys = npcDic.Keys.ToList();

                if (Game1.player.currentLocation.lastQuestionKey == "ProceduralDialogue_Player_Question")
                {
                    PMonitor.Log($"asking {parts[1]} about {parts[2]}");
                    var possibleResponses = npcKeys.FindAll(k => k.StartsWith("ProceduralDialogue_response_" + parts[2] + "_"));
                    NPC n = Game1.getCharacterFromName(parts[1]);
                    Game1.drawDialogue(n, npcDic[possibleResponses[myRand.Next(possibleResponses.Count)]]);
                }
                else if(Game1.player.currentLocation.lastQuestionKey == "ProceduralDialogue_NPC_Question")
                {
                    PMonitor.Log($"{parts[1]} is asking player about {loadedTopicNames[parts[2]]}, player response: {loadedResponseStrings[parts[3]]}");

                    var possibleReactions = npcKeys.FindAll(k => k.StartsWith("ProceduralDialogue_reaction_" + parts[2] + "_" + parts[3] + "_"));

                    if (!possibleReactions.Any())
                    {
                        PMonitor.Log($"{parts[1]} has no reaction to {loadedTopicNames[parts[2]]} response {loadedResponseStrings[parts[3]]}! Check the [PD] content pack.", LogLevel.Warn);
                    }
                    else
                    {
                        NPC n = Game1.getCharacterFromName(parts[1]);
                        Game1.drawDialogue(n, npcDic[possibleReactions[myRand.Next(possibleReactions.Count)]]);
                        if (npcDic.ContainsKey("ProceduralDialogue_friendship_" + parts[2] + "_" + parts[3]) && int.TryParse(npcDic["ProceduralDialogue_friendship_" + parts[2] + "_" + parts[3]], out int amount))
                        {
                            PMonitor.Log($"changing friendship with {n.Name} by {amount}");
                            Game1.player.changeFriendship(amount, n);
                        }
                    }
                }
                
                Game1.player.currentLocation.lastQuestionKey = "";
                return;
            }

            // check for click on NPC

            if (Game1.activeClickableMenu != null || !Context.CanPlayerMove || (Config.ModButton != SButton.None && !Helper.Input.IsDown(Config.ModButton)) || (e.Button != Config.AskButton && e.Button != Config.AnswerButton))
                return;

            PMonitor.Log($"Pressed modkey + {e.Button}");

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
                PMonitor.Log($"No dialogue file for {npc.Name}", LogLevel.Warn);

                return;
            }
            npcKeys = npcDic.Keys.ToList();

            if (e.Button == Config.AskButton)
            {

                PMonitor.Log($"Asking question of {npc.Name}");

                var shuffled = loadedDialogues.Values.ToList().FindAll(d => npcKeys.Exists(k => k.StartsWith("ProceduralDialogue_response_" + d.topicID+"_")));

                if(!shuffled.Any())
                {
                    PMonitor.Log($"No questions that {npc.Name} has a response to! Check the [PD] content pack.", LogLevel.Warn);
                    return;
                }

                ShuffleList(shuffled);
                var questions = shuffled.Take(Config.MaxPlayerQuestions);
                List<Response> responses = new List<Response>();
                foreach(var q in questions)
                {
                    PMonitor.Log(q.topicID);
                    responses.Add(new Response($"ProceduralDialogue_Response#{npc.Name}#{q.topicID}", string.Format(loadedQuestionStrings[q.questionIDs[myRand.Next(q.questionIDs.Count)]], loadedTopicNames[q.topicID])));
                }

                Game1.player.currentLocation.createQuestionDialogue(string.Format(loadedUIStrings["player-question-header"], npc.Name), responses.ToArray(), "ProceduralDialogue_Player_Question");
            }
            else if (e.Button == Config.AnswerButton)
            {
                PMonitor.Log($"Answering {npc.Name}'s question");

                var possibleQuestions = loadedDialogues.Values.ToList().FindAll(d => npcKeys.Exists(k => k.StartsWith("ProceduralDialogue_reaction_" + d.topicID+"_")));

                if (!possibleQuestions.Any())
                {
                    PMonitor.Log($"No questions that {npc.Name} can ask (no reactions)! Check the [PD] content pack.", LogLevel.Warn);
                    return;
                }

                ProceduralDialogue p = possibleQuestions[myRand.Next(possibleQuestions.Count)];

                PMonitor.Log($"Asking about {loadedTopicNames[p.topicID]}");

                List<Response> responses = new List<Response>();
                foreach(var r in p.responseIDs)
                {
                    PMonitor.Log(r);

                    responses.Add(new Response($"ProceduralDialogue_Response#{npc.Name}#{p.topicID}#{r}", string.Format(loadedResponseStrings[r], loadedTopicNames[p.topicID])));
                }
                string qid = p.questionIDs[myRand.Next(p.questionIDs.Count)];
                string questionString = npcDic.ContainsKey("ProceduralDialogue_question_" + qid) ? npcDic["ProceduralDialogue_question_" + qid] : loadedQuestionStrings[qid];

                Game1.player.currentLocation.createQuestionDialogue(string.Format(questionString, loadedTopicNames[p.topicID]), responses.ToArray(), "ProceduralDialogue_NPC_Question");
                Game1.objectDialoguePortraitPerson = npc;
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            loadedDialogues = new Dictionary<string, ProceduralDialogue>();
            loadedQuestionStrings = new Dictionary<string, string>();
            loadedResponseStrings = new Dictionary<string, string>();
            loadedTopicNames = new Dictionary<string, string>();
            loadedUIStrings = new Dictionary<string, string>();
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                try
                {
                    int add = 0;
                    Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                    ProceduralDialogueData json = contentPack.ReadJsonFile<ProceduralDialogueData>("content.json");
                    foreach (ProceduralDialogue dialogue in json.dialogues)
                    {
                        loadedDialogues.Add(dialogue.topicID, dialogue);
                        add++;
                    }
                    foreach (var kvp in json.playerQuestions)
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
                    foreach (var kvp in json.UIStrings)
                    {
                        loadedUIStrings[kvp.Key] = kvp.Value;
                    }
                    Monitor.Log($"Got {add} dialogues from content pack {contentPack.Manifest.Name}", LogLevel.Debug);
                }
                catch (Exception ex)
                {
                    PMonitor.Log($"Error processing content.json in content pack {contentPack.Manifest.Name} {ex}", LogLevel.Error);
                }
            }
            Monitor.Log($"Got {loadedDialogues.Count} dialogues total", LogLevel.Debug);
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