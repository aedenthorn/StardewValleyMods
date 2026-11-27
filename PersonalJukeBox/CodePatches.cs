using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace PersonalJukeBox
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.HandleMusicChange))]
        public static class GameLocation_HandleMusicChange_Patch
        {
            public static bool Prefix(GameLocation __instance)
            {
                if (!Config.ModEnabled || !Context.IsWorldReady || PlayerSong == null)
                    return true;
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkForMusic))]
        public static class GameLocation_checkForMusic_Patch
        {
            public static bool Prefix(GameLocation __instance)
            {
                if (!Config.ModEnabled || !Context.IsWorldReady || PlayerSong == null)
                    return true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.updateMusic))]
        public static class Game1_updateMusic_Patch
        {
            public static bool Prefix()
            {
                if (!Config.ModEnabled)
                    return true;
                return true;
            }
        }
    }
}