using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;

namespace LocationMap
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;


        private static ClickableTextureComponent upperRightCloseButton;
        private static ClickableTextureComponent northButton;
        private static ClickableTextureComponent westButton;
        private static ClickableTextureComponent eastButton;
        private static ClickableTextureComponent southButton;
        public static Texture2D renderTarget;
        public static bool playerTileChanged;

        private static bool showingMap;
        private static Rectangle mapRect;
        public static Point mapOffset;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            helper.Events.Display.RenderedActiveMenu += Display_RenderedActiveMenu;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.Display.WindowResized += Display_WindowResized;


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (Game1.activeClickableMenu is GameMenu menu && menu.GetCurrentPage() is MapPage && Config.MapKey.JustPressed())
            {
                showingMap = !showingMap;
                Game1.playSound(showingMap ? "bigSelect" : "bigDeSelect");
                if (showingMap && renderTarget == null)
                {
                    TakeMapScreenshot(Game1.currentLocation, Config.MapScale);
                }
                foreach (var s in e.Pressed)
                {
                    SHelper.Input.Suppress(s);
                }
            }
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            renderTarget = null;
        }

        private void Display_WindowResized(object sender, WindowResizedEventArgs e)
        {
            renderTarget = null;
        }

        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            CheckForMapPage();
        }


        private void Display_RenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || Game1.activeClickableMenu is not GameMenu menu || menu.GetCurrentPage() is not MapPage)
                return;
            DrawMap(e);
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;

            if (showingMap && e.Button == SButton.MouseLeft && renderTarget != null)
            {
                var mp = Game1.getMousePosition(true);
                if (upperRightCloseButton.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true)))
                {
                    Game1.playSound("bigDeSelect");
                    showingMap = false;
                }
                else if (Config.AllowTeleport && mapRect.Contains(mp))
                {
                    var l = Game1.currentLocation;
                    int tileSize = 16;
                    var scale = Game1.options.uiScale;


                    int width = Math.Min((Game1.uiViewport.Width - tileSize) / tileSize, l.map.Layers[0].LayerWidth) * 64;
                    int height = Math.Min((Game1.uiViewport.Height - tileSize) / tileSize, l.map.Layers[0].LayerHeight) * 64;

                    int start_x = Math.Clamp((int)Game1.player.Position.X - width / 2, 0, l.map.Layers[0].LayerWidth * 64 - width);
                    int start_y = Math.Clamp((int)Game1.player.Position.Y - height / 2, 0, l.map.Layers[0].LayerHeight * 64 - height);
                    float x = (mp.X - mapRect.X) * 64f / tileSize + start_x;
                    float y = (mp.Y - mapRect.Y) * 64f / tileSize + start_y;
                    Game1.player.Position = new Vector2(x, y);
                    Game1.activeClickableMenu.exitThisMenu(true);
                }
                SHelper.Input.Suppress(e.Button);
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {   
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.ShowByDefault.Name"),
                    getValue: () => Config.ShowByDefault,
                    setValue: value => Config.ShowByDefault = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.AllowTeleport.Name"),
                    getValue: () => Config.AllowTeleport,
                    setValue: value => Config.AllowTeleport = value
                );

                configMenu.AddKeybindList(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM.MapKey.Name"),
                    getValue: () => Config.MapKey,
                    setValue: value => Config.MapKey = value
                );
            }
        }
    }
}