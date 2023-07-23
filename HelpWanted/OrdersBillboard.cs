using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Quests;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HelpWanted
{
    public class OrdersBillboard : Billboard
    {
        public static List<ClickableTextureComponent> ccList = new();
        public static Dictionary<int, IQuestData> questDict = new();
        public static Rectangle boardRect = new Rectangle(78 * 4, 58 * 4, 184 * 4, 96 * 4);
        public static int ccIndex = -4200;
        public Texture2D billboardTexture;

        public static int showingQuest;
        public static Billboard questBillboard;

        public string hoverTitle = "";
        public string hoverText = "";

        public OrdersBillboard() : base(true)
        {
            billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Billboard");
            questBillboard = null;
            if (ModEntry.questList.Count > 0)
            {
                ccList.Clear();
                questDict.Clear();
                List<IQuestData> quests = ModEntry.questList;

                for (int i = 0; i < quests.Count; i++)
                {
                    Point size = new Point((int)(quests[i].padTextureSource.Width * ModEntry.Config.NoteScale), (int)(quests[i].padTextureSource.Height * ModEntry.Config.NoteScale));
                    Rectangle? bounds = GetFreeBounds(size.X, size.Y);
                    if (bounds is null)
                        break;
                    ccList.Add(new ClickableTextureComponent(bounds.Value, quests[i].padTexture, quests[i].padTextureSource, ModEntry.Config.NoteScale) 
                    { 
                        myID = ccIndex - i,
                        leftNeighborID = i > 0 ? ccIndex - i + 1 : -1,
                        rightNeighborID = i < quests.Count - 1 ? ccIndex - i  - 1 : -1
                    });
                    questDict[ccIndex - i] = quests[i];
                }
                ModEntry.questList.Clear();
            }
            exitFunction = delegate ()
            {
                if (questBillboard is not null)
                    Game1.activeClickableMenu = new OrdersBillboard();
            };
            populateClickableComponentList();
        }


        public override void performHoverAction(int x, int y)
        {
            if(questBillboard is not null)
            {
                questBillboard.performHoverAction(x, y);
                return;
            }
            hoverTitle = "";
            hoverText = "";
            foreach (var cc in ccList)
            {
                //cc.tryHover(x, y);
                if (cc.containsPoint(x, y))
                {
                    hoverTitle = questDict[cc.myID].quest.questTitle;
                    hoverText = questDict[cc.myID].quest.currentObjective;
                    break;
                }
            }

        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if(questBillboard is null)
            {
                foreach (var cc in ccList)
                {
                    if (cc.containsPoint(x, y))
                    {
                        if (questDict[cc.myID].acceptable)
                        {
                            Game1.questOfTheDay = questDict[cc.myID].quest;
                            showingQuest = cc.myID;
                            questBillboard = new Billboard(true);
                        }
                        return;
                    }
                }
                var method = AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.receiveLeftClick));
                var ftn = method.MethodHandle.GetFunctionPointer();
                var func = (Action<int, int, bool>)Activator.CreateInstance(typeof(Action<int, int, bool>), this, ftn);
                func.Invoke(x, y, playSound);
            }
            else
            {
                questBillboard.receiveLeftClick(x, y, playSound);
            }
        }
        public override bool readyToClose()
        {
            if(questBillboard is not null)
            {
                questBillboard = null;
                return false;
            }
            return true;
        }

        public override void applyMovementKey(int direction)
        {
            if(questBillboard is null)
            {
                base.applyMovementKey(direction);
            }
            else
            {
                questBillboard.applyMovementKey(direction);
            }
        }
        public override void automaticSnapBehavior(int direction, int oldRegion, int oldID)
        {
            if(questBillboard is null)
            {
                base.automaticSnapBehavior(direction, oldRegion, oldID);
            }
            else
            {
                questBillboard.automaticSnapBehavior(direction, oldRegion, oldID);
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            if (questBillboard is null)
            {
                base.snapToDefaultClickableComponent();
                currentlySnappedComponent = getComponentWithID(ccIndex);
                snapCursorToCurrentSnappedComponent();
            }
            else
            {
                questBillboard.snapToDefaultClickableComponent();
            }
        }

        public override void draw(SpriteBatch b)
        {
            if(questBillboard is not null)
            {
                questBillboard.draw(b);
                return;
            }
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            b.Draw(billboardTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(0, 0, 338, 198), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);

            if (!ccList.Any())
            {
                b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:Billboard_NothingPosted"), new Vector2(xPositionOnScreen + 384, yPositionOnScreen + 320), Game1.textColor);
            }
            else
            {
                foreach (var c in ccList)
                {
                    c.draw(b, questDict[c.myID].padColor, 1);
                    b.Draw(questDict[c.myID].pinTexture, c.bounds, questDict[c.myID].iconSource, questDict[c.myID].pinColor);
                    if (questDict[c.myID].icon is not null)
                    {
                        b.Draw(questDict[c.myID].icon, new Vector2(c.bounds.X + questDict[c.myID].iconOffset.X, c.bounds.Y + questDict[c.myID].iconOffset.Y), questDict[c.myID].iconSource, questDict[c.myID].iconColor, 0, Vector2.Zero, questDict[c.myID].iconScale, SpriteEffects.FlipHorizontally, 1);
                    }
                }
            }
            if (upperRightCloseButton != null && shouldDrawCloseButton())
            {
                upperRightCloseButton.draw(b);
            }
            if (hoverText?.Length > 0)
            {
                drawHoverText(b, hoverText, Game1.smallFont, 0, 0, -1, (hoverTitle.Length > 0) ? hoverTitle : null, -1, null, null, 0, -1, -1, -1, -1, 1f, null, null);
            }
            var method = AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.draw), new Type[] { typeof(SpriteBatch) });
            var ftn = method.MethodHandle.GetFunctionPointer();
            var func = (Action<SpriteBatch>)Activator.CreateInstance(typeof(Action<SpriteBatch>), this, ftn);
            func.Invoke(b);
            Game1.mouseCursorTransparency = 1f;
            drawMouse(b, false, -1);
        }
        private Rectangle? GetFreeBounds(int w, int h)
        {
            if(w >= boardRect.Width || h >= boardRect.Height)
            {
                ModEntry.SMonitor.Log($"note size {w},{h} is too big for the screen", StardewModdingAPI.LogLevel.Warn);
                return null;
            }
            int tries = 10000;
            while(tries > 0)
            {
                Rectangle rect = new Rectangle(xPositionOnScreen + Game1.random.Next(boardRect.X, boardRect.Right - w), yPositionOnScreen + Game1.random.Next(boardRect.Y, boardRect.Bottom - h), w, h);
                foreach(var cc in ccList)
                {
                    if (Math.Abs(cc.bounds.Center.X - rect.Center.X) < rect.Width * ModEntry.Config.XOverlapBoundary || Math.Abs(cc.bounds.Center.Y - rect.Center.Y) < rect.Height * ModEntry.Config.YOverlapBoundary)
                        goto cont;
                }
                return rect;
            cont:
                tries--;
            }
            return null;
        }
    }
}