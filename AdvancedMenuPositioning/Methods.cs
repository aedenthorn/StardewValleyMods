using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace AdvancedMenuPositioning
{
    public partial class ModEntry : Mod
    {

        private static void AdjustMenu(IClickableMenu menu, Point delta, bool first = false)
        {
            if (first)
            {
                adjustedMenus.Clear();
                adjustedComponents.Clear();
            }
            if (menu is null || adjustedMenus.Contains(menu))
                return;
            menu.xPositionOnScreen += delta.X;
            menu.yPositionOnScreen += delta.Y;
            var types = AccessTools.GetDeclaredFields(menu.GetType());
            if (menu is ItemGrabMenu)
            {
                types.AddRange(AccessTools.GetDeclaredFields(typeof(MenuWithInventory)));
            }
            foreach (var f in types)
            {

                if (f.FieldType.IsSubclassOf(typeof(ClickableComponent)) || f.FieldType == typeof(ClickableComponent))
                {
                    AdjustComponent((ClickableComponent)f.GetValue(menu), delta);

                }
                else if (f.FieldType.IsSubclassOf(typeof(IClickableMenu)) || f.FieldType == typeof(IClickableMenu))
                {
                    var child = (IClickableMenu)f.GetValue(menu);
                    if (child != menu)
                    {
                        AdjustMenu(child, delta);

                    }
                }
                else if (f.FieldType == typeof(InventoryMenu))
                {
                    var child = (IClickableMenu)f.GetValue(menu);
                    if (child != menu)
                    {
                        AdjustMenu(child, delta);

                    }
                }
                else if (f.Name == "scrollBarRunner")
                {
                    var c = (Rectangle)f.GetValue(menu);
                    c = new Rectangle(c.X + delta.X, c.Y + delta.Y, c.Width, c.Height);
                    f.SetValue(menu, c);
                }
                else if (f.FieldType == typeof(List<ClickableComponent>))
                {
                    var ol = (List<ClickableComponent>)f.GetValue(menu);
                    if (ol is null)
                        continue;
                    foreach (var o in ol)
                    {
                        AdjustComponent(o, delta);
                    }
                }
                else if (f.FieldType == typeof(List<IClickableMenu>))
                {
                    var ol = (List<IClickableMenu>)f.GetValue(menu);
                    if (ol is null)
                        continue;
                    foreach (var o in ol)
                    {
                        if (o != menu)
                        {
                            AdjustMenu(o, delta);

                        }
                    }
                }
                else if (f.FieldType == typeof(Dictionary<int, ClickableTextureComponent>))
                {
                    var ol = (Dictionary<int, ClickableTextureComponent>)f.GetValue(menu);
                    if (ol is null)
                        continue;
                    foreach (var o in ol.Values)
                    {
                        AdjustComponent(o, delta);
                    }
                }
                else if (f.FieldType == typeof(Dictionary<int, ClickableComponent>))
                {
                    var ol = (Dictionary<int, ClickableComponent>)f.GetValue(menu);
                    if (ol is null)
                        continue;
                    foreach (var o in ol.Values)
                    {
                        AdjustComponent(o, delta);
                    }
                }
                else if (f.FieldType == typeof(Dictionary<int, List<List<ClickableTextureComponent>>>))
                {
                    var ol = (Dictionary<int, List<List<ClickableTextureComponent>>>)f.GetValue(menu);
                    if (ol is null)
                        continue;
                    foreach (var o in ol.Values)
                    {
                        foreach (var o2 in o)
                        {
                            foreach (var o3 in o2)
                            {
                                AdjustComponent(o3, delta);
                            }
                        }
                    }
                }
                else if (f.FieldType == typeof(Dictionary<int, List<List<ClickableComponent>>>))
                {
                    var ol = (Dictionary<int, List<List<ClickableComponent>>>)f.GetValue(menu);
                    if (ol is null)
                        continue;
                    foreach (var o in ol.Values)
                    {
                        foreach (var o2 in o)
                        {
                            foreach (var o3 in o2)
                            {
                                AdjustComponent(o3, delta);
                            }
                        }
                    }
                }
                else if (f.FieldType == typeof(List<ClickableTextureComponent>))
                {
                    var ol = (List<ClickableTextureComponent>)f.GetValue(menu);
                    if (ol is null)
                        continue;
                    foreach (var o in ol)
                    {
                        AdjustComponent(o, delta);
                    }
                }
            }
            if (menu is GameMenu)
            {
                for (int i = 0; i < (menu as GameMenu).pages.Count; i++)
                {
                    if (i != (menu as GameMenu).currentTab)
                        AdjustMenu((menu as GameMenu).pages[i], delta);
                }
            }
            AdjustComponent(menu.upperRightCloseButton, delta);
            adjustedMenus.Add(menu);
        }

        private static void AdjustComponent(ClickableComponent c, Point delta)
        {
            if (c is not null && !adjustedComponents.Contains(c))
            {
                c.bounds = new Rectangle(c.bounds.X + delta.X, c.bounds.Y + delta.Y, c.bounds.Width, c.bounds.Height);
                adjustedComponents.Add(c);
            }
        }

        private static bool isKeybindPressed(SButton[] buttons)
        {
            if (!buttons.All(button => SHelper.Input.IsDown(button) || SHelper.Input.IsSuppressed(button)))
                return false;
            if (!Config.StrictKeybindings)
                return true;

            KeyboardState keyboardState = Game1.input.GetKeyboardState();
            MouseState mouseState = Game1.input.GetMouseState();
            GamePadState gamePadState = Game1.input.GetGamePadState();
            int numberOfButtonsPressed = 0;

            foreach (SButton button in Enum.GetValues(typeof(SButton)))
            {
                if (SHelper.Input.IsDown(button) || SHelper.Input.IsSuppressed(button))
                    numberOfButtonsPressed++;
            }
            return numberOfButtonsPressed == buttons.Length;
        }
    }
}