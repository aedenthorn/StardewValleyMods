using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace ContentPatcherEditor
{
    public class ContentPackMenu : IClickableMenu
    {
        public static int scrolled;
        public ContentPatcherPack pack;
        public static int setsPerPage = 6;
        public static int linesPerPage = 17;
        public static int windowWidth = 64 * 24;
        public List<ChangeSet> changesList = new();
        public List<ClickableComponent> allComponents = new();
        public List<ClickableComponent> tabs = new();
        public List<int> tabTexts = new();
        public List<TextBox> allTextBoxes = new();
        public ClickableTextureComponent addCC;
        public ClickableTextureComponent saveCC;
        public ClickableTextureComponent backCC;
        public ClickableTextureComponent upCC;
        public ClickableTextureComponent downCC;
        public string hoverText;
        public string hoveredItem;
        public int tab;
        private TextBox nameText;
        private int tabHeight;
        private int changeChanging = -1;
        private bool modified;
        public Rectangle changingRect;
        public string[] actionTypes =
        {
            "Load",
            "EditData",
            "EditImage",
            "EditMap",
            "Include"
        };
        public int totalLines;

        public ContentPackMenu(ContentPatcherPack _pack) : base(Game1.uiViewport.Width / 2 - (windowWidth + borderWidth * 2) / 2, -borderWidth, windowWidth + borderWidth * 2, Game1.uiViewport.Height, false)
        {
            pack = _pack;
            setsPerPage = 6;
            linesPerPage = 17;
            width = 64 * 24;
            tabHeight = 80;
            changingRect = new Rectangle(Game1.viewport.Width / 2 - width / 8, Game1.viewport.Height / 2 - height / 4, width / 4, height / 2);
            RepopulateComponentList();
            saveCC = null;

            exitFunction = emergencyShutDown;

            snapToDefaultClickableComponent();
        }

        private void RepopulateComponentList()
        {
            changesList.Clear();
            allComponents.Clear();
            allTextBoxes.Clear();
            Texture2D textBox = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
            int xStart = xPositionOnScreen + spaceToClearSideBorder + borderWidth;
            int yStart = yPositionOnScreen + borderWidth + spaceToClearTopBorder - 24;
            tabs.Clear();
            tabTexts.Clear();
            backCC = new ClickableTextureComponent("Back", new Rectangle(xPositionOnScreen - 44, yStart, 44, 40), "", ModEntry.SHelper.Translation.Get("back"), Game1.mouseCursors, new Rectangle(8, 268, 44, 40), 1)
            {
                myID = -3,
                rightNeighborID = 0,
            };
            tabs.Add(new ClickableComponent(new Rectangle(xStart, yStart, width / 3, tabHeight), "manifest", ModEntry.SHelper.Translation.Get("manifest"))
            {

            });
            tabTexts.Add((int)Game1.dialogueFont.MeasureString(ModEntry.SHelper.Translation.Get("manifest")).X);
            tabs.Add(new ClickableComponent(new Rectangle(xStart + width / 3, yStart, width / 3, tabHeight), "changes", ModEntry.SHelper.Translation.Get("changes"))
            {

            });
            tabTexts.Add((int)Game1.dialogueFont.MeasureString(ModEntry.SHelper.Translation.Get("changes")).X);
            tabs.Add(new ClickableComponent(new Rectangle(xStart + width * 2 / 3, yStart, width / 3, tabHeight), "config", ModEntry.SHelper.Translation.Get("config"))
            {

            });
            tabTexts.Add((int)Game1.dialogueFont.MeasureString(ModEntry.SHelper.Translation.Get("config")).X);
            yStart += tabHeight + 24;
            addCC = null;
            upCC = null;
            downCC = null;
            if (tab == 0)
            {
                int count = 0;
                int lineHeight = 96;
                foreach (var fi in typeof(MyManifest).GetFields())
                {
                    if(fi.FieldType == typeof(string) || fi.FieldType == typeof(Version))
                    {
                        var nameWidth = (int)Game1.dialogueFont.MeasureString(fi.Name).X;
                        allTextBoxes.Add(new TextBox(textBox, null, Game1.smallFont, Game1.textColor)
                        {
                            X = xStart + nameWidth,
                            Y = yStart + lineHeight * count,
                            Width = width - nameWidth - (spaceToClearSideBorder + borderWidth) * 2,
                            Text = fi.GetValue(pack.manifest).ToString()
                        });
                        allComponents.Add(new ClickableComponent(new Rectangle(xStart, yStart + lineHeight * count, width, lineHeight), fi.Name)
                        {
                            myID = 1000 * count,
                            upNeighborID = 1000 * (count - 1),
                            downNeighborID = 1000 * (count + 1)
                        });
                        count++;
                    }
                }
            }
            else if (tab == 1)
            {
                int count = 0;
                int setHeight = 0;
                int lineHeight = 64;
                var lines = 0;
                for(int i = 0; i < pack.content.Changes.Count; i++)
                {
                    var change = pack.content.Changes[i];
                    ChangeSet set = null;

                    switch ((string)change["Action"])
                    {
                        case "Load":
                            set = new LoadSet(xStart, yStart + setHeight * count, width - (spaceToClearSideBorder + borderWidth) * 2, lineHeight, ref lines, change);
                            break;
                        case "EditData":
                            set = new EditDataSet(xStart, yStart + setHeight * count, width - (spaceToClearSideBorder + borderWidth) * 2, lineHeight, ref lines, change);
                            break;
                        case "EditImage":
                            set = new EditImageSet(xStart, yStart + setHeight * count, width - (spaceToClearSideBorder + borderWidth) * 2, lineHeight, ref lines, change);
                            break;
                        case "EditMap":
                            set = new EditMapSet(xStart, yStart + setHeight * count, width - (spaceToClearSideBorder + borderWidth) * 2, lineHeight, ref lines, change);
                            break;
                        case "Include":
                            set = new IncludeSet(xStart, yStart + setHeight * count, width - (spaceToClearSideBorder + borderWidth) * 2, lineHeight, ref lines, change);
                            break;

                    }
                    if (set is not null)
                    {
                        allComponents.AddRange(set.Update);
                        if(set.ActionCC is not null)
                            allComponents.Add(set.ActionCC);
                        allComponents.AddRange(set.WhenSubCC);
                        changesList.Add(set);
                    }
                    count++;
                }
                totalLines = lines;
                addCC = new ClickableTextureComponent("Add", new Rectangle(xPositionOnScreen + width - 104, yPositionOnScreen - 96 + height, 56, 56), "", ModEntry.SHelper.Translation.Get("add"), Game1.mouseCursors, new Rectangle(1, 412, 14, 14), 4)
                {
                    myID = count * 1000,
                    upNeighborID = (count - 1) * 1000,
                    rightNeighborID = -2,
                };
                if (scrolled > 0)
                {
                    upCC = new ClickableTextureComponent("Up", new Rectangle(xPositionOnScreen + width + 40, yPositionOnScreen + 84, 40, 44), "", ModEntry.SHelper.Translation.Get("up"), Game1.mouseCursors, new Rectangle(76, 72, 40, 44), 1)
                    {
                        myID = -1,
                        leftNeighborID = 0,
                        downNeighborID = -2,
                    };

                }
                else
                    upCC = null;
                if (scrolled < totalLines - linesPerPage)
                {
                    downCC = new ClickableTextureComponent("Down", new Rectangle(xPositionOnScreen + width + 40, yPositionOnScreen + height - 64, 40, 44), "", ModEntry.SHelper.Translation.Get("down"), Game1.mouseCursors, new Rectangle(12, 76, 40, 44), 1)
                    {
                        myID = -2,
                        leftNeighborID = 0,
                        upNeighborID = -1,
                    };
                }
                else
                    downCC = null;
            }
            populateClickableComponentList();
        }

        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true);
            int count = 0;
            backCC.draw(b);
            for(int i = 0; i < tabs.Count; i++)
            {
                var pos = new Vector2(tabs[i].bounds.Center.X - tabTexts[i] / 2, tabs[i].bounds.Y);
                if(tab == i)
                {
                    b.DrawString(Game1.dialogueFont, tabs[i].label, pos + new Vector2(-1, 1), Color.Black * 0.5f);
                    b.DrawString(Game1.dialogueFont, tabs[i].label, pos, Color.Brown);
                }
                else
                {
                    b.DrawString(Game1.dialogueFont, tabs[i].label, pos, Color.Gray);
                }
            }
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + borderWidth + spaceToClearTopBorder - 48 + tabHeight, width - 64, 16), new Rectangle(40, 16, 1, 16), Color.White);
            if (tab == 0)
            {
                for (int i = 0; i <  allTextBoxes.Count; i++)
                {
                    allTextBoxes[i].Draw(b);
                    b.DrawString(Game1.dialogueFont, allComponents[i].name, allComponents[i].bounds.Location.ToVector2(), Color.Brown);
                }
            }
            else if(tab == 1)
            {
                for (int i = 0; i < changesList.Count; i++)
                {
                    var set = changesList[i];
                    if(i > 0 && set.Action is not null)
                    {
                        b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + spaceToClearSideBorder + 16, set.Action.Y - 14, width - 64, 8), new Rectangle(40, 16, 1, 16), Color.White);
                    }
                    set.DeleteCC?.draw(b);
                    set.Action?.Draw(b);
                    set.WhenAddCC?.draw(b);
                    foreach(var cc in set.WhenSubCC)
                    {
                        cc.draw(b);
                    }
                    if(changesList[i].LogName is not null)
                    {
                        changesList[i].LogName.Draw(b);
                    }
                    if(changesList[i].When is not null)
                    {
                        foreach(var when in changesList[i].When) 
                        {
                            foreach(var tb in when)
                            {
                                tb.Draw(b);
                            }
                        }
                    }
                    foreach (var cc in changesList[i].Update)
                    {
                        var p = new Vector2(cc.bounds.Center.X - Game1.dialogueFont.MeasureString(cc.name).X / 2, cc.bounds.Y);
                        b.DrawString(Game1.dialogueFont, cc.name, p + new Vector2(-1, 1), Color.Black * 0.5f);
                        b.DrawString(Game1.dialogueFont, cc.name, p, cc.label == "active" ? Color.Brown : Color.Gray);
                    }
                    foreach (var l in changesList[i].labels)
                    {
                        b.DrawString(Game1.dialogueFont, l.Value, l.Key, Color.Brown);
                    }

                    if (set is LoadSet)
                    {
                        (set as LoadSet).Target?.Draw(b);
                        (set as LoadSet).FromFile?.Draw(b);
                    }
                    else if (set is EditDataSet)
                    {
                        (set as EditDataSet).Target?.Draw(b);
                        (set as EditDataSet).EntriesAddCC?.draw(b);
                        foreach(var cc in (set as EditDataSet).EntriesSubCC)
                        {
                            cc.draw(b);
                        }
                        foreach (var e in (set as EditDataSet).Entries)
                        {
                            foreach (var tb in e)
                            {
                                tb.Draw(b);
                            }
                        }
                    }
                    upCC?.draw(b);
                    downCC?.draw(b);
                }
                b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + spaceToClearSideBorder + 16, yPositionOnScreen + height - 112, width - 64, 8), new Rectangle(40, 16, 1, 16), Color.White);
                addCC.draw(b);
            }
            saveCC?.draw(b);
            if (changeChanging > -1)
            {
                Game1.drawDialogueBox(changingRect.X, changingRect.Y, changingRect.Width, changingRect.Height, false, true, null, false, true);
                float h = (changingRect.Height - spaceToClearTopBorder - 56) / actionTypes.Length;
                var pos = Game1.getMousePosition();
                int wi = changingRect.Contains(pos) ? (int)((pos.Y - changingRect.Y - spaceToClearTopBorder) / (float)(changingRect.Height - spaceToClearTopBorder - 56) * actionTypes.Length) : -1;
                for (int i = 0; i < actionTypes.Length; i++)
                {
                    SpriteText.drawStringHorizontallyCenteredAt(b, actionTypes[i], changingRect.Center.X, changingRect.Y + spaceToClearTopBorder + (int)( h * i + h / 2 - SpriteText.characterHeight), color: wi != i ? -1 : 2);
                }
            }
            if (hoverText != null && hoveredItem == null)
            {
                drawHoverText(b, hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, -1, -1, -1, -1, 1f, null, null);
            }
            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if(changeChanging > -1)
            {
                var pos = Game1.getMousePosition();
                
                if (changingRect.Contains(pos.X, pos.Y))
                {
                    int wi = (int)((pos.Y - changingRect.Y - spaceToClearTopBorder) / (float)(changingRect.Height - spaceToClearTopBorder - 56) * actionTypes.Length);
                    string which = actionTypes[wi];
                    var newChange = ModEntry.CreateNewChange(which);
                    pack.content.Changes[changeChanging] = newChange;
                }
                Game1.playSound("bigSelect");
                changeChanging = -1;
                RepopulateComponentList();
                return;
            }
            if (saveCC?.containsPoint(x, y) == true)
            {
                ModEntry.SaveContentPack(pack);
                Game1.playSound("bigSelect");
                saveCC = null;
                return;
            }
            if (backCC.containsPoint(x, y))
            {
                Game1.playSound("bigDeSelect");
                Game1.activeClickableMenu = new ContentPatcherMenu();
                return;
            }
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].containsPoint(x, y))
                {
                    Game1.playSound("bigSelect");
                    scrolled = 0;
                    tab = i;
                    RepopulateComponentList();
                    return;
                }
            }
            if (tab == 0)
            {
                foreach (var tb in allTextBoxes)
                {
                    tb.Selected = false;
                    tb.Update();
                }
            }
            else if(tab == 1)
            {
                for (int i = 0; i < changesList.Count; i++)
                {
                    var set = changesList[i];
                    if(set.DeleteCC?.containsPoint(x, y) == true)
                    {
                        Game1.playSound("trashcan");
                        pack.content.Changes.RemoveAt(i);
                        ShowSaveButton();
                        RepopulateComponentList();
                        return;
                    }
                    if(set.WhenAddCC?.containsPoint(x, y) == true)
                    {
                        Game1.playSound("bigSelect");
                        if (!pack.content.Changes[i].TryGetValue("When", out var whenObj))
                        {
                            whenObj = new JObject();
                        }
                        var when = (JObject)whenObj;
                        int count = 0;
                        while (when.ContainsKey(count == 0 ? "" : count + ""))
                        {
                            count++;
                        }
                        when.Add(count == 0 ? "" : count + "", "");
                        pack.content.Changes[i]["When"] = when;
                        ShowSaveButton();
                        RepopulateComponentList();
                        return;
                    }
                    if(set.WhenSubCC.Count > 0)
                    {
                        for (int j = 0; j < set.WhenSubCC.Count; j++)
                        {
                            if (set.WhenSubCC[j].containsPoint(x, y))
                            {
                                ((JObject)pack.content.Changes[i]["When"]).Remove(set.When[j][0].Text);
                                Game1.playSound("trashcan");
                                ShowSaveButton();
                                RepopulateComponentList();
                                return;
                            }
                        }
                    }
                    if(set.Action is not null)
                    {
                        set.Action.Selected = false;
                        if (set.ActionCC.containsPoint(x, y))
                        {
                            changeChanging = i;
                            Game1.playSound("bigSelect");
                            return;
                        }
                    }
                    if(set.LogName is not null)
                    {
                        set.LogName.Selected = false;
                        set.LogName.Update();
                    }
                    foreach (var w in set.When)
                    {
                        foreach(var tb in w)
                        {
                            tb.Selected = false;
                            tb.Update();
                        }
                    }
                    foreach (var u in set.Update)
                    {
                        if(u.containsPoint(x, y))
                        {
                            Game1.playSound("bigSelect");
                            if (!pack.content.Changes[i].TryGetValue("Update", out var updateObj))
                                updateObj = "OnDayStart";
                            var updateString = (string)updateObj;
                            if (u.label == "active")
                            {
                                u.label = "inactive";
                                var list = updateString.Split(',');
                                var newList = new List<string>();
                                foreach(var s in list)
                                {
                                    if(s.Trim() != u.name)
                                        newList.Add(s.Trim());
                                }
                                pack.content.Changes[i]["Update"] = string.Join(",", newList);
                            }
                            else
                            {
                                u.label = "active";
                                pack.content.Changes[i]["Update"] = updateString + "," + u.name;
                            }
                            ShowSaveButton();
                            RepopulateComponentList();
                            return;
                        }
                    }
                    if(set is LoadSet)
                    {
                        if((set as LoadSet).Target is not null)
                        {
                            (set as LoadSet).Target.Selected = false;
                            (set as LoadSet).Target.Update();
                        }
                        if((set as LoadSet).FromFile is not null)
                        {
                            (set as LoadSet).FromFile.Selected = false;
                            (set as LoadSet).FromFile.Update();
                        }
                    }
                    if(set is EditDataSet)
                    {
                        if((set as EditDataSet).EntriesAddCC.containsPoint(x, y))
                        {
                            Game1.playSound("bigSelect");
                            if (!pack.content.Changes[i].TryGetValue("Entries", out var entriesObj))
                            {
                                entriesObj = new JObject();
                            }
                            var entries = (JObject)entriesObj;
                            int count = 0;
                            while (entries.ContainsKey(count == 0 ? "" : count + ""))
                            {
                                count++;
                            }
                            entries.Add(count == 0 ? "" : count + "", "");
                            pack.content.Changes[i]["Entries"] = entries;
                            ShowSaveButton();
                            RepopulateComponentList();
                            return;
                        }
                        if((set as EditDataSet).Target is not null)
                        {
                            (set as EditDataSet).Target.Selected = false;
                            (set as EditDataSet).Target.Update();
                        }
                        foreach (var e in (set as EditDataSet).Entries)
                        {
                            foreach(var tb in e)
                            {
                                tb.Selected = false;
                                if (pack.content.Changes[i]["Entries"][0].Type == JTokenType.String)
                                    tb.Update();
                            }
                        }
                        if ((set as EditDataSet).EntriesSubCC.Count > 0)
                        {
                            for (int j = 0; j < (set as EditDataSet).EntriesSubCC.Count; j++)
                            {
                                if ((set as EditDataSet).EntriesSubCC[j].containsPoint(x, y))
                                {
                                    ((JObject)pack.content.Changes[i]["Entries"]).Remove((set as EditDataSet).Entries[j][0].Text);
                                    Game1.playSound("trashcan");
                                    ShowSaveButton();
                                    RepopulateComponentList();
                                    return;
                                }
                            }
                        }
                    }
                }
                if (addCC.containsPoint(x, y))
                {
                    pack.content.Changes.Add(ModEntry.CreateNewChange("Load"));
                    ShowSaveButton();
                    Game1.playSound("bigSelect");
                    RepopulateComponentList();
                    return;
                }
                if (upCC?.containsPoint(x, y) == true)
                {
                    if (DoScroll(1))
                    {
                        RepopulateComponentList();
                    }
                    return;
                }
                if (downCC?.containsPoint(x, y) == true)
                {
                    if (DoScroll(-1))
                    {
                        RepopulateComponentList();
                    }
                    return;
                }
            }

        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            base.receiveRightClick(x, y, playSound);
        }
        public override void receiveScrollWheelAction(int direction)
        {
            DoScroll(direction);
            RepopulateComponentList();
        }

        private bool DoScroll(int direction)
        {
            if(tab == 0)
            {
                return false;
            }
            else
            {
                if (direction < 0 && scrolled < totalLines - linesPerPage)
                {
                    Game1.playSound("shiny4");
                    scrolled++;
                }
                else if (direction > 0 && scrolled > 0)
                {
                    Game1.playSound("shiny4");
                    scrolled--;
                }
                else
                {
                    return false;
                }
                return true;
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            bool close = Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose();
            if (!Game1.options.snappyMenus || !Game1.options.gamepadControls)
            {
                if (tab == 0)
                {
                    for (int i = 0; i < allTextBoxes.Count; i++)
                    {
                        var tb = allTextBoxes[i];
                        if (!tb.Selected)
                            continue;
                        var fi = typeof(MyManifest).GetField(allComponents[i].name);
                        if(fi.GetValue(pack.manifest).ToString() != tb.Text)
                        {
                            Game1.playSound("cowboy_monsterhit");
                            if (fi.FieldType == typeof(string))
                                fi.SetValue(pack.manifest, tb.Text);
                            else
                                fi.SetValue(pack.manifest, new Version(tb.Text));
                            ShowSaveButton();
                            return;
                        }
                    }
                }
                else if (tab == 1)
                {
                    for (int i = 0; i < changesList.Count; i++)
                    {
                        var set = changesList[i];
                        if (set.Action?.Selected == true && (string)pack.content.Changes[i]["Action"] != set.Action.Text)
                        {
                            Game1.playSound("cowboy_monsterhit");
                            pack.content.Changes[i]["Action"] = set.Action.Text;
                            ShowSaveButton();
                            return;
                        }
                        if (set.LogName?.Selected == true && (string)pack.content.Changes[i]["LogName"] != set.Action.Text)
                        {
                            Game1.playSound("cowboy_monsterhit");
                            pack.content.Changes[i]["LogName"] = set.LogName.Text;
                            ShowSaveButton();
                            return;
                        }
                        var dict = new JObject();
                        bool changed = false;
                        foreach (var w in set.When)
                        {
                            dict[w[0].Text] = w[1].Text;
                            foreach (var tb in w)
                            {
                                if (!changed && tb.Selected && (!((JObject)pack.content.Changes[i]["When"]).TryGetValue(w[0].Text, out JToken? v) || (string)v != w[1].Text))
                                {
                                    changed = true;
                                }
                            }
                        }
                        if (changed)
                        {
                            pack.content.Changes[i]["When"] = dict;
                            Game1.playSound("cowboy_monsterhit");
                            ShowSaveButton();
                            return;
                        }
                        if (set is LoadSet)
                        {
                            if ((set as LoadSet).Target?.Selected == true && (string)pack.content.Changes[i]["Target"] != (set as LoadSet).Target.Text)
                            {
                                Game1.playSound("cowboy_monsterhit");
                                pack.content.Changes[i]["Target"] = (set as LoadSet).Target.Text;
                                ShowSaveButton();
                                return;
                            }
                            if ((set as LoadSet).FromFile?.Selected == true && (string)pack.content.Changes[i]["FromFile"] != (set as LoadSet).FromFile.Text)
                            {
                                Game1.playSound("cowboy_monsterhit");
                                pack.content.Changes[i]["FromFile"] = (set as LoadSet).FromFile.Text;
                                ShowSaveButton();
                                return;
                            }
                        }
                        else if (set is EditDataSet)
                        {
                            if ((set as EditDataSet).Target?.Selected == true && (string)pack.content.Changes[i]["Target"] != (set as EditDataSet).Target.Text)
                            {
                                Game1.playSound("cowboy_monsterhit");
                                pack.content.Changes[i]["Target"] = (set as EditDataSet).Target.Text;
                                ShowSaveButton();
                                return;
                            }
                            dict = new JObject();
                            changed = false;
                            foreach (var e in (set as EditDataSet).Entries)
                            {
                                dict[e[0].Text] = e[1].Text;
                                foreach (var tb in e)
                                {
                                    if (!changed && tb.Selected && (!((JObject)pack.content.Changes[i]["Entries"]).TryGetValue(e[0].Text, out JToken? v) || (string)v != e[1].Text))
                                    {
                                        changed = true;
                                    }
                                }
                            }
                            if (changed)
                            {
                                pack.content.Changes[i]["Entries"] = dict;
                                Game1.playSound("cowboy_monsterhit");
                                ShowSaveButton();
                                return;
                            }
                        }
                    }
                }
            }
            if (close)
            {
                exitThisMenu(true);
            }
            else if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                applyMovementKey(key);
            }
        }

        private void ShowSaveButton()
        {
            saveCC = new ClickableTextureComponent("Save", new Rectangle(xPositionOnScreen + spaceToClearSideBorder + borderWidth, yPositionOnScreen - 100 + height, 56, 56), "", ModEntry.SHelper.Translation.Get("save"), Game1.mouseCursors, new Rectangle(241, 320, 16, 16), 4)
            {
                myID = -4,
                upNeighborID = 0,
                rightNeighborID = -2,
                leftNeighborID = -3
            };
        }

        public override void snapToDefaultClickableComponent()
        {
            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                currentlySnappedComponent = this.getComponentWithID(0);
                snapCursorToCurrentSnappedComponent();
            }
        }
        public override void applyMovementKey(int direction)
        {

            if (currentlySnappedComponent != null)
            {
                ClickableComponent next = null;
                switch (direction)
                {
                    case 0:
                        next = getComponentWithID(currentlySnappedComponent.upNeighborID);
                        break;
                    case 1:
                        next = getComponentWithID(currentlySnappedComponent.rightNeighborID);
                        break;
                    case 2:
                        next = getComponentWithID(currentlySnappedComponent.downNeighborID);
                        break;
                    case 3:
                        next = getComponentWithID(currentlySnappedComponent.leftNeighborID);
                        break;
                }
                if (next is not null)
                {
                    Game1.playSound("shiny4");
                    currentlySnappedComponent = next;
                    snapCursorToCurrentSnappedComponent();
                }
            }
        }
        public override void update(GameTime time)
        {
            base.update(time);
        }

        public override void performHoverAction(int x, int y)
        {
            hoveredItem = null;
            hoverText = "";
            base.performHoverAction(x, y);
            if(tab == 0)
            {

                if (saveCC?.containsPoint(x, y) == true)
                {
                    hoverText = saveCC.hoverText;
                    return;
                }
            }
            else if (tab == 1)
            {
                for (int i = 0; i < changesList.Count; i++)
                {
                }
                if (addCC.containsPoint(x, y))
                {
                    hoverText = addCC.hoverText;
                    return;
                }
                if (upCC?.containsPoint(x, y) == true)
                {
                    hoverText = upCC.hoverText;
                    return;
                }
                if (downCC?.containsPoint(x, y) == true)
                {
                    hoverText = downCC.hoverText;
                    return;
                }
            }
        }
        public override void emergencyShutDown()
        {
            base.emergencyShutDown();
        }

    }
}
