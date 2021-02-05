using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;

namespace ParrotPerch
{
    public interface IAdvancedLootFrameworkApi
    {
        List<object> LoadPossibleTreasures(string[] itemTypeList, int minItemValue, int maxItemValue);
        List<Item> GetChestItems(List<object> treasures, Dictionary<string, int> itemChances, int maxItems, int minItemValue, int maxItemValue, int mult, float increaseRate, int baseValue);
        int GetChestCoins(int mult, float increaseRate, int baseMin, int baseMax);
        Chest MakeChest(List<Item> chestItems, int coins, Vector2 chestSpot);
        Chest MakeChest(List<object> treasures, Dictionary<string, int> itemChances, int maxItems, int minItemValue, int maxItemValue, int mult, float increaseRate, int itemBaseValue, int coinBaseMin, int coinBaseMax, Vector2 chestSpot);
    }
}