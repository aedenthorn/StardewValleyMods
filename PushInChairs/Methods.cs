using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;

namespace PushInChairs
{
    public partial class ModEntry
    {
        private static bool IsTable(Furniture pf)
        {
            return pf.furniture_type.Value == 11 || pf.furniture_type.Value == 5;
        }

        private static void CheckFurniture(Furniture f)
        {
            if (!NextToTable(f))
            {
                f.modData.Remove(modKey);
                return;
            }
        }
        private static bool NextToTable(Furniture f)
        {
            if (f.Location == null)
                return false;
            Vector2 pushTile = f.currentRotation.Value switch
            {
                2 => new Vector2(f.TileLocation.X, f.TileLocation.Y - 1),
                1 => new Vector2(f.TileLocation.X + 1, f.TileLocation.Y),
                0 => new Vector2(f.TileLocation.X, f.TileLocation.Y + 1),
                3 => new Vector2(f.TileLocation.X - 1, f.TileLocation.Y),
                _ => f.TileLocation
            };
            var pf = f.Location.GetFurnitureAt(pushTile);
            return pf != null && IsTable(pf);
        }

    }
}