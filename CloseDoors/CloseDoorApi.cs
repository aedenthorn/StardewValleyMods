using Microsoft.Xna.Framework;
using StardewValley;

namespace CloseDoors
{
    public interface ICloseDoorApi
    {
        bool CloseDoor(GameLocation location, Point tile);
    }
    public class CloseDoorApi : ICloseDoorApi
    {
        public bool CloseDoor(GameLocation location, Point tile)
        {
            return ModEntry.TryCloseDoor(location, tile);
        }
    }
}