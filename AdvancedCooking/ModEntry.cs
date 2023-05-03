using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace AdvancedCooking
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static bool startedWalking;
        private static Dictionary<string, int> currentCookables; 
        private static bool isCookingMenu;
        private static ClickableTextureComponent cookButton;
        private static ClickableTextureComponent fridgeRightButton;
        private static ClickableTextureComponent fridgeLeftButton;
        private static List<Chest> containers;
        private static NetList<Item, NetRef<Item>> ingredients;
        private static Item[] oldIngredients;
        private static InventoryMenu ingredientMenu;
        private static int fridgeIndex = 0;
        private Harmony harmony;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Constructor(typeof(CraftingPage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(List<Chest>) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.CraftingPage_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(CraftingPage), nameof(CraftingPage.receiveLeftClick)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.CraftingPage_receiveLeftClick_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(InventoryMenu), new Type[] { typeof(int), typeof(int), typeof(bool), typeof(IList<Item>), typeof(InventoryMenu.highlightThisItem), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.InventoryMenu_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.drawDialogueBox), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(string), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_drawDialogueBox_Postfix))
            );

        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (Config.EnableMod && Game1.activeClickableMenu is CraftingPage && AccessTools.FieldRefAccess<CraftingPage, bool>(Game1.activeClickableMenu as CraftingPage, "cooking"))
            {
                for(int i = 0; i < oldIngredients.Length; i++)
                {
                    if(ingredients[i] is null != oldIngredients is null || ingredients[i]?.Stack != oldIngredients[i]?.Stack || ingredients[i]?.ParentSheetIndex != oldIngredients[i]?.ParentSheetIndex)
                        UpdateCurrentCookables();
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            currentCookables = null;
            ingredients = new NetList<Item, NetRef<Item>>() { null, null, null, null, null, null, null, null, null, null, null };
            oldIngredients = new Item[]{ null, null, null, null, null, null, null, null, null, null, null };
            ingredients.OnElementChanged += Ingredients_OnElementChanged;
        }

        private void Ingredients_OnElementChanged(NetList<Item, NetRef<Item>> list, int index, Item oldValue, Item newValue)
        {
            UpdateCurrentCookables();
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            if (false && Helper.ModRegistry.IsLoaded("blueberry.LoveOfCooking"))
            {
                harmony.Patch(
                   original: AccessTools.Method("LoveOfCooking.Objects.CookingMenu:DrawActualInventory"),
                   postfix: new HarmonyMethod(typeof(ModEntry), nameof(CookingMenu_DrawActualInventory_Postfix))
                );
            }

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_CookAllModKey_Name"),
                getValue: () => Config.CookAllModKey,
                setValue: value => Config.CookAllModKey = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_StoreOtherHeldItemOnCook_Name"),
                getValue: () => Config.StoreOtherHeldItemOnCook,
                setValue: value => Config.StoreOtherHeldItemOnCook = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ConsumeIngredientsOnFail_Name"),
                getValue: () => Config.ConsumeIngredientsOnFail,
                setValue: value => Config.ConsumeIngredientsOnFail = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ConsumeExtraIngredientsOnSucceed_Name"),
                getValue: () => Config.ConsumeExtraIngredientsOnSucceed,
                setValue: value => Config.ConsumeExtraIngredientsOnSucceed = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_GiveTrashOnFail_Name"),
                getValue: () => Config.GiveTrashOnFail,
                setValue: value => Config.GiveTrashOnFail = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_AllowUnknownRecipes_Name"),
                getValue: () => Config.AllowUnknownRecipes,
                setValue: value => Config.AllowUnknownRecipes = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_LearnUnknownRecipes_Name"),
                getValue: () => Config.LearnUnknownRecipes,
                setValue: value => Config.LearnUnknownRecipes = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ShowProductInfo_Name"),
                getValue: () => Config.ShowProductInfo,
                setValue: value => Config.ShowProductInfo = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ShowCookTooltip_Name"),
                getValue: () => Config.ShowCookTooltip,
                setValue: value => Config.ShowCookTooltip = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ShowProductsInTooltip_Name"),
                getValue: () => Config.ShowProductsInTooltip,
                setValue: value => Config.ShowProductsInTooltip = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_YOffset_Name"),
                getValue: () => Config.YOffset,
                setValue: value => Config.YOffset = value
            );
        }

        private static void DrawCookButtonTooltip()
        {
            List<string> text = new List<string>();
            if (Config.ShowProductsInTooltip)
            {
                var keys = currentCookables.Keys.ToArray();
                for (int i = 0; i < keys.Length; i++)
                {
                    if (i > Config.MaxTypesInTooltip)
                    {
                        text.Add(string.Format(SHelper.Translation.Get("plus-x-more"), keys.Length - i));
                        break;
                    }
                    string[] recipeInfo = CraftingRecipe.cookingRecipes[keys[i]].Split('/');
                    text.Add(string.Format(SHelper.Translation.Get("x-of-y"), !SHelper.Input.IsDown(Config.CookAllModKey) ? 1 : currentCookables[keys[i]], (recipeInfo.Length > 4) ? recipeInfo[4] : keys[i]));
                    if (!SHelper.Input.IsDown(Config.CookAllModKey))
                        break;
                }
            }
            IClickableMenu.drawHoverText(Game1.spriteBatch, string.Join("\n", text), Game1.smallFont, 0, 0, -1, SHelper.Translation.Get("cook"), -1, null, null, 0, -1, -1, -1, -1, 1f, null, null);
        }
        private static void TryCookRecipe(CraftingPage cookingMenu, ref Item heldItem)
        {
            SMonitor.Log("Trying to cook recipe");
            bool succeeded = false;
            bool wouldHaveCooked = false;
            while (true)
            {
                bool keepCooking = false;
                List<string> possible = new List<string>();
                foreach (var name in CraftingRecipe.cookingRecipes.Keys)
                {
                    if (!Config.AllowUnknownRecipes && !Game1.player.cookingRecipes.ContainsKey(name))
                        continue;
                    CraftingRecipe recipe = new CraftingRecipe(name, true);
                    Item crafted = recipe.createItem();
                    if (crafted == null)
                        continue;
                    foreach (var key in recipe.recipeList.Keys)
                    {
                        int need = recipe.recipeList[key];
                        for (int i = 0; i < ingredients.Count; i++)
                        {
                            if (IsCorrectIngredient(ingredients[i], key))
                            {
                                int consume = Math.Min(need, ingredients[i].Stack);
                                need -= consume;
                            }
                            if (need <= 0)
                                break;

                        }

                        if (need > 0)
                            goto nextRecipe;
                    }
                    possible.Add(name);
                nextRecipe:
                    continue;

                }
                possible.Sort(delegate (string a, string b) { return new CraftingRecipe(b, true).recipeList.Count.CompareTo(new CraftingRecipe(a, true).recipeList.Count); });
                foreach (var name in possible)
                {
                    CraftingRecipe recipe = new CraftingRecipe(name, true);
                    Item crafted = recipe.createItem();
                    if (crafted == null)
                        continue;
                    if (heldItem is not null && (!heldItem.Name.Equals(crafted.Name) || !heldItem.getOne().canStackWith(crafted.getOne()) || heldItem.Stack + recipe.numberProducedPerCraft - 1 >= heldItem.maximumStackSize()))
                    {
                        if (Config.StoreOtherHeldItemOnCook)
                        {
                            heldItem = Utility.addItemToThisInventoryList(heldItem, cookingMenu.inventory.actualInventory, 36);
                        }
                        if (heldItem != null)
                        {
                            wouldHaveCooked = true;
                            continue;
                        }
                    }
                    SMonitor.Log($"Cooking recipe {name}");
                    foreach (var key in recipe.recipeList.Keys)
                    {
                        int need = recipe.recipeList[key];
                        for (int i = 0; i < ingredients.Count; i++)
                        {
                            if (IsCorrectIngredient(ingredients[i], key))
                            {
                                int consume = Math.Min(need, ingredients[i].Stack);
                                ingredients[i].Stack -= consume;
                                if (ingredients[i].Stack <= 0)
                                    ingredients[i] = null;
                                need -= consume;
                            }
                            if (need <= 0)
                                break;

                        }
                    }
                    bool seasoned = false;
                    for (int i = 0; i < ingredients.Count; i++)
                    {
                        if (ingredients[i]?.ParentSheetIndex == 917)
                        {
                            ingredients[i].Stack--;
                            if (ingredients[i].Stack <= 0)
                                ingredients[i] = null;
                            seasoned = true;
                            (crafted as Object).Quality = 2;
                            break;
                        }
                    }
                    Game1.playSound("coin");
                    if (heldItem == null)
                    {
                        heldItem = crafted;
                    }
                    else
                    {
                        heldItem.Stack += recipe.numberProducedPerCraft;
                    }
                    if (seasoned)
                    {
                        SMonitor.Log("Added seasoning to recipe");
                        Game1.playSound("breathin");
                    }
                    Game1.player.checkForQuestComplete(null, -1, -1, crafted, null, 2, -1);
                    Game1.player.cookedRecipe(heldItem.ParentSheetIndex);
                    Game1.stats.checkForCookingAchievements();
                    if (Game1.options.gamepadControls && heldItem != null && Game1.player.couldInventoryAcceptThisItem(heldItem))
                    {
                        Game1.player.addItemToInventoryBool(heldItem, false);
                        heldItem = null;
                    }
                    if (Config.LearnUnknownRecipes && !Game1.player.cookingRecipes.ContainsKey(name))
                    {
                        Game1.player.cookingRecipes.Add(name, 0);
                        Game1.playSound("yoba");
                        SMonitor.Log("Added new recipe");
                        Game1.showGlobalMessage(string.Format(SHelper.Translation.Get("new-recipe-x"), name));
                        AccessTools.Method(typeof(CraftingPage), "layoutRecipes").Invoke(cookingMenu, new object[] { CraftingRecipe.cookingRecipes.Keys.ToList() });
                    }
                    succeeded = true;
                    if (SHelper.Input.IsDown(Config.CookAllModKey))
                    {
                        keepCooking = true;
                    }
                    else
                    {
                        UpdateCurrentCookables();
                        return;
                    }
                }
                if (!keepCooking)
                    break;
            }
            if (succeeded)
            {
                if (!SHelper.Input.IsDown(Config.CookAllModKey) && Config.ConsumeExtraIngredientsOnSucceed)
                {
                    for (int i = 0; i < ingredients.Count; i++)
                        ingredients[i] = null;
                }
            }
            else if (wouldHaveCooked)
            {
                Game1.addHUDMessage(new HUDMessage(SHelper.Translation.Get("cannot-cook"), 3));
                Game1.playSound("cancel");
            }
            else
            {
                Game1.addHUDMessage(new HUDMessage(SHelper.Translation.Get("cooking-failed"), 3));
                Game1.playSound("cancel");
                SMonitor.Log("Failed to cook recipe");
                if (!SHelper.Input.IsDown(Config.CookAllModKey))
                {
                    if (Config.GiveTrashOnFail || Config.ConsumeIngredientsOnFail)
                    {
                        for (int i = 0; i < ingredients.Count; i++)
                            ingredients[i] = null;
                        if (Config.GiveTrashOnFail)
                        {
                            Object trash = new Object(168, 1);
                            if (heldItem == null)
                            {
                                heldItem = trash;
                            }
                            else if(heldItem.Name.Equals(trash.Name) && heldItem.getOne().canStackWith(trash.getOne()) && heldItem.Stack < heldItem.maximumStackSize())
                            {
                                heldItem.Stack++;
                            }
                        }
                    }
                }
            }
            UpdateCurrentCookables();
        }

        private static void UpdateCurrentCookables()
        {
            oldIngredients = ingredients.ToArray();
            Dictionary<string, int> dict = new Dictionary<string, int>();
            if (Game1.activeClickableMenu is not CraftingPage)
            {
                currentCookables = dict;
                return;
            }
            List<Item> tempIngredients = new List<Item>();
            foreach(var item in ingredients)
            {
                if(item == null)
                {
                    tempIngredients.Add(null);
                    continue;
                }
                Item clone = item.getOne();
                clone.Stack = item.Stack;
                tempIngredients.Add(clone);
            }
            while (true)
            {
                bool keepCooking = false;
                foreach (var name in CraftingRecipe.cookingRecipes.Keys)
                {
                    if (!Config.AllowUnknownRecipes && !Game1.player.cookingRecipes.ContainsKey(name))
                        continue;
                    CraftingRecipe recipe = new CraftingRecipe(name, true);
                    Item crafted = recipe.createItem();
                    if (crafted == null)
                        continue;
                    foreach (var key in recipe.recipeList.Keys)
                    {
                        int need = recipe.recipeList[key];
                        for (int i = 0; i < tempIngredients.Count; i++)
                        {
                            if (IsCorrectIngredient(tempIngredients[i], key))
                            {
                                int consume = Math.Min(need, tempIngredients[i].Stack);
                                need -= consume;
                            }
                            if (need <= 0)
                                break;
                        }

                        if (need > 0)
                            goto next;
                    }
                    if (dict.ContainsKey(name))
                        dict[name]++;
                    else
                        dict[name] = 1;
                    keepCooking = true;
                    foreach (var key in recipe.recipeList.Keys)
                    {
                        int need = recipe.recipeList[key];
                        for (int i = 0; i < tempIngredients.Count; i++)
                        {
                            if (IsCorrectIngredient(tempIngredients[i], key))
                            {
                                int consume = Math.Min(need, tempIngredients[i].Stack);
                                tempIngredients[i].Stack -= consume;
                                if (tempIngredients[i].Stack <= 0)
                                    tempIngredients[i] = null;
                                need -= consume;
                            }
                            if (need <= 0)
                                break;
                        }
                    }
                next:
                    continue;
                }
                if (!keepCooking)
                    break;
            }
            currentCookables = dict;
        }

        private static void UpdateActualInventory(CraftingPage instance)
        {
            var list = fridgeIndex < 0 ? Game1.player.Items : instance._materialContainers[Math.Min(instance._materialContainers.Count - 1 ,fridgeIndex)].items;

            for (int i = 0; i < Game1.player.maxItems.Value; i++)
            {
                if (list.Count <= i)
                    list.Add(null);
            }
            instance.inventory.actualInventory = list;
        }
        private static bool IsCorrectIngredient(Item item, int key)
        {
            Object obj = item as Object;

            if (obj is null)
                return false;
            else
                return !obj.bigCraftable.Value && (obj.ParentSheetIndex == key || obj.Category == key || CraftingRecipe.isThereSpecialIngredientRule(obj, key));
        }
    }
}