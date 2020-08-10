using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MobilePhone
{
    public class MobilePhoneApp
    {
        private static IModHelper Helper;
        private static IMonitor Monitor;
        private static ModConfig Config;
        private static Texture2D appIcon;
        private static List<CallableNPC> callableList = new List<CallableNPC>();
        private static bool dragging;
        private static int yOffset;
        private static int lastMousePositionY;
        private static float listHeight;
        private static bool clicked;
        private static List<Reminisce> inCallReminiscence;
        private static Dictionary<string, string> inCallDialogue;

        public static void Initialize(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            Helper = helper;
            Monitor = monitor;
            Config = config;
            appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets","app_icon.png"));
            ModEntry.apps.Add(Helper.ModRegistry.ModID, GetApp());
        }

        private static MobileApp GetApp()
        {
            return new MobileApp("Mobile Phone", OpenPhoneBook, appIcon);
        }

        public static void OpenPhoneBook()
        {
            Monitor.Log($"opening phone book");
            ModEntry.appRunning = true;
            ModEntry.phoneAppRunning = true;
            ModEntry.runningApp = Helper.ModRegistry.ModID;
            CreateCallableList();
            listHeight = Config.ContactMarginY + (int)Math.Ceiling(callableList.Count / (float)ModEntry.gridWidth) * (Config.ContactHeight + Config.ContactMarginY);
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private static void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {

            if (ModEntry.inCall || !ModEntry.appRunning || ModEntry.runningApp != Helper.ModRegistry.ModID || !ModEntry.screenRect.Contains(Game1.getMousePosition()))
                return;

            if (e.Button == SButton.MouseLeft)
            {
                Helper.Input.Suppress(SButton.MouseLeft);

                clicked = true;

                lastMousePositionY = Game1.getMouseY();
            }
        }

        public static void CallNPC(NPC npc)
        {
            inCallReminiscence = null;
            ModEntry.inCall = true;
            ModEntry.callingNPC = npc;

            ShowMainCallDialogue(npc);

            return;
        }

        private static void ShowMainCallDialogue(NPC npc)
        {
            Monitor.Log($"Showing Main Call Dialogue");

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }

            List<Response> answers = new List<Response>();
            if (npc.CurrentDialogue != null && npc.CurrentDialogue.Count > 0)
                answers.Add(new Response("PhoneApp_InCall_Chat", Helper.Translation.Get("chat")));
                
            if(inCallReminiscence == null)
            {
                Reminiscence r = Helper.Data.ReadJsonFile<Reminiscence>(Path.Combine("assets", "events", $"{npc.Name}.json")) ?? new Reminiscence();
                Monitor.Log($"Total Reminisces: {r.events.Count}");
                r.WeedOutUnseen();
                Monitor.Log($"Seen Reminisces: {r.events.Count}");
                inCallReminiscence = r.events;
            }
            if (inCallReminiscence != null && inCallReminiscence.Count > 0)
            {
                answers.Add(new Response("PhoneApp_InCall_Reminisce", Helper.Translation.Get("reminisce")));
            }
            if (ModEntry.npcAdventureModApi != null && ModEntry.npcAdventureModApi.IsPossibleCompanion( npc) && ModEntry.npcAdventureModApi.CanAskToFollow(npc))
            {
                answers.Add(new Response("PhoneApp_InCall_Recruit", Helper.Translation.Get("recruit")));
            }

            answers.Add(new Response("PhoneApp_InCall_GoodBye", Helper.Translation.Get("goodbye")));

            Game1.player.currentLocation.createQuestionDialogue(GetCallGreeting(npc), answers.ToArray(), "PhoneApp_InCall_Begin");
            Game1.objectDialoguePortraitPerson = npc;
        }

        public static void CallDialogueAnswer(string whichAnswer, NPC npc)
        {
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
            else if (whichAnswer == "PhoneApp_InCall_Recruit")
            {
                RecruitOnPhone(npc);
            }
            else if (whichAnswer == "PhoneApp_InCall_Recruit_Yes")
            {
                StartRecruit(npc);
            }
            else if (whichAnswer == "PhoneApp_InCall_Recruit_No")
            {
                ShowMainCallDialogue(npc);
            }
            else if (whichAnswer.StartsWith("PhoneApp_InCall_Reminiscence_"))
            {
                int which = int.Parse(whichAnswer.Substring("PhoneApp_InCall_Reminiscence_".Length));
                DoReminisce(which, npc);
            }
            else if (whichAnswer == "PhoneApp_InCall_GoodBye")
            {
                Game1.drawDialogue(npc, GetGoodBye(npc));
                EndCall();
            }
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
            for(int i = 0; i < inCallReminiscence.Count; i++)
                responses.Add(new Response($"PhoneApp_InCall_Reminiscence_{i}", inCallReminiscence[i].name));

            responses.Add(new Response("PhoneApp_InCall_Return", Helper.Translation.Get("back")));

            Game1.player.currentLocation.createQuestionDialogue(GetReminiscePrefix(npc), responses.ToArray(), "PhoneApp_InCall_Reminisce_Question");
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
            else if (!dict.FirstOrDefault(k => k.Key.StartsWith($"{r.eventId}/")).Equals(default(KeyValuePair<string, string>)))
                eventString = dict.FirstOrDefault(k => k.Key.StartsWith($"{r.eventId}/")).Value;
            else
            {
                Monitor.Log($"Event not found for id {r.eventId}");
                return;
            }


            ModEntry.isReminiscing = true;
            (Game1.activeClickableMenu as DialogueBox).closeDialogue();
            Game1.player.currentLocation.lastQuestionKey = "";
            LocationRequest l = Game1.getLocationRequest(r.location);
            if (r.night)
            {

            }
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
                exitLocation = new LocationRequest(Game1.player.currentLocation.Name, Game1.player.currentLocation.isStructure, Game1.player.currentLocation)
            };
            Vector2 exitPos = Game1.player.getTileLocation();
            e.onEventFinished += delegate ()
            {
                Monitor.Log($"Event finished");
                ReturnToReminisce();
            };
            Game1.warpFarmer(l, 0, 0, 0);
            l.Location.startEvent(e);
            Game1.player.positionBeforeEvent = exitPos;
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
				new Response("PhoneApp_InCall_Recruit_No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No"))
            };
            Game1.player.currentLocation.createQuestionDialogue(ModEntry.npcAdventureModApi.LoadString("Strings/Strings:askToFollow", npc.Name), responses, "PhoneApp_InCall_Recruit_Question");
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
                Game1.drawDialogue(npc, ModEntry.npcAdventureModApi.GetFriendSpecificDialogueText(Game1.player, npc, "companionAccepted"));
                while(Game1.activeClickableMenu is DialogueBox)
                {
                    await Task.Delay(50);
                }
                DoRecruit(npc);
            }
            else
            {
                Game1.drawDialogue(npc, ModEntry.npcAdventureModApi.GetFriendSpecificDialogueText(Game1.player, npc, Game1.timeOfDay >= 2200 ? "companionRejectedNight" : "companionRejected"));
            }
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

        private static async void ReturnToReminisce()
        {
            await Task.Delay(1000);
            Monitor.Log($"Returning to reminisce menu");

            if (!ModEntry.inCall)
            {
                Monitor.Log($"Not in call, exiting");
                return;
            }

            ModEntry.isReminiscing = false;
            ReminisceOnPhone(ModEntry.callingNPC);
        }
        public static void EndCall()
        {
            Monitor.Log($"Ending call");
            ModEntry.inCall = false;
            ModEntry.callingNPC = null;
            ModEntry.isReminiscing = false;
            inCallDialogue = null;
            inCallReminiscence = null;
        }
        private static string GetCallGreeting(NPC npc)
        {
            try
            {
                Dictionary<string, string> dict = Helper.Content.Load<Dictionary<string, string>>($"Characters/Dialogue/{npc.name}", ContentSource.GameContent);
                inCallDialogue = new Dictionary<string, string>(dict);
                if (dict.ContainsKey("MobilePhoneGreeting"))
                    return string.Format(dict["MobilePhoneGreeting"],Game1.player.displayName);
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
            Monitor.Log($"{npc.Name} has no greeting, using generic reminisce question");
            return Helper.Translation.Get("generic-reminisce");
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

        private static void CreateCallableList()
        {
            callableList.Clear();
            foreach(KeyValuePair<string,Netcode.NetRef<Friendship>> kvp in Game1.player.friendshipData.FieldDict)
            {
                try
                {
                    if (kvp.Value.Value.Points >= Config.MinPointsToCall)
                    {
                        NPC npc = Game1.getCharacterFromName(kvp.Key);
                        Texture2D portrait = npc.Sprite.Texture;
                        Rectangle sourceRect = npc.getMugShotSourceRect();
                        string name = Config.UseRealNamesInPhoneBook && npc.displayName != null ? npc.displayName : npc.Name;
                        callableList.Add(new CallableNPC(name, npc, portrait, sourceRect));
                    }
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Couldn't load npc {kvp.Key}: {ex}");
                }
            }
            callableList = callableList.OrderBy(a => a.npc.Name).ToList();
        }

        private static void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (ModEntry.callingNPC != null)
            {
                return;
            }

            if (!ModEntry.appRunning || !ModEntry.phoneOpen || ModEntry.runningApp != Helper.ModRegistry.ModID)
            {
                ModEntry.appRunning = false;
                ModEntry.phoneAppRunning = false;
                Helper.Events.Display.RenderedWorld -= Display_RenderedWorld;
                Helper.Events.Input.ButtonPressed -= Input_ButtonPressed;
                return;
            }
            Vector2 screenPos = PhoneUtils.GetScreenPosition();
            Vector2 screenSize = PhoneUtils.GetScreenSize();
            Rectangle headerRect = new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenSize.X, Config.AppHeaderHeight);
            Point mousePos = Game1.getMousePosition();

            if (Helper.Input.IsSuppressed(SButton.MouseLeft))
            {
                int dy = mousePos.Y - lastMousePositionY;
                if (Math.Abs(dy) > 0 && ModEntry.screenRect.Contains(mousePos))
                {
                    dragging = true;
                }
                if (dragging)
                {
                    yOffset = (int)Math.Max(Math.Min(0, yOffset + dy), -1 * Math.Max(0, listHeight - (screenSize.Y - Config.AppHeaderHeight)));
                }
            }

            if (clicked && !Helper.Input.IsSuppressed(SButton.MouseLeft))
            {
                clicked = false;
                if (dragging)
                {
                    Monitor.Log($"was dragging");
                    dragging = false;
                }
                else
                {
                    if (headerRect.Contains(mousePos))
                    {
                        if (new Rectangle((int)screenPos.X + (int)screenSize.X - Config.AppHeaderHeight, (int)screenPos.Y, Config.AppHeaderHeight, Config.AppHeaderHeight).Contains(mousePos))
                        {
                            PhoneUtils.ToggleApp(false);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < callableList.Count; i++)
                        {
                            Vector2 pos = GetNPCPos(i);
                            Rectangle r = new Rectangle((int)pos.X, (int)pos.Y, Config.ContactWidth, Config.ContactHeight);
                            if (r.Contains(mousePos))
                            {
                                Monitor.Log($"calling {callableList[i].npc.Name}");
                                PhoneUtils.PlayRingTone();
                                CallNPC(callableList[i].npc);
                            }
                        }
                    }
                }
            }

            lastMousePositionY = Game1.getMouseY();
            int startListY = (int)screenPos.Y + Config.AppHeaderHeight;
            e.SpriteBatch.Draw(ModEntry.phoneBookTexture, screenPos, Color.White);

            if(yOffset < 0)
            {
                e.SpriteBatch.Draw(ModEntry.upArrowTexture, ModEntry.upArrowPosition, Color.White);
            }
            if (yOffset > PhoneUtils.GetScreenSize().Y - Config.AppHeaderHeight - listHeight)
            {
                e.SpriteBatch.Draw(ModEntry.downArrowTexture, ModEntry.downArrowPosition, Color.White);
            }

            int screenBottom = (int)(screenPos.Y + screenSize.Y);
            for (int i = 0; i < callableList.Count; i++)
            {
                Vector2 npcPos = GetNPCPos(i);
                Rectangle r = callableList[i].sourceRect;
                if (npcPos.Y < startListY - r.Height * 2 || npcPos.Y >= screenBottom)
                {
                    continue;
                }
                Rectangle sourceRect = r;
                int cutTop = 0;
                int cutBottom = 0;
                if(npcPos.Y < startListY)
                {
                    cutTop = (int)Math.Round((startListY - (int)npcPos.Y) / 2f);
                    sourceRect = new Rectangle(r.X, r.Y + cutTop, r.Width, r.Height - cutTop);
                    npcPos = new Vector2(npcPos.X, startListY);
                }
                else if(npcPos.Y > screenBottom - r.Height * 2)
                {
                    cutBottom = (int)Math.Round((screenBottom - r.Height * 2 - (int)npcPos.Y) / 2f);
                    sourceRect = new Rectangle(r.X, r.Y, r.Width, r.Height + cutBottom);
                }

                e.SpriteBatch.Draw(callableList[i].portrait, npcPos + new Vector2((Config.ContactWidth - 32) / 2f,0), sourceRect, Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0.86f);
                if(Config.ShowNamesInPhoneBook && npcPos.Y < screenBottom - Config.ContactHeight - callableList[i].nameSize.Y * 0.4f + 6)
                    e.SpriteBatch.DrawString(Game1.dialogueFont, callableList[i].name, GetNPCPos(i) + new Vector2(Config.ContactWidth / 2f - callableList[i].nameSize.X * 0.2f, Config.ContactHeight - 6 ), Color.Black, 0, Vector2.Zero, 0.4f, SpriteEffects.None, 0.86f);
            }
            e.SpriteBatch.Draw(ModEntry.phoneBookHeaderTexture, headerRect, Color.White);
            string headerText = Helper.Translation.Get("phone-book");
            Vector2 headerTextSize = Game1.dialogueFont.MeasureString(headerText) * Config.HeaderTextScale;
            e.SpriteBatch.DrawString(Game1.dialogueFont, headerText, screenPos + new Vector2(screenSize.X / 2f - headerTextSize.X / 2f, Config.AppHeaderHeight / 2f - headerTextSize.Y / 2f ), Config.PhoneBookHeaderTextColor, 0, Vector2.Zero, Config.HeaderTextScale, SpriteEffects.None, 0.86f);
            e.SpriteBatch.DrawString(Game1.dialogueFont, "x", screenPos + new Vector2(screenSize.X - Config.AppHeaderHeight / 2f - Game1.dialogueFont.MeasureString("x").X * Config.HeaderTextScale / 2f, Config.AppHeaderHeight / 2f - headerTextSize.Y / 2f), Config.PhoneBookHeaderTextColor, 0, Vector2.Zero, Config.HeaderTextScale, SpriteEffects.None, 0.86f);
        }

        private static Vector2 GetNPCPos(int i)
        {
            float x = ModEntry.screenPosition.X + Config.ContactMarginX + ((i % ModEntry.gridWidth) * (Config.ContactWidth + Config.ContactMarginX));
            float y = ModEntry.screenPosition.Y + Config.AppHeaderHeight + Config.ContactMarginY + ((i / ModEntry.gridWidth) * (Config.ContactHeight + Config.ContactMarginY));

            return new Vector2(x, y + yOffset);
        }
        public static void ReceiveRandomCall()
        {
            CreateCallableList();
            if (callableList.Count == 0)
            {
                Monitor.Log($"You have no friends.", LogLevel.Debug);
                return;
            }
            CallableNPC[] callers = callableList.Where(s => s.npc.CurrentDialogue.Count >= 1 || s.npc.endOfRouteMessage.Value != null).ToArray();
            if (callers.Length == 0)
            {
                Monitor.Log($"None of your friends want to talk to you.", LogLevel.Debug);
                return;
            }
            CallableNPC caller = callers[Game1.random.Next(callers.Length)];
            
            Monitor.Log($"Friend calling: {caller.npc.displayName}", LogLevel.Debug);
            PhoneUtils.PlayRingTone();
            ModEntry.currentCallRings = 0;
            ModEntry.currentCallMaxRings = Game1.random.Next(Math.Max(0, Config.IncomingCallMinRings), Math.Max(Config.IncomingCallMinRings + 1, Config.IncomingCallMaxRings));
            ModEntry.callingNPC = caller.npc;
        }
    }
}