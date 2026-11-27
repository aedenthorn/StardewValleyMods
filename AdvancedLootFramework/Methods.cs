using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Shirts;
using StardewValley.GameData.Weapons;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace AdvancedLootFramework
{
	public partial class ModEntry
	{
		public static List<object> LoadPossibleTreasures(string[] includeList, int minItemValue, int maxItemValue)
		{
			List<object> treasures = new();

			if (includeList.Contains("MeleeWeapon"))
			{
				foreach (KeyValuePair<string, WeaponData> kvp in Game1.weaponData)
				{
					if (!Config.ForbiddenWeapons.Contains(kvp.Key))
					{
						int price = new MeleeWeapon(kvp.Key).salePrice();

						if (CanAddTreasure(price, minItemValue, maxItemValue))
						{
							treasures.Add(new Treasure(kvp.Key, price, "MeleeWeapon"));
						}
					}
				}
			}
			if (includeList.Contains("Hat"))
			{
				foreach (KeyValuePair<string, string> kvp in Game1.content.Load<Dictionary<string, string>>("Data\\hats"))
				{
					if (CanAddTreasure(1000, minItemValue, maxItemValue))
					{
						treasures.Add(new Treasure(kvp.Key, 1000, "Hat"));
					}
				}
			}
			if (includeList.Contains("Shirt"))
			{
				foreach (KeyValuePair<string, ShirtData> kvp in Game1.shirtData)
				{
					int price = kvp.Value.Price;

					if (CanAddTreasure(price, minItemValue, maxItemValue))
					{
						treasures.Add(new Treasure(kvp.Key, price, "Shirt"));
					}
				}
			}
			if (includeList.Contains("Pants"))
			{
				foreach (KeyValuePair<string, PantsData> kvp in Game1.pantsData)
				{
					int price = kvp.Value.Price;

					if (CanAddTreasure(price, minItemValue, maxItemValue))
					{
						treasures.Add(new Treasure(kvp.Key, price, "Pants"));
					}
				}
			}
			if (includeList.Contains("Boots"))
			{
				foreach (KeyValuePair<string, string> kvp in Game1.content.Load<Dictionary<string, string>>("Data\\Boots"))
				{
					int price = Convert.ToInt32(kvp.Value.Split('/')[2]);

					if (CanAddTreasure(price, minItemValue, maxItemValue))
					{
						treasures.Add(new Treasure(kvp.Key, price, "Boots"));
					}
				}
			}
			if (includeList.Contains("BigCraftable"))
			{
				foreach (KeyValuePair<string, BigCraftableData> kvp in Game1.bigCraftableData)
				{
					if (!Config.ForbiddenBigCraftables.Contains(kvp.Key))
					{
						int price = kvp.Value.Price;

						if (CanAddTreasure(price, minItemValue, maxItemValue))
						{
							treasures.Add(new Treasure(kvp.Key, price, "BigCraftable"));
						}
					}
				}
			}
			foreach (KeyValuePair<string, ObjectData> kvp in Game1.objectData)
			{
				if (Config.ForbiddenObjects.Contains(kvp.Key) || kvp.Key.Equals("GoldCoin"))
					continue;

				string type;
				int price = new Object(kvp.Key, 1).salePrice();

				if (includeList.Contains("Ring") && kvp.Value.Type == "Ring")
				{
					type = "Ring";
				}
				else if (includeList.Contains("Cooking") && kvp.Value.Type == "Cooking")
				{
					type = "Cooking";
				}
				else if (includeList.Contains("Seed") && kvp.Value.Type == "Seeds")
				{
					type = "Seed";
				}
				else if (includeList.Contains("Mineral") && kvp.Value.Type == "Minerals")
				{
					type = "Mineral";
				}
				else if (includeList.Contains("Fish") && kvp.Value.Type == "Fish")
				{
					type = "Fish";
				}
				else if (includeList.Contains("Relic") && kvp.Value.Type == "Arch")
				{
					type = "Relic";
				}
				else if (includeList.Contains("BasicObject") && kvp.Value.Type == "Basic")
				{
					type = "BasicObject";
				}
				else
				{
					continue;
				}
				if (CanAddTreasure(price, minItemValue, maxItemValue))
				{
					treasures.Add(new Treasure(kvp.Key, price, type));
				}
			}
			return treasures;
		}

		public static List<Item> GetChestItems(List<object> treasures, Dictionary<string, int> itemChances, int maxItems, int minItemValue, int maxItemValue, int mult, float increaseRate, int baseValue)
		{
			// shuffle list
			int n = treasures.Count;

			while (n-- > 1)
			{
				int k = myRand.Next(n + 1);

				(treasures[n], treasures[k]) = (treasures[k], treasures[n]);
			}

			List<Item> chestItems = new();
			double maxValue = Math.Pow(mult, increaseRate) * baseValue;
			int currentValue = 0;

			foreach (Treasure treasure in treasures.Cast<Treasure>())
			{
				if (CanAddTreasure(treasure.value, minItemValue, maxItemValue) && currentValue + treasure.value <= maxValue)
				{
					switch (treasure.type)
					{
						case "MeleeWeapon":
							if (myRand.NextDouble() < itemChances["MeleeWeapon"] / 100f)
							{
								chestItems.Add(new MeleeWeapon(treasure.id));
							}
							break;
						case "Shirt":
							if (myRand.NextDouble() < itemChances["Shirt"] / 100f)
							{
								chestItems.Add(new Clothing(treasure.id));
							}
							break;
						case "Pants":
							if (myRand.NextDouble() < itemChances["Pants"] / 100f)
							{
								chestItems.Add(new Clothing(treasure.id));
							}
							break;
						case "Boots":
							if (myRand.NextDouble() < itemChances["Boots"] / 100f)
							{
								chestItems.Add(new Boots(treasure.id));
							}
							break;
						case "Hat":
							if (myRand.NextDouble() < itemChances["Hat"] / 100f)
							{
								chestItems.Add(new Hat(treasure.id));
							}
							break;
						case "Ring":
							if (myRand.NextDouble() < itemChances["Ring"] / 100f)
							{
								chestItems.Add(new Ring(treasure.id));
							}
							break;
						case "BigCraftable":
							if (myRand.NextDouble() < itemChances["BigCraftable"] / 100f)
							{
								chestItems.Add(new Object(Vector2.Zero, treasure.id, false));
							}
							break;
						default:
							int number = GetNumberOfObjects(treasure.value, maxValue - currentValue);

							chestItems.Add(new Object(treasure.id, number));
							currentValue += treasure.value * (number - 1);
							break;
					}
					currentValue += treasure.value;
				}
				if (maxValue - currentValue < minItemValue || chestItems.Count >= maxItems)
					break;
			}
			//SMonitor.Log($"chest contains {chestItems.Count} items valued at {currentValue}");
			return chestItems;
		}

		public static int GetChestCoins(int mult, float increaseRate, int baseMin, int baseMax)
		{
			int coins = (int)Math.Round(Math.Pow(mult, increaseRate) * myRand.Next(baseMin, baseMax));

			//SMonitor.Log($"chest contains {coins} coins");
			return coins;
		}

		public static Chest MakeChest(List<Item> chestItems, Vector2 chestSpot)
		{
			Chest chest = new(true);

			chest.Items.Clear();
			chest.Items.AddRange(chestItems);
			chest.TileLocation = chestSpot;
			chest.bigCraftable.Value = true;
			chest.modData["Pathoschild.ChestsAnywhere/IsIgnored"] = "true";
			chest.modData["Pathoschild.Automate/StoreItems"] = "Disable";
			chest.modData["Pathoschild.Automate/TakeItems"] = "Disable";
			chest.modData["aedenthorn.AdvancedLootFramework/IsAdvancedLootFrameworkChest"] = "true";
			return chest;
		}

		private static int GetNumberOfObjects(int value, double maxValue)
		{
			return myRand.Next(1, (int)Math.Floor(maxValue / value));
		}

		private static bool CanAddTreasure(int price, int min, int max)
		{
			if (min > 0 && min > price)
			{
				return false;
			}
			if (max > 0 && max < price)
			{
				return false;
			}
			return true;
		}
	}
}
