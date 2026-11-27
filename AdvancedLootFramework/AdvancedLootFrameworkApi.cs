using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;

namespace AdvancedLootFramework
{
	public interface IAdvancedLootFrameworkApi
	{
		List<object> LoadPossibleTreasures(string[] itemTypeList, int minItemValue, int maxItemValue);
		List<Item> GetChestItems(List<object> treasures, Dictionary<string, int> itemChances, int maxItems, int minItemValue, int maxItemValue, int mult, float increaseRate, int baseValue);
		int GetChestCoins(int mult, float increaseRate, int baseMin, int baseMax);
		Chest MakeChest(List<Item> chestItems, Vector2 chestSpot);
		Chest MakeChest(List<object> treasures, Dictionary<string, int> itemChances, int maxItems, int minItemValue, int maxItemValue, int mult, float increaseRate, int itemBaseValue, Vector2 chestSpot);
	}

	public class AdvancedLootFrameworkApi : IAdvancedLootFrameworkApi
	{
		public List<object> LoadPossibleTreasures(string[] itemTypeList, int minItemValue, int maxItemValue)
		{
			return ModEntry.LoadPossibleTreasures(itemTypeList, minItemValue, maxItemValue);
		}

		public List<Item> GetChestItems(List<object> treasures, Dictionary<string, int> itemChances, int maxItems, int minItemValue, int maxItemValue, int mult, float increaseRate, int baseValue)
		{
			return ModEntry.GetChestItems(treasures, itemChances, maxItems, minItemValue, maxItemValue, mult, increaseRate, baseValue);
		}

		public int GetChestCoins(int mult, float increaseRate, int baseMin, int baseMax)
		{
			return ModEntry.GetChestCoins(mult, increaseRate, baseMin, baseMax);
		}

		public Chest MakeChest(List<Item> chestItems, Vector2 chestSpot)
		{
			return ModEntry.MakeChest(chestItems, chestSpot);
		}

		public Chest MakeChest(List<object> treasures, Dictionary<string, int> itemChances, int maxItems, int minItemValue, int maxItemValue, int mult, float increaseRate, int itemBaseValue, Vector2 chestSpot)
		{
			return ModEntry.MakeChest(GetChestItems(treasures, itemChances, maxItems, minItemValue, maxItemValue, mult, increaseRate, itemBaseValue), chestSpot);
		}
	}
}
