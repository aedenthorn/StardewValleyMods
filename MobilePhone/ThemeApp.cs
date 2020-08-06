using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        private static bool clicked;
        public static Dictionary<string, Texture2D[]> skinDict = new Dictionary<string, Texture2D[]>();
        public static Dictionary<string, Texture2D[]> backgroundDict = new Dictionary<string, Texture2D[]>();
        private static bool skinsNotBacks = true;
        public static List<string> skinList = new List<string>();
        public static List<string> backgroundList = new List<string>();

        public static void Initialize(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            Helper = helper;
            Monitor = monitor;
            Config = config;
            appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets","theme_app_icon.png"));
            ModEntry.apps.Add(Helper.ModRegistry.ModID + "_themes", GetApp());
            CreateThemeLists();
        }

        private static MobileApp GetApp()
        {
            return new MobileApp("Themes", OpenThemesApp, appIcon);
        }

        public static void OpenThemesApp()
        {
            Monitor.Log($"opening themes app");
            ModEntry.appRunning = true;
            ModEntry.runningApp = Helper.ModRegistry.ModID;
            skinListHeight = Config.ThemeItemMarginY + (int)Math.Ceiling(skinDict.Count / (float)ModEntry.themeGridWidth) * (Config.ThemeItemHeight + Config.ThemeItemMarginY) + Config.AppHeaderHeight;
            backListHeight = Config.ThemeItemMarginY + (int)Math.Ceiling(backgroundList.Count / (float)ModEntry.themeGridWidth) * (Config.ThemeItemHeight + Config.ThemeItemMarginY) + Config.AppHeaderHeight;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }
        private static void CreateThemeLists()
        {
            skinDict.Clear();
            backgroundDict.Clear();

            string[] skins = Directory.GetFiles(Path.Combine(Helper.DirectoryPath, "assets", "skins"), "*_landscape.png");

            foreach (string path in skins)
            {
                try
                {
                    Texture2D skin = Helper.Content.Load<Texture2D>(Path.Combine("assets", "skins", Path.GetFileName(path).Replace("_landscape","")));
                    Texture2D skinl = Helper.Content.Load<Texture2D>(Path.Combine("assets", "skins", Path.GetFileName(path)));
                    if(skin != null && skinl != null)
                    {
                        skinDict.Add(Path.Combine("assets", "skins", Path.GetFileName(path).Replace("_landscape", "")), new Texture2D[] { skin, skinl });
                        Monitor.Log($"loaded skin {path.Replace("_landscape", "")}");
                    }
                    else
                        Monitor.Log($"Couldn't load skin {path.Replace("_landscape", "")}: texture was null");
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Couldn't load skin {path.Replace("_landscape", "")}: {ex}");
                }
            }

            string[] papers = Directory.GetFiles(Path.Combine(Helper.DirectoryPath, "assets", "backgrounds"), "*_landscape.png");
            foreach (string path in papers)
            {
                try
                {
                    Texture2D back = Helper.Content.Load<Texture2D>(Path.Combine("assets", "backgrounds", Path.GetFileName(path).Replace("_landscape","")));
                    Texture2D backl = Helper.Content.Load<Texture2D>(Path.Combine("assets", "backgrounds", Path.GetFileName(path)));
                    if (back != null && backl != null)
                    {
                        backgroundDict.Add(Path.Combine("assets", "backgrounds", Path.GetFileName(path).Replace("_landscape", "")), new Texture2D[] { back, backl });
                        Monitor.Log($"loaded background {path.Replace("_landscape", "")}");
                    }
                    else
                        Monitor.Log($"Couldn't load background {path.Replace("_landscape", "")}: texture was null");
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Couldn't load background {path.Replace("_landscape", "")}: {ex}");
                }
            }
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
                    yOffset = (int)Math.Max(Math.Min(0, yOffset + dy), -1 * Math.Max(0, (skinsNotBacks ? skinListHeight : backListHeight) - (screenSize.Y - Config.AppHeaderHeight)));
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
                        bool sorb = mousePos.X < screenPos.X + screenSize.X / 2;
                        if(sorb != skinsNotBacks)
                        {
                            skinsNotBacks = sorb;
                            yOffset = 0;
                        }
                        
                    }
                    else
                    {
                        if (skinsNotBacks)
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
                        else
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
                    }
                }
            }

            lastMousePositionY = Game1.getMouseY();
            int startListY = (int)screenPos.Y + Config.AppHeaderHeight;
            //e.SpriteBatch.Draw(ModEntry.backgroundTexture, ModEntry.phoneRect, Color.White);

            if(yOffset < 0)
            {
                e.SpriteBatch.Draw(ModEntry.upArrowTexture, ModEntry.upArrowPosition, Color.White);
            }
            if (yOffset > PhoneUtils.GetScreenSize().Y - Config.AppHeaderHeight - skinListHeight)
            {
                e.SpriteBatch.Draw(ModEntry.downArrowTexture, ModEntry.downArrowPosition, Color.White);
            }

            int screenBottom = (int)(screenPos.Y + screenSize.Y);

            int count = skinsNotBacks ? skinList.Count : backgroundList.Count;

            for (int i = 0; i < count; i++)
            {
                Vector2 itemPos = GetItemPos(i);
                Rectangle r = new Rectangle(0,0,Config.PhoneWidth,Config.PhoneHeight);
                Rectangle destRect = new Rectangle((int)itemPos.X, (int)itemPos.Y, Config.ThemeItemWidth, Config.ThemeItemHeight);
                float yScale = Config.ThemeItemHeight / (float) Config.PhoneHeight;
                if (itemPos.Y < startListY - r.Height * yScale || itemPos.Y >= screenBottom)
                {
                    continue;
                }
                Rectangle sourceRect = r;
                int cutTop = 0;
                int cutBottom = 0;
                if(itemPos.Y < startListY)
                {
                    cutTop = (int)Math.Ceiling((startListY - itemPos.Y ) / yScale);
                    sourceRect = new Rectangle(r.X, (int)(r.Y + cutTop), r.Width, (int)(r.Height - cutTop));
                    destRect.Y = startListY;
                    destRect.Height -= (int)(cutTop * yScale);
                    itemPos = new Vector2(itemPos.X, startListY);
                }
                else if(itemPos.Y > screenBottom - r.Height * yScale - Config.AppHeaderHeight)
                {
                    cutBottom = (int)Math.Ceiling((screenBottom - Config.AppHeaderHeight - r.Height * yScale - itemPos.Y) / yScale);
                    destRect.Height += (int)(cutBottom * yScale);
                    sourceRect = new Rectangle(r.X, r.Y, r.Width, r.Height + cutBottom);
                }

                Texture2D texture = skinsNotBacks ? skinDict[skinList[i]][0] : backgroundDict[backgroundList[i]][0];

                e.SpriteBatch.Draw(texture, destRect, sourceRect, Color.White);
            }
            e.SpriteBatch.Draw(ModEntry.themesHeaderTexture, headerRect, Color.White);
            e.SpriteBatch.Draw(ModEntry.themesHeaderTexture, footerRect, Color.White);
            e.SpriteBatch.Draw(ModEntry.themesHighlightTexture, new Rectangle((int)(screenPos.X + (skinsNotBacks?0:screenSize.X/2f)),screenBottom - Config.AppHeaderHeight, (int)(screenSize.X / 2f), Config.AppHeaderHeight), Color.White);
            string headerText = Helper.Translation.Get("themes");
            string skinsText = Helper.Translation.Get("skins");
            string backsText = Helper.Translation.Get("backs");
            Vector2 headerTextSize = Game1.dialogueFont.MeasureString(headerText) * Config.HeaderTextScale;
            Vector2 skinsTextSize = Game1.dialogueFont.MeasureString(skinsText) * Config.HeaderTextScale;
            Vector2 backsTextSize = Game1.dialogueFont.MeasureString(backsText) * Config.HeaderTextScale;
            e.SpriteBatch.DrawString(Game1.dialogueFont, headerText, screenPos + new Vector2(screenSize.X / 2f - headerTextSize.X / 2f, Config.AppHeaderHeight / 2f - headerTextSize.Y / 2f ), Config.PhoneBookHeaderTextColor, 0, Vector2.Zero, Config.HeaderTextScale, SpriteEffects.None, 0.86f);
            e.SpriteBatch.DrawString(Game1.dialogueFont, "x", screenPos + new Vector2(screenSize.X - Config.AppHeaderHeight / 2f - Game1.dialogueFont.MeasureString("x").X * Config.HeaderTextScale / 2f, Config.AppHeaderHeight / 2f - headerTextSize.Y / 2f), Config.PhoneBookHeaderTextColor, 0, Vector2.Zero, Config.HeaderTextScale, SpriteEffects.None, 0.86f);
            e.SpriteBatch.DrawString(Game1.dialogueFont, skinsText, screenPos + new Vector2(screenSize.X / 4f - skinsTextSize.X / 2f, screenSize.Y - Config.AppHeaderHeight / 2f - headerTextSize.Y / 2f), skinsNotBacks ? Config.ThemesHeaderHighlightedTextColor : Config.ThemesHeaderTextColor, 0, Vector2.Zero, Config.HeaderTextScale, SpriteEffects.None, 0.86f);
            e.SpriteBatch.DrawString(Game1.dialogueFont, backsText, screenPos + new Vector2(screenSize.X * 3f / 4f - backsTextSize.X / 2f, screenSize.Y - Config.AppHeaderHeight / 2f - headerTextSize.Y / 2f), skinsNotBacks ? Config.ThemesHeaderTextColor : Config.ThemesHeaderHighlightedTextColor, 0, Vector2.Zero, Config.HeaderTextScale, SpriteEffects.None, 0.86f);

        }

        private static Vector2 GetItemPos(int i)
        {
            float x = ModEntry.screenPosition.X + Config.ContactMarginX + ((i % ModEntry.themeGridWidth) * (Config.ThemeItemWidth + Config.ThemeItemMarginX));
            float y = ModEntry.screenPosition.Y + Config.AppHeaderHeight + Config.ContactMarginY + ((i / ModEntry.themeGridWidth) * (Config.ThemeItemHeight + Config.ThemeItemMarginY));

            return new Vector2(x, y + yOffset);
        }
    }
}