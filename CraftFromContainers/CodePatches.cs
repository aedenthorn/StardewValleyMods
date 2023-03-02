using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace CraftFromContainers
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.doesFarmerHaveIngredientsInInventory))]
        public class CraftingRecipe_doesFarmerHaveIngredientsInInventory_Patch
        {
            public static void Postfix(CraftingRecipe __instance, IList<Item> extraToCheck, ref bool __result)
            {
                if (!Config.EnableMod || __result || (Config.ToggleButton != SButton.None && !SHelper.Input.IsDown(Config.ToggleButton)) || !IsValidToPull())
                    return;
                __result = CheckAllAmounts(__instance.recipeList.ToList(), extraToCheck);
            }
        }
        [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.DoesFarmerHaveAdditionalIngredientsInInventory))]
        public class CraftingRecipe_DoesFarmerHaveAdditionalIngredientsInInventory_Patch
        {
            public static void Postfix(CraftingRecipe __instance, List<KeyValuePair<int, int>> additional_recipe_items, IList<Item> extraToCheck, ref bool __result)
            {
                if (!Config.EnableMod || __result || (Config.ToggleButton != SButton.None && !SHelper.Input.IsDown(Config.ToggleButton)) || !IsValidToPull())
                    return;
                __result = CheckAllAmounts(additional_recipe_items, extraToCheck);
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.hasItemInInventory))]
        public class Farmer_hasItemInInventory_Patch
        {
            public static void Postfix(Farmer __instance, int itemIndex, int quantity, int minPrice, ref bool __result)
            {
                if (!Config.EnableMod || __result || itemIndex == 858 || itemIndex == 73 || (Config.ToggleButton != SButton.None && !SHelper.Input.IsDown(Config.ToggleButton)) || !IsValidToPull())
                    return;
                var remain = quantity - __instance.getItemCount(itemIndex, minPrice);
                __result = CheckAmount(itemIndex, remain, minPrice);
            }
        }
        [HarmonyPatch(typeof(BluePrint), nameof(BluePrint.consumeResources))]
        public class BluePrint_consumeResources_Patch
        {
            public static bool Prefix(BluePrint __instance)
            {
                if (!Config.EnableMod || (Config.ToggleButton != SButton.None && !SHelper.Input.IsDown(Config.ToggleButton)) || !IsValidToPull())
                    return true;
                Dictionary<int, int> missing = new();
                foreach (var kvp in __instance.itemsRequired)
                {
                    var count = Game1.player.getItemCount(kvp.Key);
                    if(count < kvp.Value)
                    {
                        missing[kvp.Key] = kvp.Value - count;
                    }
                    Game1.player.consumeObject(kvp.Key, kvp.Value);
                }
                ConsumeMissing(missing);
                Game1.player.Money -= __instance.moneyRequired;
                return false;
            }
        }
        [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.drawRecipeDescription))]
        public class CraftingRecipe_drawRecipeDescription_Patch
        {
            public static void Prefix(CraftingRecipe __instance, ref IList<Item> additional_crafting_items)
            {
                if (!Config.EnableMod || (Config.ToggleButton != SButton.None && !SHelper.Input.IsDown(Config.ToggleButton)) || !IsValidToPull())
                    return;
                if(additional_crafting_items is null)
                    additional_crafting_items = new List<Item>();
                foreach (var kvp in __instance.recipeList)
                {
                    foreach (NetObjectList<Item> items in GetContainers())
                    {
                        var amount = Game1.player.getItemCountInList(items, kvp.Key, 8);
                        if (amount <= 0)
                            continue;
                        additional_crafting_items.Add(new Object(kvp.Key, amount));
                    }
                }
                return;
            }
        }
        [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.consumeIngredients))]
        public class CraftingRecipe_consumeIngredients_Patch
        {
            public static bool Prefix(CraftingRecipe __instance, List<Chest> additional_materials)
            {
                if (!Config.EnableMod || (Config.ToggleButton != SButton.None && !SHelper.Input.IsDown(Config.ToggleButton)) || !IsValidToPull())
                    return true;
                ConsumeAll(__instance.recipeList.ToList(), additional_materials);
                return false;
            }
        }
        [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.ConsumeAdditionalIngredients))]
        public class CraftingRecipe_ConsumeAdditionalIngredients_Patch
        {
            public static bool Prefix(CraftingRecipe __instance, List<KeyValuePair<int, int>> additional_recipe_items, List<Chest> additional_materials)
            {
                if (!Config.EnableMod || (Config.ToggleButton != SButton.None && !SHelper.Input.IsDown(Config.ToggleButton)) || !IsValidToPull())
                    return true;
                ConsumeAll(additional_recipe_items, additional_materials);
                return false;
            }

        }
    }
}