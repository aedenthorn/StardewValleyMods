using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace GardenPotTweaks
{
    public partial class ModEntry
    {
        private static bool IsPotModified(IndoorPot indoorPot)
        {
            return indoorPot.hoeDirt.Value?.crop is not null || indoorPot.bush?.Value is not null || indoorPot.hoeDirt?.Value?.fertilizer?.Value != 0;

        }
        private static int GetBushEffectiveSize(Bush bush)
        {
            if (bush.size.Value == 3)
            {
                return 0;
            }
            if (bush.size.Value == 4)
            {
                return 1;
            }
            return bush.size.Value;
        }
    }
}