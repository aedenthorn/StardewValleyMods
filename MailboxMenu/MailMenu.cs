using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MailboxMenu
{
    public class MailMenu : IClickableMenu
    {
        private static int windowWidth = 1200;
        private static int windowHeight = 1000;

        private ClickableComponent inboxButton;
        private ClickableComponent allMailButton;
        private List<ClickableTextureComponent> currentMailList = new List<ClickableTextureComponent>();
        private int mailIndex = 99942000;
        private int gridColumns = 3;
        private int envelopeWidth = 256;
        private int envelopeHeight = 192;
        private int gridSpace = 64;
        private Dictionary<string, string> mailTitles = new Dictionary<string, string>();
        private bool canScroll;
        private int scrolled;
        private int contained;

        public MailMenu() : base(Game1.uiViewport.Width / 2 - (windowWidth + borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (windowHeight + borderWidth * 2) / 2, windowWidth + borderWidth * 2, windowHeight + borderWidth * 2, false)
        {
            var textHeight = (int)Game1.dialogueFont.MeasureString("Inbox").Y;
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
                rightNeighborID = mailIndex,
                region = 15923
            };
            PopulateMailList();
            populateClickableComponentList();
        }
        public override void draw(SpriteBatch b)
        {
            canScroll = false;
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, false);
            b.Draw(Game1.mouseCursors, new Rectangle(xPositionOnScreen + borderWidth + 194, yPositionOnScreen + 56 + borderWidth, 36, height - 88 - borderWidth), new Rectangle?(new Rectangle(278, 324, 9, 1)), Color.White);
            var cutoff = yPositionOnScreen + height - 32;
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
                if(ModEntry.mailDataDict.TryGetValue(cc.name, out MailData data) && data.frames > 1)
                {
                    cc.draw(b, Color.White, 1, (int)(Game1.currentGameTime.TotalGameTime.TotalSeconds / data.frameSeconds) % data.frames);
                }
                else if (cc.bounds.Y + cc.bounds.Height >= cutoff)
                {
                    cc.sourceRect = new Rectangle(xOffset, 0, width, Math.Min((cutoff - cc.bounds.Y) / (envelopeWidth / cc.texture.Width), cc.texture.Height));
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
                    b.DrawString(Game1.smallFont, str, new Vector2(cc.bounds.X + (cc.bounds.Width - m.X) / 2, y), Color.Black, 0, Vector2.Zero, scale, SpriteEffects.None, 1f);
                    lines++;
                }
            }
            SpriteText.drawString(b, "Inbox", inboxButton.bounds.X, inboxButton.bounds.Y, color: ModEntry.whichTab == 0 ? 0 : 8);
            SpriteText.drawString(b, "Archive", allMailButton.bounds.X, allMailButton.bounds.Y, color: ModEntry.whichTab == 1 ? 0 : 8);
            drawMouse(b);
            base.draw(b);
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (inboxButton.containsPoint(x, y))
            {
                if (ModEntry.whichTab == 0)
                    return;
                scrolled = 0;
                ModEntry.whichTab = 0;
                Game1.playSound("shiny4");
                PopulateMailList();
                return;
            }
            if (allMailButton.containsPoint(x, y))
            {
                scrolled = 0;
                if (ModEntry.whichTab == 1)
                    return;
                ModEntry.whichTab = 1;
                Game1.playSound("shiny4");
                PopulateMailList();
                return;
            }
            for(int i = 0; i < contained; i++)
            {
                var cc = currentMailList[i];
                if(cc.containsPoint(x, y))
                {
                    var mailTitle = cc.name;
                    Game1.playSound("shiny4");
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
                    Game1.activeClickableMenu = new MyLetterViewerMenu(mail, mailTitle, !Game1.mailbox.Contains(cc.name));
                    if (Game1.mailbox.Contains(cc.name))
                        Game1.mailbox.Remove(cc.name);
                    return;
                }
            }
            base.receiveLeftClick(x, y, playSound);
        }
        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if(direction < 0 && canScroll)
            {
                int count = ModEntry.whichTab == 0 ? Game1.mailbox.Count : Game1.player.mailReceived.Count;
                if (count < (scrolled + 1) * gridColumns)
                    return;
                scrolled++;
            }
            else if(direction > 0 && scrolled > 0)
            {
                scrolled--;
            }
            else
            {
                return;
            }
            Game1.playSound("shiny4");
            PopulateMailList();
        }

        private void PopulateMailList()
        {            
            currentMailList.Clear();
            if(ModEntry.whichTab == 0)
            {
                for(int i = scrolled * gridColumns; i < Game1.mailbox.Count; i++)
                {
                    AddMail(Game1.mailbox[i], i);
                }
            }
            else if (ModEntry.whichTab == 1)
            {
                Dictionary<string, string> mail = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
                List<string> strings = new List<string>();
                int count = 0;
                for (int i = 0; i < Game1.player.mailReceived.Count; i++)
                {
                    if (Game1.mailbox.Contains(Game1.player.mailReceived[i]) || !mail.ContainsKey(Game1.player.mailReceived[i]))
                        continue; 
                    if(count >= scrolled * gridColumns)
                        AddMail(Game1.player.mailReceived[i], count - scrolled * gridColumns);
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
                mailTitles[id] = split.Length > 0 ? split[1] : "???";
            }
            var gridX = i % gridColumns;
            var gridY = i / gridColumns;
            Texture2D texture;
            if(ModEntry.mailDataDict.TryGetValue(id, out MailData data))
            {
                if (!string.IsNullOrEmpty(data.texturePath))
                {
                    texture = ModEntry.SHelper.GameContent.Load<Texture2D>(data.texturePath);
                }
                else if(string.IsNullOrEmpty(data.sender) || !ModEntry.npcEnvelopeTextures.TryGetValue(data.sender, out texture))
                {
                    texture = ModEntry.mailDataDict["default"].texture;
                }
            }
            else
            {
                data = ModEntry.mailDataDict["default"];
                texture = data.texture;
            }
            Rectangle textureBounds = new Rectangle(0, 0, texture.Width, texture.Height);
            if(data.frames > 1)
            {
                textureBounds.Size = new Point(data.frameWidth, texture.Height);
            }

            currentMailList.Add(new ClickableTextureComponent(id, new Rectangle(xPositionOnScreen + borderWidth + 256 + 10 + gridX * (envelopeWidth + gridSpace), yPositionOnScreen + borderWidth + 64 + gridY * (envelopeHeight + gridSpace), envelopeWidth, envelopeHeight), "", "", texture, textureBounds, data.scale, false)
            {
                hoverText = GetMailName(id),
                myID = mailIndex + i,
                upNeighborID = mailIndex + i - 1,
                downNeighborID = mailIndex + i + 1,
                leftNeighborID = 901,
                region = 15924
            });
        }

        private string GetMailName(string id)
        {
            return id;
        }
    }
}