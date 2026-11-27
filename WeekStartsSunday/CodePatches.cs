using HarmonyLib;
using StardewValley;
using System;

namespace WeekStartsSunday
{
    public partial class ModEntry
    {

        public static void ShiftCPDayOfWeek(ref DayOfWeek __result)
        {
            if (!Config.ModEnabled)
                return;
            if(__result == 0)
            {
                __result = DayOfWeek.Saturday;
            }
            else
            {
                __result = __result - 1;
            }
        }


        [HarmonyPatch(typeof(Game1), nameof(Game1.shortDayNameFromDayOfSeason))]
        public class Game1_shortDayNameFromDayOfSeason_Patch
        {
            public static void Prefix(ref int dayOfSeason)
            {
                if (!Config.ModEnabled)
                    return;
                dayOfSeason -= 1;
            }
        }
        

        [HarmonyPatch(typeof(Game1), nameof(Game1.shortDayDisplayNameFromDayOfSeason))]
        public class Game1_shortDayDisplayNameFromDayOfSeason_Patch
        {
            public static void Prefix(ref int dayOfSeason)
            {
                if (!Config.ModEnabled)
                    return;
                dayOfSeason -= 1;
            }
        }

        [HarmonyPatch(typeof(WorldDate), nameof(WorldDate.DayOfWeek))]
        [HarmonyPatch(MethodType.Getter)]
        public class WorldDate_DayOfWeek_Patch
        {
            public static bool Prefix(WorldDate __instance, ref DayOfWeek __result)
            {
                if (!Config.ModEnabled)
                    return true;
                __result = (DayOfWeek)((__instance.DayOfMonth - 1) % 7);
                return false;
            }
        }
        
   }
}