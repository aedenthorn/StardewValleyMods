using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OutfitSets
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(InventoryPage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class InventoryPage_Patch
        {
            public static void Postfix()
            {
                if (!Config.ModEnabled)
                    return;

            }
        }
        [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.moveCursorInDirection))]
        public class IClickableMenu_moveCursorInDirection_Patch
        {

            public static bool Prefix(IClickableMenu __instance, int direction)
            {
                if (!Config.ModEnabled || __instance.currentlySnappedComponent is null || __instance is not InventoryPage)
                    return true;

                ClickableComponent old = __instance.currentlySnappedComponent;
                int offset = 12;
                if (direction == 2 && old.myID == 104)
                {
                    __instance.currentlySnappedComponent = new ClickableComponent(new Rectangle(GetCenterPoint(__instance as InventoryPage, 0) - new Point(offset, offset), new Point(offset * 2, offset * 2)), "") { myID = IDOffset, downNeighborID = -99998, leftNeighborID = -99998, rightNeighborID = -99998};
                }
                else if (direction == 2 && old.myID == 109)
                {
                    __instance.currentlySnappedComponent = new ClickableComponent(new Rectangle(GetCenterPoint(__instance as InventoryPage, Config.Sets - 1) - new Point(offset, offset), new Point(offset * 2, offset * 2)), "") { myID = IDOffset + Config.Sets - 1 };
                }
                else if (direction == 1 && old.myID >= IDOffset && old.myID < IDOffset + Config.Sets - 1)
                {
                    int oldIndex = old.myID - IDOffset;
                    __instance.currentlySnappedComponent = new ClickableComponent(new Rectangle(GetCenterPoint(__instance as InventoryPage, oldIndex + 1) - new Point(offset, offset), new Point(offset * 2, offset * 2)), "") { myID = IDOffset + oldIndex + 1 };
                }
                else if (direction == 3 && old.myID > IDOffset && old.myID < IDOffset + Config.Sets)
                {
                    int oldIndex = old.myID - IDOffset;
                    __instance.currentlySnappedComponent = new ClickableComponent(new Rectangle(GetCenterPoint(__instance as InventoryPage, oldIndex - 1) - new Point(offset, offset), new Point(offset * 2, offset * 2)), "") { myID = IDOffset + oldIndex - 1 };
                }
                else if (direction == 0 && old.myID >= IDOffset && old.myID < IDOffset + Config.Sets)
                {
                    __instance.currentlySnappedComponent = __instance.getComponentWithID(old.myID < IDOffset + (Config.Sets / 2) ? 104 : 109);
                }
                else
                {
                    return true;
                }
                __instance.snapCursorToCurrentSnappedComponent();
                if (__instance.currentlySnappedComponent != old)
                {
                    Game1.playSound("shiny4");
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.receiveLeftClick))]
        public class InventoryPage_receiveLeftClick_Patch
        {

            public static bool Prefix(InventoryPage __instance, int x, int y)
            {
                if (!Config.ModEnabled)
                    return true;
                for (int i = 0; i < Config.Sets; i++)
                {
                    var strToDraw = (1 + i) + "";
                    var offset = 12;
                    Point center = GetCenterPoint(__instance, i);
                    if (new Rectangle(center - new Point(offset, offset), new Point(offset * 2, offset * 2)).Contains(x, y))
                    {
                        SMonitor.Log($"Clicked on {i + 1}");
                        SwitchSet(i + 1);
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.draw), new Type[] { typeof(SpriteBatch) })]
        public class InventoryPage_draw_Patch
        {

            public static void Postfix(InventoryPage __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled)
                    return;
                int which = int.Parse(Game1.player.modData[keyPrefix + "currentSet"]);
                for(int i = 0; i < Config.Sets; i++)
                {
                    Vector2 toDraw = GetCenterPoint(__instance, i).ToVector2();
                    var strToDraw = (1 + i) + "";
                    Vector2 strSize = Game1.tinyFont.MeasureString(strToDraw);
                    b.DrawString(Game1.tinyFont, strToDraw, toDraw + new Vector2(-strSize.X / 2f, -strSize.Y / 2), which == i + 1 ? Config.CurrentColor : Config.DefaultColor);
                }
			}
        }
    }
}