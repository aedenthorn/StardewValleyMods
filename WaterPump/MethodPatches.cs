using StardewValley;

namespace WaterPump
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {

        private static bool Utility_playerCanPlaceItemHere_Prefix()
        {
            if (Config.EnableMod && (Game1.dayOfMonth != 0 || Game1.year != 1 || Game1.currentSeason != "spring"))
            {
                SMonitor.Log($"Repeating {Utility.getDateString()}");
                Game1.dayOfMonth--;
            }
        }
    }
}