using Microsoft.Xna.Framework;
using StardewValley.Objects;

namespace FishTankCapacity
{
    public partial class ModEntry
    {
        private static string GetItemCountCheck(string id)
        {
            if (!Config.ModEnabled || !DecorationDict.ContainsKey(id))
            {
                return id;
            }
            return "this.is.not.an.item.id";
        }

        public static void AdjustPos(FishTankFurniture instance, ref Vector2 pos, int width)
        {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            if (x < width / 2)
            {
                x = width / 2;
            }
            if (x > instance.GetTankBounds().Width / 4 - width / 2)
            {
                x = instance.GetTankBounds().Width / 4 - width / 2;
            }
            pos = new(x, y);
        }
    }
}