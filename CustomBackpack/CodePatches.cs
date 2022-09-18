using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;

namespace CustomBackpack
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(InventoryMenu), new Type[] { typeof(int), typeof(int), typeof(bool), typeof(IList<Item>), typeof(InventoryMenu.highlightThisItem), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class InventoryMenu_Patch
        {
            public static void Postfix(InventoryMenu __instance, int xPosition, int yPosition, bool playerInventory, IList<Item> actualInventory, InventoryMenu.highlightThisItem highlightMethod, int capacity, int rows, int horizontalGap, int verticalGap, bool drawSlots)
            {
                if (!Config.ModEnabled || (__instance.actualInventory != Game1.player.Items) || __instance.actualInventory.Count <= 36)
                    return;

                SMonitor.Log($"Created new inventory menu with {__instance.actualInventory.Count} slots");
                upArrow = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(__instance.xPositionOnScreen + 768 + 32 - 50, __instance.yPositionOnScreen - 46, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12, -1, -1), 0.8f, false)
                {
                    myID = 88,
                    downNeighborID = 89,
                    rightNeighborID = 106,
                    leftNeighborID = -99998
                };
                downArrow = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(__instance.xPositionOnScreen + 768 + 32 - 50, __instance.yPositionOnScreen + 192 + 32 - 53, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11, -1, -1), 0.8f, false)
                {
                    myID = 89,
                    upNeighborID = 88,
                    rightNeighborID = 106,
                    leftNeighborID = -99998
                };
                scrolled = 0;
                __instance.inventory.Clear();
                for (int j = 0; j < __instance.actualInventory.Count; j++)
                {
                    __instance.inventory.Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(xPosition + j % (__instance.capacity / rows) * 64 + horizontalGap * (j % (__instance.capacity / rows)), __instance.yPositionOnScreen + j / (__instance.capacity / rows) * (64 + verticalGap) + (j / (__instance.capacity / rows) - 1) * 4 - ((j > __instance.capacity / rows || !playerInventory || verticalGap != 0) ? 0 : 12), 64, 64), j.ToString() ?? "")
                    {
                        myID = 90000 + j,
                        leftNeighborID = ((j % (__instance.capacity / rows) != 0) ? 90000 + (j - 1) : 107),
                        rightNeighborID = (((j + 1) % (__instance.capacity / rows) != 0) ? 90000 + (j + 1) : 106),
                        downNeighborID = ((j >= __instance.capacity - __instance.capacity / rows) ? 102 : 90000 + (j + __instance.capacity / rows)),
                        upNeighborID = ((j < __instance.capacity / rows) ? (12340 + j) : 90000 + (j - __instance.capacity / rows)),
                        region = 9000,
                        upNeighborImmutable = true,
                        downNeighborImmutable = true,
                        leftNeighborImmutable = true,
                        rightNeighborImmutable = true
                    });
                }

            }
        }

        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.hover))]
        public class InventoryMenu_hover_Patch
        {
            public static void Prefix(InventoryMenu __instance, int x, int y)
            {
                if (!Config.ModEnabled || (__instance.actualInventory != Game1.player.Items) || __instance.actualInventory.Count <= 36 || !__instance.isWithinBounds(x, y))
                    return;
                if (Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadUp) && !Game1.oldPadState.IsButtonDown(Buttons.DPadUp))
                {
                    if (scrolled > 0)
                    {
                        scrolled--;
                    }
                }
                else if (Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadDown) && !Game1.oldPadState.IsButtonDown(Buttons.DPadDown))
                {
                    if (__instance.actualInventory.Count >= __instance.capacity / __instance.rows * (scrolled + 1) + __instance.capacity)
                    {
                        scrolled++;
                    }
                }
                else if (Game1.input.GetMouseState().ScrollWheelValue != Game1.oldMouseState.ScrollWheelValue)
                {
                    var oldScrolled = scrolled;
                    if (Game1.oldMouseState.ScrollWheelValue - Game1.input.GetMouseState().ScrollWheelValue > 0)
                    {
                        if (__instance.actualInventory.Count >= __instance.capacity / __instance.rows * (scrolled + 1) + __instance.capacity)
                        {
                            scrolled++;
                        }
                    }
                    else
                    {
                        if (scrolled > 0)
                        {
                            scrolled--;
                        }
                    }
                    if (scrolled != oldScrolled)
                    {
                        Game1.playSound("shiny4");
                        int width = __instance.capacity / __instance.rows;
                        for (int i = 0; i < __instance.inventory.Count; i++)
                        {
                            if(i < scrolled * width || i >= scrolled * width + __instance.capacity)
                            {
                                __instance.inventory[i].bounds = new Microsoft.Xna.Framework.Rectangle();
                            }
                            else
                            {
                                int j = i - scrolled * width;
                                __instance.inventory[i].bounds = new Microsoft.Xna.Framework.Rectangle(__instance.xPositionOnScreen + i % (__instance.capacity / __instance.rows) * 64 + __instance.horizontalGap * (i % width), __instance.yPositionOnScreen + j / width * (64 + __instance.verticalGap) + (j / width - 1) * 4 - ((j > width || !__instance.playerInventory || __instance.verticalGap != 0) ? 0 : 12), 64, 64);
                            }
                            __instance.inventory[i].downNeighborID = (i >= __instance.capacity + width * (scrolled - 1)) ? 102 : 90000 + (i + width);
                            __instance.inventory[i].upNeighborID = (i < width * (scrolled + 1)) ? (12340 + i - width * scrolled) : 90000 + (i - width);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.snapToClickableComponent))]
        public class InventoryMenu_snapToClickableComponent_Patch
        {
            public static void Prefix(InventoryMenu __instance, int x, int y)
            {
                if (!Config.ModEnabled || (__instance.actualInventory != Game1.player.Items) || __instance.actualInventory.Count <= 36)
                    return;
                var oldScrolled = scrolled;

                var where = GetGridPosition(__instance, x, y);
                if(where == Where.above)
                {
                    scrolled--;
                }
                else if(where == Where.below)
                {
                    scrolled++;
                }
                if(scrolled != oldScrolled)
                {
                    int width = __instance.capacity / __instance.rows;
                    Game1.playSound("shiny4");
                    for(int i = 0; i < __instance.inventory.Count; i++)
                    {
                        __instance.inventory[i].bounds = new Microsoft.Xna.Framework.Rectangle(__instance.inventory[i].bounds.Location - new Point(0, (scrolled - oldScrolled) * __instance.inventory[i].bounds.Height), __instance.inventory[i].bounds.Size);
                        __instance.inventory[i].downNeighborID = (i >= __instance.capacity + width * (scrolled - 1)) ? 102 : 90000 + (i + width);
                        __instance.inventory[i].upNeighborID = (i < width * (scrolled + 1)) ? (12340 + i - width * scrolled) : 90000 + (i - width);
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.getInventoryPositionOfClick))]
        public class InventoryMenu_getInventoryPositionOfClick_Patch
        {
            public static bool Prefix(InventoryMenu __instance, int x, int y, ref int __result)
            {
                if (!Config.ModEnabled || (__instance.actualInventory != Game1.player.Items) || __instance.actualInventory.Count <= 36)
                    return true;
                if(!__instance.isWithinBounds(x, y))
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
                if (!Config.ModEnabled || (__instance.actualInventory != Game1.player.Items) || __instance.actualInventory.Count <= 36)
                    return true;
                if(!__instance.isWithinBounds(x, y))
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
                if (!Config.ModEnabled || !__instance.playerInventory || __instance.actualInventory.Count <= 36)
                    return true;
                if (!__instance.isWithinBounds(x, y))
                {
                    __result = toAddTo;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) })]
        public class InventoryMenu_draw_Patch
        {
            public static void Prefix(InventoryMenu __instance, ref object[] __state)
            {
                if (!Config.ModEnabled || (__instance.actualInventory != Game1.player.Items) || __instance.actualInventory.Count <= 36)
                    return;
                __state = new object[]{
                    Game1.player.Items,
                    __instance.inventory
                };

                __instance.actualInventory = new List<Item>(__instance.actualInventory.Skip(__instance.capacity / __instance.rows * scrolled).Take(__instance.capacity));
                __instance.inventory = new List<ClickableComponent>(__instance.inventory.Skip(__instance.capacity / __instance.rows * scrolled).Take(__instance.capacity));
            }
            public static void Postfix(SpriteBatch b, InventoryMenu __instance, ref object[] __state)
            {
                if (__state is null)
                    return;
                __instance.actualInventory = (IList<Item>)__state[0];
                __instance.inventory = (List<ClickableComponent>)__state[1];
                if (Config.ShowArrows)
                {
                    if (scrolled > 0)
                    {
                        upArrow.setPosition(__instance.xPositionOnScreen + 768 + 32 - 50, __instance.yPositionOnScreen - (__instance.playerInventory ? 46 : 24));
                        upArrow.draw(b);
                    }
                    if (scrolled * __instance.capacity / __instance.rows + __instance.capacity < __instance.actualInventory.Count)
                    {
                        downArrow.setPosition(__instance.xPositionOnScreen + 768 + 32 - 50, __instance.yPositionOnScreen + 192 + 32 - (__instance.playerInventory ? 53 : 60));
                        downArrow.draw(b);
                    }
                }
                if (Config.ShowRowNumbers)
                {
                    Vector2 toDraw = new Vector2(__instance.xPositionOnScreen - 8, __instance.yPositionOnScreen + (__instance.playerInventory ? 0 : 12));
                    for (int i = 0; i < 3; i++)
                    {
                        var strToDraw = (scrolled + 1 + i) + "";
                        Vector2 strSize = Game1.tinyFont.MeasureString(strToDraw);
                        b.DrawString(Game1.tinyFont, strToDraw, toDraw + new Vector2(-strSize.X / 2f, -strSize.Y + i * 68), Color.DimGray);
                        if (i == 0 && __instance.playerInventory)
                            toDraw += new Vector2(0, 12);
                    }
                }
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
   }
}