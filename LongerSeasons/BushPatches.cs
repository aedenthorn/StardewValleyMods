using StardewValley;
using StardewValley.TerrainFeatures;

namespace LongerSeasons
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static bool Bush_inBloom_Prefix(Bush __instance, ref bool __result)
        {
            if (!Config.EnableMod || !Config.ExtendBerry || __instance.size.Value == 4 || __instance.size.Value == 3)
                return true;
            GameLocation location = __instance.Location;
            Season season = ((location != null) ? location.GetSeason() : Game1.season);
            int dayOfMonth = Game1.dayOfMonth;

            float mult = Config.DaysPerMonth / 28f;
            if (season == Season.Spring)
            {
                __result = dayOfMonth > 14 * mult && dayOfMonth < 19 * mult;
                return false;
            }
            if (season == Season.Fall)
            {
                __result = dayOfMonth > 7 * mult && dayOfMonth < 12 * mult;
                return false;
            }

            return true;
        }

    }
}