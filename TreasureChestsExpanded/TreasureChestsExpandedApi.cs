using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;

namespace TreasureChestsExpanded
{
    public class TreasureChestsExpandedApi
    {
        public List<Treasure> GetTreasureList()
        {
            return ModEntry.treasures;
        }
        public List<Item> GetChestItems(int mult)
        {
            return ModEntry.GetChestItems(mult);
        }
        public int GetChestCoins(int mult)
        {
            return ModEntry.GetChestCoins(mult);
        }
        public Chest MakeChest(List<Item> chestItems, int coins, Vector2 chestSpot)
        {
            return ModEntry.MakeChest(chestItems, coins, chestSpot);
        }
        public Chest MakeChest(int mult, Vector2 chestSpot)
        {
            return ModEntry.MakeChest(GetChestItems(mult), GetChestCoins(mult), chestSpot);
        }
    }
}