using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;
using Object = StardewValley.Object;

namespace FishingChestsExpanded
{
    public class ModEntry : Mod 
	{
		public static ModEntry context;

		private static ModConfig Config;
        private static System.Random myRand;
        private static IMonitor SMonitor;
        private static IModHelper SHelper;
        private static List<Treasure> treasures = new List<Treasure>();

        public override void Entry(IModHelper helper)
		{
            context = this;
			Config = Helper.ReadConfig<ModConfig>();
			if (!Config.EnableMod)
				return;

            SMonitor = Monitor;
            SHelper = Helper;

            myRand = new Random();

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

			var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

            ConstructorInfo ci = typeof(ItemGrabMenu).GetConstructor(new Type[] { typeof(IList<Item>), typeof(object) });
            harmony.Patch(
               original: ci,
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.ItemGrabMenu_Prefix))
            );

        }

        private static void ItemGrabMenu_Prefix(ref IList<Item> inventory, object context)
        {
            if (!(context is FishingRod))
                return;
            FishingRod fr = context as FishingRod;
            int fish = SHelper.Reflection.GetField<int>(fr, "whichFish").GetValue();
            Dictionary<int, string> data = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
            int difficulty = 5;
            if(data.ContainsKey(fish))
                int.TryParse(data[fish].Split('/')[1], out difficulty);

            int coins = (int)Math.Round(Math.Pow(difficulty, Config.CoinsIncreaseRate) * myRand.Next(Config.BaseCoinsMin, Config.BaseCoinsMax));

            IList<Item> items = GetTreasure(difficulty);
            foreach(Item item in inventory)
            {
                if (item.parentSheetIndex == fish)
                    items.Add(item);
            }
            inventory = items;
            Game1.player.Money += coins;
            SMonitor.Log($"chest contains {coins} gold");
        }

        private static IList<Item> GetTreasure(int difficulty)
        {
            double maxValue = Math.Pow(difficulty, Config.TreasureIncreaseRate) * Config.TreasureBaseValue;
            int currentValue = 0;
            List<Item> chestItems = new List<Item>();

            IList<Treasure> list = new List<Treasure>(treasures);

            // shuffle list

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = myRand.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            foreach (Treasure t in list)
            {
                if(currentValue + t.value <= maxValue)
                {
                    SMonitor.Log($"adding {t.type} {t.index} {t.value} to chest");
                    switch (t.type)
                    {
                        case "MeleeWeapon":
                            chestItems.Add(new MeleeWeapon(t.index));
                            break;
                        case "Clothing":
                            chestItems.Add(new Clothing(t.index));
                            break;
                        case "Boots":
                            chestItems.Add(new Boots(t.index));
                            break;
                        case "Hat":
                            chestItems.Add(new Hat(t.index));
                            break;
                        case "Ring":
                            chestItems.Add(new Ring(t.index));
                            break;
                        case "BigCraftable":
                            chestItems.Add(new Object(Vector2.Zero, t.index, false));
                            break;
                        default:
                            int number = GetNumberOfObjects(t.value,maxValue);
                            chestItems.Add(new Object(t.index, number));
                            currentValue += t.value * number;
                            continue;
                    }
                    currentValue += t.value;
                }
                if (maxValue - currentValue < Config.ItemMinValue)
                    break;
            }
            SMonitor.Log($"chest contains {chestItems.Count} items valued at {currentValue}");

            return chestItems;
        }

        private static int GetNumberOfObjects(int value, double maxValue)
        {
            return myRand.Next(1, (int) Math.Floor(maxValue / value));
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            FishingRod.baseChanceForTreasure = Config.BaseTreasureChance;
            LoadTreasures();
        }


        private void LoadTreasures()
        {
            int currentCount = 0;
            if (Config.IncludeWeapons)
            {
                foreach (KeyValuePair<int, string> kvp in Game1.content.Load<Dictionary<int, string>>("Data\\weapons"))
                {
                    int price = new MeleeWeapon(kvp.Key).salePrice();
                    TryToAddTreasure(kvp.Key, price, "MeleeWeapon");
                }
                Monitor.Log($"Added {treasures.Count - currentCount} weapons");
                currentCount = treasures.Count;
            }
            if (Config.IncludeShirts || Config.IncludePants)
            {
                foreach (KeyValuePair<int, string> kvp in Game1.clothingInformation)
                {
                    int price = 0;
                    if (Config.IncludeShirts && kvp.Value.Split('/')[8].ToLower().Trim() == "shirt")
                    {
                        price = Convert.ToInt32(kvp.Value.Split('/')[5]);
                    }
                    else if (Config.IncludePants && kvp.Value.Split('/')[8].ToLower().Trim() == "pants")
                    {
                        price = Convert.ToInt32(kvp.Value.Split('/')[5]);
                    }
                    else
                        continue;

                    TryToAddTreasure(kvp.Key, price, "Clothing");
                }
                Monitor.Log($"Added {treasures.Count - currentCount} clothes");
                currentCount = treasures.Count;
            }
            if (Config.IncludeHats)
            {
                foreach (KeyValuePair<int, string> kvp in Game1.content.Load<Dictionary<int, string>>("Data\\hats"))
                {
                    TryToAddTreasure(kvp.Key, 1000, "Hat");
                }

                Monitor.Log($"Added {treasures.Count - currentCount} hats");
                currentCount = treasures.Count;
            }
            if (Config.IncludeBoots)
            {
                foreach (KeyValuePair<int, string> kvp in Game1.content.Load<Dictionary<int, string>>("Data\\Boots"))
                {
                    int price = Convert.ToInt32(kvp.Value.Split('/')[2]);
                    TryToAddTreasure(kvp.Key, price, "Boots");
                }
                Monitor.Log($"Added {treasures.Count - currentCount} boots");
                currentCount = treasures.Count;
            }

            if (Config.IncludeBigCraftables)
            {
                foreach (KeyValuePair<int, string> kvp in Game1.bigCraftablesInformation)
                {
                    TryToAddTreasure(kvp.Key, new Object(Vector2.Zero, kvp.Key, false).sellToStorePrice(), "BigCraftable");
                }
                Monitor.Log($"Added {treasures.Count - currentCount} boots");
                currentCount = treasures.Count;
            }

            foreach (KeyValuePair<int, string> kvp in Game1.objectInformation)
            {
                if (kvp.Value.Split('/')[5] == "...")
                    continue;
                if (Config.IncludeRings && kvp.Value.Split('/')[3] == "Ring")
                {
                    int price = Convert.ToInt32(kvp.Value.Split('/')[1]);
                    TryToAddTreasure(kvp.Key, price, "Ring");
                }
                else if (Config.IncludeFood && kvp.Value.Split('/')[3].StartsWith("Cooking"))
                {
                    int price = Convert.ToInt32(kvp.Value.Split('/')[1]);
                    TryToAddTreasure(kvp.Key, price, "Cooking");
                }
                else if (Config.IncludeSeeds && kvp.Value.Split('/')[3].StartsWith("Seeds"))
                {
                    int price = Convert.ToInt32(kvp.Value.Split('/')[1]);
                    TryToAddTreasure(kvp.Key, price, "Seeds");
                }
                else if (Config.IncludeMinerals && kvp.Value.Split('/')[3].StartsWith("Mineral"))
                {
                    int price = Convert.ToInt32(kvp.Value.Split('/')[1]);
                    TryToAddTreasure(kvp.Key, price, "Mineral");
                }
                else if (Config.IncludeFish && kvp.Value.Split('/')[3].StartsWith("Fish"))
                {
                    int price = Convert.ToInt32(kvp.Value.Split('/')[1]);
                    TryToAddTreasure(kvp.Key, price, "Fish");
                }
                else if (Config.IncludeRelics && kvp.Value.Split('/')[3].StartsWith("Arch"))
                {
                    int price = Convert.ToInt32(kvp.Value.Split('/')[1]);
                    TryToAddTreasure(kvp.Key, price, "Arch");
                }
                else if (Config.IncludeBasicObjects && kvp.Value.Split('/')[3].StartsWith("Basic"))
                {
                    int price = Convert.ToInt32(kvp.Value.Split('/')[1]);
                    TryToAddTreasure(kvp.Key, price, "Basic");
                }
            }
            Monitor.Log($"Added {treasures.Count - currentCount} objects");
        }

        private void TryToAddTreasure(int index, int price, string type)
        {
            if (Config.ItemMinValue > 0 && Config.ItemMinValue > price)
                return;
            if (Config.ItemMaxValue > 0 && Config.ItemMaxValue < price)
                return;
            treasures.Add(new Treasure(index, price, type));
        }

	}
}
