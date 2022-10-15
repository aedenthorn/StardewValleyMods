using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace AdvancedLootFramework
{
    public class ModEntry : Mod 
    {
        public static ModEntry context;
        private static ModConfig Config;
        private static Random myRand;
        private static IMonitor SMonitor;
        private static IModHelper SHelper;

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            SMonitor = Monitor;
            SHelper = Helper;

            myRand = new Random();

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.ShowMenu)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Chest_ShowMenu_Prefix))
            );

        }
        public override object GetApi()
        {
            return new AdvancedLootFrameworkApi();
        }

        private static void Chest_ShowMenu_Prefix(Chest __instance)
        {
            if (!__instance.playerChest.Value || __instance.coins.Value <= 0)
                return;
            context.Monitor.Log($"Giving {__instance.coins} gold to player from chest");
            Game1.player.Money += __instance.coins.Value;
            __instance.coins.Value = 0;
        }

        public static List<object> LoadPossibleTreasures(string[] includeList, int minItemValue, int maxItemValue)
        {
            List<object> treasures = new List<object>();

            int currentCount = 0;
            if (includeList.Contains("MeleeWeapon"))
            {
                foreach (KeyValuePair<int, string> kvp in Game1.content.Load<Dictionary<int, string>>("Data\\weapons"))
                {
                    if (Config.ForbiddenWeapons.Contains(kvp.Key))
                        continue;
                    int price = new MeleeWeapon(kvp.Key).salePrice();
                    if (CanAddTreasure(price, minItemValue, maxItemValue))
                        treasures.Add(new Treasure(kvp.Key, price, "MeleeWeapon"));
                }
                //context.Monitor.Log($"Added {treasures.Count - currentCount} weapons");
                currentCount = treasures.Count;
            }
            if (includeList.Contains("Shirt") || includeList.Contains("Pants"))
            {
                foreach (KeyValuePair<int, string> kvp in Game1.clothingInformation)
                {
                    int price = 0;
                    if (includeList.Contains("Shirt") && kvp.Value.Split('/')[8].ToLower().Trim() == "shirt")
                    {
                        price = Convert.ToInt32(kvp.Value.Split('/')[5]);
                        if (CanAddTreasure(price, minItemValue, maxItemValue))
                            treasures.Add(new Treasure(kvp.Key, price, "Shirt"));
                    }
                    else if (includeList.Contains("Pants") && kvp.Value.Split('/')[8].ToLower().Trim() == "pants")
                    {
                        price = Convert.ToInt32(kvp.Value.Split('/')[5]);
                        if (CanAddTreasure(price, minItemValue, maxItemValue))
                            treasures.Add(new Treasure(kvp.Key, price, "Pants"));
                    }
                    else
                        continue;
                }
                //Monitor.Log($"Added {treasures.Count - currentCount} clothes");
                currentCount = treasures.Count;
            }
            if (includeList.Contains("Hat"))
            {
                foreach (KeyValuePair<int, string> kvp in Game1.content.Load<Dictionary<int, string>>("Data\\hats"))
                {
                    if (CanAddTreasure(1000, minItemValue, maxItemValue))
                        treasures.Add(new Treasure(kvp.Key, 1000, "Hat"));
                }

                //Monitor.Log($"Added {treasures.Count - currentCount} hats");
                currentCount = treasures.Count;
            }
            if (includeList.Contains("Boots"))
            {
                foreach (KeyValuePair<int, string> kvp in Game1.content.Load<Dictionary<int, string>>("Data\\Boots"))
                {
                    int price = Convert.ToInt32(kvp.Value.Split('/')[2]);
                    if (CanAddTreasure(price, minItemValue, maxItemValue))
                        treasures.Add(new Treasure(kvp.Key, price, "Boots"));
                }
                //Monitor.Log($"Added {treasures.Count - currentCount} boots");
                currentCount = treasures.Count;
            }

            if (includeList.Contains("BigCraftable"))
            {
                foreach (KeyValuePair<int, string> kvp in Game1.bigCraftablesInformation)
                {
                    if (Config.ForbiddenBigCraftables.Contains(kvp.Key))
                        continue;
                    //int price = GetStorePrice(kvp.Key);
                    //if(price == 0)
                    
                    int price = new Object(Vector2.Zero, kvp.Key, false).Price;
                    if (CanAddTreasure(price, minItemValue, maxItemValue))
                        treasures.Add(new Treasure(kvp.Key, price, "BigCraftable"));
                }
                //Monitor.Log($"Added {treasures.Count - currentCount} boots");
                currentCount = treasures.Count; 
            }

            foreach (KeyValuePair<int, string> kvp in Game1.objectInformation)
            {
                if (Config.ForbiddenObjects.Contains(kvp.Key))
                    continue;
                var split = kvp.Value.Split('/');
                if (split[5] == "...")
                    continue;
                string type = "";
                if (!int.TryParse(split[1], out int price))
                {
                    SMonitor.Log($"Warning, invalid price for {kvp.Value}", LogLevel.Warn);
                    continue;
                }
                if (includeList.Contains("Ring") && split[3] == "Ring")
                {
                    type = "Ring";
                }
                else if (includeList.Contains("Cooking") && split[3].StartsWith("Cooking"))
                {
                    type = "Cooking";
                }
                else if (includeList.Contains("Seed") && split[3].StartsWith("Seeds"))
                {
                    type = "Seed";
                }
                else if (includeList.Contains("Mineral") && split[3].StartsWith("Mineral"))
                {
                    type = "Mineral";
                }
                else if (includeList.Contains("Fish") && split[3].StartsWith("Fish"))
                {
                    type = "Fish";
                }
                else if (includeList.Contains("Relic") && split[3].StartsWith("Arch"))
                {
                    type = "Relic";
                }
                else if (includeList.Contains("BasicObject") && split[3].StartsWith("Basic"))
                {
                    type = "BasicObject";
                }
                else
                    continue;
                if (CanAddTreasure(price, minItemValue, maxItemValue))
                    treasures.Add(new Treasure(kvp.Key, price, type));
            }
            //Monitor.Log($"Added {treasures.Count - currentCount} objects"); 
            return treasures;
        }

        private static int GetStorePrice(int idx)
        {
            Dictionary<ISalable, int[]> stock = Utility.getAnimalShopStock();
            KeyValuePair<ISalable, int[]> item = stock.FirstOrDefault(i => (i.Key as Item)?.ParentSheetIndex == idx);
            if ((item.Key as Item)?.ParentSheetIndex == idx)
            {
                SMonitor.Log($"Setting price for {idx} at {item.Value[0]} from animal shop");
                return item.Value[0];
            }

            stock = Utility.getCarpenterStock();
            item = stock.FirstOrDefault(i => (i.Key as Item)?.ParentSheetIndex == idx);
            if ((item.Key as Item)?.ParentSheetIndex == idx)
            {
                SMonitor.Log($"Setting price for {idx} at {item.Value[0]} from carpenter shop");
                return item.Value[0];
            }

            stock = Utility.getCarpenterStock();
            item = stock.FirstOrDefault(i => (i.Key as Item)?.ParentSheetIndex == idx);
            if ((item.Key as Item)?.ParentSheetIndex == idx)
            {
                SMonitor.Log($"Setting price for {idx} at {item.Value[0]} from carpenter shop");
                return item.Value[0];
            }

            stock = Utility.getDwarfShopStock();
            item = stock.FirstOrDefault(i => (i.Key as Item)?.ParentSheetIndex == idx);
            if ((item.Key as Item)?.ParentSheetIndex == idx)
            {
                SMonitor.Log($"Setting price for {idx} at {item.Value[0]} from dawrf shop");
                return item.Value[0];
            }

            stock = Utility.getDwarfShopStock();
            item = stock.FirstOrDefault(i => (i.Key as Item)?.ParentSheetIndex == idx);
            if ((item.Key as Item)?.ParentSheetIndex == idx)
            {
                SMonitor.Log($"Setting price for {idx} at {item.Value[0]} from dawrf shop");
                return item.Value[0];
            }


            return 0;
        }

        public static List<Item> GetChestItems(List<object> treasures, Dictionary<string, int> itemChances, int maxItems, int minItemValue, int maxItemValue, int mult, float increaseRate, int baseValue)
        {

            // shuffle list

            int n = treasures.Count;
            while (n > 1)
            {
                n--;
                int k = myRand.Next(n + 1);
                var value = treasures[k]; 
                treasures[k] = treasures[n];
                treasures[n] = value;
            }

            List<Item> chestItems = new List<Item>();

            double maxValue = Math.Pow(mult, increaseRate) * baseValue;

            //SMonitor.Log($"Max chest value: {maxValue}");

            int currentValue = 0;

            foreach (Treasure t in treasures)
            {
                if (CanAddTreasure(t.value, minItemValue, maxItemValue) && currentValue + t.value <= maxValue)
                {
                    //SMonitor.Log($"adding {t.type} {t.index} {t.value} to chest");
                    switch (t.type)
                    {
                        case "MeleeWeapon":
                            if(myRand.NextDouble() < itemChances["MeleeWeapon"] / 100f)
                                chestItems.Add(new MeleeWeapon(t.index));
                            break;
                        case "Shirt":
                            if (myRand.NextDouble() < itemChances["Shirt"] / 100f)
                                chestItems.Add(new Clothing(t.index));
                            break;
                        case "Pants":
                            if (myRand.NextDouble() < itemChances["Pants"] / 100f)
                                chestItems.Add(new Clothing(t.index));
                            break;
                        case "Boots":
                            if (myRand.NextDouble() < itemChances["Boots"] / 100f)
                                chestItems.Add(new Boots(t.index));
                            break;
                        case "Hat":
                            if (myRand.NextDouble() < itemChances["Hat"] / 100f)
                                chestItems.Add(new Hat(t.index));
                            break;
                        case "Ring":
                            if (myRand.NextDouble() < itemChances["Ring"] / 100f)
                                chestItems.Add(new Ring(t.index));
                            break;
                        case "BigCraftable":
                            if (myRand.NextDouble() < itemChances["BigCraftable"] / 100f)
                                chestItems.Add(new Object(Vector2.Zero, t.index, false));
                            break;
                        default:
                            int number = GetNumberOfObjects(t.value, maxValue - currentValue);
                            chestItems.Add(new Object(t.index, number));
                            currentValue += t.value * (number - 1);
                            break;
                    }
                    currentValue += t.value;
                }
                if (maxValue - currentValue < minItemValue || chestItems.Count >= maxItems)
                    break;
            }
            SMonitor.Log($"chest contains {chestItems.Count} items valued at {currentValue}");
            return chestItems;
        }

        public static int GetChestCoins(int mult, float increaseRate, int baseMin, int baseMax)
        {
            int coins = (int)Math.Round(Math.Pow(mult, increaseRate) * myRand.Next(baseMin, baseMax));
            SMonitor.Log($"chest contains {coins} coins");
            return coins;
        }

        public static Chest MakeChest(List<Item> chestItems, int coins, Vector2 chestSpot)
        {
            Chest chest = new Chest(true);
            chest.coins.Value = coins;
            chest.items.Clear();
            chest.items.AddRange(chestItems);
            chest.tileLocation.Value = chestSpot;
            chest.bigCraftable.Value = true;
            chest.modData["Pathoschild.ChestsAnywhere/IsIgnored"] = "true";
            chest.modData["Pathoschild.Automate/StoreItems"] = "Disable";
            chest.modData["Pathoschild.Automate/TakeItems"] = "Disable";
            chest.modData["aedenthorn.AdvancedLootFramework/IsAdvancedLootFrameworkChest"] = "true";
            return chest;
        }

        private static int GetNumberOfObjects(int value, double maxValue)
        {
            return myRand.Next(1, (int) Math.Floor(maxValue / value));
        }
        

        private static bool CanAddTreasure(int price, int min, int max)
        {

            if (min > 0 && min > price)
                return false;
            if (max > 0 && max < price)
                return false;
            return true;
        }

    }
}
