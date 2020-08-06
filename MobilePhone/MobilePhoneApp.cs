using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Helper.Events.Input.ButtonReleased += Input_ButtonReleased;
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

        private static void Input_ButtonReleased(object sender, StardewModdingAPI.Events.ButtonReleasedEventArgs e)
        {
            if (e.Button == SButton.MouseLeft)
            {
            }
        }

        public static async void CallNPC(NPC npc)
        {
            if (npc.CurrentDialogue.Count >= 1 || npc.endOfRouteMessage.Value != null)
            {
                Monitor.Log($"{npc.Name} has dialogue");
                npc.grantConversationFriendship(Game1.player, 20);
                Dialogue greet = GetCallGreeting(npc);
                if(greet != null)
                    npc.CurrentDialogue.Push(greet);
                int i = 0;
                ModEntry.inCall = true;
                while (npc.CurrentDialogue.Count > 0 && ModEntry.phoneOpen)
                {
                    if(!(Game1.activeClickableMenu is DialogueBox))
                    {
                        Game1.drawDialogue(npc);
                        Monitor.Log($"Dialogues left {npc.CurrentDialogue.Count}");
                    }
                    await Task.Delay(100);
                }
                ModEntry.inCall = false;
                ModEntry.callingNPC = null;
            }
            else
            {
                Monitor.Log($"{npc.Name} has no dialogue");
                Game1.drawObjectDialogue(Helper.Translation.Get("no-answer"));
            }
        }

        private static Dialogue GetCallGreeting(NPC npc)
        {
            try
            {
                Dictionary<string, string> dict = Helper.Content.Load<Dictionary<string, string>>($"Characters/Dialogue/{npc.name}", ContentSource.GameContent);
                if (dict.ContainsKey("MobilePhoneGreeting"))
                    return new Dialogue(dict["MobilePhoneGreeting"], npc);
            }
            catch
            {
                Monitor.Log($"{npc.Name} has no dialogue file, using generic greeting");
            }
            Monitor.Log($"{npc.Name} has no greeting, using generic greeting");
            return new Dialogue(Helper.Translation.Get("generic-greeting"), npc);
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
                Helper.Events.Input.ButtonReleased -= Input_ButtonReleased;
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