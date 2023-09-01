using StardewValley.Network;
using StardewValley;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ResourceStorage
{
    public interface IInventory
    {
        object Object { get; }

        GameLocation Location { get; }

        Farmer Player { get; }

        NetMutex Mutex { get; }

        bool IsLocked();

        bool IsValid();

        bool CanInsertItems();

        bool CanExtractItems();

        Rectangle? GetMultiTileRegion();

        Vector2? GetTilePosition();

        IList<Item> GetItems();

        bool IsItemValid(Item item);

        void CleanInventory();

        int GetActualCapacity();
    }
}