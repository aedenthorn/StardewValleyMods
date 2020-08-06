using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MobilePhone
{
    public class PhoneUtils
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }

        public static void RotatePhone()
        {
            ModEntry.phoneRotated = !ModEntry.phoneRotated;
            RefreshPhoneLayout();
            Game1.playSound("dwop");
        }

        public static void TogglePhone(bool value)
        {
            ModEntry.phoneOpen = value;
            Game1.playSound("dwop");
            if (!value)
            {
                Monitor.Log($"Closing phone");
                if (Game1.activeClickableMenu is MobilePhoneMenu)
                    Game1.activeClickableMenu = null;
                ModEntry.appRunning = false;
            }
            else
            {
                Monitor.Log($"Opening phone");
                Game1.activeClickableMenu = new MobilePhoneMenu();
            }
        }
        public static void TogglePhone()
        {
            if(ModEntry.phoneOpen && ModEntry.appRunning)
            {
                Game1.playSound("dwop");
                ModEntry.appRunning = false;
                Game1.activeClickableMenu = new MobilePhoneMenu();
                return;
            }
            TogglePhone(!ModEntry.phoneOpen);
        }

        public static void ToggleApp(bool open)
        {
            ModEntry.appRunning = open;
            if (!open)
                ModEntry.runningApp = null;

            if (!ModEntry.phoneOpen)
            {
                Game1.activeClickableMenu = null;
            }
            else if(ModEntry.phoneOpen)
            {
                Game1.activeClickableMenu = new MobilePhoneMenu();
            }
        }

        internal static void OrderApps()
        {
            string[] appOrder = Config.AppList;
            ModEntry.appOrder = new List<string>();
            foreach(string app in appOrder)
            {
                if (ModEntry.apps.ContainsKey(app))
                    ModEntry.appOrder.Add(app);
            }
            foreach(string app in ModEntry.apps.Keys)
            {
                if (!ModEntry.appOrder.Contains(app))
                    ModEntry.appOrder.Add(app);
            }
            Config.AppList = ModEntry.appOrder.ToArray();
            Helper.WriteConfig(Config);
        }

        public static void RefreshPhoneLayout()
        {
            if (ModEntry.phoneRotated)
            {
                ModEntry.phoneWidth = Config.PhoneRotatedWidth;
                ModEntry.phoneHeight = Config.PhoneRotatedHeight;
                ModEntry.screenWidth = Config.ScreenRotatedWidth;
                ModEntry.screenHeight = Config.ScreenRotatedHeight;
                ModEntry.phoneOffsetX = Config.PhoneRotatedOffsetX;
                ModEntry.phoneOffsetY = Config.PhoneRotatedOffsetY;
                ModEntry.screenOffsetX = Config.ScreenRotatedOffsetX;
                ModEntry.screenOffsetY = Config.ScreenRotatedOffsetY;
            }
            else
            {
                ModEntry.phoneWidth = Config.PhoneWidth;
                ModEntry.phoneHeight = Config.PhoneHeight;
                ModEntry.screenWidth = Config.ScreenWidth;
                ModEntry.screenHeight = Config.ScreenHeight;
                ModEntry.phoneOffsetX = Config.PhoneOffsetX;
                ModEntry.phoneOffsetY = Config.PhoneOffsetY;
                ModEntry.screenOffsetX = Config.ScreenOffsetX;
                ModEntry.screenOffsetY = Config.ScreenOffsetY;
            }
            ModEntry.phonePosition = GetPhonePosition();
            ModEntry.screenPosition = GetScreenPosition();
            ModEntry.phoneIconPosition = GetPhoneIconPosition();
            ModEntry.screenRect = new Rectangle((int)ModEntry.screenPosition.X, (int)ModEntry.screenPosition.Y, (int)ModEntry.screenWidth, (int)ModEntry.screenHeight);
            ModEntry.phoneRect = new Rectangle((int)ModEntry.phonePosition.X, (int)ModEntry.phonePosition.Y, ModEntry.phoneWidth, ModEntry.phoneHeight);
            GetArrowPositions();
            ModEntry.appColumns = (ModEntry.screenWidth - Config.IconMarginX) / (Config.IconWidth + Config.IconMarginX);
            ModEntry.appRows = (ModEntry.screenHeight - Config.IconMarginY) / (Config.IconHeight + Config.IconMarginY);
            ModEntry.gridWidth = (ModEntry.screenWidth - Config.ContactMarginX) / (Config.ContactWidth + Config.ContactMarginX);
            ModEntry.gridHeight = (ModEntry.screenHeight - Config.ContactMarginY) / (Config.ContactHeight + Config.ContactMarginY);
            ModEntry.themeGridWidth = (ModEntry.screenWidth - Config.ThemeItemMarginX) / (Config.ThemeItemWidth + Config.ThemeItemMarginX);
            ModEntry.themeGridHeight = (ModEntry.screenHeight - Config.ThemeItemMarginY) / (Config.ThemeItemHeight + Config.ThemeItemMarginY);
            Vector2 screenSize = GetScreenSize();
            ModEntry.phoneBookTexture = PhoneVisuals.MakeColorTexture(Config.PhoneBookBackgroundColor, screenSize);
            ModEntry.phoneBookHeaderTexture = PhoneVisuals.MakeColorTexture(Config.PhoneBookHeaderColor, new Vector2(screenSize.X, Config.AppHeaderHeight));
            ModEntry.themesHeaderTexture = PhoneVisuals.MakeColorTexture(Config.ThemesHeaderColor, new Vector2(screenSize.X, Config.AppHeaderHeight));
            ModEntry.themesHighlightTexture = PhoneVisuals.MakeColorTexture(Config.ThemesFooterHighlightColor, new Vector2(screenSize.X / 2, Config.AppHeaderHeight));
            ModEntry.answerTexture = PhoneVisuals.MakeColorTexture(Config.AnswerColor, new Vector2(screenSize.X / 2, Config.AppHeaderHeight));
            ModEntry.declineTexture = PhoneVisuals.MakeColorTexture(Config.DeclineColor, new Vector2(screenSize.X / 2, Config.AppHeaderHeight));
        }

        public static Vector2 GetPhonePosition()
        {
            int x = 0;
            int y = 0;
            switch (Config.PhonePosition.ToLower())
            {
                case "mid":
                    x = Game1.viewport.Width / 2 - ModEntry.phoneWidth / 2 + ModEntry.phoneOffsetX;
                    y = Game1.viewport.Height / 2 - ModEntry.phoneHeight / 2 + ModEntry.phoneOffsetY;
                    break;
                case "top-left":
                    x = ModEntry.phoneOffsetX;
                    y = ModEntry.phoneOffsetY;
                    break;
                case "top-right":
                    x = Game1.viewport.Width - ModEntry.phoneWidth + ModEntry.phoneOffsetX;
                    y = ModEntry.phoneOffsetY;
                    break;
                case "bottom-left":
                    x = ModEntry.phoneOffsetX;
                    y = Game1.viewport.Height - ModEntry.phoneHeight + ModEntry.phoneOffsetY;
                    break;
                case "bottom-right":
                    x = Game1.viewport.Width - ModEntry.phoneWidth + ModEntry.phoneOffsetX;
                    y = Game1.viewport.Height - ModEntry.phoneHeight + ModEntry.phoneOffsetY;
                    break;
            }

            return new Vector2(x, y);
        }

        internal static void CreateTones()
        {
            if (Config.PhoneRingTone.Contains("."))
            {

            }
            if (Config.NotificationTone.Contains("."))
            {

            }
        }

        public static Vector2 GetPhoneIconPosition()
        {
            int x = 0;
            int y = 0;
            switch (Config.PhoneIconPosition.ToLower())
            {
                case "mid":
                    x = Game1.viewport.Width / 2 - Config.PhoneIconWidth / 2 + Config.PhoneIconOffsetX;
                    y = Game1.viewport.Height / 2 - Config.PhoneIconHeight / 2 + Config.PhoneIconOffsetY;
                    break;
                case "top-left":
                    x = Config.PhoneIconOffsetX;
                    y = Config.PhoneIconOffsetY;
                    break;
                case "top-right":
                    x = Game1.viewport.Width - Config.PhoneIconWidth + Config.PhoneIconOffsetX;
                    y = Config.PhoneIconOffsetY;
                    break;
                case "bottom-left":
                    x = Config.PhoneIconOffsetX;
                    y = Game1.viewport.Height - Config.PhoneIconHeight + Config.PhoneIconOffsetY;
                    break;
                case "bottom-right":
                    x = Game1.viewport.Width - Config.PhoneIconWidth + Config.PhoneIconOffsetX;
                    y = Game1.viewport.Height - Config.PhoneIconHeight + Config.PhoneIconOffsetY;
                    break;
            }

            return new Vector2(x, y);
        }

        public static Vector2 GetScreenPosition()
        {
            
            return new Vector2(ModEntry.phonePosition.X + ModEntry.screenOffsetX, ModEntry.phonePosition.Y + ModEntry.screenOffsetY);
        }

        public static Vector2 GetScreenSize()
        {
            return new Vector2(ModEntry.screenWidth, ModEntry.screenHeight);
        }
        public static Vector2 GetScreenSize(bool rotated)
        {
            if (rotated)
            {
                return new Vector2(Config.ScreenRotatedWidth, Config.ScreenRotatedHeight);
            }
            else
            {
                return new Vector2(Config.ScreenWidth, Config.ScreenHeight);
            }
        }
        public static Vector2 GetAppPos(int i, bool initial = false)
        {
            float x = ModEntry.screenPosition.X + Config.IconMarginX + (( i % ModEntry.appColumns) * (Config.IconWidth + Config.IconMarginX));
            float y = ModEntry.screenPosition.Y + Config.IconMarginY + ((i / ModEntry.appColumns) * (Config.IconHeight + Config.IconMarginY)) + ModEntry.yOffset;

            if (ModEntry.movingAppIcon && !initial)
            {
                if(ModEntry.clickingApp == i)
                    return new Vector2(x + ModEntry.movingAppIconOffset.X, y + ModEntry.movingAppIconOffset.Y);
                else if (ModEntry.switchingApp == i)
                {
                    x = ModEntry.screenPosition.X + Config.IconMarginX + ((ModEntry.clickingApp % ModEntry.appColumns) * (Config.IconWidth + Config.IconMarginX));
                    y = ModEntry.screenPosition.Y + Config.IconMarginY + ((ModEntry.clickingApp / ModEntry.appColumns) * (Config.IconHeight + Config.IconMarginY)) + ModEntry.yOffset;
                }
            }

            return new Vector2(x, y);
        }
        
        public static Point GetAppGridPos(int i)
        {
            int x = i % ModEntry.appColumns;
            int y = i / ModEntry.appColumns;

            return new Point(x, y);
        }
        public static void GetArrowPositions()
        {
            ModEntry.upArrowPosition = new Vector2(ModEntry.screenPosition.X + ModEntry.screenWidth - Config.ContactArrowWidth, ModEntry.screenPosition.Y + Config.AppHeaderHeight);
            ModEntry.downArrowPosition = new Vector2(ModEntry.screenPosition.X + ModEntry.screenWidth - Config.ContactArrowWidth, ModEntry.screenPosition.Y + ModEntry.screenHeight - Config.ContactArrowHeight);
        }

        public static bool IsOnScreen(int i, int top)
        {
            return i >= top * ModEntry.appColumns && i < ModEntry.appRows * ModEntry.appColumns - top * ModEntry.appColumns;
        }
    }
}
