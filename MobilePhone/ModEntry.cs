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

        public static MobilePhoneApi api;
        public static int appColumns;
        public static int appRows;
        public static int gridWidth;
        public static int gridHeight;
        public static int themeGridWidth;
        public static int themeGridHeight;

        public static Texture2D phoneBookTexture;
        public static Texture2D phoneBookHeaderTexture;

        public static Dictionary<string, MobileApp> apps = new Dictionary<string, MobileApp>();
        public static List<string> appOrder;
        public static string runningApp;
        public static int listHeight;

        public static bool clicking;
        public static bool draggingPhone;
        public static bool draggingIcons;
        public static bool clickingPhoneIcon;
        public static bool draggingPhoneIcon;
        public static bool movingAppIcon;

        public static Point lastMousePosition;
        public static Point movingAppIconOffset;

        public static float yOffset;

        public static int clickingApp = -1;
        public static int switchingApp = -1;

        public static int clickingTicks;
        public static int ticksSinceMoved;
        internal static Texture2D themesHighlightTexture;
        internal static Texture2D themesHeaderTexture;

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
            ThemeApp.Initialize(Helper, Monitor, Config);
            PhoneVisuals.Initialize(Helper, Monitor, Config);
            PhoneInput.Initialize(Helper, Monitor, Config);
            PhoneGameLoop.Initialize(Helper, Monitor, Config);
            PhoneUtils.Initialize(Helper, Monitor, Config);

            Helper.Events.Input.ButtonPressed += PhoneInput.Input_ButtonPressed;
            Helper.Events.Input.ButtonReleased += PhoneInput.Input_ButtonReleased;
            Helper.Events.GameLoop.SaveLoaded += PhoneGameLoop.GameLoop_SaveLoaded;
            Helper.Events.GameLoop.GameLaunched += PhoneGameLoop.GameLoop_GameLaunched;
            Helper.Events.Display.WindowResized += PhoneVisuals.Display_WindowResized;
        }

        public override object GetApi()
        {
            return new MobilePhoneApi();
        }
    }
}
