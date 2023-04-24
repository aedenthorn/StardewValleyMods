using HarmonyLib;
using StardewValley;
using StardewValley.Menus;

namespace AFKTimePause
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Game1), nameof(Game1.UpdateGameClock))]
        public class Game1_UpdateGameClock_Patch
        {
            public static bool Prefix()
            {
                if (!Config.ModEnabled || !Game1.IsMasterGame || Game1.eventUp || Game1.isFestival() || elapsedSeconds < Config.SecondsTilAFK)
                    return true;
                return false;
            }
        }
    }
}