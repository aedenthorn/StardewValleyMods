using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FishingChestsExpanded
{
    public class ModEntry : Mod 
    {
        public static ModEntry context;

        private static ModConfig Config;
        private static IMonitor SMonitor;
        private static IModHelper SHelper;
        private static List<object> treasuresList = new List<object>();
        private static IAdvancedLootFrameworkApi advancedLootFrameworkApi;

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            SMonitor = Monitor;
            SHelper = Helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

            ConstructorInfo ci = typeof(ItemGrabMenu).GetConstructor(new Type[] { typeof(IList<Item>), typeof(object) });
            harmony.Patch(
               original: ci,
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.ItemGrabMenu_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.startMinigameEndFunction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.startMinigameEndFunction_Prefix))
            );

        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            advancedLootFrameworkApi = context.Helper.ModRegistry.GetApi<IAdvancedLootFrameworkApi>("aedenthorn.AdvancedLootFramework");
            if (advancedLootFrameworkApi != null)
            {
                Monitor.Log($"loaded AdvancedLootFramework API", LogLevel.Debug);
            }
            treasuresList = advancedLootFrameworkApi.LoadPossibleTreasures(Config.ItemListTypes, Config.MinItemValue, Config.MaxItemValue);
            Monitor.Log($"Got {treasuresList.Count} possible treasures");
        }

        private static void startMinigameEndFunction_Prefix() 
        {
            if(Config.BaseChanceForTreasureChest > 0)
                FishingRod.baseChanceForTreasure = Config.BaseChanceForTreasureChest;
        }

        private static void ItemGrabMenu_Prefix(ref IList<Item> inventory, object context)
        {
            if (!(context is FishingRod))
                return;

            FishingRod fr = context as FishingRod;
            int fish = SHelper.Reflection.GetField<int>(fr, "whichFish").GetValue();
            bool treasure = false;
            foreach (Item item in inventory)
            {
                if (item.parentSheetIndex != fish)
                    treasure = true;
            }
            if (!treasure)
                return;

            Dictionary<int, string> data = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
            int difficulty = 5;
            if(data.ContainsKey(fish))
                int.TryParse(data[fish].Split('/')[1], out difficulty);

            int coins = advancedLootFrameworkApi.GetChestCoins(difficulty, Config.IncreaseRate, Config.CoinBaseMin, Config.CoinBaseMax);

            IList<Item> items = advancedLootFrameworkApi.GetChestItems(treasuresList, Config.MaxItems, Config.MinItemValue, Config.MaxItemValue, difficulty, Config.IncreaseRate, Config.ItemsBaseMaxValue);
            foreach(Item item in inventory)
            {
                if (item.parentSheetIndex == fish)
                    items.Add(item);
            }
            inventory = items;
            Game1.player.Money += coins;
            SMonitor.Log($"chest contains {coins} gold");
        }
    }
}
