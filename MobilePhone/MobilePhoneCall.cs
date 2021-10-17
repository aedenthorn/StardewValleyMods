using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MobilePhone
{
    internal class MobilePhoneCall
    {
        private static IModHelper Helper;
        private static IMonitor Monitor;
        private static ModConfig Config;
        public static List<Reminisce> inCallReminiscence;
        public static Dictionary<string, string> inCallDialogue;
        internal static List<EventInvite> eventInvites = new List<EventInvite>();
        internal static Dictionary<string, Reminiscence> contentPackReminiscences = new Dictionary<string, Reminiscence>();

        public static void Initialize(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            Helper = helper;
            Monitor = monitor;
            Config = config;
        }

        public static void CallDialogueAnswer(Farmer who, string whichAnswer)
        {
            NPC npc = ModEntry.callingNPC;

            Monitor.Log($"Showing Call Dialogue based on answer {whichAnswer}");

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }

            if (whichAnswer == "PhoneApp_InCall_Return")
            {
                ShowMainCallDialogue(npc);
            }
            else if (whichAnswer == "PhoneApp_InCall_Chat")
            {
                ChatOnPhone(npc);
            }
            else if (whichAnswer == "PhoneApp_InCall_Reminisce")
            {
                ReminisceOnPhone(npc);
            }
            else if (whichAnswer == "PhoneApp_InCall_Invite")
            {
                InviteOnPhone(npc);
            }
            else if (whichAnswer == "PhoneApp_InCall_Recruit")
            {
                RecruitOnPhone(npc);
            }
            else if (whichAnswer == "PhoneApp_InCall_Recruit_Yes")
            {
                StartRecruit(npc);
            }
            else if (whichAnswer == "PhoneApp_InCall_No")
            {
                ShowMainCallDialogue(npc);
            }
            else if (whichAnswer == "PhoneApp_InCall_Build")
            {
                BuildOnPhone();
            }
            else if (whichAnswer == "PhoneApp_InCall_Upgrade")
            {
                UpgradeOnPhone(npc);
            }
            else if (whichAnswer == "PhoneApp_InCall_Upgrade_Yes")
            {
                DoUpgrade(npc);
            }
            else if (whichAnswer.StartsWith("PhoneApp_InCall_Reminiscence_"))
            {
                int which = int.Parse(whichAnswer.Substring("PhoneApp_InCall_Reminiscence_".Length));
                DoReminisce(which, npc);
            }
            else if (whichAnswer.StartsWith("PhoneApp_InCall_Invitation_"))
            {
                int which = int.Parse(whichAnswer.Substring("PhoneApp_InCall_Invitation_".Length));
                DoInvite(which, npc);
            }
            else if (whichAnswer == "PhoneApp_InCall_GoodBye")
            {
                Game1.drawDialogue(npc, GetGoodBye(npc));
                EndCall();
            }
        }

        public static void ShowMainCallDialogue(NPC npc)
        {
            Monitor.Log($"Showing Main Call Dialogue");

            ModEntry.buildingInCall = false;

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }

            List<Response> answers = new List<Response>();
            if (npc.CurrentDialogue != null && npc.CurrentDialogue.Count > 0)
                answers.Add(new Response("PhoneApp_InCall_Chat", Helper.Translation.Get("chat")));

            if (inCallReminiscence == null)
            {
                Reminiscence r;
                if (contentPackReminiscences.ContainsKey(npc.Name))
                {
                    r = contentPackReminiscences[npc.Name];
                }
                else
                {
                    r = Helper.Data.ReadJsonFile<Reminiscence>(Path.Combine("assets", "events", $"{npc.Name}.json")) ?? new Reminiscence();
                }
                Monitor.Log($"Total Reminisces: {r.events.Count}");
                r.WeedOutUnseen();
                Monitor.Log($"Seen Reminisces: {r.events.Count}");
                inCallReminiscence = new List<Reminisce>(r.events);
            }
            if (inCallReminiscence != null && inCallReminiscence.Count > 0)
            {
                answers.Add(new Response("PhoneApp_InCall_Reminisce", Helper.Translation.Get("reminisce")));
            }
            if (eventInvites.Count > 0)
            {
                foreach (EventInvite ei in eventInvites)
                {
                    if (ei.CanInvite(npc))
                    {
                        answers.Add(new Response("PhoneApp_InCall_Invite", Helper.Translation.Get("invite")));
                        break;
                    }
                }
            }
            if (ModEntry.npcAdventureModApi != null && ModEntry.npcAdventureModApi.IsPossibleCompanion(npc) && ModEntry.npcAdventureModApi.CanAskToFollow(npc) && !npc.isSleeping.Value)
            {
                answers.Add(new Response("PhoneApp_InCall_Recruit", Helper.Translation.Get("recruit")));
            }
            if (npc.Name == "Robin" && Game1.player.daysUntilHouseUpgrade.Value < 0 && !Game1.getFarm().isThereABuildingUnderConstruction())
            {
                if (Game1.player.HouseUpgradeLevel < 3)
                    answers.Add(new Response("PhoneApp_InCall_Upgrade", Helper.Translation.Get("upgrade-house")));
                answers.Add(new Response("PhoneApp_InCall_Build", Helper.Translation.Get("build-buildings")));
            }
            answers.Add(new Response("PhoneApp_InCall_GoodBye", Helper.Translation.Get("goodbye")));

            Game1.player.currentLocation.createQuestionDialogue(GetCallGreeting(npc), answers.ToArray(), CallDialogueAnswer);
            Game1.objectDialoguePortraitPerson = npc;
        }


        private static async void ChatOnPhone(NPC npc)
        {
            Monitor.Log($"Showing chat dialogue");

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }


            (Game1.activeClickableMenu as DialogueBox).closeDialogue();

            npc.grantConversationFriendship(Game1.player, 20);
            while (npc.CurrentDialogue.Count > 0 && ModEntry.phoneOpen)
            {
                if (!(Game1.activeClickableMenu is DialogueBox))
                {
                    Game1.drawDialogue(npc);
                    Monitor.Log($"Dialogues left {npc.CurrentDialogue.Count}");
                }
                await Task.Delay(50);
            }
            ShowMainCallDialogue(npc);
        }
        private static void ReminisceOnPhone(NPC npc)
        {
            Monitor.Log($"Showing reminisce menu");

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }


            List<Response> responses = new List<Response>();
            for (int i = 0; i < inCallReminiscence.Count; i++)
            {
                string title = inCallReminiscence[i].name;
                if (Helper.Translation.Get(inCallReminiscence[i].name).HasValue())
                    title = Helper.Translation.Get(inCallReminiscence[i].name);
                responses.Add(new Response($"PhoneApp_InCall_Reminiscence_{i}", title));
            }

            responses.Add(new Response("PhoneApp_InCall_Return", Helper.Translation.Get("back")));

            Game1.player.currentLocation.createQuestionDialogue(GetReminiscePrefix(npc), responses.ToArray(), CallDialogueAnswer);
        }
        private static void InviteOnPhone(NPC npc)
        {
            Monitor.Log($"Showing Invite menu");

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }

            List<Response> responses = new List<Response>();
            for (int i = 0; i < eventInvites.Count; i++)
            {
                if (eventInvites[i].CanInvite(npc))
                {
                    responses.Add(new Response($"PhoneApp_InCall_Invitation_{i}", eventInvites[i].name));
                }
            }

            responses.Add(new Response("PhoneApp_InCall_Return", Helper.Translation.Get("back")));

            Game1.player.currentLocation.createQuestionDialogue(GetInvitePrefix(npc), responses.ToArray(), CallDialogueAnswer);
        }
        private static void RecruitOnPhone(NPC npc)
        {
            Monitor.Log($"Showing recruit question");

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }


            Response[] responses = new Response[]
            {
                new Response("PhoneApp_InCall_Recruit_Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")),
                new Response("PhoneApp_InCall_No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No"))
            };
            Game1.player.currentLocation.createQuestionDialogue(ModEntry.npcAdventureModApi.LoadString("Strings/Strings:askToFollow", npc.Name), responses, CallDialogueAnswer);
        }
        private static void BuildOnPhone()
        {
            Monitor.Log($"Showing build menu");

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }
            ModEntry.buildingInCall = true;
            (Game1.activeClickableMenu as DialogueBox).closeDialogue();
            ModEntry.callLocation = Game1.getLocationRequest(Game1.player.currentLocation.Name, false);
            ModEntry.callPosition = Game1.player.position;
            ModEntry.callViewportLocation = Game1.viewport.Location;
            Game1.activeClickableMenu = new CarpenterPhoneMenu(false, Game1.player, Helper);
        }
        private static void UpgradeOnPhone(NPC npc)
        {
            Monitor.Log($"Showing upgrade question");

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }
            Response[] responses = new Response[]
            {
                new Response("PhoneApp_InCall_Upgrade_Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")),
                new Response("PhoneApp_InCall_No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No"))
            };

            string question;
            switch (Game1.player.HouseUpgradeLevel)
            {
                case 0:
                    question = Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse1"));
                    break;
                case 1:
                    question = Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse2"));
                    break;
                case 2:
                    question = Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse3"));
                    break;
                default:
                    ShowMainCallDialogue(npc);
                    return;
            }
            Game1.player.currentLocation.createQuestionDialogue(question, responses, CallDialogueAnswer);

        }
        private static void DoReminisce(int which, NPC npc)
        {
            Monitor.Log($"Doing reminisce");

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }


            Reminisce r = inCallReminiscence[which];
            Dictionary<string, string> dict;
            try
            {
                dict = Helper.Content.Load<Dictionary<string, string>>(Path.Combine("Data", "Events", r.location), ContentSource.GameContent);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Exception loading event dictionary for {r.location}: {ex}");
                return;
            }

            string eventString;
            if (dict.ContainsKey(r.eventId))
                eventString = dict[r.eventId];
            else if (dict.Any(k => k.Key.StartsWith($"{r.eventId}/")))
                eventString = dict.First(k => k.Key.StartsWith($"{r.eventId}/")).Value;
            else
            {
                Monitor.Log($"Event not found for id {r.eventId}");
                return;
            }

            ModEntry.isReminiscing = true;
            (Game1.activeClickableMenu as DialogueBox).closeDialogue();
            Game1.player.currentLocation.lastQuestionKey = "";
            LocationRequest l = Game1.getLocationRequest(r.location);
            ModEntry.isReminiscingAtNight = r.night;
            if (r.mail != null)
            {
                foreach (string m in r.mail.Split(','))
                {
                    if (Game1.player.mailReceived.Contains(m))
                    {
                        Monitor.Log($"Removing received mail {m}");
                        Game1.player.mailReceived.Remove(m);
                    }
                }
            }

            Event e = new Event(eventString)
            {
                exitLocation = new LocationRequest(Game1.player.currentLocation.Name, Game1.player.currentLocation.isStructure.Value, Game1.player.currentLocation)
            };
            Vector2 exitPos = Game1.player.getTileLocation();
            e.onEventFinished += delegate ()
            {
                Monitor.Log($"Event finished");
                ReturnToReminisce();
            };
            ModEntry.reminisceEvent = e;
            Game1.warpFarmer(l, 0, 0, 0);
            l.Location.startEvent(e);
            Game1.player.positionBeforeEvent = exitPos;
        }
        private static void DoInvite(int which, NPC npc)
        {
            Monitor.Log($"Doing invite");

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }


            EventInvite ei = eventInvites[which];
            ModEntry.isInviting = true;

            (Game1.activeClickableMenu as DialogueBox).closeDialogue();
            Game1.player.currentLocation.lastQuestionKey = "";
            LocationRequest l = Game1.getLocationRequest(ei.location);

            Event e = new Event(CreateEventString(ei.nodes, npc))
            {
                exitLocation = new LocationRequest(Game1.player.currentLocation.Name, Game1.player.currentLocation.isStructure.Value, Game1.player.currentLocation)
            };
            Vector2 exitPos = Game1.player.getTileLocation();
            e.onEventFinished += delegate ()
            {
                Monitor.Log($"Event finished");
                ModEntry.isInviting = false;
                ModEntry.invitedNPC = null;
            };
            if (ei.forks != null)
                Helper.Content.InvalidateCache($"Data/Events/{ei.location}");

            ModEntry.invitedNPC = npc;

            EndCall();
            ModEntry.ClosePhone();
            Game1.warpFarmer(l, 0, 0, 0);
            l.Location.startEvent(e);
            Game1.player.positionBeforeEvent = exitPos;
        }
        private static async void StartRecruit(NPC npc)
        {
            Monitor.Log($"Showing recruit response");

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }


            if (ModEntry.npcAdventureModApi.CanRecruit(Game1.player, npc))
            {
                Game1.drawDialogue(npc, ModEntry.npcAdventureModApi.LoadString($"Dialogue/{npc.Name}:companionAccepted"));
                while (Game1.activeClickableMenu is DialogueBox)
                {
                    await Task.Delay(50);
                }
                DoRecruit(npc);
            }
            else
            {
                Game1.drawDialogue(npc, ModEntry.npcAdventureModApi.LoadString($"Dialogue/{npc.Name}:" + (Game1.timeOfDay >= 2200 ? "companionRejectedNight" : "companionRejected")));
                while (Game1.activeClickableMenu is DialogueBox)
                {
                    await Task.Delay(50);
                }
                ShowMainCallDialogue(npc);
            }
        }

        internal static string CreateEventString(List<EventNode> nodes, NPC npc)
        {
            List<string> outNodes = new List<string>();
            foreach(EventNode node in nodes)
            {
                outNodes.Add(node.GetCustomNode(npc.Name));
            }
            return string.Join("/", outNodes);
        }


        private static void DoRecruit(NPC npc)
        {
            Monitor.Log($"Doing recruit");

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }

            if (ModEntry.npcAdventureModApi.RecruitCompanion(Game1.player, npc))
            {
                if (ModEntry.npcAdventureModApi.IsRecruited(npc))
                {
                    Vector2 targetPos = PhoneUtils.GetOpenSurroundingPosition();
                    Monitor.Log($"Recruiting {npc.Name} to {targetPos} (player: {Game1.player.getTileLocation()})");
                    Game1.warpCharacter(npc, Game1.player.currentLocation, targetPos);
                    npc.Sprite.StopAnimation();
                    EndCall();
                }
                else
                {
                    ShowMainCallDialogue(npc);
                }
            }
            else
            {
                Monitor.Log($"Error trying to recruit {npc.Name}", LogLevel.Error);
                ShowMainCallDialogue(npc);
            }
        }
        private static async void DoUpgrade(NPC npc)
        {
            Monitor.Log($"Doing recruit");

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }
            Helper.Reflection.GetMethod(Game1.player.currentLocation, "houseUpgradeAccept").Invoke(new object[] { });
            while (Game1.activeClickableMenu is DialogueBox)
            {
                await Task.Delay(50);
            }
            ShowMainCallDialogue(npc);
        }
        private static async void ReturnToReminisce()
        {
            Game1.outdoorLight = Color.White;
            ModEntry.isReminiscing = false;
            await Task.Delay(1000);
            Monitor.Log($"Returning to reminisce menu");

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }
            ReminisceOnPhone(ModEntry.callingNPC);
        }
        public static void EndCall()
        {
            Monitor.Log($"Ending call");
            ModEntry.inCall = false;
            ModEntry.callingNPC = null;
            ModEntry.isReminiscing = false;
            ModEntry.buildingInCall = false;
            inCallDialogue = null;
            inCallReminiscence = null;
        }
        private static string GetCallGreeting(NPC npc)
        {
            try
            {
                Dictionary<string, string> dict = Helper.Content.Load<Dictionary<string, string>>($"Characters/Dialogue/{npc.Name}", ContentSource.GameContent);
                inCallDialogue = new Dictionary<string, string>(dict);
                if (dict.ContainsKey("MobilePhoneGreeting"))
                    return string.Format(dict["MobilePhoneGreeting"], Game1.player.displayName);
            }
            catch
            {
                Monitor.Log($"{npc.Name} has no dialogue file");
            }
            Monitor.Log($"{npc.Name} has no greeting, using generic greeting");
            return string.Format(Helper.Translation.Get("generic-greeting"), Game1.player.displayName);
        }

        private static string GetReminiscePrefix(NPC npc)
        {
            try
            {
                if (inCallDialogue.ContainsKey("MobilePhoneReminisce"))
                    return inCallDialogue["MobilePhoneReminisce"];
            }
            catch
            {
                Monitor.Log($"{npc.Name} has no dialogue file");
            }
            Monitor.Log($"{npc.Name} has no reminisce string, using generic reminisce question");
            return Helper.Translation.Get("generic-reminisce");
        }
        private static string GetInvitePrefix(NPC npc)
        {
            try
            {
                if (inCallDialogue.ContainsKey("MobilePhoneInvite"))
                    return inCallDialogue["MobilePhoneInvite"];
            }
            catch
            {
                Monitor.Log($"{npc.Name} has no dialogue file");
            }
            Monitor.Log($"{npc.Name} has no invitation string, using generic reminisce question");
            return Helper.Translation.Get("generic-invite");
        }

        private static string GetGoodBye(NPC npc)
        {
            try
            {
                if (inCallDialogue.ContainsKey("MobilePhoneGoodBye"))
                    return inCallDialogue["MobilePhoneGoodBye"];
            }
            catch
            {
                Monitor.Log($"{npc.Name} has no dialogue file");
            }
            Monitor.Log($"{npc.Name} has no goodbye, using generic goodbye");
            return Helper.Translation.Get("generic-goodbye");
        }

    }
}