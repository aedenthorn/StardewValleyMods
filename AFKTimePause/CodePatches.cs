using HarmonyLib;
using StardewValley;
using StardewValley.Menus;

namespace AFKTimePause
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Game1), nameof(Game1.shouldTimePass))]
        public class Game1_shouldTimePass_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                if (!Config.ModEnabled || !Game1.IsMasterGame || elapsedSeconds < Config.SecondsTilAFK)
                    return true;
                if(elapsedSeconds == Config.SecondsTilAFK)
                {
                    elapsedSeconds++;
                    SMonitor.Log("Going AFK");
                }
                __result = false;
                return false;
            }
        }
    }
}