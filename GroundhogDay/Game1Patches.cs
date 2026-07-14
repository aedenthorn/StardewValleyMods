using StardewModdingAPI;
using StardewValley;

namespace GroundhogDay
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static void Game1_newDayAfterFade_Prefix()
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

            // Some scripted content (e.g. the vanilla earthquake event) is gated on
            // total days played rather than the calendar date, so it can still fire
            // "early" while the date is frozen unless this is held back too.
            Game1.stats.DaysPlayed--;
        }

        /// <summary>
        /// Mail queued via the "tomorrow" mechanism (vanilla or any mod's "AddMail ... tomorrow"
        /// trigger action) isn't keyed to a calendar date at all - it's just a queue
        /// (Farmer.mailForTomorrow) that this vanilla method unconditionally drains into the
        /// mailbox on every sleep. Left alone, that content would arrive on every repeated day
        /// instead of waiting for the date to actually advance. Hold back visible letters while
        /// the mod is enabled; they keep accumulating and get delivered once the mod is toggled
        /// off and a real day passes.
        ///
        /// Entries suffixed with "%&amp;NL&amp;%" aren't letters at all - they're silent, permanent
        /// world-state flags (e.g. "ccBoilerRoom" for a completed Community Center room, or
        /// "leoMoved" for Leo relocating from Ginger Island). Those always go through immediately:
        /// otherwise things like a just-completed Boiler Room repair would never actually take
        /// effect (the cutscene plays, since that's decided by checking mailForTomorrow directly,
        /// but the follow-up permanent flag never lands, so e.g. the minecarts stay "out of order"
        /// forever while the mod is enabled).
        /// </summary>
        private static bool Game1_ReceiveMailForTomorrow_Prefix(string mail_to_transfer)
        {
            if (!Config.EnableMod)
                return true;

            // Let the game's own internal one-off calls (e.g. clearing a specific mail key)
            // through; only hold back the general "flush everything for the new day" call.
            if (mail_to_transfer != null)
                return true;

            foreach (string item in Game1.player.mailForTomorrow)
            {
                if (item == null || !item.Contains("%&NL&%"))
                    continue;

                Game1.mailDeliveredFromMailForTomorrow.Add(item);
                Game1.player.mailReceived.Add(item.Replace("%&NL&%", ""));
            }

            return false;
        }
    }
}