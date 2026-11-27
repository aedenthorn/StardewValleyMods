using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace Pockets
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.receiveLeftClick))]
        public static class InventoryPage_receiveLeftClick_Patch
        {
            public static bool Prefix(InventoryPage __instance, int x, int y, bool playSound)
            {
                if (!Config.ModEnabled || !__instance.portrait.containsPoint(x, y))
                    return true;
                if (TryGetPocket(Game1.player, out var data, out IList<Item> inventory, x - __instance.portrait.bounds.Left, y - __instance.portrait.bounds.Top))
                {
                    OpenPocket(__instance, data, inventory, playSound);
                    return false;
                }
                else if(openPocket != null)
                {
                    openPocket = null;
                    __instance.inventory = new InventoryMenu(__instance.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth, true, null, null, -1, 3, 0, 0, true);
                    if (playSound)
                    {
                        Game1.playSound("bigDeSelect");
                    }
                }
                return true;
            }

        }

        [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.receiveRightClick))]
        public static class InventoryPage_receiveRightClick_Patch
        {
            public static bool Prefix(InventoryPage __instance, int x, int y, bool playSound)
            {
                if (!Config.ModEnabled || Game1.player.CursorSlotItem is not Item item || !__instance.portrait.containsPoint(x, y))
                    return true;
                if (TryGetPocket(Game1.player, out var data, out IList<Item> inventory, x - __instance.portrait.bounds.Left, y - __instance.portrait.bounds.Top))
                {
                    int stack = item.Stack;
                    Game1.player.CursorSlotItem = AddItemToInventory(inventory, item);
                    if (Game1.player.CursorSlotItem?.Stack == stack)
                    {
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), false);
                        Game1.playSound("cancel", null);
                    }
                    else
                    {
                        Game1.playSound("grassyStep");
                    }
                    return false;
                }
                else
                {
                    int stack = item.Stack;
                    Game1.player.CursorSlotItem = Game1.player.addItemToInventory(item);
                    if (Game1.player.CursorSlotItem?.Stack == stack)
                    {
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), false);
                        Game1.playSound("cancel", null);
                    }
                    else
                    {
                        Game1.playSound("grassyStep");
                    }
                    return false;
                }
            }

        }


        [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.draw), new Type[] { typeof(SpriteBatch) })]
        public static class InventoryPage_draw_Patch
        {
            public static void Postfix(InventoryPage __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled)
                    return;
                var mousePos = Game1.getMousePosition(true);

                if (openPocket!= null)
                {
                    b.Draw(Game1.staminaRect, new Rectangle(__instance.portrait.bounds.Location + new Point(openPocket.StartX, openPocket.StartY), new Point(openPocket.Width, openPocket.Height)), Config.PocketColor * (Config.PocketAlpha + (1 - Config.PocketAlpha) / 2) );
                }
                    
                if (__instance.portrait.containsPoint(mousePos.X, mousePos.Y) && TryGetPocket(Game1.player, out var data, out IList<Item> inventory, mousePos.X - __instance.portrait.bounds.Left, mousePos.Y - __instance.portrait.bounds.Top))
                {
                    b.Draw(Game1.staminaRect, new Rectangle(__instance.portrait.bounds.Location + new Point(data.StartX, data.StartY), new Point(data.Width, data.Height)), Config.PocketColor * Config.PocketAlpha);
                }
            }

        }
    }
}