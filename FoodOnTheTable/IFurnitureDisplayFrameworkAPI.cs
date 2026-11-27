using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;

namespace FoodOnTheTable
{
    public interface IFurnitureDisplayFrameworkAPI
    {
        public int GetTotalSlots(Furniture f);
        Rectangle? GetSlotRect(Furniture f, int i);
        List<Object> GetSlotObjects(Furniture f);
    }
}