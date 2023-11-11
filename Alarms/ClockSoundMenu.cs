using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.SDKs;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Alarms
{
    public class ClockSoundMenu : IClickableMenu
    {
        public static int scrolled;
        public static int setsPerPage = 6;
        public static int windowWidth = 64 * 16;
        public static ClockSoundMenu instance;
        public List<SoundComponentSet> soundComponentSets = new();
        public List<ClickableComponent> allComponents = new();
        public ClickableTextureComponent addCC;
        public ClickableTextureComponent upCC;
        public ClickableTextureComponent downCC;
        public string hoverText;
        public string hoveredItem;
        public static List<ClockSound> soundList = new();

        public ClockSoundMenu() : base(Game1.uiViewport.Width / 2 - (windowWidth + borderWidth * 2) / 2, -borderWidth, windowWidth + borderWidth * 2, Game1.uiViewport.Height, false)
        {
            ReloadSounds();
            RepopulateComponentList();

            exitFunction = emergencyShutDown;

            snapToDefaultClickableComponent();
        }

        private void RepopulateComponentList()
        {
            soundComponentSets.Clear();
            allComponents.Clear();
            
            int count = 0;
            int setHeight = 192;
            int lineHeight = 96;
            int clockWidth = 144;
            int seasonWidth = 108;
            int dowWidth = 56;
            int weekWidth = dowWidth * 7 + 8;
            int domWidth = 40;
            int monthWidth = domWidth * 7 + 12;
            Texture2D textBox = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
            for (int i = scrolled; i < Math.Min(setsPerPage + scrolled, soundList.Count); i++)
            {
                int xStart = xPositionOnScreen + spaceToClearSideBorder + borderWidth;
                int yStart = yPositionOnScreen + borderWidth + spaceToClearTopBorder - 24 + count * setHeight;
                int baseID = count * 1000;
                allComponents.Add(new ClickableComponent(new Rectangle(xStart, yStart + 8, 59, 48), "hourCC", "Hours "+i)
                {
                    myID = baseID,
                    upNeighborID = count > 0 ? baseID - 1000 : -9999,
                    rightNeighborID = baseID + 1,
                    downNeighborID = baseID + 2
                });
                allComponents.Add(new ClickableComponent(new Rectangle(xStart + 64, yStart + 8, 59, 48), "minCC", "Minutes " + i)
                {
                    myID = baseID + 1,
                    upNeighborID = count > 0 ? baseID - 1000 : -9999,
                    rightNeighborID = baseID + 4,
                    downNeighborID = baseID + 2
                });
                allComponents.Add(new ClickableComponent(new Rectangle(xStart, yStart + lineHeight, 360, 48), "notifyCC", "Notification " + i)
                {
                    myID = baseID + 2,
                    rightNeighborID = baseID + 3,
                    upNeighborID = baseID,
                    downNeighborID = baseID + 1000
                });
                allComponents.Add(new ClickableComponent(new Rectangle(xStart + 360 + 48, yStart + lineHeight, 192, 48), "soundCC", "Sound " + i) 
                {
                    myID = baseID + 3,
                    rightNeighborID = baseID + 15,
                    leftNeighborID = baseID + 2,
                    upNeighborID = baseID + 8,
                    downNeighborID = baseID + 1000
                });
                var set = new SoundComponentSet()
                {
                    hourText = new TextBox(textBox, null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart,
                        Y = yStart + 8,
                        Width = 64,
                        Text = soundList[i].hours + "",
                        
                    },
                    minText = new TextBox(textBox, null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + 64,
                        Y = yStart + 8,
                        Width = 64,
                        Text = (soundList[i].minutes < 10 && soundList[i].minutes >= 0 ? "0" : "") + soundList[i].minutes + ""
                    },
                    notificationText = new TextBox(textBox, null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart,
                        Y = yStart + lineHeight,
                        Width = 360,
                        Text = soundList[i].notification
                    },
                    soundText = new TextBox(textBox, null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + 420,
                        Y = yStart + lineHeight,
                        Text = soundList[i].sound
                    },
                    seasonCCs = new ClickableTextureComponent[]
                    {
                        new ClickableTextureComponent("Spring", new Rectangle(xStart + clockWidth, yStart, 48, 32), "", Utility.getSeasonNameFromNumber(0), Game1.mouseCursors, new Rectangle(406, 441, 12, 8), 4f)
                        {
                            myID = baseID + 4,
                            upNeighborID = count > 0 ? baseID - 1000 : -9999,
                            rightNeighborID = baseID + 5,
                            leftNeighborID = baseID + 1,
                            downNeighborID = baseID + 6
                        },
                        new ClickableTextureComponent("Summer", new Rectangle(xStart + clockWidth + 48, yStart, 48, 32), "", Utility.getSeasonNameFromNumber(1),  Game1.mouseCursors, new Rectangle(406, 441 + 8, 12, 8), 4f)
                        {
                            myID = baseID + 5,
                            upNeighborID = count > 0 ? baseID - 1000 : -9999,
                            rightNeighborID = baseID + 8,
                            leftNeighborID = baseID + 4,
                            downNeighborID = baseID + 7
                        },
                        new ClickableTextureComponent("Fall", new Rectangle(xStart + clockWidth, yStart + 32, 48, 32), "", Utility.getSeasonNameFromNumber(2), Game1.mouseCursors, new Rectangle(406, 441 + 16, 12, 8), 4f)
                        {
                            myID = baseID + 6,
                            upNeighborID = baseID + 4,
                            rightNeighborID = baseID + 7,
                            leftNeighborID = baseID + 1,
                            downNeighborID = baseID + 2
                        },
                        new ClickableTextureComponent("Winter", new Rectangle(xStart + clockWidth + 48, yStart + 32, 48, 32),"", Utility.getSeasonNameFromNumber(3), Game1.mouseCursors, new Rectangle(406, 441 + 24, 12, 8), 4f)
                        {
                            myID = baseID + 7,
                            upNeighborID = baseID + 5,
                            rightNeighborID = baseID + 8,
                            leftNeighborID = baseID + 6,
                            downNeighborID = baseID + 2
                        }
                    },
                    weekCCs = new ClickableComponent[]
                    {
                        new ClickableComponent(new Rectangle(xStart + clockWidth + seasonWidth, yStart + 8, dowWidth, lineHeight), ModEntry.SHelper.Translation.Get("monday"), ModEntry.SHelper.Translation.Get("monday-s"))
                        {
                            myID = baseID + 8,
                            upNeighborID = count > 0 ? baseID - 1000 : -9999,
                            rightNeighborID = baseID + 9,
                            leftNeighborID = baseID + 7,
                            downNeighborID = baseID + 2
                        },
                        new ClickableComponent(new Rectangle(xStart + clockWidth + seasonWidth + dowWidth, yStart + 8, dowWidth, lineHeight), ModEntry.SHelper.Translation.Get("tuesday"), ModEntry.SHelper.Translation.Get("tuesday-s"))
                        {
                            myID = baseID + 9,
                            upNeighborID = count > 0 ? baseID - 1000 : -9999,
                            rightNeighborID = baseID + 10,
                            leftNeighborID = baseID + 8,
                            downNeighborID = baseID + 2
                        },
                        new ClickableComponent(new Rectangle(xStart + clockWidth + seasonWidth + dowWidth * 2, yStart + 8, dowWidth, lineHeight), ModEntry.SHelper.Translation.Get("wednesday"), ModEntry.SHelper.Translation.Get("wednesday-s"))
                        {
                            myID = baseID + 10,
                            upNeighborID = count > 0 ? baseID - 1000 : -9999,
                            rightNeighborID = baseID + 11,
                            leftNeighborID = baseID + 9,
                            downNeighborID = baseID + 2
                        },
                        new ClickableComponent(new Rectangle(xStart + clockWidth + seasonWidth + dowWidth * 3, yStart + 8, dowWidth, lineHeight), ModEntry.SHelper.Translation.Get("thursday"), ModEntry.SHelper.Translation.Get("thursday-s"))
                        {
                            myID = baseID + 11,
                            upNeighborID = count > 0 ? baseID - 1000 : -9999,
                            rightNeighborID = baseID + 12,
                            leftNeighborID = baseID + 10,
                            downNeighborID = baseID + 3
                        },
                        new ClickableComponent(new Rectangle(xStart + clockWidth + seasonWidth + dowWidth * 4, yStart + 8, dowWidth, lineHeight), ModEntry.SHelper.Translation.Get("friday"), ModEntry.SHelper.Translation.Get("friday-s"))
                        {
                            myID = baseID + 12,
                            upNeighborID = count > 0 ? baseID - 1000 : -9999,
                            rightNeighborID = baseID + 13,
                            leftNeighborID = baseID + 11,
                            downNeighborID = baseID + 3
                        },
                        new ClickableComponent(new Rectangle(xStart + clockWidth + seasonWidth + dowWidth * 5, yStart + 8, dowWidth, lineHeight), ModEntry.SHelper.Translation.Get("saturday"), ModEntry.SHelper.Translation.Get("saturday-s"))
                        {
                            myID = baseID + 13,
                            upNeighborID = count > 0 ? baseID - 1000 : -9999,
                            rightNeighborID = baseID + 14,
                            leftNeighborID = baseID + 12,
                            downNeighborID = baseID + 3
                        },
                        new ClickableComponent(new Rectangle(xStart + clockWidth + seasonWidth + dowWidth * 6, yStart + 8, dowWidth, lineHeight), ModEntry.SHelper.Translation.Get("sunday"), ModEntry.SHelper.Translation.Get("sunday-s"))
                        {
                            myID = baseID + 14,
                            upNeighborID = count > 0 ? baseID - 1000 : -9999,
                            rightNeighborID = baseID + 15,
                            leftNeighborID = baseID + 13,
                            downNeighborID = baseID + 3
                        }
                    },
                    notifCC = new ClickableTextureComponent("", new Rectangle(xStart - 12, yStart + lineHeight + 8, 10, 28), "", ModEntry.SHelper.Translation.Get("notification"), Game1.mouseCursors, new Rectangle(403, 496, 5, 14), 2),
                    soundCC = new ClickableTextureComponent("", new Rectangle(xStart + 380, yStart + lineHeight + 8, 36, 36), "", ModEntry.SHelper.Translation.Get("sound"), Game1.mouseCursors, new Rectangle(128, 384, 9, 9), 4),
                    enabledBox = new ClickableTextureComponent("Enable " + i, new Rectangle(xStart + clockWidth + seasonWidth + weekWidth + monthWidth, yStart + 4, 36, 36), "", ModEntry.SHelper.Translation.Get("enabled"), Game1.mouseCursors, new Rectangle(227 + (soundList[i].enabled ? 9 : 0), 425, 9, 9), 4)
                    {
                        myID = baseID + 43,
                        upNeighborID = baseID - 1000,
                        rightNeighborID = count < setsPerPage / 2 ? -1 : -2,
                        leftNeighborID = baseID + 21,
                        downNeighborID = baseID + 44
                    },
                    deleteCC = new ClickableTextureComponent("Delete " + i, new Rectangle(xStart + clockWidth + seasonWidth + weekWidth + monthWidth, yStart + lineHeight + 16, 36, 36), "", ModEntry.SHelper.Translation.Get("delete"), Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 3)
                    {
                        myID = baseID + 44,
                        upNeighborID = baseID + 43,
                        leftNeighborID = baseID + 42,
                        rightNeighborID = count < setsPerPage / 2 ? -1 : -2,
                        downNeighborID = baseID + 1000
                    },
                    sound = soundList[i]
                    
                };
                allComponents.AddRange(set.seasonCCs);
                allComponents.AddRange(set.weekCCs);
                allComponents.Add(set.enabledBox);
                allComponents.Add(set.deleteCC);

                List<ClickableComponent> days = new();
                for(int j = 0; j < 28; j++)
                {
                    var bid = baseID + 15 + j;
                    var cc = new ClickableComponent(new Rectangle(xStart + clockWidth + seasonWidth + weekWidth + (j % 7) * domWidth, yStart + j / 7 * domWidth, domWidth, domWidth), (j + 1) + "", (j + 1) + "")
                    {
                        myID = bid,
                        upNeighborID = j < 7 ? baseID - 1000 : bid - 7,
                        rightNeighborID = j % 7 == 6 ? baseID + 43 : bid + 1,
                        leftNeighborID = j % 7 == 0 ? baseID + 14 : bid - 1,
                        downNeighborID = j >= 21 ? baseID + 1000 : bid + 7
                    };
                    allComponents.Add(cc);
                    days.Add(cc);
                }
                set.monthCCs = days.ToArray();
                soundComponentSets.Add(set);
                count++;
            }
            addCC = new ClickableTextureComponent("Add", new Rectangle(xPositionOnScreen + width - 128, yPositionOnScreen - 128 + height, 56, 56), "", ModEntry.SHelper.Translation.Get("add"), Game1.mouseCursors, new Rectangle(1, 412, 14, 14), 4)
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
            if (count + scrolled < soundList.Count)
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
            populateClickableComponentList();
        }

        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true);
            int count = 0;
            foreach(var set in soundComponentSets)
            {
                b.DrawString(Game1.tinyFont, (count+1 + scrolled) + "", new Vector2(xPositionOnScreen + 40, yPositionOnScreen + 96 + count * 192), Color.Black * 0.5f);
                set.hourText.Draw(b);
                set.minText.Draw(b);
                set.notifCC.draw(b);
                set.notificationText.Draw(b);
                set.soundCC.draw(b);
                set.soundText.Draw(b);
                for (int i = 0; i < set.seasonCCs.Length; i++)
                {
                    set.seasonCCs[i].draw(b);
                    if (set.sound.seasons is null || !set.sound.seasons[i])
                    {
                        b.Draw(Game1.staminaRect, set.seasonCCs[i].bounds, Color.DarkGray * 0.7f);
                    }
                }
                for(int i = 0; i < set.weekCCs.Length; i++)
                {
                    var m = Game1.dialogueFont.MeasureString(set.weekCCs[i].label);
                    var v = new Vector2(set.weekCCs[i].bounds.Center.X - m.X / 2, set.weekCCs[i].bounds.Y);
                    b.DrawString(Game1.dialogueFont, set.weekCCs[i].label, v + new Vector2(-1, 1), Color.Black * 0.5f);
                    b.DrawString(Game1.dialogueFont, set.weekCCs[i].label, v, set.sound.daysOfWeek is not null && set.sound.daysOfWeek[i] ? Color.Green : Color.White);
                }
                for(int i = 0; i < set.monthCCs.Length; i++)
                {
                    var m = Game1.smallFont.MeasureString(set.monthCCs[i].label);
                    var v = new Vector2(set.monthCCs[i].bounds.Center.X - m.X / 2, set.monthCCs[i].bounds.Y);
                    b.DrawString(Game1.smallFont, set.monthCCs[i].label, v + new Vector2(-1, 1), Color.Black * 0.5f);
                    b.DrawString(Game1.smallFont, set.monthCCs[i].label, v, set.sound.daysOfMonth is not null && set.sound.daysOfMonth[i] ? Color.Green : Color.White);
                }
                set.soundCC.draw(b);
                set.notifCC.draw(b);
                set.enabledBox.draw(b);
                set.deleteCC.draw(b);
                b.Draw(Game1.menuTexture, new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + 84 + (count + 1) * 192, width - 64, 16), new Rectangle(40, 16, 1, 16), Color.White);
                count++;
            }
            addCC.draw(b);
            upCC?.draw(b);
            downCC?.draw(b);
            if (hoverText != null && hoveredItem == null)
            {
                drawHoverText(b, hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null);
            }
            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            for(int i = 0; i < soundComponentSets.Count; i++)
            {
                var set = soundComponentSets[i];
                set.hourText.Selected = false;
                set.minText.Selected = false;
                set.notificationText.Selected = false;
                set.soundText.Selected = false;
                set.hourText.Update();
                set.minText.Update();
                set.notificationText.Update();
                set.soundText.Update();
                for(int j = 0; j < set.seasonCCs.Length; j++)
                {
                    if (set.seasonCCs[j].containsPoint(x, y))
                    {
                        
                        if (soundComponentSets[i].sound.seasons is null)
                            soundComponentSets[i].sound.seasons = new bool[4];
                        soundComponentSets[i].sound.seasons[j] = !soundComponentSets[i].sound.seasons[j];
                        Game1.playSound(soundComponentSets[i].sound.seasons[j] ? "bigSelect" : "bigDeSelect");
                        soundList[i + scrolled] = soundComponentSets[i].sound;
                        SaveSounds();
                        RepopulateComponentList();
                        return;
                    }
                }
                for(int j = 0; j < set.weekCCs.Length; j++)
                {
                    if (set.weekCCs[j].containsPoint(x, y))
                    {
                        if (soundComponentSets[i].sound.daysOfWeek is null)
                            soundComponentSets[i].sound.daysOfWeek = new bool[7];
                        soundComponentSets[i].sound.daysOfWeek[j] = !soundComponentSets[i].sound.daysOfWeek[j];
                        Game1.playSound(soundComponentSets[i].sound.daysOfWeek[j] ? "bigSelect" : "bigDeSelect");
                        soundList[i + scrolled] = soundComponentSets[i].sound;
                        SaveSounds();
                        RepopulateComponentList();
                        return;
                    }
                }
                for(int j = 0; j < set.monthCCs.Length; j++)
                {
                    if (set.monthCCs[j].containsPoint(x, y))
                    {
                        if (soundComponentSets[i].sound.daysOfMonth is null)
                            soundComponentSets[i].sound.daysOfMonth = new bool[28];
                        soundComponentSets[i].sound.daysOfMonth[j] = !soundComponentSets[i].sound.daysOfMonth[j];
                        Game1.playSound(soundComponentSets[i].sound.daysOfMonth[j] ? "bigSelect" : "bigDeSelect");
                        soundList[i + scrolled] = soundComponentSets[i].sound;
                        SaveSounds();
                        RepopulateComponentList();
                        return;
                    }
                }
                if(set.soundCC.containsPoint(x, y))
                {
                    try
                    {
                        Game1.playSound(set.soundText.Text);
                    }
                    catch { }
                    return;
                }
                if(set.enabledBox.containsPoint(x, y))
                {
                    soundComponentSets[i].sound.enabled = !soundComponentSets[i].sound.enabled;
                    Game1.playSound(soundComponentSets[i].sound.enabled ? "bigSelect" : "bigDeSelect");
                    soundList[i + scrolled] = soundComponentSets[i].sound;
                    SaveSounds();
                    RepopulateComponentList();
                    return;
                }
                if(set.deleteCC.containsPoint(x, y)) 
                {
                    Game1.playSound("trashcan");
                    soundList.RemoveAt(i + scrolled);
                    if(scrolled >= soundList.Count - setsPerPage)
                    {
                        scrolled = Math.Max(0, soundList.Count - setsPerPage);
                    }
                    SaveSounds();
                    RepopulateComponentList();
                    return;
                }
            }
            if(addCC.containsPoint(x, y))
            {
                Game1.playSound("bigSelect");
                soundList.Add(new ClockSound());
                SaveSounds();
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
            if (direction < 0 && scrolled < soundList.Count - setsPerPage)
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

        public override void receiveKeyPress(Keys key)
        {
            bool close = Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose();
            if(!Game1.options.snappyMenus || !Game1.options.gamepadControls)
            {
                for (int i = 0; i < soundComponentSets.Count; i++)
                {
                    var hc = soundComponentSets[i].hourText.Text != soundList[i + scrolled].hours + "";
                    if (soundComponentSets[i].hourText.Selected && (!close || hc))
                    {
                        Game1.playSound("cowboy_monsterhit");
                        if (hc && int.TryParse(soundComponentSets[i].hourText.Text, out int h) && ((h >= 6 && h < 26) || h < 0))
                        {
                            soundList[i + scrolled].hours = h;
                            SaveSounds();
                        }
                        return;
                    }
                    var mc = soundComponentSets[i].minText.Text != soundList[i + scrolled].minutes + "";
                    if (soundComponentSets[i].minText.Selected && (!close || mc))
                    {
                        Game1.playSound("cowboy_monsterhit");
                        if (mc && int.TryParse(soundComponentSets[i].minText.Text, out int m) && m < 60)
                        {
                            soundList[i + scrolled].minutes = m;
                            SaveSounds();
                        }
                        return;
                    }
                    var nc = soundComponentSets[i].notificationText.Text != soundList[i + scrolled].notification;
                    if (soundComponentSets[i].notificationText.Selected && (!close || nc))
                    {
                        Game1.playSound("cowboy_monsterhit");
                        if (nc)
                        {
                            soundList[i + scrolled].notification = soundComponentSets[i].notificationText.Text;
                            SaveSounds();
                        }
                        return;
                    }
                    var sc = soundComponentSets[i].soundText.Text != soundList[i + scrolled].sound;
                    if (soundComponentSets[i].soundText.Selected && (!close || sc))
                    {
                        Game1.playSound("cowboy_monsterhit");
                        if (sc)
                        {
                            soundList[i + scrolled].sound = soundComponentSets[i].soundText.Text;
                            SaveSounds();
                        }
                        return;
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


        public override void snapToDefaultClickableComponent()
        {
            if(Game1.options.snappyMenus && Game1.options.gamepadControls)
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

            for (int i = 0; i < soundComponentSets.Count; i++)
            {
                var set = soundComponentSets[i];
                for (int j = 0; j < set.seasonCCs.Length; j++)
                {
                    if (set.seasonCCs[j].containsPoint(x, y))
                    {
                        hoverText = Utility.getSeasonNameFromNumber(j);
                        return;
                    }
                }
                for (int j = 0; j < set.weekCCs.Length; j++)
                {
                    if (set.weekCCs[j].containsPoint(x, y))
                    {
                        hoverText = set.weekCCs[j].name;
                        return;
                    }
                }
                for (int j = 0; j < set.monthCCs.Length; j++)
                {
                    if (set.monthCCs[j].containsPoint(x, y))
                    {
                        hoverText = (j + 1) + "";
                        return;
                    }
                }
                if(set.notifCC.containsPoint(x, y))
                {
                    hoverText = set.notifCC.hoverText;
                    return;
                }
                if(set.soundCC.containsPoint(x, y))
                {
                    hoverText = set.soundCC.hoverText;
                    return;
                }
                if(set.enabledBox.containsPoint(x, y))
                {
                    hoverText = set.enabledBox.hoverText;
                    return;
                }
                if(set.deleteCC.containsPoint(x, y))
                {
                    hoverText = set.deleteCC.hoverText;
                    return;
                }
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
        public override void emergencyShutDown()
        {
            base.emergencyShutDown();
        }

        private void ReloadSounds()
        {
            var path = Path.Combine(ModEntry.SHelper.DirectoryPath, "assets", $"sounds-{Constants.SaveFolderName}.json");
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "[]");
                soundList = new();
            }
            else
            {
                soundList = JsonConvert.DeserializeObject<List<ClockSound>>(File.ReadAllText(path));
            }
        }
        public static void SaveSounds()
        {
            for (int i = 0; i < soundList.Count; i++)
            {
                var sound = soundList[i];
                if (sound.seasons is not null)
                {
                    if (!sound.seasons.ToList().Contains(true))
                        soundList[i].seasons = null;
                }
                if (sound.daysOfWeek is not null)
                {
                    if (!sound.daysOfWeek.ToList().Contains(true))
                        soundList[i].daysOfWeek = null;
                }
                if (sound.daysOfMonth is not null)
                {
                    if (!sound.daysOfMonth.ToList().Contains(true))
                        soundList[i].daysOfMonth = null;
                }
            }
            var path = Path.Combine(ModEntry.SHelper.DirectoryPath, "assets", $"sounds-{Constants.SaveFolderName}.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(soundList, Formatting.Indented));

        }
    }
}
