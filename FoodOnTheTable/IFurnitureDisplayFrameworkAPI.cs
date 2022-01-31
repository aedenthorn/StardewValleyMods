using Microsoft.Xna.Framework;
using StardewValley.Objects;

namespace FoodOnTheTable
{
    public interface IFurnitureDisplayFrameworkAPI
    {
        public int GetTotalSlots(Furniture f);
        Rectangle? GetSlotRect(Furniture f, int i);
    }
}