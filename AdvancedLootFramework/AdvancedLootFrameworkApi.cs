using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;

namespace AdvancedLootFramework
{
    public class AdvancedLootFrameworkApi
    {
        public List<object> LoadPossibleTreasures(List<string> itemTypeList, int minItemValue, int maxItemValue)
        {
            return ModEntry.LoadPossibleTreasures(itemTypeList, minItemValue, maxItemValue);
        }
        public List<Item> GetChestItems(List<object> treasures, int maxItems, int minItemValue, int maxItemValue, int mult, float increaseRate, int baseValue) 
        {
            return ModEntry.GetChestItems(treasures, maxItems, minItemValue, maxItemValue, mult, increaseRate, baseValue);
        }
        public int GetChestCoins(int mult, float increaseRate, int baseMin, int baseMax)
        {
            return ModEntry.GetChestCoins(mult, increaseRate, baseMin, baseMax);
        }
        public Chest MakeChest(List<Item> chestItems, int coins, Vector2 chestSpot)
        {
            return ModEntry.MakeChest(chestItems, coins, chestSpot);
        }
        public Chest MakeChest(List<object> treasures, int maxItems, int minItemValue, int maxItemValue, int mult, float increaseRate, int itemBaseValue, int coinBaseMin, int coinBaseMax, Vector2 chestSpot)
        {
            return ModEntry.MakeChest(GetChestItems(treasures, maxItems, minItemValue, maxItemValue, mult, increaseRate, itemBaseValue), GetChestCoins(mult, increaseRate, coinBaseMin, coinBaseMax), chestSpot);
        }
    }
}