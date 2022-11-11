using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MailboxMenu
{
    public class MailMenu : IClickableMenu
    {
        private static int whichTab;
        private static string whichSender;
        private static bool preserveScroll;
        private static int mainScrolled;
        private static int sideScrolled;

        private ClickableComponent inboxButton;
        private ClickableComponent allMailButton;
        private List<ClickableTextureComponent> currentMailList = new List<ClickableTextureComponent>();
        private List<ClickableComponent> senders = new List<ClickableComponent>();
        private List<string> possibleSenders = new List<string>();
        private int mailIndex = 99942000;
        private Dictionary<string, string> mailTitles = new Dictionary<string, string>();
        private bool canScroll;
        private int contained;

        public MailMenu() : base(Game1.uiViewport.Width / 2 - (ModEntry.Config.WindowWidth + borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (ModEntry.Config.WindowHeight + borderWidth * 2) / 2, ModEntry.Config.WindowWidth + borderWidth * 2, ModEntry.Config.WindowHeight+ borderWidth * 2, false)
        {
            if (!preserveScroll)
            {
                mainScrolled = 0;
                sideScrolled = 0;
                whichTab = 0;
                whichSender = null;
            }
            preserveScroll = false;

            var textHeight = (int)Game1.dialogueFont.MeasureString(ModEntry.Config.InboxText).Y;
            currentMailList = new List<ClickableTextureComponent>();
            inboxButton = new ClickableComponent(new Rectangle(xPositionOnScreen + borderWidth + 16, yPositionOnScreen + borderWidth + 64, 256, textHeight), "Inbox")
            {
                myID = 900,
                downNeighborID = 901,
                rightNeighborID = mailIndex,
                region = 15923
            };
            allMailButton = new ClickableComponent(new Rectangle(xPositionOnScreen + borderWidth + 16, yPositionOnScreen + borderWidth + 64 + textHeight + 8, 256, textHeight), "Archive")
            {
                myID = 901,
                upNeighborID = 900,
                downNeighborID = 902,
                rightNeighborID = mailIndex,
                region = 15923
            };
            PopulateSenders();
            PopulateMailList();
            populateClickableComponentList();
        }

        private void PopulateSenders()
        {
            senders.Clear();
            possibleSenders.Clear();
            foreach(var kvp in ModEntry.envelopeData)
            {
                if (Game1.mailbox.Contains(kvp.Key) || !Game1.player.mailReceived.Contains(kvp.Key))
                    continue;
                if(!string.IsNullOrEmpty(kvp.Value.sender) && !possibleSenders.Contains(kvp.Value.sender))
                {
                    possibleSenders.Add(kvp.Value.sender);
                }
            }
            var textHeight = (int)Game1.dialogueFont.MeasureString(ModEntry.Config.InboxText).Y;
            var textHeight2 = (int)Game1.smallFont.MeasureString(ModEntry.Config.InboxText).Y;
            int count = (height - borderWidth * 2 - 64 - (textHeight + 8) * 2) / textHeight2;
            possibleSenders.Sort();
            var list = possibleSenders.Skip(sideScrolled).Take(count).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                senders.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + borderWidth + 16, yPositionOnScreen + borderWidth + 64 + (textHeight + 8) * 2 + i * textHeight2, ModEntry.Config.SideWidth - borderWidth / 2, textHeight2), list[i])
                {
                    myID = 902 + i,
                    upNeighborID = 902 + i - 1,
                    downNeighborID = 902 + i + 1,
                    rightNeighborID = mailIndex,
                    region = 15923
                });
            }
        }

        private void PopulateMailList()
        {
            currentMailList.Clear();
            if (whichTab == 0)
            {
                for (int i = mainScrolled * ModEntry.Config.GridColumns; i < Game1.mailbox.Count; i++)
                {
                    ModEntry.SMonitor.Log(Game1.mailbox[i]);
                    AddMail(Game1.mailbox[i], i);
                }
            }
            else
            {
                Dictionary<string, string> mail = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
                List<string> strings = new List<string>();
                int count = 0;
                for (int i = 0; i < Game1.player.mailReceived.Count; i++)
                {
                    if (Game1.mailbox.Contains(Game1.player.mailReceived[i]) || !mail.ContainsKey(Game1.player.mailReceived[i]))
                        continue;
                    if (whichSender is not null && (!ModEntry.envelopeData.TryGetValue(Game1.player.mailReceived[i], out EnvelopeData data) || data.sender != whichSender))
                        continue;
                    if (count >= mainScrolled * ModEntry.Config.GridColumns)
                    {
                        AddMail(Game1.player.mailReceived[i], count - mainScrolled * ModEntry.Config.GridColumns);
                    }
                    count++;
                }
            }
        }

        private void AddMail(string id, int i)
        {
            Dictionary<string, string> mail = Game1.content.Load<Dictionary<string, string>>("Data\\mail");

            if (!mailTitles.ContainsKey(id))
            {
                string[] split = mail[id].Split(new string[]
                {
                "[#]"
                }, StringSplitOptions.None);
                mailTitles[id] = split.Length > 1 ? split[1] : "???";
            }
            var gridX = i % ModEntry.Config.GridColumns;
            var gridY = i / ModEntry.Config.GridColumns;
            Texture2D texture;
            if (ModEntry.envelopeData.TryGetValue(id, out EnvelopeData data))
            {
                if (data.texture is not null)
                {
                    texture = data.texture;
                }
                else if (!string.IsNullOrEmpty(data.texturePath))
                {
                    texture = ModEntry.SHelper.GameContent.Load<Texture2D>(data.texturePath);
                }
                else if (!string.IsNullOrEmpty(data.sender) && ModEntry.npcEnvelopeData.TryGetValue(data.sender, out data))
                {
                    if (data.texture is not null)
                    {
                        texture = data.texture;
                    }
                    else if (!string.IsNullOrEmpty(data.texturePath))
                    {
                        texture = ModEntry.SHelper.GameContent.Load<Texture2D>(data.texturePath);
                    }
                    else
                    {
                        data = ModEntry.envelopeData["default"];
                        texture = data.texture;
                    }
                }
                else
                {
                    data = ModEntry.envelopeData["default"];
                    texture = data.texture;
                }
            }
            else
            {
                data = ModEntry.envelopeData["default"];
                texture = data.texture;
            }
            Rectangle textureBounds = new Rectangle(0, 0, texture.Width, texture.Height);
            if (data.frames > 1)
            {
                textureBounds.Size = new Point(data.frameWidth, texture.Height);
            }
            int xOffset =  (width - (borderWidth * 2 + ModEntry.Config.SideWidth) - (ModEntry.Config.EnvelopeWidth + ModEntry.Config.GridSpace) * ModEntry.Config.GridColumns) / 2;
            currentMailList.Add(new ClickableTextureComponent(id, new Rectangle(xPositionOnScreen + borderWidth * 2 + ModEntry.Config.SideWidth + xOffset + gridX * (ModEntry.Config.EnvelopeWidth + ModEntry.Config.GridSpace), yPositionOnScreen + borderWidth + 132 + gridY * (ModEntry.Config.EnvelopeHeight + ModEntry.Config.GridSpace + 16), ModEntry.Config.EnvelopeWidth, ModEntry.Config.EnvelopeHeight), "", "", texture, textureBounds, data.scale, false)
            {
                hoverText = mailTitles[id],
                myID = mailIndex + i,
                upNeighborID = mailIndex + i - 1,
                downNeighborID = mailIndex + i + 1,
                leftNeighborID = 901,
                region = 15924
            });
        }

        public override void draw(SpriteBatch b)
        {
            canScroll = false;
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, false);
            b.Draw(Game1.mouseCursors, new Rectangle(xPositionOnScreen + borderWidth + ModEntry.Config.SideWidth, yPositionOnScreen + 56 + borderWidth, 36, height - 88 - borderWidth), new Rectangle?(new Rectangle(278, 324, 9, 1)), Color.White);
            var cutoff = yPositionOnScreen + height - 32;
            if (whichSender is null)
            {
                SpriteText.drawStringWithScrollCenteredAt(b, whichTab == 0 ? ModEntry.Config.InboxText : ModEntry.Config.ArchiveText, xPositionOnScreen + (borderWidth + ModEntry.Config.SideWidth + width) / 2, yPositionOnScreen + borderWidth + 64, width - borderWidth * 5 - ModEntry.Config.SideWidth);
            }
            else
            {
                SpriteText.drawStringWithScrollCenteredAt(b, whichSender, xPositionOnScreen + (borderWidth + ModEntry.Config.SideWidth + width) / 2, yPositionOnScreen + borderWidth + 64, width - borderWidth * 5 - ModEntry.Config.SideWidth);
            }
            contained = 0;
            foreach (var cc in currentMailList)
            {
                if (cc.bounds.Y >= cutoff)
                {
                    canScroll = true;
                    break;
                }
                contained++;
                int width = cc.texture.Width;
                int xOffset = 0;
                if(ModEntry.envelopeData.TryGetValue(cc.name, out EnvelopeData data) && data.frames > 1)
                {
                    cc.draw(b, Color.White, 1, (int)(Game1.currentGameTime.TotalGameTime.TotalSeconds / data.frameSeconds) % data.frames);
                }
                else if (cc.bounds.Y + cc.bounds.Height >= cutoff)
                {
                    cc.sourceRect = new Rectangle(xOffset, 0, width, Math.Min((cutoff - cc.bounds.Y) / (ModEntry.Config.EnvelopeWidth / cc.texture.Width), cc.texture.Height));
                    cc.draw(b);
                    canScroll = true;
                    continue;
                }
                else
                {
                    cc.draw(b);
                }
                var s = mailTitles[cc.name];
                var scale = 1f;
                var split = s.Split(' ');
                int lines = 0;
                for(int i = 0; i < split.Length; i++)
                {
                    string str = split[i];
                    if(i < split.Length - 1 && Game1.smallFont.MeasureString(str + " " + split[i + 1]).X < cc.bounds.Width * 1.5f)
                    {
                        str += " " + split[i + 1];
                        i++;
                    }
                    var m = Game1.smallFont.MeasureString(str) * scale;
                    
                    var y = cc.bounds.Y + cc.bounds.Height + (int)(m.Y * (lines * 0.8f + 0.5f));
                    if (y + m.Y > cutoff)
                    {
                        canScroll = true;
                        break;
                    }
                    b.DrawString(Game1.smallFont, str, new Vector2(cc.bounds.X + (cc.bounds.Width - m.X) / 2 - 1, y + 1), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 1f);
                    b.DrawString(Game1.smallFont, str, new Vector2(cc.bounds.X + (cc.bounds.Width - m.X) / 2, y), Color.Black, 0, Vector2.Zero, scale, SpriteEffects.None, 1f);
                    lines++;
                }
            }
            SpriteText.drawString(b, ModEntry.Config.InboxText, inboxButton.bounds.X, inboxButton.bounds.Y, color: whichTab == 0 ? 0 : 8);
            SpriteText.drawString(b, ModEntry.Config.ArchiveText, allMailButton.bounds.X, allMailButton.bounds.Y, color: whichTab == 1 ? 0 : 8);
            for(int i = 0; i < senders.Count; i++)
            {
                string str = senders[i].name;
                int cut = 0;
                if(Game1.smallFont.MeasureString(str).X > ModEntry.Config.SideWidth)
                {
                    do
                    {
                        cut++;
                        str = str.Substring(0, str.Length - cut);
                    }
                    while (Game1.smallFont.MeasureString(str + "...").X > ModEntry.Config.SideWidth);
                    str += "...";
                }

                b.DrawString(Game1.smallFont, str, new Vector2(senders[i].bounds.X + senders[i].bounds.Width - Game1.smallFont.MeasureString(str).X - 1, senders[i].bounds.Y + 1), whichSender == senders[i].name ? new Color(50, 50, 50, 255) : new Color(75, 75, 75, 255));
                b.DrawString(Game1.smallFont, str, new Vector2(senders[i].bounds.X + senders[i].bounds.Width - Game1.smallFont.MeasureString(str).X, senders[i].bounds.Y), whichSender == senders[i].name ? Color.Black : new Color(100, 100, 100, 255));
            }
            drawMouse(b);
            base.draw(b);
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (inboxButton.containsPoint(x, y))
            {
                if (whichTab == 0)
                    return;
                mainScrolled = 0;
                whichTab = 0;
                whichSender = null;
                Game1.playSound("bigSelect");
                PopulateMailList();
                return;
            }
            if (allMailButton.containsPoint(x, y))
            {
                mainScrolled = 0;
                if (whichTab == 1 && whichSender is null)
                    return;
                whichTab = 1;
                whichSender = null;
                Game1.playSound("bigSelect");
                PopulateMailList();
                return;
            }
            for (int i = 0; i < senders.Count; i++)
            {
                if(senders[i].containsPoint(x, y))
                {
                    whichTab = 1;
                    whichSender = senders[i].name;
                    ModEntry.SMonitor.Log($"clicked on {senders[i].name}");
                    Game1.playSound("bigSelect");
                    PopulateMailList();
                    return;
                }
            }
            for (int i = 0; i < contained; i++)
            {
                var cc = currentMailList[i];
                if(cc.containsPoint(x, y))
                {
                    var mailTitle = cc.name;
                    Game1.playSound("bigSelect");
                    Dictionary<string, string> mails = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
                    string mail = mails.ContainsKey(mailTitle) ? mails[mailTitle] : "";
                    if (mailTitle.StartsWith("passedOut "))
                    {
                        string[] split = mailTitle.Split(' ', StringSplitOptions.None);
                        int moneyTaken = (split.Length > 1) ? Convert.ToInt32(split[1]) : 0;
                        switch (new Random(moneyTaken).Next((Game1.player.getSpouse() != null && Game1.player.getSpouse().Name.Equals("Harvey")) ? 2 : 3))
                        {
                            case 0:
                                if (Game1.MasterPlayer.hasCompletedCommunityCenter() && !Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
                                {
                                    mail = string.Format(mails["passedOut4"], moneyTaken);
                                }
                                else
                                {
                                    mail = string.Format(mails["passedOut1_" + ((moneyTaken > 0) ? "Billed" : "NotBilled") + "_" + (Game1.player.IsMale ? "Male" : "Female")], moneyTaken);
                                }
                                break;
                            case 1:
                                mail = string.Format(mails["passedOut2"], moneyTaken);
                                break;
                            case 2:
                                mail = string.Format(mails["passedOut3_" + ((moneyTaken > 0) ? "Billed" : "NotBilled")], moneyTaken);
                                break;
                        }
                    }
                    else if (mailTitle.StartsWith("passedOut"))
                    {
                        string[] split2 = mailTitle.Split(' ', StringSplitOptions.None);
                        if (split2.Length > 1)
                        {
                            int moneyTaken2 = Convert.ToInt32(split2[1]);
                            mail = string.Format(mails[split2[0]], moneyTaken2);
                        }
                    }
                    if (mail.Length == 0)
                    {
                        return;
                    }
                    if(whichTab > 0)
                        preserveScroll = true;
                    Game1.activeClickableMenu = new MyLetterViewerMenu(mail, mailTitle, !Game1.mailbox.Contains(cc.name));
                    if (Game1.mailbox.Contains(cc.name))
                    {
                        if (!Game1.player.mailReceived.Contains(cc.name) && !cc.name.Contains("passedOut") && !cc.name.Contains("Cooking"))
                        {
                            Game1.player.mailReceived.Add(cc.name);
                        }
                        Game1.mailbox.Remove(cc.name);
                        PopulateSenders();
                    }
                    return;
                }
            }
            base.receiveLeftClick(x, y, playSound);
        }
        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if(Game1.getMousePosition().X < xPositionOnScreen + borderWidth * 1.5 + ModEntry.Config.SideWidth)
            {
                if (direction < 0 && senders.Count < possibleSenders.Count - sideScrolled)
                {
                    sideScrolled++;
                }
                else if (direction > 0 && sideScrolled > 0)
                {
                    sideScrolled--;
                }
                else
                {
                    return;
                }
                PopulateSenders();
            }
            else
            {
                if (direction < 0 && canScroll)
                {
                    int count = whichTab == 0 ? Game1.mailbox.Count : Game1.player.mailReceived.Count;
                    if (count < (mainScrolled + 1) * ModEntry.Config.GridColumns)
                        return;
                    mainScrolled++;
                }
                else if (direction > 0 && mainScrolled > 0)
                {
                    mainScrolled--;
                }
                else
                {
                    return;
                }
            }
            Game1.playSound("shiny4");
            PopulateMailList();
        }
    }
}