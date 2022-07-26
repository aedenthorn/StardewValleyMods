using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;

namespace MobilePhone
{
    public class ThemeApp
    {
        private static IModHelper Helper;
        private static IMonitor Monitor;
        private static ModConfig Config;
        private static Texture2D appIcon;
        private static bool dragging;
        private static int yOffset;
        private static int lastMousePositionY;
        private static float skinListHeight;
        private static float backListHeight;
        private static int ringListHeight;
        private static bool clicked;
        public static Dictionary<string, Texture2D[]> skinDict = new Dictionary<string, Texture2D[]>();
        public static Dictionary<string, Texture2D[]> backgroundDict = new Dictionary<string, Texture2D[]>();
        public static Dictionary<string, object> ringDict = new Dictionary<string, object>();
        public static List<string> ringList;
        public static List<string> skinList = new List<string>();
        public static List<string> backgroundList = new List<string>();
        private static int whichTab = 0;

        public static void Initialize(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            Helper = helper;
            Monitor = monitor;
            Config = config;
            appIcon = Helper.ModContent.Load<Texture2D>(Path.Combine("assets","theme_app_icon.png"));
            ModEntry.apps.Add(Helper.ModRegistry.ModID + "_themes", GetApp());
            CreateThemeLists();
        }

        private static MobileApp GetApp()
        {
            return new MobileApp(Helper.Translation.Get("customize"), OpenThemesApp, appIcon);
        }

