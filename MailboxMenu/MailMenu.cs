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
        public static int whichTab;
        public static string whichSender;
        public static bool preserveScroll;
        public static int mainScrolled;
        public static int sideScrolled;

        public ClickableComponent inboxButton;
        public ClickableComponent allMailButton;
        public List<ClickableTextureComponent> currentMailList = new List<ClickableTextureComponent>();
        public List<ClickableComponent> senders = new List<ClickableComponent>();
        public List<string> possibleSenders = new List<string>();
        public int mailIndex = 99942000;
        public Dictionary<string, string> mailTitles = new Dictionary<string, string>();
        public bool canScroll;
        public int lastVisibleMailId;
        public int contained;

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

            ResetPositions();
        }

        private void ResetPositions()
        {
            var textHeight = (int)Game1.dialogueFont.MeasureString(ModEntry.Config.InboxText).Y;
            currentMailList = new List<ClickableTextureComponent>();
            inboxButton = new ClickableComponent(new Rectangle(xPositionOnScreen + borderWidth + 16, yPositionOnScreen + borderWidth + 64, ModEntry.Config.SideWidth - 16, textHeight), "Inbox")
            {
                myID = 900,
                downNeighborID = 901,
                rightNeighborID = mailIndex,
                region = 42
            };
            allMailButton = new ClickableComponent(new Rectangle(xPositionOnScreen + borderWidth + 16, yPositionOnScreen + borderWidth + 64 + textHeight + 8, ModEntry.Config.SideWidth - 16, textHeight), "Archive")
            {
                myID = 901,
                upNeighborID = 900,
                downNeighborID = 902,
                rightNeighborID = mailIndex,
                region = 42
            };
            PopulateSenders();
            PopulateMailList();
            snapToDefaultClickableComponent();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            xPositionOnScreen = Math.Max(0, Game1.uiViewport.Width / 2 - (ModEntry.Config.WindowWidth + borderWidth * 2) / 2);
            yPositionOnScreen = Math.Max(0, Game1.uiViewport.Height / 2 - (ModEntry.Config.WindowHeight + borderWidth * 2) / 2);
            width = Math.Min(Game1.uiViewport.Width, ModEntry.Config.WindowWidth + borderWidth * 2);
            height = Math.Min(Game1.uiViewport.Height, ModEntry.Config.WindowHeight + borderWidth * 2);
            ResetPositions();
        }
        private void PopulateSenders()
        {
            senders.Clear();
            possibleSenders.Clear();
            bool addUnknown = false;
            Dictionary<string, string> mail = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
            foreach (var id in Game1.player.mailReceived)
            {
                if (!mail.ContainsKey(id))
                    continue;
                if(ModEntry.envelopeData.TryGetValue(id, out EnvelopeData data) && !string.IsNullOrEmpty(data.sender))
                {
                    if(!possibleSenders.Contains(data.sender))
                        possibleSenders.Add(data.sender);
                }
                else
                {
                    addUnknown = true;
                }
            }
            var textHeight = (int)Game1.dialogueFont.MeasureString(ModEntry.Config.InboxText).Y;
            var textHeight2 = (int)Game1.smallFont.MeasureString(ModEntry.Config.InboxText).Y;
            int count = (height - borderWidth * 2 - 64 - (textHeight + 8) * 2) / textHeight2;
            possibleSenders.Sort();
            if (addUnknown)
                possibleSenders.Add("???");
            var list = possibleSenders.Skip(sideScrolled).Take(count).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                senders.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + borderWidth + 16, yPositionOnScreen + borderWidth + 64 + (textHeight + 8) * 2 + i * textHeight2, ModEntry.Config.SideWidth - borderWidth / 2, textHeight2), list[i])
                {
                    myID = 902 + i,
                    upNeighborID = 902 + i - 1,
                    downNeighborID = 902 + i + 1,
                    rightNeighborID = mailIndex,
                    region = 42
                });
            }
            populateClickableComponentList();
        }

        private void PopulateMailList()
        {
            Dictionary<string, string> mail = Game1.content.Load<Dictionary<string, string>>("Data\\mail");

            currentMailList.Clear();
            if (whichTab == 0)
            {
                for (int i = mainScrolled * ModEntry.Config.GridColumns; i < Game1.mailbox.Count; i++)
                {
                    if (mail.TryGetValue(Game1.mailbox[i], out string mailData))
                        AddMail(Game1.mailbox[i], i, mailData);
                    else
                    {
                        AddMail(Game1.mailbox[i], i, "");
                    }
                }
            }
            else
            {
                List<string> strings = new List<string>();
                int count = 0;
                for (int i = 0; i < Game1.player.mailReceived.Count; i++)
                {
                    string id = Game1.player.mailReceived[i];
                    if (!mail.TryGetValue(id, out string mailData))
                        continue;
                    if (Game1.mailbox.Contains(id))
                        continue;
                    if (whichSender is not null)
                    {
                        if (ModEntry.envelopeData.TryGetValue(id, out EnvelopeData data))
                        {
                            if(data.sender != whichSender)
                                continue;
                        }
                        else if(whichSender != "???") 
                        {
                            continue;
                        }
                    }
                    if (count >= mainScrolled * ModEntry.Config.GridColumns)
                    {
                            AddMail(id, count - mainScrolled * ModEntry.Config.GridColumns, mailData);
                    }
                    count++;
                }
            }
            populateClickableComponentList();
        }

        private void AddMail(string id, int i, string mailData)
        {
            if (!mailTitles.ContainsKey(id))
            {
                string[] split = mailData.Split(new string[]
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
            int xOffset =  Math.Max(0, (width - (borderWidth * 2 + ModEntry.Config.SideWidth) - (ModEntry.Config.EnvelopeWidth + ModEntry.Config.GridSpace) * ModEntry.Config.GridColumns) / 2);
            currentMailList.Add(new ClickableTextureComponent(id, new Rectangle(xPositionOnScreen + borderWidth * 2 + ModEntry.Config.SideWidth + xOffset + gridX * (ModEntry.Config.EnvelopeWidth + ModEntry.Config.GridSpace), yPositionOnScreen + borderWidth + 132 + gridY * (ModEntry.Config.EnvelopeHeight + ModEntry.Config.GridSpace + 16), ModEntry.Config.EnvelopeWidth, ModEntry.Config.EnvelopeHeight), "", "", texture, textureBounds, data.scale, false)
            {
                hoverText = mailTitles[id],
                myID = mailIndex + i,
                upNeighborID = mailIndex + i - ModEntry.Config.GridColumns,
                downNeighborID = mailIndex + i + ModEntry.Config.GridColumns,
                leftNeighborID = (i % ModEntry.Config.GridColumns == 0 ? 901 : mailIndex + i - 1),
                rightNeighborID = (i % ModEntry.Config.GridColumns == ModEntry.Config.GridColumns - 1 ? -99999 : mailIndex + i + 1),
                region = 4242
            });
        }
        public override void snapToDefaultClickableComponent()
        {
            if (!Game1.options.snappyMenus || !Game1.options.gamepadControls)
                return;
            if(currentlySnappedComponent == null)
                currentlySnappedComponent = getComponentWithID(900);
            snapCursorToCurrentSnappedComponent();
        }
        public override void applyMovementKey(int direction)
        {
            if(currentlySnappedComponent != null)
            {
                if (direction == 1 && currentlySnappedComponent.rightNeighborID == -99999)
                    return;
                if (direction == 0)
                {
                    if (currentlySnappedComponent.region == 4242 && mainScrolled > 0 && currentlySnappedComponent.myID < mailIndex + ModEntry.Config.GridColumns)
                    {
                        mainScrolled--;
                        Game1.playSound("shiny4");
                        PopulateMailList();
                        return;
                    }
                    if (currentlySnappedComponent.region == 42 && sideScrolled > 0 && currentlySnappedComponent.myID == 902)
                    {
                        sideScrolled--;
                        Game1.playSound("shiny4");
                        PopulateSenders();
                        return;
                    }
                }
                else if (direction == 2)
                {
                    if (currentlySnappedComponent.region == 4242 && canScroll && currentlySnappedComponent.myID > lastVisibleMailId - ModEntry.Config.GridColumns && currentlySnappedComponent.myID < mailIndex + currentMailList.Count - ModEntry.Config.GridColumns)
                    {
                        mainScrolled++;
                        Game1.playSound("shiny4");
                        PopulateMailList();
                        return;
                    }
                    if (currentlySnappedComponent.region == 42 && possibleSenders.Count > sideScrolled + senders.Count && currentlySnappedComponent.myID == 901 + senders.Count)
                    {
                        sideScrolled++;
                        Game1.playSound("shiny4");
                        PopulateSenders();
                        return;
                    }
                }
            }
            base.applyMovementKey(direction);
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
                int frameOffset = (ModEntry.envelopeData.TryGetValue(cc.name, out EnvelopeData data) && data.frames > 1) ? (int)(Game1.currentGameTime.TotalGameTime.TotalSeconds / data.frameSeconds) % data.frames : 0;

                if (cc.bounds.Y + cc.bounds.Height >= cutoff)
                {
                    cc.sourceRect = new Rectangle(xOffset, 0, width, Math.Min((cutoff - cc.bounds.Y) / (ModEntry.Config.EnvelopeWidth / cc.texture.Width), cc.texture.Height));
                    cc.draw(b, Color.White, 1, frameOffset);
                    canScroll = true;
                    continue;
                }
                else
                {
                    lastVisibleMailId = cc.myID;
                    cc.draw(b, Color.White, 1, frameOffset);
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
                    string mail = ModEntry.GetMailString(mailTitle);
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