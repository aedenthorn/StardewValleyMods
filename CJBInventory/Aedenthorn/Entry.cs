using CJB.Common;
using CJB.Common.UI;
using CJBInventory.Framework;
using CJBInventory.Framework.Constants;
using CJBInventory.Framework.Models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace CJBInventory
{
    public static partial class AedenthornInventory
    {
        internal static void SaveLoaded(string uniqueID, ModEntry entry, ModConfig config, IEnumerable<SpawnableItem> items)
        {
            Config = config;
            modEntry = entry;
            var harmony = new Harmony(uniqueID);
            harmony.Patch(
               original: AccessTools.Constructor(typeof(InventoryMenu), new System.Type[] { typeof(int), typeof(int), typeof(bool), typeof(IList<Item>), typeof(InventoryMenu.highlightThisItem), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) }),
               postfix: new HarmonyMethod(typeof(AedenthornInventory), nameof(AedenthornInventory.InventoryMenu_Ctor_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.draw), new System.Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) }),
               prefix: new HarmonyMethod(typeof(AedenthornInventory), nameof(AedenthornInventory.InventoryMenu_draw_Prefix)),
               postfix: new HarmonyMethod(typeof(AedenthornInventory), nameof(AedenthornInventory.InventoryMenu_draw_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.performHoverAction)),
               prefix: new HarmonyMethod(typeof(AedenthornInventory), nameof(AedenthornInventory.InventoryMenu_performHoverAction_Prefix))
            );

            AllItems = items.ToArray();

            entry.Helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;
            entry.Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private static void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.MouseLeft)
                leftMouse = true;
            else if (e.Button == SButton.MouseRight)
                rightMouse = true;
        }

        private static void Input_MouseWheelScrolled(object sender, StardewModdingAPI.Events.MouseWheelScrolledEventArgs e)
        {
            scrollChange = e.Delta;
        }

        private static void InventoryMenu_Ctor_Postfix(InventoryMenu __instance)
        {

            modEntry.Monitor.Log("InventoryMenu_Ctor_Postfix");

            leftMouse = false;
            rightMouse = false;

            menu = new ItemMenu(AllItems, modEntry.TextEntryManager, modEntry.Helper.Content, modEntry.Monitor, __instance);
            ResetItemView(ref __instance);
        }

        private static void InventoryMenu_draw_Prefix(InventoryMenu __instance, SpriteBatch b)
        {
            menu.drawPrefix(b);
        }
        private static void InventoryMenu_draw_Postfix(InventoryMenu __instance, SpriteBatch b)
        {
            if (!__instance.playerInventory)
                return;

            if (shouldResetItemView)
            {
                ResetItemView(ref __instance);
            }

            if(scrollChange != 0)
            {
                scrolled += scrollChange > 0 ? 1 : -1;
                if (scrolled < 0)
                    scrolled = 0;
                else if (scrolled > totalCapacity / 12 - 3)
                    scrolled = totalCapacity / 12 - 3;
                scrollChange = 0;
            }
            if (leftMouse)
            {
                Point mousePos = Game1.getMousePosition(true);
                menu.receiveLeftClick(mousePos.X - Game1.viewport.X, mousePos.Y - Game1.viewport.Y);
                leftMouse = false;
            }
            else if (rightMouse)
            {
                Point mousePos = Game1.getMousePosition(true);
                menu.receiveRightClick(mousePos.X - Game1.viewport.X, mousePos.Y - Game1.viewport.Y);
                rightMouse = false;
            }

            menu.draw(b);
        }

        public static void InventoryMenu_performHoverAction_Prefix(InventoryMenu __instance, int x, int y)
        {
            if (!__instance.playerInventory)
                return;

            menu.performHoverAction(x, y);
        }
    }
}