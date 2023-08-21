using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection.Emit;

namespace ContentPatcherEditor
{
    public class ContentPackMenu : IClickableMenu
    {
        public static int scrolled;
        public ContentPatcherPack pack;
        public static int linesPerPage = 17;
        public static int windowWidth = 64 * 24;
        public Dictionary<Vector2, string> labels = new();
        public List<ChangeSet> changesList = new();
        public List<ClickableComponent> allComponents = new();
        public List<ClickableComponent> tabs = new();
        public List<int> tabTexts = new();
        
        public List<TextBox> fieldTextBoxes = new();
        public List<TextBox> updateKeysTextBoxes = new();
        public ClickableTextureComponent updateKeysAddCC;
        public List<ClickableTextureComponent> updateKeysSubCCs = new();
        
        public ClickableTextureComponent addCC;
        public ClickableTextureComponent revertCC;
        public ClickableTextureComponent reloadCC;
        public ClickableTextureComponent saveCC;
        public ClickableTextureComponent backCC;
        public ClickableTextureComponent zipCC;
        public ClickableTextureComponent upCC;
        public ClickableTextureComponent downCC;
        public ClickableTextureComponent folderCC;
        public ClickableTextureComponent scrollBar;
        public Rectangle scrollBarRunner;
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
        public List<ConfigSetup> configList;
        public bool scrolling;

        public ContentPackMenu(ContentPatcherPack _pack) : base(Game1.uiViewport.Width / 2 - (windowWidth + borderWidth * 2) / 2, -borderWidth, windowWidth + borderWidth * 2, Game1.uiViewport.Height, false)
        {
            scrolled = 0;
            pack = _pack;

            RepopulateComponentList();
            saveCC = null;
            revertCC = null;

            exitFunction = emergencyShutDown;

            snapToDefaultClickableComponent();
        }


