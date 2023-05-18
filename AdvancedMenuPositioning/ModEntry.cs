using Force.DeepCloner;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Reflection;
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
        private static int RightClickLimiter;

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
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking_MoveMenus;
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking_RightClick;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;

        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            adjustedComponents.Clear();
            adjustedMenus.Clear();
            detachedMenus.Clear();
        }

        private void Input_MouseWheelScrolled(object sender, StardewModdingAPI.Events.MouseWheelScrolledEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            for (int i = detachedMenus.Count - 1; i >= 0; i--)
            {
                detachedMenus[i].receiveScrollWheelAction(e.Delta);
            }
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
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
            if (!Context.IsWorldReady || !Config.EnableMod)
                return;
            if (Game1.activeClickableMenu != null && isKeybindPressed(Config.DetachKeys.Keybinds[0].Buttons) && new Rectangle(Game1.activeClickableMenu.xPositionOnScreen, Game1.activeClickableMenu.yPositionOnScreen, Game1.activeClickableMenu.width, Game1.activeClickableMenu.height).Contains(Game1.getMouseX(), Game1.getMouseY()))
            {
                detachedMenus.Add(Game1.activeClickableMenu);
                Game1.activeClickableMenu = null;
                Helper.Input.Suppress(e.Button);
                Game1.playSound("bigDeSelect");
                return;
            }
            else if(detachedMenus.Count > 0)
            {
                if (isKeybindPressed(Config.MoveKeys.Keybinds[0].Buttons))
                    return;
                if (isKeybindPressed(Config.CloseKeys.Keybinds[0].Buttons))
                {
                    for (int i = detachedMenus.Count - 1; i >= 0; i--)
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
                if (isKeybindPressed(Config.DetachKeys.Keybinds[0].Buttons) && Game1.activeClickableMenu == null)
                {
                    for (int i = detachedMenus.Count - 1; i >= 0; i--)
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
                    if (Game1.activeClickableMenu is not null && Game1.activeClickableMenu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
                        return;
                    for (int i = detachedMenus.Count - 1; i >= 0; i--)
                    {
                        bool toBreak = detachedMenus[i].isWithinBounds(Game1.getMouseX(), Game1.getMouseY());

                        var menu = Game1.activeClickableMenu;
                        Game1.activeClickableMenu = detachedMenus[i];
                        Game1.activeClickableMenu.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY());
                        ItemGrabMenu menuAsItemGrabMenu = Game1.activeClickableMenu as ItemGrabMenu;
                        if (menuAsItemGrabMenu is not null)
                            Game1.activeClickableMenu = (IClickableMenu) new ItemGrabMenu(menuAsItemGrabMenu.ItemsToGrabMenu.actualInventory, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), (ItemGrabMenu.behaviorOnItemSelect)typeof(ItemGrabMenu).GetField("behaviorFunction", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menuAsItemGrabMenu), (string) null, menuAsItemGrabMenu.behaviorOnItemGrab, canBeExitedWithKey: true, showOrganizeButton: true, source: menuAsItemGrabMenu.source, sourceItem: (Item)typeof(ItemGrabMenu).GetField("sourceItem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menuAsItemGrabMenu), whichSpecialButton: menuAsItemGrabMenu.whichSpecialButton, context: menuAsItemGrabMenu.context).setEssential((bool)typeof(ItemGrabMenu).GetField("essential", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menuAsItemGrabMenu));
                        if (Game1.activeClickableMenu != null)
                        {
                            var d = new Point(detachedMenus[i].xPositionOnScreen - Game1.activeClickableMenu.xPositionOnScreen, detachedMenus[i].yPositionOnScreen - Game1.activeClickableMenu.yPositionOnScreen);
                            detachedMenus[i] = Game1.activeClickableMenu;
                            AdjustMenu(detachedMenus[i], d, true);
                            Game1.activeClickableMenu = menu;
                            if (toBreak)
                            {
                                detachedMenus.Add(detachedMenus[i]);
                                detachedMenus.RemoveAt(i);
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
                    if (Game1.activeClickableMenu is not null && Game1.activeClickableMenu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
                        return;
                    for (int i = detachedMenus.Count - 1; i >= 0; i--)
                    {
                        bool toBreak = detachedMenus[i].isWithinBounds(Game1.getMouseX(), Game1.getMouseY());

                        var menu = Game1.activeClickableMenu;
                        Game1.activeClickableMenu = detachedMenus[i];
                        Game1.activeClickableMenu.receiveRightClick(Game1.getMouseX(), Game1.getMouseY());
                        if (Game1.activeClickableMenu != null)
                        {
                            var d = new Point(detachedMenus[i].xPositionOnScreen - Game1.activeClickableMenu.xPositionOnScreen, detachedMenus[i].yPositionOnScreen - Game1.activeClickableMenu.yPositionOnScreen);
                            detachedMenus[i] = Game1.activeClickableMenu;
                            AdjustMenu(detachedMenus[i], d, true);
                            Game1.activeClickableMenu = menu;
                            if (toBreak)
                            {
                                detachedMenus.Add(detachedMenus[i]);
                                detachedMenus.RemoveAt(i);
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

        private void GameLoop_UpdateTicking_RightClick(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || !Config.EnableMod)
                return;
            if(detachedMenus.Count > 0)
            {
                if (Helper.Input.IsDown(SButton.MouseRight) || Helper.Input.IsSuppressed(SButton.MouseRight))
                {
                    if (Game1.activeClickableMenu is not null && Game1.activeClickableMenu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
                        return;
                    if (RightClickLimiter < 30)
                    {
                        RightClickLimiter++;
                        return ;
                    }
                    else
                    {
                        RightClickLimiter -= 3;
                        for (int i = detachedMenus.Count - 1; i >= 0; i--)
                        {
                            bool toBreak = detachedMenus[i].isWithinBounds(Game1.getMouseX(), Game1.getMouseY());

                            var menu = Game1.activeClickableMenu;
                            Game1.activeClickableMenu = detachedMenus[i];
                            Game1.activeClickableMenu.receiveRightClick(Game1.getMouseX(), Game1.getMouseY());
                            if (Game1.activeClickableMenu != null)
                            {
                                var d = new Point(detachedMenus[i].xPositionOnScreen - Game1.activeClickableMenu.xPositionOnScreen, detachedMenus[i].yPositionOnScreen - Game1.activeClickableMenu.yPositionOnScreen);
                                detachedMenus[i] = Game1.activeClickableMenu;
                                AdjustMenu(detachedMenus[i], d, true);
                                Game1.activeClickableMenu = menu;
                                if (toBreak)
                                {
                                    detachedMenus.Add(detachedMenus[i]);
                                    detachedMenus.RemoveAt(i);
                                }
                            }
                            else
                                detachedMenus.RemoveAt(i);
                            Game1.activeClickableMenu = menu;
                            if (toBreak)
                            {
                                Helper.Input.Suppress(SButton.MouseRight);
                                return;
                            }
                        }
                    }
                }
                else
                {
                    RightClickLimiter = 0;
                }
            }
        }

        private void GameLoop_UpdateTicking_MoveMenus(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || !Config.EnableMod)
                return;
            if(isKeybindPressed(Config.MoveKeys.Keybinds[0].Buttons))
            {
                if(Game1.activeClickableMenu != null)
                {
                    if (currentlyDragging == Game1.activeClickableMenu || currentlyDragging is null && Game1.activeClickableMenu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        currentlyDragging = Game1.activeClickableMenu;
                        AdjustMenu(Game1.activeClickableMenu, Game1.getMousePosition() - lastMousePosition, true);
                        Array.ForEach(Config.MoveKeys.Keybinds[0].Buttons, button => Helper.Input.Suppress(button));
                        if (Game1.activeClickableMenu is ItemGrabMenu && Helper.ModRegistry.IsLoaded("Pathoschild.ChestsAnywhere"))
                        {
                            Game1.activeClickableMenu = Game1.activeClickableMenu.ShallowClone();
                        }
                        goto next;
                    }
                }
                for (int i = Game1.onScreenMenus.Count - 1; i >= 0; i--)
                {
                    if (Game1.onScreenMenus[i] is null)
                        continue;
                    if (currentlyDragging == Game1.onScreenMenus[i] || currentlyDragging is null && Game1.onScreenMenus[i].isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        currentlyDragging = Game1.onScreenMenus[i];

                        Game1.onScreenMenus.Add(Game1.onScreenMenus[i]);
                        Game1.onScreenMenus.RemoveAt(i);
                        AdjustMenu(Game1.onScreenMenus[i], Game1.getMousePosition() - lastMousePosition, true);
                        Array.ForEach(Config.MoveKeys.Keybinds[0].Buttons, button => Helper.Input.Suppress(button));
                        goto next;
                    }
                }
                for (int i = detachedMenus.Count - 1; i >= 0; i--)
                {
                    if (detachedMenus[i] is null)
                        continue;
                    if (currentlyDragging == detachedMenus[i] || currentlyDragging is null && detachedMenus[i].isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        currentlyDragging = detachedMenus[i];

                        detachedMenus.Add(detachedMenus[i]);
                        detachedMenus.RemoveAt(i);
                        AdjustMenu(detachedMenus[i], Game1.getMousePosition() - lastMousePosition, true);
                        Array.ForEach(Config.MoveKeys.Keybinds[0].Buttons, button => Helper.Input.Suppress(button));
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
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_MoveKeys_Name"),
                getValue: () => Config.MoveKeys,
                setValue: value => Config.MoveKeys = value
            );
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_DetachKeys_Name"),
                getValue: () => Config.DetachKeys,
                setValue: value => Config.DetachKeys = value
            );
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_CloseKeys_Name"),
                getValue: () => Config.CloseKeys,
                setValue: value => Config.CloseKeys = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_StrictKeybindings_Name"),
                tooltip: () => ModEntry.SHelper.Translation.Get("GMCM_Option_StrictKeybindings_Tooltip"),
                getValue: () => Config.StrictKeybindings,
                setValue: value => Config.StrictKeybindings = value
            );
        }
   }
}