        public static void OpenThemesApp()
        {
            Monitor.Log($"opening customize app");
            ModEntry.appRunning = true;
            ModEntry.runningApp = Helper.ModRegistry.ModID;
            skinListHeight = Config.ThemeItemMarginY + (int)Math.Ceiling(skinDict.Count / (float)ModEntry.themeGridWidth) * (Config.ThemeItemHeight + Config.ThemeItemMarginY) + Config.AppHeaderHeight * 2;
            backListHeight = Config.ThemeItemMarginY + (int)Math.Ceiling(backgroundList.Count / (float)ModEntry.themeGridWidth) * (Config.ThemeItemHeight + Config.ThemeItemMarginY) + Config.AppHeaderHeight * 2;
            ringListHeight = Config.RingListItemMarginY + ringList.Count * Config.RingListItemHeight + Config.AppHeaderHeight * 2;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }
        private static void CreateThemeLists()
        {

            if (Directory.Exists(Path.Combine(Helper.DirectoryPath, "assets", "skins")))
            {
                string[] skins = Directory.GetFiles(Path.Combine(Helper.DirectoryPath, "assets", "skins"), "*_landscape.png");

                foreach (string path in skins)
                {
                    try
                    {
                        Texture2D skin = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "skins", Path.GetFileName(path).Replace("_landscape", "")));
                        Texture2D skinl = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "skins", Path.GetFileName(path)));
                        if (skin != null && skinl != null)
                        {
                            skinDict.Add(Path.Combine("assets", "skins", Path.GetFileName(path).Replace("_landscape", "")), new Texture2D[] { skin, skinl });
                            Monitor.Log($"loaded skin {path.Replace("_landscape", "")}");
                        }
                        else
                            Monitor.Log($"Couldn't load skin {path.Replace("_landscape", "")}: texture was null", LogLevel.Error);
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log($"Couldn't load skin {path.Replace("_landscape", "")}: {ex}", LogLevel.Error);
                    }
                }
            }

            if (Directory.Exists(Path.Combine(Helper.DirectoryPath, "assets", "backgrounds")))
            {
                string[] papers = Directory.GetFiles(Path.Combine(Helper.DirectoryPath, "assets", "backgrounds"), "*_landscape.png");
                foreach (string path in papers)
                {
                    try
                    {
                        Texture2D back = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "backgrounds", Path.GetFileName(path).Replace("_landscape", "")));
                        Texture2D backl = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "backgrounds", Path.GetFileName(path)));
                        if (back != null && backl != null)
                        {
                            backgroundDict.Add(Path.Combine("assets", "backgrounds", Path.GetFileName(path).Replace("_landscape", "")), new Texture2D[] { back, backl });
                            Monitor.Log($"loaded background {path.Replace("_landscape", "")}");
                        }
                        else
                            Monitor.Log($"Couldn't load background {path.Replace("_landscape", "")}: texture was null", LogLevel.Error);
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log($"Couldn't load background {path.Replace("_landscape", "")}: {ex}", LogLevel.Error);
                    }
                }
            }


            if (Directory.Exists(Path.Combine(Helper.DirectoryPath, "assets", "ringtones")))
            {
                string[] rings = Directory.GetFiles(Path.Combine(Helper.DirectoryPath, "assets", "ringtones"), "*.wav");
                foreach (string path in rings)
                {
                    try
                    {
                        object ring;
                        try
                        {
                            var type = Type.GetType("System.Media.SoundPlayer, System");
                            ring = Activator.CreateInstance(type, new object[] { path });
                        }
                        catch 
                        {
                            ring = SoundEffect.FromStream(new FileStream(path, FileMode.Open));
                        }
                        if (ring != null)
                        {
                            ringDict.Add(Path.GetFileName(path).Replace(".wav", ""), ring);
                            Monitor.Log($"loaded ring {path}");
                        }
                        else
                            Monitor.Log($"Couldn't load ring {path}", LogLevel.Error);
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log($"Couldn't load ring {path}:\r\n{ex}", LogLevel.Error);
                    }
                }
                rings = Config.BuiltInRingTones.Split(',');
                foreach (string ring in rings)
                {
                    ringDict.Add(ring, null);
                }
            }
            ringList = ringDict.Keys.ToList();
            skinList = skinDict.Keys.ToList();
            backgroundList = backgroundDict.Keys.ToList();
        }


        private static void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (ModEntry.callingNPC != null || !ModEntry.appRunning || ModEntry.runningApp != Helper.ModRegistry.ModID || !ModEntry.screenRect.Contains(Game1.getMousePosition()))
                return;

            if (e.Button == SButton.MouseLeft)
            {
                Monitor.Log($"Theme app caught click");

                Helper.Input.Suppress(SButton.MouseLeft);

                clicked = true;

                lastMousePositionY = Game1.getMouseY();
            }
        }

        private static void SetSkin(string skinName)
        {
            if (!skinDict.ContainsKey(skinName) || skinDict[skinName][0] == null || skinDict[skinName][1] == null)
                return;
            ModEntry.phoneTexture = skinDict[skinName][0];
            ModEntry.phoneRotatedTexture = skinDict[skinName][1];
            Config.PhoneSkinPath = skinName;
            Helper.WriteConfig(Config);

        }

        private static void SetBackground(string backgroundName)
        {
            if (!backgroundDict.ContainsKey(backgroundName) || backgroundDict[backgroundName][0] == null || backgroundDict[backgroundName][1] == null)
                return;
            ModEntry.backgroundTexture = backgroundDict[backgroundName][0];
            ModEntry.backgroundRotatedTexture = backgroundDict[backgroundName][1];
            Config.BackgroundPath = backgroundName;
            Helper.WriteConfig(Config);
        }

        private static void SetRing(string ringName)
        {
            if (!ringDict.ContainsKey(ringName))
                return;
            ModEntry.ringSound = ringDict[ringName];
            if (ModEntry.ringSound != null && ModEntry.ringSound is SoundEffect)
                (ModEntry.ringSound as SoundEffect).Play();
            else if(Config.BuiltInRingTones.Split(',').Contains(ringName))
                Game1.playSound(ringName);
            Config.PhoneRingTone = ringName;
            Helper.WriteConfig(Config);
        }

        private static void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (ModEntry.callingNPC != null)
                return;

            if (!ModEntry.appRunning || !ModEntry.phoneOpen || ModEntry.runningApp != Helper.ModRegistry.ModID)
            {
                ModEntry.appRunning = false;
                Helper.Events.Display.RenderedWorld -= Display_RenderedWorld;
                Helper.Events.Input.ButtonPressed -= Input_ButtonPressed;
                return;
            }
            Vector2 screenPos = PhoneUtils.GetScreenPosition();
            Vector2 screenSize = PhoneUtils.GetScreenSize();
            Rectangle headerRect = new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)screenSize.X, Config.AppHeaderHeight);
            Rectangle footerRect = new Rectangle((int)screenPos.X, (int)screenPos.Y + (int)screenSize.Y - Config.AppHeaderHeight, (int)screenSize.X, Config.AppHeaderHeight);
            Point mousePos = Game1.getMousePosition();

            if (Helper.Input.IsSuppressed(SButton.MouseLeft))
            {
                int dy = mousePos.Y - lastMousePositionY;
                if (Math.Abs(dy) > 0 && ModEntry.screenRect.Contains(mousePos) && !headerRect.Contains(mousePos) && !footerRect.Contains(mousePos))
                {
                    dragging = true;
                }
                if (dragging)
                {
                    float listHeight;
                    if (whichTab == 0)
                        listHeight = skinListHeight;
                    else if (whichTab == 1)
                        listHeight = backListHeight;
                    else 
                        listHeight = ringListHeight;
                    yOffset = (int)Math.Max(Math.Min(0, yOffset + dy), -1 * Math.Max(0, listHeight - screenSize.Y));
                }
            }

            if (clicked && !Helper.Input.IsSuppressed(SButton.MouseLeft))
            {
                clicked = false;
                Monitor.Log($"unclicked");
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
                    else if(footerRect.Contains(mousePos))
                    {
                        int newTab = (int)((mousePos.X - screenPos.X) / (screenSize.X / 3));
                        if(whichTab != newTab)
                        {
                            whichTab = newTab;
                            yOffset = 0;
                            int listcount = whichTab == 0 ? skinList.Count : (whichTab == 1 ? backgroundList.Count : ringList.Count);
                            Monitor.Log($"switching to tab {whichTab}: {listcount} items");
                        }

                    }
                    else
                    {
                        if (whichTab == 0)
                        {
                            for(int i = 0; i < skinList.Count; i++)
                            {
                                Vector2 pos = GetItemPos(i);
                                Rectangle r = new Rectangle((int)pos.X, (int)pos.Y, Config.ThemeItemWidth, Config.ThemeItemHeight);
                                if (r.Contains(mousePos))
                                {
                                    Monitor.Log($"switching to {skinList[i]}");
                                    SetSkin(skinList[i]);
                                }
                            }

                        }
                        else if (whichTab == 1)
                        {
                            for(int i = 0; i < backgroundList.Count; i++)
                            {
                                Vector2 pos = GetItemPos(i);
                                Rectangle r = new Rectangle((int)pos.X, (int)pos.Y, Config.ThemeItemWidth, Config.ThemeItemHeight);
                                if (r.Contains(mousePos))
                                {
                                    Monitor.Log($"switching to {backgroundList[i]}");
                                    SetBackground(backgroundList[i]);
                                }
                            }
                        }
                        else if (whichTab == 2)
                        {
                            for(int i = 0; i < ringList.Count; i++)
                            {
                                Vector2 pos = GetItemPos(i);
                                Rectangle r = new Rectangle((int)pos.X, (int)pos.Y, ModEntry.phoneWidth, Config.RingListItemHeight);
                                if (r.Contains(mousePos))
                                {
                                    Monitor.Log($"switching to {ringList[i]}");
                                    SetRing(ringList[i]);
                                }
                            }
                        }
                    }
                }
            }

            lastMousePositionY = Game1.getMouseY();
            int startListY = (int)screenPos.Y + Config.AppHeaderHeight;

            if (whichTab == 2)
                e.SpriteBatch.Draw(ModEntry.ringListBackgroundTexture, ModEntry.screenRect, Color.White);
            else
                e.SpriteBatch.Draw(ModEntry.backgroundTexture, ModEntry.phoneRect, Color.White);

            if(yOffset < 0)
            {
                e.SpriteBatch.Draw(ModEntry.upArrowTexture, ModEntry.upArrowPosition, Color.White);
            }
            if (yOffset > PhoneUtils.GetScreenSize().Y - Config.AppHeaderHeight - skinListHeight)
            {
                e.SpriteBatch.Draw(ModEntry.downArrowTexture, ModEntry.downArrowPosition, Color.White);
            }

            int screenBottom = (int)(screenPos.Y + screenSize.Y);

            int count = whichTab == 0 ? skinList.Count : (whichTab == 1 ? backgroundList.Count : ringList.Count );


            for (int i = 0; i < count; i++)
            {
                Vector2 itemPos = GetItemPos(i);
                if (whichTab < 2)
                {
                    Rectangle r = new Rectangle(0,0,Config.PhoneWidth, Config.PhoneHeight);
                    Rectangle sourceRect = r;
                    Rectangle destRect;
                    destRect = new Rectangle((int)itemPos.X, (int)itemPos.Y, Config.ThemeItemWidth, Config.ThemeItemHeight);
                    float yScale = Config.ThemeItemHeight / (float)Config.PhoneHeight;
                    if (itemPos.Y < startListY - r.Height * yScale || itemPos.Y >= screenBottom)
                    {
                        continue;
                    }
                    int cutTop = 0;
                    int cutBottom = 0;
                    if (itemPos.Y < startListY)
                    {
                        cutTop = (int)Math.Ceiling((startListY - itemPos.Y) / yScale);
                        sourceRect = new Rectangle(r.X, r.Y + cutTop, r.Width, r.Height - cutTop);
                        destRect.Y = startListY;
                        destRect.Height -= (int)(cutTop * yScale);
                        itemPos = new Vector2(itemPos.X, startListY);
                    }
                    else if (itemPos.Y > screenBottom - r.Height * yScale - Config.AppHeaderHeight)
                    {
                        cutBottom = (int)Math.Ceiling((screenBottom - Config.AppHeaderHeight - r.Height * yScale - itemPos.Y) / yScale);
                        destRect.Height += (int)(cutBottom * yScale);
                        sourceRect = new Rectangle(r.X, r.Y, r.Width, r.Height + cutBottom);
                    }
                    Texture2D texture = whichTab == 0 ? skinDict[skinList[i]][0] : backgroundDict[backgroundList[i]][0];
                    //Monitor.Log($"drawing texture {i} {texture.Width}x{texture.Height} {destRect} {ModEntry.screenRect}");
                    e.SpriteBatch.Draw(texture, destRect, sourceRect, Color.White);
                }
                else
                {
                    if (itemPos.Y < screenPos.Y || itemPos.Y >= screenBottom - Config.AppHeaderHeight)
                    {
                        continue;
                    }
                    if (ringList[i] == Config.PhoneRingTone)
                        e.SpriteBatch.Draw(ModEntry.ringListHighlightTexture, new Rectangle((int)(itemPos.X), (int)itemPos.Y, (int)(screenSize.X), Config.RingListItemHeight), Color.White);

                    string itemName = ringList[i];
                    if (itemName.Contains(":"))
                        itemName = itemName.Split(':')[1];
                    e.SpriteBatch.DrawString(Game1.dialogueFont, itemName, itemPos, Config.RingListItemColor, 0, Vector2.Zero, Config.RingListItemScale, SpriteEffects.None, 0.86f);
                }
            }
            e.SpriteBatch.Draw(ModEntry.themesHeaderTexture, headerRect, Color.White);
            e.SpriteBatch.Draw(ModEntry.themesHeaderTexture, footerRect, Color.White);
            e.SpriteBatch.Draw(ModEntry.themesHighlightTexture, new Rectangle((int)(screenPos.X + (screenSize.X/3f) * whichTab),screenBottom - Config.AppHeaderHeight, (int)(screenSize.X / 3f), Config.AppHeaderHeight), Color.White);
            string headerText = Helper.Translation.Get("themes");
            string skinsText = Helper.Translation.Get("skins");
            string backsText = Helper.Translation.Get("backs");
            string ringsText = Helper.Translation.Get("rings");
            Vector2 headerTextSize = Game1.dialogueFont.MeasureString(headerText) * Config.HeaderTextScale;
            Vector2 skinsTextSize = Game1.dialogueFont.MeasureString(skinsText) * Config.TabTextScale;
            Vector2 backsTextSize = Game1.dialogueFont.MeasureString(backsText) * Config.TabTextScale;
            Vector2 ringsTextSize = Game1.dialogueFont.MeasureString(ringsText) * Config.TabTextScale;
            e.SpriteBatch.DrawString(Game1.dialogueFont, headerText, screenPos + new Vector2(screenSize.X / 2f - headerTextSize.X / 2f, Config.AppHeaderHeight / 2f - headerTextSize.Y / 2f ), Config.PhoneBookHeaderTextColor, 0, Vector2.Zero, Config.HeaderTextScale, SpriteEffects.None, 0.86f);
            e.SpriteBatch.DrawString(Game1.dialogueFont, "x", screenPos + new Vector2(screenSize.X - Config.AppHeaderHeight / 2f - Game1.dialogueFont.MeasureString("x").X * Config.HeaderTextScale / 2f, Config.AppHeaderHeight / 2f - headerTextSize.Y / 2f), Config.PhoneBookHeaderTextColor, 0, Vector2.Zero, Config.HeaderTextScale, SpriteEffects.None, 0.86f);
            e.SpriteBatch.DrawString(Game1.dialogueFont, skinsText, screenPos + new Vector2(screenSize.X / 6f - skinsTextSize.X / 2f, screenSize.Y - Config.AppHeaderHeight / 2f - headerTextSize.Y / 2f), whichTab == 0 ? Config.ThemesHeaderHighlightedTextColor : Config.ThemesHeaderTextColor, 0, Vector2.Zero, Config.TabTextScale, SpriteEffects.None, 0.86f);
            e.SpriteBatch.DrawString(Game1.dialogueFont, backsText, screenPos + new Vector2(screenSize.X / 2f - backsTextSize.X / 2f, screenSize.Y - Config.AppHeaderHeight / 2f - headerTextSize.Y / 2f), whichTab == 1 ? Config.ThemesHeaderHighlightedTextColor : Config.ThemesHeaderTextColor, 0, Vector2.Zero, Config.TabTextScale, SpriteEffects.None, 0.86f);
            e.SpriteBatch.DrawString(Game1.dialogueFont, ringsText, screenPos + new Vector2(screenSize.X * 5f / 6f - ringsTextSize.X / 2f, screenSize.Y - Config.AppHeaderHeight / 2f - headerTextSize.Y / 2f), whichTab == 2 ? Config.ThemesHeaderHighlightedTextColor : Config.ThemesHeaderTextColor, 0, Vector2.Zero, Config.TabTextScale, SpriteEffects.None, 0.86f);

        }

        private static Vector2 GetItemPos(int i)
        {
            float x;
            float y;
            if(whichTab < 2)
            {
                x = ModEntry.screenPosition.X + Config.ContactMarginX + ((i % ModEntry.themeGridWidth) * (Config.ThemeItemWidth + Config.ThemeItemMarginX));
                y = ModEntry.screenPosition.Y + Config.AppHeaderHeight + Config.ContactMarginY + ((i / ModEntry.themeGridWidth) * (Config.ThemeItemHeight + Config.ThemeItemMarginY));
            }
            else
            {
                x = ModEntry.screenPosition.X;
                y = ModEntry.screenPosition.Y + Config.AppHeaderHeight + (i * Config.RingListItemHeight);
            }
            return new Vector2(x, y + yOffset);
        }
    }
}