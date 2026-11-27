using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;

namespace TreasureChestsExpanded
{
	public interface IAdvancedLootFrameworkApi
	{
		List<object> LoadPossibleTreasures(string[] itemTypeList, int minItemValue, int maxItemValue);
		List<Item> GetChestItems(List<object> treasures, Dictionary<string, int> itemChances, int maxItems, int minItemValue, int maxItemValue, int mult, float increaseRate, int baseValue);
		int GetChestCoins(int mult, float increaseRate, int baseMin, int baseMax);
		Chest MakeChest(List<Item> chestItems, Vector2 chestSpot);
		Chest MakeChest(List<object> treasures, Dictionary<string, int> itemChances, int maxItems, int minItemValue, int maxItemValue, int mult, float increaseRate, int itemBaseValue, Vector2 chestSpot);
	}
}
