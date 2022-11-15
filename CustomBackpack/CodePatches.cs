using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CustomBackpack
{
    public partial class ModEntry
    {
        public static ClickableTextureComponent upArrow;
        public static ClickableTextureComponent downArrow;
        public static ClickableTextureComponent expandButton;

        [HarmonyPatch(typeof(InventoryMenu), new Type[] { typeof(int), typeof(int), typeof(bool), typeof(IList<Item>), typeof(InventoryMenu.highlightThisItem), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class InventoryMenu_Patch
        {
            public static void Postfix(InventoryMenu __instance, int xPosition, int yPosition, bool playerInventory, IList<Item> actualInventory, InventoryMenu.highlightThisItem highlightMethod, int capacity, int rows, int horizontalGap, int verticalGap, bool drawSlots)
            {
                if (!Config.ModEnabled || (__instance.actualInventory != Game1.player.Items) || __instance.capacity >= __instance.actualInventory.Count)
                    return;

                if (capacity != oldCapacity.Value || rows != oldRows.Value)
                    scrolled.Value = 0;

                oldRows.Value = rows;
                oldCapacity.Value = capacity;

                SMonitor.Log($"Created new inventory menu with {__instance.actualInventory.Count} slots");
                upArrow = new ClickableTextureComponent(new Rectangle(__instance.xPositionOnScreen + 768 + 32 - 50, __instance.yPositionOnScreen - 46, 24, 24), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12, -1, -1), 0.4f, false)
                {
                    myID = 88,
                    downNeighborID = 90,
                    rightNeighborID = 106,
                    leftNeighborID = -99998
                };
                downArrow = new ClickableTextureComponent(new Rectangle(__instance.xPositionOnScreen + 768 + 32 - 50, __instance.yPositionOnScreen + 192 + 32 - 53, 24, 24), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11, -1, -1), 0.4f, false)
                {
                    myID = 89,
                    upNeighborID = 90,
                    rightNeighborID = 106,
                    leftNeighborID = -99998
                };
                expandButton = new ClickableTextureComponent(new Rectangle(__instance.xPositionOnScreen + 768 + 32 - 50, __instance.yPositionOnScreen + 192 + 32 - 53, 32, 32), Game1.mouseCursors, OptionsPlusMinus.plusButtonSource, 4f, false)
                {
                    myID = 90,
                    upNeighborID = 88,
                    downNeighborID = 89,
                    rightNeighborID = 106,
                    leftNeighborID = -99998
                };
                __instance.inventory.Clear();
                int offset = __instance.GetOffset();
                for (int i = 0; i < __instance.actualInventory.Count; i++)
                {
                    var cc = new ClickableComponent(GetBounds(__instance, i), i.ToString() ?? "")
                    {
                        myID = (i >= offset  && i < offset + __instance.capacity) ? IDOffset + i - offset : -99999,
                        leftNeighborID = GetLeftNeighbor(__instance, i),
                        rightNeighborID = GetRightNeighbor (__instance, i),
                        downNeighborID = GetDownNeighbor(__instance, i),
                        upNeighborID = GetUpNeighbor(__instance, i),
                        region = 9000,
                        upNeighborImmutable = true,
                        downNeighborImmutable = true,
                        leftNeighborImmutable = true,
                        rightNeighborImmutable = true
                    };
                    __instance.inventory.Add(cc);
                }

            }

        }

        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.hover))]
        public class InventoryMenu_hover_Patch
        {
            public static void Prefix(InventoryMenu __instance, int x, int y)
            {
                if (!Config.ModEnabled || (__instance.actualInventory != Game1.player.Items) || __instance.capacity >= __instance.actualInventory.Count || !__instance.isWithinBounds(x, y))
                    return;
                OnHover(ref __instance, x, y);
            }
        }
        
        [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.performHoverAction))]
        public class ShopMenu_performHoverAction_Patch
        {
            public static void Prefix(ShopMenu __instance, int x, int y)
            {
                if (!Config.ModEnabled || (__instance.inventory.actualInventory != Game1.player.Items) || __instance.inventory.capacity >= __instance.inventory.actualInventory.Count || !__instance.inventory.isWithinBounds(x, y))
                    return;
                OnHover(ref __instance.inventory, x, y);
            }
        }
        
        [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.receiveScrollWheelAction))]
        public class ShopMenu_receiveScrollWheelAction_Patch
        {
            public static bool Prefix(ShopMenu __instance, int direction)
            {
                return !Config.ModEnabled || (__instance.inventory.actualInventory != Game1.player.Items) || __instance.inventory.capacity >= __instance.inventory.actualInventory.Count || !__instance.inventory.isWithinBounds(Game1.getMouseX(), Game1.getMouseY());
            }
        }

        
        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.getInventoryPositionOfClick))]
        public class InventoryMenu_getInventoryPositionOfClick_Patch
        {
            public static bool Prefix(InventoryMenu __instance, int x, int y, ref int __result)
            {
                if (!Config.ModEnabled || (__instance.actualInventory != Game1.player.Items) || __instance.capacity >= __instance.actualInventory.Count)
                    return true;
                if (!IsWithinBounds(__instance, x, y))
                {
                    __result = -1;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.leftClick))]
        public class InventoryMenu_leftClick_Patch
        {
            public static bool Prefix(InventoryMenu __instance, int x, int y, Item toPlace, ref Item __result)
            {
                if (!Config.ModEnabled || (__instance.actualInventory != Game1.player.Items) || __instance.capacity >= __instance.actualInventory.Count)
                    return true;
                if(!IsWithinBounds(__instance, x, y))
                {
                    __result = toPlace;
                    return false;
                }
                return true;
            }
        }
        
        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.rightClick))]
        public class InventoryMenu_rightClick_Patch
        {
            public static bool Prefix(InventoryMenu __instance, int x, int y, Item toAddTo, ref Item __result)
            {
                if (!Config.ModEnabled || !__instance.playerInventory || __instance.capacity >= __instance.actualInventory.Count)
                    return true;
                if (!__instance.isWithinBounds(x, y))
                {
                    __result = toAddTo;
                    return false;
                }
                return true;
            }
        }
        
        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.setUpForGamePadMode))]
        public class InventoryMenu_setUpForGamePadMode_Patch
        {
            public static void Postfix(InventoryMenu __instance)
            {
                if (!Config.ModEnabled || __instance.inventory is null || __instance.capacity >= __instance.actualInventory.Count)
                    return;
                if (__instance.inventory.Count > 0)
                {
                    var bounds = __instance.inventory[scrolled.Value * __instance.capacity / __instance.rows].bounds;
                    Game1.setMousePosition(bounds.Right - bounds.Width / 8, bounds.Bottom - bounds.Height / 8);
                    var x = Game1.getMousePosition();
                    var y = x;
                }
            }
        }

        [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.applyMovementKey), new Type[] {typeof(int) })]
        public class IClickableMenu_moveCursorInDirection_Patch
        {
            public static bool Prefix(IClickableMenu __instance, int direction)
            {
                if (!Config.ModEnabled || __instance.currentlySnappedComponent is null)
                    return true;
                InventoryMenu menu = null;
                if (__instance is InventoryPage)
                {
                    menu = (__instance as InventoryPage).inventory;
                }
                else if (__instance is ItemGrabMenu)
                {
                    menu = (__instance as ItemGrabMenu).inventory;
                }
                else if (__instance is GeodeMenu)
                {
                    menu = (__instance as GeodeMenu).inventory;
                }
                else if (__instance is JunimoNoteMenu)
                {
                    menu = (__instance as JunimoNoteMenu).inventory;
                }
                else
                {
                    foreach(var field in AccessTools.GetDeclaredFields(__instance.GetType()))
                    {
                        if(field.FieldType == typeof(InventoryMenu) && ((InventoryMenu)field.GetValue(__instance)).actualInventory == Game1.player.Items)
                        {
                            menu = (InventoryMenu)field.GetValue(__instance);
                        }
                    }
                }
                if(menu is null)
                    return true;
                if (!menu.Scrolling())
                    return true;
                int columns = menu.Columns();
                if(__instance is ItemGrabMenu && menu.inventory != null && menu.inventory.Count >= (__instance as ItemGrabMenu).GetColumnCount())
                {
                    SMonitor.Log($"items to grab {(__instance as ItemGrabMenu).ItemsToGrabMenu.inventory.Count}");
                    for (int i = 0; i < columns; i++)
                    {
                        menu.inventory[i + menu.GetOffset()].upNeighborID = ((__instance as ItemGrabMenu).shippingBin ? 12598 : (Math.Min(i, (__instance as ItemGrabMenu).ItemsToGrabMenu.inventory.Count - 1) + 53910));
                    }
                }
                else if(__instance is GeodeMenu && menu.inventory != null)
                {
                    for (int i = 0; i < columns; i++)
                    {
                        menu.inventory[i + menu.GetOffset()].upNeighborID = 998;
                    }
                }
                else if(__instance is JunimoNoteMenu && menu.inventory != null)
                {
                    for (int i = 0; i < columns; i++)
                    {
                        menu.inventory[i + menu.GetOffset() + (menu.rows - 1 * columns)].downNeighborID = -99998;
                    }
                    for (int i = 0; i < menu.rows; i++)
                    {
                        menu.inventory[menu.GetOffset() + (i * columns) + columns - 1].rightNeighborID = -99998;
                    }
                }
                ClickableComponent old = __instance.currentlySnappedComponent;
                if (direction == 0 && (old.myID == 102 || old.myID == 101))
                {
                    __instance.currentlySnappedComponent = __instance.getComponentWithID(IDOffset + columns * (menu.rows - 1));
                }
                else if (direction == 2 && old.myID >= 12340 && old.myID < 12340 + columns)
                {
                    __instance.currentlySnappedComponent = __instance.getComponentWithID(IDOffset + old.myID % 12340);
                }
                else if (direction == 2 && __instance is GeodeMenu && old.myID == 998)
                {
                    __instance.currentlySnappedComponent = __instance.getComponentWithID(IDOffset);
                }
                else if (direction == 2 && __instance is ItemGrabMenu && old.myID >= 53910 + (__instance as ItemGrabMenu).ItemsToGrabMenu.capacity  - (__instance as ItemGrabMenu).ItemsToGrabMenu.Columns() && old.myID < 53910 + (__instance as ItemGrabMenu).ItemsToGrabMenu.capacity)
                {
                    int idx = IDOffset + (old.myID - 53910) % (__instance as ItemGrabMenu).ItemsToGrabMenu.Columns();
                    __instance.currentlySnappedComponent = __instance.getComponentWithID(idx);
                }
                else if (direction == 0 && old.myID >= IDOffset && old.myID < IDOffset + columns && scrolled.Value > 0)
                {
                    ChangeScroll(menu, -1);
                    __instance.currentlySnappedComponent = __instance.getComponentWithID(old.myID - columns);
                }
                else if (direction == 2 && old.myID >= IDOffset + menu.capacity - columns && old.myID < IDOffset + menu.capacity && scrolled.Value < (menu.actualInventory.Count  / columns) - menu.rows)
                {
                    SMonitor.Log($"a {direction}, {old.myID}, {old.upNeighborID}, {old.downNeighborID}");
                    ChangeScroll(menu, 1);
                    __instance.currentlySnappedComponent = __instance.getComponentWithID(old.myID + columns);
                }
                else
                {
                    SMonitor.Log($"b {direction}, {old.myID}, {old.upNeighborID}, {old.downNeighborID}");
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
        
        [HarmonyPatch(typeof(ItemGrabMenu), "customSnapBehavior")]
        public class ItemGrabMenu_customSnapBehavior_Patch
        {
            public static bool Prefix(ItemGrabMenu __instance, int direction, int oldRegion, int oldID)
            {
                if (!Config.ModEnabled || __instance.inventory is null || !__instance.inventory.Scrolling())
                    return true;
                InventoryMenu menu = __instance.inventory;
                int columns = menu.Columns();
                if (direction == 2)
                {
                    if (menu.inventory != null && menu.inventory.Count >= __instance.GetColumnCount() && __instance.shippingBin)
                    {
                        for (int i = 0; i < columns; i++)
                        {
                            menu.inventory[i + menu.GetOffset()].upNeighborID = (__instance.shippingBin ? 12598 : (Math.Min(i, __instance.ItemsToGrabMenu.inventory.Count - 1) + 53910));
                        }
                    }
                    SMonitor.Log($"{oldID}, {menu.capacity}, {scrolled.Value}");
                    if (oldID >= IDOffset && oldID < IDOffset + menu.capacity && oldID >= IDOffset + menu.capacity - columns && scrolled.Value < menu.actualInventory.Count / columns - menu.rows)
                    {
                        ChangeScroll(menu, 1);
                        __instance.currentlySnappedComponent = __instance.getComponentWithID(oldID);
                    }
                    else if (!__instance.shippingBin && oldID >= 53910)
                    {
                        int index = oldID - 53910;
                        if (index + __instance.GetColumnCount() <= __instance.ItemsToGrabMenu.inventory.Count - 1)
                        {
                            __instance.currentlySnappedComponent = __instance.getComponentWithID(index + __instance.GetColumnCount() + 53910);
                            __instance.snapCursorToCurrentSnappedComponent();
                            return false;
                        }
                    }
                    else
                    {
                        __instance.currentlySnappedComponent = __instance.getComponentWithID((oldRegion == 12598) ? IDOffset : IDOffset + ((oldID - 53910) % __instance.GetColumnCount()));
                    }
                }
                else if (direction == 0)
                {
                    if(oldID >= IDOffset && oldID < IDOffset + columns)
                    {
                        if (scrolled.Value > 0)
                        {
                            ChangeScroll(menu, -1);
                            __instance.currentlySnappedComponent = __instance.getComponentWithID(oldID);
                        }
                        else if (__instance.shippingBin && Game1.getFarm().lastItemShipped != null)
                        {
                            __instance.currentlySnappedComponent = __instance.getComponentWithID(12598);
                            __instance.currentlySnappedComponent.downNeighborID = oldID;
                        }
                    }
                    else if (oldID >= IDOffset + menu.Columns())
                    {
                        __instance.currentlySnappedComponent = __instance.getComponentWithID(oldID - menu.Columns());
                    }
                    else
                        return true;
                }
                __instance.snapCursorToCurrentSnappedComponent();
                return false;
            }
        }

        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) })]
        public class InventoryMenu_draw_Patch
        {
            public static void Prefix(InventoryMenu __instance, ref object[] __state)
            {
                if (!Config.ModEnabled || (__instance.actualInventory != Game1.player.Items) || __instance.capacity >= __instance.actualInventory.Count)
                    return;
                __state = new object[]{
                    Game1.player.Items,
                    __instance.inventory
                };

                __instance.actualInventory = new List<Item>(__instance.actualInventory.Skip(__instance.capacity / __instance.rows * scrolled.Value).Take(__instance.capacity));
                __instance.inventory = new List<ClickableComponent>(__instance.inventory.Skip(__instance.capacity / __instance.rows * scrolled.Value).Take(__instance.capacity));
            }
            public static void Postfix(SpriteBatch b, InventoryMenu __instance, ref object[] __state)
            {
                if (__state is null)
                    return;
                __instance.actualInventory = (IList<Item>)__state[0];
                __instance.inventory = (List<ClickableComponent>)__state[1];
                DrawUIElements(b, __instance);
            }
        }
        [HarmonyPatch(typeof(SeedShop), nameof(SeedShop.draw))]
        public class SeedShop_draw_Patch
        {
            public static bool Prefix(SeedShop __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !dataDict.Any())
                    return true;

                var ptr = AccessTools.Method(typeof(GameLocation), "draw", new Type[] { typeof(SpriteBatch) }).MethodHandle.GetFunctionPointer();
                var baseMethod = (Func<SpriteBatch, GameLocation>)Activator.CreateInstance(typeof(Func<SpriteBatch, GameLocation>), __instance, ptr);
                baseMethod(b);

                var list = dataDict.Keys.ToList();
                list.Sort();
                foreach(var i in list)
                {
                    if(Game1.player.maxItems.Value < i)
                    {
                        b.Draw(dataDict[i].texture, Game1.GlobalToLocal(Config.BackpackPosition), dataDict[i].textureRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1232f);
                        return false;
                    }
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performAction))]
        public class GameLocation_performAction_Patch
        {
            public static bool Prefix(GameLocation __instance, string action, Farmer who, Location tileLocation, ref bool __result)
            {
                if (!Config.ModEnabled || !dataDict.Any())
                    return true;
                string[] actionParams = action.Split(' ', StringSplitOptions.None);
                string text = actionParams[0];
                if (text != "BuyBackpack")
                {
                    return true;
                }
                var list = dataDict.Keys.ToList();
                list.Sort();
                foreach (var i in list)
                {
                    if (Game1.player.maxItems.Value < i)
                    {
                        SMonitor.Log($"showing dialogue to buy backpack {dataDict[i].name} for {dataDict[i].cost}");
                        __instance.createQuestionDialogue(string.Format(SHelper.Translation.Get("backpack-upgrade-x"), i), new Response[]
                        {
                            new Response("Purchase", string.Format(SHelper.Translation.Get("buy-backpack-for-x"), dataDict[i].cost)),
                            new Response("Not", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_ResponseNo"))
                        }, "Backpack");
                        __result = true;
                        return false;
                    }
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.answerDialogueAction))]
        public class GameLocation_answerDialogueAction_Patch
        {
            public static bool Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
            {
                if (!Config.ModEnabled || questionAndAnswer != "Backpack_Purchase" || !dataDict.Any())
                    return true;
                var list = dataDict.Keys.ToList();
                list.Sort();
                foreach (var i in list)
                {
                    if (Game1.player.maxItems.Value < i)
                    {
                        if (Game1.player.Money >= dataDict[i].cost)
                        {
                            SMonitor.Log($"buying backpack {dataDict[i].name} for {dataDict[i].cost}");
                            Game1.player.Money -= dataDict[i].cost;
                            SetPlayerSlots(i);
                            Game1.player.holdUpItemThenMessage(new SpecialItem(99, dataDict[i].name), true);
                            ((Multiplayer)typeof(Game1).GetField("multiplayer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(Game1.game1)).globalChatInfoMessage($"CustomBackpack_{i}", new string[]
                            {
                                Game1.player.Name
                            });
                        }
                        else
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney2"));
                        }
                        __result = true;
                        return false;
                    }
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(SpecialItem), "displayName")]
        [HarmonyPatch(MethodType.Getter)]
        public class SpecialItem_displayName_Patch
        {
            public static bool Prefix(SpecialItem __instance, ref string __result)
            {
                if (!Config.ModEnabled || __instance.which.Value != 99 || !dataDict.Any())
                    return true;

                __result = __instance.Name;
                return false;
            }
        }
        [HarmonyPatch(typeof(SpecialItem), nameof(SpecialItem.getTemporarySpriteForHoldingUp))]
        public class SpecialItem_getTemporarySpriteForHoldingUp_Patch
        {
            public static bool Prefix(SpecialItem __instance, Vector2 position, ref TemporaryAnimatedSprite __result)
            {
                if (!Config.ModEnabled || __instance.which.Value != 99 || !dataDict.Any())
                    return true;

                var data = dataDict.FirstOrDefault(p => p.Value.name == __instance.Name).Value;
                if (data is null)
                    return true;

                __result = new TemporaryAnimatedSprite(data.texturePath, data.textureRect, position + new Vector2(16f, 0f), false, 0f, Color.White)
                {
                    scale = 4f,
                    layerDepth = 1f
                };
                return false;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.shiftToolbar))]
        private static class Farmer_shiftToolbar_Patch
        {
            public static bool Prefix(Farmer __instance, bool right)
            {
                if (!Config.ModEnabled || Config.ShiftRows < 1 || Config.ShiftRows >= __instance.Items.Count / 12 || __instance.Items is null || __instance.Items.Count < 37 || __instance.UsingTool || Game1.dialogueUp || (!Game1.pickingTool && !Game1.player.CanMove) || __instance.areAllItemsNull() || Game1.eventUp || Game1.farmEvent != null)
                    return true;
                if (Config.ShiftRows == 1)
                    return false;
                Game1.playSound("shwip");

                if (__instance.CurrentItem != null)
                {
                    __instance.CurrentItem.actionWhenStopBeingHeld(__instance);
                }
                List<Item> toAlter = __instance.Items.Take(Config.ShiftRows * 12).ToList();
                if (right)
                {
                    List<Item> toMove = toAlter.Take(12).ToList();
                    for (int i = 0; i < toMove.Count; i++)
                    {
                        toAlter.RemoveAt(0);
                    }
                    toAlter.AddRange(toMove);
                }
                else
                {
                    List<Item> toMove = toAlter.Skip(toAlter.Count - 12).Take(12).ToList();
                    for (int i = 0; i < toAlter.Count - 12; i++)
                    {
                        toMove.Add(toAlter[i]);
                    }
                    for (int i = 0; i < toMove.Count; i++)
                    {
                        toAlter[i] = toMove[i];
                    }

                }
                for (int i = 0; i < toAlter.Count; i++)
                {
                    __instance.Items[i] = toAlter[i];
                }

                __instance.netItemStowed.Set(false);
                if (__instance.CurrentItem != null)
                {
                    __instance.CurrentItem.actionWhenBeingHeld(__instance);
                }
                for (int j = 0; j < Game1.onScreenMenus.Count; j++)
                {
                    if (Game1.onScreenMenus[j] is Toolbar)
                    {
                        (Game1.onScreenMenus[j] as Toolbar).shifted(right);
                        return false;
                    }
                }
                return false;
            }
        }
    }
}