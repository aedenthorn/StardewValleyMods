using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CropStacking
{
    public partial class ModEntry
    {
        private static bool skip;

        [HarmonyPatch(typeof(Item), "GetOneCopyFrom")]
        public class Item_GetOneCopyFrom_Patch
        {
            public static void Postfix(Item __instance)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(modKey))
                    return;
                __instance.modData.Remove(modKey);
            }
        }
        [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.drawHoverText), new Type[] { typeof(SpriteBatch), typeof(StringBuilder), typeof(SpriteFont), typeof(int), typeof(int), typeof(int), typeof(string), typeof(int), typeof(string[]), typeof(Item), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(float), typeof(CraftingRecipe), typeof(IList<Item>), typeof(Texture2D), typeof(Rectangle?), typeof(Color?), typeof(Color?)})]
        public class IClickableMenu_drawToolTip_Patch
        {

            public static void Postfix(IClickableMenu __instance, SpriteBatch b, Item hoveredItem)
            {
                if(!Config.ModEnabled || hoveredItem == null || !hoveredItem.modData.TryGetValue(modKey, out var dataString)) 
                    return;
                var list = GetDataList(dataString);
                int cols = 4;
                int cellHeight = 64;
                int cellWidth = 64;
                var rows = (int)Math.Ceiling((list.Count + 1) / (float)cols);
                int x = Game1.getOldMouseX() + 32;
                int y = Game1.getOldMouseY() + 32 - rows * cellHeight;
                IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, cols * cellWidth, rows * cellHeight, Color.White, 1f, true, -1f);

                skip = true;
                hoveredItem.drawInMenu(b, new Vector2(x, y), 1f, 1, 1, StackDrawType.Draw, Color.White, true);
                skip = false;

                for (int i = 1; i < list.Count + 1; i++)
                {
                    var pos = new Vector2(x + i % cols * cellWidth, y + i / cols * cellHeight);
                    var item = CreateItem(list[i - 1]);
                    item.drawInMenu(b, pos, 1f, 1, 1, StackDrawType.Draw, Color.White, true);
                }

            }
        }
        [HarmonyPatch(typeof(Item), nameof(Item.DrawMenuIcons))]
        public class Item_DrawMenuIcons_Patch
        {
            public static void Prefix(Item __instance, SpriteBatch sb, Vector2 location, float scale_size, float transparency, float layer_depth, ref StackDrawType drawStackNumber, Color color)
            {
                if (!Config.ModEnabled || skip || !__instance.modData.TryGetValue(modKey, out var dataString) || drawStackNumber == StackDrawType.Hide)
                    return;
                drawStackNumber = StackDrawType.Hide;
                var list = GetDataList(dataString);
                int[] qualities = new int[4];
                foreach (var data in list)
                {
                    if (data.quality >= 4)
                        qualities[3] += data.stack;
                    else
                        qualities[data.quality] += data.stack;
                }
                if (__instance.Quality >= 4)
                    qualities[3] += __instance.Stack;
                else
                    qualities[__instance.Quality] += __instance.Stack;
                for (int i = 0; i < qualities.Length; i++)
                {
                    var q = qualities[i];
                    if (q == 0)
                        continue;
                    var width = Utility.getWidthOfTinyDigitString(q, 2f * scale_size) + 2f * scale_size;
                    var height = 16f * scale_size + 1f;
                    Vector2 v1 = Vector2.Zero;
                    Vector2 v2 = Vector2.Zero;
                    float yOffset = 0f;
                    Rectangle qualityRect = i <= 2 ? new Rectangle(338 + (i - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8);

                    switch (i)
                    {
                        case 0:
                            v1 = new Vector2(64 - width, 64 - height);
                            break;
                        case 1:
                            v1 = new Vector2(4, 12);
                            v2 = new Vector2(12, 12);
                            break;
                        case 2:
                            v1 = new Vector2(64 - width, 12);
                            v2 = new Vector2(64 - 12, 12);
                            break;
                        case 3:
                            v1 = new Vector2(4, 64 - height);
                            yOffset = ((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * 3.1415926535897931 / 512.0) + 1f) * 0.05f;
                            v2 = new Vector2(12, 64 - height + yOffset);
                            break;

                    }
                    if(i > 0)
                        sb.Draw(Game1.mouseCursors, location + v2, new Rectangle?(qualityRect), color * transparency, 0f, new Vector2(4f, 4f), 2f * scale_size * (1f + yOffset), SpriteEffects.None, layer_depth);
                    Utility.drawTinyDigits(q, sb, location + v1, 2f * scale_size, 1f, color);
                }
            }
        }
        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.hover))]
        public class InventoryMenu_hover_Patch
        {
            public static void Postfix(InventoryMenu __instance, int x, int y, Item heldItem, Item __result)
            {
                if (!Config.ModEnabled || __result is null || __result.IsRecipe || heldItem is not null || !Config.CombineKey.JustPressed())
                    return;
                if (__result.modData.TryGetValue(modKey, out var dataString))
                {
                    var list = GetDataList(dataString);
                    foreach(ItemData data in list)
                    {
                        Item item = CreateItem(data);
                        Item leftOver = __instance.tryToAddItem(item);
                        if (leftOver is not null && leftOver.Stack > 0)
                        {
                            Game1.createItemDebris(leftOver, Game1.player.getStandingPosition(), 1, null, -1);
                        }
                    }
                    SMonitor.Log($"Uncombined {list.Count} items");
                    __result.modData.Remove(modKey);
                }
                else if((__result.GetType() == typeof(Object) || __result.GetType() == typeof(ColoredObject)) && !(__result as Object).HasTypeBigCraftable())
                {
                    int remainder = 0;
                    List<ItemData> dataList = new();
                    for (int i = __instance.actualInventory.Count - 1; i >= 0; i--)
                    {
                        var tmp = __instance.actualInventory[i];
                        if (tmp is null || tmp.ItemId != __result.ItemId || tmp == __result || tmp.modData.Count() > 0)
                            continue;
                        if(tmp.canStackWith(__result))
                        {
                            int stackLeft = __result.addToStack(tmp);
                            if(stackLeft > 0)
                            {
                                tmp.Stack = stackLeft;
                            }
                            else
                            {
                                __instance.actualInventory[i] = null;
                            }
                            Game1.playSound("dwop");
                            continue;
                        }
                        else if (__result is ColoredObject && tmp is ColoredObject)
                        {
                            if (!Config.CombineColored && (__result as ColoredObject).color.Value != (tmp as ColoredObject).color.Value)
                                continue;
                            var data = new ItemData()
                            {
                                id = tmp.ItemId,
                                stack = tmp.Stack,
                                quality = tmp.Quality,
                                color = (tmp as ColoredObject).color.Value
                            };
                            remainder = GetRemainder(dataList, data);
                            data.stack -= remainder;
                            dataList.Add(data);
                        }
                        else if (tmp is Object && (tmp as Object).preserve.Value is not null)
                        {
                            if (!Config.CombinePreserves && (__result as Object).preservedParentSheetIndex.Value != (tmp as Object).preservedParentSheetIndex.Value)
                                continue;
                            var data = new ItemData()
                            {
                                id = tmp.ItemId,
                                stack = tmp.Stack,
                                quality = tmp.Quality,
                                preservedParentSheetIndex = (tmp as Object).preservedParentSheetIndex.Value,
                                preserveType = (tmp as Object).preserve.Value.Value
                            };
                            remainder = GetRemainder(dataList, data);
                            data.stack -= remainder;
                            dataList.Add(data);
                        }
                        else if (Config.CombineQualities)
                        {
                            var data = new ItemData()
                            {
                                id = tmp.ItemId,
                                stack = tmp.Stack,
                                quality = tmp.Quality,
                            };
                            remainder = GetRemainder(dataList, data);
                            data.stack -= remainder;
                            dataList.Add(data);
                        }
                        else
                            continue;
                        if(remainder == 0)
                            __instance.actualInventory[i] = null;
                        else
                            __instance.actualInventory[i].Stack = remainder;
                    }
                    if (dataList.Count > 0)
                    {
                        SortList(dataList);
                        __result.modData[modKey] = JsonConvert.SerializeObject(dataList);
                        SMonitor.Log($"Combined {dataList.Count} items");
                        Game1.playSound("dwop");
                    }
                }

            }

        }
    }
}