using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ResourceStorage
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.consumeObject))]
        public class Farmer_consumeObject_Patch
        {
            public static bool Prefix(Farmer __instance, int index, ref int quantity)
            {
                if (!Config.ModEnabled || !Game1.objectInformation.TryGetValue(index, out string data))
                    return true;

                quantity += ModifyResourceLevel(__instance, GetIdString(data), -quantity);
                return quantity > 0;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.getItemCount))]
        public class Farmer_getItemCount_Patch
        {
            public static void Postfix(Farmer __instance, int item_index, ref int __result)
            {
                if (!Config.ModEnabled || !Game1.objectInformation.TryGetValue(item_index, out string data))
                    return;

                __result += GetResourceAmount(__instance, GetIdString(data));
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.getTallyOfObject))]
        public class Farmer_getTallyOfObject_Patch
        {
            public static void Postfix(Farmer __instance, int index, bool bigCraftable, ref int __result)
            {
                if (!Config.ModEnabled || bigCraftable || !Game1.objectInformation.TryGetValue(index, out string data))
                    return;

                __result += GetResourceAmount(__instance, GetIdString(data));
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.hasItemInInventory))]
        public class Farmer_hasItemInInventory_Patch
        {
            public static bool Prefix(Farmer __instance, int itemIndex, ref int quantity, ref bool __result)
            {
                if (!Config.ModEnabled || !Game1.objectInformation.TryGetValue(itemIndex, out string data))
                    return true;
                quantity -= GetResourceAmount(__instance, GetIdString(data));
                if(quantity <= 0)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.couldInventoryAcceptThisItem))]
        public class Farmer_couldInventoryAcceptThisItem_Patch
        {
            public static void Postfix(Farmer __instance, Item item, ref bool __result)
            {
                if (!Config.ModEnabled || __result || item is not Object || !CanStore(item as Object) || !Game1.objectInformation.TryGetValue(item.ParentSheetIndex, out string data))
                    return;
                string id = GetIdString(data);
                if (GetResourceAmount(__instance, id) > 0 || CanAutoStore(id))
                    __result = true;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.couldInventoryAcceptThisObject))]
        public class Farmer_couldInventoryAcceptThisObject_Patch
        {
            public static void Postfix(Farmer __instance, int index, int stack, int quality, ref bool __result)
            {
                if (!Config.ModEnabled || __result || quality > 0 || !Game1.objectInformation.TryGetValue(index, out string data))
                    return;
                string id = GetIdString(data);
                if (GetResourceAmount(__instance, id) > 0 || CanAutoStore(id))
                    __result = true;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.ConsumeInventoryItem), new Type[] { typeof(Farmer), typeof(Item), typeof(int) })]
        public class Object_ConsumeInventoryItem_Patch
        {
            public static bool Prefix(Farmer who, Item drop_in, ref int amount)
            {
                if (!Config.ModEnabled || !Game1.objectInformation.TryGetValue(drop_in.ParentSheetIndex, out string data))
                    return true;

                amount += ModifyResourceLevel(who, GetIdString(data), -amount);
                return amount > 0;
            }
        }
        [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.ConsumeAdditionalIngredients))]
        public class CraftingRecipe_ConsumeAdditionalIngredients_Patch
        {
            public static void Prefix(List<KeyValuePair<int, int>> additional_recipe_items)
            {
                if (!Config.ModEnabled)
                    return;
                for(int i = 0; i < additional_recipe_items.Count; i++)
                {
                    if (!Game1.objectInformation.TryGetValue(additional_recipe_items[i].Key, out string data))
                        continue;
                    string id = GetIdString(data);
                    additional_recipe_items[i] = new KeyValuePair<int, int>(additional_recipe_items[i].Key, additional_recipe_items[i].Value + ModifyResourceLevel(Game1.player, id, -additional_recipe_items[i].Value));
                }
            }
        }
        [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.getCraftableCount), new Type[] { typeof(IList<Item>) })]
        public class CraftingRecipe_getCraftableCount_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling CraftingRecipe.getCraftableCount");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldloc_2 && codes[i + 1].opcode == OpCodes.Ldloc_3 && codes[i + 2].opcode == OpCodes.Div)
                    {
                        SMonitor.Log($"adding method to increase ingredient count");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(AddIngredientAmount))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldloc_1));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.consumeIngredients))]
        public class CraftingRecipe_consumeIngredients_Patch
        {
            public static void Prefix(CraftingRecipe __instance, ref Dictionary<int, int> __state)
            {
                if (!Config.ModEnabled)
                    return;

                __state = __instance.recipeList;
                Dictionary<int, int> dict = new();
                foreach(var s in __state)
                {
                    if (!Game1.objectInformation.TryGetValue(s.Key, out string data))
                        continue;
                    int amount = s.Value + ModifyResourceLevel(Game1.player, GetIdString(data), -s.Value, false);
                    if (amount <= 0)
                        continue;
                    dict.Add(s.Key, amount);
                }
                __instance.recipeList = dict;
            }
            public static void Postfix(CraftingRecipe __instance, ref Dictionary<int, int> __state)
            {
                if (!Config.ModEnabled)
                    return;
                __instance.recipeList = __state;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.addItemToInventory), new Type[] { typeof(Item), typeof(List<Item>) })]
        public class Farmer_addItemToInventory_Patch
        {
            public static bool Prefix(Farmer __instance, Item item)
            {
                if (!Config.ModEnabled || Game1.activeClickableMenu is ResourceMenu || item is not Object || !CanStore(item as Object) || !Game1.objectInformation.TryGetValue(item.ParentSheetIndex, out string data))
                    return true;
                return ModifyResourceLevel(__instance, GetIdString(data), item.Stack) <= 0;
            }
        }
        [HarmonyPatch(typeof(GameMenu), new Type[] { typeof(bool)})]
        [HarmonyPatch(MethodType.Constructor)]
        public class GameMenu_Patch
        {
            public static void Postfix()
            {
                if (!Config.ModEnabled)
                    return;
                gameMenu = null;
                resourceButton = new ClickableTextureComponent("Up", new Rectangle(Game1.viewport.Width / 2 - 22, Game1.viewport.Height * 2 /3 - 22, 44, 44), "", SHelper.Translation.Get("resources"), Game1.mouseCursors, new Rectangle(116, 442, 22, 22), 2)
                {
                    myID = -1,
                    leftNeighborID = 0,
                    downNeighborID = -2,
                };
            }
        }
        [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.draw))]
        public class InventoryPage_draw_Patch
        {
            public static void Postfix(SpriteBatch b)
            {
                if (!Config.ModEnabled)
                    return;
                resourceButton.draw(b);
            }
        }
        [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.receiveLeftClick))]
        public class InventoryPage_receiveLeftClick_Patch
        {
            public static bool Prefix(InventoryPage __instance, int x, int y)
            {
                if (!Config.ModEnabled)
                    return true;
                if(resourceButton.containsPoint(x, y))
                {
                    if(Game1.player.CursorSlotItem is Object)
                    {
                        if (CanStore(Game1.player.CursorSlotItem as Object) && Game1.objectInformation.TryGetValue(Game1.player.CursorSlotItem.ParentSheetIndex, out string data))
                        {
                            Game1.playSound("Ship");
                            ModifyResourceLevel(Game1.player, GetIdString(data), Game1.player.CursorSlotItem.Stack, false);
                            Game1.player.CursorSlotItem = null;
                        }
                    }
                    else
                    {
                        Game1.playSound("bigSelect");
                        gameMenu = Game1.activeClickableMenu as GameMenu;
                        Game1.activeClickableMenu = new ResourceMenu();
                    }
                    return false;
                }
                return true;
            }
        }
    }
}