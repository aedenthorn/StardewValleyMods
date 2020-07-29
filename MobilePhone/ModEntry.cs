using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.SDKs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MobilePhone
{
    public class ModEntry : Mod
    {
        public static ModEntry context;

        public static ModConfig Config;
        public static IModHelper SHelper;

        public static bool phoneOpen;
        public static bool phoneRotated;
        public static bool appRunning;
        public static bool phoneAppRunning;

        public static Texture2D phoneTexture;
        public static Texture2D backgroundTexture;
        public static Texture2D phoneRotatedTexture;
        public static Texture2D backgroundRotatedTexture;
        public static Texture2D upArrowTexture;
        public static Texture2D downArrowTexture;
        public static Texture2D iconTexture;

        public static int phoneWidth;
        public static int phoneHeight;
        public static int screenWidth;
        public static int screenHeight;
        public static int phoneOffsetX;
        public static int phoneOffsetY;
        public static int screenOffsetX;
        public static int screenOffsetY;
        public static Rectangle screenRect;
        public static Rectangle phoneRect;
        public static Vector2 phonePosition;
        public static Vector2 phoneIconPosition;
        public static Vector2 screenPosition;
        public static Vector2 upArrowPosition;
        public static Vector2 downArrowPosition;

        private static MobilePhoneApi api;
        public static int appColumns;
        public static int appRows;
        public static int gridWidth;
        public static int gridHeight;
        public static int topRow;

        public static Dictionary<string, MobileApp> apps = new Dictionary<string, MobileApp>();
        public static Texture2D phoneBookTexture;
        public static bool draggingPhone;
        public static Point lastMousePosition;
        public static int ticksSinceMoved = 0;
        public static Point lastPos = new Point();
        public static bool clickingPhoneIcon;
        public static bool draggingPhoneIcon;

        public static event EventHandler OnScreenRotated;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = helper.ReadConfig<ModConfig>();
            SHelper = helper;
            if (!Config.EnableMod)
                return;
            api = (MobilePhoneApi)GetApi();

            MobilePhoneApp.Initialize(Helper, Monitor, Config);

            CreatePhoneTextures();
            RefreshPhoneLayout();

            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Input.ButtonReleased += Input_ButtonReleased;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Display.WindowResized += Display_WindowResized;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            return;
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                try
                {
                    Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                    MobileAppJSON json = contentPack.ReadJsonFile<MobileAppJSON>("content.json");
                    Texture2D icon = contentPack.LoadAsset<Texture2D>(json.iconPath);
                    apps.Add(json.id, new MobileApp(json.name, json.dllName, json.className, json.methodName, json.keyPress, icon));
                    Monitor.Log($"Added app {json.name} from {contentPack.DirectoryPath}");
                }
                catch (Exception ex)
                {
                    Monitor.Log($"error reading content.json file in content pack {contentPack.Manifest.Name}.\r\n{ex}", LogLevel.Error);
                }
            }
        }

        private void Display_WindowResized(object sender, StardewModdingAPI.Events.WindowResizedEventArgs e)
        {
            RefreshPhoneLayout();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            Monitor.Log($"total apps: {apps.Count}");
            RefreshPhoneLayout();
            SHelper.Events.Display.RenderedWorld += Display_RenderedWorld;
        }

        public override object GetApi()
        {
            return new MobilePhoneApi();
        }
        private void CreatePhoneTextures()
        {
            phoneTexture = Helper.Content.Load<Texture2D>(Path.Combine("assets",Config.PhoneTexturePath));
            backgroundTexture = Helper.Content.Load<Texture2D>(Path.Combine("assets",Config.BackgroundTexturePath));
            phoneRotatedTexture = Helper.Content.Load<Texture2D>(Path.Combine("assets",Config.PhoneRotatedTexturePath));
            backgroundRotatedTexture = Helper.Content.Load<Texture2D>(Path.Combine("assets",Config.BackgroundRotatedTexturePath));
            upArrowTexture = Helper.Content.Load<Texture2D>(Path.Combine("assets",Config.UpArrowTexturePath));
            downArrowTexture = Helper.Content.Load<Texture2D>(Path.Combine("assets",Config.DownArrowTexturePath));
            if (Config.ShowPhoneIcon)
            {
                iconTexture = Helper.Content.Load<Texture2D>(Path.Combine("assets", Config.iconTexturePath));
            }
        }

        private static void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {

            Point mousePos = Game1.getMousePosition();

            if (!phoneOpen)
            {
                appRunning = false;
                if (Game1.activeClickableMenu is MobilePhoneMenu)
                {
                    Game1.activeClickableMenu = null;
                }
                if ((Game1.displayHUD || Game1.eventUp) && Game1.currentBillboard == 0 && Game1.gameMode == 3 && !Game1.freezeControls && !Game1.panMode && !Game1.HostPaused && !Game1.game1.takingMapScreenshot)
                {
                    if (clickingPhoneIcon)
                    {
                        if(SHelper.Input.IsDown(SButton.MouseLeft) && lastMousePosition != mousePos)
                        {
                            draggingPhoneIcon = true;
                            Config.PhoneIconOffsetX += mousePos.X - lastMousePosition.X;
                            Config.PhoneIconOffsetY += mousePos.Y - lastMousePosition.Y;
                            phoneIconPosition = GetPhoneIconPosition();
                            lastMousePosition = mousePos;
                        }
                    }
                    e.SpriteBatch.Draw(iconTexture, phoneIconPosition, Color.White);
                }

                return;
            }

            if (Game1.game1.takingMapScreenshot)
                return;

            if (!appRunning)
            {
                if(Game1.activeClickableMenu == null)
                    Game1.activeClickableMenu = new MobilePhoneMenu();
            }
            else
            {
                if(Game1.activeClickableMenu is MobilePhoneMenu)
                {
                    Game1.activeClickableMenu = null;
                }
            }

            if (draggingPhone)
            {
                if(mousePos != lastMousePosition)
                {
                    int x = mousePos.X - lastMousePosition.X;
                    int y = mousePos.Y - lastMousePosition.Y; 
                    if (phoneRotated)
                    {
                        Config.PhoneRotatedOffsetX += x;
                        Config.PhoneRotatedOffsetY += y;
                    }
                    else
                    {
                        Config.PhoneOffsetX += x;
                        Config.PhoneOffsetY += y;
                    }
                    RefreshPhoneLayout();
                    lastMousePosition = mousePos;
                }
            }

            e.SpriteBatch.Draw(phoneRotated ? backgroundRotatedTexture : backgroundTexture, phonePosition, Color.White);
            e.SpriteBatch.Draw(phoneRotated ? phoneRotatedTexture : phoneTexture, phonePosition, Color.White);

            if (appRunning)
                return;

            List<string> keys = apps.Keys.ToList();
            string appHover = null;
            bool hover = false;
            if (mousePos == lastPos)
            {
                hover = true;
            }
            else
            {
                ticksSinceMoved = 0;
            }

            for (int i = keys.Count - 1; i >= 0; i--)
            {
                if (!IsOnScreen(i, topRow))
                    continue;

                MobileApp app = apps[keys[i]];
                if (app.icon == null)
                {
                    context.Monitor.Log($"no icon for app {app.name}, removing from app list.", LogLevel.Error);
                    apps.Remove(keys[i]);
                    continue;
                }
                
                Vector2 appPos = context.GetAppPos(i);
                e.SpriteBatch.Draw(app.icon, appPos, Color.White);

                Rectangle rect = new Rectangle((int)appPos.X, (int)appPos.Y, Config.IconWidth, Config.IconHeight);
                if (hover && rect.Contains(mousePos))
                {
                    ticksSinceMoved++;
                    if (ticksSinceMoved > Config.ToolTipDelayTicks)
                        appHover = app.name;
                }
            }
            if (appHover != null)
                Utility.drawTextWithShadow(e.SpriteBatch, appHover, Game1.dialogueFont, new Vector2(mousePos.X, mousePos.Y) - Game1.dialogueFont.MeasureString(appHover), Color.White, 1f, -1f, -1, -1, 1f, 3);
            lastPos = mousePos;
        }

        private void Input_ButtonReleased(object sender, StardewModdingAPI.Events.ButtonReleasedEventArgs e)
        {
            if (e.Button == SButton.MouseLeft)
            {
                if (draggingPhone)
                {
                    Helper.WriteConfig(Config);
                    draggingPhone = false;
                }
                else if (clickingPhoneIcon)
                {
                    if (!draggingPhoneIcon)
                    {
                        TogglePhone(true);
                    }
                    else
                    {
                        Helper.WriteConfig(Config);
                    }
                    if(Game1.activeClickableMenu is DraggingPhoneIconMenu)
                        Game1.activeClickableMenu = null;
                    clickingPhoneIcon = false;
                    draggingPhoneIcon = false;
                }
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.Button == Config.OpenPhoneKey || (phoneOpen && e.Button == SButton.Escape))
            {
                TogglePhone();
                if (phoneOpen && e.Button == SButton.Escape)
                    Helper.Input.Suppress(SButton.Escape);
                return;
            }
            if(e.Button == Config.RotatePhoneKey)
            {
                RotatePhone();
                return;
            }

            Point mousePos = Game1.getMousePosition();

            if (!phoneOpen)
            {
                if (e.Button == SButton.MouseLeft && Game1.displayHUD && Config.ShowPhoneIcon && new Rectangle((int)phoneIconPosition.X, (int)phoneIconPosition.Y, Config.PhoneIconWidth, Config.PhoneIconHeight).Contains(mousePos))
                {
                    clickingPhoneIcon = true;
                    draggingPhoneIcon = false;
                    lastMousePosition = mousePos;
                    Game1.activeClickableMenu = new DraggingPhoneIconMenu();
                }
                return;
            }

            if (e.Button == SButton.MouseLeft && (appRunning || Game1.activeClickableMenu is MobilePhoneMenu))
            {
                if (!appRunning && !phoneRect.Contains(mousePos))
                {
                    Helper.Input.Suppress(SButton.MouseLeft);
                    TogglePhone();
                    return;
                }

                if (phoneRect.Contains(mousePos) && !screenRect.Contains(mousePos))
                {
                    draggingPhone = true;
                    lastMousePosition = mousePos;
                    return;
                }
            }

            if (!appRunning && Game1.activeClickableMenu is MobilePhoneMenu)
            {
                if (e.Button == SButton.MouseLeft)
                {
                    Monitor.Log($"pressing mouse key in phone");
                    Helper.Input.Suppress(SButton.MouseLeft);

                    string[] keys = apps.Keys.ToArray();
                    for (int i = 0; i < keys.Length; i++)
                    {

                        if (!IsOnScreen(i, topRow))
                            continue;

                        Vector2 pos = GetAppPos(i);
                        Rectangle r = new Rectangle((int) pos.X, (int) pos.Y, Config.IconWidth, Config.IconHeight);
                        if (r.Contains(Game1.getMousePosition()))
                        {
                            Monitor.Log($"rect: {r.X},{r.Y},{r.Width},{r.Height} pos {Game1.getMousePosition()} running app {keys[i]}");
                            if(apps[keys[i]].keyPress != null)
                            {
                                Monitor.Log($"pressing key {apps[keys[i]].keyPress}");
                                PressKey(apps[keys[i]]);
                            }
                            else
                            {
                                Monitor.Log($"starting app {apps[keys[i]].name}");
                                apps[keys[i]].action.Invoke();
                            }
                            return;
                        }
                    }
                }
            }
        }

        private void PressKey(MobileApp app)
        {
            Assembly a = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(n => n.GetName().Name == app.dllName);
            if(a == null)
            {
                Monitor.Log($"Couldn't find assembly named {app.dllName}. Sample: {AppDomain.CurrentDomain.GetAssemblies()[0].GetName().Name}");
                return;
            }
           // a.GetType(app.className).GetMethod(app.methodName, BindingFlags.NonPublic | BindingFlags.Instance).Invoke()
        }

        private void RotatePhone()
        {
            phoneRotated = !phoneRotated;
            RefreshPhoneLayout();
            Game1.playSound("dwop");
        }

        public static void TogglePhone(bool value)
        {
            phoneOpen = value;
            Game1.playSound("dwop");
            if (!value)
            {
                context.Monitor.Log($"Closing phone");
                if (Game1.activeClickableMenu is MobilePhoneMenu)
                    Game1.activeClickableMenu = null;
                appRunning = false;
            }
            else
            {
                context.Monitor.Log($"Opening phone");
                Game1.activeClickableMenu = new MobilePhoneMenu();
            }
        }
        public static void TogglePhone()
        {
            if(phoneOpen && appRunning)
            {
                Game1.playSound("dwop");
                appRunning = false;
                Game1.activeClickableMenu = new MobilePhoneMenu();
                return;
            }
            TogglePhone(!phoneOpen);
        }

        public static void ToggleApp(bool value)
        {
            appRunning = value;
            if (!phoneOpen)
            {
                Game1.activeClickableMenu = null;
            }
            else if(phoneOpen)
            {
                Game1.activeClickableMenu = new MobilePhoneMenu();
            }
        }
        private static void RefreshPhoneLayout()
        {
            if (phoneRotated)
            {
                phoneWidth = Config.PhoneRotatedWidth;
                phoneHeight = Config.PhoneRotatedHeight;
                screenWidth = Config.ScreenRotatedWidth;
                screenHeight = Config.ScreenRotatedHeight;
                phoneOffsetX = Config.PhoneRotatedOffsetX;
                phoneOffsetY = Config.PhoneRotatedOffsetY;
                screenOffsetX = Config.ScreenRotatedOffsetX;
                screenOffsetY = Config.ScreenRotatedOffsetY;
            }
            else
            {
                phoneWidth = Config.PhoneWidth;
                phoneHeight = Config.PhoneHeight;
                screenWidth = Config.ScreenWidth;
                screenHeight = Config.ScreenHeight;
                phoneOffsetX = Config.PhoneOffsetX;
                phoneOffsetY = Config.PhoneOffsetY;
                screenOffsetX = Config.ScreenOffsetX;
                screenOffsetY = Config.ScreenOffsetY;
            }
            phonePosition = GetPhonePosition();
            screenPosition = GetScreenPosition();
            phoneIconPosition = GetPhoneIconPosition();
            screenRect = new Rectangle((int)screenPosition.X, (int)screenPosition.Y, (int)screenWidth, (int)screenHeight);
            phoneRect = new Rectangle((int)phonePosition.X, (int)phonePosition.Y, phoneWidth, phoneHeight);
            GetArrowPositions();
            appColumns = (screenWidth - Config.IconMarginX) / (Config.IconWidth + Config.IconMarginX);
            appRows = (screenHeight - Config.IconMarginY) / (Config.IconHeight + Config.IconMarginY);
            gridWidth = (screenWidth - Config.ContactMarginX) / (Config.ContactWidth + Config.ContactMarginX);
            gridHeight = (screenHeight - Config.ContactMarginY) / (Config.ContactHeight + Config.ContactMarginY); 
            phoneBookTexture = MobilePhoneApp.MakeBackground();
        }


        public static Vector2 GetPhonePosition()
        {
            int x = 0;
            int y = 0;
            switch (Config.PhonePosition.ToLower())
            {
                case "mid":
                    x = Game1.viewport.Width / 2 - phoneWidth / 2 + phoneOffsetX;
                    y = Game1.viewport.Height / 2 - phoneHeight / 2 + phoneOffsetY;
                    break;
                case "top-left":
                    x = phoneOffsetX;
                    y = phoneOffsetY;
                    break;
                case "top-right":
                    x = Game1.viewport.Width - phoneWidth + phoneOffsetX;
                    y = phoneOffsetY;
                    break;
                case "bottom-left":
                    x = phoneOffsetX;
                    y = Game1.viewport.Height - phoneHeight + phoneOffsetY;
                    break;
                case "bottom-right":
                    x = Game1.viewport.Width - phoneWidth + phoneOffsetX;
                    y = Game1.viewport.Height - phoneHeight + phoneOffsetY;
                    break;
            }

            return new Vector2(x, y);
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
            
            return new Vector2(phonePosition.X + screenOffsetX, phonePosition.Y + screenOffsetY);
        }

        public static Vector2 GetScreenSize()
        {
            return new Vector2(screenWidth, screenHeight);
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
        private Vector2 GetAppPos(int i)
        {
            i -= topRow * appColumns;
            float x = screenPosition.X + Config.IconMarginX + (( i % appColumns) * (Config.IconWidth + Config.IconMarginX));
            float y = screenPosition.Y + Config.IconMarginY + ((i / appColumns) * (Config.IconHeight + Config.IconMarginY));

            return new Vector2(x, y);
        }
        private static void GetArrowPositions()
        {
            upArrowPosition = new Vector2(screenPosition.X + screenWidth - Config.ContactArrowWidth, screenPosition.Y);
            downArrowPosition = new Vector2(screenPosition.X + screenWidth - Config.ContactArrowWidth, screenPosition.Y + screenHeight - Config.ContactArrowHeight);
        }

        public static bool IsOnScreen(int i, int top)
        {
            return i >= top * appColumns && i < appRows * appColumns - top * appColumns;
        }
    }
}
