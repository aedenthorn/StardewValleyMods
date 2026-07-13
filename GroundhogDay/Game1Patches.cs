using StardewModdingAPI;
using StardewValley;

namespace GroundhogDay
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static void Game1__newDayAfterFade_Prefix()
        {
            if (!Config.EnableMod)
                return;

            // Only the host should rewind the shared calendar; farmhands receive the
            // (unchanged) date from the host via the game's normal multiplayer sync,
            // so decrementing it locally on a farmhand would just desync the date.
            if (!Context.IsMainPlayer)
                return;

            // Don't repeat day 1 of spring, year 1 (the very start of a new save).
            if (Game1.dayOfMonth == 1 && Game1.season == Season.Spring && Game1.year == 1)
                return;

            SMonitor.Log($"Repeating {Utility.getDateString()}");
            Game1.dayOfMonth--;
        }
    }
}