        private void RepopulateComponentList()
        {
            int lineHeight = 64;

            width = Math.Min(64 * 24, Game1.viewport.Width - 96);
            height = Game1.viewport.Height + 72;
            xPositionOnScreen = (Game1.viewport.Width - width) / 2;
            yPositionOnScreen = - 72;
            tabHeight = 80;
            linesPerPage = (height - spaceToClearTopBorder * 2 - 116) / lineHeight;
            
            changingRect = new Rectangle(Game1.viewport.Width / 2 - width / 8, Game1.viewport.Height / 2 - height / 4, width / 4, height / 2);

            changesList.Clear();
            allComponents.Clear();
            fieldTextBoxes.Clear();
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
            var tabsWidth = width - (spaceToClearSideBorder + borderWidth) * 2;
            tabs.Add(new ClickableComponent(new Rectangle(xStart, yStart, tabsWidth / 3, tabHeight), "manifest", ModEntry.SHelper.Translation.Get("manifest"))
            {

            });
            tabTexts.Add((int)Game1.dialogueFont.MeasureString(ModEntry.SHelper.Translation.Get("manifest")).X);
            tabs.Add(new ClickableComponent(new Rectangle(xStart + tabsWidth / 3, yStart, tabsWidth / 3, tabHeight), "changes", ModEntry.SHelper.Translation.Get("changes"))
            {

            });
            tabTexts.Add((int)Game1.dialogueFont.MeasureString(ModEntry.SHelper.Translation.Get("changes")).X);
            tabs.Add(new ClickableComponent(new Rectangle(xStart + tabsWidth * 2 / 3, yStart, tabsWidth / 3, tabHeight), "config", ModEntry.SHelper.Translation.Get("config"))
            {

            });
            tabTexts.Add((int)Game1.dialogueFont.MeasureString(ModEntry.SHelper.Translation.Get("config")).X);
            yStart += tabHeight + 24;
            addCC = null;
            upCC = null;
            downCC = null;

            if (pack.content.lists is null)
            {
                ModEntry.RebuildLists(pack);
            }
            var lines = 0;
            if (tab == 0)
            {
                foreach (var fi in typeof(MyManifest).GetFields())
                {
                    if (fi.FieldType == typeof(string) || fi.FieldType == typeof(Version))
                    {
                        if (ModEntry.CanShowLine(lines))
                        {
                            var nameWidth = (int)Game1.dialogueFont.MeasureString(fi.Name).X;
                            fieldTextBoxes.Add(new TextBox(textBox, null, Game1.smallFont, Game1.textColor)
                            {
                                X = xStart + nameWidth,
                                Y = yStart + lineHeight * lines,
                                Width = width - nameWidth - (spaceToClearSideBorder + borderWidth) * 2,
                                Text = fi.GetValue(pack.manifest)?.ToString(),
                                limitWidth = false
                            });
                            allComponents.Add(new ClickableComponent(new Rectangle(xStart, yStart + lineHeight * lines, width, lineHeight), fi.Name));
                        }
                        lines++;
                    }
                }
                JArray list = pack.manifest.UpdateKeys ?? new JArray();
                labels.Clear();
                lines--;
                ModEntry.TryAddList("UpdateKeys", xStart, yStart, width - (spaceToClearSideBorder + borderWidth) * 2, lineHeight, ref lines, list, labels, updateKeysTextBoxes, ref updateKeysAddCC, updateKeysSubCCs);
            }
            else if (tab == 1)
            {
                int count = 0;
                int setHeight = 0;
                for (int i = 0; i < pack.content.Changes.Count; i++)
                {
                    var change = pack.content.Changes[i];
                    var lists = pack.content.lists[i];
                    ChangeSet set = null;

                    switch ((string)change["Action"])
                    {
                        case "Load":
                            set = new LoadSet(xStart, yStart + setHeight * count, width - (spaceToClearSideBorder + borderWidth) * 2, lineHeight, ref lines, change, lists);
                            break;
                        case "EditData":
                            set = new EditDataSet(xStart, yStart + setHeight * count, width - (spaceToClearSideBorder + borderWidth) * 2, lineHeight, ref lines, change, lists);
                            break;
                        case "EditImage":
                            set = new EditImageSet(xStart, yStart + setHeight * count, width - (spaceToClearSideBorder + borderWidth) * 2, lineHeight, ref lines, change, lists);
                            break;
                        case "EditMap":
                            set = new EditMapSet(xStart, yStart + setHeight * count, width - (spaceToClearSideBorder + borderWidth) * 2, lineHeight, ref lines, change, lists);
                            break;
                        case "Include":
                            set = new IncludeSet(xStart, yStart + setHeight * count, width - (spaceToClearSideBorder + borderWidth) * 2, lineHeight, ref lines, change, lists);
                            break;

                    }
                    if (set is not null)
                    {
                        allComponents.AddRange(set.Update);
                        if (set.ActionCC is not null)
                            allComponents.Add(set.ActionCC);
                        allComponents.AddRange(set.WhenSubCC);
                        changesList.Add(set);
                    }
                    count++;
                }
                addCC = new ClickableTextureComponent("Add", new Rectangle(xPositionOnScreen + width - 104, yPositionOnScreen - 96 + height, 56, 56), "", ModEntry.SHelper.Translation.Get("add"), Game1.mouseCursors, new Rectangle(1, 412, 14, 14), 4)
                {
                    myID = count * 1000,
                    upNeighborID = (count - 1) * 1000,
                    rightNeighborID = -2,
                };
            }
            else if (tab == 2)
            {

                int count = 0;
                var boxWidth = width - (spaceToClearSideBorder + borderWidth) * 2;
                configList = new List<ConfigSetup>();
                foreach (var kvp in pack.config)
                {
                    var cfgs = new ConfigSetup();
                    cfgs.oldKey = kvp.Key;
                    if (ModEntry.CanShowLine(lines))
                    {
                        cfgs.Key = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                        {
                            X = xStart + (int)Game1.dialogueFont.MeasureString("Key").X,
                            Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                            Width = boxWidth - (int)Game1.dialogueFont.MeasureString("Key").X,
                            Text = kvp.Key,
                            limitWidth = false
                        };
                        cfgs.DeleteCC = new ClickableTextureComponent("Remove", new Rectangle(xStart - 16, yStart + (lines - ContentPackMenu.scrolled) * lineHeight + 16, 56, 56), "", ModEntry.SHelper.Translation.Get("remove"), Game1.mouseCursors, new Rectangle(269, 471, 14, 15), 1);
                        cfgs.labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "Key");
                    }
                    if (ModEntry.CanShowLine(++lines))
                    {
                        cfgs.Default = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                        {
                            X = xStart + (int)Game1.dialogueFont.MeasureString("Default").X,
                            Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                            Width = boxWidth - (int)Game1.dialogueFont.MeasureString("Default").X,
                            Text = kvp.Value.Default,
                            limitWidth = false
                        };
                        cfgs.labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "Default");
                    }
                    ModEntry.TryAddList("AllowValues", xStart, yStart, boxWidth, lineHeight, ref lines, ModEntry.MakeJArray(kvp.Value.AllowValues), cfgs.labels, cfgs.AllowValues, ref cfgs.AllowValuesAddCC, cfgs.AllowValuesSubCCs);
                    if (ModEntry.CanShowLine(lines))
                    {
                        int sep = 16;
                        cfgs.AllowBlank = new ClickableTextureComponent("AllowBlank", new Rectangle(xStart + (int)Game1.dialogueFont.MeasureString("AllowBlank").X + sep, yStart + (lines - ContentPackMenu.scrolled) * lineHeight, 36, 36), "", ModEntry.SHelper.Translation.Get("allow-blank"), Game1.mouseCursors, new Rectangle(227 + (kvp.Value.AllowBlank ? 9 : 0), 425, 9, 9), 4);
                        cfgs.labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "AllowBlank");
                        allComponents.Add(cfgs.AllowBlank);
                        cfgs.AllowMultiple = new ClickableTextureComponent("AllowMultiple", new Rectangle(cfgs.AllowBlank.bounds.Right + (int)Game1.dialogueFont.MeasureString("AllowMultiple").X + sep * 4, yStart + (lines - ContentPackMenu.scrolled) * lineHeight, 36, 36), "", ModEntry.SHelper.Translation.Get("allow-multiple"), Game1.mouseCursors, new Rectangle(227 + (kvp.Value.AllowMultiple ? 9 : 0), 425, 9, 9), 4);
                        cfgs.labels.Add(new Vector2(cfgs.AllowBlank.bounds.Right + sep * 3, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "AllowMultiple");
                        allComponents.Add(cfgs.AllowMultiple);
                    }
                    lines++;
                    configList.Add(cfgs);
                    count++;
                }
                addCC = new ClickableTextureComponent("Add", new Rectangle(xPositionOnScreen + width - 104, yPositionOnScreen - 96 + height, 56, 56), "", ModEntry.SHelper.Translation.Get("add"), Game1.mouseCursors, new Rectangle(1, 412, 14, 14), 4)
                {
                    myID = count * 1000,
                    upNeighborID = (count - 1) * 1000,
                    rightNeighborID = -2,
                };
            }
            totalLines = lines;
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
            if (upCC != null || downCC != null) 
            {
                scrollBar = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 40 + 8, yPositionOnScreen + 132, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
                scrollBarRunner = new Rectangle(scrollBar.bounds.X, scrollBar.bounds.Y, scrollBar.bounds.Width, height - 200);
                float interval = (scrollBarRunner.Height - scrollBar.bounds.Height) / (float)(totalLines - linesPerPage);
                scrollBar.bounds.Y = Math.Min(scrollBarRunner.Y + (int)Math.Round(interval * scrolled), scrollBarRunner.Bottom - scrollBar.bounds.Height);
            }
            else
            {
                scrollBar = null;
            }
            if (ModEntry.SHelper.ModRegistry.IsLoaded(pack.manifest.UniqueID))
            {
                reloadCC = new ClickableTextureComponent("Reload", new Rectangle(xPositionOnScreen + spaceToClearSideBorder + borderWidth / 2 + 80, yPositionOnScreen - 100 + height, 64, 64), "", ModEntry.SHelper.Translation.Get("reload"), Game1.mouseCursors, new Rectangle(274, 284, 16, 16), 4);
            }
            zipCC = new ClickableTextureComponent("Zip", new Rectangle(xPositionOnScreen + spaceToClearSideBorder + borderWidth / 2, yPositionOnScreen - 100 + height, 64, 64), "", ModEntry.SHelper.Translation.Get("zip"), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), 4);
            folderCC = new ClickableTextureComponent("Folder", new Rectangle(xPositionOnScreen + spaceToClearSideBorder + borderWidth / 2 + 160, yPositionOnScreen - 100 + height, 64, 64), "", ModEntry.SHelper.Translation.Get("folder"), Game1.mouseCursors, new Rectangle(366, 373, 16, 16), 4);
            populateClickableComponentList();
        }

        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true);
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
            reloadCC?.draw(b);
            zipCC?.draw(b);
            folderCC.draw(b);
            if (tab == 0)
            {
                for (int i = 0; i <  fieldTextBoxes.Count; i++)
                {
                    fieldTextBoxes[i].Draw(b);
                    b.DrawString(Game1.dialogueFont, allComponents[i].name, allComponents[i].bounds.Location.ToVector2(), Color.Brown);
                }
                foreach(var l in labels)
                {
                    b.DrawString(Game1.dialogueFont, l.Value, l.Key, Color.Brown);
                }
                updateKeysAddCC?.draw(b);
                foreach(var tb in updateKeysTextBoxes)
                {
                    tb.Draw(b);
                }
                foreach(var cc in updateKeysSubCCs)
                {
                    cc.draw(b);
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
                    else if (set is EditImageSet)
                    {
                        (set as EditImageSet).Target?.Draw(b);
                        (set as EditImageSet).FromFile?.Draw(b);
                        if((set as EditImageSet).PatchMode is not null)
                        {
                            var cc = (set as EditImageSet).PatchMode;
                            var p = new Vector2(cc.bounds.X + 16, cc.bounds.Y);
                            b.DrawString(Game1.dialogueFont, cc.name, p + new Vector2(-1, 1), Color.Black * 0.5f);
                            b.DrawString(Game1.dialogueFont, cc.name, p, Color.Brown);
                        }

                        if((set as EditImageSet).FromArea is not null)
                        {
                            foreach (var tb in (set as EditImageSet).FromArea)
                            {
                                tb.Draw(b);
                            }

                        }
                        if((set as EditImageSet).ToArea is not null)
                        {
                            foreach (var tb in (set as EditImageSet).ToArea)
                            {
                                tb.Draw(b);
                            }

                        }
                    }
                    else if (set is EditMapSet)
                    {
                        (set as EditMapSet).Target?.Draw(b);
                        (set as EditMapSet).FromFile?.Draw(b);
                        if((set as EditMapSet).PatchMode is not null)
                        {
                            var cc = (set as EditMapSet).PatchMode;
                            var p = new Vector2(cc.bounds.X + 16, cc.bounds.Y);
                            b.DrawString(Game1.dialogueFont, cc.name, p + new Vector2(-1, 1), Color.Black * 0.5f);
                            b.DrawString(Game1.dialogueFont, cc.name, p, Color.Brown);
                        }

                        if((set as EditMapSet).FromArea is not null)
                        {
                            foreach (var tb in (set as EditMapSet).FromArea)
                            {
                                tb.Draw(b);
                            }

                        }
                        if((set as EditMapSet).ToArea is not null)
                        {
                            foreach (var tb in (set as EditMapSet).ToArea)
                            {
                                tb.Draw(b);
                            }

                        }
                        (set as EditMapSet).AddWarpsAddCC?.draw(b);
                        foreach(var cc in (set as EditMapSet).AddWarpsSubCC)
                        {
                            cc.draw(b);
                        }
                        foreach(var tb in (set as EditMapSet).AddWarps)
                        {
                            tb.Draw(b);
                        }
                        (set as EditMapSet).MapPropertiesAddCC?.draw(b);
                        foreach(var cc in (set as EditMapSet).MapPropertiesSubCC)
                        {
                            cc.draw(b);
                        }
                        foreach(var e in (set as EditMapSet).MapProperties)
                        {
                            foreach (var tb in e)
                            {
                                tb.Draw(b);
                            }
                        }
                    }
                    else if(set is IncludeSet)
                    {
                        (set as IncludeSet).FromFile?.Draw(b);
                    }
                    upCC?.draw(b);
                    downCC?.draw(b);
                }
                addCC.draw(b);
            }
            else if(tab == 2)
            {

                for (int i = 0; i < configList.Count; i++)
                {
                    var cfgs = configList[i];
                    if (i > 0 && cfgs.Key is not null)
                    {
                        b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + spaceToClearSideBorder + 16, cfgs.Key.Y - 14, width - 64, 8), new Rectangle(40, 16, 1, 16), Color.White);
                    }
                    cfgs.Key?.Draw(b);
                    cfgs.Default?.Draw(b);
                    cfgs.DeleteCC?.draw(b);
                    cfgs.AllowValuesAddCC?.draw(b);
                    cfgs.AllowBlank?.draw(b);
                    cfgs.AllowMultiple?.draw(b);

                    foreach (var cc in cfgs.AllowValuesSubCCs)
                    {
                        cc.draw(b);
                    }
                    foreach (var tb in cfgs.AllowValues)
                    {
                        tb.Draw(b);
                    }
                    foreach (var l in cfgs.labels)
                    {
                        b.DrawString(Game1.dialogueFont, l.Value, l.Key, Color.Brown);
                    }
                    upCC?.draw(b);
                    downCC?.draw(b);
                }
                addCC.draw(b);
            }
            if(scrollBar is not null)
            {
                drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f, true, -1f);
                scrollBar.draw(b);
            }
            b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + spaceToClearSideBorder + 16, yPositionOnScreen + height - 112, width - 64, 8), new Rectangle(40, 16, 1, 16), Color.White);
            saveCC?.draw(b);
            revertCC?.draw(b);
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
                revertCC = null;
                RepopulateComponentList();
                return;
            }
            if (zipCC?.containsPoint(x, y) == true)
            {
                ModEntry.ZipContentPack(pack);
                Game1.playSound("Ship");
                RepopulateComponentList();
                return;
            }
            if (reloadCC?.containsPoint(x, y) == true)
            {
                ModEntry.AddToRawCommandQueue.Value($"patch reload {pack.manifest.UniqueID}");
                var newPack = new ContentPatcherPack()
                {
                    directory = pack.directory,
                    manifest = JsonConvert.DeserializeObject<MyManifest>(File.ReadAllText(Path.Combine(pack.directory, "manifest.json"))),
                    content = JsonConvert.DeserializeObject<ContentPatcherContent>(File.ReadAllText(Path.Combine(pack.directory, "content.json"))),
                };
                ModEntry.RebuildLists(newPack);
                pack = newPack;
                Game1.playSound("bigSelect");
                RepopulateComponentList();
                return;
            }
            if (revertCC?.containsPoint(x, y) == true)
            {
                Game1.playSound("trashcan");
                var newPack = new ContentPatcherPack()
                {
                    directory = pack.directory,
                    manifest = JsonConvert.DeserializeObject<MyManifest>(File.ReadAllText(Path.Combine(pack.directory, "manifest.json"))),
                    content = JsonConvert.DeserializeObject<ContentPatcherContent>(File.ReadAllText(Path.Combine(pack.directory, "content.json"))),
                };
                ModEntry.RebuildLists(newPack);
                pack = newPack;

                saveCC = null;
                revertCC = null;
                RepopulateComponentList();
                return;
            }
            if (folderCC.containsPoint(x, y))
            {
                ModEntry.TryOpenFolder(pack.directory);
                Game1.playSound("bigSelect");
                return;
            }
            if (backCC.containsPoint(x, y))
            {
                Game1.playSound("bigDeSelect");
                if (Game1.activeClickableMenu is TitleMenu)
                {
                    TitleMenu.subMenu = new ContentPatcherMenu();
                }
                else
                {
                    Game1.activeClickableMenu = new ContentPatcherMenu();
                }
                return;
            }
            if(scrollBar?.containsPoint(x, y) == true)
            {
                scrolling = true;
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
                foreach (var tb in fieldTextBoxes)
                {
                    tb.Selected = false;
                    tb.Update();
                }
                if(updateKeysAddCC?.containsPoint(x, y) == true)
                {
                    Game1.playSound("bigSelect");
                    if (pack.manifest.UpdateKeys is null)
                    {
                        pack.manifest.UpdateKeys = new JArray();
                    }
                    pack.manifest.UpdateKeys.Add("");
                    ShowSaveButton();
                    RepopulateComponentList();
                    return;
                }
                foreach (var tb in updateKeysTextBoxes)
                {
                    tb.Selected = false;
                    tb.Update();
                }
                if(updateKeysSubCCs?.Count > 0)
                {
                    for (int j = 0; j < updateKeysSubCCs.Count; j++)
                    {
                        if (updateKeysSubCCs[j].containsPoint(x, y))
                        {
                            pack.manifest.UpdateKeys.RemoveAt(j);
                            Game1.playSound("trashcan");
                            ShowSaveButton();
                            RepopulateComponentList();
                            return;
                        }
                    }
                }
            }
            else if (tab == 1)
            {
                for (int i = 0; i < changesList.Count; i++)
                {
                    var set = changesList[i];
                    if (set.DeleteCC?.containsPoint(x, y) == true)
                    {
                        Game1.playSound("trashcan");
                        pack.content.Changes.RemoveAt(i);
                        ShowSaveButton();
                        ModEntry.RebuildLists(pack);
                        RepopulateComponentList();
                        return;
                    }
                    if (set.WhenAddCC?.containsPoint(x, y) == true)
                    {
                        Game1.playSound("bigSelect");
                        if (!pack.content.lists[i].TryGetValue("When", out var when))
                        {
                            when = new List<KeyValuePair<string, JToken?>>();
                        }
                        when.Add(new KeyValuePair<string, JToken?>("",""));
                        pack.content.lists[i]["When"] = when;
                        ShowSaveButton();
                        RepopulateComponentList();
                        return;
                    }
                    if (set.WhenSubCC.Count > 0)
                    {
                        for (int j = 0; j < set.WhenSubCC.Count; j++)
                        {
                            if (set.WhenSubCC[j].containsPoint(x, y))
                            {
                                pack.content.lists[i]["When"].RemoveAt(j);
                                Game1.playSound("trashcan");
                                ShowSaveButton();
                                RepopulateComponentList();
                                return;
                            }
                        }
                    }
                    if (set.Action is not null)
                    {
                        set.Action.Selected = false;
                        if (set.ActionCC.containsPoint(x, y))
                        {
                            changeChanging = i;
                            Game1.playSound("bigSelect");
                            return;
                        }
                    }
                    if (set.LogName is not null)
                    {
                        set.LogName.Selected = false;
                        set.LogName.Update();
                    }
                    foreach (var w in set.When)
                    {
                        foreach (var tb in w)
                        {
                            tb.Selected = false;
                            tb.Update();
                        }
                    }
                    foreach (var u in set.Update)
                    {
                        if (u.containsPoint(x, y))
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
                                foreach (var s in list)
                                {
                                    if (s.Trim() != u.name)
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
                    if (set is LoadSet)
                    {
                        if ((set as LoadSet).Target is not null)
                        {
                            (set as LoadSet).Target.Selected = false;
                            (set as LoadSet).Target.Update();
                        }
                        if ((set as LoadSet).FromFile is not null)
                        {
                            (set as LoadSet).FromFile.Selected = false;
                            (set as LoadSet).FromFile.Update();
                        }
                    }
                    else if (set is EditDataSet)
                    {
                        if ((set as EditDataSet).EntriesAddCC?.containsPoint(x, y) == true)
                        {
                            Game1.playSound("bigSelect");
                            if (!pack.content.lists[i].TryGetValue("Entries", out var entries))
                            {
                                entries = new List<KeyValuePair<string, JToken?>>();
                            }
                            entries.Add(new KeyValuePair<string, JToken?>("",""));
                            pack.content.lists[i]["Entries"] = entries;
                            ShowSaveButton();
                            RepopulateComponentList();
                            return;
                        }
                        if ((set as EditDataSet).Target is not null)
                        {
                            (set as EditDataSet).Target.Selected = false;
                            (set as EditDataSet).Target.Update();
                        }
                        for (int j = 0; j < (set as EditDataSet).Entries.Count; j++)
                        {
                            (set as EditDataSet).Entries[j][0].Selected = false;
                            (set as EditDataSet).Entries[j][0].Update();
                            (set as EditDataSet).Entries[j][1].Selected = false;
                            if (pack.content.lists[i]["Entries"][j].Value.Type == JTokenType.String)
                            {
                                (set as EditDataSet).Entries[j][1].Update();
                            }
                        }
                        if ((set as EditDataSet).EntriesSubCC.Count > 0)
                        {
                            for (int j = 0; j < (set as EditDataSet).EntriesSubCC.Count; j++)
                            {
                                if ((set as EditDataSet).EntriesSubCC[j].containsPoint(x, y))
                                {
                                    pack.content.lists[i]["Entries"].RemoveAt(j);
                                    Game1.playSound("trashcan");
                                    ShowSaveButton();
                                    RepopulateComponentList();
                                    return;
                                }
                            }
                        }
                    }
                    else if (set is EditImageSet)
                    {
                        if ((set as EditImageSet).Target is not null)
                        {
                            (set as EditImageSet).Target.Selected = false;
                            (set as EditImageSet).Target.Update();
                        }
                        if ((set as EditImageSet).FromFile is not null)
                        {
                            (set as EditImageSet).FromFile.Selected = false;
                            (set as EditImageSet).FromFile.Update();
                        }
                        if ((set as EditImageSet).FromArea is not null)
                        {
                            foreach (var tb in (set as EditImageSet).FromArea)
                            {
                                tb.Selected = false;
                                tb.Update();
                            }
                        }
                        if ((set as EditImageSet).ToArea is not null)
                        {
                            foreach (var tb in (set as EditImageSet).ToArea)
                            {
                                tb.Selected = false;
                                tb.Update();
                            }
                        }
                        if ((set as EditImageSet).PatchMode?.containsPoint(x, y) == true)
                        {
                            Game1.playSound("bigSelect");
                            if (!pack.content.Changes[i].TryGetValue("PatchMode", out var obj))
                            {
                                obj = "ReplaceByLayer";
                            }
                            var str = (string)obj;
                            switch (str)
                            {
                                case "ReplaceByLayer":
                                    str = "Replace";
                                    break;
                                case "Replace":
                                    str = "Overlay";
                                    break;
                                case "Overlay":
                                    str = "ReplaceByLayer";
                                    break;
                            }
                            pack.content.Changes[i]["PatchMode"] = str;
                            (set as EditImageSet).PatchMode.name = str;
                            ShowSaveButton();
                            RepopulateComponentList();
                            return;
                        }
                    }
                    else if (set is EditMapSet)
                    {
                        if ((set as EditMapSet).Target is not null)
                        {
                            (set as EditMapSet).Target.Selected = false;
                            (set as EditMapSet).Target.Update();
                        }
                        if ((set as EditMapSet).FromFile is not null)
                        {
                            (set as EditMapSet).FromFile.Selected = false;
                            (set as EditMapSet).FromFile.Update();
                        }
                        if ((set as EditMapSet).FromArea is not null)
                        {
                            foreach (var tb in (set as EditMapSet).FromArea)
                            {
                                tb.Selected = false;
                                tb.Update();
                            }
                        }
                        if ((set as EditMapSet).ToArea is not null)
                        {
                            foreach (var tb in (set as EditMapSet).ToArea)
                            {
                                tb.Selected = false;
                                tb.Update();
                            }
                        }
                        if ((set as EditMapSet).AddWarpsAddCC?.containsPoint(x, y) == true)
                        {
                            Game1.playSound("bigSelect");
                            if (!pack.content.Changes[i].TryGetValue("AddWarps", out var obj))
                            {
                                obj = new JArray();
                            }
                            var entries = (JArray)obj;
                            entries.Add("");
                            pack.content.Changes[i]["AddWarps"] = entries;
                            ShowSaveButton();
                            RepopulateComponentList();
                            return;
                        }
                        if ((set as EditMapSet).PatchMode?.containsPoint(x, y) == true)
                        {
                            Game1.playSound("bigSelect");
                            if (!pack.content.Changes[i].TryGetValue("PatchMode", out var obj))
                            {
                                obj = "ReplaceByLayer";
                            }
                            var str = (string)obj;
                            switch (str)
                            {
                                case "ReplaceByLayer":
                                    str = "Replace";
                                    break;
                                case "Replace":
                                    str = "Overlay";
                                    break;
                                case "Overlay":
                                    str = "ReplaceByLayer";
                                    break;
                            }
                            pack.content.Changes[i]["PatchMode"] = str;
                            (set as EditMapSet).PatchMode.name = str;
                            ShowSaveButton();
                            RepopulateComponentList();
                            return;
                        }
                        if ((set as EditMapSet).AddWarpsSubCC.Count > 0)
                        {
                            for (int j = 0; j < (set as EditMapSet).AddWarpsSubCC.Count; j++)
                            {
                                if ((set as EditMapSet).AddWarpsSubCC[j].containsPoint(x, y))
                                {
                                    ((JArray)pack.content.Changes[i]["AddWarps"]).RemoveAt(j);
                                    Game1.playSound("trashcan");
                                    ShowSaveButton();
                                    RepopulateComponentList();
                                    return;
                                }
                            }
                        }
                        foreach (var tb in (set as EditMapSet).AddWarps)
                        {
                            tb.Selected = false;
                            tb.Update();
                        }
                        if ((set as EditMapSet).MapPropertiesAddCC?.containsPoint(x, y) == true)
                        {
                            Game1.playSound("bigSelect");
                            if (!pack.content.lists[i].TryGetValue("MapProperties", out var entries))
                            {
                                entries = new List<KeyValuePair<string, JToken?>>();
                            }
                            entries.Add(new KeyValuePair<string, JToken?>("", ""));
                            pack.content.lists[i]["MapProperties"] = entries;
                            ShowSaveButton();
                            RepopulateComponentList();
                            return;
                        }
                        if ((set as EditMapSet).MapPropertiesSubCC.Count > 0)
                        {
                            for (int j = 0; j < (set as EditMapSet).MapPropertiesSubCC.Count; j++)
                            {
                                if ((set as EditMapSet).MapPropertiesSubCC[j].containsPoint(x, y))
                                {
                                    pack.content.lists[i]["MapProperties"].RemoveAt(j);
                                    Game1.playSound("trashcan");
                                    ShowSaveButton();
                                    RepopulateComponentList();
                                    return;
                                }
                            }
                        }
                        foreach (var e in (set as EditMapSet).MapProperties)
                        {
                            foreach (var tb in e)
                            {
                                tb.Selected = false;
                                tb.Update();
                            }
                        }
                    }
                    else if (set is IncludeSet)
                    {
                        if ((set as IncludeSet).FromFile is not null)
                        {
                            (set as IncludeSet).FromFile.Selected = false;
                            (set as IncludeSet).FromFile.Update();
                        }
                    }
                }

                if (addCC?.containsPoint(x, y) == true)
                {
                    pack.content.Changes.Add(ModEntry.CreateNewChange("Load"));
                    ModEntry.RebuildLists(pack);
                    ShowSaveButton();
                    Game1.playSound("bigSelect");
                    RepopulateComponentList();
                    return;
                }
            }
            else if (tab == 2)
            {
                if (configList.Any())
                {
                    for (int i = 0; i < configList.Count; i++)
                    {
                        var cfg = configList[i];
                        cfg.Default.Selected = false;
                        cfg.Default.Update();
                        cfg.Key.Selected = false;
                        cfg.Key.Update();
                        foreach (var tb in cfg.AllowValues)
                        {
                            tb.Selected = false;
                            tb.Update();
                        }
                        if (cfg.DeleteCC?.containsPoint(x, y) == true)
                        {
                            Game1.playSound("trashcan");
                            pack.config.RemoveAt(i);
                            ShowSaveButton();
                            RepopulateComponentList();
                            return;
                        }
                        if (cfg.AllowBlank?.containsPoint(x, y) == true)
                        {
                            Game1.playSound("bigSelect");
                            pack.config[i].Value.AllowBlank = !pack.config[i].Value.AllowBlank;
                            ShowSaveButton();
                            RepopulateComponentList();
                            return;
                        }
                        if (cfg.AllowMultiple?.containsPoint(x, y) == true)
                        {
                            Game1.playSound("bigSelect");
                            pack.config[i].Value.AllowMultiple = !pack.config[i].Value.AllowMultiple;
                            ShowSaveButton();
                            RepopulateComponentList();
                            return;
                        }
                        if (cfg.AllowValuesAddCC?.containsPoint(x, y) == true)
                        {
                            Game1.playSound("bigSelect");
                            pack.config[i].Value.AllowValues = pack.config[i].Value.AllowValues is null ? "" : pack.config[i].Value.AllowValues + ", ";
                            ShowSaveButton();
                            RepopulateComponentList();
                            return;
                        }
                        if (cfg.AllowValues?.Any() == true)
                        {
                            List<string> values = cfg.AllowValues.Select(tb => tb.Text).ToList();
                            for (int j = 0; j < cfg.AllowValuesSubCCs.Count; j++)
                            {
                                if (cfg.AllowValuesSubCCs[j].containsPoint(x, y) == true)
                                {
                                    values.RemoveAt(j);
                                    Game1.playSound("trashcan");
                                    pack.config[i].Value.AllowValues = values.Any() ? string.Join(", ", values) : null;
                                    ShowSaveButton();
                                    RepopulateComponentList();
                                    return;
                                }
                            }
                        }
                    }
                }


                if (addCC?.containsPoint(x, y) == true)
                {
                    if(pack.config is null)
                    {
                        pack.config = new();
                    }
                    pack.config.Add(new KeyValuePair<string, ConfigVar>("Key", new ConfigVar()));
                    ShowSaveButton();
                    Game1.playSound("bigSelect");
                    RepopulateComponentList();
                    return;
                }
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

            if (tab == 0)
            {
                for (int i = 0; i < fieldTextBoxes.Count; i++)
                {
                    var tb = fieldTextBoxes[i];
                    if (!tb.Selected)
                        continue;
                    var fi = typeof(MyManifest).GetField(allComponents[i].name);
                    if (fi.GetValue(pack.manifest)?.ToString() != tb.Text)
                    {
                        ModEntry.PlaySound("typing");
                        if (fi.FieldType == typeof(string))
                            fi.SetValue(pack.manifest, tb.Text);
                        else
                        {
                            try
                            {
                                fi.SetValue(pack.manifest, new Version(tb.Text));
                            }
                            catch { }
                        }
                        ShowSaveButton();
                        return;
                    }
                }
                if (updateKeysTextBoxes?.Count > 0)
                {
                    for (int j = 0; j < updateKeysTextBoxes.Count; j++)
                    {
                        if (updateKeysTextBoxes[j].Selected && updateKeysTextBoxes[j].Text != (string)pack.manifest.UpdateKeys[j])
                        {
                            pack.manifest.UpdateKeys[j] = updateKeysTextBoxes[j].Text;
                            ModEntry.PlaySound("typing");
                            ShowSaveButton();
                            return;
                        }
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
                        ModEntry.PlaySound("typing");
                        pack.content.Changes[i]["Action"] = set.Action.Text;
                        ShowSaveButton();
                        return;
                    }
                    if (set.LogName?.Selected == true && (string)pack.content.Changes[i]["LogName"] != set.Action.Text)
                    {
                        ModEntry.PlaySound("typing");
                        pack.content.Changes[i]["LogName"] = set.LogName.Text;
                        ShowSaveButton();
                        return;
                    }
                    for (int j = 0; j < set.When.Count; j++)
                    {
                        if (set.When[j][0].Selected && set.When[j][0].Text != (string)pack.content.lists[i]["When"][j].Key ||
                            set.When[j][1].Selected && set.When[j][1].Text != (string)pack.content.lists[i]["When"][j].Value)
                        {
                            pack.content.lists[i]["When"][j] = new KeyValuePair<string, JToken?>(set.When[j][0].Text, set.When[j][1].Text);
                            ModEntry.PlaySound("typing");
                            ShowSaveButton();
                            return;
                        }
                    }
                    if (set is LoadSet)
                    {
                        if ((set as LoadSet).Target?.Selected == true && (string)pack.content.Changes[i]["Target"] != (set as LoadSet).Target.Text)
                        {
                            ModEntry.PlaySound("typing");
                            pack.content.Changes[i]["Target"] = (set as LoadSet).Target.Text;
                            ShowSaveButton();
                            return;
                        }
                        if ((set as LoadSet).FromFile?.Selected == true && (string)pack.content.Changes[i]["FromFile"] != (set as LoadSet).FromFile.Text)
                        {
                            ModEntry.PlaySound("typing");
                            pack.content.Changes[i]["FromFile"] = (set as LoadSet).FromFile.Text;
                            ShowSaveButton();
                            return;
                        }
                    }
                    else if (set is EditDataSet)
                    {
                        if ((set as EditDataSet).Target?.Selected == true && (string)pack.content.Changes[i]["Target"] != (set as EditDataSet).Target.Text)
                        {
                            ModEntry.PlaySound("typing");
                            pack.content.Changes[i]["Target"] = (set as EditDataSet).Target.Text;
                            ShowSaveButton();
                            return;
                        }
                        for (int j = 0; j < (set as EditDataSet).Entries.Count; j++)
                        {
                            var entry = (set as EditDataSet).Entries[j];
                            if (entry[0].Selected && entry[0].Text != (string)pack.content.lists[i]["Entries"][j].Key)
                            {
                                pack.content.lists[i]["Entries"][j] = new KeyValuePair<string, JToken?>(entry[0].Text, pack.content.lists[i]["Entries"][j].Value);
                                ModEntry.PlaySound("typing");
                                ShowSaveButton();
                                return;
                            }
                            if (pack.content.lists[i]["Entries"][j].Value.Type == JTokenType.String && entry[1].Selected && entry[1].Text != (string)pack.content.lists[i]["Entries"][j].Value)
                            {
                                pack.content.lists[i]["Entries"][j] = new KeyValuePair<string, JToken?>(pack.content.lists[i]["Entries"][j].Key, entry[1].Text);
                                ModEntry.PlaySound("typing");
                                ShowSaveButton();
                                return;
                            }
                        }
                    }
                    else if (set is EditImageSet)
                    {
                        if ((set as EditImageSet).Target?.Selected == true && (string)pack.content.Changes[i]["Target"] != (set as EditImageSet).Target.Text)
                        {
                            ModEntry.PlaySound("typing");
                            pack.content.Changes[i]["Target"] = (set as EditImageSet).Target.Text;
                            ShowSaveButton();
                            return;
                        }
                        if ((set as EditImageSet).FromFile?.Selected == true && (string)pack.content.Changes[i]["FromFile"] != (set as EditImageSet).FromFile.Text)
                        {
                            ModEntry.PlaySound("typing");
                            pack.content.Changes[i]["FromFile"] = (set as EditImageSet).FromFile.Text;
                            ShowSaveButton();
                            return;
                        }
                        if ((set as EditImageSet).FromArea is not null && CheckRectChanged((set as EditImageSet).FromArea, "FromArea", i))
                        {
                            ModEntry.PlaySound("typing");
                            ShowSaveButton();
                            return;
                        }
                        if ((set as EditImageSet).ToArea is not null && CheckRectChanged((set as EditImageSet).ToArea, "ToArea", i))
                        {
                            ModEntry.PlaySound("typing");
                            ShowSaveButton();
                            return;
                        }
                    }
                    else if (set is EditMapSet)
                    {
                        if ((set as EditMapSet).Target?.Selected == true && (string)pack.content.Changes[i]["Target"] != (set as EditMapSet).Target.Text)
                        {
                            ModEntry.PlaySound("typing");
                            pack.content.Changes[i]["Target"] = (set as EditMapSet).Target.Text;
                            ShowSaveButton();
                            return;
                        }
                        if ((set as EditMapSet).FromFile?.Selected == true && (string)pack.content.Changes[i]["FromFile"] != (set as EditMapSet).FromFile.Text)
                        {
                            ModEntry.PlaySound("typing");
                            pack.content.Changes[i]["FromFile"] = (set as EditMapSet).FromFile.Text;
                            ShowSaveButton();
                            return;
                        }
                        for (int j = 0; j < (set as EditMapSet).MapProperties.Count; j++)
                        {
                            var entry = (set as EditMapSet).MapProperties[j];
                            if (entry[0].Selected && entry[0].Text != (string)pack.content.lists[i]["MapProperties"][j].Key ||
                                entry[1].Selected && entry[1].Text != (string)pack.content.lists[i]["MapProperties"][j].Value)
                            {
                                pack.content.lists[i]["MapProperties"][j] = new KeyValuePair<string, JToken?>(entry[0].Text, entry[1].Text);
                                ModEntry.PlaySound("typing");
                                ShowSaveButton();
                                return;
                            }
                        }
                        for (int j = 0; j < (set as EditMapSet).AddWarps.Count; j++)
                        {
                            if ((set as EditMapSet).AddWarps[j].Selected && (set as EditMapSet).AddWarps[j].Text != pack.content.Changes[i]["AddWarps"][j].ToString())
                            {
                                pack.content.Changes[i]["AddWarps"][j] = (set as EditMapSet).AddWarps[j].Text;
                                ModEntry.PlaySound("typing");
                                ShowSaveButton();
                                return;
                            }
                        }
                        if ((set as EditMapSet).FromArea is not null && CheckRectChanged((set as EditMapSet).FromArea, "FromArea", i))
                        {
                            ModEntry.PlaySound("typing");
                            ShowSaveButton();
                            return;
                        }
                        if ((set as EditMapSet).ToArea is not null && CheckRectChanged((set as EditMapSet).ToArea, "ToArea", i))
                        {
                            ModEntry.PlaySound("typing");
                            ShowSaveButton();
                            return;
                        }
                    }
                    else if (set is IncludeSet)
                    {
                        if ((set as IncludeSet).FromFile?.Selected == true && (string)pack.content.Changes[i]["FromFile"] != (set as IncludeSet).FromFile.Text)
                        {
                            ModEntry.PlaySound("typing");
                            pack.content.Changes[i]["FromFile"] = (set as IncludeSet).FromFile.Text;
                            ShowSaveButton();
                            return;
                        }
                    }
                }
            }
            else if (tab == 2)
            {
                for (int i = 0; i < configList.Count; i++)
                {
                    var cfg = configList[i];
                    if (cfg.Key?.Selected == true && cfg.oldKey != cfg.Key.Text)
                    {
                        ModEntry.PlaySound("typing");
                        pack.config[i] =  new KeyValuePair<string, ConfigVar>(cfg.Key.Text, pack.config[i].Value);
                        ShowSaveButton();
                        return;
                    }
                    if (cfg.Default?.Selected == true && pack.config[i].Value.Default != cfg.Default.Text)
                    {
                        ModEntry.PlaySound("typing");
                        pack.config[i].Value.Default = cfg.Default.Text;
                        ShowSaveButton();
                        return;
                    }
                    if(cfg.AllowValues?.Any() == true)
                    {
                        for(int j = 0; j < cfg.AllowValues.Count; j++)
                        {
                            string[] values = pack.config[i].Value.AllowValues.Split(',');
                            if (cfg.AllowValues[j].Selected && values[j].Trim() != cfg.AllowValues[j].Text.Trim())
                            {
                                ModEntry.PlaySound("typing");
                                values[j] = cfg.AllowValues[j].Text.Trim();
                                pack.config[i].Value.AllowValues = string.Join(", ", values);
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

        private bool CheckRectChanged(TextBox[] list, string key, int idx)
        {

            if (list[0].Selected ||
                list[1].Selected ||
                list[2].Selected ||
                list[3].Selected
                )
            {

                if (int.TryParse(list[0].Text, out int X) &&
                    int.TryParse(list[1].Text, out int Y) &&
                    int.TryParse(list[2].Text, out int W) &&
                    int.TryParse(list[3].Text, out int H)
                    )
                {
                    if (!pack.content.Changes[idx].TryGetValue(key, out var fa) ||
                        (int)((JObject)fa)["X"] != X ||
                        (int)((JObject)fa)["Y"] != Y ||
                        (int)((JObject)fa)["Width"] != W ||
                        (int)((JObject)fa)["Height"] != H
                        )
                    {
                        if (X == 0 && Y == 0 && W == 0 && H == 0)
                        {
                            if (!pack.content.Changes[idx].ContainsKey(key))
                                return false;
                            pack.content.Changes[idx].Remove(key);
                        }
                        else
                        {
                            pack.content.Changes[idx][key] = new JObject()
                            {
                                { "X", X },
                                { "Y", Y },
                                { "Width", W },
                                { "Height", H }
                            };
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private void ShowSaveButton()
        {
            saveCC = new ClickableTextureComponent("Save", new Rectangle(xPositionOnScreen + width / 2 - 70, yPositionOnScreen - 100 + height, 64, 64), "", ModEntry.SHelper.Translation.Get("save"), Game1.mouseCursors, new Rectangle(241, 320, 16, 16), 4)
            {
                myID = -4,
                upNeighborID = 0,
                rightNeighborID = -2,
                leftNeighborID = -3
            };
            revertCC = new ClickableTextureComponent("Revert", new Rectangle(xPositionOnScreen + width / 2 + 10, yPositionOnScreen - 102 + height, 68, 68), "", ModEntry.SHelper.Translation.Get("revert"), Game1.mouseCursors, new Rectangle(348, 372, 17, 17), 4)
            {
                myID = -5,
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

            if (addCC?.containsPoint(x, y) == true)
            {
                hoverText = addCC.hoverText;
                return;
            }
            if (reloadCC?.containsPoint(x, y) == true)
            {
                hoverText = reloadCC.hoverText;
                return;
            }
            if (zipCC?.containsPoint(x, y) == true)
            {
                hoverText = zipCC.hoverText;
                return;
            }
            if (folderCC?.containsPoint(x, y) == true)
            {
                hoverText = folderCC.hoverText;
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
            if (saveCC?.containsPoint(x, y) == true)
            {
                hoverText = saveCC.hoverText;
                return;
            }
            if (saveCC?.containsPoint(x, y) == true)
            {
                hoverText = saveCC.hoverText;
                return;
            }
            if (revertCC?.containsPoint(x, y) == true)
            {
                hoverText = revertCC.hoverText;
                return;
            }
            if (backCC?.containsPoint(x, y) == true)
            {
                hoverText = backCC.hoverText;
                return;
            }
        }
        public override void emergencyShutDown()
        {
            base.emergencyShutDown();
        }
        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (scrolling)
            {
                float interval = (scrollBarRunner.Height - scrollBar.bounds.Height) / (float)(totalLines - linesPerPage);

                float percent = (y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
                int which = (int)Math.Round(scrollBarRunner.Height / interval * percent);

                int newScroll = Math.Max(0, Math.Min(totalLines - linesPerPage, which));
                if (newScroll != scrolled)
                {
                    Game1.playSound("shiny4");
                    scrolled = newScroll;
                    RepopulateComponentList();
                }
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            scrolling = false;
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            scrolled = Math.Min(scrolled, totalLines - ((Game1.viewport.Height + 72 - spaceToClearTopBorder * 2 - 116) / 64));
            RepopulateComponentList();
        }
    }
}
