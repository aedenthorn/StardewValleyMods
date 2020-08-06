using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace SocialNetwork
{
    public class ModEntry : Mod
    {
        private static ModEntry context;
        public static ModConfig Config;
        public static Random myRand;

        public static IMobilePhoneApi api;
        public static Texture2D backgroundTexture;
        public static Texture2D postBackgroundTexture;
        public static List<SocialPost> postList = new List<SocialPost>();
        public static float yOffset;
        public static bool dragging;
        public static Point lastMousePosition;
        public static Dictionary<string, Dictionary<string, string>> todaysPosts = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;
            Utils.Helper = Helper;
            Utils.Monitor = Monitor;
            Utils.Config = Config;

            HelperEvents.Helper = Helper;
            HelperEvents.Monitor = Monitor;
            HelperEvents.Config = Config;


            myRand = new Random(Guid.NewGuid().GetHashCode());
            Helper.Events.GameLoop.GameLaunched += HelperEvents.GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += HelperEvents.GameLoop_DayStarted;
            Helper.Events.GameLoop.TimeChanged += HelperEvents.GameLoop_TimeChanged;
        }
        public static void OpenFeed()
        {
            api.SetAppRunning(true);
            api.SetRunningApp(context.Helper.ModRegistry.ModID);
            Game1.activeClickableMenu = new SocialNetworkMenu();
            context.Helper.Events.Display.RenderedWorld += HelperEvents.Display_RenderedWorld;
            context.Helper.Events.Input.ButtonPressed += HelperEvents.Input_ButtonPressed;
            context.Helper.Events.Input.ButtonReleased += HelperEvents.Input_ButtonReleased;
        }
    }
}
