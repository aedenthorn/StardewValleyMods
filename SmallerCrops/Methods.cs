using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace SmallerCrops
{
    public partial class ModEntry
    {
        private static int GetMouseIndex(int tileX, int tileY, bool inTile = true)
        {
            var mousePos = Game1.getMousePosition();
            mousePos = new Point(mousePos.X + Game1.viewport.X, mousePos.Y + Game1.viewport.Y);
            if (inTile) 
            { 
                var box = new Rectangle(tileX * 64, tileY * 64, 64, 64);
                if(!box.Contains(mousePos))
                    return -1;
            }

            var x = mousePos.X % 64;
            var y = mousePos.Y % 64;
            if (x < 32 && y < 32)
                return 0;
            int idx = 1;
            if (y > 32)
            {
                if (x > 32)
                {
                    idx = 3;
                }
                else
                {
                    idx = 2;
                }
            }
            return idx;
        }
        private static float GetCropScale(Crop __instance)
        {
            if (!Config.ModEnabled || __instance.forageCrop.Value)
                return 4f;
            return 2f;
        }
        private static float GetPlacementScale(Object __instance)
        {
            if (!Config.ModEnabled || (__instance.Category != -74 && __instance.Category != -19))
                return 4f;
            return 2f;
        }
        private static int GetPlacementX(Object __instance)
        {
            if (!Config.ModEnabled || (__instance.Category != -74 && __instance.Category != -19))
                return Game1.viewport.X;
            int idx = GetMouseIndex(0, 0, false);
            return Game1.viewport.X - 32 * (idx % 2);
        }
        private static int GetPlacementY(Object __instance)
        {
            if (!Config.ModEnabled || (__instance.Category != -74 && __instance.Category != -19))
                return Game1.viewport.Y;
            int idx = GetMouseIndex(0, 0, false);
            return Game1.viewport.Y - (idx > 1 ? 32 : 0);
        }
        private static float GetScale()
        {
            if (!Config.ModEnabled)
                return 4f;
            return 2f;
        }
    }
}