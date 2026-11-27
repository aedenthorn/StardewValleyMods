using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;

namespace UndergroundSecrets
{
    public interface ITreasureChestsExpandedApi 
    {
        List<Item> GetChestItems(int level);
        int GetChestCoins(int level);
        Chest MakeChest(List<Item> chestItems, int coins, Vector2 chestSpot);
        Chest MakeChest(int mult, Vector2 chestSpot);
    }
}