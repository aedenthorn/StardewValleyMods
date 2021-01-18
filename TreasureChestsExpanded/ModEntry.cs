using Harmony;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace TreasureChestsExpanded
{
    public class ModEntry : Mod 
	{
		public static ModEntry context;

		private static ModConfig Config;
        private static Random myRand;
        private static IMonitor SMonitor;
        private static IModHelper SHelper;
        public static List<Treasure> treasures = new List<Treasure>();

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

			harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "addLevelChests"),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(MineShaft_addLevelChests_postfix))
			);
			harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.ShowMenu)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Chest_ShowMenu_Prefix))
			);

		}
        public override object GetApi()
        {
            return new TreasureChestsExpandedApi();
        }
        private static void Chest_ShowMenu_Prefix(Chest __instance)
        {
            if (!__instance.playerChest || __instance.coins <= 0)
                return;
            Game1.player.Money += __instance.coins;
            __instance.coins.Value = 0;
        }

        private static void MineShaft_addLevelChests_postfix(MineShaft __instance)
        {
            if (__instance.mineLevel < 121)
                return;

            Vector2 chestSpot = new Vector2(9f, 9f);

            NetBool treasureRoom = SHelper.Reflection.GetField<NetBool>(__instance, "netIsTreasureRoom").GetValue();

            if (treasureRoom.Value && __instance.overlayObjects.ContainsKey(chestSpot))
            {
                List<Item> chestItems = GetChestItems(__instance.mineLevel - 120);

                int coins = GetChestCoins(__instance.mineLevel - 120);

                __instance.overlayObjects[chestSpot] = MakeChest(chestItems, coins, chestSpot);
            }
        }

        public static List<Item> GetChestItems(int mult)
        {

            List<Treasure> list = new List<Treasure>(treasures);

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

            List<Item> chestItems = new List<Item>();

            double maxValue = Math.Pow(mult, Config.TreasureIncreaseRate) * Config.TreasureBaseValue;
            int currentValue = 0;

            foreach (Treasure t in list)
            {
                if (currentValue + t.value <= maxValue)
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
                            int number = GetNumberOfObjects(t.value, maxValue);
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

        public static int GetChestCoins(int mult)
        {
            int coins = (int)Math.Round(Math.Pow(mult, Config.CoinsIncreaseRate) * myRand.Next(Config.BaseCoinsMin, Config.BaseCoinsMax));
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
            return chest;
        }

        private static int GetNumberOfObjects(int value, double maxValue)
        {
            return myRand.Next(1, (int) Math.Floor(maxValue / value));
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            LoadTreasures();
        }


        private void LoadTreasures()
        {
            int currentCount = 0;
            if (Config.IncludeWeapons)
            {
                foreach(KeyValuePair<int,string> kvp in Game1.content.Load<Dictionary<int, string>>("Data\\weapons"))
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
