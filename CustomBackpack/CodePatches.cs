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

                SMonitor.Log($"Created new inventory menu with {__instance.actualInventory.Count} slots");
                upArrow = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(__instance.xPositionOnScreen + 768 + 32 - 50, __instance.yPositionOnScreen - 46, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12, -1, -1), 0.8f, false)
                {
                    myID = 88,
                    downNeighborID = 90,
                    rightNeighborID = 106,
                    leftNeighborID = -99998
                };
                downArrow = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(__instance.xPositionOnScreen + 768 + 32 - 50, __instance.yPositionOnScreen + 192 + 32 - 53, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11, -1, -1), 0.8f, false)
                {
                    myID = 89,
                    upNeighborID = 90,
                    rightNeighborID = 106,
                    leftNeighborID = -99998
                };
                expandButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(__instance.xPositionOnScreen + 768 + 32 - 50, __instance.yPositionOnScreen + 192 + 32 - 53, 64, 64), Game1.mouseCursors, OptionsPlusMinus.plusButtonSource, 4f, false)
                {
                    myID = 90,
                    upNeighborID = 88,
                    downNeighborID = 89,
                    rightNeighborID = 106,
                    leftNeighborID = -99998
                };
                __instance.inventory.Clear();
                for (int j = 0; j < __instance.actualInventory.Count; j++)
                {
                    var cc = new ClickableComponent(GetBounds(__instance, j), j.ToString() ?? "")
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

                __instance.actualInventory = new List<Item>(__instance.actualInventory.Skip(__instance.capacity / __instance.rows * scrolled).Take(__instance.capacity));
                __instance.inventory = new List<ClickableComponent>(__instance.inventory.Skip(__instance.capacity / __instance.rows * scrolled).Take(__instance.capacity));
            }
            public static void Postfix(SpriteBatch b, InventoryMenu __instance, ref object[] __state)
            {
                if (__state is null)
                    return;
                __instance.actualInventory = (IList<Item>)__state[0];
                __instance.inventory = (List<ClickableComponent>)__state[1];
                var cc1 = __instance.inventory[(scrolled + 1) * (__instance.capacity / __instance.rows) - 1];
                Point corner1 = cc1.bounds.Location + new Point(cc1.bounds.Width, 0);
                var cc2 = __instance.inventory[(scrolled + __instance.rows) * (__instance.capacity / __instance.rows) - 1];
                Point corner2 = cc2.bounds.Location + new Point(cc2.bounds.Width, cc2.bounds.Height);
                Point middle = corner1 + new Point(0, (corner2.Y - corner1.Y) / 2);
                if (Config.ShowArrows)
                {
                    if (scrolled > 0)
                    {
                        upArrow.setPosition(corner1.X - 16, corner1.Y - 30);
                        upArrow.draw(b);
                    }
                    if (scrolled * __instance.capacity / __instance.rows + __instance.capacity < __instance.actualInventory.Count)
                    {
                        downArrow.setPosition(corner2.X - 16, corner2.Y - 25);
                        downArrow.draw(b);
                    }
                }
                expandButton.setPosition(middle.X + 12, middle.Y - 11);
                expandButton.draw(b);
                if(SHelper.Input.IsDown(StardewModdingAPI.SButton.MouseLeft) && expandButton.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                {
                    Game1.playSound("shwip");
                    lastMenu = Game1.activeClickableMenu;
                    Game1.activeClickableMenu = new FullInventoryPage(__instance, __instance.xPositionOnScreen, __instance.yPositionOnScreen, __instance.width, __instance.height);
                }
                if (Config.ShowRowNumbers)
                {
                    int width = __instance.capacity / __instance.rows;
                    for (int i = 0; i < __instance.rows; i++)
                    {
                        var cc = __instance.inventory[(scrolled + i) * width];

                        Vector2 toDraw = new Vector2(cc.bounds.X - 8, cc.bounds.Y + 16);

                        var strToDraw = (scrolled + 1 + i) + "";
                        Vector2 strSize = Game1.tinyFont.MeasureString(strToDraw);
                        b.DrawString(Game1.tinyFont, strToDraw, toDraw + new Vector2(-strSize.X / 2f, -strSize.Y), Color.DimGray);
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