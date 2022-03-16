using Force.DeepCloner;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace AdvancedMenuPositioning
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        private static Point lastMousePosition;
        private static IClickableMenu currentlyDragging;

        private static List<ClickableComponent> adjustedComponents = new List<ClickableComponent>();
        private static List<IClickableMenu> adjustedMenus = new List<IClickableMenu>();
        private static List<IClickableMenu> detachedMenus = new List<IClickableMenu>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;

        }

        private void Input_MouseWheelScrolled(object sender, StardewModdingAPI.Events.MouseWheelScrolledEventArgs e)
        {
            foreach (var m in detachedMenus)
            {
                m.receiveScrollWheelAction(e.Delta);
            }
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (detachedMenus.Any())
            {
                var back = Game1.options.showMenuBackground;
                Game1.options.showMenuBackground = true;
                foreach (var m in detachedMenus)
                {
                    var f = AccessTools.Field(m.GetType(), "drawBG");
                    if (f != null)
                        f.SetValue(m, false);
                    m.draw(e.SpriteBatch);
                }
                Game1.options.showMenuBackground = back;
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (Game1.activeClickableMenu != null && Helper.Input.IsDown(Config.DetachModKey) && e.Button == Config.DetachKey && new Rectangle(Game1.activeClickableMenu.xPositionOnScreen, Game1.activeClickableMenu.yPositionOnScreen, Game1.activeClickableMenu.width, Game1.activeClickableMenu.height).Contains(Game1.getMouseX(), Game1.getMouseY()))
            {
                detachedMenus.Add(Game1.activeClickableMenu);
                Game1.activeClickableMenu = null;
                Helper.Input.Suppress(e.Button);
                Game1.playSound("bigDeSelect");
                return;
            }
            else if(detachedMenus.Count > 0)
            {
                if (Helper.Input.IsDown(Config.CloseModKey) && e.Button == Config.CloseKey)
                {
                    for (int i = 0; i < detachedMenus.Count; i++)
                    {
                        if(detachedMenus[i].isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
                        {
                            detachedMenus.RemoveAt(i);
                            Helper.Input.Suppress(e.Button);
                            Game1.playSound("bigDeSelect");
                            return;
                        }
                    }
                }
                if (Helper.Input.IsDown(Config.DetachModKey) && e.Button == Config.DetachKey && Game1.activeClickableMenu == null)
                {
                    for (int i = 0; i < detachedMenus.Count; i++)
                    {
                        if(detachedMenus[i].isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
                        {
                            Game1.activeClickableMenu = detachedMenus[i];
                            detachedMenus.RemoveAt(i);
                            Helper.Input.Suppress(e.Button);
                            Game1.playSound("bigSelect");
                            return;
                        }
                    }
                }
                else if (e.Button == SButton.MouseLeft)
                {
                    for (int i = 0; i < detachedMenus.Count; i++)
                    {
                        bool toBreak = detachedMenus[i].isWithinBounds(Game1.getMouseX(), Game1.getMouseY());

                        var menu = Game1.activeClickableMenu;
                        Game1.activeClickableMenu = detachedMenus[i];
                        Game1.activeClickableMenu.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY());
                        if (Game1.activeClickableMenu != null)
                        {
                            var d = new Point(detachedMenus[i].xPositionOnScreen - Game1.activeClickableMenu.xPositionOnScreen, detachedMenus[i].yPositionOnScreen - Game1.activeClickableMenu.yPositionOnScreen);
                            if (d != Point.Zero)
                            {
                                detachedMenus[i] = Game1.activeClickableMenu;
                                AdjustMenu(detachedMenus[i], d, true);
                                Game1.activeClickableMenu = menu;
                            }

                        }
                        else
                            detachedMenus.RemoveAt(i);
                        Game1.activeClickableMenu = menu;
                        if (toBreak)
                        {
                            Helper.Input.Suppress(e.Button);
                            return;
                        }

                    }
                }
                else if (e.Button == SButton.MouseRight)
                {
                    for (int i = 0; i < detachedMenus.Count; i++)
                    {
                        bool toBreak = detachedMenus[i].isWithinBounds(Game1.getMouseX(), Game1.getMouseY());

                        var menu = Game1.activeClickableMenu;
                        Game1.activeClickableMenu = detachedMenus[i];
                        Game1.activeClickableMenu.receiveRightClick(Game1.getMouseX(), Game1.getMouseY());
                        if (Game1.activeClickableMenu != null)
                        {
                            var d = new Point(detachedMenus[i].xPositionOnScreen - Game1.activeClickableMenu.xPositionOnScreen, detachedMenus[i].yPositionOnScreen - Game1.activeClickableMenu.yPositionOnScreen);
                            if(d != Point.Zero)
                            {
                                detachedMenus[i] = Game1.activeClickableMenu;
                                AdjustMenu(detachedMenus[i], d, true);
                                Game1.activeClickableMenu = menu;
                            }
                        }
                        else
                            detachedMenus.RemoveAt(i);
                        Game1.activeClickableMenu = menu;
                        if (toBreak)
                        {
                            Helper.Input.Suppress(e.Button);
                            return;
                        }
                    }
                }
            }
        }

        private void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            if(Config.EnableMod && Helper.Input.IsDown(Config.MoveModKey) && (Helper.Input.IsDown(Config.MoveKey) || Helper.Input.IsSuppressed(Config.MoveKey)))
            {
                if(Game1.activeClickableMenu != null)
                {
                    if (currentlyDragging == Game1.activeClickableMenu || Game1.activeClickableMenu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        currentlyDragging = Game1.activeClickableMenu;
                        AdjustMenu(Game1.activeClickableMenu, Game1.getMousePosition() - lastMousePosition, true);
                        Helper.Input.Suppress(Config.MoveKey);
                        if (Game1.activeClickableMenu is ItemGrabMenu && Helper.ModRegistry.IsLoaded("Pathoschild.ChestsAnywhere"))
                        {
                            Game1.activeClickableMenu = Game1.activeClickableMenu.ShallowClone();
                        }
                        goto next;
                    }
                }
                foreach (var menu in Game1.onScreenMenus)
                {
                    if (menu is null)
                        continue;
                    if (currentlyDragging == menu || menu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        currentlyDragging = menu;

                        AdjustMenu(menu, Game1.getMousePosition() - lastMousePosition, true);
                        Helper.Input.Suppress(Config.MoveKey);
                        goto next;
                    }
                }
                foreach (var menu in detachedMenus)
                {
                    if (menu is null)
                        continue;
                    if (currentlyDragging == menu || menu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        currentlyDragging = menu;

                        AdjustMenu(menu, Game1.getMousePosition() - lastMousePosition, true);
                        Helper.Input.Suppress(Config.MoveKey);
                        goto next;
                    }
                }
            }
            currentlyDragging = null;
        next:
            lastMousePosition = Game1.getMousePosition();
            foreach (var menu in detachedMenus)
            {
                if (menu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
                {
                    menu.performHoverAction(Game1.getMouseX(), Game1.getMouseY());
                }
            }
        }


        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Move Key",
                getValue: () => Config.MoveKey,
                setValue: value => Config.MoveKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Detach Key",
                getValue: () => Config.DetachKey,
                setValue: value => Config.DetachKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Close Key",
                getValue: () => Config.CloseKey,
                setValue: value => Config.CloseKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Move Mod Key",
                getValue: () => Config.MoveModKey,
                setValue: value => Config.MoveModKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "DetachModKey Key",
                getValue: () => Config.DetachModKey,
                setValue: value => Config.DetachModKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "CloseModKey Key",
                getValue: () => Config.CloseModKey,
                setValue: value => Config.CloseModKey = value
            );
        }
   }
}